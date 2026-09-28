using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 潜庭之脊：攻击时附加森属性伤害，并随机强化属性栏中一格。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMigardSpinePower : AlchemyStarsPowerBase
{
    private const decimal BonusDamage = 4m;

    private bool _isResolving;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_isResolving)
            return;

        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        _isResolving = true;
        try
        {
            foreach (var target in ResolveAttackTargets(cardPlay))
            {
                if (target.IsDead)
                    continue;

                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    player,
                    cardPlay.Card,
                    target,
                    BonusDamage,
                    LightElement.Forest,
                    cardPlay: null,
                    playAttackerAnim: false);

                await AlchemyStarsCardHelpers.TryExecuteBelowHpThreshold(choiceContext, target);
            }

            LightMechanic.TryEnhanceRandomUnenhancedCell(player);
        }
        finally
        {
            _isResolving = false;
        }
    }

    private IEnumerable<Creature> ResolveAttackTargets(CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
            return [cardPlay.Target];

        var enemies = Owner.CombatState?.HittableEnemies;
        if (enemies == null)
            return [];

        return enemies.ToList();
    }
}
