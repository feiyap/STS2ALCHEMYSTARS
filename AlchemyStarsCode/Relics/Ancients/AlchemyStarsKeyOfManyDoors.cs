using AlchemyStars.Cards;
using AlchemyStars.Characters;
using AlchemyStars.Events;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 多门钥匙：拾起时在 4 个回忆事件中选一个；先古点「继续」后进入该事件。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsKeyOfManyDoors : AlchemyStarsAncientRelicBase
{
    private ModelId? _pendingEventEntry;

    public override bool HasUponPickupEffect => true;

    /// <summary>
    /// 待进入的回忆事件 Id；由先古 Proceed Patch 消费。
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

        var displayCards = new CardModel[]
        {
            Owner.RunState.CreateCard<AlchemyStarsMemoryDoorSecondBirthday>(Owner),
            Owner.RunState.CreateCard<AlchemyStarsMemoryDoorHotSandDefense>(Owner),
            Owner.RunState.CreateCard<AlchemyStarsMemoryDoorRedieselRally>(Owner),
            Owner.RunState.CreateCard<AlchemyStarsMemoryDoorGapTraveler>(Owner),
        };

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
        var picked = (await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            displayCards,
            Owner,
            prefs)).FirstOrDefault() ?? displayCards[0];

        // 用 ModelDb.Event<T>().Id，避免 GetId(Type) 与存档解析不一致。
        PendingEventEntry = ResolveEventId(picked);
        Entry.Logger.Info($"[KeyOfManyDoors] 已选择回忆事件 Pending={PendingEventEntry?.Entry}");
        Flash();
    }

    private static ModelId ResolveEventId(CardModel picked) => picked switch
    {
        AlchemyStarsMemoryDoorSecondBirthday => ModelDb.Event<AlchemyStarsSecondBirthday>().Id,
        AlchemyStarsMemoryDoorHotSandDefense => ModelDb.Event<AlchemyStarsHotSandDefense>().Id,
        AlchemyStarsMemoryDoorRedieselRally => ModelDb.Event<AlchemyStarsRedieselRally>().Id,
        AlchemyStarsMemoryDoorGapTraveler => ModelDb.Event<AlchemyStarsGapTraveler>().Id,
        _ => ModelDb.Event<AlchemyStarsSecondBirthday>().Id,
    };
}
