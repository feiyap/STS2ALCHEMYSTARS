using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EventModel), "GetAssetPaths")]
internal static class VagrantEventAssetPathPatch
{
	private const string BadInferredPortraitPath = "res://images/events/valencina_event_vagrant_event.png";

	private const string BadInferredKaiserPhaseChoicePortraitPath = "res://images/events/valencina_event_cockroach_emperor_passive_disable_event.png";

	private const string BadInferredWarAmbushReturnPortraitPath = "res://images/events/valencina_event_war_ambush_return_event.png";

	private static void Postfix(EventModel __instance, ref IEnumerable<string> __result)
	{
		if (__instance is VagrantEvent)
		{
			__result = __result.Where((string path) => !string.Equals(path, "res://images/events/valencina_event_vagrant_event.png", StringComparison.Ordinal)).Append("res://Valencina/images/events/vagrant.png").Distinct<string>(StringComparer.Ordinal);
		}
		else if (__instance is CockroachEmperorPassiveDisableEvent)
		{
			__result = __result.Where((string path) => !string.Equals(path, "res://images/events/valencina_event_cockroach_emperor_passive_disable_event.png", StringComparison.Ordinal)).Append("res://Valencina/images/events/cockroach_emperor_phase_choice_background.png").Append("res://scenes/events/background_scenes/cockroach_emperor_phase_choice.tscn")
				.Distinct<string>(StringComparer.Ordinal);
		}
		else if (__instance is WarAmbushReturnEvent)
		{
			__result = __result.Where((string path) => !string.Equals(path, "res://images/events/valencina_event_war_ambush_return_event.png", StringComparison.Ordinal)).Append("res://Valencina/images/events/lucio_choice_background.png").Append("res://scenes/events/background_scenes/lucio_choice.tscn")
				.Distinct<string>(StringComparer.Ordinal);
		}
	}
}
