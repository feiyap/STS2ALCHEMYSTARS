using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Enchantments;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Powers;

public sealed class ElegantSwordplayPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Cards", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0)
		{
			return Task.CompletedTask;
		}
		InstantEnchantment instant = ModelDb.Enchantment<InstantEnchantment>();
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, player).Cards.Where((CardModel card) => CanEnchantWithInstant(card, instant)).ToList();
		if (list.Count == 0)
		{
			return Task.CompletedTask;
		}
		int num = Math.Max(0, ((PowerModel)this).Amount);
		for (int num2 = 0; num2 < num; num2++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			CardModel val = player.RunState.Rng.CombatCardSelection.NextItem<CardModel>((IEnumerable<CardModel>)list) ?? list[0];
			list.Remove(val);
			if (!CanEnchantWithInstant(val, instant))
			{
				num2--;
				continue;
			}
			try
			{
				((PowerModel)this).Flash();
				CardCmd.Enchant<InstantEnchantment>(val, 1m);
			}
			catch
			{
				num2--;
			}
		}
		return Task.CompletedTask;
	}

	private static bool CanEnchantWithInstant(CardModel? card, InstantEnchantment instant)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if (card == null || (int)card.Type != 1 || card is IInstantAttackCard)
		{
			return false;
		}
		if (card.Enchantment != null)
		{
			return false;
		}
		return ((EnchantmentModel)instant).CanEnchant(card);
	}
}
