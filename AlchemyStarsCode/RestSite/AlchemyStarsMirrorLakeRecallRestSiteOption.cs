using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Relics.Character;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.RestSite;

/// <summary>
/// 镜湖之水：火堆回忆被遗忘的牌（回忆后必升级）。
/// </summary>
public sealed class AlchemyStarsMirrorLakeRecallRestSiteOption : ModRestSiteOptionTemplate
{
    public const string OptionIdValue = "ALCHEMY_STARS_MIRROR_LAKE_RECALL";

    private readonly AlchemyStarsMirrorLakeWater _relic;

    public override string OptionId => OptionIdValue;

    public override RestSiteOptionAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/AlchemyStarsMirrorLakeWater.png");

    public override LocString? CustomTitle =>
        new("relics", "ALCHEMY_STARS_RELIC_ALCHEMY_STARS_MIRROR_LAKE_WATER.recallTitle");

    public override LocString Description =>
        new("relics", "ALCHEMY_STARS_RELIC_ALCHEMY_STARS_MIRROR_LAKE_WATER.recallDescription");

    public AlchemyStarsMirrorLakeRecallRestSiteOption(Player owner, AlchemyStarsMirrorLakeWater relic)
        : base(owner)
    {
        _relic = relic;
    }

    public override async Task<bool> OnSelect()
    {
        return await _relic.RecallOneAsync(new BlockingPlayerChoiceContext());
    }

    public static bool TryAddOption(
        Player player,
        ICollection<RestSiteOption> options,
        AlchemyStarsMirrorLakeWater relic)
    {
        if (options.Any(option => option.OptionId == OptionIdValue))
            return false;

        if (!relic.HasForgottenCards)
            return false;

        options.Add(new AlchemyStarsMirrorLakeRecallRestSiteOption(player, relic));
        return true;
    }
}
