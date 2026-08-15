using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 帝国雷霆：正道威严等效果的目标标记，层数上限 99。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsImperialThunderPower : ModPowerTemplate
{
    public const int MaxStacks = 99;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ClampToMax(this);
        return Task.CompletedTask;
    }

    public static void ClampOwner(Creature creature)
    {
        var power = creature.GetPower<AlchemyStarsImperialThunderPower>();
        if (power != null)
            ClampToMax(power);
    }

    private static void ClampToMax(AlchemyStarsImperialThunderPower power)
    {
        if (power.Amount > MaxStacks)
            power.SetAmount(MaxStacks);
    }
}
