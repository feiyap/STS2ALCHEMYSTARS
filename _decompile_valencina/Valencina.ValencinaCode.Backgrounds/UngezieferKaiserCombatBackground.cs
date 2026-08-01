using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Valencina.ValencinaCode.Backgrounds;

[ScriptPath("res://ValencinaCode/Backgrounds/UngezieferKaiserCombatBackground.cs")]
public class UngezieferKaiserCombatBackground : NCombatBackground
{
	public class MethodName : MethodName
	{
	}

	public class PropertyName : PropertyName
	{
	}

	public class SignalName : SignalName
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		((NCombatBackground)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NCombatBackground)this).RestoreGodotObjectData(info);
	}
}
