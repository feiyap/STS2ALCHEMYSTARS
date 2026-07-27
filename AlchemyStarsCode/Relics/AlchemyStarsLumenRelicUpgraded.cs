using MegaCrit.Sts2.Core.Entities.Relics;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics;

/// <summary>
/// 先古升级遗物�? 槽光能栏�?8 槽转色栏�?
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLumenRelicUpgraded : AlchemyStarsLumenRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override int SlotLimit => 8;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");
}
