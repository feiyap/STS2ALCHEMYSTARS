using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Relics;
using Valencina.ValencinaCode.Relics.Rien;
using Valencina.ValencinaCode.UI;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Powers;

public class InstantForesightPower : ValencinaPower
{
	private sealed class AttackTracker
	{
		private readonly Queue<PrecognitionDamageDecision> _pendingDecisions = new Queue<PrecognitionDamageDecision>();

		public AttackCommand Command { get; }

		public Creature Attacker { get; }

		public decimal DodgeRemaining { get; set; }

		public bool CounterEligible { get; }

		public bool HadAttackDamage { get; set; }

		public bool WasFullyPrevented { get; set; }

		public decimal PreventedDamage { get; set; }

		public decimal FinalHpLoss { get; set; }

		public AttackTracker(AttackCommand command, Creature attacker, int dodgeValue, bool counterEligible)
		{
			Command = command;
			Attacker = attacker;
			DodgeRemaining = Math.Max(0, dodgeValue);
			CounterEligible = counterEligible;
			WasFullyPrevented = true;
			base._002Ector();
		}

		public void EnqueueDecision(PrecognitionDamageDecision decision)
		{
			_pendingDecisions.Enqueue(decision);
		}

		public bool TryDequeueDecision(out PrecognitionDamageDecision decision)
		{
			if (_pendingDecisions.Count <= 0)
			{
				decision = default(PrecognitionDamageDecision);
				return false;
			}
			decision = _pendingDecisions.Dequeue();
			return true;
		}
	}

	private readonly record struct PrecognitionDamageDecision(decimal PreventedDamage, decimal FinalHpLoss, bool FullyDodged);

	internal readonly record struct PreparedDodgeCounter(Creature Attacker, decimal PreventedDamage);

	private readonly record struct DodgeCounterSnapshot(Creature Attacker, decimal PreventedDamage);

	private sealed class PrecognitionDamageMarker
	{
		public Creature? Dealer { get; set; }
	}

	private sealed class PendingDodgeVisualMarker
	{
		public Creature? Dealer { get; set; }

		public int Count { get; set; }
	}

	private const int DefaultMaxPrecognition = 30;

	private const int CompleteForesightEyeMaxPrecognition = 40;

	private const int RestorePerPlayerTurn = 10;

	private const int TemporaryPrecognitionPerDodge = 6;

	private const int ShinTriggerSpent = 30;

	private const string NormalPackedIconPath = "res://Valencina/images/powers/odin_eye_power.png";

	private const string OverheatPackedIconPath = "res://Valencina/images/powers/instant_foresight_power_overheat.png";

	private const string OverheatBigIconPath = "res://Valencina/images/powers/big/instant_foresight_power_overheat.png";

	private static readonly Vector2 AmountChangeVfxOffsetFromBodyTopRight = new Vector2(34f, -28f);

	private static readonly ConditionalWeakTable<DamageResult, PrecognitionDamageMarker> PreventedDamageResults = new ConditionalWeakTable<DamageResult, PrecognitionDamageMarker>();

	private static readonly ConditionalWeakTable<Creature, PendingDodgeVisualMarker> PendingDodgeVisuals = new ConditionalWeakTable<Creature, PendingDodgeVisualMarker>();

	private readonly IPrecognitionCounterCardProvider _counterProvider = new DefaultPrecognitionCounterProvider();

	private readonly HashSet<Creature> _successfulCounterTargetsThisTurn = new HashSet<Creature>();

	private readonly Queue<Creature> _pendingActiveCounterTargets = new Queue<Creature>();

	private readonly Queue<DodgeCounterSnapshot> _pendingDodgeCounters = new Queue<DodgeCounterSnapshot>();

	private AttackTracker? _activeAttack;

	private Task? _lastDodgeAnimationTask;

	private int _precognitionAmount = 30;

	private int _temporaryPrecognitionAmount;

	private int _basePrecognitionDodgeThreshold;

	private int _temporaryDodgeThreshold;

	private decimal _preventedDamageThisTurn;

	private int _successfulCounterCountThisCombat;

	private int _precognitionSpentThisCombat;

	private int _precognitionSpentThisTurn;

	private int _precognitionSpentLastTurn;

	private bool _isOverheated;

	private bool _isTriggeringCounter;

	private bool _shinTriggered;

	private bool _trackAllEnemyAttacksDodged;

	private bool _trackedEnemyAttackSeen;

	private bool _trackedAllEnemyAttacksDodged;

	private bool _counterBlockedUntilPlayerTurnStart;

	private bool _counterBlockedUntilCombatEnd;

	private bool _isDrainingCounterQueue;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override int DisplayAmount => _precognitionAmount;

	public override LocString Description
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (!IsWarMode)
			{
				return ((PowerModel)this).Description;
			}
			return new LocString("powers", "VALENCINA.war_precognition.description");
		}
	}

	protected override string SmartDescriptionLocKey
	{
		get
		{
			if (!IsWarMode)
			{
				return ((PowerModel)this).SmartDescriptionLocKey;
			}
			return "VALENCINA.war_precognition.description";
		}
	}

	public override string CustomIconPath
	{
		get
		{
			object obj;
			if (!_isOverheated || !ResourceLoader.Exists("res://Valencina/images/powers/instant_foresight_power_overheat.png", ""))
			{
				obj = base.CustomIconPath;
				if (obj == null)
				{
					return string.Empty;
				}
			}
			else
			{
				obj = "res://Valencina/images/powers/instant_foresight_power_overheat.png";
			}
			return (string)obj;
		}
	}

	public override string CustomBigIconPath
	{
		get
		{
			object obj;
			if (!_isOverheated || !ResourceLoader.Exists("res://Valencina/images/powers/big/instant_foresight_power_overheat.png", ""))
			{
				obj = base.CustomBigIconPath;
				if (obj == null)
				{
					return string.Empty;
				}
			}
			else
			{
				obj = "res://Valencina/images/powers/big/instant_foresight_power_overheat.png";
			}
			return (string)obj;
		}
	}

	public int TemporaryPrecognitionAmount => _temporaryPrecognitionAmount;

	public int TemporaryDodgeThreshold => _temporaryDodgeThreshold;

	public int EffectivePrecognition
	{
		get
		{
			if (!_isOverheated)
			{
				return _precognitionAmount;
			}
			return 0;
		}
	}

	public int DodgeConversionRatio
	{
		get
		{
			Creature owner = ((PowerModel)this).Owner;
			OdinEyeRatioPower odinEyeRatioPower = ((owner != null) ? owner.GetPower<OdinEyeRatioPower>() : null);
			if (odinEyeRatioPower == null || ((PowerModel)odinEyeRatioPower).Amount <= 0)
			{
				return 6;
			}
			return 3;
		}
	}

	public int DodgeValue
	{
		get
		{
			if (_isOverheated)
			{
				return 0;
			}
			return Math.Max(0, _basePrecognitionDodgeThreshold + _temporaryDodgeThreshold);
		}
	}

	public bool IsOverheated => _isOverheated;

	public int PrecognitionSpentLastTurn => _precognitionSpentLastTurn;

	public int PrecognitionSpentThisTurn => _precognitionSpentThisTurn;

	private bool IsWarMode
	{
		get
		{
			object runState;
			if (!((AbstractModel)this).IsMutable)
			{
				IRunState val = (IRunState)(object)RunManager.Instance.DebugOnlyGetState();
				runState = val;
			}
			else
			{
				Creature owner = ((PowerModel)this).Owner;
				if (owner == null)
				{
					runState = null;
				}
				else
				{
					Player player = owner.Player;
					runState = ((player != null) ? player.RunState : null);
				}
			}
			return ValencinaWarDifficulty.IsActive((IRunState?)runState);
		}
	}

	public bool IsPrecognitionLockedByFarewell
	{
		get
		{
			Creature owner = ((PowerModel)this).Owner;
			FarewellPower farewellPower = ((owner != null) ? owner.GetPower<FarewellPower>() : null);
			if (farewellPower != null)
			{
				return ((PowerModel)farewellPower).Amount > 0;
			}
			return false;
		}
	}

	public int MaxPrecognitionForOwner => MaxPrecognitionFor(((PowerModel)this).Owner);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			if (!IsWarMode)
			{
				yield return (IHoverTip)(object)new HoverTip(new LocString("static_hover_tips", "VALENCINA-DODGE_STATIC.title"), new LocString("static_hover_tips", "VALENCINA-DODGE_STATIC.description"), ((PowerModel)this).Icon);
				yield return (IHoverTip)(object)new HoverTip(new LocString("static_hover_tips", "VALENCINA-PRECOGNITION_OVERHEAT.title"), new LocString("static_hover_tips", "VALENCINA-PRECOGNITION_OVERHEAT.description"), ((PowerModel)this).Icon);
			}
		}
	}

	public IReadOnlyCollection<Creature> SuccessfulCounterTargetsThisTurn => _successfulCounterTargetsThisTurn;

	public int SuccessfulCounterCountThisCombat => _successfulCounterCountThisCombat;

	private bool IsCounterBlocked
	{
		get
		{
			if (!_counterBlockedUntilPlayerTurnStart)
			{
				return _counterBlockedUntilCombatEnd;
			}
			return true;
		}
	}

	public static int MaxPrecognitionFor(Creature? owner)
	{
		object obj;
		if (owner == null)
		{
			obj = null;
		}
		else
		{
			Player player = owner.Player;
			obj = ((player != null) ? player.GetRelic<CompleteForesightEye>() : null);
		}
		int num = ((obj != null) ? 40 : 30);
		MemoryExpansionPower memoryExpansionPower = ((owner != null) ? owner.GetPower<MemoryExpansionPower>() : null);
		if (memoryExpansionPower != null && ((PowerModel)memoryExpansionPower).Amount > 0)
		{
			num += ((PowerModel)memoryExpansionPower).Amount;
		}
		return num;
	}

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (target != ((PowerModel)this).Owner || !(canonicalPower is InstantForesightPower) || amount <= 0m)
		{
			return false;
		}
		if (IsPrecognitionLockedByFarewell)
		{
			modifiedAmount = default(decimal);
			return true;
		}
		modifiedAmount = Math.Max(0m, Math.Min(amount, MaxPrecognitionForOwner - _precognitionAmount));
		return modifiedAmount != amount;
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (IsPrecognitionLockedByFarewell)
		{
			SetPrecognitionAmount(1);
			return Task.CompletedTask;
		}
		int maxPrecognitionForOwner = MaxPrecognitionForOwner;
		SetPrecognitionAmount(Math.Clamp(((PowerModel)this).Amount, 0, maxPrecognitionForOwner));
		RefreshBasePrecognitionDodge();
		return Task.CompletedTask;
	}

	public override Task BeforeAttack(AttackCommand command)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || command.Attacker == null)
		{
			return Task.CompletedTask;
		}
		if (((PowerModel)this).CombatState.CurrentSide == ((PowerModel)this).Owner.Side || command.Attacker.Side == ((PowerModel)this).Owner.Side || command.TargetSide != ((PowerModel)this).Owner.Side)
		{
			return Task.CompletedTask;
		}
		_activeAttack = new AttackTracker(command, command.Attacker, DodgeValue, !_isOverheated && !IsCounterBlocked);
		if (_trackAllEnemyAttacksDodged)
		{
			_trackedEnemyAttackSeen = true;
		}
		return Task.CompletedTask;
	}

	public async Task ValencinaAfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		PreparedDodgeCounter? prepared = await PrepareDodgeCounterAfterAttackAsync(command);
		if (prepared.HasValue)
		{
			await Cmd.CustomScaledWait(0.08f, 0.08f, false, default(CancellationToken));
			await TriggerPreparedDodgeCounterAsync(prepared.Value);
		}
	}

	internal Task<PreparedDodgeCounter?> PrepareDodgeCounterAfterAttackAsync(AttackCommand command)
	{
		if (_activeAttack != null && _activeAttack.Command == command)
		{
			Creature owner = ((PowerModel)this).Owner;
			if (((owner != null) ? owner.Player : null) != null)
			{
				AttackTracker activeAttack = _activeAttack;
				_activeAttack = null;
				bool flag = activeAttack.FinalHpLoss > 0m;
				bool flag2 = flag && activeAttack.HadAttackDamage && ((PowerModel)this).Owner.Player.GetRelic<SomeonesComic>() != null;
				if (!activeAttack.HadAttackDamage || !((activeAttack.PreventedDamage > 0m && !flag) || flag2) || !activeAttack.CounterEligible || IsCounterBlocked || activeAttack.Attacker.IsDead || !activeAttack.Attacker.IsAlive)
				{
					if (_trackAllEnemyAttacksDodged && activeAttack.HadAttackDamage && flag)
					{
						_trackedAllEnemyAttacksDodged = false;
					}
					if (activeAttack.HadAttackDamage && flag && activeAttack.PreventedDamage > 0m && ((PowerModel)this).Owner != null)
					{
						Creature owner2 = ((PowerModel)this).Owner;
						NCombatRoom instance = NCombatRoom.Instance;
						ValencinaVoiceSfx.TryPlayDodgeFail(owner2, (Node?)(object)((instance != null) ? instance.GetCreatureNode(((PowerModel)this).Owner) : null));
					}
					return Task.FromResult<PreparedDodgeCounter?>(null);
				}
				if (_trackAllEnemyAttacksDodged && flag)
				{
					_trackedAllEnemyAttacksDodged = false;
				}
				((PowerModel)this).Flash();
				Creature owner3 = ((PowerModel)this).Owner;
				NCombatRoom instance2 = NCombatRoom.Instance;
				ValencinaVoiceSfx.TryPlayDodgeSuccess(owner3, (Node?)(object)((instance2 != null) ? instance2.GetCreatureNode(((PowerModel)this).Owner) : null));
				return Task.FromResult((PreparedDodgeCounter?)new PreparedDodgeCounter(activeAttack.Attacker, activeAttack.PreventedDamage));
			}
		}
		return Task.FromResult<PreparedDodgeCounter?>(null);
	}

	internal Task TriggerPreparedDodgeCounterAsync(PreparedDodgeCounter prepared)
	{
		return TriggerSynchronizedCounterAsync(new DodgeCounterSnapshot(prepared.Attacker, prepared.PreventedDamage));
	}

	private async Task TriggerSynchronizedCounterAsync(DodgeCounterSnapshot snapshot, bool drainAfter = true)
	{
		if (!CanResolveCounter(snapshot.Attacker, "dodge-counter-start"))
		{
			return;
		}
		Creature owner = ((PowerModel)this).Owner;
		Player val = ((owner != null) ? owner.Player : null);
		if (owner == null || val == null)
		{
			return;
		}
		if (_isTriggeringCounter)
		{
			_pendingDodgeCounters.Enqueue(snapshot);
			MainFile.Logger.Info($"[Precognition] queued delayed dodge counter owner={owner.Name}/net={val.NetId} attacker={snapshot.Attacker.Name}", 1);
			return;
		}
		PrecognitionCounterContext context = CreateCounterContext(val, snapshot.Attacker, snapshot.PreventedDamage, isActiveTrigger: false);
		if (!_counterProvider.CanProvide(context))
		{
			return;
		}
		CardModel counterCard = _counterProvider.CreateCounterCard(context);
		if (!(counterCard is IPrecognitionVirtualCounterCard precognitionVirtualCounterCard))
		{
			return;
		}
		_isTriggeringCounter = true;
		try
		{
			ValencinaProbeLog.Info("precog-dodge-counter-trigger", $"Dodge counter trigger owner={owner.Name}/net={val.NetId} attacker={snapshot.Attacker.Name} pendingDodge={_pendingDodgeCounters.Count} pendingActive={_pendingActiveCounterTargets.Count}", 40);
			MainFile.Logger.Info($"[Precognition] dodge counter triggered owner={owner.Name}/net={val.NetId} attacker={snapshot.Attacker.Name}", 1);
			if (await precognitionVirtualCounterCard.TriggerFromPrecognition(context))
			{
				await AfterSuccessfulCounterAsync(snapshot.Attacker);
			}
		}
		finally
		{
			_isTriggeringCounter = false;
			RemoveTemporaryCounterCard(counterCard, context);
		}
		if (drainAfter)
		{
			await DrainPendingCounterQueuesAsync((PlayerChoiceContext)new BlockingPlayerChoiceContext());
		}
	}

	public async Task TriggerCounterAgainstAsync(PlayerChoiceContext choiceContext, Creature target)
	{
		if (CanResolveCounter(target, "active-counter-start"))
		{
			if (_isTriggeringCounter)
			{
				_pendingActiveCounterTargets.Enqueue(target);
				return;
			}
			await TriggerCounterAgainstCoreAsync(choiceContext, target);
			await DrainPendingCounterQueuesAsync(choiceContext);
		}
	}

	public async Task TriggerCounterAgainstImmediatelyAsync(PlayerChoiceContext choiceContext, Creature target, int times, bool fastAnimation = false)
	{
		if (times <= 0 || !CanResolveCounter(target, "active-counter-immediate-start"))
		{
			return;
		}
		for (int i = 0; i < times; i++)
		{
			if (!CanResolveCounter(target, "active-counter-immediate-loop"))
			{
				break;
			}
			if (_isTriggeringCounter)
			{
				_pendingActiveCounterTargets.Enqueue(target);
				continue;
			}
			await TriggerCounterAgainstCoreAsync(choiceContext, target, fastAnimation);
			if (i + 1 < times)
			{
				await WaitBetweenImmediateCountersAsync(fastAnimation);
			}
		}
		await DrainPendingCounterQueuesAsync(choiceContext);
	}

	private static Task WaitBetweenImmediateCountersAsync(bool fastAnimation = false)
	{
		float num = (fastAnimation ? 0.025f : 0.08f);
		return Cmd.CustomScaledWait(num, num, false, default(CancellationToken));
	}

	private async Task DrainPendingCounterQueuesAsync(PlayerChoiceContext choiceContext)
	{
		if (_isDrainingCounterQueue)
		{
			return;
		}
		_isDrainingCounterQueue = true;
		try
		{
			while (((PowerModel)this).Owner != null && ((PowerModel)this).Owner.Player != null && !IsCounterBlocked)
			{
				if (_pendingDodgeCounters.Count > 0)
				{
					DodgeCounterSnapshot snapshot = _pendingDodgeCounters.Dequeue();
					if (CanResolveCounter(snapshot.Attacker, "drain-dodge-counter"))
					{
						await TriggerSynchronizedCounterAsync(snapshot, drainAfter: false);
						if (_pendingDodgeCounters.Count > 0 || _pendingActiveCounterTargets.Count > 0)
						{
							await WaitBetweenImmediateCountersAsync();
						}
					}
					continue;
				}
				if (_pendingActiveCounterTargets.Count <= 0)
				{
					break;
				}
				Creature target = _pendingActiveCounterTargets.Dequeue();
				if (CanResolveCounter(target, "drain-active-counter"))
				{
					await TriggerCounterAgainstCoreAsync(choiceContext, target);
					if (_pendingDodgeCounters.Count > 0 || _pendingActiveCounterTargets.Count > 0)
					{
						await WaitBetweenImmediateCountersAsync();
					}
				}
			}
		}
		finally
		{
			_isDrainingCounterQueue = false;
		}
	}

	private async Task TriggerCounterAgainstCoreAsync(PlayerChoiceContext choiceContext, Creature target, bool fastAnimation = false)
	{
		if (!CanResolveCounter(target, "active-counter-start"))
		{
			return;
		}
		Creature owner = ((PowerModel)this).Owner;
		Player val = ((owner != null) ? owner.Player : null);
		if (owner == null || val == null)
		{
			return;
		}
		PrecognitionCounterContext context = CreateCounterContext(val, target, 0m, IsActiveCounterTrigger(), fastAnimation);
		if (!_counterProvider.CanProvide(context))
		{
			return;
		}
		CardModel counterCard = _counterProvider.CreateCounterCard(context);
		if (!(counterCard is IPrecognitionVirtualCounterCard precognitionVirtualCounterCard))
		{
			return;
		}
		_isTriggeringCounter = true;
		try
		{
			ValencinaProbeLog.Info("precog-active-counter-trigger", $"Active counter trigger owner={owner.Name}/net={val.NetId} target={target.Name} pendingDodge={_pendingDodgeCounters.Count} pendingActive={_pendingActiveCounterTargets.Count}", 40);
			MainFile.Logger.Info($"[Precognition] active counter triggered owner={owner.Name}/net={val.NetId} target={target.Name}", 1);
			if (await precognitionVirtualCounterCard.TriggerFromPrecognition(context))
			{
				await AfterSuccessfulCounterAsync(target);
			}
		}
		finally
		{
			_isTriggeringCounter = false;
			RemoveTemporaryCounterCard(counterCard, context);
		}
	}

	private async Task AfterSuccessfulCounterAsync(Creature target)
	{
		if (((PowerModel)this).Owner != null)
		{
			_successfulCounterTargetsThisTurn.Add(target);
			_successfulCounterCountThisCombat++;
			HuntsEndPower power = ((PowerModel)this).Owner.GetPower<HuntsEndPower>();
			if (power != null && ((PowerModel)power).Amount > 0)
			{
				await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, (decimal)((PowerModel)power).Amount, ((PowerModel)this).Owner, (CardModel?)null, silent: false);
			}
			CounterDrawPower power2 = ((PowerModel)this).Owner.GetPower<CounterDrawPower>();
			if (power2 != null && ((PowerModel)power2).Amount > 0 && ((PowerModel)this).Owner.Player != null)
			{
				await CardPileCmd.Draw((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (decimal)Math.Max(0, ((PowerModel)power2).Amount), ((PowerModel)this).Owner.Player, false);
			}
			SecondAccelerationPower power3 = ((PowerModel)this).Owner.GetPower<SecondAccelerationPower>();
			if (power3 != null && ((PowerModel)power3).Amount > 0)
			{
				await CompatPowerCmd.Apply<ValencinaFreeNextAttackPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, 1m, ((PowerModel)this).Owner, (CardModel?)null, silent: false);
			}
		}
	}

	private static void RemoveTemporaryCounterCard(CardModel counterCard, PrecognitionCounterContext context)
	{
		try
		{
			Creature creature = context.Owner.Creature;
			ICombatState val = ((creature != null) ? creature.CombatState : null);
			if (val != null)
			{
				val.RemoveCard(counterCard);
			}
		}
		catch
		{
		}
		try
		{
			((ICardScope)context.Owner.RunState).RemoveCard(counterCard);
		}
		catch
		{
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return 1m;
	}

	public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || target != ((PowerModel)this).Owner || amount <= 0m || _isOverheated)
		{
			return amount;
		}
		if (_activeAttack != null && dealer == _activeAttack.Attacker && ((PowerModel)this).CombatState.CurrentSide != ((PowerModel)this).Owner.Side)
		{
			if (!CanDodgeDamage(props, cardSource))
			{
				return amount;
			}
			decimal num = Math.Min(amount, Math.Max(0m, _activeAttack.DodgeRemaining));
			decimal num2 = amount - num;
			_activeAttack.DodgeRemaining -= num;
			if (num2 <= 0m)
			{
				_activeAttack.EnqueueDecision(new PrecognitionDamageDecision(amount, 0m, FullyDodged: true));
				MainFile.Logger.Info($"[Precognition] dodge decision hpLoss={amount} dodge={DodgeValue} owner={target.Name}", 1);
				return 0m;
			}
			decimal num3 = ApplyUndodgeableAttackReduction(num2);
			_activeAttack.EnqueueDecision(new PrecognitionDamageDecision(num, num3, FullyDodged: false));
			MainFile.Logger.Info($"[Precognition] threshold spent hpLoss={amount}->{num3} prevented={num} dodge={DodgeValue} owner={target.Name}", 1);
			return num3;
		}
		if (_isTriggeringCounter)
		{
			return amount;
		}
		if (CanDodgeDamage(props, cardSource) && !IsActiveAttackReflectionDamage(props, dealer, cardSource))
		{
			decimal num4 = Math.Min(amount, DodgeValue);
			decimal num5 = amount - num4;
			if (num4 <= 0m)
			{
				return amount;
			}
			_preventedDamageThisTurn += num4;
			((PowerModel)this).Flash();
			if (num5 <= 0m)
			{
				MainFile.Logger.Info($"[Precognition] dodged blockable damage={amount} owner={target.Name} props={props} source={((object)cardSource)?.GetType().Name ?? "null"}", 1);
				_lastDodgeAnimationTask = ValencinaAnimation.PlayPrecognitionDodge(((PowerModel)this).Owner, dealer);
				Creature owner = ((PowerModel)this).Owner;
				NCombatRoom instance = NCombatRoom.Instance;
				ValencinaVoiceSfx.TryPlayDodgeSuccess(owner, (Node?)(object)((instance != null) ? instance.GetCreatureNode(((PowerModel)this).Owner) : null));
				return 0m;
			}
			MainFile.Logger.Info($"[Precognition] reduced blockable damage={amount}->{num5} prevented={num4} owner={target.Name} props={props} source={((object)cardSource)?.GetType().Name ?? "null"}", 1);
			return num5;
		}
		return amount;
	}

	private bool IsActiveAttackReflectionDamage(ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && dealer != null && cardSource == null && ((Enum)props).HasFlag((Enum)(object)(ValueProp)16) && ((PowerModel)this).CombatState.CurrentSide == ((PowerModel)this).Owner.Side)
		{
			return dealer.Side != ((PowerModel)this).Owner.Side;
		}
		return false;
	}

	private static bool CanDodgeDamage(ValueProp props, CardModel? cardSource)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (cardSource is Infection || (cardSource != null && ((object)cardSource).GetType().Name.Equals("Infection", StringComparison.OrdinalIgnoreCase)))
		{
			return true;
		}
		return !((Enum)props).HasFlag((Enum)(object)(ValueProp)2);
	}

	private decimal ApplyUndodgeableAttackReduction(decimal amount)
	{
		if (!(amount <= 0m))
		{
			Creature owner = ((PowerModel)this).Owner;
			SettlementCompensationPower settlementCompensationPower = ((owner != null) ? owner.GetPower<SettlementCompensationPower>() : null);
			if (settlementCompensationPower != null && ((PowerModel)settlementCompensationPower).Amount > 0)
			{
				settlementCompensationPower.FlashFromForesight();
				decimal num = Math.Max(0m, 1m - (decimal)((PowerModel)settlementCompensationPower).Amount / 100m);
				return Math.Floor(amount * num);
			}
		}
		return amount;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) != player)
		{
			return;
		}
		ValencinaVoiceSfx.ResetTurn(((PowerModel)this).Owner);
		_pendingActiveCounterTargets.Clear();
		_pendingDodgeCounters.Clear();
		_successfulCounterTargetsThisTurn.Clear();
		_counterBlockedUntilPlayerTurnStart = false;
		if (IsPrecognitionLockedByFarewell)
		{
			_precognitionSpentLastTurn = _precognitionSpentThisTurn;
			_precognitionSpentThisTurn = 0;
			_preventedDamageThisTurn = default(decimal);
			_temporaryPrecognitionAmount = 0;
			_temporaryDodgeThreshold = 0;
			TemporaryPrecognitionPower power = ((PowerModel)this).Owner.GetPower<TemporaryPrecognitionPower>();
			if (power != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power);
			}
			SetPrecognitionAmount(1);
			RefreshBasePrecognitionDodge();
			RefreshDodgeUi();
			return;
		}
		int retainedDodgeThreshold = ((((PowerModel)this).Owner.GetPower<RedThreadPower>() != null) ? Math.Max(0, _temporaryDodgeThreshold / 2) : 0);
		decimal num = (ValencinaWarDifficulty.IsActive(player.RunState) ? 1m : 2m);
		int num2 = ((_preventedDamageThisTurn > 0m) ? ((int)Math.Ceiling(_preventedDamageThisTurn / num)) : 0);
		int spentAtTurnStart = 0;
		if (num2 > 0)
		{
			int precognitionAmount = _precognitionAmount;
			int num3 = Math.Min(precognitionAmount, num2);
			spentAtTurnStart = num3;
			SetPrecognitionAmount(Math.Max(0, _precognitionAmount - num2));
			if (num3 >= precognitionAmount && precognitionAmount > 0)
			{
				_isOverheated = true;
			}
			await TrackPrecognitionSpentAsync(choiceContext, num3, countForThisTurn: false);
			_preventedDamageThisTurn = default(decimal);
			((PowerModel)this).Flash();
		}
		else
		{
			_preventedDamageThisTurn = default(decimal);
		}
		_precognitionSpentLastTurn = _precognitionSpentThisTurn + spentAtTurnStart;
		_precognitionSpentThisTurn = 0;
		_temporaryPrecognitionAmount = 0;
		TemporaryPrecognitionPower power2 = ((PowerModel)this).Owner.GetPower<TemporaryPrecognitionPower>();
		if (power2 != null)
		{
			await PowerCmd.Remove((PowerModel)(object)power2);
		}
		_temporaryDodgeThreshold = retainedDodgeThreshold;
		RestorePrecognition(10);
		RefreshBasePrecognitionDodge();
		RefreshDodgeUi();
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ClearEnemyTurnRuntimeState(side);
	}

	private Task ClearEnemyTurnRuntimeState(CombatSide side)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || side == ((PowerModel)this).Owner.Side)
		{
			return Task.CompletedTask;
		}
		ValencinaVoiceSfx.ResetTurn(((PowerModel)this).Owner);
		ClearCounterRuntimeState("after-enemy-turn-end");
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		ClearCounterRuntimeState("after-combat-end");
		_activeAttack = null;
		_counterBlockedUntilPlayerTurnStart = true;
		_counterBlockedUntilCombatEnd = false;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 3);
		defaultInterpolatedStringHandler.AppendLiteral("Combat end cleanup owner=");
		Creature owner = ((PowerModel)this).Owner;
		defaultInterpolatedStringHandler.AppendFormatted(((owner != null) ? owner.Name : null) ?? "null");
		defaultInterpolatedStringHandler.AppendLiteral("/net=");
		Creature owner2 = ((PowerModel)this).Owner;
		object obj;
		if (owner2 == null)
		{
			obj = null;
		}
		else
		{
			Player player = owner2.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
		}
		if (obj == null)
		{
			obj = "null";
		}
		defaultInterpolatedStringHandler.AppendFormatted((string?)obj);
		defaultInterpolatedStringHandler.AppendLiteral(" room=");
		defaultInterpolatedStringHandler.AppendFormatted(((object)room)?.GetType().Name ?? "null");
		ValencinaProbeLog.Info("precog-combat-end-cleanup", defaultInterpolatedStringHandler.ToStringAndClear(), 20);
		return Task.CompletedTask;
	}

	private void ClearCounterRuntimeState(string reason)
	{
		int count = _pendingActiveCounterTargets.Count;
		int count2 = _pendingDodgeCounters.Count;
		_pendingActiveCounterTargets.Clear();
		_pendingDodgeCounters.Clear();
		_isTriggeringCounter = false;
		_isDrainingCounterQueue = false;
		if (count > 0 || count2 > 0)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 5);
			defaultInterpolatedStringHandler.AppendLiteral("Counter queues cleared reason=");
			defaultInterpolatedStringHandler.AppendFormatted(reason);
			defaultInterpolatedStringHandler.AppendLiteral(" owner=");
			Creature owner = ((PowerModel)this).Owner;
			defaultInterpolatedStringHandler.AppendFormatted(((owner != null) ? owner.Name : null) ?? "null");
			defaultInterpolatedStringHandler.AppendLiteral("/net=");
			Creature owner2 = ((PowerModel)this).Owner;
			object obj;
			if (owner2 == null)
			{
				obj = null;
			}
			else
			{
				Player player = owner2.Player;
				obj = ((player != null) ? player.NetId.ToString() : null);
			}
			if (obj == null)
			{
				obj = "null";
			}
			defaultInterpolatedStringHandler.AppendFormatted((string?)obj);
			defaultInterpolatedStringHandler.AppendLiteral(" active=");
			defaultInterpolatedStringHandler.AppendFormatted(count);
			defaultInterpolatedStringHandler.AppendLiteral(" dodge=");
			defaultInterpolatedStringHandler.AppendFormatted(count2);
			ValencinaProbeLog.Info("precog-counter-queue-cleared", defaultInterpolatedStringHandler.ToStringAndClear(), 30);
		}
	}

	private bool CanResolveCounter(Creature? target, string phase)
	{
		Creature owner = ((PowerModel)this).Owner;
		Player val = ((owner != null) ? owner.Player : null);
		ICombatState obj = ((owner != null) ? owner.CombatState : null);
		CombatState val2 = (CombatState)(object)((obj is CombatState) ? obj : null);
		bool flag = IsCreatureStillInCombat(val2, owner);
		bool flag2 = IsCreatureStillInCombat(val2, target);
		bool flag3 = IsCombatStillAcceptingCounterActions();
		int num;
		if (owner != null && val != null && val2 != null && flag3 && owner.IsAlive && target != null && target.IsAlive && !target.IsDead && flag && flag2)
		{
			num = ((!IsCounterBlocked) ? 1 : 0);
			if (num != 0)
			{
				goto IL_0191;
			}
		}
		else
		{
			num = 0;
		}
		ValencinaProbeLog.Warn("precog-counter-skipped", $"Counter skipped phase={phase} owner={((owner != null) ? owner.Name : null) ?? "null"}/net={((val != null) ? val.NetId.ToString() : null) ?? "null"} target={((target != null) ? target.Name : null) ?? "null"} combatActive={flag3} ownerInCombat={flag} targetInCombat={flag2} blocked={IsCounterBlocked}", 40);
		goto IL_0191;
		IL_0191:
		return (byte)num != 0;
	}

	private static bool IsCreatureStillInCombat(CombatState? combatState, Creature? creature)
	{
		if (combatState == null || creature == null)
		{
			return false;
		}
		try
		{
			return combatState.Creatures.Contains(creature);
		}
		catch
		{
			return false;
		}
	}

	private static bool IsCombatStillAcceptingCounterActions()
	{
		try
		{
			return CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsOverOrEnding;
		}
		catch
		{
			return false;
		}
	}

	private void RefreshDodgeUi()
	{
		AmmoUiSync.RefreshAll(showFallbackLabel: false);
		DodgeHealthBarOverlay.RefreshForCreature(((PowerModel)this).Owner);
	}

	private void RefreshBasePrecognitionDodge()
	{
		_basePrecognitionDodgeThreshold = ((!_isOverheated) ? (Math.Max(0, _precognitionAmount) / DodgeConversionRatio) : 0);
	}

	private async Task SyncTemporaryPrecognitionPowerAsync(CardModel? sourceCard)
	{
		Creature owner = ((PowerModel)this).Owner;
		TemporaryPrecognitionPower temporaryPrecognitionPower = ((owner != null) ? owner.GetPower<TemporaryPrecognitionPower>() : null);
		if (temporaryPrecognitionPower != null)
		{
			await PowerCmd.Remove((PowerModel)(object)temporaryPrecognitionPower);
		}
	}

	private bool RestorePrecognition(int amount)
	{
		if (amount <= 0 || IsPrecognitionLockedByFarewell)
		{
			return false;
		}
		int maxPrecognitionForOwner = MaxPrecognitionForOwner;
		if (_precognitionAmount >= maxPrecognitionForOwner)
		{
			return false;
		}
		int num = Math.Min(maxPrecognitionForOwner, _precognitionAmount + amount);
		if (num == _precognitionAmount)
		{
			return false;
		}
		SetPrecognitionAmount(num);
		return true;
	}

	public void GainTemporaryDodgeThreshold(int amount)
	{
		if (amount <= 0 || IsPrecognitionLockedByFarewell)
		{
			return;
		}
		if (_isOverheated)
		{
			if (RestorePrecognition(amount))
			{
				((PowerModel)this).Flash();
			}
		}
		else
		{
			_temporaryDodgeThreshold += amount;
			((PowerModel)this).InvokeDisplayAmountChanged();
			RefreshDodgeUi();
			((PowerModel)this).Flash();
		}
	}

	public async Task GainTemporaryPrecognitionAsync(int amount, CardModel? sourceCard = null)
	{
		if (amount > 0 && !IsPrecognitionLockedByFarewell)
		{
			if (_isOverheated && RestorePrecognition(amount))
			{
				((PowerModel)this).Flash();
			}
			await SyncTemporaryPrecognitionPowerAsync(sourceCard);
		}
	}

	public void GainTemporaryPrecognition(int amount)
	{
		GainTemporaryPrecognitionAsync(amount);
	}

	public void GainPrecognition(int amount)
	{
		if (amount > 0 && !IsPrecognitionLockedByFarewell && RestorePrecognition(amount))
		{
			((PowerModel)this).Flash();
		}
	}

	public void SetPrecognition(int amount)
	{
		SetPrecognitionAmount(amount);
		((PowerModel)this).Flash();
	}

	public int SpendPrecognition(int amount, bool preferTemporary = true)
	{
		if (amount <= 0 || _isOverheated || IsPrecognitionLockedByFarewell)
		{
			return 0;
		}
		int num = Math.Min(_precognitionAmount, amount);
		if (num <= 0)
		{
			return 0;
		}
		SetPrecognitionAmount(_precognitionAmount - num);
		_precognitionSpentThisTurn += num;
		RefreshDodgeUi();
		((PowerModel)this).Flash();
		return num;
	}

	public async Task<int> SpendPrecognitionForEffectAsync(PlayerChoiceContext choiceContext, int amount, bool keepAtLeastOne = false)
	{
		if (amount <= 0 || _isOverheated || IsPrecognitionLockedByFarewell)
		{
			return 0;
		}
		int num = (keepAtLeastOne ? 1 : 0);
		int val = Math.Max(0, _precognitionAmount - num);
		int spent = Math.Min(amount, val);
		if (spent <= 0)
		{
			return 0;
		}
		SetPrecognitionAmount(_precognitionAmount - spent);
		await TrackPrecognitionSpentAsync(choiceContext, spent);
		RefreshDodgeUi();
		((PowerModel)this).Flash();
		return spent;
	}

	private async Task GainPrecognitionFromPowerAsync(int amount)
	{
		if (amount > 0 && !IsPrecognitionLockedByFarewell && RestorePrecognition(amount))
		{
			((PowerModel)this).Flash();
			await Task.CompletedTask;
		}
	}

	public void ForceOverheat()
	{
		_temporaryPrecognitionAmount = 0;
		_temporaryDodgeThreshold = 0;
		Creature owner = ((PowerModel)this).Owner;
		TemporaryPrecognitionPower temporaryPrecognitionPower = ((owner != null) ? owner.GetPower<TemporaryPrecognitionPower>() : null);
		if (temporaryPrecognitionPower != null)
		{
			PowerCmd.Remove((PowerModel)(object)temporaryPrecognitionPower);
		}
		SetPrecognitionAmount(0);
		RefreshDodgeUi();
	}

	public void BeginAllEnemyAttacksDodgedCheck()
	{
		_trackAllEnemyAttacksDodged = true;
		_trackedEnemyAttackSeen = false;
		_trackedAllEnemyAttacksDodged = true;
	}

	public bool ConsumeAllEnemyAttacksDodgedCheck()
	{
		bool result = _trackAllEnemyAttacksDodged && _trackedEnemyAttackSeen && _trackedAllEnemyAttacksDodged;
		_trackAllEnemyAttacksDodged = false;
		_trackedEnemyAttackSeen = false;
		_trackedAllEnemyAttacksDodged = false;
		return result;
	}

	public void BlockCounterUntilPlayerTurnStart()
	{
		_counterBlockedUntilPlayerTurnStart = true;
	}

	public void BlockCounterUntilCombatEnd()
	{
		_counterBlockedUntilCombatEnd = true;
		_pendingActiveCounterTargets.Clear();
		_pendingDodgeCounters.Clear();
	}

	public static bool TryPreviewDefense(Player owner, decimal incomingDamage, out decimal previewDamage, out bool fullyDodged)
	{
		return TryPreviewDefense(owner, incomingDamage, ReadCurrentBlock(owner.Creature), out previewDamage, out fullyDodged);
	}

	public static bool TryPreviewDefense(Player owner, decimal incomingDamage, decimal blockAvailable, out decimal previewDamage, out bool fullyDodged)
	{
		previewDamage = incomingDamage;
		fullyDodged = false;
		object obj;
		if (owner == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = owner.Creature;
			obj = ((creature != null) ? creature.GetPower<InstantForesightPower>() : null);
		}
		InstantForesightPower instantForesightPower = (InstantForesightPower)obj;
		if (instantForesightPower == null || instantForesightPower._isOverheated || incomingDamage <= 0m)
		{
			return false;
		}
		int dodgeValue = instantForesightPower.DodgeValue;
		decimal num = Math.Max(0m, incomingDamage - Math.Max(0m, blockAvailable));
		if (num <= 0m)
		{
			return false;
		}
		if (num <= (decimal)dodgeValue)
		{
			previewDamage = default(decimal);
			fullyDodged = true;
			return true;
		}
		previewDamage = num - (decimal)dodgeValue;
		return dodgeValue > 0;
	}

	public static bool TryPreviewDefenseForCurrentCombat(decimal incomingDamage, out decimal previewDamage, out bool fullyDodged)
	{
		previewDamage = incomingDamage;
		fullyDodged = false;
		if (NCombatRoom.Instance == null)
		{
			return false;
		}
		foreach (Node child in ((Node)NCombatRoom.Instance).GetChildren(false))
		{
			if (TryFindPrecognitionOwnerRecursive(child, out Player owner) && owner != null)
			{
				return TryPreviewDefense(owner, incomingDamage, out previewDamage, out fullyDodged);
			}
		}
		return false;
	}

	private static bool TryFindPrecognitionOwnerRecursive(Node node, out Player? owner)
	{
		owner = null;
		NCreature val = (NCreature)(object)((node is NCreature) ? node : null);
		if (val != null)
		{
			Player player = val.Entity.Player;
			if (player != null)
			{
				Creature creature = player.Creature;
				if (((creature != null) ? creature.GetPower<InstantForesightPower>() : null) != null)
				{
					owner = player;
					return true;
				}
			}
		}
		foreach (Node child in node.GetChildren(false))
		{
			if (TryFindPrecognitionOwnerRecursive(child, out owner))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanDodgeAfterBlock(Creature target, decimal incomingDamage)
	{
		return Math.Max(0m, incomingDamage - ReadCurrentBlock(target)) <= (decimal)DodgeValue;
	}

	public static decimal ReadCurrentBlock(Creature? creature)
	{
		if (creature == null)
		{
			return 0m;
		}
		string[] array = new string[2] { "CurrentBlock", "Block" };
		foreach (string name in array)
		{
			object obj = ((object)creature).GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(creature);
			if (obj is decimal)
			{
				return (decimal)obj;
			}
			if (obj is int num)
			{
				return num;
			}
			if (obj is float num2)
			{
				return (decimal)num2;
			}
			if (obj is double num3)
			{
				return (decimal)num3;
			}
		}
		return 0m;
	}

	private static bool IsIntentDamagePreviewCalculation()
	{
		string stackTrace = Environment.StackTrace;
		if (!stackTrace.Contains("MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent.GetSingleDamage", StringComparison.Ordinal) && !stackTrace.Contains("MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent.GetTotalDamage", StringComparison.Ordinal) && !stackTrace.Contains("MegaCrit.Sts2.Core.Nodes.Combat.NIntent.UpdateVisuals", StringComparison.Ordinal))
		{
			return stackTrace.Contains("MegaCrit.Sts2.Core.Nodes.Combat.NIntent._Process", StringComparison.Ordinal);
		}
		return true;
	}

	private void SetPrecognitionAmount(int amount)
	{
		if (IsPrecognitionLockedByFarewell)
		{
			amount = 1;
		}
		int precognitionAmount = _precognitionAmount;
		bool isOverheated = _isOverheated;
		_precognitionAmount = Math.Max(0, amount);
		if (IsPrecognitionLockedByFarewell)
		{
			_isOverheated = false;
		}
		else if (_precognitionAmount == 0)
		{
			_isOverheated = true;
		}
		else if (_isOverheated && _precognitionAmount >= MaxPrecognitionForOwner)
		{
			_isOverheated = false;
		}
		bool flag = isOverheated != _isOverheated;
		bool flag2 = !isOverheated && _isOverheated;
		bool flag3 = isOverheated && !_isOverheated;
		if (flag2)
		{
			_basePrecognitionDodgeThreshold = 0;
		}
		else if (flag3)
		{
			RefreshBasePrecognitionDodge();
		}
		int num = Math.Max(1, _precognitionAmount);
		if (((PowerModel)this).Amount != num)
		{
			((PowerModel)this).SetAmount(num, false);
		}
		else if (precognitionAmount != _precognitionAmount || flag)
		{
			((PowerModel)this).InvokeDisplayAmountChanged();
		}
		RefreshDodgeUi();
		if (precognitionAmount != _precognitionAmount)
		{
			PlayAmountChangedVfx();
		}
		if (flag2)
		{
			PlayOverheatFeedback();
		}
	}

	private void PlayOverheatFeedback()
	{
		try
		{
			object obj;
			if (((PowerModel)this).Owner == null)
			{
				obj = null;
			}
			else
			{
				NCombatRoom instance = NCombatRoom.Instance;
				obj = ((instance != null) ? instance.GetCreatureNode(((PowerModel)this).Owner) : null);
			}
			Node anchor = (Node)obj;
			ValencinaVoiceSfx.TryPlayPrecognitionOverheat(((PowerModel)this).Owner, anchor);
			Creature owner = ((PowerModel)this).Owner;
			if (((owner != null) ? owner.Player : null) != null && LocalContext.IsMe(((PowerModel)this).Owner.Player))
			{
				PlayerHurtVignetteHelper.Play();
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Precognition] Failed to play overheat feedback: " + ex.Message, 1);
		}
	}

	private void PlayAmountChangedVfx()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		try
		{
			if (((PowerModel)this).Owner == null || NCombatRoom.Instance == null)
			{
				return;
			}
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(((PowerModel)this).Owner);
			Node2D val = (Node2D)(object)((creatureNode != null) ? creatureNode.Visuals : null);
			if (val == null)
			{
				return;
			}
			Node2D root = new Node2D
			{
				ZIndex = 24,
				Scale = new Vector2(0.66f, 0.66f),
				Modulate = new Color(1f, 1f, 1f, 0f)
			};
			((Node)val).AddChild((Node)(object)root, false, (InternalMode)0);
			root.GlobalPosition = GetCreatureBodyTopRightGlobalPosition(creatureNode) + AmountChangeVfxOffsetFromBodyTopRight;
			Texture2D val2 = ResourceLoader.Load<Texture2D>(_isOverheated ? "res://Valencina/images/powers/instant_foresight_power_overheat.png" : "res://Valencina/images/powers/odin_eye_power.png", (string)null, (CacheMode)1);
			if (val2 != null)
			{
				Sprite2D val3 = new Sprite2D
				{
					Texture = val2,
					Scale = new Vector2(58f / val2.GetSize().X, 58f / val2.GetSize().Y),
					Modulate = new Color(1f, 1f, 1f, 0.52f)
				};
				((Node)root).AddChild((Node)(object)val3, false, (InternalMode)0);
			}
			AddAmountChangedParticles(root);
			Tween obj = ((Node)root).CreateTween();
			obj.TweenProperty((GodotObject)(object)root, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.58f), 0.05999999865889549);
			obj.TweenProperty((GodotObject)(object)root, NodePath.op_Implicit("scale"), Variant.op_Implicit(new Vector2(0.86f, 0.86f)), 0.07999999821186066);
			obj.TweenProperty((GodotObject)(object)root, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.24f), 0.11999999731779099);
			obj.TweenProperty((GodotObject)(object)root, NodePath.op_Implicit("scale"), Variant.op_Implicit(new Vector2(0.7f, 0.7f)), 0.10000000149011612);
			obj.TweenProperty((GodotObject)(object)root, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.11999999731779099);
			obj.TweenCallback(Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)root))
				{
					((Node)root).QueueFree();
				}
			}));
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Precognition] Failed to play amount change VFX: " + ex.Message, 1);
		}
	}

	private static Vector2 GetCreatureBodyTopRightGlobalPosition(NCreature creatureNode)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			if (TryGetSpriteTopRightGlobalPosition(((Node)visuals).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Idle")), out var topRight))
			{
				return topRight;
			}
			if (TryGetSpriteTopRightGlobalPosition(((Node)visuals).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("DeathMiss")), out var topRight2))
			{
				return topRight2;
			}
			return visuals.ToGlobal(new Vector2(125f, -305f));
		}
		return ((Control)creatureNode).GlobalPosition + new Vector2(125f, -305f);
	}

	private static bool TryGetSpriteTopRightGlobalPosition(Sprite2D? sprite, out Vector2 topRight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		topRight = default(Vector2);
		if (((sprite != null) ? sprite.Texture : null) == null || !GodotObject.IsInstanceValid((GodotObject)(object)sprite))
		{
			return false;
		}
		Vector2 val = sprite.Texture.GetSize() * 0.5f;
		topRight = ((Node2D)sprite).ToGlobal(new Vector2(val.X, 0f - val.Y));
		return true;
	}

	private static void AddAmountChangedParticles(Node2D root)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		Vector2[] array = (Vector2[])(object)new Vector2[8]
		{
			new Vector2(-32f, -18f),
			new Vector2(-18f, -38f),
			new Vector2(10f, -42f),
			new Vector2(32f, -24f),
			new Vector2(38f, 4f),
			new Vector2(20f, 28f),
			new Vector2(-12f, 34f),
			new Vector2(-38f, 10f)
		};
		foreach (Vector2 val in array)
		{
			ColorRect particle = new ColorRect
			{
				Color = new Color(1f, 0.08f, 0.05f, 0.42f),
				Size = new Vector2(5f, 5f),
				Position = new Vector2(-2.5f, -2.5f),
				MouseFilter = (MouseFilterEnum)2
			};
			((Node)root).AddChild((Node)(object)particle, false, (InternalMode)0);
			Tween obj = ((Node)particle).CreateTween();
			obj.TweenProperty((GodotObject)(object)particle, NodePath.op_Implicit("position"), Variant.op_Implicit(val), 0.2800000011920929);
			obj.Parallel().TweenProperty((GodotObject)(object)particle, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.2800000011920929);
			obj.TweenCallback(Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)particle))
				{
					((Node)particle).QueueFree();
				}
			}));
		}
	}

	private async Task TrackPrecognitionSpentAsync(PlayerChoiceContext choiceContext, int spent, bool countForThisTurn = true)
	{
		Creature owner = ((PowerModel)this).Owner;
		Player val = ((owner != null) ? owner.Player : null);
		if (spent <= 0 || owner == null || val == null)
		{
			return;
		}
		if (countForThisTurn)
		{
			_precognitionSpentThisTurn += spent;
		}
		if (!_shinTriggered)
		{
			_precognitionSpentThisCombat += spent;
			if (_precognitionSpentThisCombat >= 30)
			{
				_shinTriggered = true;
				((PowerModel)this).Flash();
				await CommonActions.Apply<ValencinaShinPower>(choiceContext, ((PowerModel)this).Owner, (CardModel?)null, 1m, silent: false);
			}
		}
	}

	private bool IsActiveCounterTrigger()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.CombatState : null) != null)
		{
			return ((PowerModel)this).Owner.CombatState.CurrentSide == ((PowerModel)this).Owner.Side;
		}
		return false;
	}

	private PrecognitionCounterContext CreateCounterContext(Player owner, Creature attacker, decimal preventedDamageThisAttack, bool isActiveTrigger, bool fastAnimation = false)
	{
		return new PrecognitionCounterContext(owner, attacker, _precognitionAmount, MaxPrecognitionForOwner, DodgeValue, preventedDamageThisAttack, _preventedDamageThisTurn, _isOverheated, isActiveTrigger, fastAnimation);
	}

	public static bool WasPreventedByPrecognition(DamageResult result)
	{
		PrecognitionDamageMarker value;
		return PreventedDamageResults.TryGetValue(result, out value);
	}

	public static bool TryGetPrecognitionDodgeDealer(DamageResult result, out Creature? dealer)
	{
		if (PreventedDamageResults.TryGetValue(result, out PrecognitionDamageMarker value))
		{
			dealer = value.Dealer;
			TryConsumePendingPrecognitionDodge(result.Receiver, out Creature _);
			return true;
		}
		dealer = null;
		return false;
	}

	public static bool TryConsumePendingPrecognitionDodge(Creature receiver, out Creature? dealer)
	{
		dealer = null;
		if (!PendingDodgeVisuals.TryGetValue(receiver, out PendingDodgeVisualMarker value) || value.Count <= 0)
		{
			return false;
		}
		value.Count--;
		dealer = value.Dealer;
		return true;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (((PowerModel)this).Owner == null || target != ((PowerModel)this).Owner || _activeAttack == null || dealer != _activeAttack.Attacker)
		{
			return Task.CompletedTask;
		}
		if (!_activeAttack.TryDequeueDecision(out var decision))
		{
			return Task.CompletedTask;
		}
		_activeAttack.HadAttackDamage = true;
		_activeAttack.PreventedDamage += decision.PreventedDamage;
		_activeAttack.FinalHpLoss += Math.Max(0m, result.UnblockedDamage);
		_preventedDamageThisTurn += decision.PreventedDamage;
		if (!decision.FullyDodged)
		{
			if ((decimal)result.UnblockedDamage > 0m)
			{
				_activeAttack.WasFullyPrevented = false;
			}
			MainFile.Logger.Info($"[Precognition] resolved hit final={result.UnblockedDamage} prevented={decision.PreventedDamage} stacks={_precognitionAmount} owner={target.Name}", 1);
			((PowerModel)this).Flash();
			return Task.CompletedTask;
		}
		if (result.UnblockedDamage != 0)
		{
			_activeAttack.WasFullyPrevented = false;
		}
		PreventedDamageResults.GetValue(result, (DamageResult _) => new PrecognitionDamageMarker()).Dealer = dealer;
		MarkPendingDodgeVisual(target, dealer);
		_lastDodgeAnimationTask = ValencinaAnimation.PlayPrecognitionDodge(target, dealer);
		MainFile.Logger.Info($"[Precognition] played dodge hit final={result.UnblockedDamage} prevented={decision.PreventedDamage} dodge={DodgeValue} owner={target.Name}", 1);
		((PowerModel)this).Flash();
		return Task.CompletedTask;
	}

	private static void MarkPendingDodgeVisual(Creature receiver, Creature? dealer)
	{
		PendingDodgeVisualMarker value = PendingDodgeVisuals.GetValue(receiver, (Creature _) => new PendingDodgeVisualMarker());
		value.Dealer = dealer;
		value.Count++;
	}
}
