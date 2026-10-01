# -*- coding: utf-8 -*-
"""修复威廉/泽塔先古对话 visit 索引空洞，避免 NEventRoom.SetupLayout NRE。"""
from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "AlchemyStars" / "localization"

# 引擎 GetValidDialogues：先精确匹配 VisitIndex==charVisits，否则仅取
# IsRepeating 且 charVisits >= VisitIndex 的对话。visit 序列出现空洞会选空集并 NRE。
FIXES = {
    # 威廉角色专属：visit2 有独立对话，重复对话应从 visit3 起可用
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.3-visit": "3",
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.4-visit": "3",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_WILLIAM.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.3-visit": "3",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_WILLIAM.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.4-visit": "3",
    # 威廉 ANY：重复对话从 visit2 起可用
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM.talk.ANY.2-visit": "2",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_WILLIAM.talk.ANY.2-visit": "2",
    # 泽塔同样补齐空洞
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA.talk.ANY.2-visit": "2",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ZETA.talk.ANY.2-visit": "2",
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.2-visit": "2",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ZETA.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.2-visit": "2",
    "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.3-visit": "2",
    "ALCHEMY_STARS_ANCIENT_ALCHEMY_STARS_ZETA.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER.3-visit": "2",
}


def main() -> None:
    for lang in ("zhs", "eng"):
        path = ROOT / lang / "ancients.json"
        data = json.loads(path.read_text(encoding="utf-8"))
        changed = 0
        for key, value in FIXES.items():
            if data.get(key) != value:
                data[key] = value
                changed += 1
        path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"{path}: updated {changed} keys")


if __name__ == "__main__":
    main()
