using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Relics;

public sealed class AryaVijnanaRelic : ValencinaRelic
{
	private bool _triggeredThisCombat;

	public override RelicRarity Rarity => (RelicRarity)4;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new CardsVar(1),
		(DynamicVar)new PowerVar<StrengthPower>(1m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword((CardKeyword)1),
		CompatHoverTips.FromPower<StrengthPower>()
	});

	public override Task BeforeCombatStart()
	{
		_triggeredThisCombat = false;
		return Task.CompletedTask;
	}

	public async Task ValencinaBeforePlayPhaseStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (_triggeredThisCombat || player != ((RelicModel)this).Owner)
		{
			return;
		}
		ICombatState combatState = player.Creature.CombatState;
		if (combatState == null || combatState.RoundNumber != 1)
		{
			return;
		}
		_triggeredThisCombat = true;
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, ((RelicModel)this).Owner).Cards.ToList();
		int amount = list.Count;
		if (amount <= 0)
		{
			return;
		}
		((RelicModel)this).Flash();
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item, false, false);
		}
		await CardPileCmd.Draw(choiceContext, (decimal)amount, ((RelicModel)this).Owner, false);
		await CompatPowerCmd.Apply<StrengthPower>(choiceContext, ((RelicModel)this).Owner.Creature, (decimal)amount, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		_triggeredThisCombat = false;
		return Task.CompletedTask;
	}
}
