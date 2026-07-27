using MegaCrit.Sts2.Core.Entities.Cards;

using STS2RitsuLib.CardTags;

using STS2RitsuLib.Interop.AutoRegistration;



namespace AlchemyStars.Keywords;



/// <summary>

/// 模组自定义卡牌标签注册�?

/// </summary>

[RegisterOwnedCardTag("high_court_guard")]

[RegisterOwnedCardTag("legion_commander")]

[RegisterOwnedCardTag("scout")]

[RegisterOwnedCardTag("overheat_battle_skill")]

[RegisterOwnedCardTag("justice_immortal")]

[RegisterOwnedCardTag("auspicious_thunder")]

[RegisterOwnedCardTag("penetrating_star")]

[RegisterOwnedCardTag("righteous_majesty")]

[RegisterOwnedCardTag("rebellion_burning")]

[RegisterOwnedCardTag("thunder_monochrome")]

[RegisterOwnedCardTag("golden_scale_star")]

[RegisterOwnedCardTag("shadow_town_tea_party")]

[RegisterOwnedCardTag("velvet_needle_base")]

[RegisterOwnedCardTag("poison_flower_pool")]

[RegisterOwnedCardTag("on_capital")]

[RegisterOwnedCardTag("strange_animal")]

public static class AlchemyStarsCardTags

{

    public const string HighCourtGuardId = "ALCHEMY_STARS_CARD_TAG_HIGH_COURT_GUARD";

    public const string LegionCommanderId = "ALCHEMY_STARS_CARD_TAG_LEGION_COMMANDER";

    public const string ScoutId = "ALCHEMY_STARS_CARD_TAG_SCOUT";

    public const string OverheatBattleSkillId = "ALCHEMY_STARS_CARD_TAG_OVERHEAT_BATTLE_SKILL";

    public const string JusticeImmortalId = "ALCHEMY_STARS_CARD_TAG_JUSTICE_IMMORTAL";

    public const string AuspiciousThunderId = "ALCHEMY_STARS_CARD_TAG_AUSPICIOUS_THUNDER";

    public const string PenetratingStarId = "ALCHEMY_STARS_CARD_TAG_PENETRATING_STAR";

    public const string RighteousMajestyId = "ALCHEMY_STARS_CARD_TAG_RIGHTEOUS_MAJESTY";

    public const string RebellionBurningId = "ALCHEMY_STARS_CARD_TAG_REBELLION_BURNING";

    public const string ThunderMonochromeId = "ALCHEMY_STARS_CARD_TAG_THUNDER_MONOCHROME";

    public const string GoldenScaleStarId = "ALCHEMY_STARS_CARD_TAG_GOLDEN_SCALE_STAR";

    public const string ShadowTownTeaPartyId = "ALCHEMY_STARS_CARD_TAG_SHADOW_TOWN_TEA_PARTY";

    public const string VelvetNeedleBaseId = "ALCHEMY_STARS_CARD_TAG_VELVET_NEEDLE_BASE";

    public const string PoisonFlowerPoolId = "ALCHEMY_STARS_CARD_TAG_POISON_FLOWER_POOL";

    public const string OnCapitalId = "ALCHEMY_STARS_CARD_TAG_ON_CAPITAL";

    public const string StrangeAnimalId = "ALCHEMY_STARS_CARD_TAG_STRANGE_ANIMAL";



    public static CardTag HighCourtGuard => HighCourtGuardId.GetModCardTag();

    public static CardTag LegionCommander => LegionCommanderId.GetModCardTag();

    public static CardTag Scout => ScoutId.GetModCardTag();

    public static CardTag OverheatBattleSkill => OverheatBattleSkillId.GetModCardTag();

    public static CardTag JusticeImmortal => JusticeImmortalId.GetModCardTag();

    public static CardTag AuspiciousThunder => AuspiciousThunderId.GetModCardTag();

    public static CardTag PenetratingStar => PenetratingStarId.GetModCardTag();

    public static CardTag RighteousMajesty => RighteousMajestyId.GetModCardTag();

    public static CardTag RebellionBurning => RebellionBurningId.GetModCardTag();

    public static CardTag ThunderMonochrome => ThunderMonochromeId.GetModCardTag();

    public static CardTag GoldenScaleStar => GoldenScaleStarId.GetModCardTag();

    public static CardTag ShadowTownTeaParty => ShadowTownTeaPartyId.GetModCardTag();

    public static CardTag VelvetNeedleBase => VelvetNeedleBaseId.GetModCardTag();

    public static CardTag PoisonFlowerPool => PoisonFlowerPoolId.GetModCardTag();

    public static CardTag OnCapital => OnCapitalId.GetModCardTag();

    public static CardTag StrangeAnimal => StrangeAnimalId.GetModCardTag();

}


