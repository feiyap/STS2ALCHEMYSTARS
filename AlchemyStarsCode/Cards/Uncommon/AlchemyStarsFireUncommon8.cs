using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 蓝火疗心·诺维亚：灵杖庇佑；恢复体力，每回合开始消耗牌获灼燃并回血。升级后固有。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int PlayHealAmount = 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(PlayHealAmount),
        new PowerVar<AlchemyStarsSpiritStaffBlessingPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("SpiritStaffBlessing", AlchemyStarsKeywordIds.SpiritStaffBlessing),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SpiritStaffBlessing)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SpiritStaffBlessing)),
        HoverTipFactory.FromPower<AlchemyStarsSpiritStaffBlessingPower>(),
        HoverTipFactory.FromPower<AlchemyStarsIgnitionPower>()
    ];

    public AlchemyStarsFireUncommon8()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, PlayHealAmount);
        await PowerCmd.Apply<AlchemyStarsSpiritStaffBlessingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsSpiritStaffBlessingPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
