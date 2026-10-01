using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 柳叶冰刃·渡：获得水光能并对目标施加虚弱；可再消耗水光能，按水属性格数施加中毒（深色格计为 2）。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterCommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int BaseWaterLightGain = 1;
    private const int BaseWaterLightConsume = 1;
    private const decimal WeakAmount = 1m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("WaterLightGain", BaseWaterLightGain),
        new IntVar("WaterLightConsume", BaseWaterLightConsume),
        new PowerVar<WeakPower>(WeakAmount),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public AlchemyStarsWaterCommon8()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        LightMechanic.TryGrantLightEnergyMany(
            Owner,
            LightElement.Water,
            DynamicVars["WaterLightGain"].IntValue);

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Weak.BaseValue,
            Owner.Creature,
            this);

        var consumed = 0;
        var maxConsume = DynamicVars["WaterLightConsume"].IntValue;
        for (var n = 0; n < maxConsume; n++)
        {
            if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Water]))
                break;

            consumed++;
        }

        if (consumed <= 0)
            return;

        var poison = LightMechanic.CountWaterAttributeCellsWeighted(Owner) * consumed;
        if (poison <= 0)
            return;

        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            cardPlay.Target,
            poison,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 升级只增加获得的水光能，消耗保持 1。
        DynamicVars["WaterLightGain"].UpgradeValueBy(1m);
    }
}
