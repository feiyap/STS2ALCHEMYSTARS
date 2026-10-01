using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using AlchemyStars.Monsters.Powers;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace AlchemyStars.Monsters;

/// <summary>
/// 祭剑座：寂静之陵的事件 Boss。一阶段八动循环，半血锁血转阶段，二阶段循环意图 2–7。
/// 语音先不播放。
/// </summary>
[RegisterMonster]
public sealed class AlchemyStarsSwordAltar : ModMonsterTemplate
{
    private enum ChargeKind
    {
        None,
        Roar,
        Unseal,
    }

    private bool _transitionQueued;
    private bool _transitionDone;
    private bool _permanentNeutral;
    private bool _gainMightOnHit;
    private bool _maxHpRestored;
    private bool _grantUnyieldingOnNextPlayerTurn;
    private bool _roarBroken;
    private ChargeKind _charge;
    private int _blockSnapshot;
    private int _unyieldingPendingHeal;
    private int _unyieldingTempStrength;
    private int _hardToKillTurnsLeft;
    private readonly Dictionary<Player, int> _originalMaxHp = new();

    private MoveState? _fateAngerMove;
    private MoveState? _unyieldingStun;
    private MoveState? _roarStun;
    private MoveState? _unsealBreak;

    /// <summary>当前属性。null 表示无属性。</summary>
    public LightElement? CurrentElement { get; private set; }

    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1200, 1000);

    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"{Entry.ResPath}/scenes/monsters/AlchemyStarsSwordAltar.tscn");

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    /// <summary>
    /// 这套骨骼只有剧情动画，没有 idle_loop。不指定的话原版会去播不存在的 idle_loop，立绘就停在初始姿势。
    /// </summary>
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller) =>
        ModAnimStateMachines.Standard(controller, idleName: "Story_norm");

    private bool IsTough => AscensionHelper.HasAscension(AscensionLevel.ToughEnemies);

    private bool IsDeadly => AscensionHelper.HasAscension(AscensionLevel.DeadlyEnemies);

    private int PlayerCount => System.Math.Max(1, Creature.CombatState?.Players.Count ?? 1);

    private int ThunderHits => IsDeadly ? 16 : 15;

    private int HeavyDamage => IsDeadly ? 30 : 25;

    private int FireDamage => IsDeadly ? 25 : 20;

    private int Phase2StrikeDamage => IsDeadly ? 44 : 33;

    private int ZeroHitCount => IsDeadly ? 5 : 4;

    private int UnsealCounterHits => (IsDeadly ? 6 : 4) * PlayerCount;

    private int Strength => Creature.GetPowerAmount<StrengthPower>();

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await SetElement(null);
        await ApplyPower<AlchemyStarsDefectorPower>(Creature, 1m);
        await ApplyPower<AlchemyStarsFateAngerPower>(Creature, 1m);
        await ApplyPower<AlchemyStarsFourColorBurnoutPower>(Creature, 1m);
        await GainExact<ArtifactPower>(4 * PlayerCount);
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && _grantUnyieldingOnNextPlayerTurn)
        {
            _grantUnyieldingOnNextPlayerTurn = false;
            if (!_transitionQueued && !_transitionDone)
                await GrantUnyielding();
            return;
        }

        if (side != CombatSide.Enemy || !participants.Contains(Creature))
            return;

        AssertMutable();
        _blockSnapshot = Creature.Block;
        // 回忆的「受击加威能」只持续到下一轮敌方回合开始。武装发生在招式里，晚于本次清空。
        _gainMightOnHit = false;

        if (_charge == ChargeKind.Roar && Creature.Block <= 0 && _roarStun != null)
        {
            _roarBroken = true;
            SetMoveImmediate(_roarStun, true);
        }

        if (_hardToKillTurnsLeft > 0)
        {
            _hardToKillTurnsLeft--;
            if (_hardToKillTurnsLeft <= 0)
                await PowerCmd.Remove<HardToKillPower>(Creature);
        }

        if (combatState.RoundNumber % 4 == 0)
            await GainExact<ArtifactPower>(4 * PlayerCount);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Creature)
            return;

        var attackHit = props.IsPoweredAttack() && dealer?.IsPlayer == true;
        if (attackHit)
            await OnPlayerAttackHit(choiceContext, result);

        if (_charge == ChargeKind.Roar && Creature.Block <= 0 && _roarStun != null)
        {
            AssertMutable();
            _roarBroken = true;
            if (Creature.CombatState?.CurrentSide == CombatSide.Player)
                SetMoveImmediate(_roarStun, true);
        }

        if (!_transitionDone && Creature.CurrentHp <= Creature.MaxHp / 2)
            QueueTransition();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();

        MoveState Move(string id, Func<IReadOnlyList<Creature>, Task> perform, params AbstractIntent[] intents)
        {
            var move = new MoveState(id, perform, intents);
            states.Add(move);
            return move;
        }

        ConditionalBranchState Gate(string id, MoveState next)
        {
            var gate = new ConditionalBranchState(id);
            gate.AddState(_fateAngerMove!, () => _transitionQueued && !_transitionDone);
            gate.AddState(next, () => !_transitionQueued || _transitionDone);
            states.Add(gate);
            return gate;
        }

        _fateAngerMove = Move("FATE_ANGER", FateAngerMove, new BuffIntent(), new DefendIntent());
        var pioneer = Move("PIONEER", PioneerMove, new DebuffIntent(true), new BuffIntent());
        var muted = Move("MUTED", MutedMove, new DebuffIntent());
        var phase2Strike = Move("PHASE2_STRIKE", Phase2StrikeMove, new SingleAttackIntent(() => (decimal)Phase2StrikeDamage), new DebuffIntent());
        var phase2Flurry = Move("PHASE2_FLURRY", Phase2FlurryMove, new MultiAttackIntent(0, () => ZeroHitCount), new DefendIntent());
        var memory = Move("MEMORY", MemoryMove, new DefendIntent());
        var phase2Weak = Move("PHASE2_WEAK", Phase2WeakMove, new MultiAttackIntent(0, () => ZeroHitCount), new DebuffIntent());
        var roar = Move("DARKEST_ROAR", DarkestRoarMove, new DefendIntent(), new BuffIntent());
        var roarRelease = Move("DARKEST_ROAR_RELEASE", DarkestRoarReleaseMove, new MultiAttackIntent(Phase2StrikeDamage, 2));
        _roarStun = Move("DARKEST_ROAR_STUN", DarkestRoarStunMove, new StunIntent());
        var unseal = Move("MOON_UNSEAL", MoonUnsealMove, new BuffIntent());
        var unsealRelease = Move(
            "MOON_UNSEAL_RELEASE",
            MoonUnsealReleaseMove,
            new MultiAttackIntent(0, () => UnsealCounterHits));
        _unsealBreak = Move("MOON_UNSEAL_BREAK", MoonUnsealBreakMove, new BuffIntent());

        var brand = Move(
            "BRAND",
            BrandMove,
            new BuffIntent(),
            new DebuffIntent(),
            new AlchemyStarsElementSwitchIntent(LightElement.Thunder));
        var thunder = Move(
            "THUNDER_STRIKE",
            ThunderStrikeMove,
            new MultiAttackIntent(1, () => ThunderHits),
            new AlchemyStarsFrailIntent());
        var unyielding = Move(
            "UNYIELDING",
            UnyieldingMove,
            new SingleAttackIntent(0),
            new AlchemyStarsElementSwitchIntent(LightElement.Water));
        _unyieldingStun = Move("UNYIELDING_STUN", UnyieldingStunMove, new StunIntent());
        var mightStrike = Move(
            "MIGHT_STRIKE",
            MightStrikeMove,
            new SingleAttackIntent(() => (decimal)HeavyDamage),
            new BuffIntent());
        var fireBurn = Move(
            "FIRE_BURN",
            FireBurnMove,
            new SingleAttackIntent(() => (decimal)FireDamage),
            new StatusIntent(3),
            new AlchemyStarsElementSwitchIntent(LightElement.Fire));
        var afterimage = Move("AFTERIMAGE", AfterimageMove, new BuffIntent());
        var forestShell = Move(
            "FOREST_SHELL",
            ForestShellMove,
            new DefendIntent(),
            new DebuffIntent(true),
            new AlchemyStarsElementSwitchIntent(LightElement.Forest));
        var guardingBlade = Move(
            "GUARDING_BLADE",
            GuardingBladeMove,
            new SingleAttackIntent(() => (decimal)_blockSnapshot));

        _fateAngerMove.FollowUpState = pioneer;
        pioneer.FollowUpState = muted;
        muted.FollowUpState = phase2Strike;
        phase2Strike.FollowUpState = phase2Flurry;
        phase2Flurry.FollowUpState = memory;
        memory.FollowUpState = phase2Weak;
        phase2Weak.FollowUpState = roar;
        roar.FollowUpState = roarRelease;
        roarRelease.FollowUpState = unseal;
        _roarStun.FollowUpState = unseal;
        unseal.FollowUpState = unsealRelease;
        unsealRelease.FollowUpState = phase2Strike;
        _unsealBreak.FollowUpState = phase2Strike;

        brand.FollowUpState = Gate("GATE_THUNDER", thunder);
        thunder.FollowUpState = Gate("GATE_UNYIELDING", unyielding);
        unyielding.FollowUpState = Gate("GATE_MIGHT", mightStrike);
        _unyieldingStun.FollowUpState = Gate("GATE_MIGHT_STUN", mightStrike);
        mightStrike.FollowUpState = Gate("GATE_FIRE", fireBurn);
        fireBurn.FollowUpState = Gate("GATE_AFTERIMAGE", afterimage);
        afterimage.FollowUpState = Gate("GATE_FOREST", forestShell);
        forestShell.FollowUpState = Gate("GATE_BLADE", guardingBlade);
        guardingBlade.FollowUpState = Gate("GATE_BRAND", brand);

        return new MonsterMoveStateMachine(states, brand);
    }

    private async Task BrandMove(IReadOnlyList<Creature> targets)
    {
        await SetElement(LightElement.Thunder);
        var pool = LightElementExtensions.BaseElements.ToList();
        Rng.Shuffle(pool);
        var index = 0;
        foreach (var player in LivingPlayers())
        {
            var element = pool[index % pool.Count];
            index++;
            await PowerCmd.Remove<AlchemyStarsSwordAltarBrandPower>(player.Creature);
            await ApplyPower<AlchemyStarsSwordAltarBrandPower>(
                player.Creature,
                AlchemyStarsSwordAltarBrandPower.Encode(element));
        }
    }

    private async Task ThunderStrikeMove(IReadOnlyList<Creature> targets)
    {
        await AttackAll(1, ThunderHits);
        await DebuffAll<FrailPower>(1m);
        if (!_transitionQueued && !_transitionDone)
            _grantUnyieldingOnNextPlayerTurn = true;
    }

    private async Task UnyieldingMove(IReadOnlyList<Creature> targets)
    {
        await SetElement(LightElement.Water);
        await PowerCmd.Remove<AlchemyStarsUnyieldingPower>(Creature);
        await AttackAll(0, 1);
    }

    private async Task UnyieldingStunMove(IReadOnlyList<Creature> targets)
    {
        await SetElement(LightElement.Water);
        await PowerCmd.Remove<AlchemyStarsUnyieldingPower>(Creature);
    }

    private async Task MightStrikeMove(IReadOnlyList<Creature> targets)
    {
        await AttackAll(HeavyDamage, 1);
        await ApplyPower<AlchemyStarsMightPower>(Creature, 1m);
    }

    private async Task FireBurnMove(IReadOnlyList<Creature> targets)
    {
        await SetElement(LightElement.Fire);
        await AttackAll(FireDamage, 1);
        foreach (var player in LivingPlayers())
        {
            await CardPileCmd.AddToCombatAndPreview<Burn>(player.Creature, PileType.Hand, 3, null);
        }
    }

    private async Task AfterimageMove(IReadOnlyList<Creature> targets)
    {
        await ApplyPower<StrengthPower>(Creature, 1m);
        if (!Creature.HasPower<AlchemyStarsAfterimagePower>())
            await ApplyPower<AlchemyStarsAfterimagePower>(Creature, 1m);
    }

    private async Task ForestShellMove(IReadOnlyList<Creature> targets)
    {
        await SetElement(LightElement.Forest);
        if (!Creature.HasPower<SlowPower>())
            await ApplyPower<SlowPower>(Creature, 1m);

        await GainExact<PlatingPower>((IsTough ? 35 : 30) * PlayerCount);
        await DebuffAll<WeakPower>(99m);
        await DebuffAll<VulnerablePower>(99m);
    }

    private async Task GuardingBladeMove(IReadOnlyList<Creature> targets)
    {
        if (_blockSnapshot > 0)
            await AttackAll(_blockSnapshot, 1);
    }

    private async Task FateAngerMove(IReadOnlyList<Creature> targets)
    {
        // 语音以后补：20、「寂静猎兵」本该是一个被我掩埋的称号，它寓意着许多东西……英雄，牺牲，背叛，还有……希望。
        AssertMutable();
        await CreatureCmd.GainBlock(Creature, 444m, ValueProp.Move, null);
        await LockNeutral();
        await ClearOwnDebuffsAndPlating();
        await PowerCmd.Remove<AlchemyStarsFateAngerPower>(Creature);
        _transitionDone = true;
        _transitionQueued = false;
        ClearCharge();

        if (!Creature.HasPower<AlchemyStarsBlindMoonPower>())
            await ApplyPower<AlchemyStarsBlindMoonPower>(Creature, 50m);
        if (!Creature.HasPower<AlchemyStarsAbsoluteResolvePower>())
            await ApplyPower<AlchemyStarsAbsoluteResolvePower>(Creature, 1m);
    }

    private async Task PioneerMove(IReadOnlyList<Creature> targets)
    {
        AssertMutable();
        foreach (var player in LivingPlayers())
        {
            var creature = player.Creature;
            _originalMaxHp[player] = creature.MaxHp;
            var halved = System.Math.Max(1, creature.MaxHp / 2);
            var lose = creature.MaxHp - halved;
            if (lose > 0)
                await CreatureCmd.LoseMaxHp(Ctx(), creature, lose, false);

            await PowerCmd.Remove<AlchemyStarsSwordAltarBrandPower>(creature);
            await ApplyPower<VulnerablePower>(creature, 99m);
        }

        await ApplyPower<AlchemyStarsMightPower>(Creature, 5m);
    }

    private async Task MutedMove(IReadOnlyList<Creature> targets)
    {
        // 语音以后补：34、晦暗星光，爆发吧
        if (!Creature.HasPower<AlchemyStarsMutedSoundPower>())
            await ApplyPower<AlchemyStarsMutedSoundPower>(Creature, 1m);
    }

    private async Task Phase2StrikeMove(IReadOnlyList<Creature> targets)
    {
        await AttackAll(Phase2StrikeDamage, 1);
        await DebuffAll<VulnerablePower>(2m);
    }

    private async Task Phase2FlurryMove(IReadOnlyList<Creature> targets)
    {
        await AttackAll(0, ZeroHitCount);
        await GainExact<PlatingPower>((4 + Strength) * PlayerCount);
    }

    private async Task MemoryMove(IReadOnlyList<Creature> targets)
    {
        // 语音以后补：37、见敌必杀
        AssertMutable();
        await GainBlock(50 * PlayerCount, strengthAlreadyIncluded: false);
        _gainMightOnHit = true;
    }

    private async Task Phase2WeakMove(IReadOnlyList<Creature> targets)
    {
        await AttackAll(0, ZeroHitCount);
        await DebuffAll<WeakPower>(2m);
    }

    private async Task DarkestRoarMove(IReadOnlyList<Creature> targets)
    {
        AssertMutable();
        await GainBlock((4 + Strength) * PlayerCount, strengthAlreadyIncluded: true);
        _charge = ChargeKind.Roar;
        _roarBroken = false;
        if (!Creature.HasPower<AlchemyStarsDarkestRoarPower>())
            await ApplyPower<AlchemyStarsDarkestRoarPower>(Creature, 1m);
    }

    private async Task DarkestRoarReleaseMove(IReadOnlyList<Creature> targets)
    {
        var broken = _roarBroken;
        ClearCharge();
        await PowerCmd.Remove<AlchemyStarsDarkestRoarPower>(Creature);
        if (broken)
            return;

        await AttackAll(Phase2StrikeDamage, 2);
    }

    private async Task DarkestRoarStunMove(IReadOnlyList<Creature> targets)
    {
        ClearCharge();
        await PowerCmd.Remove<AlchemyStarsDarkestRoarPower>(Creature);
    }

    private async Task MoonUnsealMove(IReadOnlyList<Creature> targets)
    {
        // 语音以后补：22、我们是正义的守望者。
        AssertMutable();
        var cap = PlayerCount > 1 ? 30 : 20;
        await PowerCmd.Remove<HardToKillPower>(Creature);
        await ApplyPower<HardToKillPower>(Creature, cap);
        _hardToKillTurnsLeft = 3;

        await PowerCmd.Remove<AlchemyStarsMoonUnsealPower>(Creature);
        await ApplyPower<AlchemyStarsMoonUnsealPower>(Creature, (IsTough ? 6 : 4) * PlayerCount);
        _charge = ChargeKind.Unseal;
    }

    private async Task MoonUnsealReleaseMove(IReadOnlyList<Creature> targets)
    {
        var shield = Creature.GetPower<AlchemyStarsMoonUnsealPower>();
        var broken = shield != null && shield.HitsTaken >= (int)shield.Amount;
        ClearCharge();
        await PowerCmd.Remove<AlchemyStarsMoonUnsealPower>(Creature);
        if (broken)
        {
            await RestorePlayersFromUnseal(firstBreak: !_maxHpRestored);
            return;
        }

        var moon = Creature.GetPower<AlchemyStarsBlindMoonPower>();
        if (moon != null)
        {
            await PowerCmd.ModifyAmount(Ctx(), moon, 50m, Creature, null);
            moon.MarkUnsealed();
        }

        await AttackAll(0, UnsealCounterHits);
    }

    private async Task MoonUnsealBreakMove(IReadOnlyList<Creature> targets)
    {
        ClearCharge();
        await PowerCmd.Remove<AlchemyStarsMoonUnsealPower>(Creature);
        await RestorePlayersFromUnseal(firstBreak: !_maxHpRestored);
    }

    private async Task OnPlayerAttackHit(PlayerChoiceContext choiceContext, DamageResult result)
    {
        AssertMutable();
        if (_charge == ChargeKind.Unseal)
        {
            var shield = Creature.GetPower<AlchemyStarsMoonUnsealPower>();
            if (shield != null)
            {
                shield.RegisterHit();
                if (shield.HitsTaken >= (int)shield.Amount &&
                    _unsealBreak != null &&
                    Creature.CombatState?.CurrentSide == CombatSide.Player)
                {
                    SetMoveImmediate(_unsealBreak, true);
                }
            }
        }

        if (_gainMightOnHit)
            await ApplyPower<AlchemyStarsMightPower>(Creature, 1m);
    }

    private void QueueTransition()
    {
        if (_transitionQueued || _transitionDone || _fateAngerMove == null)
            return;

        AssertMutable();
        _transitionQueued = true;
        if (Creature.CombatState?.CurrentSide == CombatSide.Player)
            SetMoveImmediate(_fateAngerMove, true);
    }

    private async Task RestorePlayersFromUnseal(bool firstBreak)
    {
        AssertMutable();
        if (firstBreak)
            _maxHpRestored = true;

        foreach (var player in LivingPlayers())
        {
            var creature = player.Creature;
            if (firstBreak && _originalMaxHp.TryGetValue(player, out var original))
                await CreatureCmd.SetMaxHp(creature, original);

            var heal = creature.MaxHp * 0.2m;
            if (heal > 0)
                await CreatureCmd.Heal(creature, heal);
        }
    }

    private async Task DebuffAll<T>(decimal amount) where T : PowerModel
    {
        foreach (var player in LivingPlayers())
            await ApplyPower<T>(player.Creature, amount);
    }

    private async Task AttackAll(int damage, int hits)
    {
        if (hits < 1)
            return;

        await DamageCmd.Attack(damage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    private async Task GainBlock(int amount, bool strengthAlreadyIncluded)
    {
        var total = amount;
        if (!strengthAlreadyIncluded && Creature.HasPower<AlchemyStarsAbsoluteResolvePower>())
            total += Strength;

        if (total > 0)
            await CreatureCmd.GainBlock(Creature, total, ValueProp.Move, null);
    }

    private async Task GainExact<T>(int amount) where T : PowerModel
    {
        if (amount == 0)
            return;

        var before = Creature.GetPowerAmount<T>();
        await ApplyPower<T>(Creature, 1m);
        var gained = Creature.GetPowerAmount<T>() - before;
        var delta = amount - gained;
        var power = Creature.GetPower<T>();
        if (power != null && delta != 0)
            await PowerCmd.ModifyAmount(Ctx(), power, delta, Creature, null);
    }

    /// <summary>不屈：记下本回合受到的未格挡伤害，回合结束时治疗并收回临时力量。</summary>
    public void NoteUnyieldingDamage(int damage)
    {
        if (damage <= 0)
            return;

        AssertMutable();
        _unyieldingPendingHeal += damage;
        _unyieldingTempStrength += damage;
    }

    /// <summary>不屈层数被打空，下一动改为眩晕。</summary>
    public void StunUnyielding()
    {
        if (_unyieldingStun == null || Creature.CombatState?.CurrentSide != CombatSide.Player)
            return;

        AssertMutable();
        SetMoveImmediate(_unyieldingStun, true);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || (_unyieldingPendingHeal <= 0 && _unyieldingTempStrength <= 0))
            return;

        AssertMutable();
        var heal = _unyieldingPendingHeal;
        var strength = _unyieldingTempStrength;
        _unyieldingPendingHeal = 0;
        _unyieldingTempStrength = 0;

        if (heal > 0)
            await CreatureCmd.Heal(Creature, heal);

        if (strength <= 0)
            return;

        var strengthPower = Creature.GetPower<StrengthPower>();
        if (strengthPower == null)
            return;

        // 负向力量会被人工制品当成减益抵消，所以直接改层数。
        var next = strengthPower.Amount - strength;
        if (next == 0)
            await PowerCmd.Remove(strengthPower);
        else
            strengthPower.SetAmount(next);
    }

    private async Task ClearOwnDebuffsAndPlating()
    {
        var debuffs = Creature.Powers.Where(power => power.Type == PowerType.Debuff).ToList();
        foreach (var power in debuffs)
            await PowerCmd.Remove(power);

        await PowerCmd.Remove<PlatingPower>(Creature);
    }

    private async Task ApplyPower<T>(Creature target, decimal amount) where T : PowerModel
    {
        if (amount == 0)
            return;

        await PowerCmd.Apply<T>(Ctx(), target, amount, Creature, null);
    }

    private async Task GrantUnyielding()
    {
        AssertMutable();
        _unyieldingPendingHeal = 0;
        _unyieldingTempStrength = 0;
        await PowerCmd.Remove<AlchemyStarsUnyieldingPower>(Creature);
        await ApplyPower<AlchemyStarsUnyieldingPower>(Creature, (IsTough ? 4 : 3) * PlayerCount);
    }

    private async Task SetElement(LightElement? element)
    {
        AssertMutable();
        var next = _permanentNeutral ? null : element;
        if (CurrentElement == next)
            return;

        CurrentElement = next;
        await SyncAttributeIcon();
    }

    private async Task LockNeutral()
    {
        AssertMutable();
        _permanentNeutral = true;
        if (CurrentElement == null)
            return;

        CurrentElement = null;
        await SyncAttributeIcon();
    }

    private async Task SyncAttributeIcon()
    {
        await PowerCmd.Remove<AlchemyStarsEnemyAttributePower>(Creature);
        if (CurrentElement is not LightElement element)
            return;

        await ApplyPower<AlchemyStarsEnemyAttributePower>(
            Creature,
            AlchemyStarsEnemyAttributePower.Encode(element));
    }

    private void ClearCharge()
    {
        AssertMutable();
        _charge = ChargeKind.None;
        _roarBroken = false;
    }

    private IEnumerable<Player> LivingPlayers()
    {
        var players = Creature.CombatState?.Players;
        if (players == null)
            yield break;

        foreach (var player in players)
        {
            if (player.Creature is { IsDead: false })
                yield return player;
        }
    }

    private static ThrowingPlayerChoiceContext Ctx() => new();
}
