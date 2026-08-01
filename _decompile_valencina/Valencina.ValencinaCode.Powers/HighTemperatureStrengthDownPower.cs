using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Powers;

public sealed class HighTemperatureStrengthDownPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
	public PowerAssetProfile AssetProfile => new PowerAssetProfile(CustomIconPath, CustomBigIconPath);

	public string? CustomIconPath => PowerIconRegistry.GetPackedIconPath(((object)this).GetType(), "strength_power.png");

	public string? CustomBigIconPath => PowerIconRegistry.GetBigIconPath(((object)this).GetType(), "strength_power.png");

	public override AbstractModel OriginModel => (AbstractModel)(object)ModelDb.Card<HighTemperature>();

	protected override bool IsPositive => false;
}
