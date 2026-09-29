using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 潜庭之脊：攻击时按层数附加森属性伤害并随机强化一格；斩杀阈值固定 10%。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMigardSpinePower : AlchemyStarsPowerBase
{
    private const decimal BonusDamagePerStack = 4m;

    private bool _isResolving;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BonusDamage", BonusDamagePerStack)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_isResolving)
            return;

        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
            return;

        var player = Owner.Player;
        if (player == null || Amount <= 0)
            return;

        _isResolving = true;
        try
        {
            // 叠加只增加附加伤害，不改变斩杀阈值。
            var bonus = BonusDamagePerStack * Amount;
            foreach (var target in ResolveAttackTargets(cardPlay))
            {
                if (target.IsDead)
                    continue;

                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    player,
                    cardPlay.Card,
                    target,
                    bonus,
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
