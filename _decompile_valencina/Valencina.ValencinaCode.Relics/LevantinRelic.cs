using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Relics;

public sealed class LevantinRelic : ValencinaRelic
{
	public override RelicRarity Rarity => (RelicRarity)4;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<BurnPower>(2m),
		new DynamicVar("Turn", 4m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(CompatHoverTips.FromPower<BurnPower>());

	public static int ModifyBurnAmount(int amount, CardModel? sourceCard)
	{
		if (amount <= 0 || ((sourceCard != null) ? sourceCard.Owner : null) == null)
		{
			return amount;
		}
		LevantinRelic relic = sourceCard.Owner.GetRelic<LevantinRelic>();
		if (relic == null)
		{
			return amount;
		}
		((RelicModel)relic).Flash();
		ICombatState combatState = sourceCard.Owner.Creature.CombatState;
		if (((combatState == null) ? 1 : combatState.RoundNumber) < 4)
		{
			return amount + 2;
		}
		return amount * 2;
	}
}
