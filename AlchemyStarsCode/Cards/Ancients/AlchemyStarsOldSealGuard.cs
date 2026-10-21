using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 寂静猎兵·戒卫座：按威能对所有敌人造成水属性伤害，本场受伤次数提高伤害。
/// 每场战斗一次，生命归零时不死，并把这张牌抽回手牌，下回合开始死亡。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsOldSealGuard : AlchemyStarsOldSealAttackCard
{
    private const string CalculatedDamageKey = "CalculatedDamage";
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    private ICombatState? _watchCombat;

    public override bool CanBeGeneratedInCombat => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CalculatedDamageKey).WithMultiplier(CountDamage),
        AlchemyStarsKeywordText.InlineTitleVar("SilentHunter", AlchemyStarsKeywordIds.SilentHunter),
        AlchemyStarsKeywordText.InlineTitleVar("GuardWatch", AlchemyStarsKeywordIds.GuardWatch),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.GuardWatch)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.GuardWatch)),
        HoverTipFactory.FromPower<AlchemyStarsOldSealMightPower>(),
        HoverTipFactory.FromPower<AlchemyStarsGuardDoomPower>()
    ];

    public AlchemyStarsOldSealGuard()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw) =>
        AlchemyStarsOldSeal.GainHunterMightIfEnteredHand(choiceContext, this, card);

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator) =>
        AlchemyStarsOldSeal.GainHunterMightIfEnteredHand(new BlockingPlayerChoiceContext(), this, card);

    public override bool ShouldDieLate(Creature creature)
    {
        if (Owner == null || creature != Owner.Creature)
            return true;

        return ReferenceEquals(_watchCombat, CombatState);
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature.CurrentHp <= 0)
            await CreatureCmd.Heal(creature, 1m);

        AssertMutable();
        _watchCombat = creature.CombatState;

        if (Pile?.Type != PileType.Hand)
            await CardPileCmd.Add(this, PileType.Hand);

        await PowerCmd.Apply<AlchemyStarsGuardDoomPower>(
            new BlockingPlayerChoiceContext(),
            creature,
            1m,
            creature,
            this);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hurt = AlchemyStarsOldSealCombatService.GetTimesHurt(Owner);
        await DealScaledToAll(choiceContext, cardPlay, hurt, LightElement.Water);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    private static decimal CountDamage(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        var hurt = AlchemyStarsOldSealCombatService.GetTimesHurt(card.Owner);
        return AlchemyStarsOldSeal.ScaleDamage(card, _, hurt, null);
    }
}
