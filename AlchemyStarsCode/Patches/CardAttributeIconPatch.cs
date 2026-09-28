using System.Runtime.CompilerServices;
using AlchemyStars.Mechanics;
using AlchemyStars.UI;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 在拥有森/雷/水/火属性的卡牌费用图标下方显示对应属性角标。
/// </summary>
public sealed class CardAttributeIconPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_card_attribute_icon";

    public static string Description => "Show attribute icon below card energy cost";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCard), "Reload"),
    ];

    public static void Postfix(NCard __instance)
    {
        if (!__instance.IsNodeReady())
            return;

        CardAttributeIconOverlay.For(__instance).Refresh();
    }
}

/// <summary>
/// 挂在卡面费用旁的属性图标节点管理。
/// </summary>
internal sealed class CardAttributeIconOverlay
{
    private const string NodeName = "AlchemyStarsAttributeIcon";
    private const float IconSize = 42f;
    private const float GapBelowEnergy = 6f;

    private static readonly ConditionalWeakTable<NCard, CardAttributeIconOverlay> Overlays = new();

    private readonly NCard _card;
    private TextureRect? _icon;

    private CardAttributeIconOverlay(NCard card)
    {
        _card = card;
    }

    public static CardAttributeIconOverlay For(NCard card) =>
        Overlays.GetValue(card, static instance => new CardAttributeIconOverlay(instance));

    public void Refresh()
    {
        var model = _card.Model;
        var element = model == null ? null : AttributeCardTracking.TryGetCardAttribute(model);
        if (element == null)
        {
            Hide();
            return;
        }

        var texture = LightMechanicUiAssets.Load(
            LightMechanicUiAssets.GetCardAttributeIconPath(element.Value));
        if (texture == null)
        {
            Hide();
            return;
        }

        var energyIcon = _card.GetNodeOrNull<TextureRect>("%EnergyIcon");
        if (energyIcon == null)
        {
            Hide();
            return;
        }

        EnsureIcon(energyIcon);
        if (_icon == null)
            return;

        _icon.Texture = texture;
        _icon.Visible = true;
        LayoutBelowEnergy(energyIcon);
        // 布局可能尚未完成，再 deferred 对齐一次。
        var icon = _icon;
        Callable.From(() =>
        {
            if (icon == null || !GodotObject.IsInstanceValid(icon) || !GodotObject.IsInstanceValid(energyIcon))
                return;
            icon.Position = ComputePosition(energyIcon);
        }).CallDeferred();
    }

    private void EnsureIcon(TextureRect energyIcon)
    {
        if (_icon != null && GodotObject.IsInstanceValid(_icon))
            return;

        var parent = energyIcon.GetParent();
        if (parent == null)
            return;

        var existing = parent.GetNodeOrNull<TextureRect>(NodeName);
        if (existing != null)
        {
            _icon = existing;
            return;
        }

        _icon = new TextureRect
        {
            Name = NodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(IconSize, IconSize),
            Size = new Vector2(IconSize, IconSize),
        };
        parent.AddChild(_icon);

        // 保证画在费用图标之上，避免被边框等同级节点挡住。
        var energyIndex = energyIcon.GetIndex();
        parent.MoveChild(_icon, energyIndex + 1);
    }

    private void LayoutBelowEnergy(TextureRect energyIcon)
    {
        if (_icon == null)
            return;

        _icon.Size = new Vector2(IconSize, IconSize);
        _icon.CustomMinimumSize = new Vector2(IconSize, IconSize);
        _icon.Position = ComputePosition(energyIcon);
    }

    private static Vector2 ComputePosition(TextureRect energyIcon)
    {
        var energyRect = energyIcon.GetRect();
        var energySize = energyRect.Size;
        if (energySize.X <= 1f || energySize.Y <= 1f)
            energySize = energyIcon.CustomMinimumSize;
        if (energySize.X <= 1f || energySize.Y <= 1f)
            energySize = energyIcon.Texture?.GetSize() ?? new Vector2(72f, 72f);

        var origin = energyRect.Position;
        if (origin == Vector2.Zero && energyIcon.Position != Vector2.Zero)
            origin = energyIcon.Position;

        return new Vector2(
            origin.X + (energySize.X - IconSize) * 0.5f,
            origin.Y + energySize.Y + GapBelowEnergy);
    }

    private void Hide()
    {
        if (_icon != null && GodotObject.IsInstanceValid(_icon))
            _icon.Visible = false;
    }
}
