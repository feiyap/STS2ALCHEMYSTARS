using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using AlchemyStars.Powers;
using MinionLib.Minion;
using MinionLib.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace AlchemyStars.Minions;

/// <summary>
/// 光之剑甲：奥提斯式召唤物，以前排守护者替主人承伤；消失时为卡牌召唤者生成森属性强化格。
/// 视觉使用 scenes/monsters/frogSpine.tscn。该骨骼目前只有 idle。
/// </summary>
[RegisterMonster]
public sealed class AlchemyStarsLightSwordArmorMinion : AlchemyStarsModMinionTemplate
{
    public const int DefaultHp = 10;

    private const string VisualsScenePath = $"{Entry.ResPath}/scenes/monsters/frogSpine.tscn";

    public override int MinInitialHp => DefaultHp;

    public override int MaxInitialHp => DefaultHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: VisualsScenePath);

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(VisualsScenePath);

    /// <summary>
    /// frog 骨骼只有 idle。攻击、施法、受击和死亡都回到这段循环，避免去播不存在的奥提斯动画。
    /// </summary>
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller) =>
        ModAnimStateMachines.Standard(controller, idleName: "idle");

    public override async Task OnSummon(
        PlayerChoiceContext choiceContext,
        Player owner,
        MinionSummonOptions options)
    {
        var self = Creature;

        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);

        await PowerCmd.Apply<MinionGuardianPower>(
            choiceContext,
            self,
            1m,
            owner.Creature,
            options.Source);

        var deathPower = await PowerCmd.Apply<AlchemyStarsLightSwordArmorPower>(
            choiceContext,
            self,
            1m,
            owner.Creature,
            options.Source);

        var rewardPlayer = (options.Source as CardModel)?.Owner ?? owner;
        deathPower?.Configure(rewardPlayer);
    }
}
