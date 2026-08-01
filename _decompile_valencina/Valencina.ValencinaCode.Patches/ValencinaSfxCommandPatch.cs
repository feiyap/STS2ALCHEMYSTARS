using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaSfxCommandPatch
{
	private const float CharacterSelectVolumeMultiplier = 1.45f;

	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(SfxCmd), "Play", new Type[2]
		{
			typeof(string),
			typeof(float)
		}, (Type[])null);
	}

	private static bool Prepare()
	{
		bool num = TargetMethod() != null;
		if (!num)
		{
			MainFile.Logger.Info("[ValencinaSfx] SfxCmd.Play(string, float) not found in current sts2.dll; character select sound will use the button fallback only.", 1);
		}
		return num;
	}

	private static bool Prefix(string sfx, float volume)
	{
		if (sfx != "event:/mods/valencina/ui/char_select")
		{
			return true;
		}
		return ValencinaLocalSfx.PlayCharacterSelectOnce(volume * 1.45f) == null;
	}
}
