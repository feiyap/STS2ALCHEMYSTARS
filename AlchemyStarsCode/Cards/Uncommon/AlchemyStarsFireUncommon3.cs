using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
/// 炽情长殷·仲胥：炽焰断灭对全体施加 3 层灼烧；获火光能，按被灼烧敌人数获格挡。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon3 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int FireEnergyGain = 2;
    private const int ScorchAmount = 3;
    private const decimal BaseBlockPerEnemy = 4m;
    private const decimal BlockPerEnemyUpgradeBy = 1m;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(BaseBlockPerEnemy, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("BlazingSeverance", AlchemyStarsKeywordIds.BlazingSeverance),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BlazingSeverance)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BlazingSeverance)),
        HoverTipFactory.FromPower<AlchemyStarsScorchPower>()
    ];

    public AlchemyStarsFireUncommon3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Fire, FireEnergyGain);

        var scorchedCount = 0;
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<AlchemyStarsScorchPower>(
                choiceContext,
                enemy,
                ScorchAmount,
                Owner.Creature,
                this);
            scorchedCount++;
        }

        if (scorchedCount > 0)
        {
            var block = scorchedCount * DynamicVars.Block.IntValue;
            if (scorchedCount == 1)
                block *= 2;

            await CreatureCmd.GainBlock(
                Owner.Creature,
                new BlockVar(block, ValueProp.Move),
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(BlockPerEnemyUpgradeBy);
    }
}
