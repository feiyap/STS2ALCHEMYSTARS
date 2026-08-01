using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class SomeonesBlueBlade : RienRelic
{
	private const string BlockKey = "Block";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("Block", 10m)
	};

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && side == ((RelicModel)this).Owner.Creature.Side && ((RelicModel)this).Owner.Creature.CombatState != null && ((RelicModel)this).Owner.Creature.IsAlive && BreathingMethodStateHelper.GetAmount(((RelicModel)this).Owner.Creature) > 0)
		{
			((RelicModel)this).Flash();
			await CreatureCmd.GainBlock(((RelicModel)this).Owner.Creature, ((RelicModel)this).DynamicVars["Block"].BaseValue, (ValueProp)4, (CardPlay)null, false);
		}
	}
}
