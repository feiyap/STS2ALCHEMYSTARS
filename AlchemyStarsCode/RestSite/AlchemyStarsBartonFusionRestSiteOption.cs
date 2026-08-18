using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.RestSite;

/// <summary>
/// 篝火合成：消耗水/森巴顿各 1 张，获得壮志凌云·巴顿；任一素材已升级时产物也升级。
/// </summary>
public sealed class AlchemyStarsBartonFusionRestSiteOption : ModRestSiteOptionTemplate
{
    public const string FusionOptionId = "ALCHEMY_STARS_BARTON_FUSION";

    public override string OptionId => FusionOptionId;

    public override RestSiteOptionAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/AlchemyStars_character_icon.png");

    public override LocString? CustomTitle =>
        new("cards", "ALCHEMY_STARS_BARTON_FUSION.name");

    public override LocString Description =>
        new("cards", "ALCHEMY_STARS_BARTON_FUSION.description");

    public AlchemyStarsBartonFusionRestSiteOption(Player owner)
        : base(owner)
    {
    }

    public override async Task<bool> OnSelect()
    {
        if (!TryFindMaterials(Owner, out var waterBarton, out var forestBarton))
            return false;

        var fuseUpgraded = waterBarton.IsUpgraded || forestBarton.IsUpgraded;

        await CardPileCmd.RemoveFromDeck(waterBarton);
        await CardPileCmd.RemoveFromDeck(forestBarton);

        var fused = Owner.RunState.CreateCard<AlchemyStarsWaterRareBarton>(Owner);
        if (fuseUpgraded)
            fused.UpgradeInternal();

        var results = new List<CardPileAddResult>
        {
            await CardPileCmd.Add(fused, PileType.Deck)
        };
        CardCmd.PreviewCardPileAdd(results, 1.2f, CardPreviewStyle.MessyLayout);
        return true;
    }

    public static bool CanFuse(Player player) =>
        TryFindMaterials(player, out _, out _);

    public static bool TryFindMaterials(
        Player player,
        out CardModel waterBarton,
        out CardModel forestBarton)
    {
        // 优先消耗已升级的素材，避免牌组里有升级版却合成出未升级巴顿。
        waterBarton = FindPreferredMaterial<AlchemyStarsWaterCommon4>(player)!;
        forestBarton = FindPreferredMaterial<AlchemyStarsForestUncommon3>(player)!;
        return waterBarton != null && forestBarton != null;
    }

    private static CardModel? FindPreferredMaterial<T>(Player player) where T : CardModel =>
        player.Deck.Cards
            .Where(card => card is T)
            .OrderByDescending(card => card.IsUpgraded)
            .FirstOrDefault();

    /// <summary>
    /// 向篝火选项列表追加合成项（若尚未存在且材料齐全）。
    /// </summary>
    public static bool TryAddOption(Player player, ICollection<RestSiteOption> options)
    {
        if (options.Any(option => option.OptionId == FusionOptionId))
            return false;

        if (!CanFuse(player))
            return false;

        options.Add(new AlchemyStarsBartonFusionRestSiteOption(player));
        return true;
    }
}
