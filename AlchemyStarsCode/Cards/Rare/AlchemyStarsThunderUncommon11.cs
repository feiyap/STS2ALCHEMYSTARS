using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 灿星天秤·伊伦汀：多人模式稀有牌；与队友平分生命，胜利后获得金币与棱镜格。卡图按先古样式展示。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon11 : ModCardTemplate, IAncientCardArtStyle
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyAlly;
    private const bool ShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.GoldenScaleStar];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.GoldenScaleStar)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
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
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (Owner.Creature.CurrentHp <= cardPlay.Target.CurrentHp)
            return;

        var self = Owner.Creature;
        var ally = cardPlay.Target;
        var totalHp = self.CurrentHp + ally.CurrentHp;
        var average = totalHp / 2m;

        await CreatureCmd.SetCurrentHp(self, average);
        await CreatureCmd.SetCurrentHp(ally, average);

        await PowerCmd.Apply<AlchemyStarsGoldenScaleStarPower>(
            choiceContext,
            self,
            1m,
            self,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
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
