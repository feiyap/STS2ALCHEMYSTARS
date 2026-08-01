namespace Valencina.ValencinaCode.Cards;

public readonly record struct DisposalGenerationEnhancement(int ExtraHits, int ExtraTremorDetonations, bool ForceZeroCost, bool UpgradeGeneratedDisposal)
{
	public static readonly DisposalGenerationEnhancement None = new DisposalGenerationEnhancement(0, 0, ForceZeroCost: false, UpgradeGeneratedDisposal: false);

	public static readonly DisposalGenerationEnhancement Will = new DisposalGenerationEnhancement(0, 0, ForceZeroCost: false, UpgradeGeneratedDisposal: true);
}
