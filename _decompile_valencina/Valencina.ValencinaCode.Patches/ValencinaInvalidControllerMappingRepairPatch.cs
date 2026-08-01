using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NInputManager), "_UnhandledInput")]
internal static class ValencinaInvalidControllerMappingRepairPatch
{
	private static readonly FieldInfo ControllerInputMapField = AccessTools.Field(typeof(NInputManager), "_controllerInputMap");

	private static NInputManager? _repairedManager;

	private static bool _failureLogged;

	[HarmonyPrefix]
	private static void RepairInvalidMappings(NInputManager __instance)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (_repairedManager == __instance)
		{
			return;
		}
		try
		{
			if (!(ControllerInputMapField.GetValue(__instance) is Dictionary<StringName, StringName> { Count: not 0 } dictionary))
			{
				return;
			}
			Dictionary<StringName, StringName> getDefaultControllerInputMap = __instance.ControllerManager.GetDefaultControllerInputMap;
			List<string> list = new List<string>();
			KeyValuePair<StringName, StringName>[] array = dictionary.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<StringName, StringName> keyValuePair = array[i];
				var (val3, val4) = (KeyValuePair<StringName, StringName>)(ref keyValuePair);
				if (!InputMap.HasAction(val4))
				{
					if (getDefaultControllerInputMap.TryGetValue(val3, out var value) && value != null && InputMap.HasAction(value))
					{
						dictionary[val3] = value;
						list.Add($"{val3}: {val4} -> {value}");
					}
					else
					{
						dictionary.Remove(val3);
						list.Add($"{val3}: removed invalid action {val4}");
					}
				}
			}
			if (list.Count > 0)
			{
				SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
				settingsSave.ControllerMappingType = __instance.ControllerManager.ControllerMappingType;
				settingsSave.ControllerMapping = dictionary.ToDictionary((KeyValuePair<StringName, StringName> pair) => ((object)pair.Key).ToString(), (KeyValuePair<StringName, StringName> pair) => ((object)pair.Value).ToString());
				SaveManager.Instance.SaveSettings();
				MainFile.Logger.Warn($"[ControllerMappingRepair] Repaired {list.Count} obsolete controller binding(s): " + string.Join("; ", list), 1);
			}
			_repairedManager = __instance;
			_failureLogged = false;
		}
		catch (Exception ex)
		{
			if (!_failureLogged)
			{
				_failureLogged = true;
				MainFile.Logger.Warn("[ControllerMappingRepair] Deferred repair after failure: " + ex.Message, 1);
			}
		}
	}
}
