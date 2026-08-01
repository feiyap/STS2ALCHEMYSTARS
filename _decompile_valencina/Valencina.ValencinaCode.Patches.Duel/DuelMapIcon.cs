using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Systems.Duel;

namespace Valencina.ValencinaCode.Patches.Duel;

internal static class DuelMapIcon
{
	internal static void Apply(NNormalMapPoint mapPoint)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			RunManager instance = RunManager.Instance;
			if (!DuelNodeSystem.IsDuelPoint((IRunState?)(object)((instance != null) ? instance.DebugOnlyGetState() : null), ((NMapPoint)mapPoint).Point?.coord))
			{
				return;
			}
			TextureRect nodeOrNull = ((Node)mapPoint).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
			TextureRect nodeOrNull2 = ((Node)mapPoint).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Outline"));
			Texture2D val = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/map/duel_node.svg", (string)null, (CacheMode)1);
			if (val != null)
			{
				if (nodeOrNull != null)
				{
					nodeOrNull.Texture = val;
				}
				if (nodeOrNull2 != null)
				{
					nodeOrNull2.Texture = val;
				}
			}
		}
		catch
		{
		}
	}
}
