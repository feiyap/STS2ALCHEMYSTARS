using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Enchantments;

/// <summary>
/// 隐士的古籍：把选中牌的基础费用永久改为 0。附魔会随卡牌存档，读档时重新生效。
/// </summary>
[RegisterEnchantment]
public sealed class AlchemyStarsHermitsTomeEnchantment : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card) =>
        base.CanEnchant(card) && !card.EnergyCost.CostsX;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/AlchemyStarsHermitsTome.png");

    protected override void OnEnchant()
    {
        var card = Card;
        if (card == null || card.EnergyCost.CostsX)
            return;

        card.EnergyCost.SetCustomBaseCost(0);
    }
}
