using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class GregorWoundOnHitPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			foreach (IHoverTip additionalHoverTip in base.AdditionalHoverTips)
			{
				yield return additionalHoverTip;
			}
			foreach (IHoverTip item in HoverTipFactory.FromCardWithCardHoverTips<Wound>(false))
			{
				yield return item;
			}
		}
	}

	public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		if (command.Attacker != ((PowerModel)this).Owner || command.TargetSide == ((PowerModel)this).Owner.Side || !ValuePropExtensions.IsPoweredAttack(command.DamageProps))
		{
			return;
		}
		List<DamageResult> list = command.Results.SelectMany((List<DamageResult> results) => results).ToList();
		if (!list.Any((DamageResult r) => r.UnblockedDamage > 0))
		{
			return;
		}
		Dictionary<Creature, int> dictionary = new Dictionary<Creature, int>();
		foreach (DamageResult item in list)
		{
			if (item.Receiver.IsPlayer && item.UnblockedDamage > 0)
			{
				dictionary[item.Receiver] = dictionary.GetValueOrDefault(item.Receiver) + 1;
			}
		}
		bool flashed = false;
		foreach (KeyValuePair<Creature, int> item2 in dictionary)
		{
			item2.Deconstruct(out var key, out var value);
			Creature player = key;
			int num = value;
			int num2 = ((PowerModel)this).Amount * num;
			if (num2 > 0)
			{
				if (!flashed)
				{
					((PowerModel)this).Flash();
					flashed = true;
				}
				try
				{
					await CardPileCmd.AddToCombatAndPreview<Wound>(player, (PileType)3, num2, (Player)null, (CardPilePosition)1);
				}
				catch (Exception ex)
				{
					MainFile.Logger.Warn("[GregorWoundOnHitPower] Failed to add Wound for " + player.Name + ": " + ex.Message, 1);
				}
			}
		}
	}
}
