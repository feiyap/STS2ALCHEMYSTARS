# AlchemyStars

Languages: [中文](README.md) | English

A *Slay the Spire 2* character mod built on [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib). It adds the playable character **Void Descendant** (空裔) and a Forest / Thunder / Water / Fire light-energy and conversion-bar system.

> A void-born descendant from the Hollow Valley, living earnestly and cheerfully as they embark on an unknown journey alongside their companions.

| Item | Value |
|---|---|
| Mod ID | `AlchemyStars` |
| Character | Void Descendant (`AlchemyStarsCharacter`) |
| Stack | C# + Godot PCK + RitsuLib |
| Dependency | `STS2-RitsuLib` (see [`AlchemyStars.json`](AlchemyStars.json)) |

---

## Character

| Item | Details |
|---|---|
| Starting HP / Gold | 75 / 99 |
| Starter relic | **Innate Shackles** (`AlchemyStarsLumenRelic`): light-energy and conversion bars capped at 4 each; combat start grants 1 Forest / Thunder / Water / Fire light energy. Orobas can refine it into **Chord of Freedom** (cap 8). |
| Starter deck | Shoot ×4, Defense ×4, Sky Carrier ×1, **Vice** ×1, **Karen** ×1 |
| Partner transcendence | Archaic Tooth can transform Vice / Karen into Ancient forms; this mod also transforms the remaining second starter |

After picking a Neow relic, Void Descendant enters **The Enlightener** follow-up page and chooses one of four light-tracking plans that shape which attribute cards appear in rewards / the shop.

---

## Mechanics

Combat shows a light-energy bar and conversion bar on the left (enabled by the starter relic).

```text
Play cards → spend light energy → create attribute cells on the conversion bar
                ↓
     same-attribute damage / element passives / Rainbow Light
```

### Light energy

Four types: **Forest, Thunder, Water, Fire**. Some cards spend light energy for bonus effects; if you lack enough, the bonus does not apply. Each spent point creates a matching **attribute cell** on the conversion bar. **Prismatic** light counts as any attribute.

### Attribute cells

- Each cell grants **+4%** damage of that attribute.
- Every **4** cells of one attribute trigger a passive:
  - **Fire**: apply Burn when dealing Fire damage
  - **Water**: heal at end of turn
  - **Forest**: gain Block based on hand size at end of turn
  - **Thunder**: apply Paralysis when dealing Thunder damage
- Special cells: **Prism** (adjacent cells count as the same attribute), **Dark** (counts as 2), **Enhanced** (powers specific cards)

### Rainbow Light

Triggers when the conversion bar has all four real attributes (Prismatic can fill gaps 1:1): cells grant a stronger all-damage bonus this turn (can double under stricter conditions). At end of turn, deal AoE damage and reset the cells.

### Enlightener tracking plans

| Plan | Effect |
|---|---|
| A | Lock one or more attributes; shop and reward attribute cards are limited to those attributes |
| B | Same as A, but only rewards (shop unaffected) |
| C | Each time you pick up an attribute card, that attribute’s weight +15% |
| D | No tracking; original-flavor challenge |

The card pool is organized around the four attributes, plus keywords such as Lock, Flying, and Aurora Moment. Full text lives in-game and under [`AlchemyStars/localization/`](AlchemyStars/localization/).

---

## Folder layout

```text
AlchemyStars/
├── AlchemyStarsCode/          # C# game logic
│   ├── Cards/                 # Cards (subfolders by rarity)
│   │   ├── Basic/             # Starters
│   │   ├── Common/
│   │   ├── Uncommon/
│   │   ├── Rare/
│   │   ├── Ancients/          # Ancient / transcendence cards
│   │   └── Generated/         # Runtime / event-generated cards
│   ├── Characters/            # Character + card / relic / potion pools
│   ├── Mechanics/             # Light-energy & conversion-bar core
│   ├── Powers/
│   ├── Relics/                # Includes Enlightener/ tracking plans
│   ├── Patches/               # RitsuLib IPatchMethod + helpers
│   ├── Keywords/              # CardKeyword / CardTag registration
│   ├── UI/                    # Light-bar Godot UI
│   ├── Localization/          # Light-icon formatters, etc.
│   ├── Events/                # Enlightener event template (not in map pool)
│   └── Entry.cs               # Mod entry
├── AlchemyStars/              # Godot PCK assets (res://AlchemyStars)
│   ├── images/
│   ├── scenes/characters/
│   └── localization/          # zhs / eng JSON
├── AlchemyStars.csproj
├── AlchemyStars.json          # Mod manifest
├── project.godot
├── export_presets.cfg
├── local.props.template
└── README.md / README.en.md
```

`res://AlchemyStars/...` is the in-PCK asset path for the repo’s `AlchemyStars/` folder — **not** a C# namespace.

---

## Code architecture

### Entry

[`AlchemyStarsCode/Entry.cs`](AlchemyStarsCode/Entry.cs) runs via `[ModInitializer]` and:

1. `RitsuLibFramework.EnsureGodotScriptsRegistered` — register Godot C# script types
2. `ModTypeDiscoveryHub.RegisterModAssembly` — scan `[RegisterCard]` / `[RegisterRelic]` etc. for auto-registration
3. `LightMechanicUiBootstrap.Register()` — mount combat light / conversion UI
4. Create a patcher, register and apply the three patches below (critical patch failure triggers `DisableMod`)

New content classes usually need correct attributes only; no hand-written registry in the entry point.

### Layers

| Layer | Namespace / folder | Role |
|---|---|---|
| Characters | `AlchemyStars.Characters` | Character template and pools |
| Mechanics | `AlchemyStars.Mechanics` | Light state, cell queues, damage, combat hooks |
| Cards | `AlchemyStars.Cards` | Card effects (folders by `CardRarity`; shared namespace) |
| Powers | `AlchemyStars.Powers` | Powers |
| Relics | `AlchemyStars.Relics` / `.Enlightener` | Starter lumen relic, tracking plans |
| Keywords | `AlchemyStars.Keywords` | Custom CardKeywords and CardTags |
| UI / Localization | `AlchemyStars.UI` / `.Localization` | Combat UI, light icons in descriptions |
| Patches | `AlchemyStars.Patches` | Vanilla flow injection (below) |

### Core mechanic types

| Type | Role |
|---|---|
| `LightMechanic` | Static API for light / cells / conversion / damage |
| `LightMechanicCombatState` | Per-combat bar state |
| `AlchemyStarsLightMechanicService` | `[RegisterSingleton]` combat hooks |
| `LightMechanicUiBootstrap` | Combat UI register / refresh |
| `AttributeCardTracking` | Reward / shop attribute filtering & weights for Enlightener plans |

### Content scale (approx.)

| Category | Scale |
|---|---|
| Cards | Basic 5 + Common ~34 + Uncommon ~50 + Rare ~25 + Ancients 3 + Generated ~15 |
| Powers | ~60 |
| Relics | Starter lumen (and upgrade) + Enlightener plans A–D, etc. |

---

## Patches

Registered in `Entry.Initialize` via `RitsuLibFramework.CreatePatcher`. There are **3** `IPatchMethod` classes.

| Patch | File | Target | Critical | Purpose |
|---|---|---|---|---|
| `ArchaicToothTransformRemainingStartersPatch` | [`Patches/ArchaicToothTransformRemainingStartersPatch.cs`](AlchemyStarsCode/Patches/ArchaicToothTransformRemainingStartersPatch.cs) | `ArchaicTooth.AfterObtained` (Postfix) | Yes | Vanilla Archaic Tooth transforms only the first starter; after the original Task, also transform remaining Vice / Karen into Ancient forms, keeping upgrades and enchantments |
| `EnlightenerFollowUpDonePatch` | [`Patches/Enlightener/EnlightenerFollowUpDonePatch.cs`](AlchemyStarsCode/Patches/Enlightener/EnlightenerFollowUpDonePatch.cs) | `AncientEventModel.Done` (Prefix) | Yes | After Void Descendant finishes Neow relic choice, intercept `Done` and inject The Enlightener four-option page; on failure, fall back to vanilla |
| `EnlightenerRefreshVisualPatch` | [`Patches/Enlightener/EnlightenerRefreshVisualPatch.cs`](AlchemyStarsCode/Patches/Enlightener/EnlightenerRefreshVisualPatch.cs) | `NEventRoom.RefreshEventState(EventModel)` (Postfix) | No | While the follow-up is active, refresh the event room title to “The Enlightener” |

### Patch helpers (not IPatchMethod)

| Class | Role |
|---|---|
| `EnlightenerFollowUpState` | Per-run flag: whether Enlightener follow-up already triggered for a player |
| `EnlightenerFollowUpVisualState` | Weak-maps `AncientEventModel` to visual entry |
| `EnlightenerFollowUpVisuals` | Loads `ancients` localization and sets `NEventRoom` title |

---

## Build & local setup

### Paths

```powershell
Copy-Item .\local.props.template .\local.props
```

Set in `local.props` (gitignored):

| Field | Meaning |
|---|---|
| `Sts2Dir` | Slay the Spire 2 install directory |
| `Sts2DataDir` | Game DLL directory (usually `$(Sts2Dir)/data_sts2_windows_x86_64`) |
| `GodotExe` | MegaDot / Godot executable for PCK export |
| `RitsuLibDeployDir` | Optional; RitsuLib deploy dir, default `$(Sts2Dir)/mods/STS2-RitsuLib` |

### Common commands

| Command | Behavior |
|---|---|
| `dotnet build .\AlchemyStars.csproj` | Full build: compile + `CopyMod` + `ExportPCK` |
| `... /p:RunPckExport=false` | Skip PCK export |
| `... /p:CopyModOnBuild=false` | Do not copy into the game `mods/` folder |
| `... /p:RunPckExport=false /p:CopyModOnBuild=false` | C# compile only |

Default output: `$(Sts2Dir)/mods/AlchemyStars` (dll, manifest, pck).

### Before release: version alignment

`dependencies[STS2-RitsuLib].version` in `AlchemyStars.json` and the `STS2.RitsuLib` NuGet version in `.csproj` are **independent and not auto-synced**. Align them before publishing, or players may pass the manifest check and crash at runtime—or be rejected despite a working build.

---

## Learning resources

- [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) — shared framework
- [RitsuLib docs](https://github.com/GlitchedReme/SlayTheSpire2ModdingTutorials/tree/master/RitsuLib)
- [Slay the Spire 2 Modding Tutorials](https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/index.html)
- Project Wiki: [Chinese](https://github.com/alkaid616/AlchemyStars/wiki/Home) | [English](https://github.com/alkaid616/AlchemyStars/wiki/Home-EN)
