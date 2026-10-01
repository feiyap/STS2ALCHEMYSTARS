using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 离队者：无属性受伤 ×0.9；被克 ×1.4，有利 ×0.8。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsDefectorPower : AlchemyStarsSwordAltarPowerBase
{
    private const string LocPrefix = "ALCHEMY_STARS_POWER_ALCHEMY_STARS_DEFECTOR_POWER";

    protected override string PlaceholderIcon => "AlchemyStarsDefectorPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public override LocString Description =>
        new("powers", $"{LocPrefix}.description.{ElementSuffix(Altar?.CurrentElement)}");

    private AlchemyStarsSwordAltar? Altar => Owner.Monster as AlchemyStarsSwordAltar;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        var altar = Altar;
        if (altar == null || target == null)
            return 1m;

        if (target == Owner)
            return IncomingMultiplier(altar, dealer);

        if (dealer == Owner)
            return OutgoingMultiplier(altar, target);

        return 1m;
    }

    private static decimal IncomingMultiplier(AlchemyStarsSwordAltar altar, Creature? dealer)
    {
        if (altar.CurrentElement is null)
            return 0.9m;

        var brand = BrandElement(dealer);
        if (brand is null)
            return 1m;

        if (AlchemyStarsSwordAltarRules.Beats(brand.Value, altar.CurrentElement.Value))
            return 1.4m;

        if (AlchemyStarsSwordAltarRules.Beats(altar.CurrentElement.Value, brand.Value))
            return 0.8m;

        return 1m;
    }

    private static decimal OutgoingMultiplier(AlchemyStarsSwordAltar altar, Creature target)
    {
        if (altar.CurrentElement is null)
            return 1m;

        var brand = BrandElement(target);
        if (brand is null)
            return 1m;

        if (AlchemyStarsSwordAltarRules.Beats(altar.CurrentElement.Value, brand.Value))
            return 1.4m;

        if (AlchemyStarsSwordAltarRules.Beats(brand.Value, altar.CurrentElement.Value))
            return 0.8m;

        return 1m;
    }

    private static LightElement? BrandElement(Creature? creature)
    {
        var brand = creature?.GetPower<AlchemyStarsSwordAltarBrandPower>();
        return brand == null ? null : brand.Element;
    }

    private static string ElementSuffix(LightElement? element) =>
        element switch
        {
            LightElement.Thunder => "thunder",
            LightElement.Water => "water",
            LightElement.Fire => "fire",
            LightElement.Forest => "forest",
            _ => "none",
        };
}
