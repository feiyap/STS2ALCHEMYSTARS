using System.Threading.Tasks;

namespace Valencina.ValencinaCode.Precognition;

public interface IPrecognitionVirtualCounterCard
{
	Task<bool> TriggerFromPrecognition(PrecognitionCounterContext context);
}
