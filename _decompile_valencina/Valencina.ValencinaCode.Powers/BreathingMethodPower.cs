using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Enchantments;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Powers;

public sealed class BreathingMethodPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private sealed class RawAmountConversionScope : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				RawAmountChangeDepth.Value = Math.Max(0, RawAmountChangeDepth.Value - 1);
			}
		}
	}

	public const int IntensityUnit = 10000;

	private static readonly AsyncLocal<int> RawAmountChangeDepth = new AsyncLocal<int>();

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override int DisplayAmount => Charges;

	public int RawIntensity => Math.Max(0, ((PowerModel)this).Amount / 10000);

	public int Charges => Math.Max(0, ((PowerModel)this).Amount % 10000);

	public int Intensity
	{
		get
		{
			if (Charges <= 0)
			{
				return 0;
			}
			return Math.Max(1, RawIntensity);
		}
	}

	public static int Encode(int intensity, int charges)
	{
		intensity = Math.Max(0, intensity);
		charges = Math.Clamp(charges, 0, 9999);
		return intensity * 10000 + charges;
	}

	public static IDisposable SuppressLegacyRawAmountConversion()
	{
		RawAmountChangeDepth.Value++;
		return new RawAmountConversionScope();
	}

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Intensity", (decimal)Intensity);
		description.Add("Charges", (decimal)Charges);
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (((PowerModel)this).Amount > 0 && ((PowerModel)this).Amount < 10000)
		{
			await PowerCmd.ModifyAmount((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (PowerModel)(object)this, (decimal)(Encode(((PowerModel)this).Amount, 1) - ((PowerModel)this).Amount), (Creature)null, (CardModel)null, false);
		}
	}

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (RawAmountChangeDepth.Value > 0 || !(canonicalPower is BreathingMethodPower) || target != ((PowerModel)this).Owner || amount <= 0m || amount >= 10000m)
		{
			return false;
		}
		int num = Math.Max(0, (int)amount);
		modifiedAmount = ((Charges > 0) ? (num * 10000) : Encode(num, 1));
		return true;
	}

	public static async Task ConsumeAsync(PlayerChoiceContext choiceContext, Creature? owner, int amount, CardModel? sourceCard)
	{
		BreathingMethodPower breathingMethodPower = CreaturePowerAccess.Find<BreathingMethodPower>(owner);
		if (breathingMethodPower != null)
		{
			await breathingMethodPower.ConsumeChargesAsync(choiceContext, amount, sourceCard);
		}
	}

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		if (((PowerModel)this).Owner != dealer || Charges <= 0 || !ValuePropExtensions.IsPoweredAttack(props) || cardSource == null || (int)cardSource.Type != 1)
		{
			return 0m;
		}
		decimal num = ValencinaAttackScope.BreathingMethodDamageMultiplier(((PowerModel)this).Owner);
		if (cardSource is CuttingSword)
		{
			num *= 2m;
		}
		return (decimal)Intensity * num;
	}

	public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Charges > 0)
		{
			CardModel card = cardPlay.Card;
			object obj;
			if (card == null)
			{
				obj = null;
			}
			else
			{
				Player owner = card.Owner;
				obj = ((owner != null) ? owner.Creature : null);
			}
			if (obj == ((PowerModel)this).Owner && (int)cardPlay.Card.Type == 1 && !IsInstantAttack(cardPlay.Card) && !ValencinaAttackScope.ShouldSuppressBreathingMethodAfterAttack(((PowerModel)this).Owner))
			{
				await ConsumeChargesAsync(choiceContext, 1, cardPlay.Card);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Charges > 0)
		{
			Creature owner = ((PowerModel)this).Owner;
			if (owner != null && owner.Side == side)
			{
				await ConsumeChargesAsync(choiceContext, 1, null);
			}
		}
	}

	private async Task ConsumeChargesAsync(PlayerChoiceContext choiceContext, int amount, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && amount > 0 && Charges > 0)
		{
			int consumed = Math.Min(amount, Charges);
			if (consumed < Charges)
			{
				await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)this, (decimal)(-consumed), (Creature)null, (CardModel)null, false);
			}
			else
			{
				await PowerCmd.Remove((PowerModel)(object)this);
			}
			await NotifyBreathingMethodConsumedAsync(choiceContext, consumed, sourceCard);
		}
	}

	private static bool IsInstantAttack(CardModel card)
	{
		if (!(card is IInstantAttackCard) && (!(card is VibratingBlade) || !card.IsUpgraded))
		{
			return card.Enchantment is InstantEnchantment;
		}
		return true;
	}

	private async Task NotifyBreathingMethodConsumedAsync(PlayerChoiceContext choiceContext, int consumed, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner == null || consumed <= 0)
		{
			return;
		}
		List<IBreathingMethodConsumedListener> list = CreaturePowerAccess.Enumerate(((PowerModel)this).Owner).OfType<IBreathingMethodConsumedListener>().OrderBy(StableListenerKey)
			.ToList();
		foreach (IBreathingMethodConsumedListener item in list)
		{
			await item.OnBreathingMethodConsumedAsync(choiceContext, consumed, ((PowerModel)this).Owner, sourceCard);
		}
	}

	private static string StableListenerKey(object listener)
	{
		PowerModel val = (PowerModel)((listener is PowerModel) ? listener : null);
		string text;
		if (val == null)
		{
			text = listener.GetType().FullName;
			if (text == null)
			{
				return listener.GetType().Name;
			}
		}
		else
		{
			Creature owner = val.Owner;
			text = (((owner == null) ? null : owner.CombatId?.ToString("D10")) ?? "no-owner") + ":" + ((AbstractModel)val).Id.Entry;
		}
		return text;
	}
}
