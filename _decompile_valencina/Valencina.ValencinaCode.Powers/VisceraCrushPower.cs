using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class VisceraCrushPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || side == ((PowerModel)this).Owner.Side)
		{
			return;
		}
		InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
		if (power != null)
		{
			foreach (Creature item in power.SuccessfulCounterTargetsThisTurn.ToList())
			{
				if (item.IsAlive)
				{
					await StatusSystem.DetonateTremorAsync(item, null, consumeStacks: false, choiceContext);
				}
			}
		}
		await PowerCmd.Remove((PowerModel)(object)this);
	}
}
