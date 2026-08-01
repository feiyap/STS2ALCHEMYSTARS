using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// ������ԡ����᣺ѡ�������Էɻ���ࣻ����ʱ����ѣ�������ƶѡ����ġ�
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderCommon7 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int UpgradedDazedCount = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        
        HoverTipFactory.FromCard<AlchemyStarsGenerated1>(),
        HoverTipFactory.FromCard<AlchemyStarsGenerated2>(),
        HoverTipFactory.FromCard<Dazed>()
    ];

    public AlchemyStarsThunderCommon7() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            await CardPileCmd.AddToCombatAndPreview<Dazed>(
                Owner.Creature,
                PileType.Discard,
                UpgradedDazedCount,
                null);
        }

        var boomFly = CombatState!.CreateCard<AlchemyStarsGenerated1>(Owner);
        var bangCrisp = CombatState.CreateCard<AlchemyStarsGenerated2>(Owner);
        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            [boomFly, bangCrisp],
            Owner);

        if (selected != null)
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
    }
}
