using System.Linq;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 新手召集：拾起时从 10 张牌（含至少 1 张稀有）中选 1；不受光能追踪影响。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsNoviceSummon : AlchemyStarsCharacterRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        Flash();
        var options = new CardCreationOptions(
            [Owner.Character.CardPool],
            CardCreationSource.Other,
            CardRarityOddsType.Uniform).WithFlags(CardCreationFlags.NoModifyHooks);

        var offered = CardFactory.CreateForReward(Owner, 9, options).ToList();
        // 保证至少 1 张稀有。
        var rareOptions = new CardCreationOptions(
            [Owner.Character.CardPool],
            CardCreationSource.Other,
            CardRarityOddsType.Uniform,
            c => c.Rarity == CardRarity.Rare).WithFlags(CardCreationFlags.NoModifyHooks);
        var rares = CardFactory.CreateForReward(Owner, 1, rareOptions).ToList();
        offered.AddRange(rares);
        var rng = Owner.PlayerRng.Rewards;
        offered = offered.OrderBy(_ => rng.NextInt()).ToList();

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1)
        {
            Cancelable = false,
        };
        var picked = (await CardSelectCmd.FromSimpleGridForRewards(
            new BlockingPlayerChoiceContext(),
            offered,
            Owner,
            prefs)).FirstOrDefault();
        if (picked != null)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(picked, PileType.Deck));
    }
}
