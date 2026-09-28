using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 隐士金库：加入贪婪；下个商店物品免费，直到该商店结束。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsHermitsVault : AlchemyStarsAncientRelicBase
{
    private bool _freeNextShop = true;
    private bool _shopSessionActive;

    public override bool HasUponPickupEffect => true;

    /// <summary>
    /// 是否仍可享受下一次商店免费。
    /// </summary>
    [SavedProperty]
    public bool FreeNextShop
    {
        get => _freeNextShop;
        set
        {
            AssertMutable();
            _freeNextShop = value;
            Status = value ? RelicStatus.Active : RelicStatus.Disabled;
        }
    }

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        await CardPileCmd.AddCurseToDeck<Greed>(Owner);
        FreeNextShop = true;
        Flash();
    }

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
    {
        if (player != Owner || !LocalContext.IsMe(Owner))
            return originalPrice;

        if (!FreeNextShop)
            return originalPrice;

        return 0m;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (!FreeNextShop)
            return Task.CompletedTask;

        if (room is MerchantRoom)
        {
            _shopSessionActive = true;
            Flash();
            return Task.CompletedTask;
        }

        // 离开商店后（进入其他房间）结束免费标记。
        if (_shopSessionActive)
        {
            _shopSessionActive = false;
            FreeNextShop = false;
        }

        return Task.CompletedTask;
    }
}
