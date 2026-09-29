using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 心之怪盗团监视：牌组含该卡时开战施加，监听破盾/受击以触发追击与总攻击。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsPhantomThievesWatchPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        await AlchemyStarsPhantomThievesTracker.AfterDamageReceived(
            choiceContext,
            target,
            result,
            props,
            dealer,
            cardSource);
    }
}
