using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class HighPressure : ValencinaPlaceholderCard
{
	public override string CustomPortraitPath => "high_pressure.png".BigCardImagePath();

	public override string PortraitPath => "high_pressure.png".CardImagePath();

	public override string BetaPortraitPath => "high_pressure.png".CardImagePath();

	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => 1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new CardsVar(2));

	public HighPressure()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, AmmoSpendPreviewAmount, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1m);
	}
}
