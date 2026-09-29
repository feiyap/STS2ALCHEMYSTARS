# -*- coding: utf-8 -*-
import json
from pathlib import Path

root = Path(r"H:/STS2MOD/AlchemyStars/AlchemyStars/localization")

z = json.loads((root / "zhs" / "relics.json").read_text(encoding="utf-8"))
z["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LIGHT_AMBER.description"] = (
    "每当有光能被消耗，补充 [blue]1[/blue] 点该属性光能。"
)
z["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LIGHT_AMBER.flavor"] = (
    "擅长「创造」的储君提供了一些建议。\n"
    "「你的记忆塑造了他人的印象，那些虚构也能成为助力。」"
)
(root / "zhs" / "relics.json").write_text(
    json.dumps(z, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

e = json.loads((root / "eng" / "relics.json").read_text(encoding="utf-8"))
e["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LIGHT_AMBER.description"] = (
    "Whenever you spend Light Energy, gain [blue]1[/blue] of that Attribute."
)
e["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LIGHT_AMBER.flavor"] = (
    'A Sovereign skilled in "Creation" offered some advice.\n'
    '"Your memories shape others\' impressions; those fictions can become strength."'
)
(root / "eng" / "relics.json").write_text(
    json.dumps(e, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

# 希罗娜异画名：引擎用文件名中的中文段作为显示名，去日遗痕已存在。
print("light amber flavor ok")
