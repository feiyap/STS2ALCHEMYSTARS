using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 过热：下个回合结束时移除；令过热战技卡牌费用�?1�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsOverheatPower : ModPowerTemplate
{
    private int _removeAfterTurn;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.Overheat];

    public void ScheduleRemovalAfterNextTurnEnd(Player player)
    {
        _removeAfterTurn = (player.PlayerCombatState?.TurnNumber ?? 0) + 1;
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || !card.Tags.Contains(AlchemyStarsCardTags.OverheatBattleSkill))
            return false;

        modifiedCost = originalCost - 1m;
        return true;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return;

        var turnNumber = Owner.Player?.PlayerCombatState?.TurnNumber ?? 0;
        if (_removeAfterTurn <= 0 || turnNumber < _removeAfterTurn)
            return;

        Flash();
        await PowerCmd.Remove(this);
    }
}
