using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaCharacterSelectUiSfxPatch
{
	private const float CharacterSelectVolumeMultiplier = 1.45f;

	private static IEnumerable<MethodBase> TargetMethods()
	{
		MethodBase methodBase = AccessTools.Method(typeof(NCharacterSelectScreen), "SelectCharacter", new Type[2]
		{
			typeof(NCharacterSelectButton),
			typeof(CharacterModel)
		}, (Type[])null);
		if (methodBase != null)
		{
			yield return methodBase;
		}
		MethodBase methodBase2 = AccessTools.Method(typeof(NCustomRunScreen), "SelectCharacter", new Type[2]
		{
			typeof(NCharacterSelectButton),
			typeof(CharacterModel)
		}, (Type[])null);
		if (methodBase2 != null)
		{
			yield return methodBase2;
		}
	}

	private static void Postfix(CharacterModel characterModel)
	{
		if (characterModel.CharacterSelectSfx == "event:/mods/valencina/ui/char_select")
		{
			ValencinaLocalSfx.PlayCharacterSelectOnce(1.45f);
		}
	}
}
