using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 同行通融：商店可赊账（额度=房间层数×6），获得金币时自动还债。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsFellowCredit : AlchemyStarsCharacterRelicBase
{
    private const int CreditPerFloor = 6;

    private int _debt;
    private decimal _pendingDebt;

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override bool ShowCounter => Debt > 0;

    public override int DisplayAmount => Debt;

    [SavedProperty]
    public int Debt
    {
        get => _debt;
        set
        {
            AssertMutable();
            _debt = Math.Max(0, value);
            InvokeDisplayAmountChanged();
        }
    }

    private int CreditLimit =>
        Owner == null ? 0 : Math.Max(0, Owner.RunState.TotalFloor * CreditPerFloor);

    private int AvailableCredit => Math.Max(0, CreditLimit - Debt);

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
    {
        _pendingDebt = 0m;
        if (player != Owner || Owner == null || !LocalContext.IsMe(Owner))
            return originalPrice;

        var creditLeft = AvailableCredit;
        if (creditLeft <= 0)
            return originalPrice;

        var gold = player.Gold;
        if (gold >= originalPrice)
            return originalPrice;

        var need = originalPrice - gold;
        var useCredit = Math.Min(need, creditLeft);
        _pendingDebt = useCredit;
        return originalPrice - useCredit;
    }

    public override Task AfterItemPurchased(Player player, MerchantEntry entry, int paidAmount)
    {
        if (player != Owner || _pendingDebt <= 0m)
        {
            _pendingDebt = 0m;
            return Task.CompletedTask;
        }

        Flash();
        Debt += (int)Math.Ceiling(_pendingDebt);
        _pendingDebt = 0m;
        return Task.CompletedTask;
    }

    public override async Task AfterGoldGained(Player player)
    {
        if (player != Owner || Owner == null || Debt <= 0)
            return;

        var pay = Math.Min(Debt, (int)player.Gold);
        if (pay <= 0)
            return;

        Flash();
        await PlayerCmd.LoseGold(pay, Owner, GoldLossType.Spent);
        Debt -= pay;
    }
}
