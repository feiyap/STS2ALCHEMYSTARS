using AlchemyStars.Mechanics;
using AlchemyStars.UI;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 敌人属性标记（与世隔绝者的评估）：Amount 为元素编码（1=森,2=雷,3=水,4=火），避免 0 被移除。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsEnemyAttributePower : AlchemyStarsPowerBase
{
    private const string TitleLocPrefix =
        "ALCHEMY_STARS_POWER_ALCHEMY_STARS_ENEMY_ATTRIBUTE_POWER.title.";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>不显示层数数字（Amount 仅作属性编码）。</summary>
    public override int DisplayAmount => 0;

    public LightElement Element => Decode(Amount);

    /// <summary>按当前属性显示「森/火/雷/水属性」。</summary>
    public override LocString Title =>
        new("powers", TitleLocPrefix + ElementLocSuffix(Element));

    /// <summary>按当前属性使用卡面同款角标。</summary>
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

    private static string ElementLocSuffix(LightElement element) =>
        element switch
        {
            LightElement.Thunder => "thunder",
            LightElement.Water => "water",
            LightElement.Fire => "fire",
            _ => "forest",
        };
}
