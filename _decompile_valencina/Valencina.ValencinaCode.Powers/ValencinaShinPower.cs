using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Vfx;

namespace Valencina.ValencinaCode.Powers;

public sealed class ValencinaShinPower : ValencinaPower
{
	private sealed class ReferenceEqualityComparer : IEqualityComparer<Creature>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public bool Equals(Creature? x, Creature? y)
		{
			return x == y;
		}

		public int GetHashCode(Creature obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private static readonly HashSet<Creature> StrengthDexterityGrantedThisCombat = new HashSet<Creature>(ReferenceEqualityComparer.Instance);

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public static void ResetCombatState()
	{
		StrengthDexterityGrantedThisCombat.Clear();
	}

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (((PowerModel)this).Owner == null || target != ((PowerModel)this).Owner || !(canonicalPower is ValencinaShinPower) || amount <= 0m)
		{
			return false;
		}
		modifiedAmount = default(decimal);
		return true;
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (((PowerModel)this).Owner != null)
		{
			((PowerModel)this).Flash();
			ShinAuraController.Show(((PowerModel)this).Owner);
			if (!StrengthDexterityGrantedThisCombat.Add(((PowerModel)this).Owner))
			{
				MainFile.Logger.Info("[ValencinaShinPower] duplicate apply ignored for Strength/Dexterity bonus.", 1);
				return;
			}
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, 1m, ((PowerModel)this).Owner, cardSource, silent: false);
			await CompatPowerCmd.Apply<DexterityPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, 1m, ((PowerModel)this).Owner, cardSource, silent: false);
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		ShinAuraController.Refresh(oldOwner);
		await Task.CompletedTask;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner)
		{
			((PowerModel)this).Flash();
			ShinAuraController.Show(((PowerModel)this).Owner);
			await CardPileCmd.Draw(choiceContext, 1m, player, false);
		}
	}
}
