using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class AccumulatedExperience : ValencinaCard, IBurnApplyingCard
{
	public override int MaxUpgradeLevel => 999;

	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => (int)((CardModel)this).DynamicVars["Amount"].BaseValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new CardsVar(1),
		new DynamicVar("Amount", 1m)
	});

	public AccumulatedExperience()
		: base(0, (CardType)2, (CardRarity)1, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int amount = (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		int value = await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, amount, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		MainFile.Logger.Info($"[AccumulatedExperience] consumed {value}/{amount} ammo. current={AmmoSystem.CurrentAmmo(((CardModel)this).Owner.Creature)}/{AmmoSystem.MaxAmmoFor(((CardModel)this).Owner.Creature)}", 1);
		await CommonActions.Draw((CardModel)(object)this, choiceContext);
		if (play.Target != null)
		{
			await StatusSystem.ApplyTremorAsync(play.Target, amount, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
			await StatusSystem.ApplyBurnAsync(play.Target, amount, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
