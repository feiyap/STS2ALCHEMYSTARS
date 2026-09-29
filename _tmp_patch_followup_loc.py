# -*- coding: utf-8 -*-
import json
from pathlib import Path

root = Path(r"H:/STS2MOD/AlchemyStars/AlchemyStars/localization")

# relics flavors / descriptions
z_rel = json.loads((root / "zhs" / "relics.json").read_text(encoding="utf-8"))
z_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_WEIRD_CANDY.description"] = (
    "拾取时，选择 [blue]1[/blue] 张任意卡牌为其注能。战斗开始时失去{Energy:energyIcons()}。"
)
z_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_WEIRD_CANDY.flavor"] = (
    "人生就像糖果罐，你永远不知道下一颗糖是什么味道——伟大的勇者泽塔如是说。"
)
z_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_ODD_GAME_CARD.flavor"] = (
    "蕾切尔带给泽塔的礼物。\n「天呐！她怎么会送这么正常的礼物？」"
)
z_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_ZETA_CANDY_JAR.flavor"] = (
    "它到底是糖果罐还是机器人还是武器？"
)
z_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_STAR_CREST.flavor"] = (
    "故事结束了。\n黑暗过去之后，人们不再需要「星辰」来照亮前方。\n「愿你们能够自由地活下去」"
)
z_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LIGHT_AMBER.flavor"] = (
    "擅长「创造」的储君提供了一些建议。\n「你的记忆塑造了他人的印象，那些虚构也能成为助力。」"
)
(root / "zhs" / "relics.json").write_text(
    json.dumps(z_rel, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

e_rel = json.loads((root / "eng" / "relics.json").read_text(encoding="utf-8"))
e_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_WEIRD_CANDY.description"] = (
    "Upon pickup, choose [blue]1[/blue] card to Imbue. At the start of combat, lose {Energy:energyIcons()}."
)
e_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_WEIRD_CANDY.flavor"] = (
    "Life is like a candy jar—you never know what the next one tastes like. —Great Hero Zeta"
)
e_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_ODD_GAME_CARD.flavor"] = (
    "A gift from Rachel to Zeta.\n\"Wow! How did she give something so normal?\""
)
e_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_ZETA_CANDY_JAR.flavor"] = (
    "Is it a candy jar, a robot, or a weapon?"
)
e_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_STAR_CREST.flavor"] = (
    "The story ends.\nAfter the darkness passed, people no longer needed \"Stars\" to light the way.\n\"May you live freely.\""
)
e_rel["ALCHEMY_STARS_RELIC_ALCHEMY_STARS_LIGHT_AMBER.flavor"] = (
    'A Sovereign skilled in "Creation" offered some advice.\n'
    '"Your memories shape others\' impressions; those fictions can become strength."'
)
(root / "eng" / "relics.json").write_text(
    json.dumps(e_rel, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

# silent mausoleum
z_ev = json.loads((root / "zhs" / "events.json").read_text(encoding="utf-8"))
key = "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_SILENT_MAUSOLEUM.pages.INITIAL.description"
if key in z_ev:
    z_ev[key] = z_ev[key].replace("一个徽章", "[red]一个纹章[/red]")
(root / "zhs" / "events.json").write_text(
    json.dumps(z_ev, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

# powers for ignore status
for lang, title, desc in [
    (
        "zhs",
        "无视状态伤害",
        "本回合内，[gold]状态[/gold]牌对你造成的伤害无效。",
    ),
    (
        "eng",
        "Ignore Status Damage",
        "This turn, damage from [gold]Status[/gold] cards is negated.",
    ),
]:
    p = json.loads((root / lang / "powers.json").read_text(encoding="utf-8"))
    p["ALCHEMY_STARS_POWER_ALCHEMY_STARS_IGNORE_STATUS_DAMAGE_POWER.title"] = title
    p["ALCHEMY_STARS_POWER_ALCHEMY_STARS_IGNORE_STATUS_DAMAGE_POWER.description"] = desc
    (root / lang / "powers.json").write_text(
        json.dumps(p, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

# card loc snippets
z_cards = json.loads((root / "zhs" / "cards.json").read_text(encoding="utf-8"))
z_cards["ALCHEMY_STARS_CARD_ALCHEMY_STARS_FIRE_COMMON3.description"] = (
    "抽 {Cards:diff()} 张牌。将 [blue]1[/blue] 张[gold]灼烧[/gold]加入[gold]手牌[/gold]。\n本回合无视[gold]状态[/gold]牌伤害。"
)
z_cards["ALCHEMY_STARS_CARD_ALCHEMY_STARS_FIRE_COMMON3.smartDescription"] = z_cards[
    "ALCHEMY_STARS_CARD_ALCHEMY_STARS_FIRE_COMMON3.description"
]
z_cards["ALCHEMY_STARS_CARD_ALCHEMY_STARS_FIRE_COMMON1.description"] = (
    "获得{fireLightPrefix:lightIcons(2)}与 {Block:diff()} 点[gold]格挡[/gold]。\n引爆所有敌人的[gold]灼烧[/gold]（不消耗层数）。"
)
z_cards["ALCHEMY_STARS_CARD_ALCHEMY_STARS_FIRE_COMMON1.smartDescription"] = z_cards[
    "ALCHEMY_STARS_CARD_ALCHEMY_STARS_FIRE_COMMON1.description"
]
z_cards["ALCHEMY_STARS_CARD_ALCHEMY_STARS_FOREST_UNCOMMON9.description"] = (
    "消耗{forestLightPrefix:lightIcons(1)}：对敌人造成 {Damage:diff()} 点 {ForestTitle} 属性伤害。"
)
z_cards["ALCHEMY_STARS_CARD_ALCHEMY_STARS_FOREST_UNCOMMON9.smartDescription"] = z_cards[
    "ALCHEMY_STARS_CARD_ALCHEMY_STARS_FOREST_UNCOMMON9.description"
]
(root / "zhs" / "cards.json").write_text(
    json.dumps(z_cards, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
)

print("follow-up loc ok")
