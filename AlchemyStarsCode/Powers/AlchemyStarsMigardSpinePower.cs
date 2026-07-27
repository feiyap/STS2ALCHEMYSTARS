using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 潜庭之脊：攻击时附加森属性伤害并随机强化一格�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMigardSpinePower : ModPowerTemplate
{
    private const decimal BonusDamage = 4m;

    private bool _executeOnLowHp;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>升级弥加德卡牌时启用：攻击后若目标生命低�?10% 则斩杀�?/summary>
    public void ConfigureExecuteOnLowHp(bool enabled) => _executeOnLowHp = enabled;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        if (cardPlay.Target != null && !cardPlay.Target.IsDead)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                player,
                cardPlay.Card,
                cardPlay.Target,
                BonusDamage,
                LightElement.Forest,
                cardPlay);

            if (_executeOnLowHp)
                await AlchemyStarsCardHelpers.TryExecuteBelowHpThreshold(choiceContext, cardPlay.Target);
        }

        LightMechanic.TryEnhanceRandomCell(player, LightElement.Forest);
    }
}
