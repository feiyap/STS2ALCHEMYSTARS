using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Systems.Duel;

namespace Valencina.ValencinaCode.Events;

public sealed class DuelMemoryEvent : EventModel, IModEventAssetOverrides
{
	private const string LocKey = "VALENCINA_EVENT_DUEL_MEMORY_EVENT";

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Character<Valencina.ValencinaCode.Character.Valencina>();

	public EventAssetProfile AssetProfile => new EventAssetProfile((string)null, "res://Valencina/images/ui/map/duel_node.svg", (string)null, (string)null);

	public override bool IsShared => true;

	public override LocString InitialDescription => ((EventModel)this).L10NLookup("VALENCINA_EVENT_DUEL_MEMORY_EVENT.pages.INITIAL.description");

	public override bool IsAllowed(IRunState runState)
	{
		return DuelNodeSystem.IsDuelPoint(runState);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new _003C_003Ez__ReadOnlyArray<EventOption>((EventOption[])(object)new EventOption[2]
		{
			CreateOption(Accept, "ACCEPT"),
			CreateOption(Watch, "WATCH")
		});
	}

	private EventOption CreateOption(Func<Task> onChosen, string optionId)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		string text = "VALENCINA_EVENT_DUEL_MEMORY_EVENT.pages.INITIAL.options." + optionId;
		return new EventOption((EventModel)(object)this, onChosen, ((EventModel)this).L10NLookup(text + ".title"), ((EventModel)this).L10NLookup(text + ".description"), text, (IEnumerable<IHoverTip>)Array.Empty<IHoverTip>());
	}

	private Task Accept()
	{
		Player owner = ((EventModel)this).Owner ?? throw new InvalidOperationException("Duel event has no owner.");
		EncounterModel val = ((EncounterModel)ModelDb.Encounter<DuelEncounter>()).ToMutable();
		IReadOnlyList<Reward> readOnlyList = CreateVictoryRewards(owner);
		((EventModel)this).EnterCombatWithoutExitingEvent(val, readOnlyList, false);
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup("VALENCINA_EVENT_DUEL_MEMORY_EVENT.pages.ACCEPT.description"));
		return Task.CompletedTask;
	}

	private Task Watch()
	{
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup("VALENCINA_EVENT_DUEL_MEMORY_EVENT.pages.WATCH.description"));
		return Task.CompletedTask;
	}

	private static IReadOnlyList<Reward> CreateVictoryRewards(Player owner)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		int num = 1;
		List<Reward> list = new List<Reward>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Reward> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = (Reward)new GoldReward(75, owner, false);
		RelicModel val = DuelNodeSystem.CreateValencinaAncientReward(owner);
		list.Add((Reward)((val != null) ? new RelicReward(val, owner) : new RelicReward((RelicRarity)3, owner)));
		return list;
	}
}
