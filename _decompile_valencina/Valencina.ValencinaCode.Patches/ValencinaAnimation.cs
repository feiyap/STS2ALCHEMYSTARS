using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Settings;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaAnimation
{
	private enum AttackVisualKind
	{
		None,
		Attack1,
		Attack2,
		Disposal
	}

	private readonly struct AttackVisualRequest
	{
		public AttackVisualKind Kind { get; }

		public int HitCount { get; }

		public Creature? DisposalTarget { get; }

		public ulong? TriggerLockMs { get; }

		public AttackVisualRequest(AttackVisualKind kind, int hitCount = 1, Creature? disposalTarget = null, ulong? triggerLockMs = null)
		{
			Kind = kind;
			HitCount = hitCount;
			DisposalTarget = disposalTarget;
			TriggerLockMs = triggerLockMs;
		}
	}

	private sealed class DodgeTweenState
	{
		private readonly WeakReference<NCreature> _creatureNode = new WeakReference<NCreature>(creatureNode);

		public Tween Tween { get; }

		public DodgeTweenState(Tween tween, NCreature creatureNode)
		{
			Tween = tween;
			base._002Ector();
		}

		public NCreature? TryGetCreatureNode()
		{
			if (!_creatureNode.TryGetTarget(out NCreature target))
			{
				return null;
			}
			if (!GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				return null;
			}
			return target;
		}
	}

	private const string AnimationPlayerName = "IdleAnimationPlayer";

	private const string Attack2AnimationPlayerName = "Attack2AnimationPlayer";

	private const string DisposalAnimationPlayerName = "DisposalAnimationPlayer";

	private const string MissTexturePath = "res://Valencina/images/charui/miss.png";

	private const string DamageTexturePath = "res://Valencina/images/charui/dmg.png";

	private const string Attack2Body2Path = "res://Valencina/images/charui/attack2/skill2_2.png";

	private const string Attack2Body35Path = "res://Valencina/images/charui/attack2/skill2_3-5.png";

	private const string Attack2Slash35Path = "res://Valencina/images/charui/attack2/skill2_3-5_e.png";

	private const string Attack2Body46Path = "res://Valencina/images/charui/attack2/skill2_4-6.png";

	private const string Attack2Slash46Path = "res://Valencina/images/charui/attack2/skill2_4-6_e.png";

	private const string Attack2Body7Path = "res://Valencina/images/charui/attack2/skill2_7.png";

	private const string DisposalFrameRootPath = "res://Valencina/images/vfx/disposal";

	private const string PrecognitionDodgeFrameName = "PrecognitionDodgeMissFrame";

	private const string DamageFrameName = "ValencinaDamageFrame";

	private const string RuntimeOverlayMeta = "ValencinaRuntimeOverlay";

	private const string Attack2RootName = "Attack2";

	private const ulong AttackTriggerLockMs = 520uL;

	private static readonly bool Skill1CardAnimationEnabled = false;

	private const ulong Skill1AnimationLengthMs = 1045uL;

	private const ulong Skill1QueueLockMs = 90uL;

	private const ulong Attack2TriggerLockMs = 740uL;

	public const ulong FastCounterAttack2TriggerLockMs = 180uL;

	private const float DisposalAnimationSpeed = 1.2f;

	private const ulong DisposalTriggerLockMs = 4600uL;

	private const float DisposalCommandWaitSeconds = 4.516667f;

	private const float DisposalFinalFrameRightLocalX = 595.075f;

	private const ulong DisposalDamageSettleSuppressMs = 1500uL;

	private const float DisposalUiFadeOutSeconds = 0.34f;

	private const float DisposalUiFadeInSeconds = 0.42f;

	private const float DisposalUiHiddenAlpha = 0.12f;

	private const ulong BlockHitTriggerLockMs = 360uL;

	private const ulong DamageTriggerLockMs = 260uL;

	internal const float DeathAnimationLengthSeconds = 1.05f;

	private const float DodgeFrameLifetimeSeconds = 0.24f;

	private const float DodgeMinimumPresentationSeconds = 0.18f;

	private const int CombatEndGracefulQuiesceDelayMs = 1200;

	private static readonly Vector2 MissFramePosition = new Vector2(-1.0843031f, -176f);

	private static readonly Vector2 MissFrameScale = new Vector2(0.6295038f, 0.7057465f);

	private static readonly Vector2 DamageFramePosition = new Vector2(-25f, -170f);

	private static readonly Vector2 DamageFrameScale = new Vector2(0.6837703f, 0.7146129f);

	private const int BodyFrameZIndex = 0;

	private const int DodgeFrameZIndex = 0;

	private const int DamageFrameZIndex = 0;

	private const int VfxFrameZIndex = 8;

	private const int FrontVfxFrameZIndex = 9;

	private static readonly Vector2 DeathMissStartPosition = new Vector2(-4f, -176f);

	private static readonly Vector2 DeathMissMidPosition = new Vector2(-440f, -176f);

	private static readonly Vector2 DeathMissEndPosition = new Vector2(-2100f, -176f);

	private static readonly HashSet<ulong> ConnectedPlayers = new HashSet<ulong>();

	private static readonly Dictionary<ulong, string> BusyAnimations = new Dictionary<ulong, string>();

	private static readonly Dictionary<ulong, ulong> SuppressUntilTicks = new Dictionary<ulong, ulong>();

	private static readonly Dictionary<ulong, ulong> PostDisposalAttackSuppressUntilTicks = new Dictionary<ulong, ulong>();

	private static readonly Dictionary<ulong, int> QueuedAttackHitCounts = new Dictionary<ulong, int>();

	private static readonly Dictionary<ulong, bool> QueuedAttackSuppressFollowups = new Dictionary<ulong, bool>();

	private static readonly Dictionary<ulong, ulong> QueuedAttackTriggerLockMs = new Dictionary<ulong, ulong>();

	private static readonly Dictionary<ulong, int> SuppressedAttackFollowupCounts = new Dictionary<ulong, int>();

	private static readonly Dictionary<ulong, SemaphoreSlim> AttackAnimationGates = new Dictionary<ulong, SemaphoreSlim>();

	private static readonly HashSet<ulong> QueuedDisposalAttacks = new HashSet<ulong>();

	private static readonly Dictionary<ulong, Creature> QueuedDisposalTargets = new Dictionary<ulong, Creature>();

	private static readonly Dictionary<ulong, Vector2> DisposalOriginalGlobalPositions = new Dictionary<ulong, Vector2>();

	private static readonly Dictionary<ulong, Vector2> DisposalOriginalVisualScales = new Dictionary<ulong, Vector2>();

	private static readonly Dictionary<ulong, Camera2D> DisposalCinematicCameras = new Dictionary<ulong, Camera2D>();

	private static readonly Dictionary<ulong, Camera2D> DisposalPreviousCameras = new Dictionary<ulong, Camera2D>();

	private static readonly Dictionary<ulong, Dictionary<CanvasItem, Color>> DisposalHiddenUiItems = new Dictionary<ulong, Dictionary<CanvasItem, Color>>();

	private static readonly Dictionary<ulong, Tween> DisposalUiFadeTweens = new Dictionary<ulong, Tween>();

	private static readonly Dictionary<ulong, Tween> DisposalFinalShakeTweens = new Dictionary<ulong, Tween>();

	private static readonly Dictionary<ulong, Vector2> DisposalFinalShakeBasePositions = new Dictionary<ulong, Vector2>();

	private static readonly Dictionary<ulong, Creature> ActiveDisposalTargets = new Dictionary<ulong, Creature>();

	private static readonly Dictionary<ulong, DodgeTweenState> DodgeTweens = new Dictionary<ulong, DodgeTweenState>();

	private static readonly Dictionary<ulong, Tween> DeathTweens = new Dictionary<ulong, Tween>();

	private static readonly Dictionary<ulong, Vector2> BaseVisualPositions = new Dictionary<ulong, Vector2>();

	private static readonly HashSet<ulong> DeathSfxPlayed = new HashSet<ulong>();

	private static readonly HashSet<ulong> DeathAnimationsStarted = new HashSet<ulong>();

	private static int VisualQuarantineGeneration;

	private static int PendingCombatEndQuiesceRequest;

	private static long CustomVisualsSuppressedUntilTicks;

	public static IEnumerable<string> Attack2AssetPaths
	{
		get
		{
			yield return "res://Valencina/images/charui/attack2/skill2_2.png";
			yield return "res://Valencina/images/charui/attack2/skill2_3-5.png";
			yield return "res://Valencina/images/charui/attack2/skill2_3-5_e.png";
			yield return "res://Valencina/images/charui/attack2/skill2_4-6.png";
			yield return "res://Valencina/images/charui/attack2/skill2_4-6_e.png";
			yield return "res://Valencina/images/charui/attack2/skill2_7.png";
		}
	}

	public static IEnumerable<string> DisposalAssetPaths
	{
		get
		{
			for (int i = 0; i <= 18; i++)
			{
				yield return $"{"res://Valencina/images/vfx/disposal"}/skill3_{i:0000}.png";
			}
		}
	}

	internal static bool ShouldSkipCustomDeathForRunEnding(NCreature creatureNode)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			NCombatRoom instance = NCombatRoom.Instance;
			CombatRoomMode? val = ((instance != null) ? new CombatRoomMode?(instance.Mode) : ((CombatRoomMode?)null));
			RunHistory history = RunManager.Instance.History;
			if (history != null && history.Win)
			{
				Creature entity = creatureNode.Entity;
				ValencinaProbeLog.Info("death-skip-victory-settlement", "Skipped custom Valencina death animation during victory settlement for " + (((entity != null) ? entity.Name : null) ?? "null") + ". roomMode=" + ((val.HasValue ? ((object)val.GetValueOrDefault()/*cast due to .constrained prefix*/).ToString() : null) ?? "null"), 12);
				return true;
			}
			if (val.HasValue && (int)val.Value != 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Skipped custom Valencina death animation outside active combat for ");
				Creature entity2 = creatureNode.Entity;
				defaultInterpolatedStringHandler.AppendFormatted(((entity2 != null) ? entity2.Name : null) ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral(". roomMode=");
				defaultInterpolatedStringHandler.AppendFormatted<CombatRoomMode>(val.Value);
				ValencinaProbeLog.Info("death-skip-non-active-combat-room", defaultInterpolatedStringHandler.ToStringAndClear(), 12);
				return true;
			}
		}
		catch (Exception ex)
		{
			ValencinaProbeLog.Warn("death-skip-check-error", "Run-ending death skip check failed: " + ex.GetType().Name + ": " + ex.Message, 8);
		}
		return false;
	}

	internal static bool AreCustomVisualsSuppressedForTeardown()
	{
		long num = Interlocked.Read(in CustomVisualsSuppressedUntilTicks);
		if (num > 0)
		{
			return Time.GetTicksMsec() < (ulong)num;
		}
		return false;
	}

	private static void SuppressCustomVisualsForTeardown(string reason, ulong milliseconds)
	{
		ulong value = Time.GetTicksMsec() + milliseconds;
		Interlocked.Exchange(ref CustomVisualsSuppressedUntilTicks, (long)value);
		ValencinaProbeLog.Info("visuals-teardown-suppressed", $"Suppressed Valencina custom combat visuals. reason={reason}, milliseconds={milliseconds}.", 30);
	}

	private static int CaptureVisualQuarantineGeneration()
	{
		return Volatile.Read(in VisualQuarantineGeneration);
	}

	private static bool HasVisualQuarantineAdvanced(int generation)
	{
		return Volatile.Read(in VisualQuarantineGeneration) != generation;
	}

	private static bool ShouldAbortVisualContinuation(int generation)
	{
		if (!HasVisualQuarantineAdvanced(generation))
		{
			return AreCustomVisualsSuppressedForTeardown();
		}
		return true;
	}

	internal static void QuiesceForCombatEnd(string reason)
	{
		int value = Interlocked.Increment(ref VisualQuarantineGeneration);
		int num = 0;
		int num2 = 0;
		try
		{
			foreach (NCombatRoom item in EnumerateKnownCombatRooms())
			{
				if (item == null || !GodotObject.IsInstanceValid((GodotObject)(object)item))
				{
					continue;
				}
				foreach (NCreature item2 in item.CreatureNodes.ToList())
				{
					if (item2.Entity != null)
					{
						ClearTransientStateForCreature(item2.Entity);
						num++;
						if (QuiesceVisualNode((Node?)(object)item2.Visuals, "combat end " + reason))
						{
							num2++;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			ValencinaProbeLog.Warn("combat-end-quiesce-error", $"Failed while quiescing Valencina visuals for combat end. reason={reason}, exception={ex.GetType().Name}: {ex.Message}", 12);
		}
		ValencinaProbeLog.Warn("combat-end-quiesce", $"Quiesced Valencina combat-end visuals. reason={reason}, generation={value}, clearedCreatures={num}, quiescedVisuals={num2}.", 30);
	}

	internal static void QuiesceForCombatEndAfterGrace(string reason)
	{
		int request = Interlocked.Increment(ref PendingCombatEndQuiesceRequest);
		QuiesceForCombatEndAfterGraceAsync(reason, request);
	}

	private static async Task QuiesceForCombatEndAfterGraceAsync(string reason, int request)
	{
		try
		{
			await Task.Delay(1200);
			if (request == Volatile.Read(in PendingCombatEndQuiesceRequest))
			{
				QuiesceForCombatEnd(reason + "-grace");
			}
		}
		catch (Exception ex)
		{
			ValencinaProbeLog.Warn("combat-end-grace-quiesce-error", $"Failed while scheduling graceful Valencina combat-end visual quiesce. reason={reason}, exception={ex.GetType().Name}: {ex.Message}", 12);
		}
	}

	internal static void QuiesceForRunTeardown(string reason)
	{
		SuppressCustomVisualsForTeardown(reason, 8000uL);
		int value = Interlocked.Increment(ref VisualQuarantineGeneration);
		int num = 0;
		int num2 = 0;
		try
		{
			foreach (NCombatRoom item in EnumerateKnownCombatRooms())
			{
				if (item == null || !GodotObject.IsInstanceValid((GodotObject)(object)item))
				{
					continue;
				}
				foreach (NCreature item2 in item.CreatureNodes.ToList())
				{
					if (item2.Entity != null)
					{
						ClearTransientStateForCreature(item2.Entity);
						num++;
						if (QuiesceVisualNode((Node?)(object)item2.Visuals, "run teardown " + reason))
						{
							num2++;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			ValencinaProbeLog.Warn("run-teardown-quiesce-error", $"Failed while quiescing Valencina visuals for run teardown. reason={reason}, exception={ex.GetType().Name}: {ex.Message}", 12);
		}
		CleanupAllDetachedState();
		ValencinaProbeLog.Warn("run-teardown-quiesce", $"Quiesced Valencina run teardown visuals. reason={reason}, generation={value}, clearedCreatures={num}, quiescedVisuals={num2}.", 30);
	}

	private static IEnumerable<NCombatRoom?> EnumerateKnownCombatRooms()
	{
		NCombatRoom val = null;
		try
		{
			val = NCombatRoom.Instance;
		}
		catch
		{
		}
		yield return val;
		NEventRoom val2 = null;
		try
		{
			val2 = NEventRoom.Instance;
		}
		catch
		{
		}
		if (val2 != null)
		{
			yield return val2.EmbeddedCombatRoom;
		}
	}

	private static void CleanupAllDetachedState()
	{
		foreach (ulong item in DodgeTweens.Keys.ToList())
		{
			KillDodgeTween(item, resetPosition: true);
		}
		foreach (ulong item2 in DisposalFinalShakeTweens.Keys.ToList())
		{
			KillDisposalFinalShake(item2);
		}
		foreach (ulong item3 in DisposalUiFadeTweens.Keys.ToList())
		{
			KillDisposalUiFade(item3);
		}
		foreach (ulong item4 in DisposalCinematicCameras.Keys.ToList())
		{
			EndDisposalCamera(item4);
		}
		ConnectedPlayers.Clear();
		BusyAnimations.Clear();
		SuppressUntilTicks.Clear();
		PostDisposalAttackSuppressUntilTicks.Clear();
		QueuedAttackHitCounts.Clear();
		QueuedAttackSuppressFollowups.Clear();
		QueuedAttackTriggerLockMs.Clear();
		SuppressedAttackFollowupCounts.Clear();
		QueuedDisposalAttacks.Clear();
		QueuedDisposalTargets.Clear();
		ActiveDisposalTargets.Clear();
		DisposalOriginalGlobalPositions.Clear();
		DisposalOriginalVisualScales.Clear();
		DisposalFinalShakeBasePositions.Clear();
		BaseVisualPositions.Clear();
		DeathSfxPlayed.Clear();
		DeathAnimationsStarted.Clear();
		AttackAnimationGates.Clear();
	}

	private static bool QuiesceVisualNode(Node? visual, string reason)
	{
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		if (visual == null || !GodotObject.IsInstanceValid((GodotObject)(object)visual))
		{
			return false;
		}
		foreach (AnimationPlayer item in FindAnimationPlayers(visual).ToList())
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item))
			{
				try
				{
					item.Stop(false);
					((AnimationMixer)item).Active = false;
					item.SpeedScale = 1f;
				}
				catch
				{
				}
			}
		}
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Idle")), visible: true);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackBody")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackSlash")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackSlashGold")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2/Slash")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DisposalVfx")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DisposalSlash")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/BlockHit")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/ValencinaDamageFrame")), visible: false);
		SetCanvasVisible(visual.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DeathMiss")), visible: false);
		Node nodeOrNull = visual.GetNodeOrNull<Node>(NodePath.op_Implicit("Visuals/ShinAura"));
		if (nodeOrNull != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull))
		{
			CanvasItem val = (CanvasItem)(object)((nodeOrNull is CanvasItem) ? nodeOrNull : null);
			if (val != null)
			{
				val.Visible = false;
			}
			nodeOrNull.SetProcess(false);
			nodeOrNull.ProcessMode = (ProcessModeEnum)4;
		}
		RemoveRuntimeOverlays(visual);
		Node2D val2 = (Node2D)(object)((visual is Node2D) ? visual : null);
		if (val2 != null)
		{
			val2.Position = Vector2.Zero;
		}
		CanvasItem val3 = (CanvasItem)(object)((visual is CanvasItem) ? visual : null);
		if (val3 != null)
		{
			Color modulate = val3.Modulate;
			modulate.A = 1f;
			val3.Modulate = modulate;
			val3.SelfModulate = Colors.White;
			val3.Visible = true;
			((Node)val3).ProcessMode = (ProcessModeEnum)0;
		}
		return true;
	}

	private static void RemoveRuntimeOverlays(Node root)
	{
		foreach (Node item in ((IEnumerable)root.GetChildren(false)).OfType<Node>().ToList())
		{
			if (((GodotObject)item).HasMeta(StringName.op_Implicit("ValencinaRuntimeOverlay")))
			{
				DetachAndQueueFree(item);
			}
			else
			{
				RemoveRuntimeOverlays(item);
			}
		}
	}

	private static void ClearTransientStateForCreature(Creature creature)
	{
		ulong creatureKey = GetCreatureKey(creature);
		BusyAnimations.Remove(creatureKey);
		SuppressUntilTicks.Remove(creatureKey);
		PostDisposalAttackSuppressUntilTicks.Remove(creatureKey);
		QueuedAttackHitCounts.Remove(creatureKey);
		QueuedAttackSuppressFollowups.Remove(creatureKey);
		QueuedAttackTriggerLockMs.Remove(creatureKey);
		SuppressedAttackFollowupCounts.Remove(creatureKey);
		QueuedDisposalAttacks.Remove(creatureKey);
		QueuedDisposalTargets.Remove(creatureKey);
		ActiveDisposalTargets.Remove(creatureKey);
		NCombatRoom instance = NCombatRoom.Instance;
		RestoreDisposalVisualMirror((instance != null) ? instance.GetCreatureNode(creature) : null);
		DisposalOriginalGlobalPositions.Remove(creatureKey);
		KillDodgeTween(creatureKey, resetPosition: true);
		KillDisposalFinalShake(creatureKey);
		AttackAnimationGates.Remove(creatureKey);
		EndDisposalCamera(creatureKey);
		ResetDisposalUiHide(creatureKey, restoreImmediately: true);
		BaseVisualPositions.Remove(creatureKey);
		DeathSfxPlayed.Remove(creatureKey);
		DeathAnimationsStarted.Remove(creatureKey);
	}

	internal static void ClearCombatRoomState(NCombatRoom? combatRoom)
	{
		if (combatRoom == null)
		{
			return;
		}
		try
		{
			foreach (NCreature item in combatRoom.CreatureNodes.ToList())
			{
				Creature entity = item.Entity;
				object obj;
				if (entity == null)
				{
					obj = null;
				}
				else
				{
					Player player = entity.Player;
					obj = ((player != null) ? player.Character : null);
				}
				if (obj is Valencina.ValencinaCode.Character.Valencina)
				{
					ClearTransientStateForCreature(item.Entity);
				}
			}
		}
		catch (Exception ex)
		{
			ValencinaProbeLog.Warn("animation-clear-combat-state-failed", "Failed to clear Valencina combat animation state. exception=" + ex.GetType().Name + ": " + ex.Message, 12);
		}
	}

	public static void QueueNextDisposalAttack(Creature? creature, Creature? target = null)
	{
		if (creature != null)
		{
			ulong creatureKey = GetCreatureKey(creature);
			QueuedDisposalAttacks.Add(creatureKey);
			if (target != null)
			{
				QueuedDisposalTargets[creatureKey] = target;
			}
		}
	}

	public static bool HasQueuedDisposalAttack(Creature? creature)
	{
		if (creature != null)
		{
			return QueuedDisposalAttacks.Contains(GetCreatureKey(creature));
		}
		return false;
	}

	public static void ClearPostDisposalAttackSuppression(Creature? creature)
	{
		if (creature != null)
		{
			PostDisposalAttackSuppressUntilTicks.Remove(GetCreatureKey(creature));
		}
	}

	public static void QueueNextAttackVariant(Creature? creature, int hitCount, bool playOnEveryHit = true, ulong? triggerLockMs = null)
	{
		if (creature == null || (hitCount <= 1 && !triggerLockMs.HasValue))
		{
			return;
		}
		ulong creatureKey = GetCreatureKey(creature);
		if (!QueuedDisposalAttacks.Contains(creatureKey))
		{
			QueuedAttackHitCounts[creatureKey] = Math.Max(triggerLockMs.HasValue ? Math.Max(1, hitCount) : Math.Max(2, hitCount), QueuedAttackHitCounts.TryGetValue(creatureKey, out var value) ? value : (triggerLockMs.HasValue ? 1 : 2));
			QueuedAttackSuppressFollowups[creatureKey] = playOnEveryHit;
			if (triggerLockMs.HasValue)
			{
				QueuedAttackTriggerLockMs[creatureKey] = (QueuedAttackTriggerLockMs.TryGetValue(creatureKey, out var value2) ? Math.Min(value2, triggerLockMs.Value) : triggerLockMs.Value);
			}
		}
	}

	public static Task PlayAttackFromCommand(Creature creature, float waitTime)
	{
		if (creature.IsDead)
		{
			return Task.CompletedTask;
		}
		if (TryConsumeSuppressedFollowupAttackTrigger(GetCreatureKey(creature)))
		{
			return Task.CompletedTask;
		}
		AttackVisualRequest request = ConsumeAttackVisualRequest(creature);
		if (request.Kind == AttackVisualKind.None)
		{
			return Task.CompletedTask;
		}
		if (request.Kind == AttackVisualKind.Disposal)
		{
			return PlayAttackFromCommandQueuedAsync(creature, waitTime, request);
		}
		PlayAttackFromCommandQueuedAsync(creature, waitTime, request);
		return Task.CompletedTask;
	}

	private static AttackVisualRequest ConsumeAttackVisualRequest(Creature creature)
	{
		ulong creatureKey = GetCreatureKey(creature);
		if (TryConsumeQueuedDisposalAttack(creatureKey, out Creature target))
		{
			return new AttackVisualRequest(AttackVisualKind.Disposal, 5, target);
		}
		if (TryConsumeQueuedAttackVariant(creatureKey, out var hitCount, out var suppressFollowupAttackTriggers, out var triggerLockMs))
		{
			if (suppressFollowupAttackTriggers)
			{
				SuppressedAttackFollowupCounts[creatureKey] = Math.Max(Math.Max(0, hitCount - 1), SuppressedAttackFollowupCounts.TryGetValue(creatureKey, out var value) ? value : 0);
			}
			return new AttackVisualRequest(AttackVisualKind.Attack2, hitCount, null, triggerLockMs);
		}
		if (TryConsumeSuppressedAttackTrigger(creatureKey))
		{
			return new AttackVisualRequest(AttackVisualKind.None);
		}
		return new AttackVisualRequest(AttackVisualKind.Attack1);
	}

	private static async Task PlayAttackFromCommandQueuedAsync(Creature creature, float waitTime, AttackVisualRequest request)
	{
		if (creature.IsDead || request.Kind == AttackVisualKind.None)
		{
			return;
		}
		int visualGeneration = CaptureVisualQuarantineGeneration();
		ulong creatureKey = GetCreatureKey(creature);
		SemaphoreSlim gate = GetAttackAnimationGate(creatureKey);
		await gate.WaitAsync();
		try
		{
			if (creature.IsDead || ShouldAbortVisualContinuation(visualGeneration))
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(creature) : null);
			if (val != null)
			{
				if (request.Kind == AttackVisualKind.Disposal)
				{
					flag2 = PlayDisposal(val, request.DisposalTarget);
					flag = flag2 || PlayAttack2(val, 5);
				}
				else
				{
					flag = request.Kind switch
					{
						AttackVisualKind.Attack2 => PlayAttack2(val, request.HitCount), 
						AttackVisualKind.Attack1 => PlayOn(val, "attack"), 
						_ => false, 
					};
				}
			}
			if (request.Kind == AttackVisualKind.Disposal && flag2)
			{
				await Cmd.CustomScaledWait(4.516667f, 4.516667f, false, default(CancellationToken));
				if (!ShouldAbortVisualContinuation(visualGeneration))
				{
					PostDisposalAttackSuppressUntilTicks[creatureKey] = Time.GetTicksMsec() + 1500;
				}
			}
			else if (request.Kind == AttackVisualKind.Disposal && flag)
			{
				await Cmd.CustomScaledWait(0.74f, 0.74f, false, default(CancellationToken));
				if (!ShouldAbortVisualContinuation(visualGeneration))
				{
				}
			}
			else if (request.Kind == AttackVisualKind.Attack2 && flag)
			{
				ulong num = request.TriggerLockMs ?? 740;
				await Cmd.CustomScaledWait((float)num / 1000f, (float)num / 1000f, false, default(CancellationToken));
				if (!ShouldAbortVisualContinuation(visualGeneration))
				{
				}
			}
			else if (flag)
			{
				await Cmd.CustomScaledWait(Mathf.Min(waitTime * 0.5f, 0.25f), waitTime, false, default(CancellationToken));
				if (!ShouldAbortVisualContinuation(visualGeneration))
				{
				}
			}
		}
		finally
		{
			gate.Release();
		}
	}

	public static bool PlayAttackFromNode(NCreature creatureNode)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return false;
		}
		if (IsDeadOrDying(creatureNode))
		{
			PlayDeath(creatureNode);
			return true;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (TryConsumeQueuedDisposalAttack(creatureKey, out Creature target))
		{
			if (!PlayDisposal(creatureNode, target))
			{
				return PlayAttack2(creatureNode, 5);
			}
			return true;
		}
		if (TryConsumeQueuedAttackVariant(creatureKey, out var hitCount, out var suppressFollowupAttackTriggers, out var _))
		{
			if (suppressFollowupAttackTriggers)
			{
				SuppressedAttackFollowupCounts[creatureKey] = Math.Max(Math.Max(0, hitCount - 1), SuppressedAttackFollowupCounts.TryGetValue(creatureKey, out var value) ? value : 0);
			}
			return PlayAttack2(creatureNode, hitCount);
		}
		if (TryConsumeSuppressedAttackTrigger(creatureKey))
		{
			return true;
		}
		return PlayOn(creatureNode, "attack");
	}

	public static void PlaySkill1FromCard(Creature creature)
	{
		if (Skill1CardAnimationEnabled && !creature.IsDead && !AreCustomVisualsSuppressedForTeardown())
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(creature) : null);
			if (val != null)
			{
				PlaySkill1QueuedAsync(val);
			}
		}
	}

	private static async Task PlaySkill1QueuedAsync(NCreature creatureNode)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		int visualGeneration = CaptureVisualQuarantineGeneration();
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		SemaphoreSlim gate = GetAttackAnimationGate(creatureKey);
		await gate.WaitAsync();
		try
		{
			if (!ShouldAbortVisualContinuation(visualGeneration) && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && !IsDeadOrDying(creatureNode) && PlayOn(creatureNode, "skill1"))
			{
				await Cmd.CustomScaledWait(0.09f, 0.09f, false, default(CancellationToken));
			}
		}
		finally
		{
			gate.Release();
		}
	}

	public static bool PlayQueuedAttackVariant(NCreature creatureNode)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return false;
		}
		if (IsDeadOrDying(creatureNode))
		{
			PlayDeath(creatureNode);
			return true;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (TryConsumeQueuedDisposalAttack(creatureKey, out Creature target))
		{
			if (!PlayDisposal(creatureNode, target))
			{
				return PlayAttack2(creatureNode, 5);
			}
			return true;
		}
		if (!TryConsumeQueuedAttackVariant(creatureKey, out var hitCount, out var suppressFollowupAttackTriggers, out var _))
		{
			return false;
		}
		if (suppressFollowupAttackTriggers)
		{
			SuppressedAttackFollowupCounts[creatureKey] = Math.Max(Math.Max(0, hitCount - 1), SuppressedAttackFollowupCounts.TryGetValue(creatureKey, out var value) ? value : 0);
		}
		return PlayAttack2(creatureNode, hitCount);
	}

	public static bool PlayOn(NCreature creatureNode, string animationName, bool allowOverride = false)
	{
		if (AreCustomVisualsSuppressedForTeardown() || IsDisabledCombatAnimation(animationName) || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return false;
		}
		NormalizeVisualLayering(creatureNode);
		if (IsDeadOrDying(creatureNode) && !string.Equals(animationName, "death", StringComparison.Ordinal) && !string.Equals(animationName, "revive", StringComparison.Ordinal))
		{
			PlayDeath(creatureNode);
			return false;
		}
		AnimationPlayer val = (string.Equals(animationName, "disposal", StringComparison.Ordinal) ? GetDisposalAnimationPlayer(creatureNode) : GetPrimaryAnimationPlayer(creatureNode));
		if (val == null || !((AnimationMixer)val).HasAnimation(StringName.op_Implicit(animationName)))
		{
			return false;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		EnsureIdleResume(creatureNode, val, creatureKey);
		if (!allowOverride && !CanStartAnimation(creatureKey, animationName))
		{
			return false;
		}
		BeginExclusiveAnimation(creatureNode, animationName);
		val.SpeedScale = (string.Equals(animationName, "disposal", StringComparison.Ordinal) ? 1.2f : 1f);
		val.Play(StringName.op_Implicit(animationName), -1.0, 1f, false);
		if (!string.Equals(animationName, "death", StringComparison.Ordinal))
		{
			ArmSafetyFinishAsync(creatureNode, animationName, TriggerLockMsFor(animationName) + 120);
		}
		if (ValencinaLocalSfx.ShouldPlayForPlayer(creatureNode.Entity.Player))
		{
			float num = ValencinaLocalSfx.VolumeMultiplierForPlayer(creatureNode.Entity.Player);
			if (string.Equals(animationName, "attack", StringComparison.Ordinal))
			{
				ValencinaLocalSfx.PlayAttackSequence((Node)(object)val, num);
			}
			else if (string.Equals(animationName, "skill1", StringComparison.Ordinal))
			{
				ValencinaLocalSfx.PlaySfx("attack/atk1_1.mp3", 0f, num, 0f, 1f, (Node?)(object)val);
			}
			else if (string.Equals(animationName, "disposal", StringComparison.Ordinal))
			{
				ValencinaLocalSfx.PlayDisposalSequence((Node)(object)val, num, 1.2f);
			}
			else if (string.Equals(animationName, "death", StringComparison.Ordinal) && DeathSfxPlayed.Add(creatureKey))
			{
				ValencinaLocalSfx.Play("res://Valencina/audio/death/death.mp3", (Node?)(object)val, num);
			}
		}
		return true;
	}

	private static bool PlayDisposal(NCreature creatureNode, Creature? target)
	{
		if (ValencinaModConfig.DisableDisposalAnimation)
		{
			return false;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (target != null)
		{
			ActiveDisposalTargets[creatureKey] = target;
		}
		else
		{
			ActiveDisposalTargets.Remove(creatureKey);
		}
		MoveDisposalActorNearTarget(creatureNode, target);
		StartDisposalCinematic(creatureNode, target);
		bool num = PlayOn(creatureNode, "disposal");
		if (!num)
		{
			ActiveDisposalTargets.Remove(creatureKey);
			EndDisposalCinematic(creatureNode);
			RestoreDisposalActorPosition(creatureNode);
		}
		return num;
	}

	private static bool PlayAttack2(NCreature creatureNode, int hitCount)
	{
		if (AreCustomVisualsSuppressedForTeardown() || ValencinaModConfig.DisableAttackAnimations || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return false;
		}
		NormalizeVisualLayering(creatureNode);
		if (IsDeadOrDying(creatureNode))
		{
			PlayDeath(creatureNode);
			return true;
		}
		AnimationPlayer attack2AnimationPlayer = GetAttack2AnimationPlayer(creatureNode);
		if (attack2AnimationPlayer == null || !((AnimationMixer)attack2AnimationPlayer).HasAnimation(StringName.op_Implicit("attack2")))
		{
			return PlayOn(creatureNode, "attack");
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		EnsureIdleResume(creatureNode, attack2AnimationPlayer, creatureKey);
		BeginExclusiveAnimation(creatureNode, "attack2");
		SetAttack2RootVisible(creatureNode, visible: true);
		attack2AnimationPlayer.Play(StringName.op_Implicit("attack2"), -1.0, 1f, false);
		ArmSafetyFinishAsync(creatureNode, "attack2", 860uL);
		if (ValencinaLocalSfx.ShouldPlayForPlayer(creatureNode.Entity.Player))
		{
			PlayAttack2SceneSfx((Node)(object)attack2AnimationPlayer, hitCount, ValencinaLocalSfx.VolumeMultiplierForPlayer(creatureNode.Entity.Player));
		}
		return true;
	}

	public static Task PlayPrecognitionDodge(NCreature creatureNode, Creature? attacker)
	{
		if (ValencinaModConfig.DisableAttackAnimations || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return Task.CompletedTask;
		}
		return PlayPrecognitionDodgeSafe(creatureNode, attacker);
	}

	public static Task PlayPrecognitionDodge(Creature receiver, Creature? attacker)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(receiver) : null);
		if (val == null)
		{
			return Task.CompletedTask;
		}
		return PlayPrecognitionDodge(val, attacker);
	}

	public static void PlayDamageFrame(NCreature creatureNode)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		if (AreCustomVisualsSuppressedForTeardown() || IsDisabledCombatAnimation("damage") || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		NormalizeVisualLayering(creatureNode);
		if (IsDeadOrDying(creatureNode))
		{
			PlayDeath(creatureNode);
		}
		else
		{
			if (PlayOn(creatureNode, "damage", allowOverride: true))
			{
				return;
			}
			Node2D bodyLayer = GetBodyLayer(creatureNode);
			if (bodyLayer == null)
			{
				return;
			}
			int visualGeneration = CaptureVisualQuarantineGeneration();
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			KillDodgeTween(creatureKey, resetPosition: true);
			BeginExclusiveAnimation(creatureNode, "damage");
			RemoveOverlayFrameFromCreature(creatureNode, "ValencinaDamageFrame");
			Sprite2D damageFrame = CreateOverlayFrame("res://Valencina/images/charui/dmg.png", "ValencinaDamageFrame", DamageFramePosition, DamageFrameScale);
			if (damageFrame == null)
			{
				FinishAnimation(creatureNode, "damage", creatureNode.Entity.CurrentHp > 0);
				return;
			}
			((Node)bodyLayer).AddChild((Node)(object)damageFrame, false, (InternalMode)0);
			Tween val = ((Node)creatureNode).CreateTween();
			DodgeTweens[creatureKey] = new DodgeTweenState(val, creatureNode);
			val.TweenInterval(0.23999999463558197);
			val.TweenCallback(Callable.From((Action)delegate
			{
				if (damageFrame != null && GodotObject.IsInstanceValid((GodotObject)(object)damageFrame))
				{
					((Node)damageFrame).QueueFree();
				}
				DodgeTweens.Remove(creatureKey);
				if (!ShouldAbortVisualContinuation(visualGeneration))
				{
					FinishAnimation(creatureNode, "damage", creatureNode.Entity.CurrentHp > 0);
				}
			}));
		}
	}

	public static bool PlayDeathFromVanillaDeathFlow(NCreature creatureNode)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) || ShouldSkipCustomDeathForRunEnding(creatureNode))
		{
			return false;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		bool flag = !DeathAnimationsStarted.Contains(creatureKey);
		bool num = PlayDeath(creatureNode);
		if (num && flag)
		{
			ExtendVanillaDeathTask(creatureNode);
		}
		return num;
	}

	public static bool PlayDeath(NCreature creatureNode)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) || ShouldSkipCustomDeathForRunEnding(creatureNode))
		{
			return false;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (!DeathAnimationsStarted.Add(creatureKey))
		{
			return true;
		}
		PrepareForDeath(creatureNode);
		bool flag = PlayOn(creatureNode, "death", allowOverride: true);
		if (!flag)
		{
			BusyAnimations[creatureKey] = "death";
			SuppressUntilTicks[creatureKey] = Time.GetTicksMsec() + 1050;
			ShowDeathFallback(creatureNode);
			if (ValencinaLocalSfx.ShouldPlayForPlayer(creatureNode.Entity.Player) && DeathSfxPlayed.Add(creatureKey))
			{
				ValencinaLocalSfx.Play("res://Valencina/audio/death/death.mp3", (Node?)(object)creatureNode, ValencinaLocalSfx.VolumeMultiplierForPlayer(creatureNode.Entity.Player));
			}
			flag = true;
		}
		StartDeathMotionGuard(creatureNode);
		return flag;
	}

	public static void PrepareForDeath(NCreature creatureNode)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && !ShouldSkipCustomDeathForRunEnding(creatureNode))
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			QueuedAttackHitCounts.Remove(creatureKey);
			QueuedAttackSuppressFollowups.Remove(creatureKey);
			QueuedAttackTriggerLockMs.Remove(creatureKey);
			SuppressedAttackFollowupCounts.Remove(creatureKey);
			QueuedDisposalAttacks.Remove(creatureKey);
			QueuedDisposalTargets.Remove(creatureKey);
			ActiveDisposalTargets.Remove(creatureKey);
			PostDisposalAttackSuppressUntilTicks.Remove(creatureKey);
			KillDodgeTween(creatureKey, resetPosition: true);
			KillDeathTween(creatureKey);
			KillDisposalFinalShake(creatureKey, creatureNode);
			EndDisposalCinematic(creatureNode);
			RestoreDisposalActorPosition(creatureNode);
			ResetDisposalUiHide(creatureKey, restoreImmediately: true);
			RemoveRuntimeOverlays((Node)(object)creatureNode.Visuals);
			NormalizeVisualLayering(creatureNode);
			HideTransientFrames(creatureNode, includeDeathMiss: true);
			SetIdleVisible(creatureNode, visible: false);
			Node2D visuals = (Node2D)(object)creatureNode.Visuals;
			if (visuals != null)
			{
				visuals.Position = GetBaseVisualPosition(creatureNode);
			}
			BusyAnimations[creatureKey] = "death";
			SuppressUntilTicks[creatureKey] = Time.GetTicksMsec() + 1050;
		}
	}

	private static void ExtendVanillaDeathTask(NCreature creatureNode)
	{
		Task deathAnimationTask = creatureNode.DeathAnimationTask;
		Task task = WaitForDeathAnimationEnd(creatureNode);
		Task deathAnimationTask2;
		if (deathAnimationTask != null)
		{
			global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = deathAnimationTask;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = task;
			deathAnimationTask2 = Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
		}
		else
		{
			deathAnimationTask2 = task;
		}
		creatureNode.DeathAnimationTask = deathAnimationTask2;
	}

	private static async Task WaitForDeathAnimationEnd(NCreature creatureNode)
	{
		try
		{
			await Cmd.Wait(1.05f, creatureNode.DeathAnimCancelToken.Token, true);
		}
		catch
		{
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			if (BusyAnimations.TryGetValue(creatureKey, out string value) && string.Equals(value, "death", StringComparison.Ordinal))
			{
				SuppressUntilTicks.Remove(creatureKey);
			}
		}
	}

	private static void ShowDeathFallback(NCreature creatureNode)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		SetIdleVisible(creatureNode, visible: false);
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			CanvasItem nodeOrNull = visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DeathMiss"));
			if (nodeOrNull != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull))
			{
				nodeOrNull.Visible = true;
				nodeOrNull.ZIndex = 0;
				nodeOrNull.ZAsRelative = true;
			}
		}
	}

	private static void StartDeathMotionGuard(NCreature creatureNode)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals == null)
		{
			return;
		}
		Node2D nodeOrNull = visuals.GetNodeOrNull<Node2D>(NodePath.op_Implicit("Visuals/DeathMiss"));
		if (nodeOrNull == null)
		{
			return;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		KillDeathTween(creatureKey);
		((CanvasItem)nodeOrNull).Visible = true;
		nodeOrNull.Position = DeathMissStartPosition;
		Tween tween = ((Node)creatureNode).CreateTween();
		DeathTweens[creatureKey] = tween;
		tween.TweenInterval(0.28);
		tween.TweenProperty((GodotObject)(object)nodeOrNull, NodePath.op_Implicit("position"), Variant.op_Implicit(DeathMissMidPosition), 0.3).SetEase((EaseType)0).SetTrans((TransitionType)7);
		tween.TweenProperty((GodotObject)(object)nodeOrNull, NodePath.op_Implicit("position"), Variant.op_Implicit(DeathMissEndPosition), 0.47).SetEase((EaseType)0).SetTrans((TransitionType)7);
		tween.TweenCallback(Callable.From((Action)delegate
		{
			if (DeathTweens.TryGetValue(creatureKey, out Tween value) && value == tween)
			{
				DeathTweens.Remove(creatureKey);
			}
		}));
	}

	public static void ResetIfAlive(NCreature creatureNode, bool forceIdle = false)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && creatureNode.Entity.CurrentHp > 0)
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			if (forceIdle || !BusyAnimations.ContainsKey(creatureKey))
			{
				FinishAnimation(creatureNode, "idle", resumeIdle: true, force: true);
			}
		}
	}

	private static async Task PlayPrecognitionDodgeSafe(NCreature creatureNode, Creature? attacker)
	{
		try
		{
			await PlayPrecognitionDodgeNow(creatureNode, attacker);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Precognition] dodge animation skipped after error: " + ex.Message, 1);
			if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				ResetIfAlive(creatureNode, forceIdle: true);
			}
		}
	}

	private static async Task PlayPrecognitionDodgeNow(NCreature creatureNode, Creature? attacker)
	{
		if (AreCustomVisualsSuppressedForTeardown() || IsDisabledCombatAnimation("dodge") || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) || IsDeadOrDying(creatureNode))
		{
			return;
		}
		Node2D bodyLayer = GetBodyLayer(creatureNode);
		if (bodyLayer == null)
		{
			return;
		}
		NormalizeVisualLayering(creatureNode);
		int visualGeneration = CaptureVisualQuarantineGeneration();
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		StoreBaseVisualPosition(creatureNode);
		KillDodgeTween(creatureKey, resetPosition: true);
		RemoveOverlayFrameFromCreature(creatureNode, "PrecognitionDodgeMissFrame");
		SetIdleVisible(creatureNode, visible: false);
		float num = ResolveDodgeDirection(creatureNode, attacker);
		Sprite2D val = CreateMissOverlayFrame();
		if (val != null)
		{
			((Node)bodyLayer).AddChild((Node)(object)val, false, (InternalMode)0);
			MainFile.Logger.Info("[Precognition] miss.png overlay frame shown.", 1);
		}
		Vector2 baseVisualPosition = GetBaseVisualPosition(creatureNode);
		Vector2 val2 = baseVisualPosition + new Vector2(24f * num, -2f);
		Tween tween = ((Node)creatureNode).CreateTween();
		DodgeTweens[creatureKey] = new DodgeTweenState(tween, creatureNode);
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			visuals.Position = baseVisualPosition;
			tween.TweenProperty((GodotObject)(object)visuals, NodePath.op_Implicit("position"), Variant.op_Implicit(val2), 0.04500000178813934).SetTrans((TransitionType)1).SetEase((EaseType)1);
			tween.TweenProperty((GodotObject)(object)visuals, NodePath.op_Implicit("position"), Variant.op_Implicit(baseVisualPosition), 0.1599999964237213).SetTrans((TransitionType)1).SetEase((EaseType)2);
			tween.TweenInterval((double)Math.Max(0f, 0.034999996f));
		}
		else
		{
			tween.TweenInterval(0.23999999463558197);
		}
		tween.TweenCallback(Callable.From((Action)delegate
		{
			if (IsCurrentDodgeTween(creatureKey, tween))
			{
				CompleteDodgeTween(creatureKey, creatureNode);
				if (!ShouldAbortVisualContinuation(visualGeneration) && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && creatureNode.Entity.CurrentHp > 0 && !BusyAnimations.ContainsKey(creatureKey))
				{
					SetIdleVisible(creatureNode, visible: true);
				}
			}
		}));
		SceneTree tree = ((Node)creatureNode).GetTree();
		if (tree != null)
		{
			SceneTreeTimer val3 = tree.CreateTimer(0.18000000715255737, true, false, false);
			await ((GodotObject)creatureNode).ToSignal((GodotObject)(object)val3, SignalName.Timeout);
		}
	}

	private static void BeginExclusiveAnimation(NCreature creatureNode, string animationName)
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (!BusyAnimations.TryGetValue(creatureKey, out string value) || !string.Equals(value, "death", StringComparison.Ordinal) || string.Equals(animationName, "death", StringComparison.Ordinal) || string.Equals(animationName, "revive", StringComparison.Ordinal))
		{
			if (string.Equals(animationName, "revive", StringComparison.Ordinal))
			{
				ClearDeathState(creatureKey);
			}
			StoreBaseVisualPosition(creatureNode);
			KillDodgeTween(creatureKey, resetPosition: true);
			KillDisposalFinalShake(creatureKey, creatureNode);
			AnimationPlayer primaryAnimationPlayer = GetPrimaryAnimationPlayer(creatureNode);
			if (primaryAnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)primaryAnimationPlayer))
			{
				primaryAnimationPlayer.SpeedScale = 1f;
				primaryAnimationPlayer.Stop(false);
			}
			AnimationPlayer attack2AnimationPlayer = GetAttack2AnimationPlayer(creatureNode);
			if (attack2AnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)attack2AnimationPlayer))
			{
				attack2AnimationPlayer.Stop(false);
			}
			AnimationPlayer disposalAnimationPlayer = GetDisposalAnimationPlayer(creatureNode);
			if (disposalAnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)disposalAnimationPlayer))
			{
				disposalAnimationPlayer.SpeedScale = 1f;
				disposalAnimationPlayer.Stop(false);
			}
			bool flag = string.Equals(animationName, "death", StringComparison.Ordinal) || string.Equals(animationName, "revive", StringComparison.Ordinal);
			HideTransientFrames(creatureNode, !flag);
			SetIdleVisible(creatureNode, string.Equals(animationName, "idle", StringComparison.Ordinal));
			Node2D visuals = (Node2D)(object)creatureNode.Visuals;
			if (visuals != null)
			{
				visuals.Position = GetBaseVisualPosition(creatureNode);
			}
			BusyAnimations[creatureKey] = animationName;
			SuppressUntilTicks[creatureKey] = Time.GetTicksMsec() + TriggerLockMsFor(animationName);
		}
	}

	private static void FinishAnimation(NCreature creatureNode, string animationName, bool resumeIdle, bool force = false)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (!force && BusyAnimations.TryGetValue(creatureKey, out string value) && !string.Equals(value, animationName, StringComparison.Ordinal))
		{
			return;
		}
		BusyAnimations.Remove(creatureKey);
		SuppressUntilTicks.Remove(creatureKey);
		AnimationPlayer primaryAnimationPlayer = GetPrimaryAnimationPlayer(creatureNode);
		AnimationPlayer attack2AnimationPlayer = GetAttack2AnimationPlayer(creatureNode);
		if (primaryAnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)primaryAnimationPlayer) && !string.Equals(animationName, "idle", StringComparison.Ordinal))
		{
			primaryAnimationPlayer.Stop(false);
		}
		if (attack2AnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)attack2AnimationPlayer))
		{
			attack2AnimationPlayer.Stop(false);
		}
		AnimationPlayer disposalAnimationPlayer = GetDisposalAnimationPlayer(creatureNode);
		if (disposalAnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)disposalAnimationPlayer))
		{
			disposalAnimationPlayer.SpeedScale = 1f;
			disposalAnimationPlayer.Stop(false);
		}
		HideTransientFrames(creatureNode, includeDeathMiss: true);
		EndDisposalCinematic(creatureNode);
		RestoreDisposalActorPosition(creatureNode);
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			visuals.Position = GetBaseVisualPosition(creatureNode);
		}
		if (resumeIdle && creatureNode.Entity.CurrentHp > 0)
		{
			SetIdleVisible(creatureNode, visible: true);
			PlayIdle(creatureNode);
		}
		if (string.Equals(animationName, "disposal", StringComparison.Ordinal))
		{
			NCreature val = ConsumeDisposalShakeTarget(creatureNode);
			if (val != null)
			{
				PlayDisposalFinalShake(val);
			}
		}
	}

	private static void PlayIdle(NCreature creatureNode)
	{
		AnimationPlayer val = FindAnimationPlayerWithAnimation(creatureNode, "idle");
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			val.SpeedScale = 1f;
			val.Play(StringName.op_Implicit("idle"), -1.0, 1f, false);
		}
	}

	private static AnimationPlayer? FindAnimationPlayerWithAnimation(NCreature creatureNode, string animationName)
	{
		AnimationPlayer primaryAnimationPlayer = GetPrimaryAnimationPlayer(creatureNode);
		if (primaryAnimationPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)primaryAnimationPlayer) && ((AnimationMixer)primaryAnimationPlayer).HasAnimation(StringName.op_Implicit(animationName)))
		{
			return primaryAnimationPlayer;
		}
		foreach (AnimationPlayer item in FindAnimationPlayers((Node)(object)creatureNode))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && ((AnimationMixer)item).HasAnimation(StringName.op_Implicit(animationName)))
			{
				return item;
			}
		}
		return null;
	}

	private static IEnumerable<AnimationPlayer> FindAnimationPlayers(Node root)
	{
		foreach (Node child in root.GetChildren(false))
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)child))
			{
				continue;
			}
			AnimationPlayer val = (AnimationPlayer)(object)((child is AnimationPlayer) ? child : null);
			if (val != null)
			{
				yield return val;
			}
			foreach (AnimationPlayer item in FindAnimationPlayers(child))
			{
				yield return item;
			}
		}
	}

	private static bool CanStartAnimation(ulong creatureKey, string animationName)
	{
		if (string.Equals(animationName, "death", StringComparison.Ordinal))
		{
			return true;
		}
		if (!BusyAnimations.TryGetValue(creatureKey, out string value))
		{
			if (SuppressUntilTicks.TryGetValue(creatureKey, out var value2) && Time.GetTicksMsec() < value2)
			{
				return false;
			}
			return true;
		}
		if (string.Equals(animationName, value, StringComparison.Ordinal))
		{
			return false;
		}
		if (string.Equals(animationName, "attack2", StringComparison.Ordinal))
		{
			return !string.Equals(value, "death", StringComparison.Ordinal);
		}
		if (string.Equals(animationName, "skill1", StringComparison.Ordinal))
		{
			return !string.Equals(value, "death", StringComparison.Ordinal);
		}
		if (string.Equals(animationName, "attack", StringComparison.Ordinal) && string.Equals(value, "skill1", StringComparison.Ordinal))
		{
			return true;
		}
		if (string.Equals(animationName, "disposal", StringComparison.Ordinal))
		{
			return !string.Equals(value, "death", StringComparison.Ordinal);
		}
		if (string.Equals(animationName, "damage", StringComparison.Ordinal) || string.Equals(animationName, "block_hit", StringComparison.Ordinal))
		{
			return !string.Equals(value, "death", StringComparison.Ordinal);
		}
		return false;
	}

	private static bool TryConsumeQueuedAttackVariant(ulong creatureKey, out int hitCount, out bool suppressFollowupAttackTriggers, out ulong? triggerLockMs)
	{
		triggerLockMs = null;
		suppressFollowupAttackTriggers = false;
		if (QueuedAttackHitCounts.TryGetValue(creatureKey, out hitCount) && hitCount > 0)
		{
			QueuedAttackHitCounts.Remove(creatureKey);
			if (QueuedAttackSuppressFollowups.TryGetValue(creatureKey, out var value))
			{
				suppressFollowupAttackTriggers = value;
			}
			QueuedAttackSuppressFollowups.Remove(creatureKey);
			if (QueuedAttackTriggerLockMs.TryGetValue(creatureKey, out var value2))
			{
				triggerLockMs = value2;
			}
			QueuedAttackTriggerLockMs.Remove(creatureKey);
			return true;
		}
		QueuedAttackSuppressFollowups.Remove(creatureKey);
		QueuedAttackTriggerLockMs.Remove(creatureKey);
		hitCount = 1;
		return false;
	}

	private static bool TryConsumeSuppressedFollowupAttackTrigger(ulong creatureKey)
	{
		if (!SuppressedAttackFollowupCounts.TryGetValue(creatureKey, out var value) || value <= 0)
		{
			SuppressedAttackFollowupCounts.Remove(creatureKey);
			return false;
		}
		if (value <= 1)
		{
			SuppressedAttackFollowupCounts.Remove(creatureKey);
		}
		else
		{
			SuppressedAttackFollowupCounts[creatureKey] = value - 1;
		}
		return true;
	}

	public static void ClearAttackCommandAnimationState(Creature? creature)
	{
		if (creature != null)
		{
			ulong creatureKey = GetCreatureKey(creature);
			QueuedAttackHitCounts.Remove(creatureKey);
			QueuedAttackSuppressFollowups.Remove(creatureKey);
			QueuedAttackTriggerLockMs.Remove(creatureKey);
			SuppressedAttackFollowupCounts.Remove(creatureKey);
		}
	}

	private static bool TryConsumeQueuedDisposalAttack(ulong creatureKey, out Creature? target)
	{
		QueuedDisposalTargets.TryGetValue(creatureKey, out target);
		QueuedDisposalTargets.Remove(creatureKey);
		if (!QueuedDisposalAttacks.Remove(creatureKey))
		{
			target = null;
			return false;
		}
		QueuedAttackHitCounts.Remove(creatureKey);
		QueuedAttackSuppressFollowups.Remove(creatureKey);
		QueuedAttackTriggerLockMs.Remove(creatureKey);
		SuppressedAttackFollowupCounts.Remove(creatureKey);
		return true;
	}

	private static bool TryConsumeSuppressedAttackTrigger(ulong creatureKey)
	{
		if (PostDisposalAttackSuppressUntilTicks.TryGetValue(creatureKey, out var value))
		{
			if (Time.GetTicksMsec() < value)
			{
				return true;
			}
			PostDisposalAttackSuppressUntilTicks.Remove(creatureKey);
		}
		return false;
	}

	private static void HideTransientFrames(NCreature creatureNode, bool includeDeathMiss)
	{
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackBody")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackSlash")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackSlashGold")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Skill1EyeVfx")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DisposalVfx")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DisposalSlash")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/BlockHit")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/ValencinaDamageFrame")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2/Body")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2/Slash")), visible: false);
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2")), visible: false);
			RemoveOverlayFrameFromCreature(creatureNode, "PrecognitionDodgeMissFrame");
			RemoveOverlayFrameFromCreature(creatureNode, "ValencinaDamageFrame");
			if (includeDeathMiss)
			{
				SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DeathMiss")), visible: false);
			}
		}
	}

	private static void RemoveOverlayFrameFromCreature(NCreature creatureNode, string frameName)
	{
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			RemoveOverlayFrame(visuals, frameName);
		}
	}

	private static void RemoveOverlayFrame(Node parent, string frameName)
	{
		foreach (Node item in ((IEnumerable)parent.GetChildren(false)).OfType<Node>().ToList())
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item))
			{
				if (IsRuntimeOverlayFrame(item, frameName))
				{
					DetachAndQueueFree(item);
				}
				else
				{
					RemoveOverlayFrame(item, frameName);
				}
			}
		}
	}

	private static bool IsRuntimeOverlayFrame(Node node, string frameName)
	{
		if (!((GodotObject)node).HasMeta(StringName.op_Implicit("ValencinaRuntimeOverlay")))
		{
			return false;
		}
		string text = ((object)node.Name).ToString();
		if (!string.Equals(text, frameName, StringComparison.Ordinal))
		{
			return text.Contains(frameName, StringComparison.Ordinal);
		}
		return true;
	}

	private static void DetachAndQueueFree(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			Node parent = node.GetParent();
			if (parent != null && GodotObject.IsInstanceValid((GodotObject)(object)parent))
			{
				parent.RemoveChild(node);
			}
			node.QueueFree();
		}
	}

	private static void SetCanvasVisible(CanvasItem? item, bool visible)
	{
		if (item != null && GodotObject.IsInstanceValid((GodotObject)(object)item))
		{
			item.Visible = visible;
		}
	}

	private static void SetIdleVisible(NCreature creatureNode, bool visible)
	{
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Idle")), visible);
		}
	}

	private static void SetAttack2RootVisible(NCreature creatureNode, bool visible)
	{
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			SetCanvasVisible(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2")), visible);
		}
	}

	private static Node2D? GetBodyLayer(NCreature creatureNode)
	{
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals == null)
		{
			return null;
		}
		return ((Node)visuals).GetNodeOrNull<Node2D>(NodePath.op_Implicit("Visuals")) ?? visuals;
	}

	private static void NormalizeVisualLayering(NCreature creatureNode)
	{
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals == null)
		{
			return;
		}
		Node nodeOrNull = visuals.GetNodeOrNull<Node>(NodePath.op_Implicit("Visuals"));
		if (nodeOrNull != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull))
		{
			Node nodeOrNull2 = nodeOrNull.GetNodeOrNull(NodePath.op_Implicit("ShinAura"));
			if (nodeOrNull2 != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull2))
			{
				nodeOrNull.MoveChild(nodeOrNull2, 0);
			}
		}
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Idle")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackBody")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/BlockHit")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/ValencinaDamageFrame")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DeathMiss")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2/Body")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackSlash")), 8);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Skill1EyeVfx")), 11);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/Attack2/Slash")), 8);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DisposalVfx")), 9);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/DisposalSlash")), 10);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/AttackSlashGold")), 9);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/PrecognitionDodgeMissFrame")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Visuals/ValencinaDamageFrame")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("PrecognitionDodgeMissFrame")), 0);
		SetLayer(visuals.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("ValencinaDamageFrame")), 0);
	}

	private static void SetLayer(CanvasItem? item, int zIndex)
	{
		if (item != null && GodotObject.IsInstanceValid((GodotObject)(object)item))
		{
			item.ZIndex = zIndex;
			item.ZAsRelative = true;
			item.ShowBehindParent = false;
		}
	}

	private static AnimationPlayer? GetPrimaryAnimationPlayer(NCreature creatureNode)
	{
		return ((Node)creatureNode).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("IdleAnimationPlayer")) ?? ((Node)creatureNode.Visuals).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("IdleAnimationPlayer"));
	}

	private static AnimationPlayer? GetAttack2AnimationPlayer(NCreature creatureNode)
	{
		return ((Node)creatureNode).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("Attack2AnimationPlayer")) ?? ((Node)creatureNode.Visuals).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("Attack2AnimationPlayer"));
	}

	private static AnimationPlayer? GetDisposalAnimationPlayer(NCreature creatureNode)
	{
		return ((Node)creatureNode).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("DisposalAnimationPlayer")) ?? ((Node)creatureNode.Visuals).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("DisposalAnimationPlayer"));
	}

	private static bool IsDeadOrDying(NCreature creatureNode)
	{
		if (!creatureNode.Entity.IsDead)
		{
			return creatureNode.Entity.CurrentHp <= 0;
		}
		return true;
	}

	private static bool IsDisabledCombatAnimation(string animationName)
	{
		if (!string.Equals(animationName, "disposal", StringComparison.Ordinal) || !ValencinaModConfig.DisableDisposalAnimation)
		{
			if (ValencinaModConfig.DisableAttackAnimations)
			{
				if (!string.Equals(animationName, "attack", StringComparison.Ordinal) && !string.Equals(animationName, "attack2", StringComparison.Ordinal) && !string.Equals(animationName, "skill1", StringComparison.Ordinal) && !string.Equals(animationName, "disposal", StringComparison.Ordinal) && !string.Equals(animationName, "damage", StringComparison.Ordinal) && !string.Equals(animationName, "dodge", StringComparison.Ordinal))
				{
					return string.Equals(animationName, "block_hit", StringComparison.Ordinal);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private static ulong TriggerLockMsFor(string animationName)
	{
		return animationName switch
		{
			"attack" => 520uL, 
			"skill1" => 1045uL, 
			"attack2" => 740uL, 
			"disposal" => 4600uL, 
			"block_hit" => 360uL, 
			"damage" => 260uL, 
			"dodge" => 260uL, 
			"revive" => 1100uL, 
			_ => 0uL, 
		};
	}

	private static async Task ArmSafetyFinishAsync(NCreature creatureNode, string animationName, ulong delayMs)
	{
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				return;
			}
			SceneTree tree = ((Node)creatureNode).GetTree();
			if (tree == null)
			{
				return;
			}
			int visualGeneration = CaptureVisualQuarantineGeneration();
			SceneTreeTimer val = tree.CreateTimer(Math.Max(0.05, (double)delayMs / 1000.0), true, false, false);
			await ((GodotObject)creatureNode).ToSignal((GodotObject)(object)val, SignalName.Timeout);
			if (!ShouldAbortVisualContinuation(visualGeneration) && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				ulong creatureKey = GetCreatureKey(creatureNode.Entity);
				if (BusyAnimations.TryGetValue(creatureKey, out string value) && string.Equals(value, animationName, StringComparison.Ordinal))
				{
					FinishAnimation(creatureNode, animationName, !string.Equals(animationName, "death", StringComparison.Ordinal));
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[ValencinaAnimation] Safety finish failed for {animationName}: {ex.GetType().Name}: {ex.Message}", 1);
		}
	}

	private static void EnsureIdleResume(NCreature creatureNode, AnimationPlayer animationPlayer, ulong creatureKey)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		ulong playerId = ((GodotObject)animationPlayer).GetInstanceId();
		if (!ConnectedPlayers.Add(playerId))
		{
			return;
		}
		((Node)animationPlayer).TreeExiting += delegate
		{
			Cleanup(playerId, creatureKey);
		};
		((AnimationMixer)animationPlayer).AnimationFinished += (AnimationFinishedEventHandler)delegate(StringName animationName)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)animationPlayer) && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				string text = ((object)animationName).ToString();
				if (text.Equals("attack", StringComparison.Ordinal) || text.Equals("skill1", StringComparison.Ordinal) || text.Equals("attack2", StringComparison.Ordinal) || text.Equals("disposal", StringComparison.Ordinal) || text.Equals("block_hit", StringComparison.Ordinal) || text.Equals("damage", StringComparison.Ordinal) || text.Equals("dodge", StringComparison.Ordinal) || text.Equals("revive", StringComparison.Ordinal))
				{
					FinishAnimation(creatureNode, text, creatureNode.Entity.CurrentHp > 0);
				}
			}
		};
	}

	private static Vector2 GetBaseVisualPosition(NCreature creatureNode)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (BaseVisualPositions.TryGetValue(creatureKey, out var value))
		{
			return value;
		}
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals == null)
		{
			return Vector2.Zero;
		}
		return visuals.Position;
	}

	private static void StoreBaseVisualPosition(NCreature creatureNode)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			if (!BaseVisualPositions.ContainsKey(creatureKey))
			{
				BaseVisualPositions[creatureKey] = visuals.Position;
			}
		}
	}

	private static void MoveDisposalActorNearTarget(NCreature creatureNode, Creature? target)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (target == null)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(target) : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			if (!DisposalOriginalGlobalPositions.ContainsKey(creatureKey))
			{
				DisposalOriginalGlobalPositions[creatureKey] = ((Control)creatureNode).GlobalPosition;
			}
			Vector2 val2 = DisposalOriginalGlobalPositions[creatureKey];
			Vector2 creatureCenterGlobalPosition = GetCreatureCenterGlobalPosition(val);
			bool flag = creatureCenterGlobalPosition.X < val2.X;
			ApplyDisposalVisualMirror(creatureNode, creatureKey, flag);
			float num = (flag ? 595.075f : (-595.075f));
			((Control)creatureNode).GlobalPosition = new Vector2(creatureCenterGlobalPosition.X + num, val2.Y);
		}
	}

	private static void RestoreDisposalActorPosition(NCreature creatureNode)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			RestoreDisposalVisualMirror(creatureNode);
			if (DisposalOriginalGlobalPositions.Remove(creatureKey, out var value))
			{
				((Control)creatureNode).GlobalPosition = value;
			}
		}
	}

	private static void ApplyDisposalVisualMirror(NCreature creatureNode, ulong creatureKey, bool mirror)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals != null && GodotObject.IsInstanceValid((GodotObject)(object)visuals))
		{
			if (!DisposalOriginalVisualScales.ContainsKey(creatureKey))
			{
				DisposalOriginalVisualScales[creatureKey] = visuals.Scale;
			}
			Vector2 val = DisposalOriginalVisualScales[creatureKey];
			visuals.Scale = (Vector2)(mirror ? new Vector2(0f - val.X, val.Y) : val);
		}
	}

	private static void RestoreDisposalVisualMirror(NCreature? creatureNode)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (DisposalOriginalVisualScales.Remove(creatureKey, out var value))
		{
			Node2D visuals = (Node2D)(object)creatureNode.Visuals;
			if (visuals != null && GodotObject.IsInstanceValid((GodotObject)(object)visuals))
			{
				visuals.Scale = value;
			}
		}
	}

	private static void StartDisposalCinematic(NCreature creatureNode, Creature? target)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		EndDisposalCamera(creatureKey);
		ResetDisposalUiHide(creatureKey, restoreImmediately: true);
		StartDisposalUiHide(creatureNode);
		if (target == null)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(target) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return;
		}
		Viewport viewport = ((Node)creatureNode).GetViewport();
		Camera2D val2 = ((viewport != null) ? viewport.GetCamera2D() : null);
		if (viewport != null && val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
		{
			Camera2D val3 = new Camera2D
			{
				Name = StringName.op_Implicit("ValencinaDisposalCinematicCamera"),
				Enabled = true,
				Zoom = new Vector2(1.18f, 1.18f)
			};
			object obj = NCombatRoom.Instance;
			if (obj == null)
			{
				SceneTree tree = ((Node)creatureNode).GetTree();
				obj = ((object)((tree != null) ? tree.CurrentScene : null)) ?? ((object)viewport);
			}
			((Node)obj).AddChild((Node)(object)val3, false, (InternalMode)0);
			Vector2 creatureCenterGlobalPosition = GetCreatureCenterGlobalPosition(val);
			float num = (((Control)creatureNode).GlobalPosition.X + creatureCenterGlobalPosition.X) * 0.5f;
			((Node2D)val3).GlobalPosition = new Vector2(num, ((Node2D)val2).GlobalPosition.Y);
			DisposalPreviousCameras[creatureKey] = val2;
			DisposalCinematicCameras[creatureKey] = val3;
			val3.MakeCurrent();
		}
	}

	private static void EndDisposalCinematic(NCreature creatureNode)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			EndDisposalCamera(creatureKey);
			EndDisposalUiHide(creatureKey);
		}
	}

	private static void EndDisposalCamera(ulong creatureKey)
	{
		if (DisposalPreviousCameras.Remove(creatureKey, out Camera2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			value.MakeCurrent();
		}
		if (DisposalCinematicCameras.Remove(creatureKey, out Camera2D value2) && GodotObject.IsInstanceValid((GodotObject)(object)value2))
		{
			((Node)value2).QueueFree();
		}
	}

	private static void StartDisposalUiHide(NCreature creatureNode)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			ulong creatureKey = GetCreatureKey(creatureNode.Entity);
			Dictionary<CanvasItem, Color> dictionary = CollectDisposalUiRoots();
			if (dictionary.Count > 0)
			{
				DisposalHiddenUiItems[creatureKey] = dictionary;
				FadeUiItems((Node)(object)creatureNode, creatureKey, dictionary, fadeOut: true, 0.34f);
			}
		}
	}

	private static void EndDisposalUiHide(ulong creatureKey)
	{
		ResetDisposalUiHide(creatureKey, restoreImmediately: false);
	}

	private static void ResetDisposalUiHide(ulong creatureKey, bool restoreImmediately)
	{
		KillDisposalUiFade(creatureKey);
		if (DisposalHiddenUiItems.Remove(creatureKey, out Dictionary<CanvasItem, Color> value))
		{
			Node val = (Node)(((object)NCombatRoom.Instance) ?? ((object)((IEnumerable<CanvasItem>)value.Keys).FirstOrDefault((Func<CanvasItem, bool>)((CanvasItem item) => GodotObject.IsInstanceValid((GodotObject)(object)item)))));
			if (restoreImmediately || val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				RestoreUiItems(value);
			}
			else
			{
				FadeUiItems(val, creatureKey, value, fadeOut: false, 0.42f);
			}
		}
	}

	private static Dictionary<CanvasItem, Color> CollectDisposalUiRoots()
	{
		Dictionary<CanvasItem, Color> dictionary = new Dictionary<CanvasItem, Color>();
		NCombatRoom instance = NCombatRoom.Instance;
		AddDisposalUiRoot((CanvasItem?)(object)((instance != null) ? instance.Ui : null), dictionary);
		AddDisposalUiRoot((CanvasItem?)(object)NCardPlayQueue.Instance, dictionary);
		Node instance2 = (Node)(object)NCombatRoom.Instance;
		if (instance2 != null)
		{
			foreach (NCreature item in FindCreatureNodes(instance2))
			{
				AddDisposalUiRoot(((Node)item).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("%HealthBar")), dictionary);
				CanvasItem intentContainer = (CanvasItem)(object)item.IntentContainer;
				if (intentContainer != null)
				{
					AddDisposalUiRoot(intentContainer, dictionary);
				}
			}
		}
		return dictionary;
	}

	private static IEnumerable<NCreature> FindCreatureNodes(Node root)
	{
		foreach (Node child in root.GetChildren(false))
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)child))
			{
				continue;
			}
			NCreature val = (NCreature)(object)((child is NCreature) ? child : null);
			if (val != null)
			{
				yield return val;
			}
			foreach (NCreature item in FindCreatureNodes(child))
			{
				yield return item;
			}
		}
	}

	private static void AddDisposalUiRoot(CanvasItem? item, Dictionary<CanvasItem, Color> originals)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || !GodotObject.IsInstanceValid((GodotObject)(object)item) || originals.ContainsKey(item))
		{
			return;
		}
		foreach (CanvasItem item2 in originals.Keys.ToList())
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item2))
			{
				if (IsAncestor((Node)(object)item2, (Node)(object)item))
				{
					return;
				}
				if (IsAncestor((Node)(object)item, (Node)(object)item2))
				{
					originals.Remove(item2);
				}
			}
		}
		originals[item] = item.Modulate;
	}

	private static bool IsAncestor(Node maybeAncestor, Node node)
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent == maybeAncestor)
			{
				return true;
			}
		}
		return false;
	}

	private static void FadeUiItems(Node tweenOwner, ulong creatureKey, Dictionary<CanvasItem, Color> originals, bool fadeOut, float seconds)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		KillDisposalUiFade(creatureKey);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)tweenOwner))
		{
			return;
		}
		Tween val = tweenOwner.CreateTween().SetParallel(true);
		DisposalUiFadeTweens[creatureKey] = val;
		foreach (var (val4, val5) in originals)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val4))
			{
				Color val6 = val5;
				if (fadeOut)
				{
					val6.A = Math.Min(val5.A, 0.12f);
				}
				val.TweenProperty((GodotObject)(object)val4, NodePath.op_Implicit("modulate"), Variant.op_Implicit(val6), (double)seconds).SetEase((EaseType)2).SetTrans((TransitionType)1);
			}
		}
		ClearUiFadeTweenLater(creatureKey, val, seconds + 0.05f);
	}

	private static async Task ClearUiFadeTweenLater(ulong creatureKey, Tween tween, double seconds)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			int visualGeneration = CaptureVisualQuarantineGeneration();
			SceneTreeTimer val2 = val.CreateTimer(seconds, true, false, false);
			await ((GodotObject)val).ToSignal((GodotObject)(object)val2, SignalName.Timeout);
			Tween value;
			if (HasVisualQuarantineAdvanced(visualGeneration))
			{
				DisposalUiFadeTweens.Remove(creatureKey);
			}
			else if (DisposalUiFadeTweens.TryGetValue(creatureKey, out value) && value == tween)
			{
				DisposalUiFadeTweens.Remove(creatureKey);
			}
		}
	}

	private static void KillDisposalUiFade(ulong creatureKey)
	{
		if (DisposalUiFadeTweens.TryGetValue(creatureKey, out Tween value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			value.Kill();
		}
		DisposalUiFadeTweens.Remove(creatureKey);
	}

	private static void RestoreUiItems(Dictionary<CanvasItem, Color> originals)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		foreach (var (val3, modulate) in originals)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val3))
			{
				val3.Modulate = modulate;
			}
		}
	}

	private static Vector2 GetCreatureCenterGlobalPosition(NCreature creatureNode)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Node2D nodeOrNull = ((Node)creatureNode).GetNodeOrNull<Node2D>(NodePath.op_Implicit("CenterPos"));
		if (nodeOrNull != null)
		{
			return nodeOrNull.GlobalPosition;
		}
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			Node2D nodeOrNull2 = visuals.GetNodeOrNull<Node2D>(NodePath.op_Implicit("CenterPos"));
			if (nodeOrNull2 != null)
			{
				return nodeOrNull2.GlobalPosition;
			}
			Node2D nodeOrNull3 = visuals.GetNodeOrNull<Node2D>(NodePath.op_Implicit("Visuals/CenterPos"));
			if (nodeOrNull3 != null)
			{
				return nodeOrNull3.GlobalPosition;
			}
		}
		return ((Control)creatureNode).GlobalPosition;
	}

	private static void KillDodgeTween(ulong creatureKey, bool resetPosition)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (DodgeTweens.TryGetValue(creatureKey, out DodgeTweenState value))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Tween))
			{
				value.Tween.Kill();
			}
			NCreature val = value.TryGetCreatureNode();
			if (val != null)
			{
				RemoveOverlayFrameFromCreature(val, "PrecognitionDodgeMissFrame");
				RemoveOverlayFrameFromCreature(val, "ValencinaDamageFrame");
				if (resetPosition)
				{
					Node2D visuals = (Node2D)(object)val.Visuals;
					if (visuals != null)
					{
						visuals.Position = GetBaseVisualPosition(val);
					}
				}
			}
		}
		DodgeTweens.Remove(creatureKey);
	}

	private static bool IsCurrentDodgeTween(ulong creatureKey, Tween tween)
	{
		if (DodgeTweens.TryGetValue(creatureKey, out DodgeTweenState value))
		{
			return value.Tween == tween;
		}
		return false;
	}

	private static void CompleteDodgeTween(ulong creatureKey, NCreature creatureNode)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		DodgeTweens.Remove(creatureKey);
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			RemoveOverlayFrameFromCreature(creatureNode, "PrecognitionDodgeMissFrame");
			Node2D visuals = (Node2D)(object)creatureNode.Visuals;
			if (visuals != null)
			{
				visuals.Position = GetBaseVisualPosition(creatureNode);
			}
		}
	}

	private static void KillDeathTween(ulong creatureKey)
	{
		if (DeathTweens.TryGetValue(creatureKey, out Tween value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			value.Kill();
		}
		DeathTweens.Remove(creatureKey);
	}

	private static void KillDodgeTween(ulong creatureKey, NCreature creatureNode, bool restoreIdle)
	{
		KillDodgeTween(creatureKey, resetPosition: true);
		if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			RemoveOverlayFrameFromCreature(creatureNode, "PrecognitionDodgeMissFrame");
			if (restoreIdle && creatureNode.Entity.CurrentHp > 0)
			{
				SetIdleVisible(creatureNode, visible: true);
			}
		}
	}

	private static NCreature? ConsumeDisposalShakeTarget(NCreature creatureNode)
	{
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		if (!ActiveDisposalTargets.Remove(creatureKey, out Creature value))
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(value) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return null;
		}
		return val;
	}

	private static void PlayDisposalFinalShake(NCreature creatureNode)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		NCreatureVisuals visuals = creatureNode.Visuals;
		Node2D visuals2 = (Node2D)(object)visuals;
		if (visuals2 == null)
		{
			return;
		}
		ulong creatureKey = GetCreatureKey(creatureNode.Entity);
		KillDisposalFinalShake(creatureKey, creatureNode);
		Vector2 basePosition = visuals2.Position;
		DisposalFinalShakeBasePositions[creatureKey] = basePosition;
		visuals2.Position = basePosition;
		Tween val = ((Node)creatureNode).CreateTween();
		DisposalFinalShakeTweens[creatureKey] = val;
		val.TweenProperty((GodotObject)(object)visuals2, NodePath.op_Implicit("position"), Variant.op_Implicit(basePosition + new Vector2(22f, 0f)), 0.03500000014901161);
		val.TweenProperty((GodotObject)(object)visuals2, NodePath.op_Implicit("position"), Variant.op_Implicit(basePosition + new Vector2(-18f, 0f)), 0.04500000178813934);
		val.TweenProperty((GodotObject)(object)visuals2, NodePath.op_Implicit("position"), Variant.op_Implicit(basePosition + new Vector2(10f, 0f)), 0.03999999910593033);
		val.TweenProperty((GodotObject)(object)visuals2, NodePath.op_Implicit("position"), Variant.op_Implicit(basePosition), 0.04500000178813934);
		val.TweenCallback(Callable.From((Action)delegate
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			DisposalFinalShakeTweens.Remove(creatureKey);
			DisposalFinalShakeBasePositions.Remove(creatureKey);
			if (GodotObject.IsInstanceValid((GodotObject)(object)visuals2))
			{
				visuals2.Position = basePosition;
			}
		}));
	}

	private static void KillDisposalFinalShake(ulong creatureKey, NCreature creatureNode)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (DisposalFinalShakeTweens.TryGetValue(creatureKey, out Tween value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			value.Kill();
		}
		DisposalFinalShakeTweens.Remove(creatureKey);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		Node2D visuals = (Node2D)(object)creatureNode.Visuals;
		if (visuals != null)
		{
			if (DisposalFinalShakeBasePositions.Remove(creatureKey, out var value2))
			{
				visuals.Position = value2;
			}
			else
			{
				visuals.Position = GetBaseVisualPosition(creatureNode);
			}
		}
	}

	private static void KillDisposalFinalShake(ulong creatureKey)
	{
		if (DisposalFinalShakeTweens.TryGetValue(creatureKey, out Tween value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			value.Kill();
		}
		DisposalFinalShakeTweens.Remove(creatureKey);
		DisposalFinalShakeBasePositions.Remove(creatureKey);
	}

	private static void PlayAttack2SceneSfx(Node anchor, int hitCount = 2, float volumeMult = 1f)
	{
		ValencinaLocalSfx.PlayAttack2Start(anchor, volumeMult);
		PlayDelayedLocalSfx(anchor, 0.145, delegate(Node node)
		{
			ValencinaLocalSfx.PlayAttack2HitOne(node, volumeMult);
		});
		PlayDelayedLocalSfx(anchor, 0.235, delegate(Node node)
		{
			ValencinaLocalSfx.PlayAttack2HitTwo(node, volumeMult);
		});
	}

	private static async Task PlayDelayedLocalSfx(Node anchor, double seconds, Action<Node> play)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return;
		}
		SceneTree tree = anchor.GetTree();
		if (tree != null)
		{
			int visualGeneration = CaptureVisualQuarantineGeneration();
			SceneTreeTimer val = tree.CreateTimer(seconds, true, false, false);
			await ((GodotObject)anchor).ToSignal((GodotObject)(object)val, SignalName.Timeout);
			if (!ShouldAbortVisualContinuation(visualGeneration) && GodotObject.IsInstanceValid((GodotObject)(object)anchor))
			{
				play(anchor);
			}
		}
	}

	private static float ResolveDodgeDirection(NCreature creatureNode, Creature? attacker)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (attacker != null)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(attacker) : null);
			if (val != null)
			{
				float value = ((Control)creatureNode).GlobalPosition.X - ((Control)val).GlobalPosition.X;
				if (Math.Abs(value) > 0.01f)
				{
					return Math.Sign(value);
				}
			}
		}
		if (!(((Control)creatureNode).Scale.X < 0f))
		{
			return -1f;
		}
		return 1f;
	}

	private static Sprite2D? CreateMissOverlayFrame()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return CreateOverlayFrame("res://Valencina/images/charui/miss.png", "PrecognitionDodgeMissFrame", MissFramePosition, MissFrameScale);
	}

	private static Sprite2D? CreateOverlayFrame(string texturePath, string frameName, Vector2 position, Vector2 scale, int zIndex = 0)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		Texture2D val = ResourceLoader.Load<Texture2D>(texturePath, (string)null, (CacheMode)1);
		if (val == null)
		{
			MainFile.Logger.Warn("[ValencinaAnimation] failed to load overlay texture: " + texturePath, 1);
			return null;
		}
		Sprite2D val2 = new Sprite2D
		{
			Name = StringName.op_Implicit(frameName),
			Texture = val,
			Position = position,
			Scale = scale,
			FlipH = true,
			ZIndex = zIndex,
			ZAsRelative = true,
			ShowBehindParent = false,
			Modulate = Colors.White
		};
		((GodotObject)val2).SetMeta(StringName.op_Implicit("ValencinaRuntimeOverlay"), Variant.op_Implicit(true));
		return val2;
	}

	private static SemaphoreSlim GetAttackAnimationGate(ulong creatureKey)
	{
		lock (AttackAnimationGates)
		{
			if (!AttackAnimationGates.TryGetValue(creatureKey, out SemaphoreSlim value))
			{
				value = new SemaphoreSlim(1, 1);
				AttackAnimationGates[creatureKey] = value;
			}
			return value;
		}
	}

	private static ulong GetCreatureKey(Creature creature)
	{
		uint hashCode = (uint)RuntimeHelpers.GetHashCode(creature);
		return ((ulong)(uint)(creature.CombatId.HasValue ? ((int)creature.CombatId.Value) : (-1)) << 32) ^ hashCode;
	}

	private static void ClearDeathState(ulong creatureKey)
	{
		DeathSfxPlayed.Remove(creatureKey);
		DeathAnimationsStarted.Remove(creatureKey);
		KillDeathTween(creatureKey);
		if (BusyAnimations.TryGetValue(creatureKey, out string value) && string.Equals(value, "death", StringComparison.Ordinal))
		{
			BusyAnimations.Remove(creatureKey);
		}
	}

	private static void Cleanup(ulong playerId, ulong creatureKey)
	{
		ConnectedPlayers.Remove(playerId);
		BusyAnimations.Remove(creatureKey);
		SuppressUntilTicks.Remove(creatureKey);
		PostDisposalAttackSuppressUntilTicks.Remove(creatureKey);
		QueuedAttackHitCounts.Remove(creatureKey);
		QueuedAttackSuppressFollowups.Remove(creatureKey);
		QueuedAttackTriggerLockMs.Remove(creatureKey);
		SuppressedAttackFollowupCounts.Remove(creatureKey);
		QueuedDisposalAttacks.Remove(creatureKey);
		QueuedDisposalTargets.Remove(creatureKey);
		ActiveDisposalTargets.Remove(creatureKey);
		DisposalOriginalGlobalPositions.Remove(creatureKey);
		DisposalOriginalVisualScales.Remove(creatureKey);
		DisposalFinalShakeBasePositions.Remove(creatureKey);
		KillDodgeTween(creatureKey, resetPosition: true);
		KillDeathTween(creatureKey);
		KillDisposalFinalShake(creatureKey);
		AttackAnimationGates.Remove(creatureKey);
		EndDisposalCamera(creatureKey);
		ResetDisposalUiHide(creatureKey, restoreImmediately: true);
		BaseVisualPositions.Remove(creatureKey);
		DeathSfxPlayed.Remove(creatureKey);
		DeathAnimationsStarted.Remove(creatureKey);
	}
}
