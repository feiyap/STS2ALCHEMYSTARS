using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class FenghuangDoll : RienRelic
{
	private const int CombatsPerGold = 3;

	private const int GoldReward = 50;

	private const int GoldPerStrength = 150;

	private int _combatsTowardGold;

	public override bool ShowCounter => true;

	public override int DisplayAmount => _combatsTowardGold;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[4]
	{
		new DynamicVar("Combats", 3m),
		new DynamicVar("Gold", 50m),
		new DynamicVar("GoldPerStrength", 150m),
		(DynamicVar)new PowerVar<StrengthPower>("Strength", 1m)
	};

	[SavedProperty]
	public int CombatsTowardGold
	{
		get
		{
			return _combatsTowardGold;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_combatsTowardGold = Math.Max(0, value);
			((RelicModel)this).InvokeDisplayAmountChanged();
		}
	}

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			int num = ((RelicModel)this).Owner.Gold / 150;
			if (num > 0)
			{
				((RelicModel)this).Flash();
				await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, (decimal)num, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		if (((RelicModel)this).Owner != null)
		{
			CombatsTowardGold++;
			if (CombatsTowardGold >= 3)
			{
				CombatsTowardGold = 0;
				((RelicModel)this).Flash();
				await PlayerCmd.GainGold(50m, ((RelicModel)this).Owner, false);
			}
		}
	}
}
