using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Enchantments;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Relics;
using Valencina.ValencinaCode.Relics.Rien;
using Valencina.ValencinaCode.UI;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Systems;

public static class AmmoSystem
{
	private static Creature? _displayOwner;

	public static Creature? DisplayOwner => _displayOwner;

	public static int MaxAmmo => AmmoState.GetMaxAmmo(_displayOwner);

	public static bool HasFrontPower
	{
		get
		{
			if (_displayOwner != null)
			{
				return CreaturePowerAccess.Find<AmmoPower>(_displayOwner) != null;
			}
			return false;
		}
	}

	public static int MaxAmmoFor(Creature? owner = null)
	{
		Creature val = ResolveContextOwner(owner, null) ?? _displayOwner;
		if (val != null)
		{
			BindOwner(val);
		}
		return AmmoState.GetMaxAmmo(val);
	}

	public static int CurrentAmmo(Creature? owner = null)
	{
		Creature val = ResolveContextOwner(owner, null) ?? _displayOwner;
		if (val != null)
		{
			BindOwner(val);
		}
		return AmmoState.GetCurrentAmmo(val);
	}

	public static int AmmoSpentThisTurn(Creature? owner = null)
	{
		Creature val = ResolveContextOwner(owner, null) ?? _displayOwner;
		if (val != null)
		{
			BindOwner(val);
		}
		return AmmoState.GetAmmoSpentThisTurn(val);
	}

	public static int AmmoSpentThisCombat(Creature? owner = null)
	{
		Creature val = ResolveContextOwner(owner, null) ?? _displayOwner;
		if (val != null)
		{
			BindOwner(val);
		}
		return AmmoState.GetAmmoSpentThisCombat(val);
	}

	public static string DisplayText(Creature? owner = null)
	{
		return AmmoState.DisplayText(ResolveContextOwner(owner, null) ?? _displayOwner);
	}

	public static bool CanGainAmmo(Creature? owner = null)
	{
		owner = ResolveContextOwner(owner, null);
		if (owner != null)
		{
			return CreaturePowerAccess.Find<AmmoGainBlockedPower>(owner) == null;
		}
		return true;
	}

	public static Task EnterCombatAsync()
	{
		AmmoState.EnterCombat();
		StatusSystem.EnterCombat();
		_displayOwner = null;
		AmmoUiSync.RefreshAll(showFallbackLabel: false);
		MainFile.Logger.Info($"[AmmoSystem] combat init -> ammo={AmmoState.CurrentAmmo}/{AmmoState.MaxAmmo}, front=power, triggerPower=missing", 1);
		return Task.CompletedTask;
	}

	public static async Task TryRegisterCombatCreatureAsync(Creature? creature, PlayerChoiceContext? choiceContext = null)
	{
		if (CreaturePowerAccess.IsValencina(creature))
		{
			BindOwner(creature);
			SyncStateFromExistingPowers(creature);
			ValencinaProbeLog.Info("ammo-register-owner", $"Combat creature registered probe. {DescribeOwnerForProbe(creature, null)}, ammo={AmmoState.GetCurrentAmmo(creature)}/{AmmoState.GetMaxAmmo(creature)}.");
			bool flag = await EnsurePowerBoundAsync(creature, null, choiceContext);
			AmmoUiSync.RefreshAll(showFallbackLabel: false);
			MainFile.Logger.Info("[AmmoSystem] combat creature registered -> " + creature.Name + ". triggerPower=" + (flag ? "bound" : "missing"), 1);
		}
	}

	public static void LeaveCombat()
	{
		_displayOwner = null;
		AmmoState.ExitCombat();
		StatusSystem.LeaveCombat();
		AmmoUiSync.RefreshAll(showFallbackLabel: false);
	}

	public static Task TryBindFromRootAsync(object? root)
	{
		return Task.CompletedTask;
	}

	public static async Task<int> TryConsumeAsync(Creature? owner, int requested, CardModel? sourceCard = null, bool grantBreathingMethod = true, PlayerChoiceContext? choiceContext = null)
	{
		owner = ResolveContextOwner(owner, sourceCard);
		if (owner == null || requested <= 0)
		{
			return 0;
		}
		BindOwner(owner);
		ValencinaProbeLog.Info("ammo-consume-owner", $"Consume request probe. requested={requested}, grantBreathingMethod={grantBreathingMethod}, source={((object)sourceCard)?.GetType().Name ?? "null"}, before={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)}, {DescribeOwnerForProbe(owner, sourceCard)}.");
		int consumed = AmmoState.TryConsume(owner, requested);
		if (consumed <= 0)
		{
			await SyncVisualsAsync(owner, sourceCard, choiceContext);
			return 0;
		}
		if (grantBreathingMethod)
		{
			int num = consumed;
			int num2 = consumed;
			GunMaintenancePower gunMaintenancePower = CreaturePowerAccess.Find<GunMaintenancePower>(owner);
			if (gunMaintenancePower != null)
			{
				num = gunMaintenancePower.ModifyAmmoBreathingMethodGain(num);
			}
			if (num > 0 || num2 > 0)
			{
				int num3;
				if (!(sourceCard is IInstantAttackCard))
				{
					num3 = ((((sourceCard != null) ? sourceCard.Enchantment : null) is InstantEnchantment) ? 1 : 0);
				}
				else
				{
					num3 = 1;
				}
				if (num3 != 0 || !(sourceCard is ValencinaCard valencinaCard))
				{
					await BreathingMethodService.GainIntensityAndChargesAsync(owner, num, num2, sourceCard, choiceContext);
				}
				else
				{
					valencinaCard.QueueBreathingMethodGain(num, num2);
				}
			}
		}
		Player ownerPlayer = ResolveOwnerPlayer(owner, sourceCard);
		MainFile.Logger.Info($"[AmmoSystem] consumed {consumed}/{requested}. now={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)} owner={owner.Name}", 1);
		await NotifyAmmoConsumedAsync(owner, ownerPlayer, consumed, requested, sourceCard);
		if (AmmoState.GetCurrentAmmo(owner) == 0 && !(await CompleteForesightEye.TryHandleAmmoDepletedAsync(owner, sourceCard, choiceContext)))
		{
			await ImperfectForesightEye.TryHandleAmmoDepletedAsync(owner, sourceCard, choiceContext);
		}
		await SyncVisualsAsync(owner, sourceCard, choiceContext);
		return consumed;
	}

	public static async Task<int> AddAmmoAsync(Creature? owner, int amount, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		owner = ResolveContextOwner(owner, sourceCard);
		if (owner == null || amount <= 0)
		{
			return 0;
		}
		BindOwner(owner);
		ValencinaProbeLog.Info("ammo-add-owner", $"Add ammo request probe. amount={amount}, source={((object)sourceCard)?.GetType().Name ?? "null"}, before={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)}, {DescribeOwnerForProbe(owner, sourceCard)}.");
		if (!CanGainAmmo(owner))
		{
			await SyncVisualsAsync(owner, sourceCard, choiceContext);
			MainFile.Logger.Info($"[AmmoSystem] add blocked by AmmoGainBlockedPower. now={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)} owner={owner.Name}", 1);
			return 0;
		}
		int added = AmmoState.Add(owner, amount);
		if (added > 0)
		{
			if (LocalContext.IsMe(owner))
			{
				ValencinaLocalSfx.Play("res://Valencina/audio/reload/reload_once.mp3");
			}
			await NotifyAmmoReloadedAsync(owner, ResolveOwnerPlayer(owner, sourceCard), added, sourceCard);
		}
		await SyncVisualsAsync(owner, sourceCard, choiceContext);
		MainFile.Logger.Info($"[AmmoSystem] added {added}. now={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)} owner={owner.Name}", 1);
		return added;
	}

	public static async Task<int> ReloadToFullAsync(Creature? owner, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		owner = ResolveContextOwner(owner, sourceCard);
		if (owner == null)
		{
			return 0;
		}
		BindOwner(owner);
		ValencinaProbeLog.Info("ammo-reload-owner", $"Reload full request probe. source={((object)sourceCard)?.GetType().Name ?? "null"}, before={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)}, {DescribeOwnerForProbe(owner, sourceCard)}.");
		if (!CanGainAmmo(owner))
		{
			await SyncVisualsAsync(owner, sourceCard, choiceContext);
			MainFile.Logger.Info($"[AmmoSystem] reload blocked by AmmoGainBlockedPower. now={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)} owner={owner.Name}", 1);
			return 0;
		}
		int added = AmmoState.ReloadToFull(owner);
		if (added > 0)
		{
			if (LocalContext.IsMe(owner))
			{
				ValencinaLocalSfx.Play("res://Valencina/audio/reload/reload_once.mp3");
			}
			await NotifyAmmoReloadedAsync(owner, ResolveOwnerPlayer(owner, sourceCard), added, sourceCard);
		}
		await SyncVisualsAsync(owner, sourceCard, choiceContext);
		MainFile.Logger.Info($"[AmmoSystem] reload to full -> {AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)} owner={owner.Name}", 1);
		return added;
	}

	public static async Task<int> IncreaseMaxAmmoAsync(Creature? owner, int amount, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		owner = ResolveContextOwner(owner, sourceCard);
		if (owner == null || amount <= 0)
		{
			return 0;
		}
		BindOwner(owner);
		ValencinaProbeLog.Info("ammo-max-owner", $"Increase max ammo request probe. amount={amount}, source={((object)sourceCard)?.GetType().Name ?? "null"}, before={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)}, {DescribeOwnerForProbe(owner, sourceCard)}.");
		int increased = AmmoState.IncreaseMaxAmmo(owner, amount);
		await SyncVisualsAsync(owner, sourceCard, choiceContext);
		MainFile.Logger.Info($"[AmmoSystem] max ammo increased by {increased}. now={AmmoState.GetCurrentAmmo(owner)}/{AmmoState.GetMaxAmmo(owner)} owner={owner.Name}", 1);
		return increased;
	}

	private static string DescribeOwnerForProbe(Creature owner, CardModel? sourceCard)
	{
		Player? obj = ResolveOwnerPlayer(owner, sourceCard);
		object obj2 = ((obj != null) ? obj.NetId.ToString() : null);
		if (obj2 == null)
		{
			Player player = owner.Player;
			obj2 = ((player != null) ? player.NetId.ToString() : null) ?? "null";
		}
		string value = (string)obj2;
		string value2 = LocalContext.NetId?.ToString() ?? "null";
		bool value3 = LocalContext.IsMe(owner);
		return $"owner={owner.Name}, playerNetId={value}, localNetId={value2}, isLocal={value3}";
	}

	private static Creature? ResolveContextOwner(Creature? owner, CardModel? sourceCard)
	{
		if (sourceCard is ValencinaCard valencinaCard && owner == null)
		{
			Player owner2 = ((CardModel)valencinaCard).Owner;
			owner = ((owner2 != null) ? owner2.Creature : null);
		}
		if (owner != null && CreaturePowerAccess.IsValencina(owner))
		{
			return owner;
		}
		return null;
	}

	private static void BindOwner(Creature owner)
	{
		AmmoState.EnsureOwner(owner);
		if (ShouldUseAsDisplayOwner(owner))
		{
			_displayOwner = owner;
		}
	}

	private static void SyncStateFromExistingPowers(Creature owner)
	{
		int num = 6;
		AmmoCapacityPower ammoCapacityPower = CreaturePowerAccess.Find<AmmoCapacityPower>(owner);
		if (ammoCapacityPower != null)
		{
			num += Math.Max(0, ((PowerModel)ammoCapacityPower).Amount);
		}
		Player player = owner.Player;
		ThumbBadge thumbBadge = ((player != null) ? player.GetRelic<ThumbBadge>() : null);
		if (thumbBadge != null)
		{
			num += Math.Max(0, thumbBadge.AmmoCapacityBonus);
		}
		int currentAmmo = num;
		AmmoPower ammoPower = CreaturePowerAccess.Find<AmmoPower>(owner);
		if (ammoPower != null)
		{
			currentAmmo = ((PowerModel)ammoPower).Amount;
		}
		AmmoState.SyncOwner(owner, currentAmmo, num);
	}

	private static bool ShouldUseAsDisplayOwner(Creature owner)
	{
		if (LocalContext.IsMe(owner))
		{
			return true;
		}
		if (_displayOwner == null)
		{
			return !LocalContext.NetId.HasValue;
		}
		return false;
	}

	private static async Task SyncVisualsAsync(Creature owner, CardModel? sourceCard, PlayerChoiceContext? choiceContext = null)
	{
		await EnsurePowerBoundAsync(owner, sourceCard, choiceContext);
		AmmoUiSync.RefreshAll(showFallbackLabel: false);
	}

	private static async Task<bool> EnsurePowerBoundAsync(Creature owner, CardModel? sourceCard, PlayerChoiceContext? choiceContext = null)
	{
		if (!CreaturePowerAccess.IsValencina(owner))
		{
			return false;
		}
		AmmoState.EnsureOwner(owner);
		AmmoPower ammoPower = CreaturePowerAccess.Find<AmmoPower>(owner);
		if (ammoPower == null)
		{
			try
			{
				if (choiceContext == null)
				{
					await WaitForSceneFrameAsync();
				}
				ammoPower = await CompatPowerCmd.Apply<AmmoPower>((PlayerChoiceContext)(((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext())), owner, (decimal)AmmoState.GetCurrentAmmo(owner), owner, sourceCard, silent: false);
			}
			catch (Exception ex)
			{
				MainFile.Logger.Error("[AmmoSystem] applying AmmoPower failed: " + ex.Message, 1);
			}
		}
		if (ammoPower == null)
		{
			ammoPower = CreaturePowerAccess.Find<AmmoPower>(owner);
		}
		if (ammoPower == null)
		{
			return false;
		}
		ammoPower.SyncAmount(AmmoState.GetCurrentAmmo(owner));
		return true;
	}

	private static async Task WaitForSceneFrameAsync()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			await ((GodotObject)val).ToSignal((GodotObject)(object)val, SignalName.ProcessFrame);
		}
	}

	private static Player? ResolveOwnerPlayer(Creature owner, CardModel? sourceCard)
	{
		return ((sourceCard != null) ? sourceCard.Owner : null) ?? CreaturePowerAccess.GetPlayer(owner);
	}

	private static async Task NotifyAmmoConsumedAsync(Creature owner, Player? ownerPlayer, int consumed, int requested, CardModel? sourceCard)
	{
		List<IAmmoConsumedListener> list = CreaturePowerAccess.Enumerate(owner).OfType<IAmmoConsumedListener>().OrderBy(StableListenerKey)
			.ToList();
		ValencinaProbeLog.Info("ammo-consumed-listeners", $"Ammo consumed listeners snapshot. count={list.Count}, consumed={consumed}, requested={requested}, {DescribeOwnerForProbe(owner, sourceCard)}.");
		foreach (IAmmoConsumedListener listener in list)
		{
			try
			{
				await listener.OnAmmoConsumedAsync(consumed, requested, owner, ownerPlayer, sourceCard);
			}
			catch (Exception ex)
			{
				MainFile.Logger.Error("[AmmoSystem] ammo listener failed on " + listener.GetType().Name + ": " + ex.Message, 1);
			}
		}
	}

	private static async Task NotifyAmmoReloadedAsync(Creature owner, Player? ownerPlayer, int added, CardModel? sourceCard)
	{
		List<IAmmoReloadedListener> list = CreaturePowerAccess.Enumerate(owner).OfType<IAmmoReloadedListener>().OrderBy(StableListenerKey)
			.ToList();
		ValencinaProbeLog.Info("ammo-reloaded-listeners", $"Ammo reloaded listeners snapshot. count={list.Count}, added={added}, {DescribeOwnerForProbe(owner, sourceCard)}.");
		foreach (IAmmoReloadedListener listener in list)
		{
			try
			{
				await listener.OnAmmoReloadedAsync(added, owner, ownerPlayer, sourceCard);
			}
			catch (Exception ex)
			{
				MainFile.Logger.Error("[AmmoSystem] reload listener failed on " + listener.GetType().Name + ": " + ex.Message, 1);
			}
		}
	}

	private static string StableListenerKey(object listener)
	{
		PowerModel val = (PowerModel)((listener is PowerModel) ? listener : null);
		string text;
		if (val == null)
		{
			text = listener.GetType().FullName;
			if (text == null)
			{
				return listener.GetType().Name;
			}
		}
		else
		{
			Creature owner = val.Owner;
			text = (((owner == null) ? null : owner.CombatId?.ToString("D10")) ?? "no-owner") + ":" + ((AbstractModel)val).Id.Entry;
		}
		return text;
	}
}
