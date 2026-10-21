using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 收割意识：每层按伤害的 10% 额外施加原版灾厄。
/// 走 AfterDamageGiven，与原版收割形态/淬毒一致；不要挂在 AfterDamageReceived 上。
/// 后者在击杀时会被跳过，而且结算链更靠后，中间一旦等待就会让后续监听看到不同的属性上下文。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsHarvestConsciousnessPower : AlchemyStarsPowerBase
{
    private bool _applying;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<DoomPower>()];

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        // 施加灾厄本身可能再触发伤害，避免同一段结算重入。
        if (_applying || dealer != Owner || Amount <= 0 || target.IsDead)
            return;

        if (!props.IsPoweredAttack())
            return;

        var damageDealt = result.UnblockedDamage;
        if (damageDealt <= 0)
            return;

        var calamity = (int)decimal.Floor(damageDealt * 0.1m * Amount);
        if (calamity <= 0)
            return;

        _applying = true;
        try
        {
            Flash();
            await PowerCmd.Apply<DoomPower>(
                choiceContext,
                target,
                calamity,
                Owner,
                cardSource);
        }
        finally
        {
            _applying = false;
        }
    }
}
