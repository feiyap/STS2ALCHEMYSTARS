using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Powers;

/// <summary>
/// 资本征收：抽到论资本牌时支付金币并提升该牌伤害；战斗胜利后退还。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCapitalTaxPower : ModPowerTemplate
{
    private const int TaxGold = 20;
    private const decimal DamageIncreaseRate = 0.2m;

    private static readonly AttachedState<CardModel, int> TaxPaidGold = new(_ => 0);
    private static readonly AttachedState<CardModel, decimal> DamageBonusRate = new(_ => 0m);

    /// <summary>
    /// 战斗中是否曾死亡。胜利结算前会强制复活，不能再用 IsDead 判断。
    /// </summary>
    private bool _ownerDiedDuringCombat;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 死亡默认会清掉能力；保留本能力以便队伍仍胜利时按本金退还。
    /// </summary>
    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    public static int TaxAmount => TaxGold;

    public static decimal DamageIncreasePerTax => DamageIncreaseRate;

    public static void RecordTax(CardModel card, int goldPaid)
    {
        TaxPaidGold[card] += goldPaid;
        DamageBonusRate[card] += DamageIncreaseRate;
    }

    public static decimal GetDamageBonusRate(CardModel card) => DamageBonusRate[card];

    public static async Task RefundOnVictory(Player player, CardModel card, bool playerSurvived)
    {
        var paid = TaxPaidGold[card];
        if (paid <= 0)
            return;

        var refundRate = playerSurvived ? 1.2m : 1.0m;
        await PlayerCmd.GainGold((int)(paid * refundRate), player);
        TaxPaidGold[card] = 0;
        DamageBonusRate[card] = 0m;
    }

    public override Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Owner && !wasRemovalPrevented)
            _ownerDiedDuringCombat = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// 必须用 AfterCombatEnd：引擎会在 AfterCombatVictory 前清掉能力与战斗牌堆。
    /// 征税记在战斗卡副本上，需遍历 PlayerCombatState.AllCards，而非 Deck.Cards。
    /// </summary>
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        var combatState = player?.PlayerCombatState;
        if (player == null || combatState == null)
            return;

        Flash();
        var survived = !_ownerDiedDuringCombat;
        foreach (var card in combatState.AllCards.Where(c => c.Tags.Contains(AlchemyStarsCardTags.OnCapital)))
            await RefundOnVictory(player, card, survived);
    }
}
