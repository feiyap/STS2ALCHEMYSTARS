using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class WeakpointDetonation : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Amount", 1m),
		new DynamicVar("Convert", 0m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			if (IsCardUpgraded())
			{
				yield return ValencinaKeywords.AmplitudeConversion;
			}
		}
	}

	public WeakpointDetonation()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			if (IsCardUpgraded())
			{
				await StatusSystem.TryConvertTremorToBurningAsync(target, (CardModel?)(object)this, choiceContext);
			}
			await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)this, consumeStacks: false, choiceContext);
			await CommonActions.Apply<WeakPower>(choiceContext, target, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Amount"].BaseValue, silent: false);
			await CommonActions.Apply<VulnerablePower>(choiceContext, target, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Amount"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["Convert"].UpgradeValueBy(1m);
	}
}
