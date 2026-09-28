# -*- coding: utf-8 -*-
import zipfile, re, json
from xml.etree import ElementTree as ET

path = r"h:\STS2MOD\白夜极光\白夜极光卡组.xlsx"
ns = {"m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}

with zipfile.ZipFile(path) as z:
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
        name = s.attrib.get("name")
        rid = s.attrib.get("{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id")
        if name == "对话":
            target = rid_to_target[rid]
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
        cells = {}
        for c in row.findall("m:c", ns):
            cells[col(c.attrib["r"])] = cell_val(c)
        if any(str(v).strip() for v in cells.values()):
            rows.append((rnum, cells))

    out = r"H:\STS2MOD\AlchemyStars\tools\_dialogue_sheet_dump.json"
    with open(out, "w", encoding="utf-8") as f:
        json.dump([{ "r": r, "c": cells} for r, cells in rows], f, ensure_ascii=False, indent=2)
    print("rows", len(rows), "->", out)
    # print header + first 50 briefly
    for rnum, cells in rows[:55]:
        b = cells.get("B", "")
        c = cells.get("C", "")
        d = cells.get("D", "")
        e = cells.get("E", "")
        print(f"R{rnum} B={b!r} C={c!r} D={d!r} E={e!r}")
