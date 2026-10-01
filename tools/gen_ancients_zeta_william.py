# -*- coding: utf-8 -*-
"""生成泽塔/威廉先古、遗物、立绘复制与本地化。"""
from __future__ import annotations

import json
import shutil
from pathlib import Path

ROOT = Path(r"H:\STS2MOD\AlchemyStars")
CODE = ROOT / "AlchemyStarsCode"
LOC_ZHS = ROOT / "AlchemyStars" / "localization" / "zhs"
LOC_ENG = ROOT / "AlchemyStars" / "localization" / "eng"
IMG_EVENTS = ROOT / "AlchemyStars" / "images" / "events"
IMG_RELICS = ROOT / "AlchemyStars" / "images" / "relics"
IMG_ANCIENTS = ROOT / "AlchemyStars" / "images" / "ancients"

SRC_EVENTS = Path(r"H:\STS2MOD\白夜极光\卡图\事件图&最终补卡")
SRC_ANCIENTS = Path(r"H:\STS2MOD\白夜极光\卡图\先古之民")
SRC_RELICS = Path(r"H:\STS2MOD\白夜极光\卡图\遗物")

CHAR_KEY = "ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER"
# 运行时 Id.Entry 为 EVENT 前缀；ModAnalyzers 误推 ANCIENT，两套都要写。
ZETA_IDS = (
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ZETA",
)
WILLIAM_IDS = (
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_WILLIAM",
)
ZETA_ID = ZETA_IDS[0]
WILLIAM_ID = WILLIAM_IDS[0]


def copy_file(src: Path, dst: Path) -> bool:
    if not src.exists():
        print("MISSING", src)
        return False
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, dst)
    print("OK", dst.relative_to(ROOT))
    return True


def copy_art() -> None:
    for d in (IMG_EVENTS, IMG_RELICS, IMG_ANCIENTS):
        d.mkdir(parents=True, exist_ok=True)

    event_map = {
        "热砂攻防战.png": "AlchemyStarsHotSandDefense.png",
        "第二次生日.png": "AlchemyStarsSecondBirthday.png",
        "红油拉力赛.png": "AlchemyStarsRedieselRally.png",
        "隙间旅人.png": "AlchemyStarsGapTraveler.png",
        "寂静之陵.png": "AlchemyStarsSilentMausoleum.png",
        "少女与遗迹.png": "AlchemyStarsGirlAndRuins.png",
        "归家（先用这个看看效果）.png": "AlchemyStarsHomecoming.png",
        "潮汐祭.png": "AlchemyStarsTideFestival.png",
        "高塔之城.png": "AlchemyStarsTowerCity.png",
        "龙洲盛宴.png": "AlchemyStarsLongzhouFeast.png",
        "事件-下次再见.png": "AlchemyStarsSeeYouNextTime.png",
    }
    for src_name, dst_name in event_map.items():
        copy_file(SRC_EVENTS / src_name, IMG_EVENTS / dst_name)

    ancient_map = {
        "泽塔.png": "AlchemyStarsZeta.png",
        "威廉.png": "AlchemyStarsWilliam.png",
        "启迪者.png": "AlchemyStarsEnlightener.png",
    }
    for src_name, dst_name in ancient_map.items():
        copy_file(SRC_ANCIENTS / src_name, IMG_ANCIENTS / dst_name)
        copy_file(SRC_ANCIENTS / src_name, IMG_EVENTS / dst_name)

    relic_map = {
        "物流从业证.png": "AlchemyStarsLogisticsLicense.png",
        "邪王魔眼.png": "AlchemyStarsDemonEye.png",
        "怪味糖果.png": "AlchemyStarsWeirdCandy.png",
        "泽塔的糖罐.png": "AlchemyStarsZetaCandyJar.png",
        "古怪游戏机.png": "AlchemyStarsOddGameCard.png",
        "甜心麦克风.png": "AlchemyStarsSweetMicrophone.png",
        "罗伊的规矩.png": "AlchemyStarsRoysRules.png",
        "罗伊的奖励.png": "AlchemyStarsRoysReward.png",
        "顺心罗盘.png": "AlchemyStarsSmoothCompass.png",
        "拉力赛奖杯.png": "AlchemyStarsRallyTrophy.png",
        "隐士的秘法.png": "AlchemyStarsHermitsArcana.png",
        "「真理的一滴」.png": "AlchemyStarsDropOfTruth.png",
        "隐士的术式.png": "AlchemyStarsHermitsRite.png",
        "「妄执之锁」.png": "AlchemyStarsLockOfObsession.png",
        "隐士的古籍.png": "AlchemyStarsHermitsTome.png",
        "「万门之钥」.png": "AlchemyStarsKeyOfManyDoors.png",
        "「虚伪的复生」.png": "AlchemyStarsFalseResurrection.png",
        "「命运之眼」.png": "AlchemyStarsEyeOfFate.png",
        "隐士的金库.png": "AlchemyStarsHermitsVault.png",
        "「灵魂的价值」.png": "AlchemyStarsValueOfSoul.png",
        "与世隔绝者的评估.png": "AlchemyStarsIsolatorsAppraisal.png",
        "真理之主遗落赐福.png": "AlchemyStarsTruthLordsBlessing.png",
        "稍微拨动那汪流动.png": "AlchemyStarsNudgeTheFlow.png",
        "故我自在莫比乌斯.png": "AlchemyStarsMobiusOfSelf.png",
        "先天枷锁.png": "AlchemyStarsLumenRelic.png",
        "自由和弦.png": "AlchemyStarsLumenRelicUpgraded.png",
        "光能追踪方案A、B.png": "AlchemyStarsLightTrackingPlanA.png",
        "光能追踪方案C.png": "AlchemyStarsLightTrackingPlanC.png",
        "光能追踪方案D.png": "AlchemyStarsLightTrackingPlanD.png",
        "律法之弦.png": "AlchemyStarsLawString.png",
        "星辰纹章.png": "AlchemyStarsStarCrest.png",
    }
    for src_name, dst_name in relic_map.items():
        copy_file(SRC_RELICS / src_name, IMG_RELICS / dst_name)
    copy_file(SRC_RELICS / "光能追踪方案A、B.png", IMG_RELICS / "AlchemyStarsLightTrackingPlanB.png")


ZETA_RELICS = [
    # pool, class, zhs_title, zhs_desc, zhs_flavor, eng_title, eng_desc, eng_flavor, impl_tag
    ("A", "AlchemyStarsLogisticsLicense", "物流从业证",
     "所有消费打折 [blue]40%[/blue]。但每场战斗从第 [blue]7[/blue] 回合开始，每回合开始时失去 [gold]10[/gold] 金币。",
     "我……正在和伙伴们一起努力做物流工作。",
     "Logistics License",
     "All purchases are [blue]40%[/blue] off. From turn [blue]7[/blue] onward each combat, lose [gold]10[/gold] Gold at the start of your turn.",
     "I... am working hard at logistics with my friends.",
     "shop_discount"),
    ("A", "AlchemyStarsDemonEye", "邪王魔眼",
     "每回合额外抽 [blue]1[/blue] 张牌，并获得{Energy:energyIcons()}。但每回合第 [blue]1[/blue] 张打出的牌必须与上一回合最后打出的卡牌类型不同，否则将 [blue]2[/blue] 张眩晕加入抽牌堆。",
     "快……快跑，封印在我眼中的力量要压抑不住了！",
     "Demon Eye",
     "Draw [blue]1[/blue] extra card and gain {Energy:energyIcons()} each turn. The first card you play each turn must be a different type than the last card played last turn, or shuffle [blue]2[/blue] Dazed into your draw pile.",
     "Run... the power sealed in my eye is breaking free!",
     "demon_eye"),
    ("A", "AlchemyStarsWeirdCandy", "怪味糖果",
     "拾取时，选择 [blue]1[/blue] 张能力卡为其注能。战斗开始时失去{Energy:energyIcons()}。",
     "味道很奇怪……但好像很有效。",
     "Weird Candy",
     "Upon pickup, choose [blue]1[/blue] Power card to Imbue. Lose {Energy:energyIcons()} at the start of combat.",
     "Tastes weird... but it works.",
     "weird_candy"),
    ("B", "AlchemyStarsZetaCandyJar", "泽塔的糖罐",
     "每场战斗的前 [blue]2[/blue] 个回合，自动喝下 [blue]1[/blue] 瓶随机药水。",
     "她真的什么糖果都喜欢，对吧。",
     "Zeta's Candy Jar",
     "For the first [blue]2[/blue] turns of each combat, automatically drink [blue]1[/blue] random potion.",
     "She really does love every kind of candy.",
     "candy_jar"),
    ("B", "AlchemyStarsOddGameCard", "古怪游戏机",
     "每回合随机添加 [blue]1[/blue] 张卡牌到手牌中，费用变为 [blue]0[/blue]，并获得消耗。",
     "按一下试试？",
     "Odd Game Console",
     "Each turn, add a random card to your hand. It costs [blue]0[/blue] and Exhausts.",
     "Try pressing it?",
     "odd_game"),
    ("B", "AlchemyStarsSweetMicrophone", "甜心麦克风",
     "战斗开始时，自动从左到右打出手牌。眩晕你与所有敌人。",
     "谁才是红油扳手第一歌手？！",
     "Sweet Microphone",
     "At the start of combat, automatically play your hand left to right. Stun you and all enemies.",
     "Who is Rediesel Wrench's number-one singer?!",
     "sweet_mic"),
    ("C", "AlchemyStarsRoysRules", "罗伊的规矩",
     "每场战斗开始时，获得 [blue]1[/blue] 点再生。每场战斗获胜后，增加 [blue]1[/blue] 层。",
     "告诉青瞳和约拿，不许在孩子们面前喝酒！",
     "Roy's Rules",
     "At the start of combat, gain [blue]1[/blue] Regenerating. After each victory, increase this by [blue]1[/blue].",
     "Tell Qing Tong and Jonah: no drinking in front of the kids!",
     "roys_rules"),
    ("C", "AlchemyStarsRoysReward", "罗伊的奖励",
     "每获胜 [blue]3[/blue] 次，额外提供 [blue]1[/blue] 次稀有卡奖励。",
     "罗伊把每个孩子都照顾得很好。",
     "Roy's Reward",
     "Every [blue]3[/blue] victories, gain an extra Rare card reward.",
     "Roy takes good care of every child.",
     "roys_reward"),
    ("C", "AlchemyStarsSmoothCompass", "顺心罗盘",
     "为你揭晓未来的 Boss，并显示未来的所有精英。",
     "在麦格芬带来的混乱中惨遭破坏的物品。\n还记得我吗？大概在第二章的剧情里活跃过——它这么说……当然，这是不现实的。",
     "Smooth Compass",
     "Reveal the future Boss and display all upcoming Elites.",
     "Broken in the chaos Maguffins brought.\nRemember me? I was active in Chapter 2—or so it claims...",
     "compass"),
    ("C", "AlchemyStarsRallyTrophy", "拉力赛奖杯",
     "问号房间遇上的敌人会被扣除一半生命。",
     "别小看泽塔，她可是红油拉力赛的明星。",
     "Rally Trophy",
     "Enemies encountered in ? rooms begin at half HP.",
     "Don't underestimate Zeta—she's a Rediesel Rally star.",
     "rally_trophy"),
]

WILLIAM_RELICS = [
    ("A", "AlchemyStarsHermitsArcana", "隐士的秘法",
     "每回合第 [blue]1[/blue] 张攻击与能力牌可免费打出。获得 [blue]1[/blue] 张诅咒「苦恼」。",
     "明明是错误的一隙，此间运行的法则却不难解密。真有趣，如果我这样做将会如何呢？",
     "Hermit's Arcana",
     "The first Attack and first Power you play each turn cost [blue]0[/blue]. Obtain [blue]1[/blue] Curse: Angst.",
     "A wrong gap, yet the laws here are easy to decode. Curious—what if I do this?",
     "hermits_arcana"),
    ("A", "AlchemyStarsDropOfTruth", "「真理的一滴」",
     "本局仅有一次机会，在死后以 [blue]50%[/blue] 生命值复活。获得 [blue]1[/blue] 张「笨拙」。",
     "穿越无穷的光阴，改写悲剧的结局吧。",
     "\"A Drop of Truth\"",
     "Once per run, revive at [blue]50%[/blue] HP when you would die. Obtain [blue]1[/blue] Clumsy.",
     "Cross endless time and rewrite a tragic ending.",
     "drop_of_truth"),
    ("A", "AlchemyStarsHermitsRite", "隐士的术式",
     "从你可获得的先古卡牌中选择 [blue]1[/blue] 张。每回合第 [blue]4[/blue] 张打出的牌将被消耗。",
     "来得正好，这是第几次了呢？总之这次来帮我试试这个吧。",
     "Hermit's Rite",
     "Choose [blue]1[/blue] Ancient card you can obtain. The 4th card played each turn is Exhausted.",
     "Perfect timing—which visit is this? Help me test this one.",
     "hermits_rite"),
    ("A", "AlchemyStarsLockOfObsession", "「妄执之锁」",
     "打光抽牌堆后不再自动洗牌；每回合改为从弃牌堆中自选 [blue]4[/blue] 张牌抽取。抽取时尽量从弃牌堆抽取。一旦触发洗牌，此遗物本场战斗中失效。",
     "别着急，年轻人。你有的是时间，不必陷入执念中。",
     "\"Lock of Obsession\"",
     "After emptying your draw pile, do not reshuffle. Each turn, choose [blue]4[/blue] cards from your discard to draw. Prefer drawing from discard. If a shuffle still occurs, this Relic is disabled.",
     "Don't rush, young one. You have time—don't sink into obsession.",
     "lock_obsession"),
    ("B", "AlchemyStarsHermitsTome", "隐士的古籍",
     "完成 [blue]3[/blue] 场战斗后，获得 [blue]3[/blue] 组从普通到稀有的自选卡牌奖励。它们必然已强化，且费用在本局都将降为 [blue]0[/blue]。",
     "我要留在这里研读新的书籍，你就带着这个上去吧。",
     "Hermit's Tome",
     "After [blue]3[/blue] combats, gain [blue]3[/blue] card rewards (Common to Rare). Chosen cards are upgraded and cost [blue]0[/blue] for the rest of the run.",
     "I'll stay and study. Take this upstairs.",
     "hermits_tome"),
    ("B", "AlchemyStarsKeyOfManyDoors", "「万门之钥」",
     "选择进入某个属于「空裔」的回忆事件。",
     "或许重逢，也是那道河流，那把钥匙所开启的门扉呢？",
     "\"Key of Many Doors\"",
     "Choose to enter one of the Caelestites' memory Events.",
     "Perhaps reunion is also a river—a door this key opens.",
     "key_of_doors"),
    ("B", "AlchemyStarsFalseResurrection", "「虚伪的复生」",
     "去除卡组里所有的「消耗」。每次洗牌时，往抽牌堆内加入 [blue]1[/blue] 张「笨拙」。获得 [blue]1[/blue] 张「愚行」。",
     "年轻的天才想在死亡手下留住自己珍贵的朋友，于是他酿下了大错。那是道仍旧霜寒刺骨的伤痕，且永远无法愈合。",
     "\"False Resurrection\"",
     "Remove Exhaust from all cards in your deck. Each shuffle, add [blue]1[/blue] Clumsy to your draw pile. Obtain [blue]1[/blue] Folly.",
     "A young genius tried to keep a friend from Death, and made a grave mistake.",
     "false_resurrection"),
    ("C", "AlchemyStarsEyeOfFate", "「命运之眼」",
     "每场战斗开始时，随机展示 [blue]5[/blue] 张本职业稀有卡，从中选择 [blue]1[/blue] 张加入手牌。那张卡获得保留，费用 [blue]-1[/blue]。",
     "飞跃无垠的宇宙，让他们见识你的自由吧！\n——某个地方写有类似这样的话。",
     "\"Eye of Fate\"",
     "At combat start, show [blue]5[/blue] random Rare cards of your class; choose [blue]1[/blue] to add to hand. It gains Retain and costs [blue]1[/blue] less.",
     "Leap across the cosmos—show them your freedom!",
     "eye_of_fate"),
    ("C", "AlchemyStarsHermitsVault", "隐士的金库",
     "获得 [blue]1[/blue] 张「贪婪」。下个商店的所有刷新前物品都将免费。",
     "这些东西于我无用，你想拿多少就拿多少。",
     "Hermit's Vault",
     "Obtain [blue]1[/blue] Greed. All items in the next shop (before refreshes) are free.",
     "These are useless to me. Take as many as you want.",
     "hermits_vault"),
    ("C", "AlchemyStarsValueOfSoul", "「灵魂的价值」",
     "选择 [blue]2[/blue] 张牌变化为「灵魂」。",
     "那个时候……在我胸口开了个洞的时候……\n我第一次评估了自己的灵魂。",
     "\"Value of the Soul\"",
     "Choose [blue]2[/blue] cards to transform into Soul.",
     "When a hole opened in my chest... I first measured my soul.",
     "value_of_soul"),
    ("D", "AlchemyStarsIsolatorsAppraisal", "与世隔绝者的评估",
     "每场战斗开始时，随机赋予所有敌人属性，应用属性克制关系。\n被克属性受到 [blue]140%[/blue] 伤害；有利属性受到 [blue]80%[/blue] 伤害；万色伤害造成 [blue]120%[/blue] 伤害。\n雷克水，水克火，火克森，森克雷。敌人不会出现万色属性。",
     "无法被我评估者暂不存在。",
     "Isolator's Appraisal",
     "At combat start, give all enemies a random attribute and apply counters (Thunder>Water>Fire>Forest>Thunder). Countered take [blue]140%[/blue] damage; advantaged take [blue]80%[/blue]; Prismatic deals [blue]120%[/blue]. Enemies never Prismatic.",
     "Nothing exists that I cannot appraise.",
     "isolators"),
    ("D", "AlchemyStarsTruthLordsBlessing", "真理之主遗落赐福",
     "每次生成属性格时，为转色栏空白格添加一个随机属性格，必然不会是当前数量最多的属性。当转色栏已满时，改为随机改变 [blue]1[/blue] 格属性，并赋予强化、深色、棱镜中的一种特质。",
     "这匹马叫霜夜绅士，是我珍贵的朋友……\n嗯，就是这样了。",
     "Truth Lord's Lost Blessing",
     "Whenever you create an Attribute Cell, fill a blank conversion slot with a random attribute that isn't currently most common. If full, recolor [blue]1[/blue] cell and give it Empowered, Dark, or Prismatic.",
     "This horse is Frostnight Gentleman, my dear friend...",
     "truth_blessing"),
    ("D", "AlchemyStarsNudgeTheFlow", "稍微拨动那汪流动",
     "改变属性格被动：\n雷：麻痹每达 [blue]30[/blue] 层击晕敌人并清空层数。\n火：灼烧伤害翻倍，但层数不超过 [blue]14[/blue]。\n水：首次达到 [blue]4[/blue] 格水属性转色格后，一次性恢复最大生命的 [blue]20%[/blue]，此后本场战斗水格不再回血。\n森：仅根据森属性手牌增加格挡，每张提供 [blue]2[/blue] 点格挡。",
     "唱吧，唱吧！直到歌声飘向宇宙，盖过那声声呼唤，令它们的母亲听不见子辈的祈祷。",
     "A Slight Nudge to the Flow",
     "Alters Attribute Cell passives (Thunder stun at 30 Paralysis; Fire Scorch doubled capped at 14; Water heals 20% Max HP once at 4 Water cells; Forest Block only from Forest hand cards, +2 each).",
     "Sing until the cosmos drowns those prayers.",
     "nudge_flow"),
    ("D", "AlchemyStarsMobiusOfSelf", "故我自在莫比乌斯",
     "每回合连续打出特定组合的属性卡牌可触发[gold]莫比乌斯连招[/gold]。连招不能跨回合。一旦停止连击组合，此遗物本场战斗中失效。",
     "自流而下，复过于此百千万亿那由他数岁月。自从是来，我常在此娑婆世界。",
     "Mobius of the Self",
     "Chain specific attribute card combos each turn for bonuses. Breaking the combo disables this Relic.",
     "Flowing onward through countless ages... I remain in this Saha world.",
     "mobius"),
]


def relic_loc_key(class_name: str) -> str:
    # AlchemyStarsLogisticsLicense -> ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LOGISTICS_LICENSE
    snake = []
    for i, ch in enumerate(class_name):
        if ch.isupper() and i > 0:
            snake.append("_")
        snake.append(ch.upper())
    return "ALCHEMY_STARS_RELIC_" + "".join(snake)


def write_relic_base() -> None:
    path = CODE / "Relics" / "Ancients" / "AlchemyStarsAncientRelicBase.cs"
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        """using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 先古之民遗物基类：先古品质，不进入随机遗物池。
/// </summary>
public abstract class AlchemyStarsAncientRelicBase : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool IsAllowed(IRunState runState) => false;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");
}
""",
        encoding="utf-8",
    )


def write_stub_relic(class_name: str, impl_tag: str, extras: str = "") -> None:
    """写出可编译的遗物骨架；具体效果在 gen 后由手工实现文件覆盖。"""
    path = CODE / "Relics" / "Ancients" / f"{class_name}.cs"
    path.parent.mkdir(parents=True, exist_ok=True)
    content = f"""using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 先古遗物：{class_name}（{impl_tag}）。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class {class_name} : AlchemyStarsAncientRelicBase
{{
}}
"""
    path.write_text(content, encoding="utf-8")


def write_all_relics() -> None:
    write_relic_base()
    for row in ZETA_RELICS + WILLIAM_RELICS:
        write_stub_relic(row[1], row[8])


def write_ancients() -> None:
    events_dir = CODE / "Events"
    events_dir.mkdir(parents=True, exist_ok=True)

    zeta = r'''using System.Collections.Generic;
using System.Linq;
using AlchemyStars.Relics.Ancients;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Events;

/// <summary>
/// 先古之民泽塔（第二层 / Hive）。
/// </summary>
[RegisterActAncient(typeof(Hive))]
public sealed class AlchemyStarsZeta : ModAncientEventTemplate
{
    public const string PortraitPath = $"{Entry.ResPath}/images/events/AlchemyStarsZeta.png";

    public override Color ButtonColor => new(0.85f, 0.45f, 0.15f, 0.45f);
    public override Color DialogueColor => new("C46A2B");

    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: PortraitPath);

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        StageProcedural: AncientEventStageProceduralVisualSetBuilder.Create()
            .Background(cues => cues.Single("loop", PortraitPath))
            .Build(),
        MapIconPath: PortraitPath,
        MapIconOutlinePath: PortraitPath,
        RunHistoryIconPath: PortraitPath,
        RunHistoryIconOutlinePath: PortraitPath);

    private IReadOnlyList<EventOption> PoolA =>
    [
        CreateModRelicOption<AlchemyStarsLogisticsLicense>(),
        CreateModRelicOption<AlchemyStarsDemonEye>(),
        CreateModRelicOption<AlchemyStarsWeirdCandy>(),
    ];

    private IReadOnlyList<EventOption> PoolB =>
    [
        CreateModRelicOption<AlchemyStarsZetaCandyJar>(),
        CreateModRelicOption<AlchemyStarsOddGameCard>(),
        CreateModRelicOption<AlchemyStarsSweetMicrophone>(),
    ];

    private IReadOnlyList<EventOption> PoolC =>
    [
        CreateModRelicOption<AlchemyStarsRoysRules>(),
        CreateModRelicOption<AlchemyStarsRoysReward>(),
        CreateModRelicOption<AlchemyStarsSmoothCompass>(),
        CreateModRelicOption<AlchemyStarsRallyTrophy>(),
    ];

    public override IEnumerable<EventOption> AllPossibleOptions =>
        PoolA.Concat(PoolB).Concat(PoolC);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Rng.NextItem(PoolA)!,
        Rng.NextItem(PoolB)!,
        Rng.NextItem(PoolC)!,
    ];
}
'''
    william = r'''using System.Collections.Generic;
using System.Linq;
using AlchemyStars.Characters;
using AlchemyStars.Relics.Ancients;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Events;

/// <summary>
/// 先古之民威廉（第三层 / Glory）。
/// </summary>
[RegisterActAncient(typeof(Glory))]
public sealed class AlchemyStarsWilliam : ModAncientEventTemplate
{
    public const string PortraitPath = $"{Entry.ResPath}/images/events/AlchemyStarsWilliam.png";

    public override Color ButtonColor => new(0.35f, 0.2f, 0.55f, 0.45f);
    public override Color DialogueColor => new("6B4C9A");

    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: PortraitPath);

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        StageProcedural: AncientEventStageProceduralVisualSetBuilder.Create()
            .Background(cues => cues.Single("loop", PortraitPath))
            .Build(),
        MapIconPath: PortraitPath,
        MapIconOutlinePath: PortraitPath,
        RunHistoryIconPath: PortraitPath,
        RunHistoryIconOutlinePath: PortraitPath);

    private IReadOnlyList<EventOption> PoolA =>
    [
        CreateModRelicOption<AlchemyStarsHermitsArcana>(),
        CreateModRelicOption<AlchemyStarsDropOfTruth>(),
        CreateModRelicOption<AlchemyStarsHermitsRite>(),
        CreateModRelicOption<AlchemyStarsLockOfObsession>(),
    ];

    private IReadOnlyList<EventOption> PoolB =>
    [
        CreateModRelicOption<AlchemyStarsHermitsTome>(),
        CreateModRelicOption<AlchemyStarsKeyOfManyDoors>(),
        CreateModRelicOption<AlchemyStarsFalseResurrection>(),
    ];

    private IReadOnlyList<EventOption> PoolC =>
    [
        CreateModRelicOption<AlchemyStarsEyeOfFate>(),
        CreateModRelicOption<AlchemyStarsHermitsVault>(),
        CreateModRelicOption<AlchemyStarsValueOfSoul>(),
    ];

    private IReadOnlyList<EventOption> PoolD =>
    [
        CreateModRelicOption<AlchemyStarsIsolatorsAppraisal>(),
        CreateModRelicOption<AlchemyStarsTruthLordsBlessing>(),
        CreateModRelicOption<AlchemyStarsNudgeTheFlow>(),
        CreateModRelicOption<AlchemyStarsMobiusOfSelf>(),
    ];

    public override IEnumerable<EventOption> AllPossibleOptions =>
        PoolA.Concat(PoolB).Concat(PoolC).Concat(PoolD);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>
        {
            Rng.NextItem(PoolA)!,
            Rng.NextItem(PoolB)!,
            Rng.NextItem(PoolC)!,
        };

        // 空裔专属第四选项。
        if (Owner?.Character is AlchemyStarsCharacter)
            options.Add(Rng.NextItem(PoolD)!);

        return options;
    }
}
'''
    (events_dir / "AlchemyStarsZeta.cs").write_text(zeta, encoding="utf-8")
    (events_dir / "AlchemyStarsWilliam.cs").write_text(william, encoding="utf-8")
    print("Wrote ancient events")


def merge_json(path: Path, updates: dict) -> None:
    data = {}
    if path.exists():
        data = json.loads(path.read_text(encoding="utf-8"))
    data.update(updates)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("Updated", path.relative_to(ROOT))


def write_localization() -> None:
    # relics
    zhs_relics = {}
    eng_relics = {}
    for row in ZETA_RELICS + WILLIAM_RELICS:
        key = relic_loc_key(row[1])
        zhs_relics[f"{key}.title"] = row[2]
        zhs_relics[f"{key}.description"] = row[3]
        zhs_relics[f"{key}.flavor"] = row[4]
        eng_relics[f"{key}.title"] = row[5]
        eng_relics[f"{key}.description"] = row[6]
        eng_relics[f"{key}.flavor"] = row[7]
    merge_json(LOC_ZHS / "relics.json", zhs_relics)
    merge_json(LOC_ENG / "relics.json", eng_relics)

    # ancients dialogue + titles
    zhs = {
        f"{ZETA_ID}.title": "泽塔",
        f"{ZETA_ID}.epithet": "红油勇者",
        f"{WILLIAM_ID}.title": "威廉",
        f"{WILLIAM_ID}.epithet": "隐修大师",

        # 不写 firstVisitEver：引擎在 totalVisits==0 时只播该套且会挡住角色第 0 套。
        # 首次遇见直接走 CHAR/ANY 完整对话。

        # AlchemyStars character dialogues — Zeta
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-0.ancient": "空之末裔？！怎么是你们？\n这里是哪里？\n勇者泽塔终于来到需要拯救的异世界了吗？",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-0.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-1.char": "冷静一点，事情大概是这样……\n总之，这里看上去很安全，你别乱走。",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-1.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-2.ancient": "好难懂……\n总而言之，你会帮本勇者回去对吧？",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-2.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-3.ancient": "正好，本勇者找到了些东西，你看看吧。",

        f"{ZETA_ID}.talk.{CHAR_KEY}.1-0.ancient": "空之末裔！你们不是上去了吗？\n怎么会从下面走上来！",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-0.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-1.char": "事情非常复杂……",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-1.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-2.ancient": "要不带上我吧，待在这里不能飙车，实在……\n太……太……太……无聊啦——",

        f"{ZETA_ID}.talk.{CHAR_KEY}.2-0r.ancient": "快看，本勇者已经交到原住民朋友了！\n咦？它怎么见到你们就跑。",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-0.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-1.char": "是上次偷了我们食物的人啊……",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-1.next": "继续",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-2.char": "在我们回去之前，你还是小心点吧。\n不要乱和奇奇怪怪的人交朋友啊！",

        # AlchemyStars — William
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-0.ancient": "一位华丽的年轻人坐在宫殿中央的地上……\n他的周围摆满了书籍。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-0.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-1.char": "……威廉先生？\n呃，好像哪里看到您也不惊讶。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-1.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-2.ancient": "坐，我等你们很久了。",

        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-0.ancient": "这里有太多值得我着迷的事情了。\n我还不打算离开。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-0.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-1.char": "但我们得回去啊。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-1.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-2.ancient": "我会继续提供帮助，你就好好加油吧。",

        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-0.char": "隐修大师，我感觉您是不是有点乐在其中。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-0.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-1.ancient": "既然追求真理，又怎会抗拒崭新的未知？\n别担心，时机到了我也不会强留。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-1.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-2.ancient": "至少此时此刻……\n让我们一同享受这次漂流的过程吧。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-visit": "2",

        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-0r.ancient": "眼神很不错，看来你找到诀窍了啊。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-0.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-1.char": "嗯，您呢？有收获吗？",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-1.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-2.ancient": "那还用说？年轻人，出发吧。",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-2.next": "继续",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-3.ancient": "我吗？不必担心。\n我和「霜夜绅士」会顺流踏上回归之旅。",
        # 可重复套从 visit3 起，避免 visit 空洞导致事件房 NRE。
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-visit": "3",

        # ANY — Zeta
        f"{ZETA_ID}.talk.ANY.0-0.ancient": "欢迎来到本勇者的宫殿！",
        f"{ZETA_ID}.talk.ANY.0-0.next": "继续",
        f"{ZETA_ID}.talk.ANY.0-1.ancient": "能告诉本勇者关于这里的事情吗？",
        f"{ZETA_ID}.talk.ANY.0-1.next": "继续",
        f"{ZETA_ID}.talk.ANY.0-2.ancient": "我……本勇者可以拿这些东西换！",

        f"{ZETA_ID}.talk.ANY.1-0.ancient": "呜呜……大叔……狂战士……",
        f"{ZETA_ID}.talk.ANY.1-0.next": "继续",
        f"{ZETA_ID}.talk.ANY.1-1.ancient": "还有空之末裔……欸？",
        f"{ZETA_ID}.talk.ANY.1-1.next": "继续",
        f"{ZETA_ID}.talk.ANY.1-2.ancient": "你什么也没看见！\n拿上东西赶紧走吧！",

        f"{ZETA_ID}.talk.ANY.2-0r.ancient": "好无聊啊……",
        f"{ZETA_ID}.talk.ANY.2-0.next": "继续",
        f"{ZETA_ID}.talk.ANY.2-1.ancient": "要不说说你的故事吧！",
        f"{ZETA_ID}.talk.ANY.2-1.next": "继续",
        f"{ZETA_ID}.talk.ANY.2-2.ancient": "本勇者用战利品来交换！",
        f"{ZETA_ID}.talk.ANY.2-visit": "2",

        # ANY — William
        f"{WILLIAM_ID}.talk.ANY.0-0.ancient": "一位华丽的年轻人坐在宫殿中央的地上……\n他的周围摆满了书籍。",
        f"{WILLIAM_ID}.talk.ANY.0-0.next": "继续",
        f"{WILLIAM_ID}.talk.ANY.0-1.ancient": "请坐，要茶吗？",
        f"{WILLIAM_ID}.talk.ANY.0-1.next": "继续",
        f"{WILLIAM_ID}.talk.ANY.0-2.ancient": "对这里的茶有阴影？\n那你想要什么帮助呢？",

        f"{WILLIAM_ID}.talk.ANY.1-0.ancient": "请坐，你想要什么样的招待呢？",
        f"{WILLIAM_ID}.talk.ANY.1-0.next": "继续",
        f"{WILLIAM_ID}.talk.ANY.1-1.ancient": "别误会，我并不喜欢与人闲聊。",
        f"{WILLIAM_ID}.talk.ANY.1-1.next": "继续",
        f"{WILLIAM_ID}.talk.ANY.1-2.ancient": "不过……观察他人也如同读书。\n阅读新书的感觉不错。",

        f"{WILLIAM_ID}.talk.ANY.2-0r.ancient": "关于你曾经说过的故事……\n那多少启发了我。",
        f"{WILLIAM_ID}.talk.ANY.2-0.next": "继续",
        f"{WILLIAM_ID}.talk.ANY.2-1.ancient": "这些就当谢礼吧。",
        f"{WILLIAM_ID}.talk.ANY.2-visit": "2",
    }

    eng = {
        f"{ZETA_ID}.title": "Zeta",
        f"{ZETA_ID}.epithet": "Rediesel Hero",
        f"{WILLIAM_ID}.title": "William",
        f"{WILLIAM_ID}.epithet": "Hermit Master",
        f"{ZETA_ID}.talk.ANY.0-0.ancient": "Welcome to this hero's palace!",
        f"{ZETA_ID}.talk.ANY.0-0.next": "Continue",
        f"{ZETA_ID}.talk.ANY.0-1.ancient": "Can you tell this hero about this place?",
        f"{ZETA_ID}.talk.ANY.0-1.next": "Continue",
        f"{ZETA_ID}.talk.ANY.0-2.ancient": "I... this hero can trade these things!",
        f"{ZETA_ID}.talk.ANY.1-0.ancient": "Ugh... uncle... berserker...",
        f"{ZETA_ID}.talk.ANY.1-0.next": "Continue",
        f"{ZETA_ID}.talk.ANY.1-1.ancient": "And Caelestites... wait?",
        f"{ZETA_ID}.talk.ANY.1-1.next": "Continue",
        f"{ZETA_ID}.talk.ANY.1-2.ancient": "You saw nothing!\nTake it and go!",
        f"{ZETA_ID}.talk.ANY.2-0r.ancient": "So boring...",
        f"{ZETA_ID}.talk.ANY.2-0.next": "Continue",
        f"{ZETA_ID}.talk.ANY.2-1.ancient": "How about telling me your story!",
        f"{ZETA_ID}.talk.ANY.2-1.next": "Continue",
        f"{ZETA_ID}.talk.ANY.2-2.ancient": "This hero trades with spoils!",
        f"{ZETA_ID}.talk.ANY.2-visit": "2",
        f"{WILLIAM_ID}.talk.ANY.0-0.ancient": "A splendid youth sits on the palace floor...\nsurrounded by books.",
        f"{WILLIAM_ID}.talk.ANY.0-0.next": "Continue",
        f"{WILLIAM_ID}.talk.ANY.0-1.ancient": "Sit. Tea?",
        f"{WILLIAM_ID}.talk.ANY.0-1.next": "Continue",
        f"{WILLIAM_ID}.talk.ANY.0-2.ancient": "Wary of the tea here?\nThen what help do you want?",
        f"{WILLIAM_ID}.talk.ANY.1-0.ancient": "Sit. What hospitality do you seek?",
        f"{WILLIAM_ID}.talk.ANY.1-0.next": "Continue",
        f"{WILLIAM_ID}.talk.ANY.1-1.ancient": "Don't misunderstand—I dislike chatter.",
        f"{WILLIAM_ID}.talk.ANY.1-1.next": "Continue",
        f"{WILLIAM_ID}.talk.ANY.1-2.ancient": "But observing others is like reading.\nA new book feels fine.",
        f"{WILLIAM_ID}.talk.ANY.2-0r.ancient": "About the story you once told...\nit inspired me somewhat.",
        f"{WILLIAM_ID}.talk.ANY.2-0.next": "Continue",
        f"{WILLIAM_ID}.talk.ANY.2-1.ancient": "Take these as thanks.",
        f"{WILLIAM_ID}.talk.ANY.2-visit": "2",
        # Mirror CN character lines in ENG for playability
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-0.ancient": "Caelestites?! Why you?!\nWhere is this?\nHas Hero Zeta finally arrived in a world that needs saving?!",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-0.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-1.char": "Calm down. Roughly speaking...\nThis place looks safe. Don't wander.",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-1.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-2.ancient": "So confusing...\nAnyway, you'll help this hero get back, right?",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-2.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.0-3.ancient": "Perfect—this hero found some things. Take a look.",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-0.ancient": "Caelestites! Weren't you going up?\nWhy come from below?!",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-0.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-1.char": "It's complicated...",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-1.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.1-2.ancient": "Take me with you? I can't race here—it's...\nso... so... so boring—",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-0r.ancient": "Look! This hero made a local friend!\nHuh? Why did it run from you?",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-0.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-1.char": "That's the one who stole our food last time...",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-1.next": "Continue",
        f"{ZETA_ID}.talk.{CHAR_KEY}.2-2.char": "Until we return, be careful.\nDon't befriend weird strangers!",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-0.ancient": "A splendid youth sits on the palace floor...\nsurrounded by books.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-0.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-1.char": "...Mr. William?\nSomehow I'm not surprised to see you.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-1.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.0-2.ancient": "Sit. I've waited long for you.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-0.ancient": "Too much here fascinates me.\nI won't leave yet.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-0.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-1.char": "But we have to go back.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-1.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.1-2.ancient": "I'll keep helping. Do your best.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-0.char": "Hermit Master, you seem to be enjoying this.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-0.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-1.ancient": "Seeking truth means welcoming the unknown.\nWhen the time comes, I won't linger.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-1.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-2.ancient": "For now...\nlet us enjoy this drift together.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.2-visit": "2",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-0r.ancient": "Good eyes. You've found the trick.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-0.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-1.char": "And you? Any gains?",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-1.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-2.ancient": "Need you ask? Go, young one.",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-2.next": "Continue",
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-3.ancient": "Me? Don't worry.\nFrostnight Gentleman and I will ride the current home.",
        # 可重复套从 visit3 起，避免 visit 空洞导致事件房 NRE。
        f"{WILLIAM_ID}.talk.{CHAR_KEY}.3-visit": "3",
    }

    # page descriptions for relic options are auto from relic title/desc via FromRelic
    merge_json(LOC_ZHS / "ancients.json", zhs)
    merge_json(LOC_ENG / "ancients.json", eng)


def update_event_portraits() -> None:
    """把事件的 FallbackPortrait 改为各自立绘。"""
    mapping = {
        "AlchemyStarsHotSandDefense.cs": "AlchemyStarsHotSandDefense.png",
        "AlchemyStarsSecondBirthday.cs": "AlchemyStarsSecondBirthday.png",
        "AlchemyStarsRedieselRally.cs": "AlchemyStarsRedieselRally.png",
        "AlchemyStarsGapTraveler.cs": "AlchemyStarsGapTraveler.png",
        "AlchemyStarsSilentMausoleum.cs": "AlchemyStarsSilentMausoleum.png",
        "AlchemyStarsGirlAndRuins.cs": "AlchemyStarsGirlAndRuins.png",
        "AlchemyStarsHomecoming.cs": "AlchemyStarsHomecoming.png",
        "AlchemyStarsTideFestival.cs": "AlchemyStarsTideFestival.png",
        "AlchemyStarsTowerCity.cs": "AlchemyStarsTowerCity.png",
        "AlchemyStarsLongzhouFeast.cs": "AlchemyStarsLongzhouFeast.png",
        "AlchemyStarsSeeYouNextTime.cs": "AlchemyStarsSeeYouNextTime.png",
    }
    events_dir = CODE / "Events"
    for fname, png in mapping.items():
        path = events_dir / fname
        if not path.exists():
            print("skip missing", fname)
            continue
        text = path.read_text(encoding="utf-8")
        old = "InitialPortraitPath: AlchemyStarsEventHelpers.FallbackPortraitPath"
        # C# 插值字符串：$"{Entry.ResPath}/images/events/..."
        new = 'InitialPortraitPath: $"' + "{Entry.ResPath}/images/events/" + png + '"'
        if old in text:
            text = text.replace(old, new)
            path.write_text(text, encoding="utf-8")
            print("portrait", fname)
        else:
            print("no fallback pattern", fname)


def patch_william_events_allowed() -> None:
    files = [
        "AlchemyStarsHotSandDefense.cs",
        "AlchemyStarsSecondBirthday.cs",
        "AlchemyStarsRedieselRally.cs",
        "AlchemyStarsGapTraveler.cs",
    ]
    events_dir = CODE / "Events"
    for fname in files:
        path = events_dir / fname
        text = path.read_text(encoding="utf-8")
        if "AlchemyStarsKeyOfManyDoors" in text and "HasRelic" in text:
            print("already patched", fname)
            continue
        if "using AlchemyStars.Relics.Ancients;" not in text:
            text = text.replace(
                "using AlchemyStars.Relics.Events;",
                "using AlchemyStars.Relics.Ancients;\nusing AlchemyStars.Relics.Events;",
            )
            if "using AlchemyStars.Relics.Ancients;" not in text:
                # insert after namespace usings block start
                lines = text.splitlines(True)
                insert_at = 0
                for i, line in enumerate(lines):
                    if line.startswith("using "):
                        insert_at = i + 1
                lines.insert(insert_at, "using AlchemyStars.Relics.Ancients;\n")
                text = "".join(lines)

        old = "public override bool IsAllowed(IRunState runState) => false;"
        new = (
            "public override bool IsAllowed(IRunState runState) =>\n"
            "        runState.Players.Any(p => p.HasRelic<AlchemyStarsKeyOfManyDoors>());"
        )
        if old not in text:
            print("IsAllowed pattern missing", fname)
            continue
        if "using System.Linq;" not in text:
            text = "using System.Linq;\n" + text
        text = text.replace(old, new)
        path.write_text(text, encoding="utf-8")
        print("allowed", fname)


def main() -> None:
    copy_art()
    write_all_relics()
    write_ancients()
    write_localization()
    update_event_portraits()
    patch_william_events_allowed()
    print("DONE scaffold")


if __name__ == "__main__":
    main()
