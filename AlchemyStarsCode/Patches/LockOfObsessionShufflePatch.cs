using System.Threading.Tasks;
using AlchemyStars.Relics.Ancients;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 妄执之锁生效时拦截自动洗牌，改为弃牌堆自选补牌。
/// </summary>
public sealed class LockOfObsessionShuffleIfNecessaryPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_lock_of_obsession_shuffle_if_necessary";
    public static string Description => "Block auto-shuffle while Lock of Obsession is active";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(CardPileCmd), nameof(CardPileCmd.ShuffleIfNecessary)),
    ];

    public static bool Prefix(PlayerChoiceContext choiceContext, Player player, ref Task __result)
    {
        var lockRelic = player.GetRelic<AlchemyStarsLockOfObsession>();
        if (lockRelic is not { IsActive: true })
            return true;

        __result = lockRelic.ReplaceShuffleIfNecessaryAsync(choiceContext, player);
        return false;
    }
}
