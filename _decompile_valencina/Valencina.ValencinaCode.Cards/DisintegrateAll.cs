using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class DisintegrateAll : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(15m, (ValueProp)8),
		new DynamicVar("Ammo", 3m)
	});

	public DisintegrateAll()
		: base(2, (CardType)1, (CardRarity)2, (TargetType)3)
	{
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if ((object)cardSource != this || !ValuePropExtensions.IsPoweredAttack(props) || target == null)
		{
			return 1m;
		}
		if (CountDebuffPowers(target) <= 0)
		{
			return 0m;
		}
		return 1m;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		List<Creature> targets = (from creature in EnumerateOpponents()
			where CountDebuffPowers(creature) > 0
			select creature).ToList();
		if (targets.Count != 0)
		{
			Player owner = ((CardModel)this).Owner;
			Creature owner2 = ((owner != null) ? owner.Creature : null);
			await ExecuteAttackAllEnemiesAsync(choiceContext, 1, "vfx/vfx_attack_slash");
			await AmmoSystem.AddAmmoAsync(owner2, targets.Count * ((CardModel)this).DynamicVars["Ammo"].IntValue, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(5m);
	}
}
