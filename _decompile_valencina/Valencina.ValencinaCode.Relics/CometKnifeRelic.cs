using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Relics;

public sealed class CometKnifeRelic : ValencinaRelic
{
	private int _debuffCardsPlayed;

	private int _currentPlaySerial;

	private int _countedPlaySerial;

	private CardModel? _currentCard;

	public override RelicRarity Rarity => (RelicRarity)4;

	public override bool ShowCounter => CombatManager.Instance.IsInProgress;

	public override int DisplayAmount => _debuffCardsPlayed;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new CardsVar(6),
		(DynamicVar)new PowerVar<AfterimagePower>(1m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(CompatHoverTips.FromPower<AfterimagePower>());

	public override Task BeforeCombatStart()
	{
		ResetCounter();
		return Task.CompletedTask;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == ((RelicModel)this).Owner)
		{
			_currentCard = cardPlay.Card;
			_currentPlaySerial++;
			_countedPlaySerial = 0;
		}
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == _currentCard)
		{
			_currentCard = null;
		}
		return Task.CompletedTask;
	}

	public async Task ValencinaAfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (_currentCard != null && cardSource == _currentCard && _countedPlaySerial != _currentPlaySerial && !(amount <= 0m) && applier == ((RelicModel)this).Owner.Creature && power.Owner != ((RelicModel)this).Owner.Creature && (int)power.Type == 2)
		{
			_countedPlaySerial = _currentPlaySerial;
			_debuffCardsPlayed++;
			UpdateCounterVisuals();
			int intValue = ((DynamicVar)((RelicModel)this).DynamicVars.Cards).IntValue;
			if (_debuffCardsPlayed >= intValue)
			{
				_debuffCardsPlayed = 0;
				UpdateCounterVisuals();
				((RelicModel)this).Flash();
				await CompatPowerCmd.Apply<AfterimagePower>(choiceContext, ((RelicModel)this).Owner.Creature, ((RelicModel)this).DynamicVars["AfterimagePower"].BaseValue, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		ResetCounter();
		return Task.CompletedTask;
	}

	private void ResetCounter()
	{
		_debuffCardsPlayed = 0;
		_currentCard = null;
		_currentPlaySerial = 0;
		_countedPlaySerial = 0;
		UpdateCounterVisuals();
	}

	private void UpdateCounterVisuals()
	{
		int intValue = ((DynamicVar)((RelicModel)this).DynamicVars.Cards).IntValue;
		((RelicModel)this).Status = (RelicStatus)((_debuffCardsPlayed == intValue - 1) ? 1 : 0);
		((RelicModel)this).InvokeDisplayAmountChanged();
	}
}
