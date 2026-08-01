using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class PressuringYou : ValencinaPlaceholderCard
{
	public override CardMultiplayerConstraint MultiplayerConstraint => (CardMultiplayerConstraint)1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Amount", 6m),
		new DynamicVar("Vulnerable", 1m)
	});

	public PressuringYou()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)6)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null && IsPlayerAllyTarget(target))
		{
			await CompatPowerCmd.Apply<BreathingMethodPower>(choiceContext, target, ((CardModel)this).DynamicVars["Amount"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel?)(object)this, silent: false);
			await CommonActions.Apply<VulnerablePower>(choiceContext, target, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Vulnerable"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(2m);
	}

	private bool IsPlayerAllyTarget(Creature target)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && target != ((CardModel)this).Owner.Creature && target.Player != null && target.Side == ((CardModel)this).Owner.Creature.Side)
		{
			return target.IsAlive;
		}
		return false;
	}
}
