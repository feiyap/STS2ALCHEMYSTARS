using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 超载：抽到时失去 1 点能量�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsGeneratedOverload : ModCardTemplate
{
    private const int BaseEnergyCost = -1;
    private const CardType CardKind = CardType.Status;
    private const CardRarity CardRarityValue = CardRarity.Status;
    private const TargetType CardTarget = TargetType.None;
    private const bool ShowInCardLibrary = false;

    /// <summary>升级巡航阵列生成时：抽到时令手牌�?1 张雷属性攻击可重放 1 次�?/summary>
    internal bool GrantsThunderAttackReplay { get; set; }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload))
    ];

    public AlchemyStarsGeneratedOverload()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (!ReferenceEquals(card, this))
            return;

        await Cmd.Wait(0.25f);
        await PlayerCmd.LoseEnergy(DynamicVars.Energy.IntValue, Owner);

        if (!GrantsThunderAttackReplay)
            return;

        var thunderAttack = Owner.PlayerCombatState?.Hand.Cards.FirstOrDefault(candidate =>
            candidate.Type == CardType.Attack && AlchemyStarsCardHelpers.HasThunderKeyword(candidate));

        if (thunderAttack != null)
            thunderAttack.BaseReplayCount += 1;
    }
}
