using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Cards;

public sealed class BulletPropulsion : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
	{
		(DynamicVar)new CalculationBaseVar(10m),
		(DynamicVar)new ExtraDamageVar(4m),
		(DynamicVar)((CalculatedVar)new CalculatedDamageVar((ValueProp)8)).WithMultiplier((Func<CardModel, Creature, decimal>)CountAmmoSpenders)
	};

	public BulletPropulsion()
		: base(2, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		ArgumentNullException.ThrowIfNull(play.Target, "Target");
		await ExecuteAttackCommandAsync(choiceContext, DamageCmd.Attack(((CardModel)this).DynamicVars.CalculatedDamage).FromCard((CardModel)(object)this).Targeting(play.Target)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null));
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.ExtraDamage).UpgradeValueBy(1m);
	}

	private static decimal CountAmmoSpenders(CardModel card, Creature? _)
	{
		if (!((AbstractModel)card).IsMutable || card.Owner == null)
		{
			return 0m;
		}
		try
		{
			if (card.Owner.PlayerCombatState != null)
			{
				return card.Owner.PlayerCombatState.AllCards.Count(IsAmmoSpender);
			}
			return PileTypeExtensions.GetPile((PileType)6, card.Owner).Cards.Count(IsAmmoSpender);
		}
		catch
		{
			return 0m;
		}
	}

	private static bool IsAmmoSpender(CardModel card)
	{
		if (card is ValencinaCard valencinaCard)
		{
			return valencinaCard.SpendsAmmo;
		}
		return false;
	}
}
