using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Valencina.ValencinaCode.Precognition;

public sealed record PrecognitionCounterContext(Player Owner, Creature Attacker, int PrecognitionAmount, int PrecognitionMax, int DodgeValue, decimal PreventedDamageThisAttack, decimal PreventedDamageThisTurn, bool IsOverheated, bool IsActiveTrigger, bool FastAnimation = false);
