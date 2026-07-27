using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
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
using STS2RitsuLib.Utils;

namespace AlchemyStars.Cards;

/// <summary>
/// 贪婪之蛇·涉：论资本；首次打出时绑定资本征收，伤害随有效水格与征收加成成长。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterRare2 : ModCardTemplate
{
    private static readonly AttachedState<CardModel, bool> CapitalTaxConfigured = new(_ => false);

    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal DamageMultiplier = 4m;
    private const int DarkCellReplayThreshold = 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(DamageMultiplier, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("OnCapital", AlchemyStarsKeywordIds.OnCapital),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.OnCapital];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.OnCapital),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.OnCapital)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell)),
        HoverTipFactory.FromPower<AlchemyStarsCapitalTaxPower>()
    ];

    public AlchemyStarsWaterRare2()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CapitalTaxConfigured[this])
        {
            await PowerCmd.Apply<AlchemyStarsCapitalTaxPower>(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
            CapitalTaxConfigured[this] = true;
        }

        if (IsUpgraded && LightMechanic.CountWaterDarkCells(Owner) >= DarkCellReplayThreshold)
            BaseReplayCount += 1;

        var cellCount = LightMechanic.CountEffectiveWaterCells(Owner);
        var bonusRate = AlchemyStarsCapitalTaxPower.GetDamageBonusRate(this);
        var damage = DamageMultiplier * cellCount * (1m + bonusRate);

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                damage,
                LightElement.Water,
                cardPlay);
        }
    }

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (!ReferenceEquals(card, this))
            return;

        if (Owner.Creature.GetPowerAmount<AlchemyStarsCapitalTaxPower>() <= 0)
            return;

        var tax = AlchemyStarsCapitalTaxPower.TaxAmount;
        await PlayerCmd.LoseGold(tax, Owner);
        AlchemyStarsCapitalTaxPower.RecordTax(this, tax);
    }

    protected override void OnUpgrade()
    {
        // 升级后水深色格达到 4 个时额外重放 1 次。
    }
}
