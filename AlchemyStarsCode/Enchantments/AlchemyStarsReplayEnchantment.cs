using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Enchantments;

/// <summary>
/// 重放：这张牌额外获得 1 次重放（永久）。
/// </summary>
[RegisterEnchantment]
public sealed class AlchemyStarsReplayEnchantment : ModEnchantmentTemplate
{
    private const string TimesKey = "Times";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(TimesKey, 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, DynamicVars[TimesKey])
    ];

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/AlchemyStarsRelic.png");

    public override int EnchantPlayCount(int originalPlayCount) =>
        originalPlayCount + DynamicVars[TimesKey].IntValue;
}
