using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class SoHotPower : ValencinaPower, IBurnReducedListener
{
	private bool _echoing;

	private CardModel? _sourceCard;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_sourceCard = cardSource;
		return Task.CompletedTask;
	}

	public async Task OnEnemyBurnReducedAsync(PlayerChoiceContext choiceContext, Creature target, int reducedAmount)
	{
		if (_echoing || reducedAmount <= 0 || target.IsDead || !target.IsAlive)
		{
			return;
		}
		try
		{
			_echoing = true;
			((PowerModel)this).Flash();
			await StatusSystem.ApplyBurnAsync(target, reducedAmount, _sourceCard, choiceContext);
		}
		finally
		{
			_echoing = false;
		}
	}
}
