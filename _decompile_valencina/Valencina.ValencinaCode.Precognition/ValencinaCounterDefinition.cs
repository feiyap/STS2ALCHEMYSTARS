namespace Valencina.ValencinaCode.Precognition;

public sealed record ValencinaCounterDefinition(ValencinaCounterStyle Style, string Key, int AmmoCost, decimal Damage, int BaseHitCount, bool Upgraded = false);
