using System;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.Minion;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace AlchemyStars.Minions;

/// <summary>
/// MinionLib 召唤物在 RitsuLib 下的基础模板（对齐 ModMonsterTemplate / ModMinionTemplate）。
/// </summary>
#pragma warning disable CS0618
public abstract class AlchemyStarsModMinionTemplate : MinionModel, IModMonsterAssetOverrides,
    IModCreatureVisualsFactory, IModMonsterCreatureVisualsFactory, IModCreatureAnimatorFactory,
    IModCreatureCombatAnimationStateMachineFactory, IModNonSpineAnimationStateMachineFactory
#pragma warning restore CS0618
{
    CreatureAnimator? IModCreatureAnimatorFactory.TryCreateCreatureAnimator(MegaSprite controller)
    {
        return SetupCustomCreatureAnimator(controller);
    }

    ModAnimStateMachine? IModCreatureCombatAnimationStateMachineFactory.TryCreateCombatAnimationStateMachine(
        Node visualsRoot)
    {
        return ResolveCombatAnimationStateMachine(visualsRoot);
    }

    NCreatureVisuals? IModCreatureVisualsFactory.TryCreateCreatureVisuals()
    {
        return TryCreateCreatureVisuals();
    }

    public virtual MonsterAssetProfile AssetProfile => MonsterAssetProfile.Empty;

    public virtual string? CustomVisualsPath => AssetProfile.VisualsScenePath;

#pragma warning disable CS0618
    NCreatureVisuals? IModMonsterCreatureVisualsFactory.TryCreateCreatureVisuals()
    {
        return TryCreateCreatureVisuals();
    }
#pragma warning restore CS0618

    ModAnimStateMachine? IModNonSpineAnimationStateMachineFactory.TryCreateNonSpineAnimationStateMachine(
        Node visualsRoot)
    {
        return ResolveCombatAnimationStateMachine(visualsRoot);
    }

    private ModAnimStateMachine? ResolveCombatAnimationStateMachine(Node visualsRoot)
    {
        var fromNew = SetupCustomCombatAnimationStateMachine(visualsRoot, this);
#pragma warning disable CS0618
        return fromNew ?? SetupCustomNonSpineAnimationStateMachine(visualsRoot, this);
#pragma warning restore CS0618
    }

    protected virtual NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return null;
    }

    protected virtual CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        return null;
    }

    protected virtual ModAnimStateMachine? SetupCustomCombatAnimationStateMachine(
        Node visualsRoot,
        MonsterModel monster)
    {
        return null;
    }

    [Obsolete("请改写 SetupCustomCombatAnimationStateMachine。")]
    protected virtual ModAnimStateMachine? SetupCustomNonSpineAnimationStateMachine(
        Node visualsRoot,
        MonsterModel monster)
    {
        return SetupCustomCombatAnimationStateMachine(visualsRoot, monster);
    }
}
