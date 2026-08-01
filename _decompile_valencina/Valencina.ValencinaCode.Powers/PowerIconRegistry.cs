using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Valencina.ValencinaCode.Powers;

public static class PowerIconRegistry
{
	private sealed record IconPair(string Packed, string Big);

	private const string BasePacked = "res://images/atlases/power_atlas.sprites/";

	private const string BaseBig = "res://images/powers/";

	private const string ModPacked = "res://Valencina/images/powers/";

	private const string ModBig = "res://Valencina/images/powers/big/";

	private static readonly Dictionary<Type, IconPair> ExplicitMap = new Dictionary<Type, IconPair>
	{
		[typeof(AmmoPower)] = new IconPair("res://Valencina/images/powers/ammo_power.png", "res://Valencina/images/powers/big/ammo_power.png"),
		[typeof(BreathingMethodPower)] = new IconPair("res://Valencina/images/powers/breathing_method_power.png", "res://Valencina/images/powers/big/breathing_method_power.png"),
		[typeof(BurnPower)] = new IconPair("res://Valencina/images/powers/burn_power.png", "res://Valencina/images/powers/big/burn_power.png"),
		[typeof(BurningTremorPower)] = new IconPair("res://Valencina/images/powers/burning_tremor_power.png", "res://Valencina/images/powers/big/burning_tremor_power.png"),
		[typeof(TremorPower)] = new IconPair("res://Valencina/images/powers/tremor_power.png", "res://Valencina/images/powers/big/tremor_power.png"),
		[typeof(AmmoCapacityPower)] = Vanilla("stock"),
		[typeof(AmmoGainBlockedPower)] = Vanilla("no_energy_gain"),
		[typeof(WarHeroRefundPower)] = new IconPair("res://Valencina/images/powers/war_hero_power.png", "res://Valencina/images/powers/big/war_hero_power.png"),
		[typeof(AcceleratedFuturePower)] = new IconPair("res://Valencina/images/powers/accelerated_future_power.png", "res://Valencina/images/powers/big/accelerated_future_power.png"),
		[typeof(HuntsEndPower)] = new IconPair("res://Valencina/images/powers/hunts_end_power.png", "res://Valencina/images/powers/big/hunts_end_power.png"),
		[typeof(HunterMarkPower)] = Vanilla("tracking"),
		[typeof(HighTemperatureStrengthDownPower)] = Vanilla("monarchs_gaze_strength_down"),
		[typeof(RedThreadPower)] = new IconPair("res://Valencina/images/powers/red_thread_power.png", "res://Valencina/images/powers/big/red_thread_power.png"),
		[typeof(WellPreparedPower)] = Vanilla("draw_cards_next_turn"),
		[typeof(OdinEyePower)] = new IconPair("res://Valencina/images/powers/odin_eye_power.png", "res://Valencina/images/powers/big/odin_eye_power.png"),
		[typeof(InstantForesightPower)] = new IconPair("res://Valencina/images/powers/odin_eye_power.png", "res://Valencina/images/powers/big/odin_eye_power.png"),
		[typeof(TemporaryPrecognitionPower)] = new IconPair("res://Valencina/images/powers/temporary_precognition_power.png", "res://Valencina/images/powers/big/temporary_precognition_power.png"),
		[typeof(InstantPredictionPower)] = new IconPair("res://Valencina/images/powers/instant_prediction_power.png", "res://Valencina/images/powers/big/instant_prediction_power.png"),
		[typeof(DodgeNextTurnPower)] = Vanilla("shadow_step"),
		[typeof(ConsumeAllAmmoNextTurnPower)] = Vanilla("star_next_turn"),
		[typeof(DuelTempoPower)] = new IconPair("res://Valencina/images/powers/duel_tempo_power.png", "res://Valencina/images/powers/big/duel_tempo_power.png"),
		[typeof(NoDodgeGainPower)] = Vanilla("no_block"),
		[typeof(NoDodgeNextTurnPower)] = Vanilla("no_block"),
		[typeof(OverheatNextTurnPower)] = new IconPair("res://Valencina/images/powers/instant_foresight_power_overheat.png", "res://Valencina/images/powers/big/instant_foresight_power_overheat.png"),
		[typeof(NotEvenClosePower)] = Vanilla("slippery"),
		[typeof(GetLostCounterPower)] = Vanilla("parry"),
		[typeof(VisceraCrushPower)] = Vanilla("crush_under"),
		[typeof(ScorchMarkPower)] = Vanilla("flame_barrier"),
		[typeof(ShatterRendPower)] = new IconPair("res://Valencina/images/powers/breathing_method_power.png", "res://Valencina/images/powers/big/breathing_method_power.png"),
		[typeof(LightSpeedExtraTurnPower)] = Vanilla("borrowed_time"),
		[typeof(HemostasisPower)] = Vanilla("vital_spark"),
		[typeof(ShinAmmoRefundPower)] = new IconPair("res://Valencina/images/powers/frenzy_power.png", "res://Valencina/images/powers/big/frenzy_power.png"),
		[typeof(ValencinaShinPower)] = new IconPair("res://Valencina/images/powers/valencina_shin_power.png", "res://Valencina/images/powers/big/valencina_shin_power.png"),
		[typeof(TightBitePower)] = Vanilla("mangle"),
		[typeof(AgileCounterPower)] = Vanilla("missing"),
		[typeof(MemoryExpansionPower)] = Vanilla("signal_boost"),
		[typeof(OdinEyeRatioPower)] = new IconPair("res://Valencina/images/powers/odin_eye_power.png", "res://Valencina/images/powers/big/odin_eye_power.png"),
		[typeof(OverwhelmingTechniquePower)] = Vanilla("parry"),
		[typeof(ScorchingEyeSocketPower)] = new IconPair("res://Valencina/images/powers/instant_foresight_power_overheat.png", "res://Valencina/images/powers/big/instant_foresight_power_overheat.png"),
		[typeof(FaceMyHatredPower)] = Vanilla("enrage"),
		[typeof(SharpPower)] = Vanilla("accuracy"),
		[typeof(SecondAccelerationPower)] = Vanilla("free_attack"),
		[typeof(ValencinaFreeNextAttackPower)] = Vanilla("free_attack"),
		[typeof(EmptyChamberPower)] = Vanilla("focused_strike"),
		[typeof(DespairHopeNoHopePower)] = Vanilla("double_damage"),
		[typeof(ThroughFireAndWaterPower)] = Vanilla("biased_cognition"),
		[typeof(FarewellPower)] = Vanilla("die_for_you"),
		[typeof(FarewellSkillRefundPower)] = Vanilla("free_skill"),
		[typeof(AnalyzeTraumaPower)] = Vanilla("missing"),
		[typeof(CrystalClearPower)] = Vanilla("clarity"),
		[typeof(UnyieldingPower)] = Vanilla("unrelenting"),
		[typeof(OverheatProtectionPower)] = new IconPair("res://Valencina/images/powers/instant_foresight_power_overheat.png", "res://Valencina/images/powers/big/instant_foresight_power_overheat.png"),
		[typeof(BecomingWholePower)] = Vanilla("missing"),
		[typeof(SettlementCompensationPower)] = Vanilla("dampen"),
		[typeof(FutureSightPower)] = Vanilla("anticipate"),
		[typeof(RollingHotPower)] = new IconPair("res://Valencina/images/powers/burning_tremor_power.png", "res://Valencina/images/powers/big/burning_tremor_power.png"),
		[typeof(GunMaintenancePower)] = Vanilla("tools_of_the_trade"),
		[typeof(ElegantSwordplayPower)] = Vanilla("sword_sage"),
		[typeof(AfterglowPower)] = new IconPair("res://Valencina/images/powers/breathing_method_power.png", "res://Valencina/images/powers/big/breathing_method_power.png"),
		[typeof(TargetDecisionPower)] = Vanilla("the_hunt"),
		[typeof(HuntingTargetPower)] = Vanilla("tracking"),
		[typeof(HuntingPreparationPower)] = Vanilla("prep_time"),
		[typeof(SoWeakPower)] = Vanilla("dark_shackles"),
		[typeof(SoHotPower)] = new IconPair("res://Valencina/images/powers/burn_power.png", "res://Valencina/images/powers/big/burn_power.png"),
		[typeof(KillingIntentPower)] = Vanilla("lethality"),
		[typeof(EnergyNextTurnPower)] = Vanilla("energy_next_turn"),
		[typeof(LieInWaitPower)] = Vanilla("retain_hand"),
		[typeof(CounterDrawPower)] = Vanilla("juggling"),
		[typeof(DestinedFuturePower)] = new IconPair("res://Valencina/images/powers/destined_future_power.png", "res://Valencina/images/powers/big/destined_future_power.png"),
		[typeof(AcceleratingMomentPower)] = Vanilla("synchronize"),
		[typeof(CoordinatedHuntPower)] = Vanilla("coordinate"),
		[typeof(DefensePestPower)] = new IconPair("res://Valencina/images/powers/defense_pest_power.png", "res://Valencina/images/powers/big/defense_pest_power.png"),
		[typeof(AttackPestPower)] = new IconPair("res://Valencina/images/powers/attack_pest_power.png", "res://Valencina/images/powers/big/attack_pest_power.png"),
		[typeof(KillMeKillMePower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserImperialMandatePower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserCitizensPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserCloakPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserArmyPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserWrathPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserBloodPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserFistPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserWhipPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserMarchDisplayPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserRustleDisplayPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserShieldPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserPredationPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserDinnerPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(KaiserExcisionStealPower)] = new IconPair("res://Valencina/images/powers/kill_me_kill_me_power.png", "res://Valencina/images/powers/big/kill_me_kill_me_power.png"),
		[typeof(AimForTheHeartPower)] = new IconPair("res://Valencina/images/powers/aim_for_the_heart_power.png", "res://Valencina/images/powers/big/aim_for_the_heart_power.png"),
		[typeof(EmperorExcisionPower)] = new IconPair("res://Valencina/images/powers/emperor_excision_power.png", "res://Valencina/images/powers/big/emperor_excision_power.png"),
		[typeof(EmperorExcisionTargetPower)] = Vanilla("tracking"),
		[typeof(KaiserNoDrawNextTurnPower)] = Vanilla("no_draw"),
		[typeof(KCorpAmpoulePower)] = new IconPair("res://Valencina/images/powers/k_corp_ampoule_power.png", "res://Valencina/images/powers/big/k_corp_ampoule_power.png"),
		[typeof(SharedKCorpAmpoulePower)] = new IconPair("res://Valencina/images/powers/k_corp_ampoule_power.png", "res://Valencina/images/powers/big/k_corp_ampoule_power.png"),
		[typeof(RodyaGuardPower)] = new IconPair("res://Valencina/images/powers/rodya_guard_power.png", "res://Valencina/images/powers/big/rodya_guard_power.png"),
		[typeof(TearBladePower)] = new IconPair("res://Valencina/images/powers/tear_blade_power.png", "res://Valencina/images/powers/big/tear_blade_power.png"),
		[typeof(HeathcliffWarningPower)] = new IconPair("res://Valencina/images/powers/heathcliff_warning_power.png", "res://Valencina/images/powers/big/heathcliff_warning_power.png"),
		[typeof(TemporaryThornsPower)] = Vanilla("thorns"),
		[typeof(BoundKingPower)] = new IconPair("res://Valencina/images/powers/bound_king_power.png", "res://Valencina/images/powers/big/bound_king_power.png"),
		[typeof(Act4EliteDrawDownNextTurnPower)] = Vanilla("no_draw"),
		[typeof(GregorMercyPower)] = new IconPair("res://Valencina/images/powers/gregor_mercy_power.png", "res://Valencina/images/powers/big/gregor_mercy_power.png"),
		[typeof(GregorWoundOnHitPower)] = Vanilla("painful_stabs"),
		[typeof(HatredAndDelightPower)] = Vanilla("enrage"),
		[typeof(OutlawPower)] = new IconPair("res://Valencina/images/powers/outlaw_power.png", "res://Valencina/images/powers/big/outlaw_power.png"),
		[typeof(PendingDisposalPower)] = Vanilla("foregone_conclusion")
	};

	public static IEnumerable<string> AllExplicitIconPaths => from path in ExplicitMap.Values.SelectMany((IconPair pair) => new string[2] { pair.Packed, pair.Big }).Distinct()
		where path.StartsWith("res://Valencina/images/powers/", StringComparison.Ordinal) || path.StartsWith("res://Valencina/images/powers/big/", StringComparison.Ordinal) || ResourceLoader.Exists(path, "")
		select path;

	private static IconPair Vanilla(string stem)
	{
		return new IconPair("res://images/atlases/power_atlas.sprites/" + stem + "_power.tres", "res://images/powers/" + stem + "_power.png");
	}

	public static string GetPackedIconPath(Type powerType, string fallbackFileName)
	{
		if (ExplicitMap.TryGetValue(powerType, out IconPair value))
		{
			if (value.Packed.StartsWith("res://Valencina/images/powers/", StringComparison.Ordinal))
			{
				return value.Packed;
			}
			if (ResourceLoader.Exists(value.Packed, ""))
			{
				return value.Packed;
			}
		}
		string text = "res://Valencina/images/powers/" + fallbackFileName;
		if (!ResourceLoader.Exists(text, ""))
		{
			return "res://Valencina/images/powers/power.png";
		}
		return text;
	}

	public static string GetBigIconPath(Type powerType, string fallbackFileName)
	{
		if (ExplicitMap.TryGetValue(powerType, out IconPair value))
		{
			if (value.Big.StartsWith("res://Valencina/images/powers/big/", StringComparison.Ordinal))
			{
				return value.Big;
			}
			if (ResourceLoader.Exists(value.Big, ""))
			{
				return value.Big;
			}
		}
		string text = "res://Valencina/images/powers/big/" + fallbackFileName;
		if (!ResourceLoader.Exists(text, ""))
		{
			return "res://Valencina/images/powers/big/power.png";
		}
		return text;
	}
}
