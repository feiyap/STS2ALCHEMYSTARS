using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 颤栗：层数达到 4 时移除并眩晕。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsTremorPower : ModPowerTemplate
{
    private const int StunThreshold = 4;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this) || amount <= 0m)
            return;

        await TryTriggerStunThreshold(choiceContext, Owner);
    }

    public static async Task TryTriggerStunThreshold(
        PlayerChoiceContext choiceContext,
        Creature target,
        int threshold = StunThreshold)
    {
        var tremor = target.GetPower<AlchemyStarsTremorPower>();
        if (tremor == null || tremor.Amount < threshold || target.IsDead)
            return;

        await CreatureCmd.Stun(target);
        await PowerCmd.Remove(tremor);
    }
}
