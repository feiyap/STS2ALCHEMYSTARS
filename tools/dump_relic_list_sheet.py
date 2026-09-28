# -*- coding: utf-8 -*-
"""导出《遗物清单》sheet 的文本与当前 relics.json 标题对比。"""
from __future__ import annotations

import json
import re
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

XLSX = Path(r"H:\STS2MOD\白夜极光\白夜极光卡组.xlsx")
NS = {"m": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
REL_NS = {"r": "http://schemas.openxmlformats.org/package/2006/relationships"}


def col_row(cell_ref: str) -> tuple[int, int]:
    m = re.match(r"([A-Z]+)(\d+)", cell_ref)
    assert m
    col = 0
    for ch in m.group(1):
        col = col * 26 + (ord(ch) - 64)
    return col, int(m.group(2))


def load_shared_strings(z: zipfile.ZipFile) -> list[str]:
    if "xl/sharedStrings.xml" not in z.namelist():
        return []
    root = ET.fromstring(z.read("xl/sharedStrings.xml"))
    out: list[str] = []
    for si in root.findall("m:si", NS):
        texts = [t.text or "" for t in si.findall(".//m:t", NS)]
        out.append("".join(texts))
    return out


def sheet_path_for_name(z: zipfile.ZipFile, name: str) -> str:
    wb = ET.fromstring(z.read("xl/workbook.xml"))
    rid = None
    for sh in wb.findall("m:sheets/m:sheet", NS):
        if sh.get("name") == name:
            rid = sh.get("{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id")
            break
    if not rid:
        raise SystemExit(f"sheet not found: {name}")
    rels = ET.fromstring(z.read("xl/_rels/workbook.xml.rels"))
    for rel in rels.findall("r:Relationship", REL_NS):
        if rel.get("Id") == rid:
            target = rel.get("Target")
            assert target
            return "xl/" + target.lstrip("/")
    raise SystemExit(f"rid not found: {rid}")


def load_sheet_grid(z: zipfile.ZipFile, sheet_name: str) -> dict[tuple[int, int], str]:
    shared = load_shared_strings(z)
    path = sheet_path_for_name(z, sheet_name)
    root = ET.fromstring(z.read(path))
    grid: dict[tuple[int, int], str] = {}
    for c in root.findall(".//m:c", NS):
        ref = c.get("r")
        if not ref:
            continue
        col, row = col_row(ref)
        t = c.get("t")
        v = c.find("m:v", NS)
        is_el = c.find("m:is", NS)
        val = ""
        if t == "s" and v is not None and v.text is not None:
            val = shared[int(v.text)]
        elif t == "inlineStr" and is_el is not None:
            val = "".join(t.text or "" for t in is_el.findall(".//m:t", NS))
        elif v is not None and v.text is not None:
            val = v.text
        if val != "":
            grid[(col, row)] = val
    return grid


def main() -> None:
    with zipfile.ZipFile(XLSX) as z:
        grid = load_sheet_grid(z, "遗物清单")

    max_r = max(r for _, r in grid)
    max_c = max(c for c, _ in grid)
    print(f"grid {max_c}x{max_r}, cells={len(grid)}")
    print("=== header row1-3 ===")
    for r in range(1, 4):
        row = [grid.get((c, r), "") for c in range(1, min(max_c, 20) + 1)]
        print(r, row)

    print("\n=== all non-empty rows (cols 1-12) ===")
    rows = []
    for r in range(1, max_r + 1):
        row = [grid.get((c, r), "") for c in range(1, min(max_c, 12) + 1)]
        if any(row):
            rows.append((r, row))
            print(r, row)

    # dump for further processing
    out = Path(r"H:\STS2MOD\AlchemyStars\tools\_relic_list_dump.json")
    out.write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8")
    print("wrote", out)


if __name__ == "__main__":
    main()
