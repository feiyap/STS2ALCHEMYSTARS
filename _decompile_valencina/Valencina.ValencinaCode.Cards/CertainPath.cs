using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class CertainPath : ValencinaPlaceholderCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount
	{
		get
		{
			if (!IsCardUpgraded())
			{
				return 2;
			}
			return 4;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Destined", 2m),
		new DynamicVar("Ammo", 2m)
	});

	public CertainPath()
		: base(2, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		await AmmoSystem.TryConsumeAsync((owner != null) ? owner.Creature : null, ((CardModel)this).DynamicVars["Ammo"].IntValue, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		await CommonActions.ApplySelf<DestinedFuturePower>(choiceContext, (CardModel)(object)this, ((CardModel)this).DynamicVars["Destined"].BaseValue, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Ammo"].UpgradeValueBy(2m);
	}
}
