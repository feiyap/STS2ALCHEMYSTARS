using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Events;

public sealed class CockroachEmperorPassiveDisableEvent : EventModel, IModEventAssetOverrides
{
	public const string BackgroundTexturePath = "res://Valencina/images/events/cockroach_emperor_phase_choice_background.png";

	public const string BackgroundScenePath = "res://scenes/events/background_scenes/cockroach_emperor_phase_choice.tscn";

	private static readonly MethodInfo? ExitCurrentRoomMethod = typeof(RunManager).GetMethod("ExitCurrentRoom", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo? ResumePreviousRoomMethod = typeof(RunManager).GetMethod("ResumePreviousRoom", BindingFlags.Instance | BindingFlags.NonPublic);

	private Func<Task>? _disableSubjects;

	private Func<Task>? _disableBlood;

	private bool _choiceResolved;

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Monster<UngezieferKaiser>();

	public EventAssetProfile AssetProfile => new EventAssetProfile((string)null, "res://Valencina/images/events/cockroach_emperor_phase_choice_background.png", "res://scenes/events/background_scenes/cockroach_emperor_phase_choice.tscn", (string)null);

	public override bool IsDeterministic => false;

	public override bool IsAllowed(IRunState runState)
	{
		return false;
	}

	public void Configure(Func<Task> disableSubjects, Func<Task> disableBlood)
	{
		((AbstractModel)this).AssertMutable();
		_disableSubjects = disableSubjects;
		_disableBlood = disableBlood;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		string text = ((AbstractModel)this).Id.Entry + ".pages.INITIAL.options.";
		return new _003C_003Ez__ReadOnlyArray<EventOption>((EventOption[])(object)new EventOption[2]
		{
			CreateOption(ChooseSubjects, text + "SUBJECTS", CompatHoverTips.FromPower<KaiserCitizensPower>()),
			CreateOption(ChooseBlood, text + "BLOOD", CompatHoverTips.FromPower<KaiserBloodPower>())
		});
	}

	private EventOption CreateOption(Func<Task> onChosen, string key, params IHoverTip[] hoverTips)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		return new EventOption((EventModel)(object)this, onChosen, ((EventModel)this).L10NLookup(key + ".title"), ((EventModel)this).L10NLookup(key + ".description"), key, (IEnumerable<IHoverTip>)hoverTips);
	}

	private Task ChooseSubjects()
	{
		return ResolveChoice("SUBJECTS", _disableSubjects, (UngezieferKaiser kaiser) => kaiser.DisableEmperorSubjectsFromPhaseChoice((PlayerChoiceContext)new BlockingPlayerChoiceContext()), "Could not find active Kaiser while choosing to disable Emperor's Subjects.");
	}

	private Task ChooseBlood()
	{
		return ResolveChoice("BLOOD", _disableBlood, (UngezieferKaiser kaiser) => kaiser.DisableEmperorBloodFromPhaseChoice((PlayerChoiceContext)new BlockingPlayerChoiceContext()), "Could not find active Kaiser while choosing to disable Emperor's Blood.");
	}

	private async Task ResolveChoice(string pageKey, Func<Task>? configuredAction, Func<UngezieferKaiser, Task> fallbackAction, string missingKaiserMessage)
	{
		if (_choiceResolved)
		{
			return;
		}
		_choiceResolved = true;
		try
		{
			if (configuredAction != null)
			{
				await configuredAction();
				return;
			}
			UngezieferKaiser ungezieferKaiser = TryFindActiveKaiser();
			if (ungezieferKaiser != null)
			{
				await fallbackAction(ungezieferKaiser);
			}
			else
			{
				MainFile.Logger.Warn("[UngezieferKaiser] " + missingKaiserMessage + " This can happen after loading an old phase-choice save.", 1);
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Phase choice option '{pageKey}' failed; applying deterministic fallback. {value}", 1);
			UngezieferKaiser ungezieferKaiser2 = TryFindActiveKaiser();
			if (ungezieferKaiser2 != null)
			{
				await ungezieferKaiser2.ResolvePhaseChoiceFallbackFromEventError((PlayerChoiceContext)new BlockingPlayerChoiceContext(), "phase choice option '" + pageKey + "' failed");
			}
		}
		finally
		{
			await CleanupPhaseChoiceInputLocks();
			((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages." + pageKey + ".description"));
			TaskHelper.RunSafely(FastResumeCombatRoomDeferred());
		}
	}

	private static UngezieferKaiser? TryFindActiveKaiser()
	{
		CombatManager instance = CombatManager.Instance;
		CombatState obj = ((instance != null) ? instance.DebugOnlyGetState() : null);
		if (obj == null)
		{
			return null;
		}
		return obj.Creatures.Select((Creature creature) => creature.Monster).OfType<UngezieferKaiser>().FirstOrDefault((UngezieferKaiser kaiser) => ((MonsterModel)kaiser).Creature.IsAlive);
	}

	private static async Task CleanupPhaseChoiceInputLocks()
	{
		CombatManager instance = CombatManager.Instance;
		CombatState val = ((instance != null) ? instance.DebugOnlyGetState() : null);
		if (val == null)
		{
			return;
		}
		foreach (KaiserPhaseChoiceInputLockPower item in (from power in val.Players.OrderBy((Player player) => (((player != null) ? player.Creature : null) != null) ? StableCreatureKey(player.Creature) : string.Empty).Select(delegate(Player player)
			{
				if (player == null)
				{
					return (KaiserPhaseChoiceInputLockPower)null;
				}
				Creature creature = player.Creature;
				return (creature == null) ? null : creature.GetPower<KaiserPhaseChoiceInputLockPower>();
			})
			where power != null
			select power).ToList())
		{
			await PowerCmd.Remove((PowerModel)(object)item);
		}
	}

	private static string StableCreatureKey(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name ?? ((object)creature).GetHashCode().ToString("D10");
			}
		}
		return (string)obj;
	}

	private static async Task FastResumeCombatRoomDeferred()
	{
		await Task.Yield();
		if (!(await TryFastResumeCombatRoom()))
		{
			UngezieferKaiser? ungezieferKaiser = TryFindActiveKaiser();
			object obj;
			if (ungezieferKaiser == null)
			{
				obj = null;
			}
			else
			{
				ICombatState combatState = ((MonsterModel)ungezieferKaiser).CombatState;
				obj = ((combatState != null) ? combatState.RunState : null);
			}
			AbstractRoom obj2 = ((obj != null) ? ((IRunState)obj).CurrentRoom : null);
			EventRoom val = (EventRoom)(object)((obj2 is EventRoom) ? obj2 : null);
			if (val != null && val.CanonicalEvent is CockroachEmperorPassiveDisableEvent)
			{
				await ResumeCombatRoomWithVanillaFallback();
			}
			else
			{
				MainFile.Logger.Warn("[UngezieferKaiser] Skipped vanilla combat resume fallback because the phase-choice event was no longer the current room.", 1);
			}
		}
	}

	private static async Task<bool> TryFastResumeCombatRoom()
	{
		if (RunManager.Instance == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume skipped: RunManager.Instance was unavailable.", 1);
			return false;
		}
		UngezieferKaiser ungezieferKaiser = TryFindActiveKaiser();
		object obj;
		if (ungezieferKaiser == null)
		{
			obj = null;
		}
		else
		{
			ICombatState combatState = ((MonsterModel)ungezieferKaiser).CombatState;
			obj = ((combatState != null) ? combatState.RunState : null);
		}
		IRunState runState = (IRunState)obj;
		if (runState == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume skipped: active Kaiser run state was unavailable.", 1);
			return false;
		}
		AbstractRoom currentRoom = runState.CurrentRoom;
		EventRoom val = (EventRoom)(object)((currentRoom is EventRoom) ? currentRoom : null);
		if (val == null || !(val.CanonicalEvent is CockroachEmperorPassiveDisableEvent))
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume skipped: current room was " + (((object)runState.CurrentRoom)?.GetType().Name ?? "<none>") + ", not the phase-choice event.", 1);
			return false;
		}
		if (!(runState.BaseRoom is CombatRoom))
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume skipped: base room was " + (((object)runState.BaseRoom)?.GetType().Name ?? "<none>") + ", not CombatRoom.", 1);
			return false;
		}
		if (ExitCurrentRoomMethod == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume skipped: RunManager.ExitCurrentRoom was unavailable.", 1);
			return false;
		}
		AbstractRoom val2;
		try
		{
			if (!(ExitCurrentRoomMethod.Invoke(RunManager.Instance, null) is Task<AbstractRoom> task))
			{
				MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume skipped: RunManager.ExitCurrentRoom returned an unexpected task type.", 1);
				return false;
			}
			val2 = await task;
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Fast combat resume failed while exiting phase-choice event. {value}", 1);
			return false;
		}
		if (val2 == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume failed: exited room was null.", 1);
			return false;
		}
		AbstractRoom currentRoom2 = runState.CurrentRoom;
		CombatRoom val3 = (CombatRoom)(object)((currentRoom2 is CombatRoom) ? currentRoom2 : null);
		if (val3 == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Fast combat resume failed: previous room after event exit was " + (((object)runState.CurrentRoom)?.GetType().Name ?? "<none>") + ", not CombatRoom.", 1);
			return false;
		}
		try
		{
			await ((AbstractRoom)val3).Resume(val2, runState);
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				instance.RunMusicController.UpdateTrack();
			}
			ActiveScreenContext.Instance.Update();
			MainFile.Logger.Info("[UngezieferKaiser] Fast-resumed combat after phase choice without vanilla room fade.", 1);
			return true;
		}
		catch (Exception value2)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Fast combat resume failed while restoring combat room. {value2}", 1);
			return true;
		}
	}

	private static async Task ResumeCombatRoomWithVanillaFallback()
	{
		if (RunManager.Instance == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Could not resume combat after phase choice: RunManager.Instance was unavailable.", 1);
			return;
		}
		if (ResumePreviousRoomMethod == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Could not resume combat after phase choice: RunManager.ResumePreviousRoom was unavailable.", 1);
			return;
		}
		try
		{
			if (ResumePreviousRoomMethod.Invoke(RunManager.Instance, null) is Task task)
			{
				await task;
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to resume combat after phase choice. {value}", 1);
		}
	}
}
