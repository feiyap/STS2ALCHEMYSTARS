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
/// 炽情长殷·仲胥：炽焰断灭；获火光能并按本场打出次数翻倍施加灼烧，按被灼烧敌人数获格挡。
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
    private const int BlockPerScorchedEnemy = 5;

    private int _playCount;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AlchemyStarsScorchPower>(2m),
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

        var baseScorch = DynamicVars["AlchemyStarsScorchPower"].IntValue;
        var scorchStacks = baseScorch * (1 << _playCount);
        var scorchedCount = 0;

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<AlchemyStarsScorchPower>(
                choiceContext,
                enemy,
                scorchStacks,
                Owner.Creature,
                this);
            scorchedCount++;
        }

        _playCount++;

        if (scorchedCount > 0)
        {
            var block = scorchedCount * BlockPerScorchedEnemy;
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
        DynamicVars["AlchemyStarsScorchPower"].UpgradeValueBy(1m);
    }
}
