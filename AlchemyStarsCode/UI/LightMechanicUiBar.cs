using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Keywords;

namespace AlchemyStars.UI;

/// <summary>
/// 战斗画面左侧的光能栏与转色栏 UI（无外框、无文本，只显示图案）。
/// 光能与转色各预留最多 2 行（每行 4 槽，适配升级后 8 槽上限）；悬停时显示对应关键词说明。
/// </summary>
public partial class LightMechanicUiBar : Control
{
    private const float LightSlotSize = 52f;
    private const float CellSlotSize = 50f;
    private const float SectionGap = 12f;
    private const float RowGap = 6f;
    private const float SlotGap = 6f;
    private const float LeftMargin = 14f;
    private const float VerticalNudge = -48f;
    private const int SlotsPerRow = 4;
    private const int ReservedRows = 2;

    private readonly VBoxContainer _root = new();
    private readonly VBoxContainer _lightSection = new();
    private readonly VBoxContainer _cellSection = new();

    public LightMechanicUiBar()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        _root.MouseFilter = MouseFilterEnum.Ignore;
        _root.AddThemeConstantOverride("separation", (int)SectionGap);

        ConfigureSection(_lightSection);
        ConfigureSection(_cellSection);

        _root.AddChild(_lightSection);
        _root.AddChild(_cellSection);
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
        ClearSection(_lightSection);
        ClearSection(_cellSection);

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

        PopulateLightSection(state, lightMax, lightCount);
        PopulateCellSection(state, cellMax, cellCount);

        var width = SlotsPerRow * Mathf.Max(LightSlotSize, CellSlotSize)
                    + (SlotsPerRow - 1) * SlotGap;
        var lightHeight = ReservedRows * LightSlotSize + (ReservedRows - 1) * RowGap;
        var cellHeight = ReservedRows * CellSlotSize + (ReservedRows - 1) * RowGap;
        var height = lightHeight + SectionGap + cellHeight;
        CustomMinimumSize = new Vector2(width, height);
        Size = CustomMinimumSize;

        ApplyLeftScreenLayout();
    }

    private void PopulateLightSection(LightMechanicCombatState? state, int lightMax, int lightCount)
    {
        for (var row = 0; row < ReservedRows; row++)
        {
            var rowStart = row * SlotsPerRow;
            if (rowStart >= lightMax)
            {
                // 当前上限未用到该行时，仍占位以预留升级后的 8 槽布局。
                _lightSection.AddChild(CreateRowSpacer(LightSlotSize));
                continue;
            }

            var rowBox = CreateSlotRow();
            var rowEnd = Math.Min(rowStart + SlotsPerRow, lightMax);
            for (var i = rowStart; i < rowEnd; i++)
            {
                LightElement? element = i < lightCount ? state!.LightEnergy.Items[i] : null;
                rowBox.AddChild(CreateLightSlot(element));
            }

            _lightSection.AddChild(rowBox);
        }
    }

    private void PopulateCellSection(LightMechanicCombatState? state, int cellMax, int cellCount)
    {
        for (var row = 0; row < ReservedRows; row++)
        {
            var rowStart = row * SlotsPerRow;
            if (rowStart >= cellMax)
            {
                _cellSection.AddChild(CreateRowSpacer(CellSlotSize));
                continue;
            }

            var rowBox = CreateSlotRow();
            var rowEnd = Math.Min(rowStart + SlotsPerRow, cellMax);
            for (var i = rowStart; i < rowEnd; i++)
            {
                if (i < cellCount)
                {
                    var cell = state!.AttributeCells.Items[i];
                    rowBox.AddChild(CreateCellSlot(cell.Element, cell.Kind));
                }
                else
                {
                    rowBox.AddChild(CreateCellSlot(element: null, kind: null));
                }
            }

            _cellSection.AddChild(rowBox);
        }
    }

    private static void ConfigureSection(VBoxContainer section)
    {
        section.MouseFilter = MouseFilterEnum.Ignore;
        section.AddThemeConstantOverride("separation", (int)RowGap);
    }

    private static HBoxContainer CreateSlotRow()
    {
        var row = new HBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Begin,
        };
        row.AddThemeConstantOverride("separation", (int)SlotGap);
        return row;
    }

    private static Control CreateRowSpacer(float slotSize) =>
        new Control
        {
            CustomMinimumSize = new Vector2(0f, slotSize),
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };

    private static Control CreateLightSlot(LightElement? element)
    {
        Control slot;
        if (element is { } value)
        {
            var texture = LightMechanicUiAssets.Load(LightMechanicUiAssets.GetLightIconPath(value));
            slot = texture != null
                ? CreatePatternSlot(texture, LightSlotSize, circular: true, Colors.White)
                : CreateEmptySlot(LightSlotSize, circular: true);
        }
        else
        {
            slot = CreateEmptySlot(LightSlotSize, circular: true);
        }

        AttachHoverTips(slot, BuildLightHoverTips(element));
        return slot;
    }

    private static Control CreateCellSlot(LightElement? element, AttributeCellKind? kind)
    {
        Control slot;
        if (element is { } value)
        {
            var texture = LightMechanicUiAssets.Load(LightMechanicUiAssets.GetCellTexturePath(value));
            slot = texture != null
                ? CreatePatternSlot(texture, CellSlotSize, circular: false, ResolveCellModulate(kind), kind)
                : CreateEmptySlot(CellSlotSize, circular: false);
        }
        else
        {
            slot = CreateEmptySlot(CellSlotSize, circular: false);
        }

        AttachHoverTips(slot, BuildCellHoverTips(element, kind));
        return slot;
    }

    private static IReadOnlyList<IHoverTip> BuildLightHoverTips(LightElement? element)
    {
        var keywordId = element switch
        {
            LightElement.Forest => AlchemyStarsKeywordIds.ForestLightEnergy,
            LightElement.Thunder => AlchemyStarsKeywordIds.ThunderLightEnergy,
            LightElement.Water => AlchemyStarsKeywordIds.WaterLightEnergy,
            LightElement.Fire => AlchemyStarsKeywordIds.FireLightEnergy,
            LightElement.Prismatic => AlchemyStarsKeywordIds.Prismatic,
            _ => AlchemyStarsKeywordIds.LightEnergy,
        };
        return [ModKeywordRegistry.CreateHoverTip(keywordId)];
    }

    private static IReadOnlyList<IHoverTip> BuildCellHoverTips(LightElement? element, AttributeCellKind? kind)
    {
        var tips = new List<IHoverTip>
        {
            ModKeywordRegistry.CreateHoverTip(ResolveAttributeCellKeywordId(element)),
        };

        switch (kind)
        {
            case AttributeCellKind.Enhanced:
                tips.Add(ModKeywordRegistry.CreateHoverTip(AlchemyStarsKeywordIds.EnhancedCell));
                break;
            case AttributeCellKind.Prism:
                tips.Add(ModKeywordRegistry.CreateHoverTip(AlchemyStarsKeywordIds.PrismCell));
                break;
            case AttributeCellKind.Dark:
                tips.Add(ModKeywordRegistry.CreateHoverTip(AlchemyStarsKeywordIds.DarkCell));
                break;
        }

        return tips;
    }

    private static string ResolveAttributeCellKeywordId(LightElement? element) => element switch
    {
        LightElement.Forest => AlchemyStarsKeywordIds.ForestAttributeCell,
        LightElement.Thunder => AlchemyStarsKeywordIds.ThunderAttributeCell,
        LightElement.Water => AlchemyStarsKeywordIds.WaterAttributeCell,
        LightElement.Fire => AlchemyStarsKeywordIds.FireAttributeCell,
        LightElement.Prismatic => AlchemyStarsKeywordIds.Prismatic,
        _ => AlchemyStarsKeywordIds.AttributeCell,
    };

    private static void AttachHoverTips(Control slot, IReadOnlyList<IHoverTip> tips)
    {
        slot.MouseFilter = MouseFilterEnum.Stop;
        slot.MouseEntered += () =>
        {
            NHoverTipSet.Remove(slot);
            // Right：说明显示在槽位右侧（左侧屏幕边缘用 Left 会飞出画面）。
            NHoverTipSet.CreateAndShow(slot, tips, HoverTipAlignment.Right);
        };
        slot.MouseExited += () => NHoverTipSet.Remove(slot);
        slot.TreeExiting += () => NHoverTipSet.Remove(slot);
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
            MouseFilter = MouseFilterEnum.Stop,
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
            MouseFilter = MouseFilterEnum.Stop,
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

    private static void ClearSection(Node section)
    {
        foreach (var child in section.GetChildren())
        {
            if (child is Container row)
            {
                foreach (var slot in row.GetChildren())
                {
                    if (slot is Control control)
                        NHoverTipSet.Remove(control);
                }
            }

            child.QueueFree();
        }
    }
}
