using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Powers;

public sealed class DespairHopeNoHopePower : PercentValencinaPower
{
	private const int TriggerCost = 2;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Percent", (decimal)base.PercentAmount);
		description.Add("Cost", 2m);
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || dealer != ((PowerModel)this).Owner || !(cardSource is PrecognitionJieTuCounterCard) || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		return 1m + Math.Max(0m, ((PowerModel)this).Amount) / 100m;
	}

	public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (((PowerModel)this).Owner == null)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		if (((owner != null) ? owner.Creature : null) != ((PowerModel)this).Owner || (int)cardPlay.Card.Type != 1 || cardPlay.Card is IPrecognitionVirtualCounterCard)
		{
			return;
		}
		ResourceInfo resources = cardPlay.Resources;
		if (((ResourceInfo)(ref resources)).EnergyValue < 2)
		{
			return;
		}
		Creature target = cardPlay.Target;
		object obj;
		if (target == null || !target.IsAlive)
		{
			ICombatState combatState = ((PowerModel)this).Owner.CombatState;
			obj = ((combatState != null) ? combatState.HittableEnemies.Where((Creature enemy) => enemy.IsAlive).OrderBy(ValencinaPowerStableKeys.Creature).FirstOrDefault() : null);
		}
		else
		{
			obj = cardPlay.Target;
		}
		Creature val = (Creature)obj;
		if (val != null)
		{
			InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
			if (power != null)
			{
				((PowerModel)this).Flash();
				await power.TriggerCounterAgainstAsync(choiceContext, val);
			}
		}
	}
}
