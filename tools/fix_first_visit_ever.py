# -*- coding: utf-8 -*-
"""删除只有首句的 firstVisitEver，让首次遇见走角色/ANY 完整第 0 套对话。"""
import json
from pathlib import Path

ROOT = Path(r"H:\STS2MOD\AlchemyStars\AlchemyStars\localization")
FILES = [ROOT / "zhs" / "ancients.json", ROOT / "eng" / "ancients.json"]


def fix(path: Path) -> int:
    data = json.loads(path.read_text(encoding="utf-8"))
    to_del = [k for k in data if ".talk.firstVisitEver." in k]
    for k in to_del:
        del data[k]
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return len(to_del)


for f in FILES:
    n = fix(f)
    print(f"{f.name}: removed {n} firstVisitEver keys")
