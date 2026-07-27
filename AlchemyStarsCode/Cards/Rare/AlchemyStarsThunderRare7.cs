using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 傲雪白狐·提亚拉：多人模式；零伤攻击触发破绽与攻击特效�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderRare7 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new RepeatVar(2),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderMonochrome", AlchemyStarsKeywordIds.ThunderMonochrome),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.ThunderMonochrome];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ThunderMonochrome)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ThunderMonochrome)),
        HoverTipFactory.FromPower<AlchemyStarsFlawPower>()
    ];

    public AlchemyStarsThunderRare7()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var teammateCount = CombatState!
            .GetTeammatesOf(Owner.Creature)
            .Count(creature => creature.IsAlive && creature.IsPlayer);
        var hitCount = teammateCount * DynamicVars.Repeat.IntValue;

        if (hitCount <= 0)
            return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            for (var i = 0; i < hitCount; i++)
            {
                if (enemy.IsDead)
                    break;

                await DamageCmd.Attack(0m)
                    .FromCard(this, cardPlay)
                    .Targeting(enemy)
                    .Execute(choiceContext);

                await PowerCmd.Apply<AlchemyStarsFlawPower>(
                    choiceContext,
                    enemy,
                    1m,
                    Owner.Creature,
                    this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
