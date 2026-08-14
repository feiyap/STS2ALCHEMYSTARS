using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 硝酸甘糖·比比比约恩：消耗最多 2 点火光能，以随机属性填满转色栏，并获得灼燃。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireCommon7 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int MaxFireLightConsume = 2;
    private const int SmallDarkChancePercent = 15;
    private const int MediumDarkChancePercent = 30;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AlchemyStarsIgnitionPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<AlchemyStarsIgnitionPower>()
    ];

    public AlchemyStarsFireCommon7()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var available = LightMechanic.CountFireLightEnergy(Owner);
        var consumeCount = System.Math.Min(MaxFireLightConsume, available);
        if (consumeCount > 0)
        {
            var cost = new List<LightElement>(consumeCount);
            for (var i = 0; i < consumeCount; i++)
                cost.Add(LightElement.Fire);
            LightMechanic.TryConsumeLightEnergy(Owner, cost);
        }

        var darkChance = IsUpgraded ? MediumDarkChancePercent : SmallDarkChancePercent;
        LightMechanic.FillAttributeBarWithRandomElements(Owner, darkChance);
        await PowerCmd.Apply<AlchemyStarsIgnitionPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsIgnitionPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsIgnitionPower"].UpgradeValueBy(1m);
    }
}
