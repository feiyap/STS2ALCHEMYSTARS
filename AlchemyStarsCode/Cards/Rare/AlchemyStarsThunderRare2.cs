using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 岁跃金鳞·辰霓：如意雷；抽牌并将抽到的牌本回合费用变为 0，获得缓冲。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderRare2 : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int DrawCount = 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(DrawCount),
        new PowerVar<BufferPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("AuspiciousThunder", AlchemyStarsKeywordIds.AuspiciousThunder),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.AuspiciousThunder];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AuspiciousThunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AuspiciousThunder)),
        
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<BufferPower>()
    ];

    public AlchemyStarsThunderRare2()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        LightMechanic.TryConvertAllCellsAuspiciousThunder(Owner);

        var drawn = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        foreach (var card in drawn)
            card.EnergyCost.SetThisTurn(0);

        await PowerCmd.Apply<BufferPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BufferPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BufferPower"].UpgradeValueBy(1m);
    }
}
