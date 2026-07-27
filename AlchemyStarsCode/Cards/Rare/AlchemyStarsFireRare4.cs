using System.Linq;
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
/// 千痕影主·伊斯塔万：从抽牌堆抽取火属性攻击牌，并可消耗火光能引爆弃牌堆。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireRare4 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int FireAttackDrawCount = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DrawFireAttacks", FireAttackDrawCount),
        AlchemyStarsKeywordText.InlineTitleVar("ShadowHerdMajesty", AlchemyStarsKeywordIds.ShadowHerdMajesty),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowHerdMajesty)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowHerdMajesty)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromPower<AlchemyStarsFireDoubleDamagePower>()
    ];

    public AlchemyStarsFireRare4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DrawFireAttacksFromDrawPile(choiceContext);

        if (!LightMechanic.TryConsumeLightEnergy(
                Owner,
                [LightElement.Fire, LightElement.Fire]))
            return;

        var discard = PileType.Discard.GetPile(Owner).Cards.ToList();
        foreach (var card in discard)
            await CardCmd.Exhaust(choiceContext, card);

        await PowerCmd.Apply<AlchemyStarsFireDoubleDamagePower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    private async Task DrawFireAttacksFromDrawPile(PlayerChoiceContext choiceContext)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var candidates = drawPile.Cards
            .Where(card =>
                card.Type == CardType.Attack &&
                AlchemyStarsCardHelpers.HasFireKeyword(card))
            .ToList();

        for (var i = 0; i < FireAttackDrawCount && candidates.Count > 0; i++)
        {
            var picked = Owner.RunState.Rng.CombatTargets.NextItem(candidates);
            if (picked == null)
                break;

            candidates.Remove(picked);
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }
}
