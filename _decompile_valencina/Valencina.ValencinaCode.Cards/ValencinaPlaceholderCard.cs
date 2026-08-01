using MegaCrit.Sts2.Core.Entities.Cards;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Cards;

public abstract class ValencinaPlaceholderCard : ValencinaCard
{
	public override string CustomPortraitPath => "card.png".BigCardImagePath();

	public override string PortraitPath => "card.png".CardImagePath();

	public override string BetaPortraitPath => "card.png".CardImagePath();

	protected ValencinaPlaceholderCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
		: base(cost, type, rarity, target, showInCardLibrary, autoAdd)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	//IL_0003: Unknown result type (might be due to invalid IL or missing references)
	//IL_0004: Unknown result type (might be due to invalid IL or missing references)

}
