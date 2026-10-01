using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
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
/// 金泽之星·伊伦汀：多人模式稀有牌；与队友平分生命，胜利后自己与目标各得金币并按已损失生命治疗。卡图按先古样式展示。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon11 : ModCardTemplate, IAncientCardArtStyle
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyAlly;
    private const bool ShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HealPercent", 35m)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.GoldenScaleStar];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Innate,
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.GoldenScaleStar)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Innate),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.GoldenScaleStar)),
        HoverTipFactory.FromPower<AlchemyStarsGoldenScaleStarPower>()
    ];

    protected override bool IsPlayable =>
        base.IsPlayable && HasEligibleAllyTarget();

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public AlchemyStarsThunderUncommon11()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (Owner.Creature.CurrentHp <= cardPlay.Target.CurrentHp)
            return;

        var self = Owner.Creature;
        var ally = cardPlay.Target;
        var totalHp = self.CurrentHp + ally.CurrentHp;
        var average = totalHp / 2m;

        await CreatureCmd.SetCurrentHp(self, average);
        await CreatureCmd.SetCurrentHp(ally, average);

        var power = await PowerCmd.Apply<AlchemyStarsGoldenScaleStarPower>(
            choiceContext,
            self,
            1m,
            self,
            this);
        power?.ConfigureLostHpHealPercent(DynamicVars["HealPercent"].BaseValue / 100m);

        // 目标队友也挂一份，胜利时仅自己与目标获得金币与回血。
        var allyPower = await PowerCmd.Apply<AlchemyStarsGoldenScaleStarPower>(
            choiceContext,
            ally,
            1m,
            self,
            this);
        allyPower?.ConfigureLostHpHealPercent(DynamicVars["HealPercent"].BaseValue / 100m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HealPercent"].UpgradeValueBy(15m);
    }

    private bool HasEligibleAllyTarget()
    {
        if (CombatState == null)
            return false;

        return CombatState.PlayerCreatures.Any(creature =>
            creature.IsAlive &&
            creature.IsPlayer &&
            creature != Owner.Creature &&
            Owner.Creature.CurrentHp > creature.CurrentHp);
    }
}
