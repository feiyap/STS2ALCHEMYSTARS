using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class WellPreparedPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			await CardPileCmd.Draw(choiceContext, (decimal)((PowerModel)this).Amount, player, false);
			await AmmoSystem.ReloadToFullAsync(((PowerModel)this).Owner, null, choiceContext);
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
