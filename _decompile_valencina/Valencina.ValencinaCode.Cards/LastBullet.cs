using System.Collections.Generic;
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

public sealed class LastBullet : ValencinaPlaceholderCard
{
	public override bool SpendsAmmo => true;

	public override string AmmoSpendPreviewText => "X";

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(24m, (ValueProp)8),
		new DynamicVar("PerAmmo", 4m)
	});

	public LastBullet()
		: base(3, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		Creature target = play.Target;
		if (target != null)
		{
			int currentAmmo = AmmoSystem.CurrentAmmo(owner.Creature);
			decimal damage = ReadDamageValue() + (decimal)currentAmmo * ((CardModel)this).DynamicVars["PerAmmo"].BaseValue;
			await ExecuteAttackAsync(choiceContext, target, damage, 1, "vfx/vfx_attack_slash");
			if (currentAmmo > 0)
			{
				await AmmoSystem.TryConsumeAsync(owner.Creature, currentAmmo, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(6m);
		((CardModel)this).DynamicVars["PerAmmo"].UpgradeValueBy(1m);
	}
}
