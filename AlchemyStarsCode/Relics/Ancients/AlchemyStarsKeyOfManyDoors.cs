using AlchemyStars.Characters;
using AlchemyStars.Events;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 多门钥匙：拾起时在 4 个回忆事件中选一个，离开事件房后进入该事件。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsKeyOfManyDoors : AlchemyStarsAncientRelicBase
{
    private ModelId? _pendingEventEntry;

    public override bool HasUponPickupEffect => true;

    /// <summary>
    /// 待进入的回忆事件 Id；由离开事件房的 Patch 消费。
    /// </summary>
    [SavedProperty]
    public ModelId? PendingEventEntry
    {
        get => _pendingEventEntry;
        set
        {
            AssertMutable();
            _pendingEventEntry = value;
        }
    }

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        var eventChoices = new EventModel[]
        {
            ModelDb.Event<AlchemyStarsSecondBirthday>(),
            ModelDb.Event<AlchemyStarsHotSandDefense>(),
            ModelDb.Event<AlchemyStarsRedieselRally>(),
            ModelDb.Event<AlchemyStarsGapTraveler>(),
        };

        // 用 4 张无色先古卡作为选项展示载体，按索引映射到事件。
        var displayCards = new CardModel[]
        {
            Owner.RunState.CreateCard<Apotheosis>(Owner),
            Owner.RunState.CreateCard<Apparition>(Owner),
            Owner.RunState.CreateCard<BiasedCognition>(Owner),
            Owner.RunState.CreateCard<Corruption>(Owner),
        };

        var picked = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(),
            displayCards,
            Owner);

        var index = picked == null ? 0 : Array.IndexOf(displayCards, picked);
        if (index < 0)
            index = 0;

        PendingEventEntry = eventChoices[index].Id;
        Flash();
    }
}
