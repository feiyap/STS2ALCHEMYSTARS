using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaMerchantSceneGuard
{
	public static bool IsValencinaMerchant(NMerchantCharacter merchant)
	{
		if (((object)((Node)merchant).Name).ToString().Contains("ValencinaMerchant"))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(((Node)merchant).SceneFilePath) && ((Node)merchant).SceneFilePath.Contains("merchant_valencina"))
		{
			return true;
		}
		Sprite2D? obj = FindSprite((Node)(object)merchant);
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			Texture2D texture = obj.Texture;
			obj2 = ((texture != null) ? ((Resource)texture).ResourcePath : null);
		}
		string text = (string)obj2;
		if (!string.IsNullOrEmpty(text))
		{
			return text.Contains("Valencina/images/charui/idle_valencina.png");
		}
		return false;
	}

	private static Sprite2D? FindSprite(Node node)
	{
		Node nodeOrNull = node.GetNodeOrNull(NodePath.op_Implicit("Visuals"));
		Sprite2D val = (Sprite2D)(object)((nodeOrNull is Sprite2D) ? nodeOrNull : null);
		if (val != null)
		{
			return val;
		}
		Sprite2D val2 = (Sprite2D)(object)((node is Sprite2D) ? node : null);
		if (val2 != null)
		{
			return val2;
		}
		foreach (Node child in node.GetChildren(false))
		{
			Sprite2D val3 = FindSprite(child);
			if (val3 != null)
			{
				return val3;
			}
		}
		return null;
	}
}
