using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Powers;
using MinionLib.Minion;
using MinionLib.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Minions;

/// <summary>
/// 光之剑甲：奥提斯式召唤物，以前排守护者替主人承伤；消失时为卡牌召唤者生成森属性强化格。
/// 视觉暂复用原版奥提斯 Spine（creature_visuals/osty）。
/// </summary>
[RegisterMonster]
public sealed class AlchemyStarsLightSwordArmorMinion : AlchemyStarsModMinionTemplate
{
    public const int DefaultHp = 10;

    public override int MinInitialHp => DefaultHp;

    public override int MaxInitialHp => DefaultHp;

    /// <summary>复用原版奥提斯战斗视觉场景。</summary>
    public override MonsterAssetProfile AssetProfile => ContentAssetProfiles.Monster("osty");

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        // 与 Osty.GenerateAnimator 对齐，才能正确播放 idle / hurt / die / revive 等动画。
        var idle = new AnimState("idle_loop", isLooping: true);
        var cast = new AnimState("cast");
        var attack = new AnimState("attack");
        var poke = new AnimState("attack_poke");
        var hurt = new AnimState("hurt");
        var die = new AnimState("die");
        var deadLoop = new AnimState("dead_loop", isLooping: true);
        var revive = new AnimState("revive");

        idle.AddBranch("Hit", hurt);
        cast.NextState = idle;
        cast.AddBranch("Hit", hurt);
        attack.NextState = idle;
        attack.AddBranch("Hit", hurt);
        poke.NextState = idle;
        poke.AddBranch("Hit", hurt);
        hurt.NextState = idle;
        hurt.AddBranch("Hit", hurt);
        die.NextState = deadLoop;
        revive.NextState = idle;

        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("attack_poke", poke);
        animator.AddAnyState("Revive", revive);
        return animator;
    }

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
