using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using AlchemyStars.UI;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 注册光能/转色栏战斗 UI。
/// </summary>
public static class LightMechanicUiBootstrap
{
    private const string UiLocalId = "light_mechanic_bar";
    private const string BarNodeName = "AlchemyStarsLightMechanicBar";

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        // 必须注册次级资源，否则 ModSecondaryResourceRegistry.HasAny 为 false，
        // RitsuLib 的战斗 UI 补丁不会运行，RegisterCombatUi 也不会被刷新。
        registry.Register(
            UiLocalId,
            new SecondaryResourceDefinition(
                defaultAmount: 0,
                persistencePolicy: SecondaryResourcePersistencePolicy.Combat));

        registry.AlwaysShowInCombatUiForCharacter<AlchemyStarsCharacter>(UiLocalId);

        registry.RegisterCombatUi(
            UiLocalId,
            _ => CreateBar(),
            ctx =>
            {
                if (ctx.Node is not LightMechanicUiBar bar)
                    return;

                // 主战斗 UI 只绑定本地存活玩家，避免多人时显示/覆盖成队友状态。
                if (ctx.Player == null ||
                    !ShouldUpdateLocalCombatUi(ctx.Player) ||
                    !LightMechanic.IsMechanicActive(ctx.Player))
                {
                    bar.Visible = false;
                    return;
                }

                var maxSlots = LightMechanic.GetSlotLimit(ctx.Player);
                LightMechanicCombatState.TryGet(ctx.Player, out var state);
                bar.Refresh(state, maxSlots);
            },
            options: new NodeAttachmentOptions
            {
                Name = BarNodeName,
                // 直接挂到战斗 UI 根节点，便于锚定画面正左方。
                AttachParentSelector = static parent => parent,
                SetupTiming = NodeAttachmentSetupTiming.AfterAdd,
                DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReplaceExistingByName,
            });
    }

    /// <summary>
    /// 刷新指定玩家的光能/转色 UI。
    /// 主战斗栏仅在本地玩家变化时更新，避免多人时被队友状态覆盖。
    /// </summary>
    public static void RefreshForPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var ui = NCombatRoom.Instance?.Ui;
        if (ui == null || !GodotObject.IsInstanceValid(ui))
            return;

        if (!ShouldUpdateLocalCombatUi(player))
            return;

        SecondaryResourceUiRuntime.UpdateCombatUi(ui, player);
    }

    private static bool ShouldUpdateLocalCombatUi(Player player)
    {
        if (LocalContext.IsMe(player))
            return true;

        // 单人 / LocalContext 尚未绑定时：仅有一名玩家则刷新。
        return player.RunState?.Players.Count == 1;
    }

    private static LightMechanicUiBar CreateBar()
    {
        var bar = new LightMechanicUiBar
        {
            Name = BarNodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            // 保持默认层级，避免盖住遗物/能力悬停说明。
            ZIndex = 0,
        };
        bar.ApplyLeftScreenLayout();
        return bar;
    }
}
