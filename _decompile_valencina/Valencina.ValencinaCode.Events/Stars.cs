using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Acts;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Events;

public sealed class Stars : ModAncientEventTemplate
{
	public const string BackgroundTexturePath = "res://Valencina/images/events/stars_background.webp";

	public const string BackgroundScenePath = "res://scenes/events/background_scenes/stars.tscn";

	public const string MapIconPath = "res://Valencina/images/ui/map/stars_map_icon.webp";

	private const int RelicsPerAct = 2;

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Act<ValencinaAct4>();

	public override LocString InitialDescription => ((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.INITIAL.description");

	public override string AmbientBgm => string.Empty;

	public override Color ButtonColor => new Color("12002e99");

	public override Color DialogueColor => new Color("38256f");

	public override EventAssetProfile AssetProfile => new EventAssetProfile((string)null, (string)null, "res://scenes/events/background_scenes/stars.tscn", (string)null);

	public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new AncientEventPresentationAssetProfile("res://Valencina/images/ui/map/stars_map_icon.webp", "res://Valencina/images/ui/map/stars_map_icon.webp", "res://Valencina/images/ui/map/stars_map_icon.webp", "res://Valencina/images/ui/map/stars_map_icon.webp", (AncientEventStageProceduralVisualSet)null);

	public override IEnumerable<EventOption> AllPossibleOptions => new _003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption((EventModel)(object)this, (Func<Task>)Recall, ((ModAncientEventTemplate)this).InitialOptionKey("RECALL"), Array.Empty<IHoverTip>()));

	public override bool IsAllowed(IRunState runState)
	{
		return true;
	}

	public override bool IsValidForAct(ActModel act)
	{
		return act is ValencinaAct4;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return ((AncientEventModel)this).AllPossibleOptions.ToList();
	}

	private async Task Recall()
	{
		Player val = ((EventModel)this).Owner ?? throw new InvalidOperationException("Stars ancient event has no owner.");
		List<Reward> list = ((val.Character is Valencina.ValencinaCode.Character.Valencina) ? CreateValencinaRewards(val) : CreateVanillaAncientRewards(val));
		await RewardsCmd.OfferCustom(val, list);
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.DONE.description"));
	}

	private static List<Reward> CreateValencinaRewards(Player owner)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		List<Reward> list = new List<Reward>();
		for (int i = 0; i < 3; i++)
		{
			foreach (ExtraAncientPoolEntry item in ValencinaExtraAncientRelicPools.DrawOptions(owner, i, 2))
			{
				RelicModel val = item.CreateCanonical().ToMutable();
				val.Owner = owner;
				list.Add((Reward)new RelicReward(val, owner));
			}
		}
		return list;
	}

	private static List<Reward> CreateVanillaAncientRewards(Player owner)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		List<Reward> list = new List<Reward>();
		for (int i = 0; i < 3; i++)
		{
			foreach (RelicModel item in DrawVanillaAncientRelics(owner, i, 2))
			{
				list.Add((Reward)new RelicReward(item, owner));
			}
		}
		return list;
	}

	private static IEnumerable<RelicModel> DrawVanillaAncientRelics(Player owner, int actIndex, int count)
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		List<RelicModel> list = (from relic in (from option in GetVanillaActAncients(actIndex).SelectMany((AncientEventModel ancient) => ancient.AllPossibleOptions)
				select option.Relic).OfType<RelicModel>()
			where !IsForbiddenStarsAncientRelic(relic)
			where owner.Relics.All((RelicModel owned) => ((AbstractModel)owned).Id != ((AbstractModel)relic).Id)
			where relic.IsAllowed(owner.RunState)
			group relic by ((AbstractModel)relic).Id into @group
			select @group.First() into relic
			select CloneRewardRelic(relic, owner) into relic
			orderby ((AbstractModel)relic).Id.Entry
			select relic).ToList();
		Rng val = new Rng((uint)((int)owner.RunState.Rng.Seed + (int)owner.NetId + (actIndex + 1) * 193 + StringHelper.GetDeterministicHashCode("ValencinaStarsAncientRewards")), 0);
		ListExtensions.UnstableShuffle<RelicModel>(list, val);
		return list.Take(count);
	}

	private static bool IsForbiddenStarsAncientRelic(RelicModel relic)
	{
		bool flag = ((relic is GoldenCompass || relic is FurCoat) ? true : false);
		bool flag2 = flag;
		if (!flag2)
		{
			string entry = ((AbstractModel)relic).Id.Entry;
			bool flag3 = ((entry == "GoldenCompass" || entry == "FurCoat") ? true : false);
			flag2 = flag3;
		}
		return flag2;
	}

	private static IEnumerable<AncientEventModel> GetVanillaActAncients(int actIndex)
	{
		return actIndex switch
		{
			0 => ((ActModel)ModelDb.Act<Overgrowth>()).AllAncients, 
			1 => ((ActModel)ModelDb.Act<Hive>()).AllAncients, 
			_ => ((ActModel)ModelDb.Act<Glory>()).AllAncients, 
		};
	}

	private static RelicModel CloneRewardRelic(RelicModel source, Player owner)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		object obj = ((!((AbstractModel)source).IsMutable) ? ((object)source.ToMutable()) : ((object)(RelicModel)((AbstractModel)source).ClonePreservingMutability()));
		((RelicModel)obj).Owner = owner;
		return (RelicModel)obj;
	}

	protected override AncientDialogueSet DefineDialogues()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		AncientDialogueSet val = new AncientDialogueSet();
		val.set_FirstVisitEverDialogue(new AncientDialogue(new string[1] { string.Empty }));
		Dictionary<string, IReadOnlyList<AncientDialogue>> dictionary = new Dictionary<string, IReadOnlyList<AncientDialogue>>();
		string key = AncientEventModel.CharKey<Valencina.ValencinaCode.Character.Valencina>();
		AncientDialogue val2 = new AncientDialogue(new string[1] { string.Empty });
		val2.set_VisitIndex((int?)0);
		dictionary[key] = new _003C_003Ez__ReadOnlySingleElementList<AncientDialogue>(val2);
		val.set_CharacterDialogues(dictionary);
		val.set_AgnosticDialogues((IReadOnlyList<AncientDialogue>)new _003C_003Ez__ReadOnlySingleElementList<AncientDialogue>(new AncientDialogue(new string[1] { string.Empty })));
		return val;
	}
}
