# -*- coding: utf-8 -*-
"""对比 relics.json 中文 title 与《遗物清单》。"""
import json
from pathlib import Path

LIST = json.loads(Path(r"H:\STS2MOD\AlchemyStars\tools\_relic_list_dump.json").read_text(encoding="utf-8"))
sheet_names = [row[2] for _, row in LIST if len(row) > 2 and row[2]]

relics = json.loads(
    Path(r"H:\STS2MOD\AlchemyStars\AlchemyStars\localization\zhs\relics.json").read_text(encoding="utf-8")
)
titles = {k: v for k, v in relics.items() if k.endswith(".title")}

print("=== sheet names (%d) ===" % len(sheet_names))
for i, n in enumerate(sheet_names, 1):
    print(f"{i:2d}. {n}")

print("\n=== current titles (%d) ===" % len(titles))
for k, v in sorted(titles.items(), key=lambda kv: kv[1]):
    print(f"  {v}  <= {k}")

print("\n=== sheet name not in current titles ===")
title_vals = set(titles.values())
for n in sheet_names:
    if n not in title_vals:
        # fuzzy: strip quotes
        bare = n.strip("「」")
        hits = [v for v in title_vals if bare in v or v in bare or v.replace("「", "").replace("」", "") == bare]
        print(f"  MISSING: {n!r}  candidates={hits}")

print("\n=== current titles not in sheet ===")
sheet_set = set(sheet_names)
sheet_bare = {n.strip("「」") for n in sheet_names}
for v in sorted(title_vals):
    if v not in sheet_set and v.strip("「」") not in sheet_bare:
        print(f"  EXTRA/OLD: {v!r}")
