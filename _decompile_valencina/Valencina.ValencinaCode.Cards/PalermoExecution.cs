using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class PalermoExecution : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Counters", 0m));

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			HashSet<CardKeyword> emitted = new HashSet<CardKeyword>();
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				if (emitted.Add(canonicalKeyword))
				{
					yield return canonicalKeyword;
				}
			}
			if (!IsCardUpgraded() && emitted.Add((CardKeyword)1))
			{
				yield return (CardKeyword)1;
			}
		}
	}

	public PalermoExecution()
		: base(3, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		InstantForesightPower power = ((CardModel)this).Owner.Creature.GetPower<InstantForesightPower>();
		if (power != null)
		{
			Creature target = play.Target;
			Creature val = (Creature)((target != null && target.IsAlive) ? ((object)play.Target) : ((object)(from enemy in EnumerateOpponents()
				where enemy.IsAlive
				select enemy).OrderBy(ValencinaCardStableKeys.Creature).FirstOrDefault()));
			if (val != null)
			{
				int times = SyncCounterPreviewAmount(power);
				await power.TriggerCounterAgainstImmediatelyAsync(choiceContext, val, times, fastAnimation: true);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).RemoveKeyword((CardKeyword)1);
	}

	public int SyncCounterPreviewAmount()
	{
		if (!((AbstractModel)this).IsMutable)
		{
			return 0;
		}
		InstantForesightPower instantForesightPower = null;
		try
		{
			Player owner = ((CardModel)this).Owner;
			object obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				Creature creature = owner.Creature;
				obj = ((creature != null) ? creature.GetPower<InstantForesightPower>() : null);
			}
			instantForesightPower = (InstantForesightPower)obj;
		}
		catch
		{
			instantForesightPower = null;
		}
		return SyncCounterPreviewAmount(instantForesightPower);
	}

	private int SyncCounterPreviewAmount(InstantForesightPower? power)
	{
		int num = Math.Max(0, power?.SuccessfulCounterCountThisCombat ?? 0);
		((CardModel)this).DynamicVars["Counters"].BaseValue = num;
		return num;
	}
}
