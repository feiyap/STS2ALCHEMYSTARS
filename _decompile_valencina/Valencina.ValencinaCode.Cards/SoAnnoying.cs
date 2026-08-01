using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class SoAnnoying : ValencinaCard
{
	public override TargetType TargetType
	{
		get
		{
			if (IsCardUpgraded())
			{
				return (TargetType)3;
			}
			return (TargetType)2;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		new DynamicVar("Amount", 5m),
		new DynamicVar("Threshold", 10m),
		new DynamicVar("All", 0m)
	});

	public SoAnnoying()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		if (IsCardUpgraded())
		{
			foreach (Creature item in EnumerateOpponents())
			{
				await ApplyScalingTremor(choiceContext, item);
			}
		}
		else
		{
			Creature target = play.Target;
			if (target != null)
			{
				await ApplyScalingTremor(choiceContext, target);
			}
		}
	}

	private async Task ApplyScalingTremor(PlayerChoiceContext choiceContext, Creature target)
	{
		int num = StatusSystem.TremorAmount(target);
		int num2 = (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		int num3 = Math.Max(0, num / (int)((CardModel)this).DynamicVars["Threshold"].BaseValue);
		int amount = num2 + num3 * num2;
		await StatusSystem.ApplyTremorAsync(target, amount, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["All"].UpgradeValueBy(1m);
	}
}
