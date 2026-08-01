using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class VisceraCrush : ValencinaPlaceholderCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => 1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Counters", 2m),
		new DynamicVar("Ammo", 1m)
	});

	public VisceraCrush()
		: base(2, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		await AmmoSystem.TryConsumeAsync((owner != null) ? owner.Creature : null, ((CardModel)this).DynamicVars["Ammo"].IntValue, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		Player owner2 = ((CardModel)this).Owner;
		InstantForesightPower instantForesightPower = ((owner2 != null) ? owner2.Creature.GetPower<InstantForesightPower>() : null);
		if (instantForesightPower != null)
		{
			Creature target = play.Target;
			Creature val = (Creature)((target != null && target.IsAlive) ? ((object)play.Target) : ((object)(from enemy in EnumerateOpponents()
				where enemy.IsAlive
				select enemy).OrderBy(StableCreatureKey).FirstOrDefault()));
			if (val != null)
			{
				await instantForesightPower.TriggerCounterAgainstImmediatelyAsync(choiceContext, val, ((CardModel)this).DynamicVars["Counters"].IntValue, fastAnimation: true);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Counters"].UpgradeValueBy(1m);
	}

	private static string StableCreatureKey(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name;
			}
		}
		return (string)obj;
	}
}
