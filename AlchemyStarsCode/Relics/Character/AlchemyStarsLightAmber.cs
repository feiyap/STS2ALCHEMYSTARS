using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 光珀：每当光能被消耗时，补充 1 点同属性光能。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLightAmber : AlchemyStarsCharacterRelicBase
{
    [ThreadStatic]
    private static bool _refilling;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public static void OnLightEnergyConsumed(Player player, IReadOnlyList<LightElement> consumed)
    {
        if (_refilling || consumed.Count == 0)
            return;

        var relic = player.GetRelic<AlchemyStarsLightAmber>();
        if (relic == null || !LightMechanic.IsMechanicActive(player))
            return;

        _refilling = true;
        try
        {
            relic.Flash();
            foreach (var element in consumed)
                LightMechanic.TryGrantLightEnergy(player, element);
        }
        finally
        {
            _refilling = false;
        }
    }
}
