# -*- coding: utf-8 -*-
"""修复先古对话：同一对话套若有 r，则该套全部行都带 r。"""
from __future__ import annotations

import json
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(r"H:\STS2MOD\AlchemyStars\AlchemyStars\localization")
PAT = re.compile(
    r"^(?P<pre>.+\.talk\.[^.]+)\.(?P<d>\d+)-(?P<l>\d+)(?P<r>r?)\.(?P<suf>ancient|char|next|sfx)$"
)


def fix_file(path: Path) -> int:
    data = json.loads(path.read_text(encoding="utf-8"))
    # 找出混用 r 的对话套
    sets: dict[str, dict] = defaultdict(lambda: {"has_r": False, "has_nor": False, "keys": []})
    for k in data:
        m = PAT.match(k)
        if not m:
            continue
        sid = f"{m['pre']}.{m['d']}"
        if m["r"]:
            sets[sid]["has_r"] = True
        else:
            sets[sid]["has_nor"] = True
        sets[sid]["keys"].append(k)

    mixed = [sid for sid, info in sets.items() if info["has_r"] and info["has_nor"]]
    if not mixed:
        print(path.name, "no mixed sets")
        return 0

    new_data = {}
    renamed = 0
    for k, v in data.items():
        m = PAT.match(k)
        if not m:
            new_data[k] = v
            continue
        sid = f"{m['pre']}.{m['d']}"
        if sid in mixed and not m["r"]:
            # 补上 r：prefix.d-lr.suf
            nk = f"{m['pre']}.{m['d']}-{m['l']}r.{m['suf']}"
            new_data[nk] = v
            renamed += 1
        else:
            new_data[k] = v

    path.write_text(json.dumps(new_data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(path.name, "fixed mixed sets:", len(mixed), "renamed keys:", renamed)
    for sid in sorted(mixed):
        print(" ", sid)
    return renamed


def main() -> None:
    for lang in ("zhs", "eng"):
        fix_file(ROOT / lang / "ancients.json")


if __name__ == "__main__":
    main()
