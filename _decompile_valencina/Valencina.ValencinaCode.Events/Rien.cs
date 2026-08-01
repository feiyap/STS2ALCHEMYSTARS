using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Events;

public class Rien : ModAncientEventTemplate
{
	public const string RunHistoryIconPath = "res://Valencina/images/ui/run_history/rien.png";

	public const string RunHistoryIconOutlinePath = "res://Valencina/images/ui/run_history/rien_outline.png";

	public const string BackgroundScenePath = "res://scenes/events/background_scenes/rien.tscn";

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Character<Valencina.ValencinaCode.Character.Valencina>();

	public override LocString InitialDescription => ((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.INITIAL.description");

	public override string AmbientBgm => string.Empty;

	public override EventAssetProfile AssetProfile => new EventAssetProfile((string)null, (string)null, "res://scenes/events/background_scenes/rien.tscn", (string)null);

	public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new AncientEventPresentationAssetProfile("res://Valencina/images/ui/run_history/rien.png", "res://Valencina/images/ui/run_history/rien_outline.png", "res://Valencina/images/ui/run_history/rien.png", "res://Valencina/images/ui/run_history/rien_outline.png", (AncientEventStageProceduralVisualSet)null);

	public override IEnumerable<EventOption> AllPossibleOptions => Options;

	private IEnumerable<EventOption> Options => (IEnumerable<EventOption>)(object)new EventOption[3]
	{
		((ModAncientEventTemplate)this).CreateModRelicOption<MagicBeeper>("INITIAL"),
		((ModAncientEventTemplate)this).CreateModRelicOption<ScatteredOracle>("INITIAL"),
		((ModAncientEventTemplate)this).CreateModRelicOption<RevengeLedgerAppendix>("INITIAL")
	};

	public override bool IsAllowed(IRunState runState)
	{
		return ((IPlayerCollection)runState).Players.Any((Player player) => player.Character is Valencina.ValencinaCode.Character.Valencina);
	}

	public override bool IsValidForAct(ActModel act)
	{
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return Options.ToList();
	}

	protected override Task BeforeEventStarted(bool isPreFinished)
	{
		return Task.CompletedTask;
	}

	protected override AncientDialogueSet DefineDialogues()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		AncientDialogueSet val = new AncientDialogueSet();
		val.set_FirstVisitEverDialogue(new AncientDialogue(new string[1] { string.Empty }));
		Dictionary<string, IReadOnlyList<AncientDialogue>> dictionary = new Dictionary<string, IReadOnlyList<AncientDialogue>>();
		string key = AncientEventModel.CharKey<Valencina.ValencinaCode.Character.Valencina>();
		AncientDialogue[] array = new AncientDialogue[1];
		AncientDialogue val2 = new AncientDialogue(new string[1] { string.Empty });
		val2.set_VisitIndex((int?)0);
		array[0] = val2;
		dictionary[key] = (IReadOnlyList<AncientDialogue>)(object)array;
		val.set_CharacterDialogues(dictionary);
		val.set_AgnosticDialogues((IReadOnlyList<AncientDialogue>)(object)new AncientDialogue[1]
		{
			new AncientDialogue(new string[1] { string.Empty })
		});
		return val;
	}
}
