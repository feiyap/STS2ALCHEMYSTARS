using Godot;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Character;

public class ValencinaPotionPool : TypeListPotionPoolModel
{
	public override Color LabOutlineColor => Valencina.Color;

	public override string EnergyColorName => "Valencina";

	public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();

	public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
