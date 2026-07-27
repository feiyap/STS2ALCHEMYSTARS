using AlchemyStars.Mechanics;
using Godot;

namespace AlchemyStars.UI;

/// <summary>
/// 战斗画面左侧的光能栏与转色栏 UI（无外框、无文本，只显示图案）。
/// 光能一行、转色一行，各自横向排列。
/// </summary>
public partial class LightMechanicUiBar : Control
{
    private const float LightSlotSize = 52f;
    private const float CellSlotSize = 50f;
    private const float RowGap = 12f;
    private const float SlotGap = 6f;
    private const float LeftMargin = 14f;
    private const float VerticalNudge = -48f;

    private readonly VBoxContainer _root = new();
    private readonly HBoxContainer _lightRow = new();
    private readonly HBoxContainer _cellRow = new();

    public LightMechanicUiBar()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        _root.MouseFilter = MouseFilterEnum.Ignore;
        _root.AddThemeConstantOverride("separation", (int)RowGap);

        ConfigureRow(_lightRow);
        ConfigureRow(_cellRow);

        _root.AddChild(_lightRow);
        _root.AddChild(_cellRow);
        AddChild(_root);
    }

    /// <summary>
    /// 锚定到战斗 UI 左侧；每次调用重置偏移，避免累加。
    /// </summary>
    public void ApplyLeftScreenLayout()
    {
        SetAnchorsPreset(LayoutPreset.CenterLeft);
        GrowHorizontal = GrowDirection.End;
        GrowVertical = GrowDirection.Both;
        OffsetLeft = LeftMargin;
        OffsetRight = LeftMargin + Mathf.Max(CustomMinimumSize.X, 1f);
        OffsetTop = VerticalNudge - Mathf.Max(CustomMinimumSize.Y, LightSlotSize) * 0.5f;
        OffsetBottom = VerticalNudge + Mathf.Max(CustomMinimumSize.Y, LightSlotSize) * 0.5f;
    }

    public void Refresh(LightMechanicCombatState? state, int maxSlots)
    {
        ClearRow(_lightRow);
        ClearRow(_cellRow);

        if (maxSlots <= 0)
        {
            Visible = false;
            return;
        }

        Visible = true;

        var lightMax = state?.LightEnergy.MaxSlots ?? maxSlots;
        var cellMax = state?.AttributeCells.MaxSlots ?? maxSlots;
        var lightCount = state?.LightEnergy.Count ?? 0;
        var cellCount = state?.AttributeCells.Count ?? 0;

        for (var i = 0; i < lightMax; i++)
        {
            LightElement? element = i < lightCount ? state!.LightEnergy.Items[i] : null;
            _lightRow.AddChild(CreateLightSlot(element));
        }

        for (var i = 0; i < cellMax; i++)
        {
            if (i < cellCount)
            {
                var cell = state!.AttributeCells.Items[i];
                _cellRow.AddChild(CreateCellSlot(cell.Element, cell.Kind));
            }
            else
            {
                _cellRow.AddChild(CreateCellSlot(element: null, kind: null));
            }
        }

        var lightWidth = lightMax * LightSlotSize + Math.Max(0, lightMax - 1) * SlotGap;
        var cellWidth = cellMax * CellSlotSize + Math.Max(0, cellMax - 1) * SlotGap;
        var width = Mathf.Max(lightWidth, cellWidth);
        var height = LightSlotSize + RowGap + CellSlotSize;
        CustomMinimumSize = new Vector2(width, height);
        Size = CustomMinimumSize;

        ApplyLeftScreenLayout();
    }

    private static void ConfigureRow(HBoxContainer row)
    {
        row.MouseFilter = MouseFilterEnum.Ignore;
        row.AddThemeConstantOverride("separation", (int)SlotGap);
        row.Alignment = BoxContainer.AlignmentMode.Begin;
    }

    private static Control CreateLightSlot(LightElement? element)
    {
        if (element is { } value)
        {
            var texture = LightMechanicUiAssets.Load(LightMechanicUiAssets.GetLightIconPath(value));
            if (texture != null)
                return CreatePatternSlot(texture, LightSlotSize, circular: true, Colors.White);
        }

        return CreateEmptySlot(LightSlotSize, circular: true);
    }

    private static Control CreateCellSlot(LightElement? element, AttributeCellKind? kind)
    {
        if (element is { } value)
        {
            var texture = LightMechanicUiAssets.Load(LightMechanicUiAssets.GetCellTexturePath(value));
            if (texture != null)
                return CreatePatternSlot(texture, CellSlotSize, circular: false, ResolveCellModulate(kind), kind);
        }

        return CreateEmptySlot(CellSlotSize, circular: false);
    }

    private static Control CreatePatternSlot(
        Texture2D texture,
        float size,
        bool circular,
        Color modulate,
        AttributeCellKind? kind = null)
    {
        var frame = new PanelContainer
        {
            CustomMinimumSize = new Vector2(size, size),
            Size = new Vector2(size, size),
            MouseFilter = MouseFilterEnum.Ignore,
            ClipContents = true,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
        };

        var radius = circular ? Mathf.RoundToInt(size * 0.5f) : 4;
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0.15f),
            BorderWidthBottom = 0,
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0,
            CornerRadiusBottomLeft = radius,
            CornerRadiusBottomRight = radius,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 3,
            ContentMarginBottom = 3,
        };

        var ring = ResolveKindRingColor(kind);
        if (ring.HasValue)
        {
            style.BorderColor = ring.Value;
            style.BorderWidthBottom = 2;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
        }

        frame.AddThemeStyleboxOverride("panel", style);
        frame.AddChild(new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Modulate = modulate,
        });
        return frame;
    }

    private static Control CreateEmptySlot(float size, bool circular)
    {
        var slot = new Panel
        {
            CustomMinimumSize = new Vector2(size, size),
            Size = new Vector2(size, size),
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
        };

        var radius = circular ? Mathf.RoundToInt(size * 0.5f) : 4;
        slot.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.10f, 0.12f, 0.35f),
            BorderColor = new Color(0.75f, 0.78f, 0.82f, 0.55f),
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            CornerRadiusBottomLeft = radius,
            CornerRadiusBottomRight = radius,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
        });
        return slot;
    }

    private static Color ResolveCellModulate(AttributeCellKind? kind) => kind switch
    {
        AttributeCellKind.Dark => new Color(0.55f, 0.55f, 0.62f, 1f),
        AttributeCellKind.Prism => new Color(1.12f, 1.12f, 1.18f, 1f),
        AttributeCellKind.Enhanced => new Color(1.08f, 1.02f, 0.88f, 1f),
        _ => Colors.White,
    };

    private static Color? ResolveKindRingColor(AttributeCellKind? kind) => kind switch
    {
        AttributeCellKind.Prism => new Color(0.85f, 0.75f, 1f, 0.9f),
        AttributeCellKind.Dark => new Color(0.30f, 0.30f, 0.40f, 0.95f),
        AttributeCellKind.Enhanced => new Color(0.95f, 0.78f, 0.35f, 0.95f),
        _ => null,
    };

    private static void ClearRow(Node row)
    {
        foreach (var child in row.GetChildren())
            child.QueueFree();
    }
}
