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
/// 篝火合成：消耗水/森巴顿各 1 张，获得壮志凌云·巴顿。
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

        await CardPileCmd.RemoveFromDeck(waterBarton);
        await CardPileCmd.RemoveFromDeck(forestBarton);

        var fused = Owner.RunState.CreateCard<AlchemyStarsWaterRareBarton>(Owner);
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
        waterBarton = player.Deck.Cards.FirstOrDefault(card => card is AlchemyStarsWaterCommon4)!;
        forestBarton = player.Deck.Cards.FirstOrDefault(card => card is AlchemyStarsForestUncommon3)!;
        return waterBarton != null && forestBarton != null;
    }

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
