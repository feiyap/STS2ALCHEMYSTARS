using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Precognition;

public static class ValencinaCounterLevelHelper
{
	public const int MinLevel = 0;

	private static readonly ValencinaCounterDefinition BaseCounter = new ValencinaCounterDefinition(ValencinaCounterStyle.BaseCounter, "BaseCounter", 0, 3m, 1);

	public static ValencinaCounterDefinition GetDefinition(Player? player)
	{
		return ((player != null) ? player.GetRelic<BernoullitMemory>() : null)?.CurrentCounterDefinition ?? BaseCounter;
	}

	public static ValencinaCounterDefinition GetDefinition(int level)
	{
		return BaseCounter;
	}

	public static CardModel GetPreviewCard(Player? player)
	{
		return (CardModel)(GetDefinition(player).Style switch
		{
			ValencinaCounterStyle.JieTu => ModelDb.Card<JieTu>(), 
			ValencinaCounterStyle.JieLu => ModelDb.Card<JieLu>(), 
			ValencinaCounterStyle.JieXiang => ModelDb.Card<JieXiang>(), 
			ValencinaCounterStyle.PalermoSwordplaySecret => ModelDb.Card<PalermoSwordplaySecret>(), 
			_ => GetPreviewCardForLevel(0), 
		});
	}

	public static CardModel GetPreviewCardForLevel(int level)
	{
		return (CardModel)(object)ModelDb.Card<CounterPreviewLv0>();
	}

	public static CardModel GetPreviewCardForStyle(ValencinaCounterStyle style)
	{
		return (CardModel)(style switch
		{
			ValencinaCounterStyle.JieTu => ModelDb.Card<JieTu>(), 
			ValencinaCounterStyle.JieLu => ModelDb.Card<JieLu>(), 
			ValencinaCounterStyle.JieXiang => ModelDb.Card<JieXiang>(), 
			ValencinaCounterStyle.PalermoSwordplaySecret => ModelDb.Card<PalermoSwordplaySecret>(), 
			_ => GetPreviewCardForLevel(0), 
		});
	}

	public static IReadOnlyList<ValencinaCounterDefinition> GetStackedDefinitions(Player? player)
	{
		BernoullitMemory bernoullitMemory = ((player != null) ? player.GetRelic<BernoullitMemory>() : null);
		if (bernoullitMemory == null)
		{
			return new ValencinaCounterDefinition[1] { GetDefinition(player) };
		}
		return bernoullitMemory.GetActiveCounterDefinitions();
	}

	public static ValencinaCounterDefinition CreateDefinition(ICounterStyleCard card, bool upgraded)
	{
		return card.Style switch
		{
			ValencinaCounterStyle.JieTu => new ValencinaCounterDefinition(card.Style, "JieTu", 0, upgraded ? 5m : 4m, 1, upgraded), 
			ValencinaCounterStyle.JieLu => new ValencinaCounterDefinition(card.Style, "JieLu", 0, upgraded ? 4m : 3m, 1, upgraded), 
			ValencinaCounterStyle.JieXiang => new ValencinaCounterDefinition(card.Style, "JieXiang", 0, upgraded ? 6m : 5m, 1, upgraded), 
			ValencinaCounterStyle.PalermoSwordplaySecret => new ValencinaCounterDefinition(card.Style, "PalermoSwordplaySecret", upgraded ? 3 : 2, 3m, 1, upgraded), 
			_ => BaseCounter, 
		};
	}
}
