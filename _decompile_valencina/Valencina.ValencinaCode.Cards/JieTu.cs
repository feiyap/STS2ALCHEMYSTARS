using MegaCrit.Sts2.Core.Entities.Cards;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Cards;

public sealed class JieTu : CounterStyleCard
{
	public JieTu()
		: base(ValencinaCounterStyle.JieTu, 4, 5, 0, 0, "jie_tu.png", (CardRarity)3)
	{
	}
}
