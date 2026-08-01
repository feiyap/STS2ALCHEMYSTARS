using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Precognition;

public interface IPrecognitionCounterCardProvider
{
	int Priority { get; }

	bool CanProvide(PrecognitionCounterContext context);

	CardModel CreateCounterCard(PrecognitionCounterContext context);
}
