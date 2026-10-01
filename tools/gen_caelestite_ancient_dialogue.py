# -*- coding: utf-8 -*-
"""按《对话》sheet 写入空裔与各先古之民的对话（zhs/eng）。"""
from __future__ import annotations

import json
import zipfile
import re
from pathlib import Path
from xml.etree import ElementTree as ET

XLSX = Path(r"h:\STS2MOD\白夜极光\白夜极光卡组.xlsx")
ZHS = Path(r"H:\STS2MOD\AlchemyStars\AlchemyStars\localization\zhs\ancients.json")
ENG = Path(r"H:\STS2MOD\AlchemyStars\AlchemyStars\localization\eng\ancients.json")

CHAR = "ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER"
NEXT = "继续"
NEXT_EN = "Continue"

# 中文名 → 游戏 ancient Id.Entry
ANCIENT_IDS = {
    "涅奥": "NEOW",
    # RitsuLib AncientEventModel 的 Id.Entry 使用 EVENT 前缀
    "启迪者": "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ENLIGHTENER",
    "大眼": "OROBAS",
    "老奶奶": "TEZCATARA",
    "佩尔": "PAEL",
    "达弗": "DARV",
    "泽塔": "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA",
    "威廉": "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM",
    "诺奴佩普": "NONUPEIPE",
    "瓦库": "VAKUU",
    "坦克斯": "TANX",
    "建筑师": "THE_ARCHITECT",
}

# 启迪者页面仍用短 id；ANCIENT 前缀供 ModAnalyzers（运行时实际为 EVENT）
ENLIGHTENER_ALIASES = [
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ENLIGHTENER",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ENLIGHTENER",
    "ALCHEMY_STARS_ENLIGHTENER",
]

# 运行时 EVENT + 分析器 ANCIENT 双写（与 Feiyap 先古处理相同）
RUNTIME_ANALYZER_MIRRORS = {
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA": "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ZETA",
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM": "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_WILLIAM",
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ENLIGHTENER": "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ENLIGHTENER",
}


def load_sheet_rows() -> list[dict]:
    ns = {"m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
    with zipfile.ZipFile(XLSX) as z:
        ss = ET.fromstring(z.read("xl/sharedStrings.xml"))
        strings = []
        for si in ss.findall("m:si", ns):
            texts = [t.text or "" for t in si.findall(".//m:t", ns)]
            strings.append("".join(texts))
        wb = ET.fromstring(z.read("xl/workbook.xml"))
        rels = ET.fromstring(z.read("xl/_rels/workbook.xml.rels"))
        rid_to_target = {r.attrib["Id"]: r.attrib["Target"] for r in rels}
        target = None
        for s in wb.findall("m:sheets/m:sheet", ns):
            if s.attrib.get("name") == "对话":
                target = rid_to_target[
                    s.attrib["{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id"]
                ]
                break
        sheet = ET.fromstring(z.read("xl/" + target))

        def cell_val(c):
            t = c.attrib.get("t")
            v = c.find("m:v", ns)
            if v is None:
                return ""
            if t == "s":
                return strings[int(v.text)]
            return v.text or ""

        def col(ref):
            return re.match(r"([A-Z]+)", ref).group(1)

        rows = []
        for row in sheet.findall("m:sheetData/m:row", ns):
            rnum = int(row.attrib["r"])
            cells = {col(c.attrib["r"]): cell_val(c) for c in row.findall("m:c", ns)}
            rows.append({"r": rnum, "c": cells})
        return rows


def pairs_from_row(cells: dict) -> list[tuple[str, str]]:
    """按 F/G H/I J/K L/M 抽取 (speaker, text)。"""
    out = []
    for sp_col, tx_col in (("F", "G"), ("H", "I"), ("J", "K"), ("L", "M")):
        sp = (cells.get(sp_col) or "").strip()
        tx = (cells.get(tx_col) or "").strip()
        if sp or tx:
            out.append((sp, tx))
    return out


def role_suffix(speaker: str) -> str:
    if speaker in ("空裔",):
        return "char"
    return "ancient"


def format_text(speaker: str, text: str) -> str:
    text = text.replace("\r\n", "\n").strip()
    if speaker == "旁白":
        # 旁白台词用「」括起，并沿用原版旁白斜体字号样式。
        return f"[i][font_size=22]「{text}」[/font_size][/i]"
    return text


# 英文对照（空裔专属对话）
ENG_MAP: dict[str, str] = {
    # Neow
    "外来的……客人……\n你的回忆……散落满地……": "A foreign... guest...\nYour memories... strewn about...",
    "您好，请问这是哪里？\n我的航线好像出问题了。": "Hello—where is this?\nMy course seems to have gone wrong.",
    "答案……就在塔顶……\n用你的回忆……武装自己……": "The answer... lies at the summit...\nArm yourself... with your memories...",
    "你们……醒了……\n我……在塔底……找到了你们……": "You... awoke...\nI... found you... at the base...",
    "谢谢。\n呃……啊！要重新再来了？": "Thank you.\nUh... ah! We have to start over?",
    "必须去塔顶……杀了他……\n你才能……回去……\n战斗……不可避免……": "You must reach the summit... kill him...\nbefore you can... return...\nCombat... is inevitable...",
    "你被……困住了……\n我会……帮助你……": "You are... trapped...\nI will... help you...",
    "没关系，我们好像找到窍门了。\n谢谢您一直帮忙。": "It's fine—we've found the trick.\nThank you for always helping.",
    "祝福你……外来的色彩……\n去结束……他的掌控……": "Bless you... foreign colors...\nGo end... his dominion...",
    # Enlightener
    "父亲，本机已经做好探索准备了。": "Father, this unit is ready to explore.",
    "这个地方好奇怪。\n你还是留下来支援我们吧。": "This place is strange.\nStay and support us.",
    "了解，转化为支援模式。": "Understood. Switching to support mode.",
    "父亲，之前真是千钧一发。": "Father, that was a close call.",
    "还好有你的信标，勉强撤回来了。": "Thankfully your beacon got us back.",
    "这次要尝试不同的路线吗？": "Shall we try a different route this time?",
    "父亲，有必要上作弊模式了。": "Father, cheat mode may be necessary.",
    "哈……？": "Huh...?",
    "锁定极光的力量战斗！\n啊，好像在这里不行。": "Lock onto Aurora power and fight!\nAh—doesn't work here.",
    # Orobas
    "好多光！闪亮亮！真好玩！！！": "So much light! Sparkly! Fun!!!",
    "是说这些虚构光能吗？\n这都多亏了那位储君的帮忙。": "You mean these fictional lights?\nAll thanks to that Regent's help.",
    "真好看！挑个礼物吧！我报答你！！！": "So pretty! Pick a gift! I repay you!!!",
    "大飞船！又来了！": "Big ship! You're back!",
    "你好，我们又见面了。": "Hello—we meet again.",
    "你还要去找收藏家吗？再努力！再努力！": "Still hunting the Collector? Try harder! Try harder!",
    "好朋友！你来啦！看看我又找到了什么？": "Good friend! You're here! Look what I found!",
    "谢谢你一直帮忙。\n不过我们接下来可能随时会离开。": "Thanks for always helping.\nWe may leave any time now.",
    "还真有点舍不得大家。\n你也要好好保重哦。": "I'll miss everyone a little.\nTake care of yourself too.",
    # Tezcatara
    "噢，多么可爱的一群人，你们好呀。": "Oh, what a lovely bunch—hello there.",
    "您好，我们能在这里休息一下吗？": "Hello—may we rest here a while?",
    "当然可以，凑过来让我好好看看你们。": "Of course. Come closer; let me look at you.",
    "欢迎回来，小可爱们。\n喝点热汤暖暖身子吧。": "Welcome back, little dears.\nHave some hot soup to warm up.",
    "薇丝！不……\n不要干扰人家比较好……": "Vice! No...\nBetter not interfere...",
    "空裔拉住了想去帮忙下厨的薇丝。": "The Caelestite stops Vice from helping in the kitchen.",
    "你们来啦，小可爱们。": "You're here, little dears.",
    "谢谢您一直招待我们。\n我们可能马上就要走了。": "Thank you for hosting us.\nWe may leave soon.",
    "真为你们高兴。\n出发前要烧掉什么负担吗？": "I'm so happy for you.\nAnything to burn before you go?",
    # Pael
    "你们也要去和收藏家战斗吗？": "Are you going to fight the Collector too?",
    "如果可以不打就更好了。\n我只是想回去。": "I'd rather not fight, if possible.\nI just want to go home.",
    "放下那个念头吧……\n没有战斗，就无法登顶……": "Let that go...\nWithout combat, you cannot reach the top...",
    "带上我的一部分，\n它是最好的。": "Take a part of me—\nit's the best.",
    "呃……": "Uh...",
    "（薇丝的眼神好奇怪。\n这个怎么看都不是食物啦。）": "(Vice's eyes look weird.\nThis is definitely not food.)",
    "佩尔正在沉睡，它的呼声震耳欲聋。": "Pael is asleep; its breathing shakes the room.",
    "一直以来都多谢了……": "Thank you for everything...",
    "所有人向佩尔小声做了告别。": "Everyone whispers farewell to Pael.",
    "临空者号轻轻带着大家离去，没有惊动那位大朋友。": "The Sky Carrier quietly carries everyone away without waking the big friend.",
    # Darv
    "什么？又一个来行商的……？\n行吧，你有什么值得来交易的吗？": "What? Another peddler...?\nFine—got anything worth trading?",
    "（可我们简直一穷二白啊。）": "(We're basically broke.)",
    "但卡莲和薇丝马上就亮出了战利品，\n交易看来成立了。": "But Karen and Vice immediately show their spoils—\nthe trade seems on.",
    "开个价吧……\n要出多少钱才能买下你那台巨像？": "Name a price...\nHow much for that colossus of yours?",
    "临空者号是我的朋友，\n不是能交易的东西。": "The Sky Carrier is my friend,\nnot something for sale.",
    "啊……行吧……真让人羡慕……": "Ah... fine... how enviable...",
    "让我找找……你说要的东西……": "Let me dig... for what you asked...",
    "我们可能随时要走啦，\n送你这个，是我带过来的稀有品哦。": "We may leave anytime—\nhere, a rare piece I brought.",
    "什么？！\n好吧……这次给你点好东西……": "What?!\nFine... I'll give you something good this time...",
    # Zeta
    "空之末裔？！怎么是你们？\n这里是哪里？\n勇者泽塔终于来到需要拯救的异世界了吗？": "Caelestites?! Why you?!\nWhere is this?\nHas Hero Zeta finally arrived in a world that needs saving?!",
    "冷静一点，事情大概是这样……\n总之，这里看上去很安全，你别乱走。": "Calm down. Roughly speaking...\nThis place looks safe. Don't wander.",
    "好难懂……\n总而言之，你会帮本勇者回去对吧？": "So confusing...\nAnyway, you'll help this hero get back, right?",
    "正好，本勇者找到了些东西，你看看吧。": "Perfect—this hero found some things. Take a look.",
    "空之末裔！你们不是上去了吗？\n怎么会从下面走上来！": "Caelestites! Weren't you going up?\nWhy come from below?!",
    "事情非常复杂……": "It's complicated...",
    "要不带上我吧，待在这里不能飙车，实在……\n太……太……太……无聊啦——": "Take me with you? I can't race here—it's...\nso... so... so boring—",
    "快看，本勇者已经交到原住民朋友了！\n咦？它怎么见到你们就跑。": "Look! This hero made a local friend!\nHuh? Why did it run from you?",
    "是上次偷了我们食物的人啊……": "That's the one who stole our food last time...",
    "在我们回去之前，你还是小心点吧。\n不要乱和奇奇怪怪的人交朋友啊！": "Until we return, be careful.\nDon't befriend weird strangers!",
    # William
    "一位华丽的年轻人坐在宫殿中央的地上……\n他的周围摆满了书籍。": "A splendid youth sits on the palace floor...\nsurrounded by books.",
    "……威廉先生？\n呃，好像哪里看到您也不惊讶。": "...Mr. William?\nSomehow I'm not surprised to see you.",
    "坐，我等你们很久了。": "Sit. I've waited long for you.",
    "这里有太多值得我着迷的事情了。\n我还不打算离开。": "Too much here fascinates me.\nI won't leave yet.",
    "但我们得回去啊。": "But we have to go back.",
    "我会继续提供帮助，你就好好加油吧。": "I'll keep helping. Do your best.",
    "隐修大师，我感觉您是不是有点乐在其中。": "Hermit Master, you seem to be enjoying this.",
    "既然追求真理，又怎会抗拒崭新的未知？\n别担心，时机到了我也不会强留。": "Seeking truth means welcoming the unknown.\nWhen the time comes, I won't linger.",
    "至少此时此刻……\n让我们一同享受这次漂流的过程吧。": "For now...\nlet us enjoy this drift together.",
    "眼神很不错，看来你找到诀窍了啊。": "Good eyes. You've found the trick.",
    "嗯，您呢？有收获吗？": "And you? Any gains?",
    "那还用说？年轻人，出发吧。": "Need you ask? Go, young one.",
    "我吗？不必担心。\n我和「霜夜绅士」会顺流踏上回归之旅。": "Me? Don't worry.\nFrostnight Gentleman and I will ride the current home.",
    # Nonupeipe
    "哎呀，多么出人意料的客人。": "My, what unexpected guests.",
    "您好。": "Hello.",
    "我喜欢有礼貌的孩子。": "I like polite children.",
    "来吧，虽然你们已经足够耀眼，但还是给自己挑些赐福吧。": "Come—though you already shine, pick some blessings anyway.",
    "我们又见面了，你们的光彩丝毫不褪色呢。": "We meet again; your radiance hasn't faded.",
    "虽然结果不是很好，不过我们会继续努力。": "Results weren't great, but we'll keep trying.",
    "很好～来，想要什么都随便拿吧。": "Wonderful~ Take whatever you like.",
    "每次见到你们，这光芒都让我移不开目光。": "Every time I see you, that light holds my eyes.",
    "谢谢您一直帮忙。\n我们可能随时会离开了。": "Thank you for always helping.\nWe may leave anytime.",
    "没关系，来喝点茶，然后带走我的临别礼物吧。": "No matter—have some tea, then take my parting gifts.",
    # Vakuu
    "又一个星间来客？\n欢迎光临，你看起来很需要帮助啊。": "Another guest from the stars?\nWelcome—you look like you need help.",
    "您好，那个……": "Hello, uh...",
    "来，拿好这份契约，好好完整地阅读它。\n在完全知情、主动、而且愿意接受所有相关责任的条件下签下名字吧。": "Here—take this contract and read it fully.\nSign only if informed, willing, and accepting all related duties.",
    "需要一份新契约吗？\n看起来你非常需要。": "Need a new contract?\nLooks like you really do.",
    "不不不，这上面根本是霸王条款，我不想签。": "No no no—these are predatory terms. I won't sign.",
    "这只不过是一点小小的代价。\n你也不想错过登顶的助力吧？": "Just a tiny price.\nYou wouldn't want to miss help for the summit, would you?",
    "别给我看了！我绝对！绝对不会签！": "Don't show me that! I absolutely will NOT sign!",
    "要是我说赠品关乎空谷呢？\n你难道一点也不好奇它从前的事情吗？": "What if the freebie concerns Caelesta?\nAren't you curious about its past?",
    "不对……\n你不可能知道的。": "No...\nYou couldn't know that.",
    "我是恶魔，孩子。": "I am a demon, child.",
    # Tanx
    "乘坐巨龙的战士，你用什么战斗？！！": "Warrior who rides a dragon—what do you fight with?!!",
    "我通常不会直接战斗，而是辅助朋友们。": "I usually don't fight directly—I support my friends.",
    "团队合作也不错！但怎能没有武器！\n这些武器任你选，来打一架吧！！！": "Teamwork's fine! But no weapons?!\nPick any—let's fight!!!",
    "飞翔的战士们，来战！！！": "Flying warriors, fight!!!",
    "众人急头白脸地和坦克斯打作一团。": "Everyone scrambles into a brawl with Tanx.",
    "痛快，收下这个！！！": "Refreshing—take this!!!",
    "你们的武器欠缺保养，拿上这些！": "Your weapons need care—take these!",
    "呃……不会还要打吧？": "Uh... we're not fighting again, right?",
    "意识到什么的空裔抱起武器就跑，靠巨像溜之大吉。": "Catching on, the Caelestite grabs the weapons and flees on the colossus.",
    # Architect
    "你好？虽然不知道你是什么目的……\n但能放我们离开吗？": "Hello? I don't know your purpose...\nbut will you let us leave?",
    "会漂流到这里的旅人……真的存在归宿吗？\n亦或只是个谎言，是来自记忆中的片段呢？\n你已经见过记忆被虚构出来的模样，\n又为何确信旅途的目的地存在？": "Travelers who drift here... does a home truly exist?\nOr is it a lie—a scrap of memory?\nYou've seen memories fabricated.\nWhy trust the destination exists?",
    "多么丰富多彩的颜色，不妨为我暂留吧。\n直到将那个世界、那些记忆，化为一份收藏。\n我承诺，你会得到与先前无异的一切。": "Such rich colors—stay for me a while.\nUntil that world and those memories become a collection.\nI promise you'll have everything as before.",
    "不要再被他人煽动，等待吧。": "Stop being stirred by others. Wait.",
    "上次我们的话还没说完呢！\n我确定有人在等我们……\n我们的归宿真实存在！": "We weren't finished last time!\nSomeone is waiting for us...\nOur home is real!",
    "我们没空陪你搞什么收藏。": "We have no time for your collecting.",
    "你……\n如果一道光能容下所有色彩，那他还会是「无色」吗？\n也许从这其中脱胎的「染料」，能为这一切重新上色。": "You...\nIf one light holds every color, is it still \"colorless\"?\nPerhaps a \"dye\" born of that could recolor everything.",
    "极好的种子啊，停下。\n我不希望你被前方吞噬。": "Excellent seed—stop.\nI don't want you swallowed by what lies ahead.",
    "告诉我回去的办法！": "Tell me how to go back!",
    "你又来了吗？\n不管尝试多少次，你都……": "You're back?\nNo matter how many times you try, you...",
    "就算是这样，我们也会努力的。\n总有一天，我们会……": "Even so, we'll keep trying.\nSomeday, we will...",
    "我是来向你告别的。\n我们要打败你，然后回去。": "I came to say goodbye.\nWe'll defeat you, then go home.",
    "告别是礼节，亦或是你的执着？\n为何你仍保持这样的仪式感，\n在我毫不犹豫地击溃你们无数次后？": "Is farewell courtesy—or obsession?\nWhy keep this ritual\nafter I've crushed you countless times?",
    "如果不说一声，总觉得事情就没有了结。\n虽然我还是搞不懂你，但我还是会跟你说再见的。\n而且我们也早就做好回去的准备了。": "Without a word, it doesn't feel finished.\nI still don't understand you, but I'll say goodbye.\nAnd we've long been ready to return.",
    "临空者号，准备返航！": "Sky Carrier—prepare for return!",
}


def translate(text: str) -> str:
    # 旁白 italic +「」 wrapper
    bare = text
    italic = False
    if text.startswith("[i][font_size=22]") and text.endswith("[/font_size][/i]"):
        italic = True
        bare = text[len("[i][font_size=22]") : -len("[/font_size][/i]")]
        if bare.startswith("「") and bare.endswith("」"):
            bare = bare[1:-1]
    en = ENG_MAP.get(bare)
    if en is None:
        en = bare  # fallback keep CN if missing
    if italic:
        return f"[i][font_size=22]「{en}」[/font_size][/i]"
    return en


def visit_override(ancient_name: str, dialogue_index: int, meeting: str) -> int | None:
    """返回自定义 VisitIndex；None 则用默认规则。"""
    if ancient_name == "威廉":
        # 1→0, 2→1, 3→2, N→4
        if meeting == "3":
            return 2
        if meeting == "N":
            return 4
    if ancient_name == "建筑师":
        # Architect 用 dialogueIndex 本身；N 作为第 3 套可重复
        return None
    if meeting == "N" and dialogue_index == 2:
        return 4  # 默认已是 4
    return None


def build_dialogues() -> tuple[dict, dict]:
    rows = load_sheet_rows()
    zhs: dict[str, str] = {}
    eng: dict[str, str] = {}

    # 仅空裔对先古：R2–R39（不含 R40+ 对其他角色）
    # 按 ancient 聚合，meeting 顺序 1,2,3,N
    by_ancient: dict[str, list[dict]] = {}
    for item in rows:
        r = item["r"]
        c = item["c"]
        if r < 2 or r > 39:
            continue
        name = (c.get("D") or "").strip()
        if name not in ANCIENT_IDS:
            continue
        by_ancient.setdefault(name, []).append(c)

    for name, meetings in by_ancient.items():
        ancient_id = ANCIENT_IDS[name]
        if name == "启迪者":
            ids = list(ENLIGHTENER_ALIASES)
        else:
            ids = [ancient_id]
            mirror = RUNTIME_ANALYZER_MIRRORS.get(ancient_id)
            if mirror:
                ids.append(mirror)

        # 排序：数字 meeting 升序，N 最后
        def sort_key(cells):
            e = str(cells.get("E", ""))
            if e == "N":
                return 99
            try:
                return int(e)
            except ValueError:
                return 50

        meetings = sorted(meetings, key=sort_key)

        for dialogue_index, cells in enumerate(meetings):
            meeting = str(cells.get("E", "")).strip()
            lines = pairs_from_row(cells)
            is_repeatable = meeting == "N"
            for line_index, (speaker, text) in enumerate(lines):
                if not text:
                    continue
                role = role_suffix(speaker)
                body = format_text(speaker, text)
                # r 标记：可重复套的每一行都必须带 r（引擎禁止同套混用）
                rflag = "r" if is_repeatable else ""
                key_mid = f"{dialogue_index}-{line_index}{rflag}.{role}"
                for aid in ids:
                    base = f"{aid}.talk.{CHAR}."
                    zhs[base + key_mid] = body
                    eng[base + key_mid] = translate(body)
                    # next（非最后一句；可重复套也要带 r）
                    if line_index < len(lines) - 1:
                        zhs[base + f"{dialogue_index}-{line_index}{rflag}.next"] = NEXT
                        eng[base + f"{dialogue_index}-{line_index}{rflag}.next"] = NEXT_EN

            # visit override
            vo = visit_override(name, dialogue_index, meeting)
            if vo is not None:
                for aid in ids:
                    zhs[f"{aid}.talk.{CHAR}.{dialogue_index}-visit"] = str(vo)
                    eng[f"{aid}.talk.{CHAR}.{dialogue_index}-visit"] = str(vo)
            # 威廉第3套：默认会把 index2→visit4，必须强制 2（独立第三遇）。
            # 可重复套则从 visit3 起可用，避免 charVisits==3 时空集导致 SetupLayout NRE。
            if name == "威廉" and meeting == "3":
                for aid in ids:
                    zhs[f"{aid}.talk.{CHAR}.2-visit"] = "2"
                    eng[f"{aid}.talk.{CHAR}.2-visit"] = "2"
            if name == "威廉" and meeting == "N":
                for aid in ids:
                    zhs[f"{aid}.talk.{CHAR}.3-visit"] = "3"
                    eng[f"{aid}.talk.{CHAR}.3-visit"] = "3"

    return zhs, eng


def merge_keep_non_talk(path: Path, talk_updates: dict, also_drop_prefixes: list[str]) -> None:
    data = json.loads(path.read_text(encoding="utf-8")) if path.exists() else {}
    # 删除旧的空裔 talk（避免残留）
    for prefix in also_drop_prefixes:
        keys = [k for k in data if k.startswith(prefix)]
        for k in keys:
            del data[k]
    data.update(talk_updates)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Updated {path} (+{len(talk_updates)} talk keys)")


def main() -> None:
    zhs, eng = build_dialogues()
    prefixes = [
        f"{aid}.talk.{CHAR}."
        for aid in set(ANCIENT_IDS.values()) | set(ENLIGHTENER_ALIASES)
    ]
    # 也清理旧短 stub
    prefixes += [
        f"NEOW.talk.{CHAR}.",
        f"DARV.talk.{CHAR}.",
    ]
    merge_keep_non_talk(ZHS, zhs, prefixes)
    merge_keep_non_talk(ENG, eng, prefixes)

    # 校验缺英文映射
    missing = sorted({t for t in ENG_MAP.values() if False})  # noqa
    bare_missing = []
    for k, v in zhs.items():
        if not k.endswith(".ancient") and not k.endswith(".char"):
            continue
        bare = v
        if bare.startswith("[i][font_size=22]"):
            bare = bare[len("[i][font_size=22]") : -len("[/font_size][/i]")]
            if bare.startswith("「") and bare.endswith("」"):
                bare = bare[1:-1]
        if bare not in ENG_MAP:
            bare_missing.append(bare)
    if bare_missing:
        print("WARN missing ENG_MAP count:", len(set(bare_missing)))
        for b in sorted(set(bare_missing))[:20]:
            print("  ", repr(b[:60]))
    else:
        print("ENG_MAP complete")


if __name__ == "__main__":
    main()
