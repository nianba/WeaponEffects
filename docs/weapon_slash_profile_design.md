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
| `Shape.LengthScale` | `1.18` | 火山比标准剑更长 |
| `Shape.ThicknessScale` | `1.55` | 火山刀光更厚、更重 |

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
| 星辰类 | Starfury、Star Wrath | 白蓝星点 | 亮色短爆闪 | 长度 1.1-1.25，宽度 0.8-1.0 |

## 8. 重点武器建议

| 武器 | 挥舞粒子 | 命中粒子 | 长宽建议 | 备注 |
| --- | --- | --- | --- | --- |
| Volcano / Fiery Greatsword | 橙红余烬 | 大型火焰爆点 | 长度 1.18，宽度 1.55 | 已实现 |
| Night's Edge | 紫/亮紫随机 `DustSoftStar = 278` | 紫/深紫随机爆点 | 长度 1.08，宽度 0.95 | 不再使用 `DarkSpark`，避免白带 |
| True Night's Edge | 亮紫/粉紫随机 `DustSoftStar = 278` | 强紫/深紫随机爆点 | 长度 1.15，宽度 1.05 | 注意保留原版弹幕 |
| Excalibur | 金白随机 `DustMetalSpark = 15` | 金白/亮金爆点 | 长度 1.08，宽度 1.18 | 使用原生金属尘，避免线状白带 |
| True Excalibur | 金白/亮白随机 `DustMetalSpark = 15` | 明亮金白/金色爆点 | 长度 1.15，宽度 1.25 | 注意保留原版弹幕 |
| Blade of Grass | 草绿/黄绿随机 `DustGrassLeaf = 107` | 草绿色命中碎屑 | 长度 1.08，宽度 1.10 | 必须加入 exact profile，否则不会有主题粒子 |
| Muramasa | 蓝/青蓝随机 `DustIceShard = 135` | 小型蓝白爆点 | 长度 1.0，宽度 0.7 | 适合展示窄快刀光 |
| Ice Blade | 浅蓝/冰白随机 `DustIceShard = 135` | 冰晶爆点 | 长度 1.0，宽度 0.8 | 使用原生冰尘 |
| Frostbrand | 亮蓝/冰白随机 `DustIceShard = 135` | 更强冰裂爆点 | 长度 1.12，宽度 0.9 | 比 Ice Blade 稍强 |
| Starfury | 粉色/金色随机 `DustSoftStar = 278` | 粉金星光爆点 | 长度 1.12，宽度 0.85 | 参考项目粉金色，noGravity，避免叠原版落星 |
| Bladetongue | 红黄粒子 | 红黄命中喷射 | 长度 1.05，宽度 1.05 | 已有 Ichor projectile，命中粒子别过量 |

## 9. 验证标准

每新增一把武器 profile，至少验证：

- `dotnet build .\MeleeWeaponEffects.csproj` 通过。
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
