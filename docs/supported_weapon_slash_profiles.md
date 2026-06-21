# Supported Weapon Slash Profiles

本文档记录当前已经通过 `SlashProfileResolver.ExactProfiles` 明确启用的武器刀光配置。

未列入本表的近战武器当前不会走专属刀光 Profile。`SlashProfiles.BalancedSword` 只是 fallback 数据，`SlashGlobalItem.ShouldUseSlashAction` 目前要求命中 exact profile 才启用重做刀光。`ItemID.ShadowFlameKnife` 被 `SlashGlobalItem` 排除，走独立的暗影焰刀系统，不属于本表的横斩刀光 Profile。

字段说明：

| 字段 | 含义 |
| --- | --- |
| 武器 | 当前显式支持的 `ItemID`，同一行表示这些武器共用同一个 Profile |
| 刀光类型 | `SlashProfileId` / `SlashProfiles` 中的 Profile 名称 |
| 长 / 宽 | `Shape.LengthScale` / `Shape.ThicknessScale`，会乘到实际武器贴图长度和刀光厚度上 |
| 挥动粒子 | `SwingParticles`，刀光活动期间沿轨迹发出 |
| 命中粒子 | `HitParticles`，命中 NPC 时发出 |

## 支持清单

| 武器 | 刀光类型 | 长 / 宽 | 挥动粒子 | 命中粒子 |
| --- | --- | ---: | --- | --- |
| `ItemID.WoodenSword`, `ItemID.BorealWoodSword`, `ItemID.PalmWoodSword`, `ItemID.EbonwoodSword`, `ItemID.ShadewoodSword`, `ItemID.RichMahoganySword`, `ItemID.PearlwoodSword`, `ItemID.AshWoodSword` | `WoodSword` 木剑 | 0.96 / 0.88 | `DustID.WoodFurniture`, 4, scale 0.55-0.88 | `DustID.WoodFurniture`, 10, scale 0.65-1.00 |
| `ItemID.CopperBroadsword`, `ItemID.TinBroadsword`, `ItemID.IronBroadsword`, `ItemID.LeadBroadsword` | `EarlyMetalSword` 早期金属剑 | 0.98 / 0.95 | `DustMetalSpark` 15, 5, scale 0.52-0.88 | `DustMetalSpark` 15, 12, scale 0.62-1.02 |
| `ItemID.SilverBroadsword`, `ItemID.TungstenBroadsword`, `ItemID.GoldBroadsword`, `ItemID.PlatinumBroadsword` | `NobleMetalSword` 贵金属剑 | 1.07 / 1.08 | `DustMetalSpark` 15, 7, scale 0.58-1.02 | `DustMetalSpark` 15, 18, scale 0.72-1.22 |
| `ItemID.CactusSword` | `CactusSword` 仙人掌剑 | 1.02 / 0.98 | `DustID.OasisCactus`, 5, scale 0.58-0.95 | `DustID.t_Cactus`, 14, scale 0.68-1.12 |
| `ItemID.BoneSword` | `BoneSword` 骨剑 | 1.00 / 0.95 | `DustID.Bone`, 6, scale 0.55-0.95 | `DustID.Bone`, 18, scale 0.65-1.15 |
| `ItemID.ZombieArm` | `ZombieArm` 僵尸臂 | 0.94 / 1.08 | `DustID.Blood`, 5, scale 0.58-0.95 | `DustID.Blood`, 18, scale 0.68-1.16 |
| `ItemID.CandyCaneSword` | `CandyCaneSword` 糖棒剑 | 1.02 / 0.90 | `DustID.Confetti_Pink`, 5, scale 0.52-0.86 | `DustID.Confetti`, 14, scale 0.66-1.10 |
| `ItemID.Katana` | `Katana` 武士刀 | 1.08 / 0.65 | `DustMetalSpark` 15, 5, scale 0.46-0.78 | `DustMetalSpark` 15, 12, scale 0.56-0.95 |
| `ItemID.FalconBlade` | `FalconBlade` 猎鹰刃 | 1.05 / 0.72 | `DustMetalSpark` 15, 5, scale 0.50-0.85 | `DustMetalSpark` 15, 13, scale 0.62-1.00 |
| `ItemID.LightsBane` | `LightsBane` 魔光剑 | 1.02 / 0.90 | `DustID.Demonite`, 6, scale 0.48-0.82 | `DustID.CursedTorch`, 16, scale 0.58-1.00 |
| `ItemID.BloodButcherer` | `BloodButcherer` 血腥屠刀 | 1.06 / 1.16 | `DustID.Crimson`, 7, scale 0.62-1.08 | `DustID.Blood`, 24, scale 0.70-1.25 |
| `ItemID.BeeKeeper` | `BeeKeeper` 养蜂人 | 0.98 / 0.95 | `DustID.Honey`, 5, scale 0.55-0.95 | `DustID.Bee`, 12, scale 0.65-1.10 |
| `ItemID.EnchantedSword` | `EnchantedSword` 附魔剑 | 1.04 / 0.78 | `DustID.Enchanted_Gold`, 5, scale 0.50-0.90 | `DustID.Enchanted_Gold`, 12, scale 0.62-1.05 |
| `ItemID.FieryGreatsword` | `Volcano` 火山 | 1.20 / 1.28 | `DustID.Torch`, 8, scale 1.10-1.65 | `DustID.Torch`, 34, scale 1.35-2.35 |
| `ItemID.NightsEdge` | `NightsEdge` 永夜刃 | 1.08 / 0.95 | `DustID.Shadowflame` / `DustID.Demonite`, 7, scale 0.55-0.92 | `DustID.Shadowflame` / `DustID.Demonite`, 20, scale 0.72-1.22 |
| `ItemID.TrueNightsEdge` | `TrueNightsEdge` 真永夜刃 | 1.15 / 1.05 | `DustID.Shadowflame` / `DustID.Demonite`, 9, scale 0.65-1.08 | `DustID.Shadowflame` / `DustID.Demonite`, 26, scale 0.82-1.35 |
| `ItemID.Excalibur` | `Excalibur` 断钢剑 | 1.08 / 1.18 | `DustMetalSpark` 15, 7, scale 0.52-0.88 | `DustMetalSpark` 15, 22, scale 0.62-1.08 |
| `ItemID.TrueExcalibur` | `TrueExcalibur` 真断钢剑 | 1.15 / 1.25 | `DustMetalSpark` 15, 9, scale 0.58-0.98 | `DustMetalSpark` 15, 28, scale 0.70-1.18 |
| `ItemID.BladeofGrass` | `BladeOfGrass` 草剑 | 1.08 / 1.10 | `DustID.Grass`, 7, scale 0.80-1.25 | `DustID.Grass`, 20, scale 0.95-1.70 |
| `ItemID.Muramasa` | `Muramasa` 村正 | 1.00 / 0.70 | `DustIceShard` 135, 6, scale 0.44-0.74 | `DustIceShard` 135, 14, scale 0.54-0.90 |
| `ItemID.IceBlade` | `IceBlade` 冰雪刃 | 1.00 / 0.80 | `DustIceShard` 135, 6, scale 0.48-0.82 | `DustIceShard` 135, 16, scale 0.58-0.98 |
| `ItemID.Frostbrand` | `Frostbrand` 寒霜剑 | 1.12 / 0.90 | `DustIceShard` 135, 8, scale 0.52-0.88 | `DustIceShard` 135, 22, scale 0.66-1.08 |
| `ItemID.Starfury` | `Starfury` 星怒 | 1.12 / 0.85 | `DustSoftStar` 278, 5, scale 0.58-0.96, `DrawnStar` | `DustSoftStar` 278, 14, scale 0.72-1.18, `DrawnStar` |
| `ItemID.Bladetongue` | `Bladetongue` 舌锋剑 | 1.05 / 1.05 | `DustFireEmber` 6, 7, scale 0.62-1.08 | `DustID.Blood`, 16, scale 0.62-1.08 |
| `ItemID.BluePhaseblade` | `BluePhaseblade` 蓝陨石光剑 | 1.05 / 0.78 | `DustID.GemSapphire`, 5, scale 0.50-0.86 | `DustID.GemSapphire`, 14, scale 0.64-1.08 |
| `ItemID.RedPhaseblade` | `RedPhaseblade` 红陨石光剑 | 1.05 / 0.78 | `DustID.GemRuby`, 5, scale 0.50-0.86 | `DustID.GemRuby`, 14, scale 0.64-1.08 |
| `ItemID.GreenPhaseblade` | `GreenPhaseblade` 绿陨石光剑 | 1.05 / 0.78 | `DustID.GemEmerald`, 5, scale 0.50-0.86 | `DustID.GemEmerald`, 14, scale 0.64-1.08 |
| `ItemID.PurplePhaseblade` | `PurplePhaseblade` 紫陨石光剑 | 1.05 / 0.78 | `DustID.GemAmethyst`, 5, scale 0.50-0.86 | `DustID.GemAmethyst`, 14, scale 0.64-1.08 |
| `ItemID.WhitePhaseblade` | `WhitePhaseblade` 白陨石光剑 | 1.05 / 0.78 | `DustID.GemDiamond`, 5, scale 0.50-0.86 | `DustID.GemDiamond`, 14, scale 0.64-1.08 |
| `ItemID.YellowPhaseblade` | `YellowPhaseblade` 黄陨石光剑 | 1.05 / 0.78 | `DustID.GemTopaz`, 5, scale 0.50-0.86 | `DustID.GemTopaz`, 14, scale 0.64-1.08 |
| `ItemID.OrangePhaseblade` | `OrangePhaseblade` 橙陨石光剑 | 1.05 / 0.78 | `DustID.GemAmber`, 5, scale 0.50-0.86 | `DustID.GemAmber`, 14, scale 0.64-1.08 |
| `ItemID.BluePhasesaber` | `BluePhasesaber` 蓝晶光刃 | 1.12 / 0.72 | `DustID.GemSapphire`, 6, scale 0.55-0.92 | `DustID.GemSapphire`, 16, scale 0.68-1.14 |
| `ItemID.RedPhasesaber` | `RedPhasesaber` 红晶光刃 | 1.12 / 0.72 | `DustID.GemRuby`, 6, scale 0.55-0.92 | `DustID.GemRuby`, 16, scale 0.68-1.14 |
| `ItemID.GreenPhasesaber` | `GreenPhasesaber` 绿晶光刃 | 1.12 / 0.72 | `DustID.GemEmerald`, 6, scale 0.55-0.92 | `DustID.GemEmerald`, 16, scale 0.68-1.14 |
| `ItemID.PurplePhasesaber` | `PurplePhasesaber` 紫晶光刃 | 1.12 / 0.72 | `DustID.GemAmethyst`, 6, scale 0.55-0.92 | `DustID.GemAmethyst`, 16, scale 0.68-1.14 |
| `ItemID.WhitePhasesaber` | `WhitePhasesaber` 白晶光刃 | 1.12 / 0.72 | `DustID.GemDiamond`, 6, scale 0.55-0.92 | `DustID.GemDiamond`, 16, scale 0.68-1.14 |
| `ItemID.YellowPhasesaber` | `YellowPhasesaber` 黄晶光刃 | 1.12 / 0.72 | `DustID.GemTopaz`, 6, scale 0.55-0.92 | `DustID.GemTopaz`, 16, scale 0.68-1.14 |
| `ItemID.OrangePhasesaber` | `OrangePhasesaber` 橙晶光刃 | 1.12 / 0.72 | `DustID.GemAmber`, 6, scale 0.55-0.92 | `DustID.GemAmber`, 16, scale 0.68-1.14 |
| `ItemID.CobaltSword` | `CobaltSword` 钴剑 | 1.04 / 0.95 | `DustMetalSpark` 15, 7, scale 0.56-0.96 | `DustMetalSpark` 15, 18, scale 0.68-1.16 |
| `ItemID.PalladiumSword` | `PalladiumSword` 钯金剑 | 1.05 / 1.02 | `DustMetalSpark` 15, 7, scale 0.56-0.96 | `DustMetalSpark` 15, 18, scale 0.68-1.16 |
| `ItemID.MythrilSword` | `MythrilSword` 秘银剑 | 1.06 / 1.00 | `DustMetalSpark` 15, 7, scale 0.56-0.96 | `DustMetalSpark` 15, 18, scale 0.68-1.16 |
| `ItemID.OrichalcumSword` | `OrichalcumSword` 山铜剑 | 1.08 / 1.05 | `DustMetalSpark` 15, 7, scale 0.56-0.96 | `DustMetalSpark` 15, 18, scale 0.68-1.16 |
| `ItemID.AdamantiteSword` | `AdamantiteSword` 精金剑 | 1.10 / 1.12 | `DustMetalSpark` 15, 7, scale 0.56-0.96 | `DustMetalSpark` 15, 18, scale 0.68-1.16 |
| `ItemID.TitaniumSword` | `TitaniumSword` 钛金剑 | 1.10 / 1.08 | `DustMetalSpark` 15, 7, scale 0.56-0.96 | `DustMetalSpark` 15, 18, scale 0.68-1.16 |
| `ItemID.ChlorophyteSaber` | `ChlorophyteSaber` 叶绿军刀 | 0.98 / 0.75 | `DustID.ChlorophyteWeapon`, 6, scale 0.50-0.88 | `DustID.ChlorophyteWeapon`, 14, scale 0.62-1.05 |
| `ItemID.ChlorophyteClaymore` | `ChlorophyteClaymore` 叶绿双刃刀 | 1.16 / 1.20 | `DustID.ChlorophyteWeapon`, 8, scale 0.60-1.05 | `DustID.ChlorophyteWeapon`, 22, scale 0.75-1.25 |
| `ItemID.BreakerBlade` | `BreakerBlade` 毁灭刃 | 1.22 / 1.60 | `DustID.Smoke`, 8, scale 0.90-1.35 | `DustID.Blood`, 30, scale 0.90-1.45 |
| `ItemID.Cutlass` | `Cutlass` 短弯刀 | 1.00 / 0.75 | `DustMetalSpark` 15, 5, scale 0.42-0.72 | `DustMetalSpark` 15, 12, scale 0.50-0.85 |
| `ItemID.Keybrand` | `Keybrand` 钥匙剑 | 1.06 / 1.10 | `DustMetalSpark` 15, 6, scale 0.50-0.85 | `DustID.Gold`, 20, scale 0.62-1.05 |
| `ItemID.BeamSword` | `BeamSword` 光束剑 | 1.10 / 0.82 | `DustID.Enchanted_Gold`, 6, scale 0.50-0.86 | `DustSoftStar` 278, 16, scale 0.62-1.08 |
| `ItemID.TerraBlade` | `TerraBlade` 泰拉刃 | 1.20 / 1.05 | `DustID.ChlorophyteWeapon`, 9, scale 0.58-0.98 | `DustID.ChlorophyteWeapon`, 24, scale 0.72-1.20 |
| `ItemID.TheHorsemansBlade` | `TheHorsemansBlade` 无头骑士剑 | 1.12 / 1.15 | `DustID.GoldFlame`, 7, scale 0.62-1.05 | `DustID.Torch`, 18, scale 0.80-1.35 |
| `ItemID.ChristmasTreeSword` | `ChristmasTreeSword` 圣诞树剑 | 1.15 / 1.05 | `DustID.Confetti_Green`, 7, scale 0.55-0.95 | `DustID.FireworksRGB`, 22, scale 0.70-1.25 |
| `ItemID.Seedler` | `Seedler` 种子弯刀 | 1.08 / 0.98 | `DustGrassLeaf` 107, 7, scale 0.58-0.98 | `DustID.ChlorophyteWeapon`, 18, scale 0.70-1.15 |
| `ItemID.InfluxWaver` | `InfluxWaver` 波涌之刃 | 1.12 / 0.90 | `DustID.Electric`, 7, scale 0.50-0.84 | `DustID.MartianHit`, 18, scale 0.62-1.08 |
| `ItemID.StarWrath` | `StarWrath` 狂星之怒 | 1.20 / 0.85 | `DustSoftStar` 278, 6, scale 0.55-0.92, `DrawnStar` | `DustSoftStar` 278, 16, scale 0.68-1.12, `DrawnStar` |
| `ItemID.Meowmere` | `Meowmere` 彩虹猫之刃 | 1.12 / 0.90 | `DustID.RainbowMk2`, 7, scale 0.52-0.90 | `DustID.FireworksRGB`, 18, scale 0.62-1.15 |
| `ItemID.PsychoKnife` | `PsychoKnife` 变态人的刀 | 0.78 / 0.55 | `DustID.Shadowflame`, 4, scale 0.42-0.68 | `DustID.Shadowflame`, 10, scale 0.52-0.92 |
| `ItemID.DD2SquireDemonSword` | `BrandOfTheInferno` 地狱之剑 | 1.18 / 1.25 | `DustFireEmber` 6, 8, scale 0.70-1.15 | `DustID.Torch`, 22, scale 0.85-1.45 |
| `ItemID.DD2SquireBetsySword` | `FlyingDragon` 飞龙 | 1.22 / 1.15 | `DustID.GoldFlame`, 9, scale 0.68-1.12 | `DustID.Torch`, 24, scale 0.85-1.45 |

## 额外说明

- 武器中文名按 terraria.wiki.gg/zh 的中文条目或 `{{tr|...}}` 翻译结果记录。
- 表中 `DustMetalSpark`、`DustSoftStar`、`DustGrassLeaf`、`DustFireEmber`、`DustIceShard` 是 `SlashProfiles.cs` 内部常量，分别对应 dust id `15`、`278`、`107`、`6`、`135`。
- `DrawnStar` 表示粒子不是普通 dust 贴图，而是通过 `StarSlashSparkleProjectile` 绘制星形闪光。
- 部分 Profile 配有 `AlternateDustColor` 或 `AlternateDustType`，表格只列主 dust 和主要备选 dust，颜色细节以 `SlashProfiles.cs` 为准。
- 当前实际长度还会乘以武器贴图对角线长度、连招段 `SlashComboStep.LengthScale`，宽度还会乘以连招段 `ThicknessScale` 和配置项 `SlashScale`。
