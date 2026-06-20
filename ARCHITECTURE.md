# Melee Weapon Effects 架构文档

## 项目定位

`MeleeWeaponEffects` 是一个 tModLoader 模组，用全局物品逻辑接管符合条件的近战武器使用方式，将原版挥砍替换为自定义剑气、蓄力斩、命中特效、屏幕震动和部分原版弹幕兼容逻辑。

当前工程的核心设计是：

- `GlobalItem` 作为入口，筛选并改写近战武器行为。
- 控制类投射物负责玩家持剑、瞄准、蓄力、连段节奏。
- 伤害类投射物负责剑气碰撞、命中、格挡敌对弹幕、绘制。
- 资源和绘制逻辑集中在 `Core/Graphics` 与 `Assets/Textures`。
- 服务端配置控制玩法数值，客户端配置控制视觉表现。

## 目录职责

| 路径 | 职责 | 主要内容 |
| --- | --- | --- |
| `MeleeWeaponEffects.cs` | 模组入口 | tModLoader `Mod` 类型，当前仅作为注册入口。 |
| `Common/Configs` | 配置定义 | `MeleeWeaponEffectsGameplayConfig` 是服务端玩法配置，`MeleeWeaponEffectsVisualConfig` 是客户端视觉配置。 |
| `Common/Players` | 玩家局部状态 | `MeleeEffectsPlayer` 负责本地屏幕震动。 |
| `Content/Items` | 物品全局改写 | `SlashGlobalItem` 筛选近战武器并替换使用行为。 |
| `Content/Projectiles` | 投射物与战斗流程 | 普通挥砍控制、蓄力控制、剑气本体、剑气光效、命中特效、原版弹幕兼容。 |
| `Content/Dusts` | 自定义粒子 | `CommonSpark` 和 `DarkSpark` 定义火花粒子的更新和绘制。 |
| `Core/Graphics` | 绘制基础设施 | 资源路径帮助类 `MeleeEffectAssets` 和自定义顶点 `SlashVertex`。 |
| `Core/Combos` | 普通攻击连段配置 | `Compact3DComboSchemeA`、`SlashComboStep`、`SlashArcVisualProfile` 描述四段挥砍角度和视觉参数。 |
| `Assets/Textures` | 模组纹理资源 | 剑气纹理、蓄力条底图和填充图。 |
| `Effects` | Shader 资源 | `Mhd.xnb` 用于剑气自定义顶点绘制。 |
| `Sounds` | 音效资源 | 蓄力、挥砍、命中、格挡等音效。 |
| `Localization` | 本地化 | 中英文配置与模组文本。 |
| `Dusts` | 粒子贴图 | 火花粒子贴图。 |

## 核心运行流程

1. tModLoader 加载模组，注册配置、全局物品、投射物、粒子和资源。
2. `SlashGlobalItem.SetDefaults` 检查物品是否符合接管条件：
   - 必须有伤害。
   - 必须是 `DamageClass.Melee` 或 `DamageClass.MeleeNoSpeed`。
   - 排除镰刀、饰品、斧镐锤等工具。
   - 支持无弹幕武器，或部分原版剑气类武器。
3. 符合条件的武器会被改写为：
   - `noUseGraphic = true`
   - `noMelee = true`
   - `channel = true`
   - `useStyle = ItemUseStyleID.Rapier`
4. 玩家使用武器时，`SlashGlobalItem.UseItem` 只在本地玩家侧创建控制投射物：
   - 普通攻击创建 `SlashChannelProjectile`。
   - 右键蓄力创建 `ChargedSlashProjectile`。
5. 控制投射物持有武器类型、使用动画时长、武器长度、瞄准点和瞄准角度，并通过 `SendExtraAI` / `ReceiveExtraAI` 同步给其他客户端。
6. 普通攻击控制器按武器 `useAnimation` 周期发射剑气。
7. 蓄力攻击控制器在持续按住到 `useAnimation * 3` 后进入可释放状态，结束时生成高伤害剑气并触发屏幕震动。
8. `SlashArcProjectile.CreateSlash` 同时创建：
   - `SlashArcProjectile`：有碰撞和伤害的剑气本体。
   - `SlashArcGlowProjectile`：只负责额外发光视觉层。
9. 剑气本体在 AI、碰撞、命中和绘制阶段分别处理武器朝向、命中判定、敌对弹幕格挡、音效粒子、Shader 绘制和武器贴图绘制。

## 普通攻击连段

普通攻击当前默认使用 `Compact3DComboSchemeA`，不再使用纯随机挥砍作为主路径。该方案包含四个循环步骤：

| 段数 | 名称 | 作用 |
| --- | --- | --- |
| 0 | `Falling Diagonal Opener` | 下落斜向起手。 |
| 1 | `Reverse Rising Cut` | 反向上挑。 |
| 2 | `Front-Plane Horizontal Slice` | 正面水平切。 |
| 3 | `Compact Downward Finisher` | 紧凑下劈收尾。 |

每个 `SlashComboStep` 包含：

- 起始角、命中角、结束角。
- 伤害活跃区间字段。
- 长度倍率、厚度倍率。
- `extraUpdates`。
- `SlashArcVisualProfile` 视觉参数。

当前发射逻辑实际使用了命中角、起始角、长度倍率、厚度倍率、`extraUpdates` 和颜色；`SlashArcVisualProfile` 中更细的深度、拖影、边缘高光等字段已经建模，但尚未完全接入绘制管线。

## 关键类型关系

| 类型 | 角色 | 依赖方向 |
| --- | --- | --- |
| `SlashGlobalItem` | 近战武器接管入口 | 创建 `SlashChannelProjectile` 或 `ChargedSlashProjectile`，调用配置与贴图长度计算。 |
| `SlashChannelProjectile` | 普通攻击控制器 | 读取本地鼠标，循环连段，调用 `VanillaMeleeProjectileEmitter` 和 `SlashArcProjectile.CreateSlash`。 |
| `ChargedSlashProjectile` | 蓄力攻击控制器 | 读取本地鼠标，绘制武器和蓄力条，蓄力完成后创建强化剑气。 |
| `SlashArcProjectile` | 伤害剑气本体 | 处理碰撞、切草、命中、弹幕格挡、武器绘制和 Shader 剑气绘制。 |
| `SlashArcGlowProjectile` | 剑气发光层 | 跟随剑气轨迹绘制 additive 光效。 |
| `SlashHitEffectProjectile` | 命中视觉 | 在命中点绘制短暂横向闪光。 |
| `VanillaMeleeProjectileEmitter` | 原版武器兼容 | 为星怒、星光、光束剑、天顶剑等恢复或强化原版弹幕行为。 |
| `MeleeEffectAssets` | 资源与封装入口 | 统一纹理路径，并封装声音和投射物创建调用。 |
| `SlashVertex` | 自定义顶点格式 | 给剑气 Shader 提供位置、颜色和纹理坐标。 |

## 多人同步边界

| 数据 | 当前策略 | 原因 |
| --- | --- | --- |
| 鼠标瞄准点 | 只有 `Projectile.owner == Main.myPlayer` 时读取 `Main.MouseWorld` | 远端客户端没有其他玩家的鼠标状态。 |
| 控制器状态 | 武器类型、动画时长、武器长度、目标点、瞄准角、连段索引通过 `SendExtraAI` 同步 | 其他客户端需要复现持剑朝向和视觉。 |
| 视觉配置 | `MeleeWeaponEffectsVisualConfig` 使用 `ClientSide` | 每个客户端可独立选择剑气样式和闪光强度。 |
| 玩法配置 | `MeleeWeaponEffectsGameplayConfig` 使用 `ServerSide` | 伤害倍率、剑气尺寸和是否格挡弹幕属于玩法规则。 |
| 武器贴图 | 同步 `weaponItemType`，本地从 `TextureAssets.Item` 取贴图 | 避免同步不可序列化的 `Texture2D`。 |
| 原版弹幕发射 | `VanillaMeleeProjectileEmitter.Emit` 内部限制本地玩家执行 | 避免多人下重复创建弹幕。 |

新增需要同步的字段时，应同时更新对应投射物的 `SendExtraAI` 和 `ReceiveExtraAI`，并确认字段可序列化。

## 绘制流程

剑气绘制主要发生在 `SlashArcProjectile.PreDraw` 和 `SlashArcGlowProjectile.PreDraw`。

1. 读取 `Projectile.oldRot` 构造剑气轨迹。
2. 将轨迹转换为 `SlashVertex` 三角带。
3. 切换 `Main.spriteBatch` 到 `SpriteSortMode.Immediate`。
4. 绑定剑气纹理和武器纹理。
5. 应用 `Effects/Mhd.xnb` Shader。
6. 调用 `DrawUserPrimitives(PrimitiveType.TriangleStrip, ...)` 绘制剑气。
7. 恢复 `Main.spriteBatch` 状态。
8. 绘制武器本体贴图。

这里是高风险区域：任何修改都要保证 `spriteBatch.End()` 和 `spriteBatch.Begin()` 成对恢复，否则容易影响其他模组或 Terraria 自身绘制。

## 配置项

| 配置类型 | 字段 | 作用 |
| --- | --- | --- |
| 服务端玩法配置 | `SlashScale` | 控制剑气内外宽度比例。 |
| 服务端玩法配置 | `ChargeDamage` | 蓄力斩伤害倍率。 |
| 服务端玩法配置 | `SlashCanKillProjectiles` | 是否允许剑气击落敌对弹幕。 |
| 服务端玩法配置 | `CanCharge` | 是否允许右键蓄力。 |
| 客户端视觉配置 | `SlashBlink` | 发光层亮度倍率。 |
| 客户端视觉配置 | `SlashStyle` | 剑气绘制风格选择。 |

## 扩展点

| 目标 | 推荐修改点 |
| --- | --- |
| 调整哪些武器被接管 | `SlashGlobalItem.ShouldUseSlashAction`。 |
| 保留或清除原版弹幕 | `SlashGlobalItem.IsVanillaSwordSlashProjectile` 和 `ShouldClearOriginalShoot`。 |
| 增加某些武器的特殊弹幕 | `VanillaMeleeProjectileEmitter.EmitNormal` / `EmitCharged`。 |
| 调整普通攻击连段 | `Core/Combos/Compact3DComboSchemeA.cs`。 |
| 增加新的连段方案 | 新建连段配置类，并在 `SlashChannelProjectile` 的发射模式中接入。 |
| 调整剑气绘制资源 | `MeleeEffectAssets` 的路径常量和 `Assets/Textures`。 |
| 改变蓄力判定 | `ChargedSlashProjectile.ChargeReadyFrame` 和 `OnKill`。 |
| 改变敌对弹幕格挡规则 | `SlashArcProjectile.HandleProjectileBlocking` 和 `CanBlock`。 |

## 文件组织约定

- 游戏内容类型放在 `Content/*`。
- 非内容但被多个内容类共享的基础设施放在 `Core/*`。
- 玩家或配置这类通用 tModLoader 类型放在 `Common/*`。
- 新增贴图优先放在 `Assets/Textures`，并通过 `MeleeEffectAssets` 暴露路径。
- 新增音效放在 `Sounds`。
- 新增本地化文本同步更新 `Localization/en-US_Mods.MeleeWeaponEffects.hjson` 和 `Localization/zh-Hans_Mods.MeleeWeaponEffects.hjson`。

## 修改风险清单

| 区域 | 风险 | 建议 |
| --- | --- | --- |
| `SlashGlobalItem.SetDefaults` | 会影响所有被筛选到的近战武器 | 调整筛选条件后要测试普通剑、发射弹幕的剑、工具和饰品。 |
| `SlashChannelProjectile` / `ChargedSlashProjectile` | 多人下瞄准和状态可能不同步 | 修改私有字段后同步更新 ExtraAI。 |
| `SlashArcProjectile.PreDraw` | SpriteBatch 状态破坏会引起全局绘制异常 | 修改后进入游戏实际看一遍剑气、UI 和其他实体绘制。 |
| `VanillaMeleeProjectileEmitter` | 容易重复发射弹幕或改变原版武器强度 | 保持本地玩家门禁，并单独测试特殊武器。 |
| `Core/Combos` | 连段参数过激可能造成判定不符合视觉 | 调整角度、长度、厚度后测试命中线和视觉轨迹是否一致。 |
| 配置类 | 改字段名会影响已有配置文件 | 需要改名时考虑兼容旧配置或接受重置。 |

## 构建与验证

常规构建命令：

```powershell
dotnet build .\MeleeWeaponEffects.csproj
```

推荐验证顺序：

1. 构建通过，确认没有编译错误。
2. 启动 tModLoader，确认模组可加载。
3. 单人测试普通剑、星怒类武器、天顶剑类武器。
4. 测试右键蓄力开启和关闭两种配置。
5. 测试敌对弹幕格挡开启和关闭两种配置。
6. 多人环境下测试其他客户端是否能看到正确的武器贴图、剑气方向和连段视觉。

