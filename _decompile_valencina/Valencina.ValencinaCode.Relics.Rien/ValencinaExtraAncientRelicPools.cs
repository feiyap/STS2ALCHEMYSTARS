using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Valencina.ValencinaCode.Relics.Rien;

internal static class ValencinaExtraAncientRelicPools
{
	internal const int RandomOptionCount = 3;

	internal static readonly IReadOnlyList<ExtraAncientPoolEntry> Act1ExtraAncientRelicPool = new _003C_003Ez__ReadOnlyArray<ExtraAncientPoolEntry>(new ExtraAncientPoolEntry[8]
	{
		new ExtraAncientPoolEntry(typeof(ThumbBadge), (Func<RelicModel>)ModelDb.Relic<ThumbBadge>),
		new ExtraAncientPoolEntry(typeof(AdvisorsDecision), (Func<RelicModel>)ModelDb.Relic<AdvisorsDecision>),
		new ExtraAncientPoolEntry(typeof(BernoulliTraining), (Func<RelicModel>)ModelDb.Relic<BernoulliTraining>),
		new ExtraAncientPoolEntry(typeof(WalkieTalkie), (Func<RelicModel>)ModelDb.Relic<WalkieTalkie>),
		new ExtraAncientPoolEntry(typeof(Pendant), (Func<RelicModel>)ModelDb.Relic<Pendant>),
		new ExtraAncientPoolEntry(typeof(RecordOfThatDay), (Func<RelicModel>)ModelDb.Relic<RecordOfThatDay>),
		new ExtraAncientPoolEntry(typeof(TornBandolier), (Func<RelicModel>)ModelDb.Relic<TornBandolier>),
		new ExtraAncientPoolEntry(typeof(ScorchingHammer), (Func<RelicModel>)ModelDb.Relic<ScorchingHammer>)
	});

	internal static readonly IReadOnlyList<ExtraAncientPoolEntry> Act2ExtraAncientRelicPool = new _003C_003Ez__ReadOnlyArray<ExtraAncientPoolEntry>(new ExtraAncientPoolEntry[8]
	{
		new ExtraAncientPoolEntry(typeof(Outlaw), (Func<RelicModel>)ModelDb.Relic<Outlaw>),
		new ExtraAncientPoolEntry(typeof(Rainstorm), (Func<RelicModel>)ModelDb.Relic<Rainstorm>),
		new ExtraAncientPoolEntry(typeof(FlameScale), (Func<RelicModel>)ModelDb.Relic<FlameScale>),
		new ExtraAncientPoolEntry(typeof(FirelightFlower), (Func<RelicModel>)ModelDb.Relic<FirelightFlower>),
		new ExtraAncientPoolEntry(typeof(UnhatchedSpark), (Func<RelicModel>)ModelDb.Relic<UnhatchedSpark>),
		new ExtraAncientPoolEntry(typeof(FenghuangDoll), (Func<RelicModel>)ModelDb.Relic<FenghuangDoll>),
		new ExtraAncientPoolEntry(typeof(EightDirectionsBell), (Func<RelicModel>)ModelDb.Relic<EightDirectionsBell>),
		new ExtraAncientPoolEntry(typeof(TremorCoupling), (Func<RelicModel>)ModelDb.Relic<TremorCoupling>)
	});

	internal static readonly IReadOnlyList<ExtraAncientPoolEntry> Act3ExtraAncientRelicPool = new _003C_003Ez__ReadOnlyArray<ExtraAncientPoolEntry>(new ExtraAncientPoolEntry[8]
	{
		new ExtraAncientPoolEntry(typeof(SomeonesBlueBlade), (Func<RelicModel>)ModelDb.Relic<SomeonesBlueBlade>),
		new ExtraAncientPoolEntry(typeof(SomeonesComic), (Func<RelicModel>)ModelDb.Relic<SomeonesComic>),
		new ExtraAncientPoolEntry(typeof(SomeonesWork), (Func<RelicModel>)ModelDb.Relic<SomeonesWork>),
		new ExtraAncientPoolEntry(typeof(MagicBeeper), (Func<RelicModel>)ModelDb.Relic<MagicBeeper>),
		new ExtraAncientPoolEntry(typeof(ScatteredOracle), (Func<RelicModel>)ModelDb.Relic<ScatteredOracle>),
		new ExtraAncientPoolEntry(typeof(RevengeLedgerAppendix), (Func<RelicModel>)ModelDb.Relic<RevengeLedgerAppendix>),
		new ExtraAncientPoolEntry(typeof(MasterpieceArtwork), (Func<RelicModel>)ModelDb.Relic<MasterpieceArtwork>),
		new ExtraAncientPoolEntry(typeof(Reverberation), (Func<RelicModel>)ModelDb.Relic<Reverberation>)
	});

	internal static readonly IReadOnlyList<Type> BossActivationRelicOptions = new _003C_003Ez__ReadOnlyArray<Type>(new Type[3]
	{
		typeof(Maggot),
		typeof(Moth),
		typeof(Fly)
	});

	internal static IReadOnlyList<ExtraAncientPoolEntry> GetPool(int actIndex)
	{
		return actIndex switch
		{
			0 => Act1ExtraAncientRelicPool, 
			1 => Act2ExtraAncientRelicPool, 
			_ => Act3ExtraAncientRelicPool, 
		};
	}

	internal static List<ExtraAncientPoolEntry> DrawOptions(Player owner, int actIndex, int count = 3)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		List<ExtraAncientPoolEntry> list = (from entry in GetPool(actIndex)
			where owner.Relics.All((RelicModel relic) => ((object)relic).GetType() != entry.RelicType)
			select entry).OrderBy<ExtraAncientPoolEntry, string>((ExtraAncientPoolEntry entry) => entry.RelicType.FullName, StringComparer.Ordinal).ToList();
		Rng val = new Rng((uint)((int)owner.RunState.Rng.Seed + (int)owner.NetId + (actIndex + 1) * 97 + StringHelper.GetDeterministicHashCode("ValencinaExtraAncientPool")), 0);
		ListExtensions.UnstableShuffle<ExtraAncientPoolEntry>(list, val);
		return list.Take(count).ToList();
	}
}
