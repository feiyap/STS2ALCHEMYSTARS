using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 碧翠之灵·纳努塞尔：清除减益并生成言灵牌；飞行状态下费用减 1。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestRare2 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int DrawCount = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(DrawCount),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromPower<AlchemyStarsEmeraldMarkPower>(),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedForestSpiritSing>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedForestSpiritPoit>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedForestSpiritBeth>()
    ];

    public AlchemyStarsForestRare2()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card, this))
            return false;

        if (Owner.Creature.GetPowerAmount<AlchemyStarsFlyingPower>() <= 0)
            return false;

        modifiedCost = Math.Max(0m, originalCost - 1m);
        return modifiedCost != originalCost;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var power in Owner.Creature.Powers.ToList())
        {
            if (power.Type == PowerType.Debuff)
                await PowerCmd.Remove(power);
        }

        await AddSpiritCard<AlchemyStarsGeneratedForestSpiritSing>(choiceContext);
        await AddSpiritCard<AlchemyStarsGeneratedForestSpiritPoit>(choiceContext);
        await AddSpiritCard<AlchemyStarsGeneratedForestSpiritBeth>(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级后生成的言灵牌自带升级效果。
    }

    private async Task AddSpiritCard<T>(PlayerChoiceContext choiceContext) where T : ModCardTemplate
    {
        var card = CombatState!.CreateCard<T>(Owner);
        if (IsUpgraded)
            card.UpgradeInternal();

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random);
    }
}
