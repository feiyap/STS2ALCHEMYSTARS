using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Relics.Rien;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(AncientEventModel), "Done")]
internal static class RienSecondAncientDonePatch
{
	private sealed class FollowUpPage
	{
		private readonly IReadOnlyList<Func<AncientEventModel, EventOption>> _optionFactories;

		internal string EventEntry { get; }

		internal string InitialDescriptionKey => EventEntry + ".pages.INITIAL.description";

		internal FollowUpPage(string eventEntry, IReadOnlyList<Func<AncientEventModel, EventOption>> optionFactories)
		{
			EventEntry = eventEntry;
			_optionFactories = optionFactories;
		}

		internal List<EventOption> CreateOptions(AncientEventModel hostEvent)
		{
			List<EventOption> list = new List<EventOption>(_optionFactories.Count);
			foreach (Func<AncientEventModel, EventOption> optionFactory in _optionFactories)
			{
				list.Add(optionFactory(hostEvent));
			}
			return list;
		}
	}

	private static readonly MethodInfo? SetEventStateMethod = AccessTools.Method(typeof(EventModel), "SetEventState", (Type[])null, (Type[])null);

	private static readonly MethodInfo? SetEventFinishedMethod = AccessTools.Method(typeof(EventModel), "SetEventFinished", (Type[])null, (Type[])null);

	private static readonly MethodInfo? UpdateRunHistoryMethod = AccessTools.Method(typeof(AncientEventModel), "UpdateRunHistory", (Type[])null, (Type[])null);

	private static bool Prefix(AncientEventModel __instance)
	{
		try
		{
			if (!ShouldStartFollowUp(__instance))
			{
				return true;
			}
			if (!RienSecondAncientState.TryMarkTriggered(((EventModel)__instance).Owner))
			{
				return true;
			}
			StartFollowUpPage(__instance);
			return false;
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[RienSecondAncient] Failed to show in-event follow-up Ancient page; falling back to original Ancient Done. {value}", 1);
			return true;
		}
	}

	private static bool ShouldStartFollowUp(AncientEventModel currentEvent)
	{
		if (!ValencinaModConfig.EnableRienFollowUpAncient)
		{
			return false;
		}
		if (((EventModel)currentEvent).Owner == null)
		{
			return false;
		}
		if (!(((EventModel)currentEvent).Owner.Character is Valencina.ValencinaCode.Character.Valencina))
		{
			return false;
		}
		IRunState runState = ((EventModel)currentEvent).Owner.RunState;
		if (runState.CurrentActIndex >= 3 || UngezieferKaiserFinalBossController.IsValencinaAct4(runState.Act))
		{
			return false;
		}
		if (((EventModel)currentEvent).IsFinished)
		{
			return false;
		}
		if ((currentEvent is ThumbAdvisor || currentEvent is LimbusCompanyHeadquarters || currentEvent is Rien) ? true : false)
		{
			return false;
		}
		return true;
	}

	private static void StartFollowUpPage(AncientEventModel hostEvent)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		if (SetEventStateMethod == null || UpdateRunHistoryMethod == null)
		{
			throw new MissingMethodException("STS2 Ancient event state methods changed.");
		}
		Player owner = ((EventModel)hostEvent).Owner;
		int currentActIndex = owner.RunState.CurrentActIndex;
		FollowUpPage followUpPage = GetFollowUpPage(owner, currentActIndex);
		RienFollowUpAncientVisualState.Set(hostEvent, followUpPage.EventEntry);
		UpdateRunHistoryMethod.Invoke(hostEvent, Array.Empty<object>());
		List<EventOption> list = followUpPage.CreateOptions(hostEvent);
		LocString val = new LocString("ancients", followUpPage.InitialDescriptionKey);
		SetEventStateMethod.Invoke(hostEvent, new object[2] { val, list });
		MainFile.Logger.Info($"[RienSecondAncient] Showing {followUpPage.EventEntry} as an in-event follow-up Ancient page after {((AbstractModel)hostEvent).Id.Entry}.", 1);
	}

	private static FollowUpPage GetFollowUpPage(Player owner, int actIndex)
	{
		string eventEntry = actIndex switch
		{
			0 => "THUMB_ADVISOR", 
			1 => "LIMBUS_COMPANY_HEADQUARTERS", 
			_ => "RIEN", 
		};
		List<Func<AncientEventModel, EventOption>> list = new List<Func<AncientEventModel, EventOption>>();
		if (ValencinaWarDifficulty.IsActive(owner.RunState) && actIndex >= 0 && actIndex <= 2)
		{
			switch (actIndex)
			{
			case 0:
				list.Add((AncientEventModel host) => RelicOption<Maggot>(host, eventEntry));
				break;
			case 1:
				list.Add((AncientEventModel host) => RelicOption<Fly>(host, eventEntry));
				break;
			default:
				list.Add((AncientEventModel host) => RelicOption<Moth>(host, eventEntry));
				break;
			}
			return new FollowUpPage(eventEntry, list);
		}
		foreach (ExtraAncientPoolEntry item in ValencinaExtraAncientRelicPools.DrawOptions(owner, actIndex))
		{
			ExtraAncientPoolEntry captured = item;
			list.Add((AncientEventModel host) => RelicOptionFromCanonical(captured.CreateCanonical(), host, eventEntry));
		}
		switch (actIndex)
		{
		case 0:
			AddKaiserRelicOption<Maggot>(list, eventEntry);
			break;
		case 1:
			AddKaiserRelicOption<Moth>(list, eventEntry);
			break;
		default:
			AddKaiserRelicOption<Fly>(list, eventEntry);
			break;
		}
		return new FollowUpPage(eventEntry, list);
	}

	private static void AddKaiserRelicOption<T>(List<Func<AncientEventModel, EventOption>> options, string eventEntry) where T : RelicModel
	{
		if (ValencinaModConfig.EnableKaiserContent)
		{
			options.Add((AncientEventModel host) => RelicOption<T>(host, eventEntry));
		}
	}

	private static EventOption RelicOption<T>(AncientEventModel hostEvent, string eventEntry) where T : RelicModel
	{
		return RelicOptionFromCanonical((RelicModel)(object)ModelDb.Relic<T>(), hostEvent, eventEntry);
	}

	private static EventOption RelicOptionFromCanonical(RelicModel canonicalRelic, AncientEventModel hostEvent, string eventEntry)
	{
		Player owner = ((EventModel)hostEvent).Owner;
		RelicModel relic = canonicalRelic.ToMutable();
		relic.Owner = owner;
		string text = eventEntry + ".pages.INITIAL.options." + ((AbstractModel)relic).Id.Entry;
		return EventOption.FromRelic(relic, (EventModel)(object)hostEvent, (Func<Task>)async delegate
		{
			if (!ValencinaWarDifficulty.IsActive(owner.RunState) && !ValencinaModConfig.EnableKaiserContent && ValencinaSpecialAncientPoolGuard.IsKaiserSummonRelicType(((object)relic).GetType()))
			{
				FinishHostEvent(hostEvent, eventEntry);
			}
			else
			{
				await RelicCmd.Obtain(relic, owner, -1);
				FinishHostEvent(hostEvent, eventEntry);
			}
		}, text);
	}

	private static void FinishHostEvent(AncientEventModel hostEvent, string eventEntry)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (SetEventFinishedMethod == null)
		{
			throw new MissingMethodException("STS2 event finish method changed.");
		}
		LocString val = new LocString("ancients", eventEntry + ".pages.DONE.description");
		SetEventFinishedMethod.Invoke(hostEvent, new object[1] { val });
	}
}
