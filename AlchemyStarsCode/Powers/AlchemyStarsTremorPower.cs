using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 颤栗：层数达�?25 时眩晕并移除目标身上所有审判�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsTremorPower : ModPowerTemplate
{
    private const int StunThreshold = 25;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.Tremor];

    public static async Task TryTriggerStunThreshold(
        PlayerChoiceContext choiceContext,
        Creature target,
        int threshold = 25)
    {
        var tremor = target.GetPower<AlchemyStarsTremorPower>();
        if (tremor == null || tremor.Amount < threshold || target.IsDead)
            return;

        if (target.HasPower<AlchemyStarsJudgmentPower>())
            await PowerCmd.Remove<AlchemyStarsJudgmentPower>(target);

        await CreatureCmd.Stun(target);
        await PowerCmd.Remove(tremor);
    }
}
