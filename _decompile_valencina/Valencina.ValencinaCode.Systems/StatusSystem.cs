using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Relics;
using Valencina.ValencinaCode.Relics.Rien;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Systems;

public static class StatusSystem
{
	private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public new bool Equals(object? x, object? y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private static readonly HashSet<object> TremorStunUsedThisCombat = new HashSet<object>(ReferenceEqualityComparer.Instance);

	private static bool _relicTremorTriggersActive;

	public static void EnterCombat()
	{
		ImperfectForesightEye.ResetCombatState();
		CompleteForesightEye.ResetCombatState();
		ValencinaShinPower.ResetCombatState();
		TremorStunUsedThisCombat.Clear();
	}

	public static void LeaveCombat()
	{
		TremorStunUsedThisCombat.Clear();
	}

	private static Creature? ResolveApplier(CardModel? sourceCard)
	{
		if (sourceCard == null)
		{
			return null;
		}
		Player owner = sourceCard.Owner;
		if (owner == null)
		{
			return null;
		}
		return owner.Creature;
	}

	private static Task<TPower?> ApplyStatusPowerAsync<TPower>(Creature target, int amount, CardModel? sourceCard, PlayerChoiceContext? choiceContext) where TPower : ValencinaPower
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		if (choiceContext == null)
		{
			return CompatPowerCmd.Apply<TPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), target, (decimal)amount, ResolveApplier(sourceCard), sourceCard, silent: false);
		}
		return CommonActions.Apply<TPower>(choiceContext, target, sourceCard, (decimal)amount, silent: false);
	}

	private static async Task NotifyBurnAppliedAsync(Creature? applier, Creature target, int amount, CardModel? sourceCard, PlayerChoiceContext? choiceContext)
	{
		if (applier == null || amount <= 0)
		{
			return;
		}
		foreach (IBurnAppliedListener item in CreaturePowerAccess.Enumerate(applier).OfType<IBurnAppliedListener>().OrderBy(StableListenerKey)
			.ToList())
		{
			await item.OnBurnAppliedAsync((PlayerChoiceContext)(((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext())), target, amount, sourceCard);
		}
	}

	private static async Task NotifyTremorAppliedAsync(Creature? applier, Creature target, int amount, CardModel? sourceCard, PlayerChoiceContext? choiceContext)
	{
		if (applier == null || amount <= 0)
		{
			return;
		}
		foreach (ITremorAppliedListener item in CreaturePowerAccess.Enumerate(applier).OfType<ITremorAppliedListener>().OrderBy(StableListenerKey)
			.ToList())
		{
			await item.OnTremorAppliedAsync((PlayerChoiceContext)(((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext())), target, amount, sourceCard);
		}
	}

	private static async Task NotifyTremorDetonatedAsync(Creature? applier, Creature target, int amount, CardModel? sourceCard, PlayerChoiceContext? choiceContext)
	{
		if (applier == null || amount <= 0)
		{
			return;
		}
		foreach (ITremorDetonatedListener item in CreaturePowerAccess.Enumerate(applier).OfType<ITremorDetonatedListener>().OrderBy(StableListenerKey)
			.ToList())
		{
			await item.OnTremorDetonatedAsync((PlayerChoiceContext)(((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext())), target, amount, sourceCard);
		}
	}

	public static async Task<BurnPower?> ApplyBurnAsync(Creature? target, int amount, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null, bool notifyOwnerPowers = true)
	{
		if (target == null || amount <= 0)
		{
			return null;
		}
		amount = LevantinRelic.ModifyBurnAmount(amount, sourceCard);
		amount = FirelightFlower.ModifyBurnAmount(amount, sourceCard);
		BurnPower power = await ApplyStatusPowerAsync<BurnPower>(target, amount, sourceCard, choiceContext);
		if (power == null)
		{
			power = FindPower<BurnPower>(target);
		}
		MainFile.Logger.Info($"[StatusSystem] applied Burn {amount} to Creature {target.Name}.", 1);
		if (notifyOwnerPowers)
		{
			await NotifyBurnAppliedAsync(ResolveApplier(sourceCard), target, amount, sourceCard, choiceContext);
		}
		return power;
	}

	public static async Task<TremorPower?> ApplyTremorAsync(Creature? target, int amount, CardModel? sourceCard = null, bool allowStarterRelicConversion = true, PlayerChoiceContext? choiceContext = null)
	{
		if (target == null || amount <= 0)
		{
			return null;
		}
		Creature applier = ResolveApplier(sourceCard);
		if (FindPower<BurningTremorPower>(target) != null)
		{
			await ApplyStatusPowerAsync<BurningTremorPower>(target, amount, sourceCard, choiceContext);
			bool starterRelicHandledAlreadyBurning = false;
			if (allowStarterRelicConversion)
			{
				starterRelicHandledAlreadyBurning = await CompleteForesightEye.TryHandleTremorAppliedAsync(target, sourceCard, choiceContext);
				if (!starterRelicHandledAlreadyBurning)
				{
					starterRelicHandledAlreadyBurning = await ImperfectForesightEye.TryHandleTremorAppliedAsync(target, sourceCard, choiceContext);
				}
			}
			MainFile.Logger.Info($"[StatusSystem] applied Tremor {amount} to Creature {target.Name}. burning=yes", 1);
			await NotifyTremorAppliedAsync(applier, target, amount, sourceCard, choiceContext);
			await NotifyRelicsTremorAppliedAsync((sourceCard != null) ? sourceCard.Owner : null, target, amount, sourceCard, choiceContext);
			if (starterRelicHandledAlreadyBurning)
			{
				MainFile.Logger.Info("[StatusSystem] starter relic handled Tremor application on already-burning Creature " + target.Name + ".", 1);
			}
			return null;
		}
		TremorPower power = await ApplyStatusPowerAsync<TremorPower>(target, amount, sourceCard, choiceContext);
		if (power == null)
		{
			power = FindPower<TremorPower>(target);
		}
		bool converted = false;
		if (allowStarterRelicConversion)
		{
			converted = await CompleteForesightEye.TryHandleTremorAppliedAsync(target, sourceCard, choiceContext);
			if (!converted)
			{
				converted = await ImperfectForesightEye.TryHandleTremorAppliedAsync(target, sourceCard, choiceContext);
			}
		}
		bool flag = FindPower<BurningTremorPower>(target) != null;
		MainFile.Logger.Info($"[StatusSystem] applied Tremor {amount} to Creature {target.Name}. burning={(flag ? "yes" : "no")}", 1);
		await NotifyTremorAppliedAsync(applier, target, amount, sourceCard, choiceContext);
		await NotifyRelicsTremorAppliedAsync((sourceCard != null) ? sourceCard.Owner : null, target, amount, sourceCard, choiceContext);
		if (converted)
		{
			MainFile.Logger.Info("[StatusSystem] converted Tremor to burning on Creature " + target.Name + ".", 1);
		}
		return power;
	}

	private static async Task NotifyRelicsTremorAppliedAsync(Player? applierPlayer, Creature target, int amount, CardModel? sourceCard, PlayerChoiceContext? choiceContext)
	{
		if (applierPlayer == null || amount <= 0 || _relicTremorTriggersActive)
		{
			return;
		}
		_relicTremorTriggersActive = true;
		try
		{
			ScorchingHammer relic = applierPlayer.GetRelic<ScorchingHammer>();
			if (relic != null && FindPower<BurningTremorPower>(target) == null)
			{
				((RelicModel)relic).Flash();
				await TryConvertTremorToBurningAsync(target, sourceCard, choiceContext);
			}
			EightDirectionsBell relic2 = applierPlayer.GetRelic<EightDirectionsBell>();
			if (relic2 != null)
			{
				Creature creature = applierPlayer.Creature;
				if (creature != null && creature.IsAlive)
				{
					((RelicModel)relic2).Flash();
					await ApplyStatusPowerAsync<BreathingMethodPower>(creature, 2, null, choiceContext);
				}
			}
			TremorCoupling relic3 = applierPlayer.GetRelic<TremorCoupling>();
			if (relic3 != null)
			{
				((RelicModel)relic3).Flash();
				await ApplyCouplingTremorAsync(applierPlayer, choiceContext);
			}
		}
		finally
		{
			_relicTremorTriggersActive = false;
		}
	}

	private static async Task ApplyCouplingTremorAsync(Player player, PlayerChoiceContext? choiceContext)
	{
		Creature creature = player.Creature;
		ICombatState val = ((creature != null) ? creature.CombatState : null);
		if (val == null)
		{
			return;
		}
		List<Creature> list = val.HittableEnemies.Where((Creature enemy) => enemy != null && enemy.IsAlive).OrderBy(StableCreatureKey).ToList();
		if (list.Count == 0)
		{
			return;
		}
		Rng niche = player.RunState.Rng.Niche;
		Dictionary<Creature, int> dictionary = new Dictionary<Creature, int>();
		for (int num = 0; num < 10; num++)
		{
			Creature key = list[niche.NextInt(0, list.Count)];
			dictionary[key] = ((!dictionary.TryGetValue(key, out var value)) ? 1 : (value + 1));
		}
		foreach (var (val3, amount) in dictionary)
		{
			if (val3.IsAlive)
			{
				await ApplyTremorAsync(val3, amount, null, allowStarterRelicConversion: false, choiceContext);
			}
		}
	}

	private static string StableCreatureKey(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name;
			}
		}
		return (string)obj;
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

	public static async Task<bool> TryConvertTremorToBurningAsync(Creature? target, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		if (target == null)
		{
			return false;
		}
		if (FindPower<BurningTremorPower>(target) != null)
		{
			return true;
		}
		TremorPower tremorPower = FindPower<TremorPower>(target);
		if (tremorPower == null || ((PowerModel)tremorPower).Amount <= 0)
		{
			return false;
		}
		int amount = ((PowerModel)tremorPower).Amount;
		await PowerCmd.Remove((PowerModel)(object)tremorPower);
		return await ApplyStatusPowerAsync<BurningTremorPower>(target, amount, sourceCard, choiceContext) != null || FindPower<BurningTremorPower>(target) != null;
	}

	public static int TremorAmount(Creature? target)
	{
		if (target == null)
		{
			return 0;
		}
		BurningTremorPower burningTremorPower = FindPower<BurningTremorPower>(target);
		if (burningTremorPower != null)
		{
			return ((PowerModel)burningTremorPower).Amount;
		}
		TremorPower tremorPower = FindPower<TremorPower>(target);
		if (tremorPower == null)
		{
			return 0;
		}
		return ((PowerModel)tremorPower).Amount;
	}

	public static bool HasBurnOrTremor(Creature? target)
	{
		if (target == null)
		{
			return false;
		}
		if (FindPower<BurnPower>(target) == null && FindPower<TremorPower>(target) == null)
		{
			return FindPower<BurningTremorPower>(target) != null;
		}
		return true;
	}

	public static async Task<int> DetonateTremorAsync(Creature? target, CardModel? sourceCard = null, bool consumeStacks = true, PlayerChoiceContext? choiceContext = null)
	{
		if (target == null)
		{
			return 0;
		}
		BurningTremorPower burning = FindPower<BurningTremorPower>(target);
		TremorPower tremor = FindPower<TremorPower>(target);
		int tremorAmount = ((burning != null) ? ((PowerModel)burning).Amount : ((tremor != null) ? ((PowerModel)tremor).Amount : 0));
		if (tremorAmount <= 0)
		{
			return 0;
		}
		await NotifyTremorDetonatedAsync(ResolveApplier(sourceCard), target, tremorAmount, sourceCard, choiceContext);
		int startingHp = target.CurrentHp;
		int threshold = (startingHp + 1) / 2;
		int extraHpLoss = 0;
		NCombatRoom instance = NCombatRoom.Instance;
		Node sfxAnchor = (Node)(object)((instance != null) ? instance.GetCreatureNode(target) : null);
		ValencinaLocalSfx.PlayTremorBurst(sfxAnchor);
		if (burning != null)
		{
			BurnPower? burnPower = FindPower<BurnPower>(target);
			int burnAmount = (int)(((decimal?)((burnPower != null) ? new int?(((PowerModel)burnPower).Amount) : ((int?)null))) ?? 0m);
			extraHpLoss = (burnAmount + tremorAmount) / 2;
			if (extraHpLoss > 0)
			{
				object obj;
				if (sourceCard == null)
				{
					obj = null;
				}
				else
				{
					Player owner = sourceCard.Owner;
					obj = ((owner != null) ? owner.GetRelic<ScorchingHammer>() : null);
				}
				ScorchingHammer scorchingHammer = (ScorchingHammer)obj;
				if (scorchingHammer != null)
				{
					((RelicModel)scorchingHammer).Flash();
					extraHpLoss = (int)Math.Floor((decimal)extraHpLoss * 1.25m);
				}
			}
			if (extraHpLoss > 0)
			{
				if (HasHemostasisProtection(target))
				{
					MainFile.Logger.Info($"[StatusSystem] Hemostasis prevented {extraHpLoss} burning tremor HP loss on {target.Name}.", 1);
				}
				else
				{
					object obj2 = ((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext());
					decimal num = extraHpLoss;
					object obj3;
					if (sourceCard == null)
					{
						obj3 = null;
					}
					else
					{
						Player owner2 = sourceCard.Owner;
						obj3 = ((owner2 != null) ? owner2.Creature : null);
					}
					await CreatureCmd.Damage((PlayerChoiceContext)obj2, target, num, (ValueProp)6, (Creature)obj3, sourceCard);
					MainFile.Logger.Info($"[StatusSystem] burning tremor detonated for {extraHpLoss} HP loss on {target.Name}. currentHp={target.CurrentHp}.", 1);
					await RefreshMonsterIntentAfterPossibleStatusReviveAsync(target, startingHp, extraHpLoss, "burning-tremor");
				}
			}
			if (consumeStacks && burnAmount > 0)
			{
				BurnPower burnPower2 = FindPower<BurnPower>(target);
				int num2 = burnAmount / 2;
				if (burnPower2 != null)
				{
					if (num2 <= 0)
					{
						await PowerCmd.Remove((PowerModel)(object)burnPower2);
					}
					else
					{
						burnPower2.SetStacks(num2);
					}
				}
			}
		}
		if (!target.IsAlive || target.IsDead)
		{
			MainFile.Logger.Info("[StatusSystem] skipped tremor stack/stun follow-up on " + target.Name + "; target died or entered a revive state.", 1);
			await ReturnCardsOnTremorDetonatedAsync(sourceCard);
			return extraHpLoss;
		}
		if (consumeStacks)
		{
			int num3 = tremorAmount / 2;
			if (burning != null)
			{
				if (num3 <= 0)
				{
					await PowerCmd.Remove((PowerModel)(object)burning);
				}
				else
				{
					burning.SetStacks(num3);
				}
			}
			else if (tremor != null)
			{
				if (num3 <= 0)
				{
					await PowerCmd.Remove((PowerModel)(object)tremor);
				}
				else
				{
					tremor.SetStacks(num3);
				}
			}
		}
		if (tremorAmount >= threshold && TremorStunUsedThisCombat.Add(target))
		{
			ValencinaLocalSfx.PlayTremorStagger(sfxAnchor);
			try
			{
				await TryStunAsync(target);
			}
			catch (Exception value)
			{
				MainFile.Logger.Info($"[StatusSystem] tremor stun crashed on {target.Name}: {value}", 1);
			}
		}
		await ReturnCardsOnTremorDetonatedAsync(sourceCard);
		return extraHpLoss;
	}

	public static async Task RefreshMonsterIntentAfterPossibleStatusReviveAsync(Creature? target, int hpBeforeDamage, int damage, string source)
	{
		if (((target != null) ? target.Monster : null) == null || damage <= 0 || hpBeforeDamage <= 0 || hpBeforeDamage > damage || !target.IsAlive || target.IsDead)
		{
			return;
		}
		try
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(target) : null);
			if (val != null)
			{
				await val.RefreshIntents();
				MainFile.Logger.Info($"[StatusSystem] refreshed revived monster intent after {source} fatal status damage on {target.Name}. hpBefore={hpBeforeDamage}, hpNow={target.CurrentHp}.", 1);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[StatusSystem] failed to refresh revived monster intent after {source} fatal status damage on {target.Name}: {ex.GetType().Name}: {ex.Message}", 1);
		}
	}

	private static async Task ReturnCardsOnTremorDetonatedAsync(CardModel? sourceCard)
	{
		Player val = ((sourceCard != null) ? sourceCard.Owner : null);
		if (val == null)
		{
			return;
		}
		foreach (BlessedRelease item in PileTypeExtensions.GetPile((PileType)3, val).Cards.ToList().OfType<BlessedRelease>())
		{
			await CardPileCmd.Add((CardModel)(object)item, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
		}
	}

	public static bool HasHemostasisProtection(Creature? target)
	{
		if (target == null)
		{
			return false;
		}
		HemostasisPower hemostasisPower = FindPower<HemostasisPower>(target);
		if (hemostasisPower != null)
		{
			return ((PowerModel)hemostasisPower).Amount > 0;
		}
		return false;
	}

	private static async Task TryStunAsync(Creature target)
	{
		if (target.Monster == null)
		{
			MainFile.Logger.Info("[StatusSystem] tremor stun skipped on " + target.Name + "; players cannot be stunned by CreatureCmd.Stun.", 1);
			return;
		}
		if (!target.IsAlive || target.IsDead)
		{
			MainFile.Logger.Info("[StatusSystem] tremor stun skipped on " + target.Name + "; target is dead or reviving.", 1);
			return;
		}
		if (target.Monster is UngezieferKaiser { IsEmperorBloodDisabled: false })
		{
			MainFile.Logger.Info("[StatusSystem] tremor stun skipped on " + target.Name + "; Ungeziefer Kaiser is immune to stun.", 1);
			return;
		}
		if (HasDeathPersistentPower(target))
		{
			MainFile.Logger.Info("[StatusSystem] tremor stun skipped on " + target.Name + "; target has a death/revive state that must not be overwritten.", 1);
			return;
		}
		await CreatureCmd.Stun(target, (string)null);
		MainFile.Logger.Info("[StatusSystem] tremor stun triggered on " + target.Name + " via CreatureCmd.Stun.", 1);
	}

	private static bool HasDeathPersistentPower(Creature target)
	{
		foreach (PowerModel item in CreaturePowerAccess.Enumerate(target))
		{
			try
			{
				if (!((AbstractModel)item).ShouldCreatureBeRemovedFromCombatAfterDeath(target))
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				MainFile.Logger.Warn($"[StatusSystem] failed to inspect death persistence on {target.Name}/{((object)item).GetType().Name}: {ex.GetType().Name}: {ex.Message}", 1);
			}
		}
		return false;
	}

	private static TPower? FindPower<TPower>(Creature creature) where TPower : class
	{
		foreach (object item in EnumeratePowers(creature))
		{
			if (item is TPower result)
			{
				return result;
			}
		}
		return null;
	}

	private static IEnumerable<object?> EnumeratePowers(Creature owner)
	{
		foreach (PowerModel item in CreaturePowerAccess.Enumerate(owner))
		{
			yield return item;
		}
	}
}
