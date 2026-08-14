using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 神鹿·贝瑟：按抽牌堆规模强化格子，全强化时改为获得飞行。消耗�?/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsGeneratedForestSpiritBeth : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int DrawCount = 1;
    private const int EmeraldMarkAmount = 2;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(DrawCount),
        new PowerVar<AlchemyStarsEmeraldMarkPower>(EmeraldMarkAmount),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.EmeraldMark)),
        HoverTipFactory.FromPower<AlchemyStarsEmeraldMarkPower>(),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>()
    ];

    public AlchemyStarsGeneratedForestSpiritBeth()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        LightMechanic.TryGrantLightEnergy(Owner, LightElement.Forest);

        var drawPileSize = PileType.Draw.GetPile(Owner).Cards.Count;
        if (AllCellsEnhanced(Owner))
        {
            await PowerCmd.Apply<AlchemyStarsFlyingPower>(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
        }
        else
        {
            for (var i = 0; i < drawPileSize; i++)
            {
                if (AllCellsEnhanced(Owner))
                {
                    await PowerCmd.Apply<AlchemyStarsFlyingPower>(
                        choiceContext,
                        Owner.Creature,
                        1m,
                        Owner.Creature,
                        this);
                    break;
                }

                if (!TryEnhanceRandomNonEnhancedCell(Owner))
                    break;
            }
        }

        await PowerCmd.Apply<AlchemyStarsEmeraldMarkPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsEmeraldMarkPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    private static bool AllCellsEnhanced(Player player)
    {
        var state = LightMechanic.GetActiveState(player);
        if (state == null)
            return false;

        var cells = state.AttributeCells.Items;
        return cells.Count > 0 && cells.All(cell => cell.Kind == AttributeCellKind.Enhanced);
    }

    private static bool TryEnhanceRandomNonEnhancedCell(Player player)
    {
        foreach (var element in LightElementExtensions.BaseElements)
        {
            if (LightMechanic.TryEnhanceRandomCell(player, element))
                return true;
        }

        return false;
    }
}
