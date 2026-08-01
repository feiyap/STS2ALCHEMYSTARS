using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Cards;

public sealed class Sadism : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(8m, (ValueProp)8),
		new DynamicVar("PerDebuff", 4m)
	});

	public Sadism()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			int num = CountDebuffCardsPlayedThisTurnBeforeThis();
			decimal damage = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue + (decimal)num * ((CardModel)this).DynamicVars["PerDebuff"].BaseValue;
			await ExecuteAttackAsync(choiceContext, target, damage, 1, "vfx/vfx_attack_slash");
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
		((CardModel)this).DynamicVars["PerDebuff"].UpgradeValueBy(1m);
	}

	private int CountDebuffCardsPlayedThisTurnBeforeThis()
	{
		if (((CardModel)this).CombatState == null || ((CardModel)this).Owner == null)
		{
			return 0;
		}
		return CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry entry) => ((CombatHistoryEntry)entry).HappenedThisTurn(((CardModel)this).CombatState) && entry.CardPlay.Card.Owner == ((CardModel)this).Owner && IsDebuffApplyingCard(entry.CardPlay.Card));
	}

	private static bool IsDebuffApplyingCard(CardModel card)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		string entry = ((AbstractModel)card).Id.Entry;
		if ((int)card.Type != 3)
		{
			if (!entry.Contains("TREMOR") && !entry.Contains("BURN") && !entry.Contains("MAGGOT") && !entry.Contains("ANNOYING") && !entry.Contains("PIERCING_BULLET") && !entry.Contains("BOOMERANG_SHOCKWAVE") && !entry.Contains("VIBRATING_BLADE") && !entry.Contains("DISMEMBER") && !entry.Contains("TAKE_AIM") && !entry.Contains("HIGH_TEMPERATURE") && !entry.Contains("ACCUMULATED_EXPERIENCE") && !entry.Contains("MAIM") && !entry.Contains("ACHILLES_TEAR"))
			{
				return entry.Contains("WEAKPOINT_DETONATION");
			}
			return true;
		}
		return false;
	}
}
