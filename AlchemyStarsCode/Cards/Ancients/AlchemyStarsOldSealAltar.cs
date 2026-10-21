using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Relics.Events;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 寂静猎兵·祭剑座：消耗全部光能并强制进入虹光，再削减所有敌人一半生命上限。
/// 打出后移出本场战斗，并唤醒星辰纹章。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsOldSealAltar : ModCardTemplate, IAlchemyStarsOldSeal
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        AlchemyStarsKeywordText.InlineTitleVar("Forgotten", AlchemyStarsKeywordIds.Forgotten),
        AlchemyStarsKeywordText.InlineTitleVar("PioneeringResolve", AlchemyStarsKeywordIds.PioneeringResolve),
        AlchemyStarsKeywordText.InlineTitleVar("ReturningMeteor", AlchemyStarsKeywordIds.ReturningMeteor),
        AlchemyStarsKeywordText.InlineTitleVar("FateAnger", AlchemyStarsKeywordIds.FateAnger),
        AlchemyStarsKeywordText.InlineTitleVar("RainbowTitle", AlchemyStarsKeywordIds.RainbowLight)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forgotten),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PioneeringResolve),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ReturningMeteor),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.FateAnger)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forgotten)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PioneeringResolve)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ReturningMeteor)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.FateAnger)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RainbowLight)),
        HoverTipFactory.FromPower<Powers.AlchemyStarsOldSealMightPower>(),
        ..HoverTipFactory.FromRelic<AlchemyStarsStarCrest>()
    ];

    public AlchemyStarsOldSealAltar()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override bool ShouldAddToDeck(CardModel card) =>
        !ReferenceEquals(card, this);

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (!ReferenceEquals(card, this) || Owner == null)
            return;

        await AlchemyStarsOldSeal.GainMight(
            new BlockingPlayerChoiceContext(),
            Owner,
            this,
            AlchemyStarsOldSeal.AltarMight);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        LightMechanic.ConsumeAllLightEnergy(Owner);
        LightMechanic.ForceRainbow(Owner);

        var combat = Owner.Creature.CombatState;
        if (combat != null)
        {
            foreach (var enemy in combat.HittableEnemies.ToList())
            {
                if (enemy.IsDead)
                    continue;

                var half = enemy.MaxHp / 2;
                if (half <= 0)
                    continue;

                await CreatureCmd.LoseMaxHp(choiceContext, enemy, half, true);
            }
        }

        Owner.GetRelic<AlchemyStarsStarCrest>()?.AwakenAfterAltarPlayed();
    }

    /// <summary>
    /// 被遗忘者：打出后移出本场战斗，不进入消耗牌堆。
    /// </summary>
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (!ReferenceEquals(card, this))
            return cardLocation;

        return cardLocation with { pileType = PileType.None };
    }
}
