using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 空裔角色专属遗物基类（进入空裔遗物池）。
/// </summary>
public abstract class AlchemyStarsCharacterRelicBase : ModRelicTemplate
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");
}
