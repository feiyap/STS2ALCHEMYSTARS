using MegaCrit.Sts2.Core.Entities.Relics;
using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics;

/// <summary>
/// 空裔光能遗物的共同逻辑�?
/// </summary>
public abstract class AlchemyStarsLumenRelicBase : ModRelicTemplate
{
    protected abstract int SlotLimit { get; }

    protected override IEnumerable<string> RegisteredKeywordIds =>
    [
        "ALCHEMY_STARS_KEYWORD_LIGHT_ENERGY",
        "ALCHEMY_STARS_KEYWORD_ATTRIBUTE_CELL",
        "ALCHEMY_STARS_KEYWORD_RAINBOW_LIGHT",
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner == null)
            return;

        LightMechanicCombatState.Reset(Owner);
        LightMechanic.InitializeForCombat(Owner);
        LightMechanicUiBootstrap.RefreshForPlayer(Owner);
        Flash();
        await Task.CompletedTask;
    }
}

/// <summary>
/// 初始遗物�? 槽光能栏�?4 槽转色栏，战斗开始时获得森雷水火�?1 点光能�?
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
[RegisterCharacterStarterRelic(typeof(AlchemyStarsCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(AlchemyStarsLumenRelicUpgraded))]
public sealed class AlchemyStarsLumenRelic : AlchemyStarsLumenRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override int SlotLimit => 4;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");
}
