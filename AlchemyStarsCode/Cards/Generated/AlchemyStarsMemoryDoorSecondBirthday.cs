using AlchemyStars.Events;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>万门之钥选项：第二次生日（森）。</summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsMemoryDoorSecondBirthday : AlchemyStarsMemoryDoorChoiceCardBase
{
    public override Type TargetEventType => typeof(AlchemyStarsSecondBirthday);

    protected override IReadOnlyList<string> AttributeKeywordIds => [AlchemyStarsKeywordIds.Forest];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsMemoryDoorSecondBirthday.png");
}
