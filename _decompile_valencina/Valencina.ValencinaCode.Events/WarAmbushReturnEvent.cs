using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Events;

public sealed class WarAmbushReturnEvent : EventModel, IModEventAssetOverrides
{
	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Character<Valencina.ValencinaCode.Character.Valencina>();

	public EventAssetProfile AssetProfile => new EventAssetProfile((string)null, "res://Valencina/images/events/lucio_choice_background.png", "res://scenes/events/background_scenes/lucio_choice.tscn", (string)null);

	public override bool IsDeterministic => true;

	public override bool IsAllowed(IRunState runState)
	{
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		return new _003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption((EventModel)(object)this, (Func<Task>)ReturnToMap, ((AbstractModel)this).Id.Entry + ".pages.INITIAL.options.RETURN", Array.Empty<IHoverTip>()));
	}

	private Task ReturnToMap()
	{
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.RETURN.description"));
		NRun instance = NRun.Instance;
		if (instance != null)
		{
			instance.GlobalUi.MapScreen.Open(false);
		}
		return Task.CompletedTask;
	}
}
