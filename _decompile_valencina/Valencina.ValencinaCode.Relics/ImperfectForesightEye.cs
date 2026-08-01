using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Relics;

public class ImperfectForesightEye : ValencinaRelic, IValencinaCounterLevelSource
{
	private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public new bool Equals(object? x, object? y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private static readonly HashSet<object> FirstEmptyRefillUsedThisCombat = new HashSet<object>(ReferenceEqualityComparer.Instance);

	private static readonly HashSet<object> FirstTremorConversionUsedThisCombat = new HashSet<object>(ReferenceEqualityComparer.Instance);

	private int _counterLevel;

	public override RelicRarity Rarity => (RelicRarity)1;

	[SavedProperty]
	public int CounterLevel
	{
		get
		{
			return _counterLevel;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_counterLevel = 0;
		}
	}

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return CompatHoverTips.FromPower<InstantForesightPower>();
			yield return CompatHoverTips.FromPower<ValencinaShinPower>();
		}
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		return false;
	}

	public static void ResetCombatState()
	{
		FirstTremorConversionUsedThisCombat.Clear();
	}

	public RelicModel GetUpgradeReplacement()
	{
		if (!((AbstractModel)this).IsMutable)
		{
			return (RelicModel)(object)ModelDb.Relic<CompleteForesightEye>();
		}
		return (RelicModel)(object)(CompleteForesightEye)(object)((RelicModel)ModelDb.Relic<CompleteForesightEye>()).ToMutable();
	}

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			await CommonActions.Apply<InstantForesightPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, (CardModel?)null, 30m, silent: false);
		}
	}

	public static async Task<bool> TryHandleTremorAppliedAsync(Creature? target, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		if (target == null)
		{
			return false;
		}
		object obj;
		if (sourceCard == null)
		{
			obj = null;
		}
		else
		{
			Player owner = sourceCard.Owner;
			obj = ((owner != null) ? owner.GetRelic<ImperfectForesightEye>() : null);
		}
		ImperfectForesightEye imperfectForesightEye = (ImperfectForesightEye)obj;
		if (imperfectForesightEye == null || FirstTremorConversionUsedThisCombat.Contains(imperfectForesightEye))
		{
			return false;
		}
		FirstTremorConversionUsedThisCombat.Add(imperfectForesightEye);
		((RelicModel)imperfectForesightEye).Flash();
		bool flag = await StatusSystem.TryConvertTremorToBurningAsync(target, sourceCard, choiceContext);
		MainFile.Logger.Info($"[ImperfectForesightEye] first tremor conversion triggered. success={flag}.", 1);
		return flag;
	}

	public static async Task<bool> TryHandleAmmoDepletedAsync(Creature? owner, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		await Task.CompletedTask;
		return false;
	}
}
