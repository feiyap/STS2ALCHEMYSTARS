using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class ScorchingEyeSocket : ValencinaPlaceholderPowerCard
{
	public override string CustomPortraitPath => "scorching_eye_socket.png".BigCardImagePath();

	public override string PortraitPath => "scorching_eye_socket.png".CardImagePath();

	public override string BetaPortraitPath => "scorching_eye_socket.png".CardImagePath();

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new EnergyVar(2),
		(DynamicVar)new CardsVar(3),
		new DynamicVar("Loss", 5m)
	});

	public ScorchingEyeSocket()
		: base(0, (CardRarity)4)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await PlayerCmd.GainEnergy(IsCardUpgraded() ? 3m : 2m, ((CardModel)this).Owner);
		await CardPileCmd.Draw(choiceContext, 3m, ((CardModel)this).Owner, false);
		await CommonActions.ApplySelf<ScorchingEyeSocketPower>(choiceContext, (CardModel)(object)this, 5m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Energy).UpgradeValueBy(1m);
	}
}
