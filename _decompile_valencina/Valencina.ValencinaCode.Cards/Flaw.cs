using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Cards;

public sealed class Flaw : ValencinaCard
{
	private const string HpLossKey = "HpLoss";

	protected override bool IsPlayable => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar("HpLoss", 5m, (ValueProp)6));

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)2;
		}
	}

	public Flaw()
		: base(-2, (CardType)4, (CardRarity)8, (TargetType)0, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		return Task.CompletedTask;
	}

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && ((CardModel)this).Owner.Creature.Side == side)
		{
			CardPile pile = ((CardModel)this).Pile;
			if (pile != null && (int)pile.Type == 2)
			{
				await CreatureCmd.Damage(choiceContext, ((CardModel)this).Owner.Creature, (DamageVar)((CardModel)this).DynamicVars["HpLoss"], (Creature)null, (CardModel)(object)this);
			}
		}
	}
}
