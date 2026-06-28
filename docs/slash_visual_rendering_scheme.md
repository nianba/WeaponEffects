# 刀光视觉效果绘制方案

## 1. 当前结论

当前项目的刀光视觉不是普通贴图动画，也不是简单 `SpriteBatch` 拖尾，而是：

```text
近战武器接管
-> 控制型 projectile 读取瞄准和连段
-> 创建刀光主体 projectile 和发光层 projectile
-> 主体 projectile 缓存旋转轨迹
-> 根据轨迹构建 TriangleStrip 顶点
-> 绑定刀光纹理、武器纹理和 shader
-> DrawUserPrimitives 绘制
-> 叠加 additive 发光、挥舞粒子、命中闪光和命中粒子
```

普通攻击当前默认使用 `Compact3DComboSchemeA` 四段连招的 profiled slash；蓄力斩仍使用较早的 `CreateSlash` 绘制路径。

## 2. 关键文件

| 文件 | 作用 |
| --- | --- |
| `Content/Items/SlashGlobalItem.cs` | 近战武器接管入口，决定哪些武器进入自定义刀光流程。 |
| `Content/Projectiles/SlashChannelProjectile.cs` | 普通攻击控制器，读取鼠标方向、推进连段、发射刀光。 |
| `Content/Projectiles/ChargedSlashProjectile.cs` | 蓄力攻击控制器，绘制持剑和蓄力条，释放蓄力刀光。 |
| `Content/Projectiles/SlashArcProjectile.cs` | 刀光主体，负责碰撞、命中、格挡敌对弹幕、主刀光绘制、武器贴图绘制和挥舞粒子。 |
| `Content/Projectiles/SlashArcGlowProjectile.cs` | 发光层，跟随刀光轨迹绘制 additive 光效。 |
| `Content/Projectiles/SlashHitEffectProjectile.cs` | 命中点横向闪光。 |
| `Content/Projectiles/StarSlashSparkleProjectile.cs` | 星辰类粒子的几何星芒绘制。 |
| `Core/Combos/Compact3DComboSchemeA.cs` | 当前普通攻击四段连招的动作和视觉参数。 |
| `Core/Combos/SlashArcVisualProfile.cs` | profiled slash 的伪 3D、边缘、高光、拖影参数结构。 |
| `Core/Graphics/SlashVertex.cs` | 刀光自定义顶点格式。 |
| `Core/Graphics/MeleeEffectAssets.cs` | 刀光纹理、发光纹理、蓄力条纹理、音效和 projectile 创建封装。 |
| `Core/Profiles/SlashProfiles.cs` | 武器主题 profile 数据。 |
| `Core/Profiles/SlashProfileResolver.cs` | 从 `ItemID` 映射到武器 profile。 |
| `Core/Profiles/SlashParticleEmitter.cs` | 挥舞粒子和命中粒子发射。 |
| `Common/Configs/WeaponEffectsConfig.cs` | 服务端玩法配置和客户端视觉配置。 |

## 3. 武器接管入口

`SlashGlobalItem.SetDefaults` 会筛选支持的近战武器，并把它们改为自定义使用方式：

- `noUseGraphic = true`：不画原版使用动作。
- `noMelee = true`：禁用原版近战 hitbox。
- `channel = true`：允许控制型 projectile 持续更新。
- `useStyle = ItemUseStyleID.Rapier`：使用一个轻量的原版使用样式作为承载。
- 需要有 exact weapon profile，否则当前不会进入刀光重做流程。

玩家使用武器时：

- 普通攻击创建 `SlashChannelProjectile`。
- 右键蓄力创建 `ChargedSlashProjectile`。
- 本地玩家负责读取 `Main.MouseWorld`，再通过 projectile 同步必要状态。

## 4. 普通攻击发射方案

`SlashChannelProjectile` 是普通攻击的控制器，本身不绘制刀光。

当前默认发射模式是：

```csharp
private static readonly SlashEmissionMode EmissionMode = SlashEmissionMode.Compact3DComboSchemeA;
```

每次满足攻击间隔时调用 `FireComboSchemeASlash`：

1. 读取当前玩家的连段索引。
2. 从 `Compact3DComboSchemeA.GetStep` 取出当前段配置。
3. 用鼠标瞄准角减去 `HitAngleDegrees` 得到基础旋转。
4. 用 `StartAngleDegrees` 作为刀光开始角。
5. 用武器贴图长度、legacy 平均长度倍率、连段长度倍率计算刀光长度。
6. 用连段厚度倍率计算刀光厚度。
7. 第四段额外读取饰品提供的重斩长度和伤害倍率。
8. 如果武器有 exact profile，再叠加 `Shape.LengthScale` 和 `Shape.ThicknessScale`。
9. 调用 `SlashArcProjectile.CreateProfiledSlash` 创建刀光主体和发光层。

普通攻击最终长度和厚度可以简化理解为：

```text
finalLength =
    weaponTextureLength
    * LegacyAverageLengthScale
    * comboStep.LengthScale
    * weaponProfile.Shape.LengthScale
    * fourthSlashLengthMultiplier

finalThickness =
    gameplayConfig.SlashScale
    * comboStep.ThicknessScale
    * weaponProfile.Shape.ThicknessScale
```

第四段倍率只在第四段连招生效。

## 5. 蓄力攻击发射方案

`ChargedSlashProjectile` 持续读取右键状态和鼠标方向。

蓄力期间：

- 可选绘制持剑。
- 可选绘制蓄力条。
- 周期性生成蓄力 dust。
- 达到蓄满阈值后生成 ready burst 粒子。

释放时调用 `ReleaseChargedSlash`：

1. 如果蓄力低于最短蓄力帧，不发出刀光。
2. 根据蓄力进度计算长度倍率和伤害倍率。
3. 读取饰品提供的蓄力加成。
4. 从武器 profile 读取 `SlashColor` 作为蓄力刀光颜色。
5. 调用 `SlashArcProjectile.CreateSlash`。

注意：蓄力斩当前走 legacy `CreateSlash`，不是普通攻击的 `CreateProfiledSlash` 多 pass 视觉路径。

## 6. 主刀光 projectile 数据模型

`SlashArcProjectile` 创建时会把关键绘制数据放到 projectile 字段里：

| 数据 | 来源 | 用途 |
| --- | --- | --- |
| `Projectile.velocity.Length()` | 发射时传入的长度向量 | 刀光外半径。 |
| `Projectile.rotation` | starting rotation | 当前帧局部挥砍角。 |
| `Projectile.ai[1]` | base rotation | 把局部挥砍角旋转到玩家瞄准方向。 |
| `Projectile.localAI[0]` | thickness | 内外半径差，也就是刀光宽度。 |
| `Projectile.localAI[1]` | yScale | 椭圆压缩比例。 |
| `Projectile.oldRot` | tModLoader trail cache | 历史旋转轨迹，用于生成拖尾面片。 |
| `_weaponItemType` | 发射武器类型 | 取武器贴图、profile、命中特效。 |
| `_usesVisualProfile` | 是否 profiled slash | 决定走 legacy 顶点构建还是多 pass profiled 顶点构建。 |

`SetStaticDefaults` 设置：

```csharp
ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
```

也就是主刀光依赖最多 40 帧旋转历史。

## 7. 刀光运动轨迹

`SlashArcProjectile.AI` 中，当 `timeLeft > 40` 时推进挥砍角：

```csharp
float rotationStep = MathHelper.Lerp(0.14f, 0f, 1f - (Projectile.timeLeft - 40) / 60f);
Projectile.rotation += _reverse ? -rotationStep : rotationStep;
```

含义：

- 刀光生命周期前 60 帧有旋转推进。
- 旋转步长从约 `0.14` 逐渐衰减到 `0`。
- `_reverse` 决定顺挥或逆挥。
- 每帧的历史角会进入 `Projectile.oldRot`，供绘制和碰撞采样使用。

视觉轨迹和碰撞轨迹都基于这套 rotation history，因此粒子如果要贴合刀光，也必须在 `SlashArcProjectile` 内根据真实轨迹生成。

## 8. Legacy 主刀光绘制

legacy 绘制路径由 `BuildVertices` 生成顶点。

对每个有效 `Projectile.oldRot[i]`：

1. 用历史旋转角得到方向向量。
2. 乘以刀光长度得到外侧点 `outer`。
3. 根据 `SlashStyle` 决定内侧点 `inner`。
4. 应用 `localAI[1]` 的 Y 轴缩放。
5. 再按 `Projectile.ai[1]` 旋转到玩家瞄准方向。
6. 写入两个 `SlashVertex`。

每个采样点写两个顶点：

```text
outer vertex: texcoord.y = 0
inner vertex: texcoord.y = 1
```

所有点组成一条 `TriangleStrip`。

legacy 路径主要用于蓄力斩和旧随机刀光。

## 9. Profiled 主刀光绘制

普通四段连招当前使用 profiled 绘制路径。

`DrawProfiledSlash` 会在同一个 shader 和纹理状态下多次调用 `DrawProfilePass`，叠加多个视觉层：

| pass | 作用 |
| --- | --- |
| `FarRim` | 远侧边缘，偏暗、偏薄、略向远侧偏移。 |
| `TrailEcho` | 拖影回声，延迟角度绘制。 |
| `Main` | 主体刀光面。 |
| `Core` | 中央亮芯。 |
| `PeakFlare` | 命中峰值附近的强闪。 |
| `NearEdge` | 近侧边缘，偏亮、偏前景、带像素偏移。 |

profiled slash 的伪 3D 感来自这些机制：

- `XScale` 和 `YScale` 把圆弧压成椭圆。
- `StartDepth`、`HitDepth`、`EndDepth` 根据生命周期插值，模拟前后景深。
- depth 会轻微影响半径和透明度。
- near/far rim 使用法线方向像素偏移，制造刀刃厚度。
- `TrailCount` 和 `TrailDelayDegrees` 生成延迟拖影。
- `PeakFlareAlpha` 在命中进度附近增强高光。
- `CrescentWidthFactor` 让刀光两端收尖，中段更宽。
- `AddProfileFrontTip` 给前端额外补一个尖端，避免前端显得断。

这套方案本质是 2D 顶点带上的多层伪 3D 表现，不是真正 3D mesh。

## 10. SpriteBatch 和 shader 状态

主刀光在 `PreDraw` 中手动切换绘制状态：

```text
Main.spriteBatch.End()
Main.spriteBatch.Begin(SpriteSortMode.Immediate, ...)
绑定 Textures[0] 和 Textures[1]
slashEffect.CurrentTechnique.Passes[0].Apply()
DrawUserPrimitives(PrimitiveType.TriangleStrip, ...)
Main.spriteBatch.End()
Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, ...)
```

绑定纹理：

- `Textures[0]`：刀光纹理。`SlashStyle == 1` 时使用 `Extra_193`，否则使用 `Assets/Textures/SlashTex.png`。
- `Textures[1]`：当前武器贴图。

shader：

- 路径是 `WeaponEffects/Effects/Mhd`。
- 加载方式是 `ModContent.Request<Effect>(..., AssetRequestMode.ImmediateLoad)`。

这是高风险区域。修改时必须保证 `spriteBatch.End()` 和 `Begin()` 成对恢复，否则会影响 Terraria 和其他模组的后续绘制。

## 11. 发光层绘制

`SlashArcGlowProjectile` 是独立 projectile，只负责视觉。

特点：

- 和主体一样缓存 40 帧 rotation history。
- 和主体一样根据 `oldRot` 构建 `TriangleStrip`。
- 只在客户端视觉配置 `SlashStyle == 1` 时绘制。
- 使用 `BlendState.Additive`。
- 使用 `MeleeEffectAssets.SlashGlowTexture`，也就是 `Assets/Textures/EX112.png`。
- 亮度受 `WeaponEffectsVisualConfig.SlashBlink` 控制。

profiled glow 会额外读取：

- `GlowAlpha`
- `PeakFlareAlpha`
- `NearEdgeOffsetPixels`
- depth 插值参数

因此普通连招的发光层也会跟随伪 3D 深度和命中峰值变化。

## 12. 武器贴图绘制

主刀光 `PreDraw` 结束前，如果客户端配置 `DrawHeldWeapon` 为 true，会调用 `DrawWeapon`。

绘制方式：

- 通过 `_weaponItemType` 从 `TextureAssets.Item` 取武器贴图。
- 根据当前刀光 rotation 计算武器朝向。
- 根据 `_reverse` 和 `Projectile.ai[1]` 判断是否水平翻转。
- 用 `Main.EntitySpriteDraw` 绘制武器本体。

因此原版武器使用图被隐藏后，视觉上的武器由刀光 projectile 自己补画。

## 13. 粒子和命中特效

### 挥舞粒子

挥舞粒子在 `SlashArcProjectile.AI` 活动期内生成，而不是在 `SlashChannelProjectile` 发射瞬间生成。

原因：

- channel projectile 只知道瞄准方向和将要发出的刀光。
- 真实刀光轨迹由 `Projectile.rotation`、`Projectile.oldRot`、`Projectile.ai[1]`、`localAI[1]` 和 profile transform 共同决定。
- 从主体刀光里发粒子，才能让粒子贴住真实刀光轨迹。

发射过程：

1. `EmitExactProfileSwingParticles` 查 exact weapon profile。
2. 从当前有效 trail sample 中选位置。
3. 用同一套 `SlashOffsetForRotation` 得到刀光上的位置。
4. 根据相邻 trail sample 估算切线方向。
5. 调用 `SlashParticleEmitter.EmitSwingTrailParticle`。

### 命中粒子

命中时在 `SlashArcProjectile.OnHitNPC` 内处理：

- 创建 `SlashHitEffectProjectile` 横向闪光。
- 播放命中或格挡音效。
- 生成基础血液或火花 dust。
- 根据 exact weapon profile 生成主题命中粒子。
- 个别武器触发额外 projectile 或 buff，例如 Bladetongue 和 Volcano。

### 星芒粒子

`SlashParticleVisualStyle.DrawnStar` 会走 `StarSlashSparkleProjectile`，用 `BasicEffect` 和 `VertexPositionColor` 绘制几何星芒。

这条路径不依赖新增星形贴图，适合 Starfury、Star Wrath 这类需要明确星形轮廓的武器。

## 14. 碰撞和视觉的关系

主体刀光的碰撞也使用 rotation history。

`Colliding` 调用 `CollisionAreaIntersects`，再通过 `ForEachCollisionLine` 采样：

- 当前旋转方向的一条线。
- 历史旋转中的若干条线。
- 每条线从 owner center 指向刀光外端。
- 线宽由刀光长度和厚度计算。

profiled slash 会根据 depth 对碰撞半径做轻微修正。整体上碰撞和视觉共享轨迹，但碰撞仍是多条线段近似，不是逐像素或完整扇形 mesh。

## 15. 配置项

### 服务端玩法配置

| 配置 | 作用 |
| --- | --- |
| `EnableSlashRework` | 是否启用近战刀光重做。 |
| `EnableSpearRework` | 是否启用长矛重做。 |
| `SlashScale` | 全局刀光宽度倍率。 |
| `NormalSlashDamageMultiplier` | 普通刀光伤害倍率。 |
| `NormalSlashIntervalMultiplier` | 普通刀光发射间隔倍率。 |
| `SlashKnockbackMultiplier` | 刀光击退倍率。 |
| `ChargeDamage` | 蓄力斩最大伤害倍率。 |
| `ChargeMinDurationMultiplier` | 最短蓄力时间。 |
| `ChargeMaxDurationMultiplier` | 蓄满时间。 |
| `ChargeLengthScale` | 蓄力斩最大长度倍率。 |
| `SlashCanKillProjectiles` | 是否允许刀光击落敌对弹幕。 |
| `CanCharge` | 是否启用右键蓄力。 |
| `EmitVanillaSwordProjectiles` | 是否恢复或强化原版剑类弹幕。 |
| `ComboResetDelay` | 连段重置延迟。 |

### 客户端视觉配置

| 配置 | 作用 |
| --- | --- |
| `SlashBlink` | 发光层亮度倍率。 |
| `SlashStyle` | 刀光绘制风格。 |
| `ShowChargeBar` | 是否显示蓄力条。 |
| `DrawHeldWeapon` | 是否由刀光 projectile 绘制武器贴图。 |
| `ParticleDensity` | 粒子数量倍率。 |
| `ScreenShakeStrength` | 屏幕震动强度倍率。 |
| `EffectVolume` | 特效音量倍率。 |

## 16. 当前能力边界

已经稳定接入的能力：

- 普通攻击四段 profiled slash。
- 主体刀光 TriangleStrip 绘制。
- 独立 additive 发光层。
- 武器贴图补画。
- 按 exact profile 调整刀光长度和厚度。
- 按 exact profile 生成挥舞粒子和命中粒子。
- 星辰类几何星芒粒子。
- 蓄力斩长度、伤害、颜色和蓄力反馈。

暂时不应当当作稳定扩展面的能力：

- 每把武器独立刀光 shader。
- 每把武器独立刀光纹理。
- 每把武器独立连招动作。
- 武器 profile 直接控制普通连招的 `YScale`、随机角度和 `extraUpdates`。
- 蓄力斩使用 profiled slash 多 pass 视觉。
- fallback 武器批量启用主题差异。

## 17. 后续调整建议

如果只做武器差异化，优先改：

1. `SlashProfileResolver.ExactProfiles`
2. `SlashProfiles`
3. `Shape.LengthScale`
4. `Shape.ThicknessScale`
5. `SwingParticles`
6. `HitParticles`

如果要改普通刀光动作和层次，优先改：

1. `Compact3DComboSchemeA`
2. `SlashArcVisualProfile`
3. `SlashArcProjectile.DrawProfiledSlash`
4. `SlashArcProjectile.BuildProfileVertices`
5. `SlashArcGlowProjectile.BuildVertices`

如果要改底层绘制方式，需要重点检查：

1. `SlashVertex` 顶点格式是否和 shader 输入一致。
2. `Mhd.xnb` 是否仍兼容绑定纹理和 texcoord。
3. `spriteBatch` 状态是否完整恢复。
4. 主体和 glow 的 trail cache 是否仍一致。
5. 碰撞采样是否仍和视觉轨迹大体一致。

## 18. 验证清单

修改刀光绘制后至少验证：

- `dotnet build .\WeaponEffects.csproj` 通过。
- 普通攻击四段连招都有刀光。
- 蓄力斩仍能释放并显示刀光。
- `SlashStyle == 1` 时有发光层。
- `DrawHeldWeapon` 开关能正确显示或隐藏武器贴图。
- 粒子贴合刀光轨迹，不从玩家中心或鼠标方向孤立喷出。
- 命中 NPC 时有命中闪光和主题粒子。
- 敌对弹幕格挡配置开关正常。
- 多人环境下其他客户端能看到正确方向和武器贴图。
