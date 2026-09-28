using System.Linq;
using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 强行感应：回合结束时若格挡低于 4，获得等同于数量最多的属性格数量的格挡。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsForcedInduction : AlchemyStarsCharacterRelicBase
{
    private const int BlockThreshold = 4;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || side != CombatSide.Player || !participants.Contains(Owner.Creature))
            return;

        if (!LightMechanic.IsMechanicActive(Owner))
            return;

        if (Owner.Creature.Block >= BlockThreshold)
            return;

        var state = LightMechanic.GetActiveState(Owner);
        if (state == null || state.AttributeCells.Items.Count == 0)
            return;

        var maxCount = state.AttributeCells.Items
            .GroupBy(cell => cell.Element)
            .Select(g => g.Count())
            .DefaultIfEmpty(0)
            .Max();
        if (maxCount <= 0)
            return;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(maxCount, ValueProp.Move), null);
    }
}
