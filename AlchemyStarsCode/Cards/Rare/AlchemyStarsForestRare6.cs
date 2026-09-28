using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
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
/// 伊斯塔万·沓痕：按森格获得格挡，本回合为全体队友承担伤害且自身无法打出攻击牌。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestRare6 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const decimal BlockPerForestCell = 10m;
    private const decimal BlockPerForestCellUpgradeBy = 5m;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(BlockPerForestCell, ValueProp.Move),
        new PowerVar<AlchemyStarsDisplacementKingShadowPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("DisplacementKingShadow", AlchemyStarsKeywordIds.DisplacementKingShadow),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DisplacementKingShadow)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DisplacementKingShadow)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromPower<AlchemyStarsDisplacementKingShadowPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public AlchemyStarsForestRare6()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        var forestCells = LightMechanic.CountEffectiveForestCellsForDamage(Owner);
        var block = forestCells * DynamicVars.Block.BaseValue;
        if (block > 0m)
        {
            await CreatureCmd.GainBlock(
                Owner.Creature,
                new BlockVar(block, ValueProp.Move),
                cardPlay);
        }

        await PowerCmd.Apply<AlchemyStarsDisplacementKingShadowPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsDisplacementKingShadowPower"].BaseValue,
            Owner.Creature,
            this);

        if (!IsUpgraded || forestCells <= 0)
            return;

        foreach (var ally in CombatState!.GetTeammatesOf(Owner.Creature)
                     .Where(creature =>
                         creature is { IsAlive: true, IsPlayer: true } &&
                         creature != Owner.Creature))
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                ally,
                forestCells,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(BlockPerForestCellUpgradeBy);
    }
}
