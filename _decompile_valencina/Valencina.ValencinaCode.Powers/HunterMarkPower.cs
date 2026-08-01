using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class HunterMarkPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (((PowerModel)this).Owner == null || target != ((PowerModel)this).Owner || dealer == null || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		if (cardSource != null && (int)cardSource.Type == 1 && result.TotalDamage > 0 && dealer.GetPower<NoDodgeGainPower>() == null)
		{
			InstantForesightPower power = dealer.GetPower<InstantForesightPower>();
			if (power != null)
			{
				((PowerModel)this).Flash();
				power.GainTemporaryDodgeThreshold(((PowerModel)this).Amount);
				await Task.CompletedTask;
			}
		}
	}
}
