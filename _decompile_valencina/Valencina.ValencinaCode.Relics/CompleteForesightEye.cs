using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Relics;

public class CompleteForesightEye : ValencinaRelic, IValencinaCounterLevelSource
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

	private static readonly HashSet<object> AmmoDepletionHandledThisCombat = new HashSet<object>(ReferenceEqualityComparer.Instance);

	private static readonly HashSet<object> TremorConversionHandledThisTurn = new HashSet<object>(ReferenceEqualityComparer.Instance);

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
			yield return CompatHoverTips.FromPower<InstantPredictionPower>();
			yield return CompatHoverTips.FromPower<ValencinaShinPower>();
		}
	}

	public override string PackedIconPath
	{
		get
		{
			string text = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").RelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic.png".RelicImagePath();
			}
			return text;
		}
	}

	protected override string PackedIconOutlinePath
	{
		get
		{
			string text = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + "_outline.png").RelicImagePath();
			if (ResourceLoader.Exists(text, ""))
			{
				return text;
			}
			string text2 = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").RelicImagePath();
			if (ResourceLoader.Exists(text2, ""))
			{
				return text2;
			}
			string text3 = "relic.png".RelicImagePath();
			if (!ResourceLoader.Exists(text3, ""))
			{
				return text2;
			}
			return text3;
		}
	}

	protected override string BigIconPath
	{
		get
		{
			string text = (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").BigRelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic.png".BigRelicImagePath();
			}
			return text;
		}
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		return false;
	}

	public static void ResetCombatState()
	{
		TremorConversionHandledThisTurn.Clear();
	}

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			BlockingPlayerChoiceContext choiceContext = new BlockingPlayerChoiceContext();
			(await CommonActions.Apply<InstantForesightPower>((PlayerChoiceContext)(object)choiceContext, ((RelicModel)this).Owner.Creature, (CardModel?)null, 40m, silent: false))?.SetPrecognition(40);
			await CommonActions.Apply<InstantPredictionPower>((PlayerChoiceContext)(object)choiceContext, ((RelicModel)this).Owner.Creature, (CardModel?)null, 1m, silent: false);
			MainFile.Logger.Info("[CompleteForesightEye] granted Precognition cap=40 and InstantPredictionPower.", 1);
		}
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((RelicModel)this).Owner == player)
		{
			TremorConversionHandledThisTurn.Remove(this);
		}
		return Task.CompletedTask;
	}

	public static async Task<bool> TryHandleTremorAppliedAsync(Creature? target, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		Player obj = ((sourceCard != null) ? sourceCard.Owner : null);
		CompleteForesightEye completeForesightEye = ((obj != null) ? obj.GetRelic<CompleteForesightEye>() : null);
		if (completeForesightEye == null || TremorConversionHandledThisTurn.Contains(completeForesightEye))
		{
			return false;
		}
		TremorConversionHandledThisTurn.Add(completeForesightEye);
		((RelicModel)completeForesightEye).Flash();
		if (CreaturePowerAccess.Find<BurningTremorPower>(target) != null)
		{
			return true;
		}
		TremorPower tremorPower = CreaturePowerAccess.Find<TremorPower>(target);
		if (tremorPower == null || ((PowerModel)tremorPower).Amount <= 0)
		{
			return false;
		}
		await StatusSystem.TryConvertTremorToBurningAsync(target, sourceCard, choiceContext);
		return true;
	}

	public static async Task<bool> TryHandleAmmoDepletedAsync(Creature? owner, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		await Task.CompletedTask;
		return false;
	}
}
