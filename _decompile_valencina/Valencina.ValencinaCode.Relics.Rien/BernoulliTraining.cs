using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class BernoulliTraining : RienRelic
{
	private const int PerfectCombatsRequired = 2;

	private int _combatStartHp;

	private int _perfectCombatsTowardUpgrade;

	private bool _tookDamage;

	public override bool ShowCounter => PerfectCombatsTowardUpgrade > 0;

	public override int DisplayAmount => PerfectCombatsTowardUpgrade;

	[SavedProperty]
	public int PerfectCombatsTowardUpgrade
	{
		get
		{
			return _perfectCombatsTowardUpgrade;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_perfectCombatsTowardUpgrade = Math.Clamp(value, 0, 2);
			UpdateCounterVisuals();
		}
	}

	public override Task BeforeCombatStart()
	{
		_combatStartHp = ((RelicModel)this).Owner.Creature.CurrentHp;
		_tookDamage = false;
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == ((RelicModel)this).Owner.Creature && result.UnblockedDamage > 0)
		{
			_tookDamage = true;
		}
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		if (_tookDamage || ((RelicModel)this).Owner.Creature.CurrentHp < _combatStartHp)
		{
			return Task.CompletedTask;
		}
		PerfectCombatsTowardUpgrade++;
		if (PerfectCombatsTowardUpgrade < 2)
		{
			return Task.CompletedTask;
		}
		PerfectCombatsTowardUpgrade = 0;
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)6, ((RelicModel)this).Owner).Cards.Where((CardModel card) => card.IsUpgradable).ToList();
		if (list.Count == 0)
		{
			return Task.CompletedTask;
		}
		CardModel obj = ListExtensions.UnstableShuffle<CardModel>(list, ((RelicModel)this).Owner.RunState.Rng.Niche).First();
		((RelicModel)this).Flash();
		CardCmd.Upgrade(obj, (CardPreviewStyle)2);
		return Task.CompletedTask;
	}

	private void UpdateCounterVisuals()
	{
		((RelicModel)this).Status = (RelicStatus)(PerfectCombatsTowardUpgrade == 1);
		((RelicModel)this).InvokeDisplayAmountChanged();
	}
}
