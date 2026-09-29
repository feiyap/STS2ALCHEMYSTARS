using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 正义执行·奈弥西斯：正义不灭；抽牌并对全体施加易伤与审判。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon10 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int DrawCount = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(DrawCount),
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<AlchemyStarsJudgmentPower>(2m),
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
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOverload>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<AlchemyStarsJudgmentPower>()
    ];

    public AlchemyStarsThunderUncommon10()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        if (await AlchemyStarsCardHelpers.TryConsumeOverloadAnywhere(choiceContext, Owner))
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            CombatState!.HittableEnemies,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<AlchemyStarsJudgmentPower>(
            choiceContext,
            CombatState.HittableEnemies,
            DynamicVars["AlchemyStarsJudgmentPower"].BaseValue,
            Owner.Creature,
            this);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
            await AlchemyStarsJudgmentPower.TryTriggerStunThreshold(choiceContext, enemy);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
        DynamicVars["AlchemyStarsJudgmentPower"].UpgradeValueBy(1m);
    }
}
