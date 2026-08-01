using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class BoomerangShockwave : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(10m, (ValueProp)8),
		new DynamicVar("Tremor", 5m),
		new DynamicVar("Top", 0m)
	});

	public BoomerangShockwave()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)this, consumeStacks: true, choiceContext);
			await StatusSystem.ApplyTremorAsync(target, (int)((CardModel)this).DynamicVars["Tremor"].BaseValue, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
			await StatusSystem.TryConvertTremorToBurningAsync(target, (CardModel?)(object)this, choiceContext);
			await ExecuteAttackAsync(choiceContext, play);
			await AmmoSystem.AddAmmoAsync(((CardModel)this).Owner.Creature, 2, (CardModel?)(object)this, choiceContext);
		}
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		if ((object)card == this && IsCardUpgraded() && (int)pileType == 3)
		{
			return ((PileType)1, (CardPilePosition)2);
		}
		return (pileType, position);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Top"].UpgradeValueBy(1m);
	}
}
