using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class StopStaring : ValencinaCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => 3;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(8m, (ValueProp)8));

	public StopStaring()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		List<(Creature Creature, int Hp)> before = (from c in GetOpponentSnapshot()
			select (Creature: c, Hp: c.CurrentHp)).ToList();
		await ExecuteAttackAllEnemiesAsync(choiceContext, 1, "vfx/vfx_attack_slash");
		if (before.Any(((Creature Creature, int Hp) entry) => entry.Hp > 0 && entry.Creature.CurrentHp <= 0))
		{
			await ExecuteAttackAllEnemiesAsync(choiceContext, 1, "vfx/vfx_attack_slash");
		}
		await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, AmmoSpendPreviewAmount, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
	}
}
