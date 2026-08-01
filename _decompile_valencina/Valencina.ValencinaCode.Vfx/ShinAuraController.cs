using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Settings;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Vfx;

public static class ShinAuraController
{
	private const string AuraNodeName = "ShinAura";

	private const int MaxDeferredAttempts = 24;

	public static bool IsShinAuraPower(PowerModel? power)
	{
		if (power is ValencinaShinPower || power is ShinAmmoRefundPower)
		{
			return true;
		}
		return false;
	}

	public static bool HasShinAuraPower(Creature? owner)
	{
		if (((owner != null) ? owner.GetPower<ValencinaShinPower>() : null) == null)
		{
			return ((owner != null) ? owner.GetPower<ShinAmmoRefundPower>() : null) != null;
		}
		return true;
	}

	public static bool HasShinAuraPower(NCreature? creatureNode)
	{
		return HasShinAuraPower((creatureNode != null) ? creatureNode.Entity : null);
	}

	public static void Refresh(Creature? owner)
	{
		if (owner != null)
		{
			if (HasShinAuraPower(owner))
			{
				Show(owner);
			}
			else
			{
				Hide(owner);
			}
		}
	}

	public static void Refresh(NCreature? creatureNode)
	{
		if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			if (HasShinAuraPower(creatureNode))
			{
				Show(creatureNode);
			}
			else
			{
				Hide(creatureNode);
			}
		}
	}

	public static void Show(Creature? owner)
	{
		if (ValencinaModConfig.DisableShinAuraEffect)
		{
			Hide(owner);
		}
		else if (owner != null)
		{
			ShowOwnerInternal(owner, 0);
		}
	}

	public static void Show(NCreature? creatureNode)
	{
		if (ValencinaModConfig.DisableShinAuraEffect)
		{
			Hide(creatureNode);
		}
		else
		{
			ShowInternal(creatureNode, 0);
		}
	}

	public static void Hide(Creature? owner)
	{
		if (owner != null)
		{
			Hide(FindCreatureNode(owner));
		}
	}

	public static void Hide(NCreature? creatureNode)
	{
		if (SetAuraActive(FindAuraNode(creatureNode), active: false))
		{
			MainFile.Logger.Info("[ShinAura] Scene aura hidden.", 1);
		}
	}

	private static void ShowOwnerInternal(Creature owner, int attempt)
	{
		if (ValencinaModConfig.DisableShinAuraEffect)
		{
			Hide(owner);
			return;
		}
		NCreature val = FindCreatureNode(owner);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			RetryShow(owner, attempt, "creature node not ready");
		}
		else
		{
			ShowInternal(val, attempt);
		}
	}

	private static void ShowInternal(NCreature? creatureNode, int attempt)
	{
		if (ValencinaModConfig.DisableShinAuraEffect)
		{
			Hide(creatureNode);
		}
		else
		{
			if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				return;
			}
			if (!((Node)creatureNode).IsNodeReady() || creatureNode.Visuals == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode.Visuals) || !((Node)creatureNode.Visuals).IsNodeReady())
			{
				RetryShow(creatureNode, attempt, "creature visuals not ready");
				return;
			}
			Node val = FindAuraNode(creatureNode);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				if (SetAuraActive(val, active: true))
				{
					MainFile.Logger.Info($"[ShinAura] Scene aura shown at {val.GetPath()}.", 1);
				}
			}
			else
			{
				RetryShow(creatureNode, attempt, "scene aura node not found");
			}
		}
	}

	private static void RetryShow(Creature owner, int attempt, string reason)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (attempt >= 24)
		{
			Node val = TryCreateFallbackAuraNode(FindCreatureNode(owner), reason);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				SetAuraActive(val, active: true);
				return;
			}
			MainFile.Logger.Warn($"[ShinAura] Show failed after {attempt} deferred attempts: {reason}. Check NCombatRoom creature node creation and creature_visuals_valencina.tscn.", 1);
		}
		else
		{
			Callable val2 = Callable.From((Action)delegate
			{
				ShowOwnerInternal(owner, attempt + 1);
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private static void RetryShow(NCreature creatureNode, int attempt, string reason)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (attempt >= 24)
		{
			Node val = TryCreateFallbackAuraNode(creatureNode, reason);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				SetAuraActive(val, active: true);
				return;
			}
			MainFile.Logger.Warn($"[ShinAura] Show failed after {attempt} deferred attempts: {reason}. Check creature_visuals_valencina.tscn has %ShinAura under Visuals.", 1);
		}
		else
		{
			Callable val2 = Callable.From((Action)delegate
			{
				ShowInternal(creatureNode, attempt + 1);
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private static Node? TryCreateFallbackAuraNode(NCreature? creatureNode, string reason)
	{
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return null;
		}
		Node visuals = (Node)(object)creatureNode.Visuals;
		if (visuals == null || !GodotObject.IsInstanceValid((GodotObject)(object)visuals))
		{
			return null;
		}
		Node val = FindByUniqueOrName(visuals, "ShinAura");
		if (val != null)
		{
			return val;
		}
		try
		{
			ShinAuraSceneNode shinAuraSceneNode = new ShinAuraSceneNode();
			((Node)shinAuraSceneNode).Name = StringName.op_Implicit("ShinAura");
			ShinAuraSceneNode shinAuraSceneNode2 = shinAuraSceneNode;
			visuals.AddChild((Node)(object)shinAuraSceneNode2, false, (InternalMode)0);
			if (visuals.IsNodeReady())
			{
				visuals.MoveChild((Node)(object)shinAuraSceneNode2, 0);
			}
			string value = SafeNodePath(visuals);
			string value2 = ((object)creatureNode.Entity)?.GetType().Name ?? "unknown";
			ValencinaProbeLog.Warn("shin-aura-fallback-created", $"Created runtime ShinAura fallback. parent={value}, creatureType={value2}, reason={reason}.");
			return (Node?)(object)shinAuraSceneNode2;
		}
		catch (Exception ex)
		{
			ValencinaProbeLog.Warn("shin-aura-fallback-failed", "Failed to create runtime ShinAura fallback. reason=" + reason + ", error=" + ex.Message);
			return null;
		}
	}

	private static string SafeNodePath(Node? node)
	{
		try
		{
			return (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node)) ? ((object)node.GetPath()).ToString() : "null";
		}
		catch
		{
			return "unavailable";
		}
	}

	public static void HideAllVisibleAuras()
	{
		Node instance = (Node)(object)NCombatRoom.Instance;
		if (instance != null && GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			int count = 0;
			HideAllRecursive(instance, ref count);
			if (count > 0)
			{
				MainFile.Logger.Info($"[ShinAura] Hidden {count} aura node(s) from config.", 1);
			}
		}
	}

	private static void HideAllRecursive(Node root, ref int count)
	{
		foreach (Node child in root.GetChildren(false))
		{
			if (child.Name == StringName.op_Implicit("ShinAura") && SetAuraActive(child, active: false))
			{
				count++;
			}
			HideAllRecursive(child, ref count);
		}
	}

	private static bool SetAuraActive(Node? node, bool active)
	{
		if (node == null || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return false;
		}
		if (node is ShinAuraSceneNode shinAuraSceneNode)
		{
			if (((CanvasItem)shinAuraSceneNode).Visible == active)
			{
				return false;
			}
			shinAuraSceneNode.SetAuraActive(active);
			return true;
		}
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null)
		{
			if (val.Visible == active)
			{
				return false;
			}
			val.Visible = active;
			((Node)val).SetProcess(active);
			return true;
		}
		return false;
	}

	private static Node? FindAuraNode(NCreature? creatureNode)
	{
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return null;
		}
		return FindByUniqueOrName((Node?)(object)creatureNode.Visuals, "ShinAura") ?? FindByUniqueOrName(TryGetCurrentBody(creatureNode), "ShinAura") ?? FindByUniqueOrName((Node?)(object)creatureNode, "ShinAura");
	}

	private static Node? TryGetCurrentBody(NCreature creatureNode)
	{
		try
		{
			NCreatureVisuals visuals = creatureNode.Visuals;
			return (Node?)(object)((visuals != null) ? visuals.GetCurrentBody() : null);
		}
		catch
		{
			return null;
		}
	}

	private static Node? FindByUniqueOrName(Node? root, string nodeName)
	{
		if (root == null || !GodotObject.IsInstanceValid((GodotObject)(object)root))
		{
			return null;
		}
		Node val = root.GetNodeOrNull(NodePath.op_Implicit("%" + nodeName)) ?? root.GetNodeOrNull(NodePath.op_Implicit(nodeName));
		if (val != null)
		{
			return val;
		}
		return FindRecursive(root, nodeName);
	}

	private static Node? FindRecursive(Node root, string nodeName)
	{
		foreach (Node child in root.GetChildren(false))
		{
			if (child.Name == StringName.op_Implicit(nodeName))
			{
				return child;
			}
			Node val = FindRecursive(child, nodeName);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static NCreature? FindCreatureNode(Creature owner)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return null;
		}
		return instance.GetCreatureNode(owner);
	}
}
