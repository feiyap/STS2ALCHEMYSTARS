using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Character;

public class ValencinaCardPool : TypeListCardPoolModel
{
	private static ShaderMaterial? _poolFrameMaterial;

	public override string Title => "Valencina";

	public override string EnergyColorName => ((CardPoolModel)this).Title;

	public override string CardFrameMaterialPath => "card_frame_red";

	public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();

	public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

	public override Material? PoolFrameMaterial => (Material?)(object)(_poolFrameMaterial ?? (_poolFrameMaterial = MaterialUtils.CreateHsvShaderMaterial(1f, 1f, 1f)));

	public override Color DeckEntryCardColor => new Color("6d0f0f");

	public override bool IsColorless => false;
}
