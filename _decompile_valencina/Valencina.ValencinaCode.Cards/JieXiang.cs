using MegaCrit.Sts2.Core.Entities.Cards;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Cards;

public sealed class JieXiang : CounterStyleCard
{
	public JieXiang()
		: base(ValencinaCounterStyle.JieXiang, 5, 6, 0, 0, "jie_xiang.png", (CardRarity)3)
	{
	}
}
