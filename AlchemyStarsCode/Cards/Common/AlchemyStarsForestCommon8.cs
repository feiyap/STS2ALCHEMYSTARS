using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 黑棘魔月·艾蕾雅：影镇茶话会；弃牌造成 2 次费用总和森伤，诅�?状态牌改为消耗并额外造成伤害�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestCommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int MaxDiscardCount = 2;
    private const int HitCount = 2;
    private const decimal CurseStatusDamage = 5m;
    private const int TeaPartyCooldownTurns = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(CurseStatusDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("ShadowTownTeaParty", AlchemyStarsKeywordIds.ShadowTownTeaParty),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.ShadowTownTeaParty];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty)),
        HoverTipFactory.FromPower<AlchemyStarsTeaPartyDiscountPower>()
    ];

    public AlchemyStarsForestCommon8()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await AlchemyStarsCardHelpers.TryTriggerTeaPartyOnPlay(
            choiceContext,
            this,
            Owner,
            TeaPartyCooldownTurns);

        var discarded = (await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, MaxDiscardCount),
            card => !ReferenceEquals(card, this),
            this)).ToList();

        decimal costSum = 0m;
        foreach (var card in discarded)
        {
            if (card.Type is CardType.Curse or CardType.Status)
            {
                await CardCmd.Exhaust(choiceContext, card);
                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    Owner,
                    this,
                    cardPlay.Target,
                    DynamicVars.Damage.BaseValue,
                    LightElement.Forest,
                    cardPlay);
            }
            else
            {
                await CardCmd.Discard(choiceContext, card);
                costSum += card.EnergyCost.GetWithModifiers(CostModifiers.All);
            }
        }

        for (var i = 0; i < HitCount; i++)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                costSum,
                LightElement.Forest,
                cardPlay);
        }

        if (IsUpgraded)
            await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
}
