using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// ?????????????1 ??????????????1% ?????/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSilentThunderPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.SilentThunder];

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0 || result.TotalDamage <= 0)
            return;

        if (!props.IsPoweredAttack())
            return;

        Flash();
        await PowerCmd.Decrement(this);

        var bonus = target.MaxHp * 0.01m;
        if (bonus <= 0m)
            return;

        using (LightMechanicDamageContext.Use(LightElement.Thunder))
        {
            await CreatureCmd.Damage(
                choiceContext,
                target,
                bonus,
                ValueProp.Unblockable | ValueProp.Unpowered,
                cardSource,
                null);
        }
    }
}
