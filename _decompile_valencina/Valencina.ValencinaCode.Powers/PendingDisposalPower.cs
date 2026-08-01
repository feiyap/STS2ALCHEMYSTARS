using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Powers;

public sealed class PendingDisposalPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private const int WillFlag = 1000;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override int DisplayAmount => -1;

	public int DestinedFutureStacks => Math.Max(0, EncodedValue + (HasWillEnhancement ? (-1000) : 0) - 1);

	public bool HasWillEnhancement => EncodedInsight >= 1000;

	private int EncodedInsight => EncodedValue;

	private int EncodedValue => Math.Max(0, ((PowerModel)this).Amount);

	private DisposalGenerationEnhancement TotalEnhancement => new DisposalGenerationEnhancement(DestinedFutureStacks + (HasWillEnhancement ? DisposalGenerationEnhancement.Will.ExtraHits : 0), DestinedFutureStacks + (HasWillEnhancement ? DisposalGenerationEnhancement.Will.ExtraTremorDetonations : 0), HasWillEnhancement && DisposalGenerationEnhancement.Will.ForceZeroCost, HasWillEnhancement && DisposalGenerationEnhancement.Will.UpgradeGeneratedDisposal);

	public static decimal Encode(int destinedFutureStacks, DisposalGenerationEnhancement enhancement)
	{
		int num = Math.Max(0, destinedFutureStacks) + 1;
		if (enhancement.Equals(DisposalGenerationEnhancement.Will))
		{
			num += 1000;
		}
		return num;
	}

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Insight", 0m);
		description.Add("InsightPercentPerPrecognition", 0m);
		DisposalGenerationEnhancement totalEnhancement = TotalEnhancement;
		description.Add("ExtraHits", (decimal)totalEnhancement.ExtraHits);
		description.Add("ExtraDetonations", (decimal)totalEnhancement.ExtraTremorDetonations);
		description.Add("ForceZero", (decimal)(HasWillEnhancement ? 1 : 0));
		description.Add("Upgrade", (decimal)(totalEnhancement.UpgradeGeneratedDisposal ? 1 : 0));
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner == null || ((PowerModel)this).Owner.Player != player)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState == null)
		{
			MainFile.Logger.Warn("[PendingDisposalPower] Could not create Disposal: owner has no CombatState.", 1);
			await PowerCmd.Remove((PowerModel)(object)this);
			return;
		}
		((PowerModel)this).Flash();
		DisposalGenerationEnhancement totalEnhancement = TotalEnhancement;
		LieInWaitPower power = ((PowerModel)this).Owner.GetPower<LieInWaitPower>();
		bool forceRetain = power != null && ((PowerModel)power).Amount > 0;
		FutureDisposal futureDisposal = DisposalAttackHelper.Configure(combatState.CreateCard<FutureDisposal>(player), 0, totalEnhancement, forceRetain);
		if (totalEnhancement.UpgradeGeneratedDisposal && ((CardModel)futureDisposal).IsUpgradable)
		{
			CardCmd.Upgrade((CardModel)(object)futureDisposal, (CardPreviewStyle)2);
		}
		await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)futureDisposal, (PileType)2, player, (CardPilePosition)2);
		await PowerCmd.Remove((PowerModel)(object)this);
	}
}
