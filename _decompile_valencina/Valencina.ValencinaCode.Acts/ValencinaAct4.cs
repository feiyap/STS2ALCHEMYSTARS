using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Acts;

public sealed class ValencinaAct4 : ModActTemplate
{
	public override int Index => 3;

	public override bool IsDefault => false;

	public override ActAssetProfile AssetProfile => ContentAssetProfiles.FromVanillaActId("glory");

	public override string ChestOpenSfx => ((ActModel)GloryAct).ChestOpenSfx;

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => new _003C_003Ez__ReadOnlySingleElementList<EncounterModel>((EncounterModel)(object)ModelDb.Encounter<UngezieferKaiserEncounter>());

	public override IEnumerable<AncientEventModel> AllAncients => new _003C_003Ez__ReadOnlySingleElementList<AncientEventModel>((AncientEventModel)(object)ModelDb.AncientEvent<Stars>());

	public override IEnumerable<EventModel> AllEvents => new _003C_003Ez__ReadOnlySingleElementList<EventModel>((EventModel)(object)ModelDb.Event<LucioChoiceEvent>());

	protected override int NumberOfWeakEncounters => 0;

	protected override int BaseNumberOfRooms => 5;

	public override string[] BgMusicOptions => new string[2] { "event:/music/act3_a1_v1", "event:/music/act3_a2_v1" };

	public override string[] MusicBankPaths => new string[2] { "res://banks/desktop/act3_a1.bank", "res://banks/desktop/act3_a2.bank" };

	public override string AmbientSfx => ((ActModel)GloryAct).AmbientSfx;

	public override string ChestSpineResourcePath => ((ActModel)GloryAct).ChestSpineResourcePath;

	public override string ChestSpineSkinNameNormal => ((ActModel)GloryAct).ChestSpineSkinNameNormal;

	public override string ChestSpineSkinNameStroke => ((ActModel)GloryAct).ChestSpineSkinNameStroke;

	public override Color MapTraveledColor => ((ActModel)GloryAct).MapTraveledColor;

	public override Color MapUntraveledColor => ((ActModel)GloryAct).MapUntraveledColor;

	public override Color MapBgColor => ((ActModel)GloryAct).MapBgColor;

	private static Glory GloryAct => ModelDb.Act<Glory>();

	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		return new _003C_003Ez__ReadOnlyArray<EncounterModel>((EncounterModel[])(object)new EncounterModel[2]
		{
			(EncounterModel)ModelDb.Encounter<ValencinaAct4EliteEncounter>(),
			(EncounterModel)ModelDb.Encounter<UngezieferKaiserEncounter>()
		});
	}

	internal void NormalizeFixedRouteRooms()
	{
		EventModel val = (EventModel)(object)ModelDb.Event<LucioChoiceEvent>();
		if (((ActModel)this)._rooms.events.Count != 1 || ((AbstractModel)((ActModel)this)._rooms.events[0]).Id != ((AbstractModel)val).Id)
		{
			((ActModel)this)._rooms.events.Clear();
			((ActModel)this)._rooms.events.Add(val);
			((ActModel)this)._rooms.eventsVisited = 0;
		}
		EncounterModel val2 = (EncounterModel)(object)ModelDb.Encounter<ValencinaAct4EliteEncounter>();
		if (((ActModel)this)._rooms.eliteEncounters.Count != 1 || ((AbstractModel)((ActModel)this)._rooms.eliteEncounters[0]).Id != ((AbstractModel)val2).Id)
		{
			((ActModel)this)._rooms.eliteEncounters.Clear();
			((ActModel)this)._rooms.eliteEncounters.Add(val2);
			((ActModel)this)._rooms.eliteEncountersVisited = 0;
		}
		if (((ActModel)this)._rooms.normalEncounters.Count != 0)
		{
			((ActModel)this)._rooms.normalEncounters.Clear();
			((ActModel)this)._rooms.normalEncountersVisited = 0;
		}
		((ActModel)this)._rooms.Ancient = (AncientEventModel)(object)ModelDb.AncientEvent<Stars>();
		((ActModel)this)._rooms.Boss = (EncounterModel)(object)ModelDb.Encounter<UngezieferKaiserEncounter>();
		((ActModel)this)._rooms.SecondBoss = null;
	}

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
	{
		return ((ActModel)this).AllAncients.ToList();
	}

	public override bool IsUnlocked(UnlockState unlockState)
	{
		return true;
	}

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		return new MapPointTypeCounts(1, 1);
	}
}
