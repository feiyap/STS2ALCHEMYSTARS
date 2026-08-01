using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaMultiplayerHandTexture
{
	public static bool TryResolve(CharacterModel character, string path, ref Texture2D result)
	{
		if (!(character is Valencina.ValencinaCode.Character.Valencina))
		{
			return true;
		}
		Texture2D val = TryLoadTexture(path);
		if (val == null)
		{
			return true;
		}
		result = val;
		return false;
	}

	public static void ApplyToHandImage(NHandImage handImage, RelicPickingFightMove? move = null)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected I4, but got Unknown
		Player player = handImage.Player;
		if (!(((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina))
		{
			return;
		}
		TextureRect val = (TextureRect)(((object)((Node)handImage).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("TextureRect"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
		if (val != null)
		{
			Texture2D val2 = TryLoadTexture(move switch
			{
				(RelicPickingFightMove)0L => "res://Valencina/images/ui/hands/multiplayer_hand_valencina_rock.png", 
				(RelicPickingFightMove)1L => "res://Valencina/images/ui/hands/multiplayer_hand_valencina_paper.png", 
				(RelicPickingFightMove)2L => "res://Valencina/images/ui/hands/multiplayer_hand_valencina_scissors.png", 
				_ => "res://Valencina/images/ui/hands/multiplayer_hand_valencina_point.png", 
			});
			if (val2 != null)
			{
				val.Texture = val2;
			}
		}
	}

	private static Texture2D? TryLoadTexture(string path)
	{
		try
		{
			return PreloadManager.Cache.GetTexture2D(path);
		}
		catch
		{
			try
			{
				return ResourceLoader.Load<Texture2D>(path, (string)null, (CacheMode)1);
			}
			catch
			{
				return null;
			}
		}
	}
}
