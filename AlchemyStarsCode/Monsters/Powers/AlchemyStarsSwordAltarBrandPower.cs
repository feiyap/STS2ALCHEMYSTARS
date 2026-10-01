using AlchemyStars.Mechanics;
using AlchemyStars.UI;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 被祭剑座赋予的属性。Amount 为元素编码（1=森,2=雷,3=水,4=火）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSwordAltarBrandPower : AlchemyStarsSwordAltarPowerBase
{
    private const string LocPrefix = "ALCHEMY_STARS_POWER_ALCHEMY_STARS_SWORD_ALTAR_BRAND_POWER";

    protected override string PlaceholderIcon => "AlchemyStarsDragonFangMarkPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public LightElement Element => Decode(Amount);

    public override LocString Title =>
        new("powers", $"{LocPrefix}.title.{ElementSuffix(Element)}");

    public override LocString Description =>
        new("powers", $"{LocPrefix}.description.{ElementSuffix(Element)}");

    public override string? CustomIconPath =>
        LightMechanicUiAssets.GetCardAttributeIconPath(Element);

    public override string? CustomBigIconPath => CustomIconPath;

    public static int Encode(LightElement element) =>
        element.IsBaseElement() ? (int)element + 1 : 1;

    public static LightElement Decode(decimal amount) =>
        amount switch
        {
            2 => LightElement.Thunder,
            3 => LightElement.Water,
            4 => LightElement.Fire,
            _ => LightElement.Forest,
        };

    private static string ElementSuffix(LightElement element) =>
        element switch
        {
            LightElement.Thunder => "thunder",
            LightElement.Water => "water",
            LightElement.Fire => "fire",
            _ => "forest",
        };
}
