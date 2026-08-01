using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class HemostasisPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void SetStacks(int amount)
	{
		if (amount < 0)
		{
			amount = 0;
		}
		((PowerModel)this).SetAmount(amount, false);
		((PowerModel)this).InitInternalData();
		((PowerModel)this).InvokeDisplayAmountChanged();
	}

	public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldPreventHpLoss(target, amount, props, dealer, cardSource))
		{
			return amount;
		}
		((PowerModel)this).Flash();
		Logger logger = MainFile.Logger;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
		defaultInterpolatedStringHandler.AppendLiteral("[HemostasisPower] Prevented ");
		defaultInterpolatedStringHandler.AppendFormatted(amount);
		defaultInterpolatedStringHandler.AppendLiteral(" non-attack HP loss on ");
		Creature owner = ((PowerModel)this).Owner;
		defaultInterpolatedStringHandler.AppendFormatted((owner != null) ? owner.Name : null);
		defaultInterpolatedStringHandler.AppendLiteral(".");
		logger.Info(defaultInterpolatedStringHandler.ToStringAndClear(), 1);
		return 0m;
	}

	public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) == player && ((PowerModel)this).Amount > 0)
		{
			int num = ((PowerModel)this).Amount - 1;
			if (num <= 0)
			{
				await PowerCmd.Remove((PowerModel)(object)this);
			}
			else
			{
				SetStacks(num);
			}
		}
	}

	private bool ShouldPreventHpLoss(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || target != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || amount <= 0m)
		{
			return false;
		}
		if (cardSource != null && (int)cardSource.Type == 1)
		{
			return false;
		}
		if (dealer != null && dealer != ((PowerModel)this).Owner && ValuePropExtensions.IsPoweredAttack(props))
		{
			return false;
		}
		return true;
	}
}
