using System.Runtime.CompilerServices;
using AlchemyStars.Cards;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.addons.mega_text;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 图鉴检视界面挂上异画复选框与“选择当前异画”按钮。
/// </summary>
public sealed class InspectCardAlternateArtReadyPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_inspect_alternate_art_ready";

    public static string Description => "Attach alternate-art controls on the inspect card screen";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "_Ready"),
    ];

    public static void Postfix(NInspectCardScreen __instance) =>
        InspectCardAlternateArtOverlay.For(__instance).EnsureUi();
}

/// <summary>
/// 切到另一张卡时，按已保存的异画恢复预览与勾选状态。
/// </summary>
public sealed class InspectCardAlternateArtSetCardPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_inspect_alternate_art_set_card";

    public static string Description => "Restore alternate-art preview when inspect card changes";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "SetCard", [typeof(int)]),
    ];

    public static void Prefix(NInspectCardScreen __instance, int index)
    {
        var cards = __instance._cards;
        if (cards is not { Count: > 0 })
            return;

        var clamped = Math.Clamp(index, 0, cards.Count - 1);
        InspectCardAlternateArtOverlay.For(__instance).BindCard(cards[clamped]);
    }
}

/// <summary>
/// 每次刷新检视卡面（含勾选强化）前套上当前异画预览。
/// </summary>
public sealed class InspectCardAlternateArtUpdateDisplayPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_inspect_alternate_art_update_display";

    public static string Description => "Apply alternate-art preview before inspect card refresh";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "UpdateCardDisplay"),
    ];

    public static void Prefix(NInspectCardScreen __instance) =>
        InspectCardAlternateArtOverlay.For(__instance).ApplyPreview();
}

/// <summary>
/// 打开时启用异画控件，关闭时清掉检视预览。
/// </summary>
public sealed class InspectCardAlternateArtOpenClosePatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_inspect_alternate_art_open_close";

    public static string Description => "Enable alternate-art controls on open and clear preview on close";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "Open"),
        new(typeof(NInspectCardScreen), "Close"),
    ];

    public static void Postfix(NInspectCardScreen __instance, System.Reflection.MethodBase __originalMethod)
    {
        var overlay = InspectCardAlternateArtOverlay.For(__instance);
        if (__originalMethod.Name == "Open")
            overlay.OnOpened();
        else
            overlay.OnClosed();
    }
}

/// <summary>
/// 复刻原版强化勾选框，挂在检视界面上预览并保存异画。
/// 不复制原版 MegaLabel：它带全屏/宽锚点，复制后会飞到左上角且文案仍是「查看升级」。
/// </summary>
internal sealed class InspectCardAlternateArtOverlay
{
    private const float AfterUpgradeGap = 20f;
    private const float ScreenPadding = 24f;
    private const float TickboxFallbackSize = 48f;
    private const int DuplicateFlagsWithoutSignals =
        (int)(Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts | Node.DuplicateFlags.UseInstantiation);

    private static readonly ConditionalWeakTable<NInspectCardScreen, InspectCardAlternateArtOverlay> Overlays = new();

    private readonly NInspectCardScreen _screen;
    private HBoxContainer? _row;
    private NTickbox? _viewTickbox;
    private MegaLabel? _viewLabel;
    private MegaLabel? _nameLabel;
    private MegaLabel? _prevButton;
    private MegaLabel? _nextButton;
    private MegaLabel? _selectLabel;
    private string? _cardTypeName;
    private IReadOnlyList<AlternateCardArt> _alts = [];
    private int _altIndex;
    private bool _viewingAlternate;
    private bool _uiReady;

    private InspectCardAlternateArtOverlay(NInspectCardScreen screen)
    {
        _screen = screen;
    }

    public static InspectCardAlternateArtOverlay For(NInspectCardScreen screen) =>
        Overlays.GetValue(screen, static instance => new InspectCardAlternateArtOverlay(instance));

    public void EnsureUi()
    {
        if (_uiReady)
            return;

        try
        {
            var upgrade = _screen.GetNode<NTickbox>("%Upgrade");
            var upgradeLabel = _screen.GetNode<MegaLabel>("%ShowUpgradeLabel");

            _row = new HBoxContainer
            {
                Name = "AlchemyStarsAlternateArtRow",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Alignment = BoxContainer.AlignmentMode.Center,
                ZIndex = 80,
            };
            _row.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            _row.AddThemeConstantOverride("separation", 8);
            StripUniqueNames(_row);
            _screen.AddChild(_row);

            _viewTickbox = (NTickbox)upgrade.Duplicate(DuplicateFlagsWithoutSignals);
            StripUniqueNames(_viewTickbox);
            ResetLayoutForBox(_viewTickbox, "AlchemyStarsViewAlternateArt");
            _row.AddChild(_viewTickbox);
            WireTickboxVisuals(_viewTickbox);
            CompactTickbox(_viewTickbox);
            // 复制体会带上原版「查看升级」子标签，先藏掉，改用我们自己排进横排的文案。
            HideOwnedLabels(_viewTickbox);
            _viewTickbox.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(OnViewToggled));

            _viewLabel = CreateStyledLabel(upgradeLabel, "AlchemyStarsViewAlternateArtLabel", Loc.ViewAlternateArt());
            _row.AddChild(_viewLabel);

            _prevButton = CreateClickableLabel(upgradeLabel, "AlchemyStarsAlternateArtPrev", "‹", OnPrevPressed);
            _nameLabel = CreateStyledLabel(upgradeLabel, "AlchemyStarsAlternateArtName", "");
            _nextButton = CreateClickableLabel(upgradeLabel, "AlchemyStarsAlternateArtNext", "›", OnNextPressed);
            _selectLabel = CreateClickableLabel(upgradeLabel, "AlchemyStarsSelectAlternateArt", Loc.SelectCurrentArt(), OnSelectPressed);
            _selectLabel.MouseEntered += OnSelectHoverEntered;
            _selectLabel.MouseExited += OnSelectHoverExited;

            _row.AddChild(_prevButton);
            _row.AddChild(_nameLabel);
            _row.AddChild(_nextButton);
            _row.AddChild(_selectLabel);

            _uiReady = true;
            SetVisible(false);
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"挂载异画检视控件失败：{ex}");
        }
    }

    public void BindCard(CardModel card)
    {
        EnsureUi();
        _cardTypeName = AlchemyStarsCardArt.GetCardTypeName(card);
        _alts = AlchemyStarsCardArt.GetAlternateArts(card);
        var savedId = AlchemyStarsCardArtSettingsStore.GetSelectedAlternateArtId(_cardTypeName);
        var savedIndex = IndexOf(savedId);
        if (savedIndex >= 0)
        {
            _viewingAlternate = true;
            _altIndex = savedIndex;
        }
        else
        {
            _viewingAlternate = false;
            _altIndex = 0;
        }

        ApplyPreview();
        RefreshUi();
    }

    public void ApplyPreview()
    {
        if (string.IsNullOrWhiteSpace(_cardTypeName) || _alts.Count == 0)
        {
            AlchemyStarsCardArt.ClearInspectPreview();
            return;
        }

        AlchemyStarsCardArt.SetInspectPreview(_cardTypeName, CurrentPreviewId());
    }

    public void OnOpened()
    {
        EnsureUi();
        TryBindCurrentCard();
        if (_viewTickbox != null)
            _viewTickbox.Enable();
        RefreshUi();
        Callable.From(RefreshUi).CallDeferred();
    }

    public void OnClosed()
    {
        AlchemyStarsCardArt.ClearInspectPreview();
        if (_viewTickbox != null)
            _viewTickbox.Disable();
        SetInteractable(false);
        SetVisible(false);
    }

    private void OnViewToggled(NTickbox tickbox)
    {
        _viewingAlternate = tickbox.IsTicked;
        if (_viewingAlternate && _alts.Count == 0)
            _viewingAlternate = false;

        ApplyPreview();
        RefreshUi();
        _screen.UpdateCardDisplay();
    }

    private void OnPrevPressed() => Cycle(-1);

    private void OnNextPressed() => Cycle(1);

    private void OnSelectPressed()
    {
        if (string.IsNullOrWhiteSpace(_cardTypeName) || _alts.Count == 0)
            return;
        if (IsCurrentArtAlreadySelected())
            return;

        var selectedId = _viewingAlternate ? CurrentAlternateId() : null;
        AlchemyStarsCardArtSettingsStore.SetSelectedAlternateArtId(_cardTypeName, selectedId);
        SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
        AlchemyStarsCardArt.ReloadDisplayedCards(_cardTypeName);
        RefreshSelectLabel();
    }

    private void OnSelectHoverEntered()
    {
        if (_selectLabel == null || IsCurrentArtAlreadySelected())
            return;
        _selectLabel.Modulate = StsColors.gold;
    }

    private void OnSelectHoverExited() => RefreshSelectLabel();

    private void Cycle(int delta)
    {
        if (!_viewingAlternate || _alts.Count <= 1)
            return;

        _altIndex = (_altIndex + delta) % _alts.Count;
        if (_altIndex < 0)
            _altIndex += _alts.Count;

        ApplyPreview();
        RefreshUi();
        _screen.UpdateCardDisplay();
        SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
    }

    private void RefreshUi()
    {
        if (!_uiReady || _viewTickbox == null || _viewLabel == null)
            return;

        var hasAlts = _alts.Count > 0;
        SetVisible(hasAlts);
        if (!hasAlts)
            return;

        SetTickboxTicked(_viewTickbox, _viewingAlternate);
        if (_viewLabel != null)
            SetLabelText(_viewLabel, Loc.ViewAlternateArt());
        if (_nameLabel != null && _viewingAlternate)
            SetLabelText(_nameLabel, CurrentAlternateId() ?? "");
        RefreshSelectLabel();
        LayoutRelativeToUpgrade();
        SetInteractable(true);
    }

    private void RefreshSelectLabel()
    {
        if (_selectLabel == null)
            return;

        var alreadySelected = IsCurrentArtAlreadySelected();
        SetLabelText(_selectLabel, alreadySelected ? Loc.CurrentArtSelected() : Loc.SelectCurrentArt());
        _selectLabel.Modulate = alreadySelected ? StsColors.gray : StsColors.cream;
        _selectLabel.MouseDefaultCursorShape = alreadySelected
            ? Control.CursorShape.Arrow
            : Control.CursorShape.PointingHand;
    }

    private bool IsCurrentArtAlreadySelected()
    {
        if (string.IsNullOrWhiteSpace(_cardTypeName))
            return false;

        var savedId = AlchemyStarsCardArtSettingsStore.GetSelectedAlternateArtId(_cardTypeName);
        var currentId = _viewingAlternate ? CurrentAlternateId() : null;
        return string.Equals(savedId, currentId, StringComparison.Ordinal);
    }

    private void LayoutRelativeToUpgrade()
    {
        if (_row == null || _viewTickbox == null)
            return;

        CompactTickbox(_viewTickbox);
        UpdateCycleVisibility();
        _row.Visible = true;
        _row.ZIndex = 80;
        _row.ResetSize();

        var upgrade = _screen.GetNode<NTickbox>("%Upgrade");
        var upgradeLabel = _screen.GetNode<MegaLabel>("%ShowUpgradeLabel");
        var visualRect = GetTickboxVisualRect(upgrade);
        var origin = new Vector2(
            visualRect.End.X + MeasureTextWidth(upgradeLabel) + AfterUpgradeGap,
            visualRect.Position.Y);

        var rowSize = _row.GetCombinedMinimumSize();
        var viewport = _screen.GetViewportRect();
        var maxX = viewport.Size.X - rowSize.X - ScreenPadding;
        if (origin.X > maxX)
        {
            // 同一行会出界时，换到「查看升级」正下方左对齐，避免挤出屏幕。
            origin = new Vector2(visualRect.Position.X, visualRect.End.Y + 8f);
        }

        _row.GlobalPosition = origin;
        _row.ResetSize();
    }

    private void SetVisible(bool visible)
    {
        if (_row != null)
            _row.Visible = visible;
        UpdateCycleVisibility();
    }

    private void UpdateCycleVisibility()
    {
        var rowVisible = _row is { Visible: true };
        if (_prevButton != null)
            _prevButton.Visible = rowVisible && _viewingAlternate && _alts.Count > 1;
        if (_nextButton != null)
            _nextButton.Visible = rowVisible && _viewingAlternate && _alts.Count > 1;
        if (_nameLabel != null)
            _nameLabel.Visible = rowVisible && _viewingAlternate;
    }

    private void SetInteractable(bool enabled)
    {
        if (_viewTickbox != null)
        {
            if (enabled)
                _viewTickbox.Enable();
            else
                _viewTickbox.Disable();
        }

        var filter = enabled ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
        if (_prevButton != null)
            _prevButton.MouseFilter = filter;
        if (_nextButton != null)
            _nextButton.MouseFilter = filter;
        if (_selectLabel != null)
        {
            _selectLabel.MouseFilter = filter;
            if (enabled)
                RefreshSelectLabel();
        }
    }

    private void TryBindCurrentCard()
    {
        var cards = _screen._cards;
        if (cards is not { Count: > 0 })
            return;

        var index = Math.Clamp(_screen._index, 0, cards.Count - 1);
        BindCard(cards[index]);
    }

    private static MegaLabel CreateStyledLabel(MegaLabel source, string name, string text)
    {
        var label = new MegaLabel
        {
            Name = name,
            UniqueNameInOwner = false,
            AutoSizeEnabled = false,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            Modulate = source.Modulate,
            ZIndex = 80,
        };
        CopyLabelStyle(source, label);
        FitLabelSize(label);
        return label;
    }

    private static MegaLabel CreateClickableLabel(MegaLabel source, string name, string text, Action pressed)
    {
        var label = CreateStyledLabel(source, name, text);
        label.MouseFilter = Control.MouseFilterEnum.Stop;
        label.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
        label.GuiInput += input =>
        {
            if (input is not InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left })
                return;
            pressed();
            label.AcceptEvent();
        };
        return label;
    }

    private static void CopyLabelStyle(MegaLabel source, MegaLabel target)
    {
        if (source.HasThemeFontOverride("font"))
            target.AddThemeFontOverride("font", source.GetThemeFont("font"));
        var fontSize = source.GetThemeFontSize("font_size");
        if (fontSize > 0)
            target.AddThemeFontSizeOverride("font_size", fontSize);
        if (source.HasThemeColorOverride("font_color"))
            target.AddThemeColorOverride("font_color", source.GetThemeColor("font_color"));
        if (source.HasThemeColorOverride("font_outline_color"))
            target.AddThemeColorOverride("font_outline_color", source.GetThemeColor("font_outline_color"));
        if (source.HasThemeConstantOverride("outline_size"))
            target.AddThemeConstantOverride("outline_size", source.GetThemeConstant("outline_size"));
    }

    private static void FitLabelSize(MegaLabel label)
    {
        var font = label.GetThemeFont("font");
        var fontSize = label.GetThemeFontSize("font_size");
        if (fontSize <= 0)
            fontSize = 28;
        var size = font.GetStringSize(label.Text, HorizontalAlignment.Left, -1, fontSize);
        label.CustomMinimumSize = new Vector2(Math.Max(size.X, 28f), Math.Max(size.Y, 36f));
        label.Size = label.CustomMinimumSize;
    }

    private static void ResetLayoutForBox(Control control, string name)
    {
        control.Name = name;
        control.UniqueNameInOwner = false;
        control.ZIndex = 80;
        control.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        control.AnchorLeft = 0f;
        control.AnchorTop = 0f;
        control.AnchorRight = 0f;
        control.AnchorBottom = 0f;
        control.OffsetLeft = 0f;
        control.OffsetTop = 0f;
        control.OffsetRight = 0f;
        control.OffsetBottom = 0f;
        control.GrowHorizontal = Control.GrowDirection.End;
        control.GrowVertical = Control.GrowDirection.End;
        control.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        control.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
    }

    private static void StripUniqueNames(Node node)
    {
        node.UniqueNameInOwner = false;
        foreach (var child in node.GetChildren())
            StripUniqueNames(child);
    }

    private static void CompactTickbox(NTickbox tickbox)
    {
        var visual = FindDescendant(tickbox, "TickboxVisuals") as Control;
        var size = visual != null
            ? visual.GetRect().Size
            : new Vector2(TickboxFallbackSize, TickboxFallbackSize);
        if (size.X < 8f || size.Y < 8f)
            size = new Vector2(TickboxFallbackSize, TickboxFallbackSize);

        if (visual != null)
        {
            ResetLayoutForBox(visual, visual.Name);
            visual.Position = Vector2.Zero;
            visual.Size = size;
        }

        tickbox.CustomMinimumSize = size;
        tickbox.Size = size;
        tickbox.ClipContents = true;
    }

    private static Rect2 GetTickboxVisualRect(NTickbox tickbox)
    {
        if (FindDescendant(tickbox, "TickboxVisuals") is Control visual)
            return visual.GetGlobalRect();
        return tickbox.GetGlobalRect();
    }

    private static void HideOwnedLabels(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is Label)
                ((CanvasItem)child).Visible = false;
            else
                HideOwnedLabels(child);
        }
    }

    private static float MeasureTextWidth(MegaLabel label)
    {
        var font = label.GetThemeFont("font");
        var fontSize = label.GetThemeFontSize("font_size");
        if (fontSize <= 0)
            fontSize = 28;
        return font.GetStringSize(label.Text, HorizontalAlignment.Left, -1, fontSize).X;
    }

    private static void SetLabelText(MegaLabel label, string text)
    {
        label.Text = text;
        if (label.AutoSizeEnabled)
            label.SetTextAutoSize(text);
        FitLabelSize(label);
    }

    private static void SetTickboxTicked(NTickbox tickbox, bool ticked)
    {
        if (!WireTickboxVisuals(tickbox))
            return;

        tickbox.IsTicked = ticked;
    }

    private static bool WireTickboxVisuals(NTickbox tickbox)
    {
        if (GodotObject.IsInstanceValid(tickbox._tickedImage) &&
            GodotObject.IsInstanceValid(tickbox._notTickedImage))
            return true;

        var visuals = FindDescendant(tickbox, "TickboxVisuals") as Control;
        var ticked = FindDescendant(tickbox, "Ticked") as Control;
        var notTicked = FindDescendant(tickbox, "NotTicked") as Control;
        if (visuals == null || ticked == null || notTicked == null)
        {
            Entry.Logger.Warn("异画勾选框复制后找不到勾选图节点，已跳过勾选状态同步。");
            return false;
        }

        tickbox._imageContainer = visuals;
        tickbox._tickedImage = ticked;
        tickbox._notTickedImage = notTicked;
        tickbox._baseScale = visuals.Scale;
        if (visuals.Material is ShaderMaterial hsv)
            tickbox._hsv = hsv;
        return true;
    }

    private static Node? FindDescendant(Node root, string name)
    {
        if (root.Name == name)
            return root;

        foreach (var child in root.GetChildren())
        {
            var found = FindDescendant(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    private string? CurrentPreviewId() =>
        _viewingAlternate ? CurrentAlternateId() : "";

    private string? CurrentAlternateId() =>
        _alts.Count == 0 ? null : _alts[Math.Clamp(_altIndex, 0, _alts.Count - 1)].Id;

    private int IndexOf(string? alternateArtId)
    {
        if (string.IsNullOrWhiteSpace(alternateArtId))
            return -1;

        for (var i = 0; i < _alts.Count; i++)
        {
            if (string.Equals(_alts[i].Id, alternateArtId, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    private static class Loc
    {
        public static string ViewAlternateArt() =>
            IsChinese() ? "查看异画" : "View Alternate Art";

        public static string SelectCurrentArt() =>
            IsChinese() ? "选择当前异画" : "Select Current Art";

        public static string CurrentArtSelected() =>
            IsChinese() ? "已选择当前异画" : "Current Art Selected";

        private static bool IsChinese() =>
            TranslationServer.GetLocale().StartsWith("zh", StringComparison.OrdinalIgnoreCase);
    }
}
