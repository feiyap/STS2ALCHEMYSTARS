using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class BurnedMemories : ValencinaCard
{
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

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Amount", 1m),
		(DynamicVar)new CardsVar(2)
	});

	public BurnedMemories()
		: base(0, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		List<CardModel> statuses = ValencinaCombatCardHelper.StatusCardsInCombatPiles(((CardModel)this).Owner).ToList();
		foreach (CardModel item in statuses)
		{
			await CardCmd.Exhaust(choiceContext, item, false, false);
		}
		await CardPileCmd.Draw(choiceContext, (decimal)statuses.Count + ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
		int num = statuses.Count * (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		if (num > 0)
		{
			await CreatureCmd.Heal(((CardModel)this).Owner.Creature, (decimal)num, true);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
