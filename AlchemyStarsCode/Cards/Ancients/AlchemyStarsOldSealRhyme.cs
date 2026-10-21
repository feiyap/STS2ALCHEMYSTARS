using AlchemyStars.Keywords;
using AlchemyStars.Powers;
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
/// 寂静猎兵·童谣座：虚无。消耗威能充能暴击；被消耗时威能翻倍。
/// 没有星辰纹章时，消耗牌堆中的这张牌会在下回合开始自动打出。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsOldSealRhyme : ModCardTemplate, IAlchemyStarsOldSeal
{
    private const string CritPercentKey = "CritPercent";
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override bool CanBeGeneratedInCombat => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(CritPercentKey, 4m),
        AlchemyStarsKeywordText.InlineTitleVar("SilentHunter", AlchemyStarsKeywordIds.SilentHunter),
        AlchemyStarsKeywordText.InlineTitleVar("RhymeLake", AlchemyStarsKeywordIds.RhymeLake),
        AlchemyStarsKeywordText.InlineTitleVar("CritTitle", AlchemyStarsKeywordIds.Crit)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RhymeLake)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RhymeLake)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Crit)),
        HoverTipFactory.FromPower<AlchemyStarsOldSealMightPower>(),
        HoverTipFactory.FromPower<AlchemyStarsCritChancePower>(),
        ..HoverTipFactory.FromRelic<AlchemyStarsStarCrest>()
    ];

    public AlchemyStarsOldSealRhyme()
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

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (!ReferenceEquals(card, this) || Owner == null)
            return;

        await AlchemyStarsOldSeal.DoubleMight(choiceContext, Owner, this);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Pile?.Type != PileType.Exhaust)
            return;

        if (Owner.GetRelic<AlchemyStarsStarCrest>() != null)
            return;

        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        var might = await AlchemyStarsOldSeal.ConsumeAllMight(choiceContext, Owner);
        if (might <= 0)
            return;

        var percent = DynamicVars[CritPercentKey].BaseValue * might;
        await PowerCmd.Apply<AlchemyStarsCritChancePower>(
            choiceContext,
            Owner.Creature,
            percent,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[CritPercentKey].UpgradeValueBy(1m);
    }
}
