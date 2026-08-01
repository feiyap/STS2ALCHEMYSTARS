using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.RestSite;

public sealed class LucioTrainingRestSiteOption : RestSiteOption
{
	public const string OptionIdValue = "LUCIO_TRAINING";

	public override string OptionId => "LUCIO_TRAINING";

	public LucioTrainingRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		Vagrant vagrant = ((RestSiteOption)this).Owner.Deck.Cards.OfType<Vagrant>().FirstOrDefault();
		if (vagrant == null)
		{
			return false;
		}
		await CardCmd.TransformTo<Lucio>((CardModel)(object)vagrant, (CardPreviewStyle)1);
		if (((RestSiteOption)this).Owner.GetRelic<LucioRelic>() == null)
		{
			await RelicCmd.Obtain<LucioRelic>(((RestSiteOption)this).Owner);
		}
		return true;
	}
}
