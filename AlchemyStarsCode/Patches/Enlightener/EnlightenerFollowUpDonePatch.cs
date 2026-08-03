using System.Reflection;
using AlchemyStars.Characters;
using AlchemyStars.Events;
using AlchemyStars.Relics.Enlightener;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 空裔在涅奥选完遗物后，拦截 Done 并注入启迪者续页。
/// </summary>
public sealed class EnlightenerFollowUpDonePatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_enlightener_follow_up_done";

    public static string Description => "Show Enlightener follow-up page after Neow for AlchemyStars";

    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(AncientEventModel), "Done"),
    ];

    private static readonly MethodInfo? SetEventStateMethod =
        AccessTools.Method(typeof(EventModel), "SetEventState");

    private static readonly MethodInfo? SetEventFinishedMethod =
        AccessTools.Method(typeof(EventModel), "SetEventFinished");

    private static readonly MethodInfo? UpdateRunHistoryMethod =
        AccessTools.Method(typeof(AncientEventModel), "UpdateRunHistory");

    public static bool Prefix(AncientEventModel __instance)
    {
        try
        {
            if (!ShouldStartFollowUp(__instance))
                return true;

            if (!EnlightenerFollowUpState.TryMarkTriggered(__instance.Owner))
                return true;

            StartFollowUpPage(__instance);
            return false;
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[Enlightener] 注入续页失败，回退原版 Done。{ex}");
            return true;
        }
    }

    private static bool ShouldStartFollowUp(AncientEventModel currentEvent)
    {
        if (currentEvent.Owner == null)
            return false;

        if (currentEvent.Owner.Character is not AlchemyStarsCharacter)
            return false;

        if (currentEvent is not Neow)
            return false;

        if (currentEvent.IsFinished)
            return false;

        if (EnlightenerFollowUpVisualState.TryGet(currentEvent, out _))
            return false;

        return true;
    }

    private static void StartFollowUpPage(AncientEventModel hostEvent)
    {
        if (SetEventStateMethod == null || UpdateRunHistoryMethod == null)
            throw new MissingMethodException("STS2 Ancient event state methods changed.");

        const string eventEntry = AlchemyStarsEnlightener.EventEntry;
        EnlightenerFollowUpVisualState.Set(hostEvent, eventEntry);
        UpdateRunHistoryMethod.Invoke(hostEvent, []);

        var options = CreateOptions(hostEvent, eventEntry);
        var description = new LocString("ancients", $"{eventEntry}.pages.INITIAL.description");
        SetEventStateMethod.Invoke(hostEvent, [description, options]);
        EnlightenerFollowUpVisuals.Apply(eventEntry);

        Entry.Logger.Info($"[Enlightener] 在 {hostEvent.Id.Entry} 后展示启迪者续页。");
    }

    private static List<EventOption> CreateOptions(AncientEventModel hostEvent, string eventEntry) =>
    [
        RelicOption<AlchemyStarsLightTrackingPlanA>(hostEvent, eventEntry),
        RelicOption<AlchemyStarsLightTrackingPlanB>(hostEvent, eventEntry),
        RelicOption<AlchemyStarsLightTrackingPlanC>(hostEvent, eventEntry),
        RelicOption<AlchemyStarsLightTrackingPlanD>(hostEvent, eventEntry),
    ];

    private static EventOption RelicOption<T>(AncientEventModel hostEvent, string eventEntry)
        where T : RelicModel
    {
        var owner = hostEvent.Owner;
        var relic = ModelDb.Relic<T>().ToMutable();
        relic.Owner = owner;
        var textKey = $"{eventEntry}.pages.INITIAL.options.{relic.Id.Entry}";
        return EventOption.FromRelic(relic, hostEvent, async () =>
        {
            await RelicCmd.Obtain(relic, owner);
            FinishHostEvent(hostEvent, eventEntry);
        }, textKey);
    }

    private static void FinishHostEvent(AncientEventModel hostEvent, string eventEntry)
    {
        if (SetEventFinishedMethod == null)
            throw new MissingMethodException("STS2 event finish method changed.");

        var description = new LocString("ancients", $"{eventEntry}.pages.DONE.description");
        SetEventFinishedMethod.Invoke(hostEvent, [description]);
    }
}
