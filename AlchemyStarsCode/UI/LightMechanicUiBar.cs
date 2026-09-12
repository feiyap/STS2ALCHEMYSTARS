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

    private int[] _lightFingerprint = [];
    private int[] _cellFingerprint = [];
    private static CanvasItemMaterial? _additiveFxMaterial;

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
        if (maxSlots <= 0)
        {
            ClearSection(_lightSection);
            ClearSection(_cellSection);
            _lightFingerprint = [];
            _cellFingerprint = [];
            Visible = false;
            return;
        }

        Visible = true;

        var lightMax = state?.LightEnergy.MaxSlots ?? maxSlots;
        var cellMax = state?.AttributeCells.MaxSlots ?? maxSlots;
        var lightCount = state?.LightEnergy.Count ?? 0;
        var cellCount = state?.AttributeCells.Count ?? 0;
        var nextLight = CaptureLightFingerprint(state, lightMax, lightCount);
        var nextCell = CaptureCellFingerprint(state, cellMax, cellCount);

        // 状态未变时跳过重建，避免把正在播放的出现特效拆掉。
        if (_lightFingerprint.AsSpan().SequenceEqual(nextLight) &&
            _cellFingerprint.AsSpan().SequenceEqual(nextCell))
        {
            ApplyLeftScreenLayout();
            return;
        }

        var appear = new bool[cellMax];
        for (var i = 0; i < cellMax; i++)
        {
            if (nextCell[i] < 0)
                continue;

            appear[i] = i >= _cellFingerprint.Length || _cellFingerprint[i] != nextCell[i];
        }

        _lightFingerprint = nextLight;
        _cellFingerprint = nextCell;

        ClearSection(_lightSection);
        ClearSection(_cellSection);

        PopulateLightSection(state, lightMax, lightCount);
        PopulateCellSection(state, cellMax, cellCount, appear);

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

    private void PopulateCellSection(
        LightMechanicCombatState? state,
        int cellMax,
        int cellCount,
        bool[] appear)
    {
        var appearOrder = 0;
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
                    var playAppear = i < appear.Length && appear[i];
                    var delay = 0f;
                    if (playAppear)
                    {
                        delay = appearOrder * 0.045f;
                        appearOrder++;
                    }

                    rowBox.AddChild(CreateCellSlot(cell.Element, cell.Kind, playAppear, delay));
                }
                else
                {
                    rowBox.AddChild(CreateCellSlot(element: null, kind: null, playAppear: false, appearDelay: 0f));
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

    private static Control CreateCellSlot(
        LightElement? element,
        AttributeCellKind? kind,
        bool playAppear,
        float appearDelay)
    {
        Control slot;
        if (element is { } value)
        {
            var texture = LightMechanicUiAssets.Load(LightMechanicUiAssets.GetCellTexturePath(value));
            slot = texture != null
                ? CreateFilledCellSlot(texture, kind, playAppear, appearDelay)
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

    private static Control CreateFilledCellSlot(
        Texture2D texture,
        AttributeCellKind? kind,
        bool playAppear,
        float appearDelay)
    {
        var size = CellSlotSize;
        var modulate = ResolveCellModulate(kind);
        var slot = new Control
        {
            CustomMinimumSize = new Vector2(size, size),
            Size = new Vector2(size, size),
            MouseFilter = MouseFilterEnum.Stop,
            ClipContents = false,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
        };

        var bg = new Panel { MouseFilter = MouseFilterEnum.Ignore };
        StretchFullRect(bg);
        var radius = 4;
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0.12f),
            CornerRadiusBottomLeft = radius,
            CornerRadiusBottomRight = radius,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
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

        bg.AddThemeStyleboxOverride("panel", style);
        slot.AddChild(bg);

        var pattern = new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Modulate = playAppear ? new Color(modulate.R, modulate.G, modulate.B, 0f) : modulate,
            PivotOffset = new Vector2(size * 0.5f, size * 0.5f),
        };
        StretchFullRect(pattern);
        pattern.OffsetLeft = 1;
        pattern.OffsetTop = 1;
        pattern.OffsetRight = -1;
        pattern.OffsetBottom = -1;
        slot.AddChild(pattern);

        if (playAppear)
        {
            var ray = CreateFxLayer(LightMechanicUiAssets.CellRayPath, size * 1.75f);
            var spark = CreateFxLayer(LightMechanicUiAssets.CellSparkPath, size * 1.4f);
            if (ray != null)
                slot.AddChild(ray);
            if (spark != null)
                slot.AddChild(spark);

            Callable.From(() => PlayCellAppear(slot, pattern, ray, spark, appearDelay, modulate))
                .CallDeferred();
        }

        return slot;
    }

    private static TextureRect? CreateFxLayer(string path, float size)
    {
        var texture = LightMechanicUiAssets.Load(path);
        if (texture == null)
            return null;

        var half = size * 0.5f;
        var layer = new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = AdditiveFxMaterial,
            CustomMinimumSize = new Vector2(size, size),
            Size = new Vector2(size, size),
            PivotOffset = new Vector2(half, half),
            Modulate = new Color(1f, 1f, 1f, 0f),
        };
        layer.SetAnchorsPreset(LayoutPreset.Center);
        layer.OffsetLeft = -half;
        layer.OffsetTop = -half;
        layer.OffsetRight = half;
        layer.OffsetBottom = half;
        return layer;
    }

    private static void PlayCellAppear(
        Control slot,
        TextureRect pattern,
        TextureRect? ray,
        TextureRect? spark,
        float delay,
        Color patternModulate)
    {
        if (!GodotObject.IsInstanceValid(slot) || !GodotObject.IsInstanceValid(pattern))
            return;

        var tween = slot.CreateTween();
        tween.SetParallel(true);

        pattern.PivotOffset = new Vector2(CellSlotSize * 0.5f, CellSlotSize * 0.5f);
        pattern.Scale = new Vector2(0.55f, 0.55f);

        tween.TweenProperty(pattern, "modulate", patternModulate, 0.22)
            .SetDelay(delay)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(pattern, "scale", Vector2.One, 0.24)
            .SetDelay(delay)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        if (ray != null && GodotObject.IsInstanceValid(ray))
        {
            ray.Scale = new Vector2(0.18f, 1.15f);
            tween.TweenProperty(ray, "modulate", new Color(1f, 1f, 1f, 0.95f), 0.05)
                .SetDelay(delay);
            tween.TweenProperty(ray, "modulate:a", 0f, 0.22)
                .SetDelay(delay + 0.08);
            tween.TweenProperty(ray, "scale", new Vector2(1.85f, 0.4f), 0.28)
                .SetDelay(delay)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }

        if (spark != null && GodotObject.IsInstanceValid(spark))
        {
            spark.Scale = new Vector2(0.22f, 0.22f);
            tween.TweenProperty(spark, "modulate", Colors.White, 0.07)
                .SetDelay(delay);
            tween.TweenProperty(spark, "modulate:a", 0f, 0.22)
                .SetDelay(delay + 0.1);
            tween.TweenProperty(spark, "scale", new Vector2(1.5f, 1.5f), 0.3)
                .SetDelay(delay)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }

        tween.Chain().TweenCallback(Callable.From(() =>
        {
            if (ray != null && GodotObject.IsInstanceValid(ray))
                ray.QueueFree();
            if (spark != null && GodotObject.IsInstanceValid(spark))
                spark.QueueFree();
        }));
    }

    private static void StretchFullRect(Control node)
    {
        node.SetAnchorsPreset(LayoutPreset.FullRect);
        node.OffsetLeft = 0;
        node.OffsetTop = 0;
        node.OffsetRight = 0;
        node.OffsetBottom = 0;
    }

    private static CanvasItemMaterial AdditiveFxMaterial =>
        _additiveFxMaterial ??= new CanvasItemMaterial
        {
            BlendMode = CanvasItemMaterial.BlendModeEnum.Add,
        };

    private static int[] CaptureLightFingerprint(LightMechanicCombatState? state, int lightMax, int lightCount)
    {
        var result = new int[lightMax];
        Array.Fill(result, -1);
        if (state == null)
            return result;

        var limit = Math.Min(lightCount, lightMax);
        for (var i = 0; i < limit; i++)
            result[i] = (int)state.LightEnergy.Items[i];

        return result;
    }

    private static int[] CaptureCellFingerprint(LightMechanicCombatState? state, int cellMax, int cellCount)
    {
        var result = new int[cellMax];
        Array.Fill(result, -1);
        if (state == null)
            return result;

        var limit = Math.Min(cellCount, cellMax);
        for (var i = 0; i < limit; i++)
        {
            var cell = state.AttributeCells.Items[i];
            result[i] = ((int)cell.Element << 8) | (int)cell.Kind;
        }

        return result;
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
