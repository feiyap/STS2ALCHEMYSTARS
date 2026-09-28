using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 先古之民遗物基类：先古品质，不进入随机遗物池。
/// </summary>
public abstract class AlchemyStarsAncientRelicBase : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool IsAllowed(IRunState runState) => false;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");
}
