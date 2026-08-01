using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Patches;

internal static class MagicBeeperQuestMarker
{
	private const string MarkerPath = "res://Valencina/images/ui/map/magic_beeper_quest_marker.png";

	public static void Apply(NNormalMapPoint mapPoint)
	{
		try
		{
			MapPoint point = ((NMapPoint)mapPoint).Point;
			if (((point != null) ? point.Quests : null) == null)
			{
				return;
			}
			bool flag = false;
			foreach (AbstractModel quest in ((NMapPoint)mapPoint).Point.Quests)
			{
				if (quest is MagicBeeper)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			TextureRect nodeOrNull = ((Node)mapPoint).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%QuestIcon"));
			if (nodeOrNull != null)
			{
				Texture2D val = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/map/magic_beeper_quest_marker.png", (string)null, (CacheMode)1);
				if (val != null)
				{
					nodeOrNull.Texture = val;
				}
			}
		}
		catch
		{
		}
	}
}
