using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace AlchemyStars.Events;

/// <summary>
/// 事件通用效果辅助：选牌附魔、升级、删除、变化、遗物与卡牌奖励等。
/// </summary>
public static class AlchemyStarsEventHelpers
{
    public static string FallbackPortraitPath =>
        $"{Entry.ResPath}/images/events/AlchemyStarsHotSandDefense.png";

    public static async Task EnchantFromDeck<T>(Player player, decimal amount = 1m, int pickCount = 1)
        where T : EnchantmentModel
    {
        var enchantment = ModelDb.Enchantment<T>();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, pickCount);
        foreach (var card in await CardSelectCmd.FromDeckForEnchantment(player, enchantment, (int)amount, prefs))
        {
            CardCmd.Enchant<T>(card, amount);
            PlayEnchantVfx(card);
        }
    }

    public static async Task EnchantFromDeck(Player player, EnchantmentModel enchantment, decimal amount = 1m, int pickCount = 1)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, pickCount);
        foreach (var card in await CardSelectCmd.FromDeckForEnchantment(player, enchantment, (int)amount, prefs))
        {
            CardCmd.Enchant(enchantment, card, amount);
            PlayEnchantVfx(card);
        }
    }

    public static async Task UpgradeFromDeck(Player player, int count)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, count);
        foreach (var card in await CardSelectCmd.FromDeckForUpgrade(player, prefs))
            CardCmd.Upgrade(card);
    }

    public static async Task RemoveFromDeck(Player player, int count, Func<CardModel, bool>? filter = null)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, count);
        var cards = (await CardSelectCmd.FromDeckForRemoval(player, prefs, filter)).ToList();
        await CardPileCmd.RemoveFromDeck(cards);
    }

    public static async Task TransformFromDeck(Player player, int count)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, count)
        {
            Cancelable = count > 1
        };
        var cards = (await CardSelectCmd.FromDeckForTransformation(player, prefs)).ToList();
        foreach (var card in cards)
            await CardCmd.TransformToRandom(card, player.RunState.Rng.Niche);
    }

    public static async Task RemoveEtherealAndExhaust(Player player)
    {
        static bool HasKeywords(CardModel c) =>
            c.Keywords.Contains(CardKeyword.Ethereal) || c.Keywords.Contains(CardKeyword.Exhaust);

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        var card = (await CardSelectCmd.FromDeckGeneric(player, prefs, HasKeywords)).FirstOrDefault();
        if (card == null)
            return;

        CardCmd.RemoveKeyword(card, CardKeyword.Ethereal, CardKeyword.Exhaust);
    }

    public static async Task GainRandomRelic(Player player, RelicRarity? minRarity = null)
    {
        RelicModel relic;
        if (minRarity is RelicRarity.Uncommon or RelicRarity.Rare)
        {
            // 至少罕见：在罕见 / 稀有中抽一件。
            var rollRare = player.PlayerRng.Rewards.NextFloat() >= 0.5f;
            var rarity = minRarity == RelicRarity.Rare || rollRare
                ? RelicRarity.Rare
                : RelicRarity.Uncommon;
            relic = RelicFactory.PullNextRelicFromFront(player, rarity).ToMutable();
        }
        else
        {
            relic = RelicFactory.PullNextRelicFromFront(player).ToMutable();
        }

        await RelicCmd.Obtain(relic, player);
    }

    public static async Task GainCharacterRelic(Player player)
    {
        // 空裔专属遗物：直接从抓取袋按过滤器抽取，避免 RelicFactory 在无匹配时回退 Circlet。
        static bool IsCharacterRelic(RelicModel relic) =>
            relic is AlchemyStars.Relics.Character.AlchemyStarsCharacterRelicBase;

        player.PopulateRelicGrabBagIfNecessary(player.PlayerRng.Rewards);

        RelicModel? relic = null;
        foreach (var rarity in new[]
                 {
                     RelicFactory.RollRarity(player),
                     RelicRarity.Common,
                     RelicRarity.Uncommon,
                     RelicRarity.Rare,
                     RelicRarity.Shop
                 })
        {
            relic = player.RelicGrabBag.PullFromFront(rarity, IsCharacterRelic, player.RunState);
            if (relic != null)
                break;
        }

        if (relic == null)
        {
            // 角色专属已抽尽时退回普通随机。
            await GainRandomRelic(player);
            return;
        }

        player.RunState.SharedRelicGrabBag.Remove(relic);
        await RelicCmd.Obtain(relic.ToMutable(), player);
    }

    public static async Task AddUpgradedCardToDeck<T>(Player player) where T : CardModel
    {
        // 必须经 RunState.CreateCard 绑定 Owner，否则 CardPileCmd.Add 会抛无 owner 异常。
        var card = player.RunState.CreateCard<T>(player);
        if (card.IsUpgradable)
            CardCmd.Upgrade(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }

    public static async Task PickCardsFromCharacterPool(Player player, int offerCount, int pickCount)
    {
        var options = CardCreationOptions.ForNonCombatWithDefaultOdds([player.Character.CardPool]);
        var cards = CardFactory.CreateForReward(player, offerCount, options).ToList();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 0, pickCount)
        {
            Cancelable = true
        };
        foreach (var picked in await CardSelectCmd.FromSimpleGridForRewards(
                     new BlockingPlayerChoiceContext(), cards, player, prefs))
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(picked, PileType.Deck));
        }
    }

    public static async Task PickOneUpgradedFromCandidates(Player player, IReadOnlyList<CardModel> prototypes)
    {
        var offered = prototypes
            .Select(p =>
            {
                // 必须经 RunState.CreateCard 绑定 Owner，否则选中后加牌组会卡死。
                var card = player.RunState.CreateCard(p, player);
                if (card.IsUpgradable)
                    CardCmd.Upgrade(card);
                return new CardCreationResult(card);
            })
            .ToList();

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1)
        {
            Cancelable = false
        };
        var picked = (await CardSelectCmd.FromSimpleGridForRewards(
            new BlockingPlayerChoiceContext(), offered, player, prefs)).FirstOrDefault();
        if (picked != null)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(picked, PileType.Deck));
    }

    public static async Task UpgradeRandomFromDeck(Player player, int count)
    {
        var upgradable = PileType.Deck.GetPile(player).Cards
            .Where(c => c?.IsUpgradable ?? false)
            .ToList()
            .StableShuffle(player.RunState.Rng.Niche)
            .Take(count);
        foreach (var card in upgradable)
            CardCmd.Upgrade(card);
        await Task.CompletedTask;
    }

    public static EncounterModel? PickRandomElite(Player player)
    {
        var elites = player.RunState.Act.AllEliteEncounters.ToList();
        if (elites.Count == 0)
            return null;
        var picked = player.PlayerRng.Rewards.NextItem(elites);
        return picked?.ToMutable();
    }

    public static bool IsAct1(IRunState run) => run.Act is Overgrowth or Underdocks;

    public static bool IsAct2(IRunState run) => run.Act is Hive;

    public static bool IsAct3(IRunState run) => run.Act is Glory;

    private static void PlayEnchantVfx(CardModel card)
    {
        var vfx = NCardEnchantVfx.Create(card);
        if (vfx != null)
            NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(vfx);
    }
}
