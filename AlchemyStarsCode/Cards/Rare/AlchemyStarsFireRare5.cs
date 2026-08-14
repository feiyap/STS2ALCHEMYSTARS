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
/// 左轮之徒·约拿：装填子弹后消耗灼燃连射，需足够火属性格才能打出。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireRare5 : ModCardTemplate
{
    private static readonly AttachedState<CardModel, int> Bullets = new(_ => 0);

    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int RequiredFireCells = 3;
    private const decimal BaseBulletDamage = 2m;
    private const int BulletsPerExhaustCard = 1;
    private const decimal IgnitionBonusRate = 0.15m;

    protected override bool IsPlayable =>
        LightMechanic.CountFireAttributeCells(Owner) >= RequiredFireCells;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseBulletDamage, ValueProp.Move),
        new DynamicVar("Bullets", 0),
        AlchemyStarsKeywordText.InlineTitleVar("HighNoon", AlchemyStarsKeywordIds.HighNoon),
        AlchemyStarsKeywordText.InlineTitleVar("NoSurvivors", AlchemyStarsKeywordIds.NoSurvivors),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighNoon),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.NoSurvivors)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighNoon)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.NoSurvivors)),
        
        HoverTipFactory.FromPower<AlchemyStarsIgnitionPower>()
    ];

    public AlchemyStarsFireRare5()
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

        Bullets[this] += PileType.Exhaust.GetPile(Owner).Cards.Count * BulletsPerExhaustCard;
        DynamicVars["Bullets"].BaseValue = Bullets[this];
        await Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var ignition = Owner.Creature.GetPower<AlchemyStarsIgnitionPower>();
        var ignitionAmount = ignition?.Amount ?? 0;
        if (ignition != null)
            await PowerCmd.Remove(ignition);

        var damageMultiplier = 1m + ignitionAmount * IgnitionBonusRate;
        var bulletDamage = DynamicVars.Damage.BaseValue * damageMultiplier;
        var shotCount = Bullets[this];

        for (var i = 0; i < shotCount; i++)
        {
            if (cardPlay.Target.IsDead)
                break;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                bulletDamage,
                LightElement.Fire,
                cardPlay);
        }

        Bullets[this] = 0;
        DynamicVars["Bullets"].BaseValue = 0;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
