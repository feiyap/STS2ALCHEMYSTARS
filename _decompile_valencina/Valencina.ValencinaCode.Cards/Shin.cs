using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class Shin : ValencinaCard
{
	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<BufferPower>(3m),
		(DynamicVar)new PowerVar<ShinAmmoRefundPower>(1m)
	};

	public Shin()
		: base(0, (CardType)3, (CardRarity)5, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(((CardModel)this).Owner.Creature, "Cast", ((CardModel)this).Owner.Character.CastAnimDelay);
		await CompatPowerCmd.Apply<BufferPower>(choiceContext, ((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars["BufferPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel?)(object)this, silent: false);
		await CompatPowerCmd.Apply<ShinAmmoRefundPower>(choiceContext, ((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars["ShinAmmoRefundPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel?)(object)this, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["ShinAmmoRefundPower"].UpgradeValueBy(1m);
	}
}
