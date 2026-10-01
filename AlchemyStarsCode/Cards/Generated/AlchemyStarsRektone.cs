using System.Linq;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 钧雷灼刃·雷文顿：弃 2 张后随机 3 段雷伤；三次全命中同一敌人则击晕。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsRektone : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int DiscardCount = 2;
    private const int HitCount = 3;
    private const decimal BaseDamage = 8m;
    private const decimal DamageUpgradeBy = 2m;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new DynamicVar("Hits", HitCount),
        new DynamicVar("Discard", DiscardCount),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public AlchemyStarsRektone()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handOthers = PileType.Hand.GetPile(Owner).Cards
            .Where(card => !ReferenceEquals(card, this))
            .ToList();
        var maxDiscard = Math.Min(DiscardCount, handOthers.Count);
        if (maxDiscard > 0)
        {
            var discarded = (await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, maxDiscard, maxDiscard),
                card => !ReferenceEquals(card, this),
                this)).ToList();

            foreach (var card in discarded)
                await CardCmd.Discard(choiceContext, card);
        }

        Creature? firstHit = null;
        var allSame = true;
        for (var i = 0; i < HitCount; i++)
        {
            var enemies = CombatState?.HittableEnemies.ToList();
            if (enemies == null || enemies.Count == 0)
            {
                allSame = false;
                break;
            }

            var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
            if (target == null)
            {
                allSame = false;
                break;
            }

            if (firstHit == null)
                firstHit = target;
            else if (!ReferenceEquals(firstHit, target))
                allSame = false;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                target,
                DynamicVars.Damage.BaseValue,
                LightElement.Thunder,
                cardPlay,
                playAttackerAnim: i == 0);
        }

        if (allSame && firstHit is { IsDead: false })
            await CreatureCmd.Stun(firstHit);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DamageUpgradeBy);
    }
}
