using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 正义执行·奈弥西斯：正义不灭；获得雷光能并对全体施加易伤，可消耗超载回收弃牌�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon10 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int ThunderEnergyGain = 2;
    private const int UpgradedJudgmentAmount = 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<AlchemyStarsJudgmentPower>(3m),
        AlchemyStarsKeywordText.InlineTitleVar("JusticeImmortal", AlchemyStarsKeywordIds.JusticeImmortal),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.JusticeImmortal];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.JusticeImmortal)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOverload>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<AlchemyStarsJudgmentPower>(),
        HoverTipFactory.FromPower<AlchemyStarsTremorPower>()
    ];

    public AlchemyStarsThunderUncommon10()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (await AlchemyStarsCardHelpers.TryConsumeOverloadFromHand(choiceContext, Owner))
        {
            await PlayerCmd.GainEnergy(1, Owner);

            var selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                PileType.Discard.GetPile(Owner),
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

            if (selected != null)
                await CardPileCmd.Add(selected, PileType.Hand);
        }

        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Thunder, ThunderEnergyGain);

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            CombatState!.HittableEnemies,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<AlchemyStarsJudgmentPower>(
                choiceContext,
                CombatState.HittableEnemies,
                DynamicVars["AlchemyStarsJudgmentPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }
}
