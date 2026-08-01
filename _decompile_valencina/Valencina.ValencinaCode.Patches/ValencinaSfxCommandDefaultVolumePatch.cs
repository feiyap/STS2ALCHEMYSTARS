using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaSfxCommandDefaultVolumePatch
{
	private const float CharacterSelectVolumeMultiplier = 1.45f;

	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(SfxCmd), "Play", new Type[1] { typeof(string) }, (Type[])null);
	}

	private static bool Prepare()
	{
		bool num = TargetMethod() != null;
		if (!num)
		{
			MainFile.Logger.Info("[ValencinaSfx] SfxCmd.Play(string) not found in current sts2.dll; character select sound fallback may be unavailable.", 1);
		}
		return num;
	}

	private static bool Prefix(string sfx)
	{
		if (sfx != "event:/mods/valencina/ui/char_select")
		{
			return true;
		}
		return ValencinaLocalSfx.PlayCharacterSelectOnce(1.45f) == null;
	}
}
