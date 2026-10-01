using System.Linq;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 演天志异·李天闲：无法打出；保留时抽弃置换；每保留 10 次对全体造成 6×9 伤（跨战斗继承）。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsLiTianXian : ModCardTemplate
{
    private const int BaseEnergyCost = 7;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int RetainThreshold = 10;
    private const int BurstHitCount = 6;
    private const decimal BurstDamage = 9m;

    private int _retainTriggerCount;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    /// <summary>本局累计保留次数（跨战斗继承）。</summary>
    [SavedProperty]
    public int RetainTriggerCount
    {
        get => _retainTriggerCount;
        set
        {
            AssertMutable();
            _retainTriggerCount = value;
            SyncRetainDisplay();
        }
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BurstDamage, ValueProp.Move),
        new DynamicVar("Hits", BurstHitCount),
        new DynamicVar("RetainNeed", RetainThreshold),
        new DynamicVar("RetainCount", 0m),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MyriadSpectacle)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Unplayable),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MyriadSpectacle))
    ];

    public AlchemyStarsLiTianXian()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override void AfterCreated()
    {
        base.AfterCreated();
        SyncRetainDisplay();
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player != Owner || retainedCards.All(card => !ReferenceEquals(card, this)))
            return;

        IncrementRetainCount();

        // 弃牌堆抽 1，抽牌堆丢 1，抽到的牌本回合保留。
        var discardPile = PileType.Discard.GetPile(Owner);
        var drawn = Owner.RunState.Rng.CombatCardSelection.NextItem(discardPile.Cards);
        if (drawn != null)
        {
            await CardPileCmd.Add(drawn, PileType.Hand);
            drawn.GiveSingleTurnRetain();
        }

        var drawPile = PileType.Draw.GetPile(Owner);
        var toDiscard = Owner.RunState.Rng.CombatCardSelection.NextItem(drawPile.Cards);
        if (toDiscard != null)
            await CardCmd.Discard(choiceContext, toDiscard);

        while (RetainTriggerCount >= RetainThreshold)
        {
            RetainTriggerCount -= RetainThreshold;
            SyncDeckRetainCount();
            var enemies = CombatState?.HittableEnemies.ToList();
            if (enemies == null || enemies.Count == 0)
                continue;

            for (var hit = 0; hit < BurstHitCount; hit++)
            {
                foreach (var enemy in enemies.Where(e => !e.IsDead).ToList())
                {
                    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                        .FromCard(this, null)
                        .Targeting(enemy)
                        .WithNoAttackerAnim()
                        .Execute(choiceContext);
                }
            }
        }
    }

    private void IncrementRetainCount()
    {
        RetainTriggerCount++;
        SyncDeckRetainCount();
    }

    private void SyncDeckRetainCount()
    {
        if (DeckVersion is AlchemyStarsLiTianXian deck && !ReferenceEquals(deck, this))
            deck.RetainTriggerCount = RetainTriggerCount;
    }

    private void SyncRetainDisplay()
    {
        if (DynamicVars == null || !DynamicVars.ContainsKey("RetainCount"))
            return;

        DynamicVars["RetainCount"].BaseValue = RetainTriggerCount;
    }
}
