# -*- coding: utf-8 -*-
import json
import re
from collections import defaultdict
from pathlib import Path

p = Path(r"H:\STS2MOD\AlchemyStars\AlchemyStars\localization\zhs\ancients.json")
data = json.loads(p.read_text(encoding="utf-8"))

prefix = "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA.talk.ALCHEMY_STARS_CHARACTER_ALCHEMY_STARS_CHARACTER."
print("=== ZETA CHAR KEYS ===")
for k in sorted(k for k in data if k.startswith(prefix)):
    print(k, "=>", repr(data[k][:60]))

pat = re.compile(
    r"^(?P<pre>.+\.talk\.[^.]+)\.(?P<d>\d+)-(?P<l>\d+)(?P<r>r?)\.(?P<suf>ancient|char|next)$"
)
sets = defaultdict(lambda: {"lines": {}, "nexts": set(), "r": ""})
for k in data:
    m = pat.match(k)
    if not m:
        continue
    sid = f"{m['pre']}.{m['d']}|{m['r']}"
    sets[sid]["r"] = m["r"]
    if m["suf"] == "next":
        sets[sid]["nexts"].add(int(m["l"]))
    else:
        sets[sid]["lines"][int(m["l"])] = m["suf"]

bad = []
for sid, info in sets.items():
    if not info["lines"]:
        continue
    maxl = max(info["lines"])
    missing = [i for i in range(maxl) if i not in info["nexts"]]
    if maxl >= 1 and missing:
        bad.append((sid, sorted(info["lines"]), sorted(info["nexts"]), missing))

print("\n=== multi-line sets missing next:", len(bad), "===")
for sid, lines, nexts, missing in bad:
    print(sid)
    print("  lines", lines, "nexts", nexts, "missing", missing)
