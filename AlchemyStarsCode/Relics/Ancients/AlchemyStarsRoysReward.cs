using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 罗伊的奖励：每 3 次战斗胜利提供一次稀有卡牌奖励。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsRoysReward : AlchemyStarsAncientRelicBase
{
    private const int VictoryThreshold = 3;

    private int _victoryCount;

    public override bool ShowCounter => true;

    public override int DisplayAmount => VictoryCount % VictoryThreshold;

    [SavedProperty]
    public int VictoryCount
    {
        get => _victoryCount;
        set
        {
            AssertMutable();
            _victoryCount = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner == null)
            return;

        VictoryCount++;
        if (VictoryCount % VictoryThreshold != 0)
            return;

        Flash();
        var options = new CardCreationOptions(
            [Owner.Character.CardPool],
            CardCreationSource.Other,
            CardRarityOddsType.Uniform,
            c => c.Rarity == CardRarity.Rare);
        await RewardsCmd.OfferCustom(Owner, [new CardReward(options, 3, Owner)]);
    }
}
