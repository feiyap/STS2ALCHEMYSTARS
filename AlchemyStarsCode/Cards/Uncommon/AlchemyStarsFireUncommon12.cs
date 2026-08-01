using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 狂热旧梦·浮士德：为所有玩家生成随机零费牌，可消耗火光能让全员抽牌。多人模式。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon12 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseDrawCount = 1;
    private const int DrawCountUpgradeBy = 1;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(BaseDrawCount),
        AlchemyStarsKeywordText.InlineTitleVar("BoxMelody", AlchemyStarsKeywordIds.BoxMelody),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BoxMelody)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BoxMelody)),
        ];

    public AlchemyStarsFireUncommon12()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        var pool = ModelDb.CardPool<ColorlessCardPool>();
        foreach (var player in CombatState.RunState.Players)
        {
            var candidates = pool
                .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                .Where(card =>
                    card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare &&
                    card is not AlchemyStarsFireUncommon12)
                .ToList();

            if (candidates.Count == 0)
                continue;

            var created = CardFactory.GetDistinctForCombat(
                player,
                candidates,
                1,
                player.RunState.Rng.CombatCardGeneration);

            foreach (var card in created)
            {
                card.EnergyCost.SetThisTurn(0);
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
            }
        }

        if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Fire]))
            return;

        foreach (var player in CombatState.RunState.Players)
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(DrawCountUpgradeBy);
    }
}
