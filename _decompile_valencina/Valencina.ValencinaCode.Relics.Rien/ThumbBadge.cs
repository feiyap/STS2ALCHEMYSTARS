using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class ThumbBadge : RienRelic
{
	private const string AmountKey = "Amount";

	public override bool HasUponPickupEffect => true;

	public override string? CustomIconOutlinePath => ((ModRelicTemplate)this).CustomIconPath;

	public int AmmoCapacityBonus => ((RelicModel)this).DynamicVars["Amount"].IntValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("Amount", 4m)
	};

	public override async Task AfterObtained()
	{
		Player owner = ((RelicModel)this).Owner;
		await AmmoSystem.IncreaseMaxAmmoAsync((owner != null) ? owner.Creature : null, ((RelicModel)this).DynamicVars["Amount"].IntValue);
	}
}
