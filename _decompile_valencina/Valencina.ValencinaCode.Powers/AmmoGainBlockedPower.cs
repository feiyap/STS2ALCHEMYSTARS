using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class AmmoGainBlockedPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side && ((PowerModel)this).Amount > 0)
		{
			int num = ((PowerModel)this).Amount - 1;
			if (num <= 0)
			{
				await PowerCmd.Remove((PowerModel)(object)this);
				return;
			}
			((PowerModel)this).SetAmount(num, false);
			((PowerModel)this).InitInternalData();
			((PowerModel)this).InvokeDisplayAmountChanged();
		}
	}
}
