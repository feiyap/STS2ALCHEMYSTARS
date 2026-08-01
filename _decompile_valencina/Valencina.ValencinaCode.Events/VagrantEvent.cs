using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Events;

public sealed class VagrantEvent : EventModel, IModEventAssetOverrides
{
	public const string PortraitPath = "res://Valencina/images/events/vagrant.png";

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Character<Valencina.ValencinaCode.Character.Valencina>();

	public EventAssetProfile AssetProfile => new EventAssetProfile((string)null, "res://Valencina/images/events/vagrant.png", (string)null, (string)null);

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new MaxHpVar(7m),
		(DynamicVar)new StringVar("Card", ((CardModel)ModelDb.Card<Vagrant>()).Title)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		string text = ((AbstractModel)this).Id.Entry + ".pages.INITIAL.options.IGNORE";
		string text2 = ((AbstractModel)this).Id.Entry + ".pages.INITIAL.options.TAKE";
		Player owner = ((EventModel)this).Owner;
		bool flag = ((owner != null) ? owner.Character : null) is Valencina.ValencinaCode.Character.Valencina;
		return new _003C_003Ez__ReadOnlyArray<EventOption>((EventOption[])(object)new EventOption[2]
		{
			new EventOption((EventModel)(object)this, (Func<Task>)Ignore, text, Array.Empty<IHoverTip>()),
			new EventOption((EventModel)(object)this, flag ? new Func<Task>(Take) : null, text2, HoverTipFactory.FromCardWithCardHoverTips<Vagrant>(false))
		});
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (VagrantEventState.IsForcedAllowed(runState) && ((IPlayerCollection)runState).Players.Any((Player player) => player.Character is Valencina.ValencinaCode.Character.Valencina))
		{
			return !VagrantEventState.AnyPlayerAlreadyHasVagrantReward(runState);
		}
		return false;
	}

	private async Task Ignore()
	{
		await CreatureCmd.GainMaxHp((((EventModel)this).Owner ?? throw new InvalidOperationException("Vagrant event has no owner.")).Creature, ((DynamicVar)((EventModel)this).DynamicVars.MaxHp).BaseValue);
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.IGNORE.description"));
	}

	private async Task Take()
	{
		Player val = ((EventModel)this).Owner ?? throw new InvalidOperationException("Vagrant event has no owner.");
		if (!(val.Character is Valencina.ValencinaCode.Character.Valencina))
		{
			((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.TAKE_LOCKED.description"));
			return;
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add((CardModel)(object)((ICardScope)val.RunState).CreateCard<Vagrant>(val), (PileType)6, (CardPilePosition)1, (AbstractModel)null, false), 2f, (CardPreviewStyle)1);
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.TAKE.description"));
	}
}
