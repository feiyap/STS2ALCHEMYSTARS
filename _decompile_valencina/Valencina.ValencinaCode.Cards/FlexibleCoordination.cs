using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Cards;

public sealed class FlexibleCoordination : ValencinaPlaceholderCard
{
	public override CardMultiplayerConstraint MultiplayerConstraint => (CardMultiplayerConstraint)1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar("Dodge", 5m, (ValueProp)8));

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)1;
		}
	}

	public FlexibleCoordination()
		: base(2, (CardType)2, (CardRarity)3, (TargetType)1, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		BlockVar dodge = (BlockVar)((CardModel)this).DynamicVars["Dodge"];
		foreach (Creature item in PlayerTeammates().ToList())
		{
			await MultiplayerCardHelpers.GainDodgeAsync(choiceContext, item, dodge, play);
		}
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if ((object)card == this)
		{
			return ((PileType)4, position);
		}
		return (pileType, position);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Dodge"].UpgradeValueBy(2m);
	}

	private IEnumerable<Creature> PlayerTeammates()
	{
		if (((CardModel)this).CombatState == null)
		{
			yield break;
		}
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null)
		{
			yield break;
		}
		foreach (Creature item in ((CardModel)this).CombatState.GetTeammatesOf(((CardModel)this).Owner.Creature))
		{
			if (item != null && item.IsAlive && item.IsPlayer)
			{
				yield return item;
			}
		}
	}
}
