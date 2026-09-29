# -*- coding: utf-8 -*-
"""从白夜极光卡组.xlsx提取与终极调整相关的设计文案。"""
import zipfile
import re
import xml.etree.ElementTree as ET
from pathlib import Path
from collections import defaultdict

xlsx = Path(r"e:\Download\白夜极光卡组.xlsx")
adjust_path = Path(r"H:\STS2MOD\AlchemyStars\_tmp_ultimate_adjust.txt")
out_path = Path(r"H:\STS2MOD\AlchemyStars\_tmp_design_specs.txt")

NS = {
    "m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main",
    "a": "http://schemas.openxmlformats.org/drawingml/2006/main",
    "xdr": "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing",
    "r": "http://schemas.openxmlformats.org/officeDocument/2006/relationships",
}


def parse_ref(ref):
    m = re.match(r"([A-Z]+)(\d+)", ref)
    return m.group(1), int(m.group(2))


def load_shared_strings(z):
    ss = []
    root = ET.fromstring(z.read("xl/sharedStrings.xml"))
    for si in root.findall("m:si", NS):
        texts = []
        for t in si.iter("{http://schemas.openxmlformats.org/spreadsheetml/2006/main}t"):
            if t.text:
                texts.append(t.text)
        ss.append("".join(texts))
    return ss


def load_sheet(z, path, ss):
    root = ET.fromstring(z.read(path))
    rows = defaultdict(dict)
    for c in root.findall(".//m:sheetData/m:row/m:c", NS):
        ref = c.get("r")
        col, rn = parse_ref(ref)
        t = c.get("t")
        v = c.find("m:v", NS)
        is_el = c.find("m:is", NS)
        val = ""
        if t == "s" and v is not None and v.text is not None:
            val = ss[int(v.text)]
        elif t == "inlineStr" and is_el is not None:
            texts = []
            for te in is_el.iter(
                "{http://schemas.openxmlformats.org/spreadsheetml/2006/main}t"
            ):
                if te.text:
                    texts.append(te.text)
            val = "".join(texts)
        elif v is not None and v.text is not None:
            val = v.text
        rows[rn][col] = val
    return rows


def get_cell(row, *cols):
    for c in cols:
        if c in row and row[c]:
            return row[c].strip()
    return ""


# ---------- 解析终极调整 ----------
adjust_text = adjust_path.read_text(encoding="utf-8")
adjust_entries = []
for line in adjust_text.splitlines():
    m = re.match(r"^R(\d+):\s*(.*)$", line)
    if not m:
        continue
    rn = int(m.group(1))
    if rn == 1:
        continue
    # 正则 \s* 会吃掉前导截图空列的 tab；剩余多为：描述 \t 备注(日期) \t 状态
    rest = m.group(2)
    parts = [p.strip() for p in rest.split("\t")]
    while len(parts) < 4:
        parts.append("")

    def looks_like_date(s):
        return bool(re.search(r"\d{4}年\d{1,2}月", s or ""))

    if parts[0] == "" and parts[1]:
        desc, remark, status = parts[1], parts[2], parts[3]
    elif parts[0] and "相关" in parts[0] and parts[1] and not looks_like_date(parts[1]):
        # 分类标题 + 正文（如「能力卡相关」）
        desc = parts[0] + "：" + parts[1]
        remark, status = parts[2], parts[3]
    else:
        desc = parts[0]
        remark = parts[1]
        status = parts[2]
        # 防止把日期误拼进描述
        if looks_like_date(remark) and "：" in desc and looks_like_date(desc.split("：")[-1]):
            pass  # 已污染则下面清洗
        if looks_like_date(desc.split("：")[-1] if "：" in desc else ""):
            # 描述尾部误含日期时剥掉
            head, _, tail = desc.rpartition("：")
            if looks_like_date(tail) and head:
                desc = head
                if not remark:
                    remark = tail

    name, body = "", desc
    if "：" in desc:
        name, body = desc.split("：", 1)
        name, body = name.strip(), body.strip()

    adjust_entries.append(
        {
            "row": rn,
            "desc": desc,
            "name": name,
            "body": body,
            "remark": remark,
            "status": status,
        }
    )

print(f"adjust entries: {len(adjust_entries)}")

with zipfile.ZipFile(xlsx) as z:
    ss = load_shared_strings(z)

    # ---------- sheet11 drawing 映射 ----------
    drawing = ET.fromstring(z.read("xl/drawings/drawing3.xml"))
    rels = ET.fromstring(z.read("xl/drawings/_rels/drawing3.xml.rels"))
    rid_to_img = {rel.get("Id"): rel.get("Target") for rel in rels}

    row_has_image = set()
    row_images = defaultdict(list)
    for tag in ("twoCellAnchor", "oneCellAnchor"):
        for anc in drawing.findall(f"xdr:{tag}", NS):
            frm = anc.find("xdr:from", NS)
            if frm is None:
                continue
            row_el = frm.find("xdr:row", NS)
            if row_el is None or row_el.text is None:
                continue
            excel_row = int(row_el.text) + 1
            blip = anc.find(".//a:blip", NS)
            img = ""
            if blip is not None:
                embed = blip.get(
                    "{http://schemas.openxmlformats.org/officeDocument/2006/relationships}embed"
                )
                img = rid_to_img.get(embed, "")
            row_has_image.add(excel_row)
            if img:
                row_images[excel_row].append(Path(img).name)

    print(
        f"rows with images: {len(row_has_image)} "
        f"range={min(row_has_image)}-{max(row_has_image)}"
    )

    for e in adjust_entries:
        e["has_screenshot"] = e["row"] in row_has_image
        e["images"] = row_images.get(e["row"], [])

    imaged = [e for e in adjust_entries if e["has_screenshot"]]
    print(f"adjust rows with screenshot: {len(imaged)}")

    # ---------- 加载设计表 ----------
    sheet_meta = [
        ("sheet2", "空裔卡组&遗物"),
        ("sheet3", "雷"),
        ("sheet4", "森"),
        ("sheet5", "水"),
        ("sheet6", "火"),
        ("sheet13", "遗物清单"),
    ]

    designs = []

    def add_design(sheet, side, row, name, cost, rarity, ctype, attr, desc, extra=None):
        if not name:
            return
        designs.append(
            {
                "sheet": sheet,
                "side": side,
                "row": row,
                "name": name.strip(),
                "cost": cost,
                "rarity": rarity,
                "type": ctype,
                "attr": attr,
                "desc": desc or "",
                "extra": extra or {},
            }
        )

    for sn, sname in sheet_meta:
        rows = load_sheet(z, f"xl/worksheets/{sn}.xml", ss)
        if sn == "sheet13":
            for rn, row in rows.items():
                name = get_cell(row, "C")
                desc = get_cell(row, "D", "E", "F", "G", "H")
                cost = get_cell(row, "B")
                add_design(sname, "main", rn, name, cost, "", "遗物", "", desc)
            continue

        if sn == "sheet2":
            for rn, row in rows.items():
                if rn == 1:
                    continue
                add_design(
                    sname,
                    "left",
                    rn,
                    get_cell(row, "C"),
                    get_cell(row, "B"),
                    get_cell(row, "D"),
                    get_cell(row, "E"),
                    get_cell(row, "F"),
                    get_cell(row, "G"),
                    {"备注": get_cell(row, "H")},
                )
                add_design(
                    sname,
                    "right",
                    rn,
                    get_cell(row, "L"),
                    get_cell(row, "O"),
                    get_cell(row, "M"),
                    get_cell(row, "N") or get_cell(row, "J"),
                    get_cell(row, "P"),
                    get_cell(row, "Q"),
                    {
                        "归属": get_cell(row, "K"),
                        "获取方式": get_cell(row, "R"),
                        "图鉴文案": get_cell(row, "S"),
                    },
                )
        else:
            for rn, row in rows.items():
                if rn == 1:
                    continue
                add_design(
                    sname,
                    "left",
                    rn,
                    get_cell(row, "C"),
                    get_cell(row, "B"),
                    get_cell(row, "D"),
                    get_cell(row, "E"),
                    get_cell(row, "F"),
                    get_cell(row, "G"),
                )
                add_design(
                    sname,
                    "right",
                    rn,
                    get_cell(row, "K"),
                    get_cell(row, "N"),
                    get_cell(row, "L"),
                    get_cell(row, "M"),
                    get_cell(row, "O"),
                    get_cell(row, "P"),
                    {
                        "归属": get_cell(row, "J"),
                        "类型标记": get_cell(row, "I"),
                        "获取方式": get_cell(row, "Q"),
                    },
                )

print(f"total design rows: {len(designs)}")

# ---------- 匹配 ----------
ALIASES = {
    "水巴顿": ["豪荣铁颚·巴顿", "巴顿"],
    "总攻击": ["极光时刻"],
    "原来的总攻击": ["极光时刻", "觉醒形态"],
    "纳努赛尔衍生牌": ["纳努赛尔", "波伊特", "灵鹃·波伊特"],
    "希罗娜皮肤": ["希罗娜", "去日遗痕", "终末之龙·希罗娜"],
    "火属性伊斯塔万": ["千痕影主·伊斯塔万", "伊斯塔万"],
    "维多利亚异画": ["维多利亚·墓歌", "维多利亚"],
    "反叛灼燃之日": ["反叛灼燃之日", "反叛灼燃"],
    "反叛灼燃": ["反叛灼燃之日", "反叛灼燃"],
    "莱因哈特相关": ["莱因哈特", "启明之光·莱因哈特", "反叛灼燃·莱因哈特"],
    "初始遗物与先古遗物": ["先天枷锁", "自由和弦"],
    "出门方案": ["光能追踪方案"],
    "回执邮件": ["回执邮件"],
    "光珀文本": ["光珀"],
    "毒脉异变·丽贝卡": ["丽蓓卡", "丽贝卡", "毒脉异变"],
    "丽贝卡": ["丽蓓卡", "毒脉异变·丽蓓卡"],
    "静默雷霆·米歇尔": ["米迦勒", "米歇尔", "静默雷霆"],
    "万应灵药·厘青": ["厘清", "厘青", "万应灵药"],
    "厘青": ["厘清", "万应灵药·厘清"],
    "渉": ["涉", "贪婪之蛇·涉"],
    "壮志凌云巴顿": ["壮志凌云·巴顿"],
    "还是伊伦汀": ["伊伦汀", "金泽之星·伊伦汀"],
    "伊伦汀": ["金泽之星·伊伦汀"],
    "寂静之陵": ["星辰纹章"],
    "觉醒形态": ["觉醒形态"],
    "温德岚之日": ["温德岚之日"],
    "镇魂座": ["镇魂座", "寂静猎兵·镇魂座"],
    "童谣座": ["童谣座", "寂静猎兵·童谣座"],
    "雷霆生成的超载+": ["超载", "巡航阵列·雷霆"],
    "雷霆": ["巡航阵列·雷霆"],
    "先古薇丝": ["薇丝·空瞳"],
    "先古卡莲": ["卡莲·煜魂"],
}


def find_designs_for(adj_name):
    if not adj_name:
        return []
    an = adj_name.strip()
    search_terms = [an]
    if an in ALIASES:
        search_terms.extend(ALIASES[an])
    if "·" in an:
        search_terms.append(an.split("·")[-1])
    if an.startswith("先古"):
        search_terms.append(an[2:])
        if "·" in an[2:]:
            search_terms.append(an[2:].split("·")[-1])

    hits = []
    for d in designs:
        dn = d["name"]
        matched = False
        for term in search_terms:
            if not term or len(term) < 1:
                continue
            if dn == term or dn == an:
                matched = True
                break
            if term in dn:
                matched = True
                break
            if "·" in dn and dn.split("·")[-1] == term:
                matched = True
                break
            if "·" in dn and dn.split("·")[-1] == an:
                matched = True
                break
            if len(term) >= 2 and ("·" + term) in dn:
                matched = True
                break
        if matched:
            hits.append(d)
    return hits


def is_vague(body):
    """仅有加强/削弱/调整等，无具体数值或机制细节。"""
    b = (body or "").strip()
    if not b:
        return True
    concrete_markers = [
        r"\d",
        "改为",
        "改成",
        "去掉",
        "不再",
        "获得",
        "造成",
        "消耗",
        "叠加",
        "层",
        "点",
        "费",
        "伤害",
        "格挡",
        "光能",
        "抽牌",
        "保留",
        "飞行",
        "重放",
        "百分比",
        "固伤",
        "回手",
        "免费",
        "升级",
        "被动",
        "卡名",
        "降低",
        "增加",
        "砍掉",
        "减1",
        "加1",
        "加到",
        "初始",
        "每回合",
        "不消耗",
        "不移除",
        "不会",
        "需要可",
        "要能叠加",
        "要可以叠加",
        "改了",
        "改名",
        "改得",
        "重做了",
        "变牌",
        "攻击牌",
        "能力",
        "文本",
        "词条",
        "敲",
        "烧",
        "过热",
        "人工",
        "权重",
        "称号",
        "皮肤",
        "对话",
        "卡图",
        "卡面",
        "先古显示",
        "普通卡面",
        "专属",
        "bug",
        "Bug",
        "BUG",
        "卡死",
        "生效",
        "写错",
        "漏改",
        "漏了",
        "没加",
        "没改",
        "没有",
        "好像",
        "应该是",
        "视为",
        "大开悟",
        "子弹",
        "检索",
        "灰烬",
        "死神",
        "海克斯",
        "解绑",
        "改绑",
        "说明文本",
        "深色格",
        "超载",
        "异画",
        "国际服",
        "打错字",
        "全自动",
    ]
    for m in concrete_markers:
        if re.search(m, b):
            return False
    vague_words = {
        "加强",
        "削弱",
        "调整",
        "重做",
        "小加强",
        "小削弱",
        "小砍",
        "小调整",
        "微调",
        "小规模重制",
        "重制",
        "调整强度",
        "调整、加强",
        "削弱下",
        "再次削弱",
    }
    if b in vague_words:
        return True
    cleaned = b
    for w in ["小规模", "再次", "下", "了", "、", "，", "。"]:
        cleaned = cleaned.replace(w, "")
    for w in ["加强", "削弱", "调整", "重做", "重制", "小砍", "小", "强度"]:
        cleaned = cleaned.replace(w, "")
    if cleaned.strip() == "" and any(
        w in b for w in ["加强", "削弱", "调整", "重做", "重制", "小砍"]
    ):
        return True
    return False


matched_by_name = {}
no_design_found = []
vague_cards = []
concrete_adjusts = []

for e in adjust_entries:
    hits = find_designs_for(e["name"]) if e["name"] else []
    seen = set()
    uniq = []
    for h in hits:
        key = (h["sheet"], h["side"], h["name"], h["desc"])
        if key not in seen:
            seen.add(key)
            uniq.append(h)
    e["designs"] = uniq
    if e["name"]:
        if uniq:
            matched_by_name.setdefault(e["name"], [])
            existing_keys = {
                (x["sheet"], x["side"], x["name"], x["desc"])
                for x in matched_by_name[e["name"]]
            }
            for u in uniq:
                k = (u["sheet"], u["side"], u["name"], u["desc"])
                if k not in existing_keys:
                    matched_by_name[e["name"]].append(u)
                    existing_keys.add(k)
        else:
            no_design_found.append(e)
    if is_vague(e["body"]):
        vague_cards.append(e)
    else:
        concrete_adjusts.append(e)

print(f"matched names: {len(matched_by_name)}")
print(f"no design: {len(no_design_found)}")
for e in no_design_found:
    print(f"  R{e['row']}: {e['name']} | {e['body'][:50]}")
print(f"vague: {len(vague_cards)} concrete: {len(concrete_adjusts)}")

# ---------- 写出 UTF-8 规格文件 ----------
lines = []
lines.append("=" * 72)
lines.append("白夜极光 · 终极调整相关设计规格提取")
lines.append("来源: 白夜极光卡组.xlsx (sheet2-6/13) + sheet11 终极调整")
lines.append("=" * 72)

# 1) sheet11 有截图的行
lines.append("")
lines.append("#" * 72)
lines.append("一、sheet11 有截图的调整行（drawing3 行锚点映射）")
lines.append("#" * 72)
for e in adjust_entries:
    if not e["has_screenshot"]:
        continue
    imgs = ", ".join(e["images"]) if e["images"] else "(未解析到文件名)"
    lines.append("")
    lines.append(f"[R{e['row']}] 截图={imgs}")
    lines.append(f"  描述: {e['desc']}")
    if e["remark"]:
        lines.append(f"  备注: {e['remark']}")
    if e["status"]:
        lines.append(f"  状态: {e['status']}")

# 2) 有明确设计文案的卡牌（匹配到设计表且描述非空）
lines.append("")
lines.append("#" * 72)
lines.append("二、有明确设计文案的卡牌/遗物（匹配终极调整提及名称）")
lines.append("#" * 72)

clear_design_list = []  # for summary
for adj_name, ds_list in matched_by_name.items():
    # 关联的调整说明
    related_adjusts = [e for e in adjust_entries if e["name"] == adj_name]
    adjust_notes = []
    for e in related_adjusts:
        note = f"R{e['row']}: {e['desc']}"
        if e["has_screenshot"]:
            note += f" [有截图:{','.join(e['images'])}]"
        adjust_notes.append(note)

    for d in ds_list:
        if not (d["desc"] or "").strip():
            continue
        clear_design_list.append((adj_name, d, adjust_notes))

# 按名称分组输出
seen_output = set()
for adj_name, ds_list in matched_by_name.items():
    related_adjusts = [e for e in adjust_entries if e["name"] == adj_name]
    has_any_desc = any((d["desc"] or "").strip() for d in ds_list)
    if not has_any_desc:
        continue
    lines.append("")
    lines.append("-" * 60)
    lines.append(f"【调整提及】{adj_name}")
    for e in related_adjusts:
        flag = " [有截图]" if e["has_screenshot"] else ""
        lines.append(f"  · R{e['row']}: {e['body']}{flag}")
    for d in ds_list:
        if not (d["desc"] or "").strip():
            continue
        key = (d["sheet"], d["name"], d["desc"])
        if key in seen_output:
            continue
        seen_output.add(key)
        lines.append("")
        lines.append(
            f"  >> [{d['sheet']}|{d['side']}] {d['name']}"
            f" | 费用:{d['cost'] or '-'} | 品质:{d['rarity'] or '-'}"
            f" | 类型:{d['type'] or '-'} | 属性:{d['attr'] or '-'}"
        )
        extra_bits = []
        for k, v in (d["extra"] or {}).items():
            if v:
                extra_bits.append(f"{k}={v}")
        if extra_bits:
            lines.append(f"     ({'; '.join(extra_bits)})")
        lines.append("  设计效果:")
        for dl in d["desc"].splitlines():
            lines.append(f"    {dl}")

# 匹配到名称但设计表无描述（如 sheet13 仅有名）
lines.append("")
lines.append("#" * 72)
lines.append("三、匹配到名称但设计表无描述正文")
lines.append("#" * 72)
for adj_name, ds_list in matched_by_name.items():
    if any((d["desc"] or "").strip() for d in ds_list):
        continue
    names = sorted({d["name"] for d in ds_list})
    sheets = sorted({d["sheet"] for d in ds_list})
    related = [e for e in adjust_entries if e["name"] == adj_name]
    lines.append("")
    lines.append(f"【{adj_name}】命中: {', '.join(names)} @ {', '.join(sheets)}")
    for e in related:
        lines.append(f"  · R{e['row']}: {e['desc']}")

# 未匹配到设计表
lines.append("")
lines.append("#" * 72)
lines.append("四、终极调整提及但未在 sheet2-6/13 命中设计行")
lines.append("#" * 72)
# 去重按 name
seen_nd = set()
for e in no_design_found:
    if not e["name"] or e["name"] in seen_nd:
        if not e["name"]:
            lines.append(f"  R{e['row']}: (无提取名称) {e['desc']}")
        continue
    seen_nd.add(e["name"])
    related = [x for x in adjust_entries if x["name"] == e["name"]]
    for x in related:
        lines.append(f"  R{x['row']}: {x['desc']}")

# 只有模糊调整语
lines.append("")
lines.append("#" * 72)
lines.append("五、仅有「调整/加强/削弱」等模糊措辞（无具体数值/机制）")
lines.append("#" * 72)
for e in vague_cards:
    flag = " [有截图]" if e["has_screenshot"] else ""
    design_hint = ""
    if e["designs"]:
        with_desc = [d for d in e["designs"] if (d["desc"] or "").strip()]
        if with_desc:
            design_hint = f" | 设计表有文案: {with_desc[0]['name']}"
        else:
            design_hint = f" | 设计表命中但无描述: {e['designs'][0]['name']}"
    else:
        design_hint = " | 设计表未命中"
    lines.append(f"  R{e['row']}: {e['desc']}{flag}{design_hint}")

# 汇总短表
lines.append("")
lines.append("#" * 72)
lines.append("六、汇总：有明确设计文案的卡牌（名称 + 效果全文）")
lines.append("#" * 72)

# unique by design card name
unique_cards = {}
for adj_name, ds_list in matched_by_name.items():
    for d in ds_list:
        if not (d["desc"] or "").strip():
            continue
        key = d["name"]
        if key not in unique_cards:
            unique_cards[key] = d
        elif len(d["desc"]) > len(unique_cards[key]["desc"]):
            unique_cards[key] = d

for name in sorted(unique_cards.keys()):
    d = unique_cards[name]
    lines.append("")
    lines.append(f"■ {d['name']}")
    lines.append(
        f"  来源:{d['sheet']} 费用:{d['cost'] or '-'} 品质:{d['rarity'] or '-'} "
        f"类型:{d['type'] or '-'} 属性:{d['attr'] or '-'}"
    )
    lines.append("  效果:")
    for dl in d["desc"].splitlines():
        lines.append(f"    {dl}")

lines.append("")
lines.append("#" * 72)
lines.append("七、汇总：仅模糊调整语的卡牌列表")
lines.append("#" * 72)
vague_names = []
for e in vague_cards:
    label = e["name"] if e["name"] else e["desc"]
    vague_names.append((e["row"], label, e["body"]))
for rn, label, body in vague_names:
    lines.append(f"  R{rn}: {label} — {body}")

out_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
print(f"\nWrote {out_path} ({out_path.stat().st_size} bytes)")
print(f"clear design cards: {len(unique_cards)}")
print(f"vague adjust rows: {len(vague_cards)}")

# 打印给父代理的简要汇总
print("\n===== CLEAR DESIGN CARDS =====")
for name in sorted(unique_cards.keys()):
    d = unique_cards[name]
    preview = d["desc"].replace("\n", " / ")[:100]
    print(f"{name} :: {preview}")

print("\n===== VAGUE ONLY =====")
for rn, label, body in vague_names:
    print(f"R{rn}: {label} — {body}")
