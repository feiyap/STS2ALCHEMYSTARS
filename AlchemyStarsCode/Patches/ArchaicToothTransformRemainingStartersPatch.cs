using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 古老牙齿转化第一张初始牌后，继续转化牌组中剩余的薇丝与卡莲。
/// </summary>
public sealed class ArchaicToothTransformRemainingStartersPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_archaic_tooth_transform_remaining";

    public static string Description => "Transform remaining Vice and Karen starters after Archaic Tooth";

    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(ArchaicTooth), nameof(ArchaicTooth.AfterObtained)),
    ];

    public static void Postfix(ArchaicTooth __instance, ref Task __result)
    {
        __result = TransformRemaining(__instance, __result);
    }

    private static async Task TransformRemaining(ArchaicTooth tooth, Task original)
    {
        await original;

        var owner = tooth.Owner;
        if (owner == null)
            return;

        foreach (var starter in owner.Deck.Cards.Where(IsAlchemyStarsStarter).ToList())
        {
            var ancient = CreateAncientCard(starter);
            if (ancient == null)
                continue;

            await CardCmd.Transform(starter, ancient);
        }
    }

    private static bool IsAlchemyStarsStarter(CardModel card) =>
        card is AlchemyStarsVice or AlchemyStarsKaren;

    private static CardModel? CreateAncientCard(CardModel starter)
    {
        CardModel? ancient = starter switch
        {
            AlchemyStarsVice => starter.Owner.RunState.CreateCard<AlchemyStarsViceEmptyPupil>(starter.Owner),
            AlchemyStarsKaren => starter.Owner.RunState.CreateCard<AlchemyStarsKarenBrightSoul>(starter.Owner),
            _ => null
        };

        if (ancient == null)
            return null;

        if (starter.IsUpgraded)
            CardCmd.Upgrade(ancient);

        if (starter.Enchantment != null)
        {
            var enchantment = (EnchantmentModel)starter.Enchantment.MutableClone();
            CardCmd.Enchant(enchantment, ancient, enchantment.Amount);
        }

        return ancient;
    }
}
