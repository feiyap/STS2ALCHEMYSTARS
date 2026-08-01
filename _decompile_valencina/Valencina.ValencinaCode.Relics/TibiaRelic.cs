using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Relics;

public sealed class TibiaRelic : ValencinaRelic
{
	public override RelicRarity Rarity => (RelicRarity)3;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new HealVar(1m));

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == ((RelicModel)this).Owner && (int)cardPlay.Card.Type == 1 && cardPlay.Target != null && cardPlay.Target.IsMonster && ((RelicModel)this).Owner.Creature.CurrentHp * 2 < ((RelicModel)this).Owner.Creature.MaxHp)
		{
			((RelicModel)this).Flash();
			await CreatureCmd.Heal(((RelicModel)this).Owner.Creature, (decimal)((DynamicVar)((RelicModel)this).DynamicVars.Heal).IntValue, true);
		}
	}
}
