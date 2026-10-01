using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 盲月：未格挡伤害按层数百分比转化为治疗。解封成功后改名为路德维希的月光。
/// Amount 为吸血百分比（50、100、150…）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsBlindMoonPower : AlchemyStarsSwordAltarPowerBase
{
    private const string LocPrefix = "ALCHEMY_STARS_POWER_ALCHEMY_STARS_BLIND_MOON_POWER";

    private bool _unsealed;

    protected override string PlaceholderIcon => "AlchemyStarsBlindMoonPower";

    public override string? CustomIconPath =>
        $"{Entry.ResPath}/images/powers/{(_unsealed ? "AlchemyStarsLudwigMoonlight" : PlaceholderIcon)}.png";

    public override string? CustomBigIconPath => CustomIconPath;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title =>
        new("powers", _unsealed ? $"{LocPrefix}.title.unsealed" : $"{LocPrefix}.title");

    public void MarkUnsealed()
    {
        AssertMutable();
        _unsealed = true;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner || result.UnblockedDamage <= 0 || Amount <= 0)
            return;

        var heal = result.UnblockedDamage * (Amount / 100m);
        if (heal <= 0)
            return;

        await CreatureCmd.Heal(Owner, heal);
    }
}
