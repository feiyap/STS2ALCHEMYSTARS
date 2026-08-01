using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public abstract class CounterStyleCard : ValencinaCard, ICounterStyleCard, IInstantAttackCard
{
	public ValencinaCounterStyle Style => _003Cstyle_003EP;

	public override bool CanBeGeneratedInCombat => false;

	public override bool CanBeGeneratedByModifiers => false;

	public int InstantAmmoCost => CurrentAmmoCost;

	public override bool SpendsAmmo => CurrentAmmoCost > 0;

	public override int AmmoSpendPreviewAmount => CurrentAmmoCost;

	public int CurrentDamage
	{
		get
		{
			if (!IsCardUpgraded())
			{
				return _003Cdamage_003EP;
			}
			return _003CupgradedDamage_003EP;
		}
	}

	public int CurrentAmmoCost
	{
		get
		{
			if (!IsCardUpgraded())
			{
				return _003CammoCost_003EP;
			}
			return _003CupgradedAmmoCost_003EP;
		}
	}

	public override string CustomPortraitPath => _003CportraitName_003EP.BigCardImagePath();

	public override string PortraitPath => _003CportraitName_003EP.CardImagePath();

	public override string BetaPortraitPath => ("beta/" + _003CportraitName_003EP).CardImagePath();

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar((decimal)_003Cdamage_003EP, (ValueProp)8),
		new DynamicVar("Ammo", (decimal)_003CammoCost_003EP)
	});

	protected CounterStyleCard(ValencinaCounterStyle style, int damage, int upgradedDamage, int ammoCost = 0, int upgradedAmmoCost = 0, string portraitName = "card.png", CardRarity rarity = (CardRarity)3)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		_003Cstyle_003EP = style;
		_003Cdamage_003EP = damage;
		_003CupgradedDamage_003EP = upgradedDamage;
		_003CammoCost_003EP = ammoCost;
		_003CupgradedAmmoCost_003EP = upgradedAmmoCost;
		_003CportraitName_003EP = portraitName;
		base._002Ector(0, (CardType)1, rarity, (TargetType)2);
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy((decimal)(_003CupgradedDamage_003EP - _003Cdamage_003EP));
		((CardModel)this).DynamicVars["Ammo"].UpgradeValueBy((decimal)(_003CupgradedAmmoCost_003EP - _003CammoCost_003EP));
	}
}
