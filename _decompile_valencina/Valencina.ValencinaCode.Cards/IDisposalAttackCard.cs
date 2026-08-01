namespace Valencina.ValencinaCode.Cards;

public interface IDisposalAttackCard
{
	int Insight { get; set; }

	int ExtraHits { get; set; }

	int ExtraTremorDetonations { get; set; }

	bool ForceZeroCost { get; set; }

	bool ForceUpgrade { get; set; }
}
