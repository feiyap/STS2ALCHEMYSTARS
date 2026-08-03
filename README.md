# AlchemyStars

语言 / Languages：中文 | [English](README.en.md)

基于 [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 的 *Slay the Spire 2* 角色 Mod，新增可玩角色 **空裔**，围绕森 / 雷 / 水 / 火四属性的光能与转色栏机制构筑。

> 来自空谷的空之末裔，努力并元气地生活着，与伙伴一起踏上未知的旅途。

| 项 | 值 |
|---|---|
| Mod ID | `AlchemyStars` |
| 角色 | 空裔（`AlchemyStarsCharacter`） |
| 技术栈 | C# + Godot PCK + RitsuLib |
| 依赖 | `STS2-RitsuLib`（见 [`AlchemyStars.json`](AlchemyStars.json)） |

---

## 角色简介

| 项 | 内容 |
|---|---|
| 初始生命 / 金币 | 75 / 99 |
| 初始遗物 | **先天枷锁**（`AlchemyStarsLumenRelic`）：光能栏与转色栏上限各 4；战斗开始获得森雷水火各 1 点光能。可由 Orobas 精炼为 **自由和弦**（上限 8） |
| 初始牌组 | 射击 ×4、防御 ×4、临空者号 ×1、**薇丝** ×1、**卡莲** ×1 |
| 伙伴升华 | 古老牙齿可将薇丝 / 卡莲转化为先古形态（薇丝·空瞳 / 卡莲·煜魂）；本 Mod 会补全第二张的转化 |

开局在涅奥选完遗物后，会进入 **启迪者** 续页，四选一光能追踪方案，影响后续属性卡在奖励 / 商店中的出现方式。

---

## 机制介绍

战斗左侧展示光能栏与转色栏（由初始遗物启用）。

```text
打出卡牌 → 消耗光能点 → 转色栏生成属性格
                ↓
     同属性伤害加成 / 元素被动 / 虹光
```

### 光能点

分为 **森、雷、水、火** 四种。部分卡牌消耗光能点触发附加效果；光能不足时附加效果不生效。每消耗 1 点光能，会在转色栏生成对应属性的 **属性格**。另有 **万色** 光能，视为任意属性。

### 属性格

- 每个属性格提供该属性 **4%** 伤害加成。
- 每拥有 **4** 个同属性格，触发元素被动：
  - **火**：造成火属性伤害时施加灼烧
  - **水**：回合结束时恢复生命
  - **森**：回合结束时按手牌数获得格挡
  - **雷**：造成雷属性伤害时施加麻痹
- 特殊格：**棱镜格**（相邻格视为同属性）、**深色格**（计为 2 格）、**强化格**（供特定卡牌强化）

### 虹光

转色栏凑齐四种真实属性后触发（万色可按 1:1 补足缺口）：本回合内属性格为所有伤害提供更高加成；条件满足时增幅可翻倍。回合结束时对全体敌人造成伤害并重置属性格。

### 启迪者追踪方案

| 方案 | 效果 |
|---|---|
| A | 锁定一种或多种属性；商店与奖励中的属性卡仅出现所选属性 |
| B | 同上，但仅限制奖励（不影响商店） |
| C | 每拾取一种属性卡，该属性出现权重 +15% |
| D | 不做追踪，原初风味挑战 |

卡牌池按四属性展开，另有锁定、飞行、极光时刻等衍生关键词；完整说明见游戏内关键词与 [`AlchemyStars/localization/`](AlchemyStars/localization/) 本地化。

---

## 文件夹架构

```text
AlchemyStars/
├── AlchemyStarsCode/          # C# 游戏逻辑
│   ├── Cards/                 # 卡牌（按稀有度分子目录）
│   │   ├── Basic/             # 初始牌
│   │   ├── Common/            # 普通
│   │   ├── Uncommon/          # 罕见
│   │   ├── Rare/              # 稀有
│   │   ├── Ancients/          # 先古 / 升华
│   │   └── Generated/         # 战斗/事件衍生卡
│   ├── Characters/            # 角色与卡池 / 遗物池 / 药水池
│   ├── Mechanics/             # 光能与转色栏核心逻辑
│   ├── Powers/                # 能力
│   ├── Relics/                # 遗物（含 Enlightener/ 追踪方案）
│   ├── Patches/               # RitsuLib IPatchMethod 与辅助类
│   ├── Keywords/              # CardKeyword / CardTag 注册
│   ├── UI/                    # 光能栏 Godot UI
│   ├── Localization/          # 光能图标等 formatter
│   ├── Events/                # 启迪者事件模板（不进地图池）
│   └── Entry.cs               # Mod 入口
├── AlchemyStars/              # Godot PCK 资源（res://AlchemyStars）
│   ├── images/                # 卡图、遗物、角色 UI、光能图标等
│   ├── scenes/characters/     # 战斗模型、能量表盘、商店/火堆/选角背景
│   └── localization/          # zhs / eng JSON
├── AlchemyStars.csproj        # MSBuild（编译 + CopyMod + ExportPCK）
├── AlchemyStars.json          # Mod manifest
├── project.godot              # Godot 工程
├── export_presets.cfg         # PCK 导出预设
├── local.props.template       # 本机路径模板（复制为 local.props）
└── README.md / README.en.md
```

`res://AlchemyStars/...` 是 PCK 内资源路径，对应仓库里的 `AlchemyStars/` 目录，**不是** C# 命名空间。

---

## 代码架构

### 入口

[`AlchemyStarsCode/Entry.cs`](AlchemyStarsCode/Entry.cs) 由 `[ModInitializer]` 启动，依次：

1. `RitsuLibFramework.EnsureGodotScriptsRegistered` — 注册 Godot C# 脚本类型
2. `ModTypeDiscoveryHub.RegisterModAssembly` — 扫描 `[RegisterCard]` / `[RegisterRelic]` 等 Attribute 自动注册内容
3. `LightMechanicUiBootstrap.Register()` — 挂载战斗左侧光能 / 转色栏 UI
4. 创建 Patcher，注册并应用下方 3 个 Patch（关键 Patch 失败会走 `DisableMod` 降级）

新增内容类只要 Attribute 与命名约定正确，一般无需在入口手写注册列表。

### 分层

| 层 | 命名空间 / 目录 | 职责 |
|---|---|---|
| Characters | `AlchemyStars.Characters` | 角色模板、专属卡池 / 遗物池 / 药水池 |
| Mechanics | `AlchemyStars.Mechanics` | 光能状态、属性格队列、伤害倍率、战斗 Hook |
| Cards | `AlchemyStars.Cards` | 卡牌效果（子文件夹按 `CardRarity`，namespace 不拆分） |
| Powers | `AlchemyStars.Powers` | 能力 |
| Relics | `AlchemyStars.Relics` / `.Enlightener` | 初始光能遗物、追踪方案等 |
| Keywords | `AlchemyStars.Keywords` | 自定义 CardKeyword 与 CardTag |
| UI / Localization | `AlchemyStars.UI` / `.Localization` | 战斗 UI、描述中的光能图标格式化 |
| Patches | `AlchemyStars.Patches` | 对原版流程的注入（见下节） |

### 机制核心类型

| 类型 | 作用 |
|---|---|
| `LightMechanic` | 增删光能 / 属性格、转化、伤害相关静态 API |
| `LightMechanicCombatState` | 单场战斗的栏位状态 |
| `AlchemyStarsLightMechanicService` | `[RegisterSingleton]` 战斗 Hook（开战初始化、伤害修正、回合起止） |
| `LightMechanicUiBootstrap` | 战斗 UI 注册与刷新 |
| `AttributeCardTracking` | 启迪者方案对奖励 / 商店属性卡权重或锁定的支持 |

### 内容规模（约）

| 类别 | 规模 |
|---|---|
| 卡牌 | Basic 5 + Common ~34 + Uncommon ~50 + Rare ~25 + Ancients 3 + Generated ~15 |
| Powers | ~60 |
| Relics | 初始光能遗物（及升级）+ 启迪者方案 A–D 等 |

---

## Patch 方法

均在 `Entry.Initialize` 中通过 `RitsuLibFramework.CreatePatcher` 注册。实现 `IPatchMethod` 的类共 **3** 个。

| Patch | 文件 | 目标 | Critical | 作用 |
|---|---|---|---|---|
| `ArchaicToothTransformRemainingStartersPatch` | [`Patches/ArchaicToothTransformRemainingStartersPatch.cs`](AlchemyStarsCode/Patches/ArchaicToothTransformRemainingStartersPatch.cs) | `ArchaicTooth.AfterObtained`（Postfix） | 是 | 原版古老牙齿只转化第一张初始牌；此 Patch 在原始 Task 完成后，继续将牌组中剩余的薇丝 / 卡莲转化为先古形态，并保留升级与附魔 |
| `EnlightenerFollowUpDonePatch` | [`Patches/Enlightener/EnlightenerFollowUpDonePatch.cs`](AlchemyStarsCode/Patches/Enlightener/EnlightenerFollowUpDonePatch.cs) | `AncientEventModel.Done`（Prefix） | 是 | 空裔在涅奥（Neow）选完遗物后拦截 `Done`，注入启迪者四选一续页；异常时回退原版流程 |
| `EnlightenerRefreshVisualPatch` | [`Patches/Enlightener/EnlightenerRefreshVisualPatch.cs`](AlchemyStarsCode/Patches/Enlightener/EnlightenerRefreshVisualPatch.cs) | `NEventRoom.RefreshEventState(EventModel)`（Postfix） | 否 | 续页激活时，将事件房间标题刷新为「启迪者」 |

### Patch 辅助类（非 IPatchMethod）

| 类 | 作用 |
|---|---|
| `EnlightenerFollowUpState` | 按 Run 记录是否已对某玩家触发过启迪者续页 |
| `EnlightenerFollowUpVisualState` | 将 `AncientEventModel` 与续页视觉 entry 弱引用绑定 |
| `EnlightenerFollowUpVisuals` | 读取 `ancients` 本地化并设置 `NEventRoom` 标题 |

---

## 构建与本机配置

### 配置路径

```powershell
Copy-Item .\local.props.template .\local.props
```

在 `local.props`（已 gitignore）中设置：

| 字段 | 说明 |
|---|---|
| `Sts2Dir` | Slay the Spire 2 安装目录 |
| `Sts2DataDir` | 游戏 dll 目录（通常 `$(Sts2Dir)/data_sts2_windows_x86_64`） |
| `GodotExe` | 导出 PCK 用的 MegaDot / Godot 可执行文件 |
| `RitsuLibDeployDir` | 可选；RitsuLib 本机部署目录，默认 `$(Sts2Dir)/mods/STS2-RitsuLib` |

### 常用命令

| 命令 | 行为 |
|---|---|
| `dotnet build .\AlchemyStars.csproj` | 完整构建：编译 + `CopyMod` + `ExportPCK` |
| `... /p:RunPckExport=false` | 跳过 PCK 导出 |
| `... /p:CopyModOnBuild=false` | 不复制到游戏 `mods/` 目录 |
| `... /p:RunPckExport=false /p:CopyModOnBuild=false` | 仅验证 C# 编译 |

产物默认输出到 `$(Sts2Dir)/mods/AlchemyStars`（dll、manifest、pck）。

### 发布前：版本对齐

`AlchemyStars.json` 里 `dependencies[STS2-RitsuLib].version` 与 `.csproj` 中 `STS2.RitsuLib` 的 NuGet 版本**互相独立、不会自动同步**。发布前请确认二者一致，否则可能出现「manifest 放行但运行时崩溃」或「本可运行却被拒绝加载」。

---

## 学习资源

- [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) — 共享框架库
- [RitsuLib 文档](https://github.com/GlitchedReme/SlayTheSpire2ModdingTutorials/tree/master/RitsuLib)
- [Slay the Spire 2 Modding Tutorials](https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/index.html)
- 项目 Wiki：[中文](https://github.com/alkaid616/AlchemyStars/wiki/Home) | [English](https://github.com/alkaid616/AlchemyStars/wiki/Home-EN)
