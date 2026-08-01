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
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class VibratingBlade : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		new DynamicVar("NoConsume", 1m),
		new DynamicVar("Instant", 0m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			if (IsCardUpgraded())
			{
				yield return ValencinaKeywords.Instant;
			}
		}
	}

	public VibratingBlade()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target == null)
		{
			return;
		}
		Player owner = ((CardModel)this).Owner;
		await ValencinaAttackScope.RunAsync((owner != null) ? owner.Creature : null, IsCardUpgraded(), async delegate
		{
			if (IsCardUpgraded())
			{
				Player owner2 = ((CardModel)this).Owner;
				Creature owner3 = ((owner2 != null) ? owner2.Creature : null);
				Player owner4 = ((CardModel)this).Owner;
				InstantAttackBreathingMethodRegistry.Begin(owner3, BreathingMethodStateHelper.GetAmount((owner4 != null) ? owner4.Creature : null));
			}
			try
			{
				int num = await ExecuteAttackAndGetUnblockedDamageAsync(choiceContext, target, 1, "vfx/vfx_attack_slash");
				if (num > 0)
				{
					await StatusSystem.ApplyTremorAsync(target, num, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
				}
				await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)this, consumeStacks: false, choiceContext);
			}
			finally
			{
				if (IsCardUpgraded())
				{
					Player owner5 = ((CardModel)this).Owner;
					InstantAttackBreathingMethodRegistry.End((owner5 != null) ? owner5.Creature : null);
				}
			}
		});
	}

	protected override void OnUpgrade()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
		((CardModel)this).DynamicVars["Instant"].UpgradeValueBy(1m);
		((CardModel)this).AddKeyword(ValencinaKeywords.Instant);
	}
}
