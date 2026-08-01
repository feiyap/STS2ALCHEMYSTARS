using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Enchantments;

public sealed class InstantEnchantment : ModEnchantmentTemplate
{
	public override EnchantmentAssetProfile AssetProfile => new EnchantmentAssetProfile("res://Valencina/images/enchantments/instant_enchantment.png");

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Character<Valencina.ValencinaCode.Character.Valencina>();

	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(ValencinaKeywords.BreathingMethod));

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 1;
	}

	public override bool CanEnchant(CardModel card)
	{
		if (!(card is IInstantAttackCard))
		{
			return ((EnchantmentModel)this).CanEnchant(card);
		}
		return false;
	}

	public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (ValuePropExtensions.IsPoweredAttack(props) && ((EnchantmentModel)this).HasCard)
		{
			Player owner = ((EnchantmentModel)this).Card.Owner;
			if (((owner != null) ? owner.Creature : null) != null && (int)((EnchantmentModel)this).Status == 0)
			{
				int num = BreathingMethodService.GetIntensity(((EnchantmentModel)this).Card.Owner.Creature) + BreathingMethodService.GetCharges(((EnchantmentModel)this).Card.Owner.Creature);
				if (num <= 0)
				{
					return 0m;
				}
				return decimal.Floor(originalDamage * (decimal)num / 100m);
			}
		}
		return 0m;
	}
}
