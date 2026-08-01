using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

public sealed class HermesAgentRelic : ValencinaRelic
{
	private readonly HashSet<string> _uniqueAttacksThisCombat = new HashSet<string>();

	private bool _pendingExtraTurn;

	private int _temporaryStrengthDelta;

	public override RelicRarity Rarity => (RelicRarity)4;

	public override bool ShowCounter => CombatManager.Instance.IsInProgress;

	public override int DisplayAmount => _uniqueAttacksThisCombat.Count;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		new DynamicVar("MinStrength", -1m),
		new DynamicVar("MaxStrength", 3m),
		(DynamicVar)new CardsVar(9)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(CompatHoverTips.FromPower<StrengthPower>());

	public override Task BeforeCombatStart()
	{
		ResetCombatState();
		return Task.CompletedTask;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == ((RelicModel)this).Owner)
		{
			int num = ((RelicModel)this).Owner.RunState.Rng.Niche.NextInt(-1, 4);
			if (num != 0)
			{
				_temporaryStrengthDelta += num;
				((RelicModel)this).Flash();
				await CompatPowerCmd.Apply<StrengthPower>(choiceContext, ((RelicModel)this).Owner.Creature, (decimal)num, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((RelicModel)this).Owner.Creature.Side && _temporaryStrengthDelta != 0)
		{
			int temporaryStrengthDelta = _temporaryStrengthDelta;
			_temporaryStrengthDelta = 0;
			await CompatPowerCmd.Apply<StrengthPower>(choiceContext, ((RelicModel)this).Owner.Creature, (decimal)(-temporaryStrengthDelta), ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
		}
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		if (cardPlay.Card.Owner != ((RelicModel)this).Owner || (int)cardPlay.Card.Type != 1)
		{
			return Task.CompletedTask;
		}
		if (!_uniqueAttacksThisCombat.Add(((object)((AbstractModel)cardPlay.Card).Id).ToString()))
		{
			return Task.CompletedTask;
		}
		UpdateCounterVisuals();
		if (_uniqueAttacksThisCombat.Count >= ((DynamicVar)((RelicModel)this).DynamicVars.Cards).IntValue && !_pendingExtraTurn)
		{
			_uniqueAttacksThisCombat.Clear();
			_pendingExtraTurn = true;
			UpdateCounterVisuals();
			((RelicModel)this).Status = (RelicStatus)1;
			((RelicModel)this).Flash();
		}
		return Task.CompletedTask;
	}

	public override bool ShouldTakeExtraTurn(Player player)
	{
		if (_pendingExtraTurn)
		{
			return player == ((RelicModel)this).Owner;
		}
		return false;
	}

	public override Task AfterTakingExtraTurn(Player player)
	{
		if (player == ((RelicModel)this).Owner)
		{
			_pendingExtraTurn = false;
			((RelicModel)this).Status = (RelicStatus)0;
			((RelicModel)this).InvokeDisplayAmountChanged();
		}
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		ResetCombatState();
		return Task.CompletedTask;
	}

	private void ResetCombatState()
	{
		_uniqueAttacksThisCombat.Clear();
		_pendingExtraTurn = false;
		_temporaryStrengthDelta = 0;
		UpdateCounterVisuals();
	}

	private void UpdateCounterVisuals()
	{
		int intValue = ((DynamicVar)((RelicModel)this).DynamicVars.Cards).IntValue;
		((RelicModel)this).Status = (RelicStatus)((_uniqueAttacksThisCombat.Count == intValue - 1) ? 1 : 0);
		((RelicModel)this).InvokeDisplayAmountChanged();
	}
}
