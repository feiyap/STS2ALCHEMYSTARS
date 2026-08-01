using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Valencina.ValencinaCode.Powers;

public sealed class TemporaryThornsPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side != side)
		{
			ThornsPower power = ((PowerModel)this).Owner.GetPower<ThornsPower>();
			if (power != null)
			{
				await PowerCmd.ModifyAmount((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (PowerModel)(object)power, (decimal)(-((PowerModel)this).Amount), ((PowerModel)this).Owner, (CardModel)null, false);
			}
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
