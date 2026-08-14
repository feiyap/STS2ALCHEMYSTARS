using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 强运天星·歌尔蒂：强耀绽放；数值随打出次数在本局游戏中成长，升级时全格转森强化格并获得能量。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestRare5 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseValue = 1;
    private const int UpgradeEnergyGain = 2;

    private int _radiantBloomBonus;

    /// <summary>
    /// 本局已打出次数带来的数值加成（牌面 = BaseValue + 加成）。
    /// </summary>
    [SavedProperty]
    public int RadiantBloomBonus
    {
        get => _radiantBloomBonus;
        set
        {
            AssertMutable();
            _radiantBloomBonus = value;
            SyncFaceValues();
        }
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(UpgradeEnergyGain),
        new HealVar(BaseValue + RadiantBloomBonus),
        new BlockVar(BaseValue + RadiantBloomBonus, ValueProp.Move),
        new DamageVar(BaseValue + RadiantBloomBonus, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("RadiantBloom", AlchemyStarsKeywordIds.RadiantBloom),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RadiantBloom)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RadiantBloom)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
    ];

    public AlchemyStarsForestRare5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var value = BaseValue + RadiantBloomBonus;

        await CreatureCmd.Heal(Owner.Creature, value);
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(value, ValueProp.Move), cardPlay);

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                value,
                LightElement.Forest,
                cardPlay);
        }

        BuffFromPlay();
        (DeckVersion as AlchemyStarsForestRare5)?.BuffFromPlay();

        if (IsUpgraded)
        {
            LightMechanic.ConvertAllCellsToForestEnhanced(Owner);
            await PlayerCmd.GainEnergy(UpgradeEnergyGain, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果在打出时触发：全格转森强化格并获得 2 点能量。
    }

    /// <summary>
    /// 打出一次后，本局数值 +1，并同步牌面与牌组实例。
    /// </summary>
    private void BuffFromPlay()
    {
        RadiantBloomBonus++;
    }

    private void SyncFaceValues()
    {
        if (DynamicVars == null)
            return;

        var value = BaseValue + _radiantBloomBonus;
        if (DynamicVars.ContainsKey("Heal"))
            DynamicVars.Heal.BaseValue = value;
        if (DynamicVars.ContainsKey("Block"))
            DynamicVars.Block.BaseValue = value;
        if (DynamicVars.ContainsKey("Damage"))
            DynamicVars.Damage.BaseValue = value;
    }
}
