using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class Lucio : ValencinaCard, IInstantAttackCard
{
	private bool _heartGuardEnhanced;

	public int InstantAmmoCost => 1;

	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => InstantAmmoCost;

	public override bool CanBeGeneratedInCombat => false;

	[SavedProperty]
	public bool HeartGuardEnhanced
	{
		get
		{
			return _heartGuardEnhanced;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_heartGuardEnhanced = value;
			SynchronizeHeartGuardDisplay();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[4]
	{
		(DynamicVar)new DamageVar(3m, (ValueProp)8),
		(DynamicVar)new CardsVar(1),
		(DynamicVar)new EnergyVar(1),
		new DynamicVar("Enhanced", HeartGuardEnhanced ? 1m : 0m)
	});

	public Lucio()
		: base(0, (CardType)1, (CardRarity)6, (TargetType)2)
	{
	}

	public void ApplyHeartGuardEnhancement()
	{
		((AbstractModel)this).AssertMutable();
		if (HeartGuardEnhanced)
		{
			SynchronizeHeartGuardDisplay();
		}
		else
		{
			HeartGuardEnhanced = true;
		}
	}

	private void SynchronizeHeartGuardDisplay()
	{
		DynamicVar val = default(DynamicVar);
		if (((CardModel)this).DynamicVars.TryGetValue("Enhanced", ref val))
		{
			val.BaseValue = (_heartGuardEnhanced ? 1m : 0m);
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await InstantAttackHelper.ExecuteAgainstTargetAsync(this, choiceContext, target, 3);
			await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)this, consumeStacks: true, choiceContext);
			if (HeartGuardEnhanced)
			{
				await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
				await PlayerCmd.GainEnergy(((DynamicVar)((CardModel)this).DynamicVars.Energy).BaseValue, ((CardModel)this).Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
	}
}
