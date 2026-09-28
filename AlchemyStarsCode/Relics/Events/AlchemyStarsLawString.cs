using AlchemyStars.Cards;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics.Events;

/// <summary>
/// 律法之弦：薇丝 / 薇丝·空瞳 未格挡的攻击伤害附加 1 层易伤。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLawString : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsAllowed(IRunState runState) => false;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (Owner == null || dealer?.Player != Owner || target == null || target.IsDead)
            return;

        if (cardSource is not (AlchemyStarsVice or AlchemyStarsViceEmptyPupil))
            return;

        if (result.UnblockedDamage <= 0m)
            return;

        Flash();
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            target,
            1m,
            Owner.Creature,
            cardSource);
    }
}
