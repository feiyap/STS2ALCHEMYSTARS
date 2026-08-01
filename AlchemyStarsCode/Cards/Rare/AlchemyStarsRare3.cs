using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 总攻击？：尽可能打出手牌中所有攻击牌并结束回合；若击杀目标则偷取属性格×4 金币。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int GoldPerAttributeCell = 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        
        HoverTipFactory.Static(StaticHoverTip.Fatal)
    ];

    public AlchemyStarsRare3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hand = PileType.Hand.GetPile(Owner);
        var attacks = hand.Cards
            .Where(card =>
                card.Type == CardType.Attack &&
                !card.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();

        foreach (var attack in attacks)
        {
            if (CombatManager.Instance.IsOverOrEnding)
                break;

            Creature? target = null;
            if (attack.TargetType == TargetType.AnyEnemy)
            {
                target = cardPlay.Target is { IsDead: false }
                    ? cardPlay.Target
                    : Owner.RunState.Rng.CombatTargets.NextItem(CombatState!.HittableEnemies);
            }

            await CardCmd.AutoPlay(choiceContext, attack, target);
        }

        if (cardPlay.Target.IsDead)
        {
            var gold = LightMechanic.CountAttributeCells(Owner) * GoldPerAttributeCell;
            if (gold > 0)
                await PlayerCmd.GainGold(gold, Owner);
        }

        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
