using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(MerchantInventory), "CreateForNormalMerchant")]
internal static class ValencinaGuaranteedCounterShopPatch
{
	private static readonly FieldInfo? CharacterCardEntriesField = AccessTools.Field(typeof(MerchantInventory), "_characterCardEntries");

	private static readonly PropertyInfo? CreationResultProperty = AccessTools.Property(typeof(MerchantCardEntry), "CreationResult");

	private static void Postfix(Player player, ref MerchantInventory __result)
	{
		try
		{
			if (!ShouldForceCounterStyleOffer(player) || !(CharacterCardEntriesField?.GetValue(__result) is List<MerchantCardEntry> { Count: not 0 } list))
			{
				return;
			}
			PropertyInfo? creationResultProperty = CreationResultProperty;
			if ((object)creationResultProperty == null || !creationResultProperty.CanWrite)
			{
				MainFile.Logger.Warn("[GuaranteedCounterShop] Could not access MerchantCardEntry.CreationResult; skipped the guaranteed offer.", 1);
				return;
			}
			CardCreationResult val = CreateCounterStyleResult(player, list);
			if (val != null)
			{
				MerchantCardEntry val2 = list[0];
				CreationResultProperty.SetValue(val2, val);
				val2.SetOnSale();
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[GuaranteedCounterShop] Failed to create the guaranteed counter offer; kept the vanilla shop. {value}", 1);
		}
	}

	private static bool ShouldForceCounterStyleOffer(Player player)
	{
		return player.GetRelic<BernoullitMemory>()?.GetActiveCounterDefinitions().All((ValencinaCounterDefinition definition) => definition.Style == ValencinaCounterStyle.BaseCounter) ?? false;
	}

	private static CardCreationResult? CreateCounterStyleResult(Player player, IReadOnlyList<MerchantCardEntry> characterEntries)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		IReadOnlyList<CardModel> counterStyleCards = GetCounterStyleCards();
		if (counterStyleCards.Count == 0)
		{
			return null;
		}
		HashSet<CardModel> occupiedCards = characterEntries.Skip(1).Select(delegate(MerchantCardEntry entry)
		{
			CardCreationResult creationResult = entry.CreationResult;
			return (creationResult == null) ? null : creationResult.Card.CanonicalInstance;
		}).OfType<CardModel>()
			.ToHashSet();
		List<CardModel> list = counterStyleCards.Where((CardModel card) => !occupiedCards.Contains(card.CanonicalInstance)).ToList();
		IReadOnlyList<CardModel> readOnlyList;
		if (list.Count <= 0)
		{
			readOnlyList = counterStyleCards;
		}
		else
		{
			IReadOnlyList<CardModel> readOnlyList2 = list;
			readOnlyList = readOnlyList2;
		}
		IReadOnlyList<CardModel> readOnlyList3 = readOnlyList;
		CardModel val = readOnlyList3[player.PlayerRng.Shops.NextInt(readOnlyList3.Count)];
		CardCreationResult val2 = new CardCreationResult(((ICardScope)player.RunState).CreateCard(val, player));
		IRunState runState = player.RunState;
		int num = 1;
		List<CardCreationResult> list2 = new List<CardCreationResult>(num);
		CollectionsMarshal.SetCount(list2, num);
		Span<CardCreationResult> span = CollectionsMarshal.AsSpan(list2);
		int index = 0;
		span[index] = val2;
		Hook.ModifyMerchantCardCreationResults(runState, player, list2);
		return val2;
	}

	private static IReadOnlyList<CardModel> GetCounterStyleCards()
	{
		return new _003C_003Ez__ReadOnlyArray<CardModel>((CardModel[])(object)new CardModel[4]
		{
			(CardModel)ModelDb.Card<JieTu>(),
			(CardModel)ModelDb.Card<JieLu>(),
			(CardModel)ModelDb.Card<JieXiang>(),
			(CardModel)ModelDb.Card<PalermoSwordplaySecret>()
		});
	}
}
