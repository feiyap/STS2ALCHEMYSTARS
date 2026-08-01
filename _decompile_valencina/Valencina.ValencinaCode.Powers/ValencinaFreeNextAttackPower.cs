using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class ValencinaFreeNextAttackPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (!IsEligible(card))
		{
			return false;
		}
		modifiedCost = default(decimal);
		return true;
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (IsEligible(cardPlay.Card))
		{
			await PowerCmd.Decrement((PowerModel)(object)this);
		}
	}

	private bool IsEligible(CardModel card)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Invalid comparison between Unknown and I4
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Invalid comparison between Unknown and I4
		if (((PowerModel)this).Owner != null && ((PowerModel)this).Amount > 0)
		{
			Player owner = card.Owner;
			if (((owner != null) ? owner.Creature : null) == ((PowerModel)this).Owner && (int)card.Type == 1 && !DisposalCostSystem.IsAnyDisposalVariant(card))
			{
				CardPile pile = card.Pile;
				PileType? val = ((pile != null) ? new PileType?(pile.Type) : ((PileType?)null));
				if (val.HasValue)
				{
					PileType valueOrDefault = val.GetValueOrDefault();
					if ((int)valueOrDefault == 2 || (int)valueOrDefault == 5)
					{
						return true;
					}
				}
				return false;
			}
		}
		return false;
	}
}
