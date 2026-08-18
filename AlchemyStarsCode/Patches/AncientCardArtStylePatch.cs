using AlchemyStars.Cards;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 对声明 <see cref="IAncientCardArtStyle"/> 的非先古稀有度卡牌，套用先古卡图布局。
/// </summary>
public sealed class AncientCardArtStylePatch : IPatchMethod
{
    private const string PortraitBlurMaterialPath = "res://scenes/cards/card_portrait_blur_material.tres";
    private const string CanvasGroupMaskMaterialPath = "res://scenes/cards/card_canvas_group_mask_material.tres";
    private const string CanvasGroupMaskBlurMaterialPath = "res://scenes/cards/card_canvas_group_mask_blur_material.tres";

    public static string PatchId => "alchemy_stars_ancient_card_art_style";

    public static string Description => "Display marked rare cards with ancient portrait chrome";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCard), "Reload"),
    ];

    public static void Postfix(NCard __instance)
    {
        var model = __instance.Model;
        if (model is not IAncientCardArtStyle || model.Rarity == CardRarity.Ancient)
            return;

        if (!__instance.IsNodeReady())
            return;

        ApplyAncientArt(__instance, model);
    }

    private static void ApplyAncientArt(NCard card, CardModel model)
    {
        var portrait = card.GetNode<TextureRect>("%Portrait");
        var ancientPortrait = card.GetNode<TextureRect>("%AncientPortrait");
        var portraitBorder = card.GetNode<TextureRect>("%PortraitBorder");
        var frame = card.GetNode<TextureRect>("%Frame");
        var ancientBorder = card.GetNode<TextureRect>("%AncientBorder");
        var ancientBorderGlass = card.GetNode<TextureRect>("%AncientBorderGlassOverlay");
        var ancientTextBg = card.GetNode<TextureRect>("%AncientTextBg");
        var ancientBanner = card.GetNode<Control>("%AncientBanner");
        var banner = card.GetNode<TextureRect>("%TitleBanner");
        var portraitCanvasGroup = card.GetNode<CanvasGroup>("%PortraitCanvasGroup");

        portraitBorder.Visible = false;
        portrait.Visible = false;
        frame.Visible = false;
        ancientPortrait.Visible = true;
        ancientBorderGlass.Visible = true;
        ancientBorder.Visible = true;
        ancientTextBg.Visible = true;
        ancientBanner.Visible = true;
        banner.Visible = false;

        var portraitTexture = model.Portrait;
        if (card.Visibility != ModelVisibility.Visible)
        {
            var blur = PreloadManager.Cache.GetMaterial(PortraitBlurMaterialPath);
            portraitCanvasGroup.Material = PreloadManager.Cache.GetMaterial(CanvasGroupMaskBlurMaterialPath);
            portrait.Material = blur;
            ancientPortrait.Material = blur;
        }
        else
        {
            portraitCanvasGroup.Material = PreloadManager.Cache.GetMaterial(CanvasGroupMaskMaterialPath);
            portrait.Material = null;
            ancientPortrait.Material = null;
        }

        ancientBorder.Texture = model.AncientBorder;
        ancientTextBg.Texture = LoadAncientTextBg(model.Type);
        ancientPortrait.Texture = portraitTexture;
        banner.Material = null;
    }

    private static Texture2D LoadAncientTextBg(CardType type)
    {
        var cardType = type is CardType.Attack or CardType.Skill or CardType.Power or CardType.Quest
            ? type
            : CardType.Skill;
        var path = ImageHelper.GetImagePath(
            "atlases/compressed_atlas.sprites/ancient_text_bg_" + cardType.ToString().ToLowerInvariant() + ".png.tres");
        return ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
    }
}
