using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class AmmoPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	private bool IsWarNoAutoReload
	{
		get
		{
			object runState;
			if (!((AbstractModel)this).IsMutable)
			{
				IRunState val = (IRunState)(object)RunManager.Instance.DebugOnlyGetState();
				runState = val;
			}
			else
			{
				Creature owner = ((PowerModel)this).Owner;
				if (owner == null)
				{
					runState = null;
				}
				else
				{
					Player player = owner.Player;
					runState = ((player != null) ? player.RunState : null);
				}
			}
			return ValencinaWarDifficulty.IsActive((IRunState?)runState);
		}
	}

	public override LocString Description
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (!IsWarNoAutoReload)
			{
				return ((PowerModel)this).Description;
			}
			return new LocString("powers", "VALENCINA.war_ammo.description");
		}
	}

	protected override string SmartDescriptionLocKey
	{
		get
		{
			if (!IsWarNoAutoReload)
			{
				return ((PowerModel)this).SmartDescriptionLocKey;
			}
			return "VALENCINA.war_ammo.description";
		}
	}

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			if (IsWarNoAutoReload)
			{
				yield break;
			}
			foreach (IHoverTip additionalHoverTip in base.AdditionalHoverTips)
			{
				yield return additionalHoverTip;
			}
		}
	}

	public void SyncAmount(int amount)
	{
		if (amount < 0)
		{
			amount = 0;
		}
		((PowerModel)this).SetAmount(amount, false);
		((PowerModel)this).InitInternalData();
		((PowerModel)this).InvokeDisplayAmountChanged();
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner)
		{
			AmmoState.StartPlayerTurn(((PowerModel)this).Owner);
			if (!ValencinaWarDifficulty.IsActive(player.RunState))
			{
				await AmmoSystem.AddAmmoAsync(((PowerModel)this).Owner, 2, null, choiceContext);
			}
		}
	}
}
