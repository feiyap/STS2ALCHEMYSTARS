using AlchemyStars.Mechanics;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 祭剑座属性克制：雷克水，水克火，火克森，森克雷。
/// </summary>
internal static class AlchemyStarsSwordAltarRules
{
    public static bool Beats(LightElement attacker, LightElement defender) =>
        (attacker, defender) is
        (LightElement.Thunder, LightElement.Water) or
        (LightElement.Water, LightElement.Fire) or
        (LightElement.Fire, LightElement.Forest) or
        (LightElement.Forest, LightElement.Thunder);
}
