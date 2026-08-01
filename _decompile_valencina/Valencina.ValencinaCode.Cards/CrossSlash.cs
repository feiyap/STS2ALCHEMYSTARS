using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class CrossSlash : ValencinaCard, IInstantAttackCard
{
	public int InstantAmmoCost => 3;

	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => InstantAmmoCost;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(1m, (ValueProp)8),
		new DynamicVar("Amount", 0m)
	});

	public CrossSlash()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)3, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await InstantAttackHelper.ExecuteAgainstPlayAsync(this, choiceContext, play, 2);
		if (!(((CardModel)this).DynamicVars["Amount"].BaseValue > 0m))
		{
			return;
		}
		foreach (Creature item in EnumerateOpponents().OrderBy(ValencinaCardStableKeys.Creature))
		{
			await CommonActions.Apply<VulnerablePower>(choiceContext, item, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Amount"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
