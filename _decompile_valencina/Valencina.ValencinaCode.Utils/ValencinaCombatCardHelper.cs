using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Utils;

public static class ValencinaCombatCardHelper
{
	public static int ReadEnergyCostForCard(CardModel? card)
	{
		if (card == null)
		{
			return 0;
		}
		try
		{
			if (DisposalCostSystem.IsDisposalCard(card))
			{
				return DisposalCostSystem.GetSharedCostFor(card);
			}
			if (card.EnergyCost.CostsX)
			{
				return 0;
			}
			return Math.Max(0, card.EnergyCost.GetAmountToSpend());
		}
		catch
		{
			return 0;
		}
	}

	public static int CurrentBreathingMethod(Creature? creature)
	{
		BreathingMethodPower breathingMethodPower = CreaturePowerAccess.Find<BreathingMethodPower>(creature);
		if (breathingMethodPower == null)
		{
			return 0;
		}
		return Math.Max(0, ((PowerModel)breathingMethodPower).Amount);
	}

	public static async Task RemoveBreathingMethodAsync(Creature? creature)
	{
		BreathingMethodPower breathingMethodPower = CreaturePowerAccess.Find<BreathingMethodPower>(creature);
		if (breathingMethodPower != null)
		{
			await PowerCmd.Remove((PowerModel)(object)breathingMethodPower);
		}
	}

	public static IReadOnlyList<CardModel> CardsIn(Player owner, params PileType[] piles)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<CardModel> list = new List<CardModel>();
		foreach (PileType val in piles)
		{
			try
			{
				list.AddRange(PileTypeExtensions.GetPile(val, owner).Cards);
			}
			catch
			{
			}
		}
		return list;
	}

	public static IReadOnlyList<CardModel> StatusCardsInCombatPiles(Player owner)
	{
		PileType[] array = new PileType[3];
		RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		return (from card in CardsIn(owner, (PileType[])(object)array)
			where (int)card.Type == 4
			select card).Distinct().ToList();
	}

	public static async Task ApplySupportedDebuffCloneAsync(PlayerChoiceContext choiceContext, Creature source, Creature target, CardModel cardSource, PowerModel power, int multiplier)
	{
		if ((int)power.Type != 2 || power.Amount <= 0)
		{
			return;
		}
		int num = Math.Max(0, power.Amount * Math.Max(1, multiplier));
		if (num <= 0)
		{
			return;
		}
		if (!(power is WeakPower))
		{
			if (!(power is VulnerablePower))
			{
				if (!(power is FrailPower))
				{
					if (!(power is NoBlockPower))
					{
						if (!(power is BurnPower))
						{
							if (power is TremorPower || power is BurningTremorPower)
							{
								await StatusSystem.ApplyTremorAsync(target, num, cardSource, allowStarterRelicConversion: true, choiceContext);
							}
							else if (power is HighTemperatureStrengthDownPower)
							{
								await CommonActions.Apply<HighTemperatureStrengthDownPower>(choiceContext, target, cardSource, (decimal)num, silent: false);
							}
						}
						else
						{
							await StatusSystem.ApplyBurnAsync(target, num, cardSource, choiceContext);
						}
					}
					else
					{
						await CommonActions.Apply<NoBlockPower>(choiceContext, target, cardSource, (decimal)num, silent: false);
					}
				}
				else
				{
					await CommonActions.Apply<FrailPower>(choiceContext, target, cardSource, (decimal)num, silent: false);
				}
			}
			else
			{
				await CommonActions.Apply<VulnerablePower>(choiceContext, target, cardSource, (decimal)num, silent: false);
			}
		}
		else
		{
			await CommonActions.Apply<WeakPower>(choiceContext, target, cardSource, (decimal)num, silent: false);
		}
	}
}
