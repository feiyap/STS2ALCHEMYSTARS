using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Relics;
using Valencina.ValencinaCode.RestSite;

namespace Valencina.ValencinaCode.Cards;

public sealed class Vagrant : ValencinaCard
{
	public override bool GainsBlock => true;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar(3m, (ValueProp)8));

	public Vagrant()
		: base(1, (CardType)2, (CardRarity)6, (TargetType)1)
	{
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != ((CardModel)this).Owner)
		{
			return false;
		}
		if (!(player.Character is Valencina.ValencinaCode.Character.Valencina))
		{
			return false;
		}
		if (player.GetRelic<LucioRelic>() != null)
		{
			return false;
		}
		if (!player.Deck.Cards.Contains((CardModel)(object)this))
		{
			return false;
		}
		if (player.Deck.Cards.Any((CardModel card) => card is Lucio))
		{
			return false;
		}
		if (options.Any((RestSiteOption option) => option.OptionId == "LUCIO_TRAINING"))
		{
			return false;
		}
		if (!ResourceLoader.Exists("res://images/ui/rest_site/option_lucio_training.png", ""))
		{
			MainFile.Logger.Warn("[LucioTraining] Missing rest site icon: res://images/ui/rest_site/option_lucio_training.png. Training option was not added.", 1);
			return false;
		}
		options.Add((RestSiteOption)(object)new LucioTrainingRestSiteOption(player));
		return true;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(2m);
	}
}
