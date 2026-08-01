using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Events;

public sealed class LucioChoiceEvent : EventModel, IModEventAssetOverrides
{
	public const string BackgroundScenePath = "res://scenes/events/background_scenes/lucio_choice.tscn";

	public const string BackgroundTexturePath = "res://Valencina/images/events/lucio_choice_background.png";

	public AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Character<Valencina.ValencinaCode.Character.Valencina>();

	public EventAssetProfile AssetProfile => new EventAssetProfile((string)null, "res://Valencina/images/events/lucio_choice_background.png", "res://scenes/events/background_scenes/lucio_choice.tscn", (string)null);

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[7]
	{
		(DynamicVar)new MaxHpVar("MaxHpLoss", 10m),
		(DynamicVar)new MaxHpVar("MaxHpGain", 10m),
		(DynamicVar)new CardsVar(1),
		(DynamicVar)new EnergyVar(1),
		new DynamicVar("Potions", 3m),
		(DynamicVar)new StringVar("Lucio", ((CardModel)ModelDb.Card<Lucio>()).Title),
		(DynamicVar)new StringVar("Heart", ((CardModel)ModelDb.Card<Shin>()).Title)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		string text = ((AbstractModel)this).Id.Entry + ".pages.INITIAL.options.";
		Player owner = ((EventModel)this).Owner;
		bool flag = ((owner != null) ? owner.Character : null) is Valencina.ValencinaCode.Character.Valencina && FindLucioInDeck() != null;
		return new _003C_003Ez__ReadOnlyArray<EventOption>((EventOption[])(object)new EventOption[3]
		{
			new EventOption((EventModel)(object)this, flag ? new Func<Task>(AimForHeart) : null, text + "AIM", HoverTipFactory.FromCardWithCardHoverTips<Shin>(false)),
			new EventOption((EventModel)(object)this, flag ? new Func<Task>(KickLucioAway) : null, text + "KICK", HoverTipFactory.FromCardWithCardHoverTips<Lucio>(false)),
			new EventOption((EventModel)(object)this, (Func<Task>)Ignore, text + "IGNORE", Array.Empty<IHoverTip>())
		});
	}

	public override bool IsAllowed(IRunState runState)
	{
		return true;
	}

	private async Task AimForHeart()
	{
		Player owner = ((EventModel)this).Owner ?? throw new InvalidOperationException("Lucio choice event has no owner.");
		Lucio lucio = FindLucioInDeck();
		if (!(owner.Character is Valencina.ValencinaCode.Character.Valencina) || lucio == null)
		{
			((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.IGNORE.description"));
			return;
		}
		await CardPileCmd.RemoveFromDeck((CardModel)(object)lucio, true);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add((CardModel)(object)((ICardScope)owner.RunState).CreateCard<Shin>(owner), (PileType)6, (CardPilePosition)1, (AbstractModel)null, false), 2f, (CardPreviewStyle)1);
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.AIM.description"));
	}

	private async Task KickLucioAway()
	{
		Player val = ((EventModel)this).Owner ?? throw new InvalidOperationException("Lucio choice event has no owner.");
		Lucio lucio = FindLucioInDeck();
		if (!(val.Character is Valencina.ValencinaCode.Character.Valencina) || lucio == null)
		{
			((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.IGNORE.description"));
			return;
		}
		await CreatureCmd.LoseMaxHp((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), val.Creature, ((EventModel)this).DynamicVars["MaxHpLoss"].BaseValue, false);
		lucio.ApplyHeartGuardEnhancement();
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.KICK.description"));
	}

	private async Task Ignore()
	{
		Player owner = ((EventModel)this).Owner ?? throw new InvalidOperationException("Lucio choice event has no owner.");
		await CreatureCmd.GainMaxHp(owner.Creature, ((EventModel)this).DynamicVars["MaxHpGain"].BaseValue);
		List<Reward> list = (from potion in PotionFactory.CreateRandomPotionsOutOfCombat(owner, ((EventModel)this).DynamicVars["Potions"].IntValue, owner.PlayerRng.Rewards, (IEnumerable<PotionModel>)null)
			select (Reward)new PotionReward(potion.ToMutable(), owner)).ToList();
		if (list.Count > 0)
		{
			await RewardsCmd.OfferCustom(owner, list);
		}
		((EventModel)this).SetEventFinished(((EventModel)this).L10NLookup(((AbstractModel)this).Id.Entry + ".pages.IGNORE.description"));
	}

	private Lucio? FindLucioInDeck()
	{
		Player owner = ((EventModel)this).Owner;
		if (owner == null)
		{
			return null;
		}
		return owner.Deck.Cards.OfType<Lucio>().FirstOrDefault();
	}
}
