using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class PointBlankShot : ValencinaCard
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

	public PointBlankShot()
		: base(2, (CardType)2, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target == null)
		{
			return;
		}
		foreach (object item in ValencinaCard.EnumeratePowersOn(target))
		{
			if (item != null && (item is BurnPower || item is TremorPower || item is BurningTremorPower))
			{
				int num = (int)ValencinaCard.ReadPowerAmount(item);
				if (num > 0)
				{
					ValencinaCard.TrySetNumericAmount(item, num * 2);
				}
			}
		}
		await Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
