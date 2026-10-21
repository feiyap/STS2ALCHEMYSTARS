using AlchemyStars.Cards;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 开始拖动空裔技能牌时播放 skill_1，再从开头循环 skill_2_loop。
/// 攻击牌和能力牌不播放。拖动过程中不因悬停结束而打断循环。
/// </summary>
public sealed class SkillSequenceDragPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_skill_sequence_drag";

    public static string Description => "Play skill intro and loop when a skill card drag starts";

    public static bool IsCritical => false;

    private static readonly Dictionary<Creature, CardModel> DraggedCards = new();

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NHandCardHolder), nameof(NHandCardHolder.BeginDrag)),
    ];

    public static void Postfix(NHandCardHolder __instance)
    {
        var card = __instance.CardModel;
        var creature = card?.Owner?.Creature;
        if (card == null || card.Type != CardType.Skill || creature == null || creature.IsDead)
            return;
        if (card.Owner?.Character is not AlchemyStarsCharacter)
            return;

        var node = creature.GetCreatureNode();
        if (node == null)
            return;

        DraggedCards[creature] = card;
        // 只切动画，不走施法等待。skill_1 播完后由 Spine 队列接上 skill_2_loop。
        node.SetAnimationTrigger(AlchemyStarsCardHelpers.ResolveSkillAnim(card, forPlay: false));
        QueueSkillLoop(node, AlchemyStarsCardHelpers.ResolveSkillLoopAnim());
    }

    /// <summary>
    /// 拖动取消时回到待机。牌已经打出时保留后续的属性打出动画。
    /// </summary>
    internal static void OnCardPlayCleanup(NCardPlay play, bool isFinished)
    {
        var card = play.Holder?.CardModel;
        var creature = card?.Owner?.Creature;
        if (card == null || creature == null)
            return;
        if (!DraggedCards.TryGetValue(creature, out var dragged) || !ReferenceEquals(dragged, card))
            return;

        DraggedCards.Remove(creature);
        if (isFinished || creature.IsDead)
            return;

        var node = creature.GetCreatureNode();
        if (node == null || !node.HasSpineAnimation)
            return;

        var name = node.SpineAnimation.GetCurrentAnimationName();
        if (name is not ("skill_1" or "skill_2_loop"))
            return;

        node.SetAnimationTrigger("Idle");
    }

    private static void QueueSkillLoop(NCreature node, string loopAnim)
    {
        var sprite = node.Visuals?.SpineBody;
        if (sprite == null || !sprite.HasAnimation(loopAnim))
            return;

        using var entry = sprite.GetAnimationState().AddAnimationTracked(loopAnim, 0f, loop: true);
        // 状态机给循环动画的随机偏移不能用在技能衔接上，这里从第 0 帧开始。
        entry.SetTrackTime(0f);
        entry.SetTimeScale(1f);
    }
}

/// <summary>
/// 技能牌拖动取消后结束循环。成功打出时不切回待机，交给打出动画接管。
/// </summary>
public sealed class SkillSequenceDragCancelPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_skill_sequence_drag_cancel";

    public static string Description => "Stop the skill drag loop when the drag is cancelled";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCardPlay), nameof(NCardPlay.Cleanup)),
    ];

    public static void Postfix(NCardPlay __instance, bool isFinished)
    {
        SkillSequenceDragPatch.OnCardPlayCleanup(__instance, isFinished);
    }
}
