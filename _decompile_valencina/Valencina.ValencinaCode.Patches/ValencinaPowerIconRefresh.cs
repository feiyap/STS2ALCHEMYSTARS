using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaPowerIconRefresh
{
	public static void Apply(NPower node)
	{
		ValencinaPower valencinaPower;
		try
		{
			valencinaPower = node.Model as ValencinaPower;
		}
		catch
		{
			return;
		}
		if (valencinaPower != null)
		{
			RefreshTexture(((Node)node).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon")), ((ModPowerTemplate)valencinaPower).CustomIconPath);
			RefreshTexture(((Node)node).GetNodeOrNull<CpuParticles2D>(NodePath.op_Implicit("%PowerFlash")), ((ModPowerTemplate)valencinaPower).CustomBigIconPath);
		}
	}

	private static void RefreshTexture(TextureRect? textureRect, string? path)
	{
		if (textureRect != null && GodotObject.IsInstanceValid((GodotObject)(object)textureRect) && !string.IsNullOrWhiteSpace(path))
		{
			Texture2D val = ResourceLoader.Load<Texture2D>(path, string.Empty, (CacheMode)1);
			if (val != null)
			{
				textureRect.Texture = val;
			}
		}
	}

	private static void RefreshTexture(CpuParticles2D? particles, string? path)
	{
		if (particles != null && GodotObject.IsInstanceValid((GodotObject)(object)particles) && !string.IsNullOrWhiteSpace(path))
		{
			Texture2D val = ResourceLoader.Load<Texture2D>(path, string.Empty, (CacheMode)1);
			if (val != null)
			{
				particles.Texture = val;
			}
		}
	}
}
