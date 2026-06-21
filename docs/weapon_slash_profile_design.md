# Weapon Slash Profile Design

## 1. 目标

武器特色系统的当前目标，是用较低工程风险让不同近战武器在视觉上更容易被区分。经过火山武器验证后，当前阶段只保留三项稳定能力：

- 挥舞粒子：刀光移动过程中沿刀光轨迹生成粒子。
- 命中粒子：刀光击中 NPC 时生成武器主题粒子。
- 刀光长宽：按武器 profile 调整刀光长度和宽度。

当前阶段不做刀光颜色、YScale、随机角度、extraUpdates、专属纹理、Shader 或每把武器专属连招动作。这些内容统一放到“未来计划”。

## 2. 当前实现模型

系统分为三层：

| 层级 | 负责内容 | 当前状态 |
| --- | --- | --- |
| 武器 profile | 每把武器的挥舞粒子、命中粒子、长度倍率、宽度倍率 | 已实现 |
| 连招 profile | 连招段数、段落角度、段落长度/宽度基础倍率、伪 3D 视觉 | 已存在，继续作为动作层 |
| projectile 运行时 | 读取最终参数，生成刀光、粒子和命中反馈 | 已接入三项能力 |

核心原则：

- 武器 profile 只描述武器主题，不定义连招段数、段落角度或动作时序。
- 连招 profile 只描述动作，不写具体武器的主题规则。
- 当前只对 exact profile 武器启用差异化效果，fallback 武器暂不批量启用，避免一次影响太多武器。

## 3. 当前代码入口

| 文件 | 作用 |
| --- | --- |
| `Core/Profiles/SlashProfiles.cs` | 定义具体武器 profile 数据 |
| `Core/Profiles/SlashProfileResolver.cs` | 按 `ItemID` 查 exact profile |
| `Core/Profiles/SlashParticleEmitter.cs` | 根据 profile 发射挥舞/命中粒子 |
| `Content/Projectiles/SlashChannelProjectile.cs` | 发起普通挥砍，并叠加 profile 的长度/宽度 |
| `Content/Projectiles/SlashArcProjectile.cs` | 在真实刀光 projectile 上生成挥舞粒子，并在命中时生成命中粒子 |

当前火山映射使用 `ItemID.FieryGreatsword`。在当前 tModLoader 版本中，游戏里的 Volcano 仍对应这个旧常量名。

## 4. 已验证效果

### 4.1 挥舞粒子

挥舞粒子必须从 `SlashArcProjectile` 发射，而不是从 `SlashChannelProjectile` 发射。

原因：

- `SlashChannelProjectile` 只知道玩家瞄准方向和准备发出哪一刀。
- 真正的刀光轨迹由 `SlashArcProjectile` 的 `Projectile.rotation`、`Projectile.oldRot`、`Projectile.ai[1]`、`localAI[1]` 和 profile 视觉变换共同决定。
- 如果在 channel 阶段提前撒粒子，粒子会像独立火星团，不会贴住刀光。

当前实现方式：

- 在 `SlashArcProjectile.AI()` 的刀光活动期内调用 `EmitExactProfileSwingParticles()`。
- 粒子位置使用和刀光一致的 owner center、rotation history、YScale/profile transform。
- 只有 `SlashProfileResolver.TryGetExactProfile(...)` 命中的武器会发出主题挥舞粒子。

### 4.2 命中粒子

命中粒子在 `SlashArcProjectile.OnHitNPC(...)` 中生成。

当前实现方式：

- 通过 `_weaponItemType` 查 exact profile。
- 命中粒子从 `target.Center` 附近喷出。
- 粒子参数来自 `profile.HitParticles`。
- 命中粒子只改变视觉，不改变伤害、击退、碰撞或原版武器弹幕。

### 4.3 刀光长度和宽度

刀光长宽在 `SlashChannelProjectile` 发起挥砍时叠加。

当前组合方式：

```text
finalLength = baseLength * comboStep.LengthScale * weaponProfile.Shape.LengthScale

finalThickness =
    config.SlashScale
    * comboStep.ThicknessScale
    * weaponProfile.Shape.ThicknessScale
```

旧随机路径也支持 `thickness` 参数。`SlashArcProjectile.CreateSlash(...)` 已修正为让传入的 `thickness` 参与 `SlashScale` 计算。

## 5. Profile 数据含义

当前 profile 结构里有些字段暂时不作为当前阶段能力使用。当前有效字段如下：

| 字段 | 当前是否使用 | 说明 |
| --- | --- | --- |
| `SwingParticles.DustType` | 是 | 挥舞粒子类型 |
| `SwingParticles.DustColor` | 是 | 挥舞粒子颜色 |
| `SwingParticles.Count` | 是 | 挥舞粒子基准数量 |
| `SwingParticles.MinScale/MaxScale` | 是 | 挥舞粒子大小范围 |
| `SwingParticles.VelocityScale` | 是 | 挥舞粒子速度倍率 |
| `SwingParticles.SpreadRadians` | 是 | 挥舞粒子扩散角 |
| `HitParticles.*` | 是 | 命中粒子同类参数 |
| `Shape.LengthScale` | 是 | 刀光长度倍率 |
| `Shape.ThicknessScale` | 是 | 刀光宽度倍率 |
| `SlashColor` | 否 | 未来计划 |
| `Shape.MinYScale/MaxYScale` | 否 | 未来计划 |
| `Shape.AngleRandomness` | 否 | 未来计划 |
| `Shape.ExtraUpdates` | 否 | 未来计划 |

## 6. 火山 Profile 作为样板

当前火山 profile 代表“火焰重剑”风格：

| 项 | 当前值 | 设计意图 |
| --- | --- | --- |
| `SwingParticles.DustType` | `DustID.Torch` | 火焰余烬 |
| `SwingParticles.Count` | `8` | 挥舞时有火星，但不遮挡刀光 |
| `HitParticles.Count` | `34` | 命中爆发更明显 |
| `HitParticles.MinScale/MaxScale` | `1.35-2.35` | 命中火星更大 |
| `HitParticles.VelocityScale` | `1.75` | 命中喷发更有冲击 |
| `Shape.LengthScale` | `1.20` | 火山比标准剑更长 |
| `Shape.ThicknessScale` | `1.28` | 火山刀光更厚、更重 |

火山的经验结论：

- 挥舞粒子要贴刀光，必须从真实刀光 projectile 里发。
- 命中粒子可以更夸张，因为它是短时反馈。
- 长度和宽度适合先用 profile 倍率做，不需要改刀光纹理。

## 7. 武器类别建议

当前只建议为每类武器设计三项参数：挥舞粒子、命中粒子、刀光长宽。

| 类别 | 适用对象 | 挥舞粒子 | 命中粒子 | 长宽建议 |
| --- | --- | --- | --- | --- |
| 基础金属剑 | 铜、铁、银、金等 | 少量黄白火星 | 小型金属火星爆点 | 长度 1.0，宽度 1.0 |
| 短剑/细剑 | 短剑、刺剑、小型高速剑 | 少量细碎火星，集中在刀尖 | 少量快速火星 | 长度 0.75-0.95，宽度 0.55-0.8 |
| 标准长剑 | 大多数普通剑 | 中等金属火星 | 中等火星爆点 | 长度 1.0，宽度 1.0 |
| 重剑/大剑 | Breaker Blade、火山等 | 大颗粒火星或碎屑 | 明显爆点 | 长度 1.1-1.25，宽度 1.2-1.6 |
| 火焰类 | 火山、火焰主题武器 | 橙红余烬 | 大量火焰爆点 | 长度 1.1-1.25，宽度 1.25-1.6 |
| 冰霜类 | Ice Blade、Frostbrand | 浅蓝碎屑 | 蓝白碎裂感 | 长度 1.0-1.15，宽度 0.75-0.95 |
| 暗影类 | Night's Edge 系 | 紫黑火星 | 深色爆点 | 长度 1.05-1.2，宽度 0.85-1.05 |
| 血色/猩红类 | Blood Butcherer、Bladetongue | 暗红粒子 | 红色喷射 | 长度 1.0-1.1，宽度 1.0-1.2 |
| 神圣/圣光类 | Excalibur 系 | 金白光点 | 亮色爆点 | 长度 1.05-1.2，宽度 1.1-1.3 |
| 星辰类 | Starfury、Star Wrath | 无贴图绘制星芒 | 亮色星芒簇爆闪 | 长度 1.1-1.25，宽度 0.8-1.0 |

## 8. 当前已适配武器

本轮以后，`SlashProfileResolver.ExactProfiles` 已经覆盖主要横斩刀剑。低身份武器使用共享族 profile，强主题武器使用单独 profile。共享 profile 仍然需要逐把 `ItemID` 映射，不能依赖 fallback。

| 武器或武器组 | Exact 映射 | 挥舞粒子 | 命中粒子 | 长宽 |
| --- | --- | --- | --- | --- |
| 木剑族 | Wooden/Boreal/Palm/Ebon/Shade/Rich Mahogany/Pearl/Ash Wood Sword | 原生木屑 `DustID.WoodFurniture`，棕/浅木色随机 | 木屑爆点 | 0.96 / 0.88 |
| 早期金属剑族 | Copper/Tin/Iron/Lead Broadsword | `DustMetalSpark = 15`，铜黄/冷白随机 | 小型金属火星 | 0.98 / 0.95 |
| 贵金属剑族 | Silver/Tungsten/Gold/Platinum Broadsword | `DustMetalSpark = 15`，金白/银白随机 | 更亮金属火星 | 1.07 / 1.08 |
| Cactus Sword | `ItemID.CactusSword` | 仙人掌/沙绿粒子 | 仙人掌碎屑 | 1.02 / 0.98 |
| Bone Sword | `ItemID.BoneSword` | 骨屑 `DustID.Bone` | 骨屑爆点 | 1.00 / 0.95 |
| Zombie Arm | `ItemID.ZombieArm` | 血色/腐绿色粒子 | 血色命中点 | 0.94 / 1.08 |
| Candy Cane Sword | `ItemID.CandyCaneSword` | 红白纸屑 | 红白纸屑爆点 | 1.02 / 0.90 |
| Katana | `ItemID.Katana` | 细金属火星 | 快速金属爆点 | 1.08 / 0.65 |
| Falcon Blade | `ItemID.FalconBlade` | 银白/金色轻快火星 | 金属爆点 | 1.05 / 0.72 |
| Light's Bane | `ItemID.LightsBane` | 恶魔矿紫尘/诅咒绿 | 紫绿命中爆点 | 1.02 / 0.90 |
| Blood Butcherer | `ItemID.BloodButcherer` | 猩红粒子 | 血色命中爆点 | 1.06 / 1.16 |
| Bee Keeper | `ItemID.BeeKeeper` | 蜂蜜/深棕粒子 | 蜂群/蜂蜜命中点 | 0.98 / 0.95 |
| Enchanted Sword | `ItemID.EnchantedSword` | 金粉/粉光 | 金粉爆点 | 1.04 / 0.78 |
| Volcano / Fiery Greatsword | `ItemID.FieryGreatsword` | 橙红余烬 | 大型火焰爆点 | 1.20 / 1.28 |
| Night's Edge | `ItemID.NightsEdge` | 暗紫 `Shadowflame` / `Demonite` 随机 dust | 紫黑爆点，避免白色带状星尘 | 1.08 / 0.95 |
| True Night's Edge | `ItemID.TrueNightsEdge` | 亮紫 `Shadowflame` / `Demonite` 随机 dust | 强紫/深紫爆点，避免白色带状星尘 | 1.15 / 1.05 |
| Excalibur | `ItemID.Excalibur` | 金白金属尘 | 金白/亮金爆点 | 1.08 / 1.18 |
| True Excalibur | `ItemID.TrueExcalibur` | 金白/亮白金属尘 | 明亮金白爆点 | 1.15 / 1.25 |
| Blade of Grass | `ItemID.BladeofGrass` | 更大、更亮的 `DustID.Grass` 草尘 | `DustID.Grass` 草绿色命中碎屑 | 1.08 / 1.10 |
| Muramasa | `ItemID.Muramasa` | 蓝/青蓝冰尘 | 小型蓝白爆点 | 1.00 / 0.70 |
| Ice Blade | `ItemID.IceBlade` | 浅蓝/冰白冰尘 | 冰晶爆点 | 1.00 / 0.80 |
| Frostbrand | `ItemID.Frostbrand` | 亮蓝/冰白冰尘 | 更强冰裂爆点 | 1.12 / 0.90 |
| Starfury | `ItemID.Starfury` | 粉/金绘制星芒，不使用星形贴图 | 粉金星芒簇爆点 | 1.12 / 0.85 |
| Bladetongue | `ItemID.Bladetongue` | 红黄火尘 | 红黄命中喷射 | 1.05 / 1.05 |
| Phaseblade 族 | Blue/Red/Green/Purple/White/Yellow/Orange Phaseblade | 对应宝石尘 | 同色宝石爆点 | 1.05 / 0.78 |
| Phasesaber 族 | Blue/Red/Green/Purple/White/Yellow/Orange Phasesaber | 更强同色宝石尘 | 同色宝石爆点 | 1.12 / 0.72 |
| 困难矿物剑族 | Cobalt/Palladium/Mythril/Orichalcum/Adamantite/Titanium Sword | 同主题色金属火星 | 同主题色金属爆点 | 1.04-1.10 / 0.95-1.12 |
| Chlorophyte Saber | `ItemID.ChlorophyteSaber` | 叶绿武器尘 | 叶绿命中点 | 0.98 / 0.75 |
| Chlorophyte Claymore | `ItemID.ChlorophyteClaymore` | 更重叶绿武器尘 | 叶绿爆点 | 1.16 / 1.20 |
| Breaker Blade | `ItemID.BreakerBlade` | 灰烟/暗红 | 血色重击爆点 | 1.22 / 1.60 |
| Cutlass | `ItemID.Cutlass` | 银白/金色金属火星 | 快速金属爆点 | 1.00 / 0.75 |
| Keybrand | `ItemID.Keybrand` | 金色金属火星 | 金色钥匙光爆点 | 1.06 / 1.10 |
| Beam Sword | `ItemID.BeamSword` | 青蓝/金色光尘 | 星光命中点 | 1.10 / 0.82 |
| Terra Blade | `ItemID.TerraBlade` | 叶绿/浅绿光尘 | 叶绿爆点 | 1.20 / 1.05 |
| The Horseman's Blade | `ItemID.TheHorsemansBlade` | 橙色火焰粒子 | 南瓜月火焰爆点 | 1.12 / 1.15 |
| Christmas Tree Sword | `ItemID.ChristmasTreeSword` | 红绿彩灯/纸屑 | 彩灯爆点 | 1.15 / 1.05 |
| Seedler | `ItemID.Seedler` | 绿叶/粉色种荚感 | 叶绿/粉色命中点 | 1.08 / 0.98 |
| Influx Waver | `ItemID.InfluxWaver` | 青绿电弧 | 火星科技命中点 | 1.12 / 0.90 |
| Star Wrath | `ItemID.StarWrath` | 蓝/金绘制星芒，不使用星形贴图 | 蓝金星芒簇爆点 | 1.20 / 0.85 |
| Meowmere | `ItemID.Meowmere` | 彩虹/粉青粒子 | 粉金彩虹爆点 | 1.12 / 0.90 |
| Psycho Knife | `ItemID.PsychoKnife` | 紫色暗影焰 | 短促紫色命中点 | 0.78 / 0.55 |
| Brand of the Inferno | `ItemID.DD2SquireDemonSword` | 红橙火尘 | 火焰爆点 | 1.18 / 1.25 |
| Flying Dragon | `ItemID.DD2SquireBetsySword` | 橙火/青色龙息感 | 火焰爆点 | 1.22 / 1.15 |

本轮有意没有把短剑、手套、长矛、鞭、鱼叉类、Arkhalis/Terragrim、Zenith 等特殊动作武器纳入“已适配”。这些武器可能不走当前横斩 slash 路径，或者需要单独验证动作和原版 projectile 行为。

## 9. 验证标准

每新增一把武器 profile，至少验证：

- `dotnet build .\WeaponEffects.csproj` 通过。
- 该武器挥舞时粒子贴住刀光，不是从玩家或鼠标方向独立喷出。
- 命中粒子只在命中 NPC 时出现。
- 刀光长度/宽度变化可见，但不遮挡角色和敌人。
- 非目标武器不应出现同样的主题粒子或长宽变化。
- 原版弹幕类武器不能重复发射或视觉过载。

## 10. 未来计划

以下内容暂不属于当前阶段，后续需要单独设计和验证：

| 内容 | 状态 | 放到未来的原因 |
| --- | --- | --- |
| 刀光颜色 `SlashColor` | 未接入 | 需要确认与现有 shader、纹理和连招视觉层的组合方式 |
| YScale 差异 | 未接入 | 会改变刀光形状和碰撞观感，需要单独调试 |
| 挥砍随机角度 | 未接入 | 当前连招动作层已经确定角度，武器层不应直接打乱动作 |
| ExtraUpdates 差异 | 未接入 | 会影响动画速度和轨迹密度，可能影响连招节奏 |
| 每把武器专属刀光纹理 | 未实现 | 美术和加载成本高，且当前三项已足够形成第一层差异 |
| Shader 参数 | 未实现 | 风险高，容易影响所有刀光 |
| 每把武器专属连招动作 | 不建议当前做 | 会让武器 profile 和连招 profile 耦合，维护成本高 |
| fallback 类别全量启用 | 未启用 | 需要先确认更多 exact profile 的调参经验 |
| 客户端粒子强度配置 | 未实现 | 等粒子系统覆盖更多武器后再加更合适 |

## 11. 当前结论

当前阶段的正确扩展路线是：

1. 为目标武器新增 exact profile。
2. 只配置挥舞粒子、命中粒子、长度倍率、宽度倍率。
3. 进游戏观察，再微调这四组数值。
4. 不要在同一轮改颜色、YScale、Shader 或连招动作。
