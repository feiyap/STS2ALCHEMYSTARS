using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Valencina.ValencinaCode.Enchantments;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class AdvisorsDecision : RienRelic
{
	private const string RewardsKey = "Rewards";

	private int _rewardsUsed;

	private bool _usedThisReward;

	public override bool HasUponPickupEffect => false;

	public override bool ShowCounter => RewardsUsed < ((RelicModel)this).DynamicVars["Rewards"].IntValue;

	public override int DisplayAmount => Math.Max(0, ((RelicModel)this).DynamicVars["Rewards"].IntValue - RewardsUsed);

	public override bool IsUsedUp => RewardsUsed >= ((RelicModel)this).DynamicVars["Rewards"].IntValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1] { (DynamicVar)new CardsVar("Rewards", 2) };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromEnchantment<InstantEnchantment>(1);

	[SavedProperty]
	public int RewardsUsed
	{
		get
		{
			return _rewardsUsed;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_rewardsUsed = value;
			((RelicModel)this).InvokeDisplayAmountChanged();
			if (((RelicModel)this).IsUsedUp)
			{
				((RelicModel)this).Status = (RelicStatus)2;
			}
		}
	}

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		_usedThisReward = false;
		if (player != ((RelicModel)this).Owner || RewardsUsed >= ((RelicModel)this).DynamicVars["Rewards"].IntValue)
		{
			return false;
		}
		bool flag = false;
		InstantEnchantment instantEnchantment = ModelDb.Enchantment<InstantEnchantment>();
		foreach (CardCreationResult cardReward in cardRewards)
		{
			CardModel card = cardReward.Card;
			if ((int)card.Type == 1 && ((EnchantmentModel)instantEnchantment).CanEnchant(card))
			{
				CardModel val = ((ICardScope)((RelicModel)this).Owner.RunState).CloneCard(card);
				CardCmd.Enchant<InstantEnchantment>(val, 1m);
				cardReward.ModifyCard(val, (RelicModel)(object)this);
				flag = true;
			}
		}
		_usedThisReward = flag;
		return flag;
	}

	public override Task AfterModifyingCardRewardOptions()
	{
		if (_usedThisReward && RewardsUsed < ((RelicModel)this).DynamicVars["Rewards"].IntValue)
		{
			RewardsUsed++;
		}
		_usedThisReward = false;
		return Task.CompletedTask;
	}
}
