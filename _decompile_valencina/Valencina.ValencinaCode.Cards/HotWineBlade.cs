using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class HotWineBlade : ValencinaCard
{
	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Amount", 0m),
		new DynamicVar("Burn", 8m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)1;
		}
	}

	public HotWineBlade()
		: base(0, (CardType)2, (CardRarity)4, (TargetType)0)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int requested = ((CardModel)this).ResolveEnergyXValue() + ((CardModel)this).DynamicVars["Amount"].IntValue;
		int num = await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, requested, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		if (num <= 0)
		{
			return;
		}
		int burn = num * ((CardModel)this).DynamicVars["Burn"].IntValue;
		ICombatState combatState = ((CardModel)this).Owner.Creature.CombatState;
		foreach (Creature item in ((combatState != null) ? combatState.HittableEnemies.Where((Creature enemy) => enemy.IsAlive).ToList() : null) ?? new List<Creature>())
		{
			await StatusSystem.ApplyBurnAsync(item, burn, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
