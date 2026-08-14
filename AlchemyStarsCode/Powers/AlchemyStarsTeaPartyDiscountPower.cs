using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 影镇茶话会：下一张茶话会成员卡牌费用 -1；消耗后进入冷却。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsTeaPartyDiscountPower : ModPowerTemplate
{
    /// <summary>与词条一致：效果结算后冷却 1 回合。</summary>
    public const int CooldownTurns = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || !AlchemyStarsCardHelpers.IsTeaPartyMember(card))
            return false;

        modifiedCost = originalCost - 1m;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
            return;

        if (!AlchemyStarsCardHelpers.IsTeaPartyMember(cardPlay.Card))
            return;

        Flash();
        var player = Owner.Player;
        await PowerCmd.Remove(this);

        // 冷却在折扣被消耗时开始，避免「本张牌消耗折扣后又立刻再触发」。
        if (player != null)
            AlchemyStarsForestState.SetTeaPartyCooldown(player, CooldownTurns);
    }
}
