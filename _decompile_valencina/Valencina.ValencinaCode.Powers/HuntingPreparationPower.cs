using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class HuntingPreparationPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Cards", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		List<CardModel> list = (from card in PileTypeExtensions.GetPile((PileType)2, player).Cards
			where card.IsUpgradable
			orderby (!DisposalCostSystem.IsAnyDisposalVariant(card)) ? 1 : 0
			select card).ToList();
		if (list.Count > 0)
		{
			((PowerModel)this).Flash();
			int count = Math.Min(list.Count, Math.Max(0, ((PowerModel)this).Amount));
			foreach (CardModel item in list.Take(count).ToList())
			{
				CardCmd.Upgrade(item, (CardPreviewStyle)2);
			}
		}
		await PowerCmd.Remove((PowerModel)(object)this);
	}
}
