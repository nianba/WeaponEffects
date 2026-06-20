# Adding Weapon Slash Profiles

这份文档给后续 AI 或维护者使用。目标是让新增武器效果时只走当前已经验证过的三项能力：

- 挥舞粒子
- 命中粒子
- 刀光长度和宽度

不要在同一轮顺手改颜色、YScale、随机角度、extraUpdates、Shader、纹理或连招动作。这些还不是当前稳定扩展面。

## 1. 先读这些文件

新增武器前，先读：

- `docs/weapon_slash_profile_design.md`
- `Core/Profiles/SlashProfiles.cs`
- `Core/Profiles/SlashProfileResolver.cs`
- `Core/Profiles/SlashParticleEmitter.cs`
- `Content/Projectiles/SlashChannelProjectile.cs`
- `Content/Projectiles/SlashArcProjectile.cs`

重点确认：

- 目标武器是否已经有 exact profile。
- 当前 tModLoader 版本里的 `ItemID` 常量名是否和游戏显示名一致。例如 Volcano 当前使用 `ItemID.FieryGreatsword`。
- 当前 tModLoader 版本里的 `DustID` 常量名是否真实存在。不要按概念猜 `DustID.Plantera_Green`、`DustID.Pumpkin` 这类名字；先查本地元数据或用构建验证。
- 目标武器是否已有原版弹幕逻辑，需要避免视觉过量。
- 不要只实现设计表里的少数样例后就假设同类武器都有特效。当前系统只对 `ExactProfiles` 里显式映射的武器生效，漏掉映射就等于完全没有主题粒子。
- 共享 profile 是允许的，例如木剑族、早期金属剑族、Phaseblade 族。但每把武器仍要显式写入 `ExactProfiles`。

## 2. 当前正确扩展点

| 要改什么 | 改哪里 | 说明 |
| --- | --- | --- |
| 新增武器 profile 数据 | `Core/Profiles/SlashProfiles.cs` | 增加一个 `public static WeaponSlashProfile Xxx => new(...)` |
| 新增 profile id | `Core/Profiles/SlashProfileId.cs` | 给 exact profile 一个稳定 ID |
| 映射武器到 profile | `Core/Profiles/SlashProfileResolver.cs` | 在 `ExactProfiles` 里添加 `[ItemID.Xxx] = SlashProfileId.Xxx` |
| 调挥舞粒子 | profile 的 `SwingParticles` | 不要改 projectile 逻辑 |
| 调命中粒子 | profile 的 `HitParticles` | 不要改 `OnHitNPC` 逻辑 |
| 调刀光长度 | profile 的 `Shape.LengthScale` | 已在 `SlashChannelProjectile` 中接入 |
| 调刀光宽度 | profile 的 `Shape.ThicknessScale` | 已在 legacy 和 combo 路径中接入 |

## 3. 不要改这些地方

通常不要为了新增一个武器 profile 修改：

- `Compact3DComboSchemeA`
- `SlashComboStep`
- `SlashArcVisualProfile`
- 刀光 shader
- 刀光纹理
- `SlashArcProjectile` 的绘制逻辑
- `SlashParticleEmitter` 的坐标逻辑

只有当新增能力不是当前三项之一时，才考虑这些文件，并且应先更新设计文档。

## 4. 标准新增流程

### 4.1 新增 ID

在 `SlashProfileId.cs` 增加一项：

```csharp
public enum SlashProfileId
{
	BalancedSword = 0,
	Volcano = 100,
	TargetWeapon = 110
}
```

ID 不需要连续，但建议按主题或实现批次留出间隔。

### 4.2 新增 profile

在 `SlashProfiles.cs` 添加：

```csharp
public static WeaponSlashProfile TargetWeapon => new(
	SlashProfileId.TargetWeapon,
	new Color(255, 255, 255),
	new SlashParticleProfile(
		DustID.Torch,
		new Color(255, 180, 80),
		count: 8,
		minScale: 1.0f,
		maxScale: 1.6f,
		velocityScale: 1.2f,
		spreadRadians: 0.45f),
	new SlashParticleProfile(
		DustID.Torch,
		new Color(255, 210, 110),
		count: 20,
		minScale: 1.1f,
		maxScale: 2.0f,
		velocityScale: 1.5f,
		spreadRadians: 0.75f),
	new SlashShapeProfile(
		lengthScale: 1.1f,
		thicknessScale: 1.2f,
		minYScale: 0.45f,
		maxYScale: 0.65f,
		angleRandomness: 0.3f,
		extraUpdates: 5));
```

当前阶段实际使用的是：

- `SwingParticles`
- `HitParticles`
- `Shape.LengthScale`
- `Shape.ThicknessScale`

`SlashColor`、`minYScale`、`maxYScale`、`angleRandomness`、`extraUpdates` 暂时不会影响当前效果。为了保持数据完整，可以先按合理默认值填，但不要期待它们生效。

### 4.3 新增 resolver 映射

在 `SlashProfileResolver.cs` 的 `ExactProfiles` 添加：

```csharp
[ItemID.TargetWeapon] = SlashProfileId.TargetWeapon
```

并在 `GetProfile(SlashProfileId profileId)` 的 switch 里添加：

```csharp
SlashProfileId.TargetWeapon => SlashProfiles.TargetWeapon,
```

如果不确定 `ItemID` 名称，先 `rg` 搜索或尝试构建。Volcano 的经验是：游戏显示名可能是 Volcano，但代码常量仍是 `ItemID.FieryGreatsword`。

本轮继续适配其他刀剑时，确认到几个类似例子：

- Light's Bane 是 `ItemID.LightsBane`。
- The Horseman's Blade 是 `ItemID.TheHorsemansBlade`。
- Brand of the Inferno 是 `ItemID.DD2SquireDemonSword`。
- Flying Dragon 是 `ItemID.DD2SquireBetsySword`。

可以用 Mono.Cecil 或构建验证确认本地 tModLoader 的常量名。构建能捕获拼写错误，但不能判断武器是否真的走当前横斩路径；特殊动作武器仍要进游戏验证。

## 5. 数值建议

### 5.1 挥舞粒子

挥舞粒子会在真实刀光 projectile 活动期间沿刀光轨迹生成。它不应该太多，否则会遮住刀光。

建议范围：

| 武器类型 | Count | Scale | VelocityScale | SpreadRadians |
| --- | ---: | --- | ---: | ---: |
| 轻/快武器 | 3-6 | 0.7-1.2 | 0.8-1.2 | 0.25-0.45 |
| 标准剑 | 5-8 | 0.8-1.5 | 1.0-1.3 | 0.35-0.55 |
| 重剑/火焰类 | 7-12 | 1.0-1.8 | 1.1-1.5 | 0.4-0.65 |

粒子类型必须符合武器主题，不能机械复用线状自定义 dust：

- `CommonSpark`、`DarkSpark`、`StarSpark` 都基于 `Dusts/Spark.png` 短线贴图，容易在挥砍中形成白带/色带。当前 active profile 不应再使用这些 dust。
- 火焰类优先用 `DustID.Torch`。
- 木剑可用 `DustID.WoodFurniture`。
- 自然/草系可用参考项目的 `DustGrassLeaf = 107`；但 Blade of Grass 当前回退为更大、更亮的 `DustID.Grass`，这是一次实测观感优先的例外。
- 冰系可用 `DustIceShard = 135`。
- 金属/圣光可用 `DustMetalSpark = 15`。
- 星辰类可使用参考项目的 `DustSoftStar = 278`，并使用粉金随机色：`new Color(255, 98, 206)` 或 `new Color(255, 218, 82)`。Starfury 当前是例外：它改用 `StarSlashSparkleProjectile` 纯几何绘制四角星芒，不新增贴图。
- 相位剑/光剑可用对应宝石尘：`DustID.GemSapphire/GemRuby/GemEmerald/GemAmethyst/GemDiamond/GemTopaz/GemAmber`。
- 火星科技可用 `DustID.Electric` 和 `DustID.MartianHit`。
- 彩虹/节日类可用 `DustID.RainbowMk2`、`DustID.FireworksRGB`、`DustID.Confetti*`。
- 需要随机色时使用 `SlashParticleProfile` 的 `alternateDustColor`；需要漂浮感时设置 `noGravity: true`。

### 5.2 命中粒子

命中粒子是短时反馈，可以比挥舞粒子更强。

建议范围：

| 武器类型 | Count | Scale | VelocityScale | SpreadRadians |
| --- | ---: | --- | ---: | ---: |
| 轻/快武器 | 8-14 | 0.8-1.5 | 1.0-1.5 | 0.45-0.75 |
| 标准剑 | 12-22 | 0.9-1.9 | 1.2-1.7 | 0.55-0.85 |
| 重剑/火焰类 | 20-36 | 1.2-2.4 | 1.4-1.9 | 0.75-1.0 |

### 5.3 刀光长度和宽度

建议范围：

| 武器类型 | LengthScale | ThicknessScale |
| --- | ---: | ---: |
| 短剑/细剑 | 0.75-0.95 | 0.55-0.8 |
| 标准剑 | 1.0 | 1.0 |
| 魔法/星辰剑 | 1.05-1.25 | 0.75-1.05 |
| 重剑/火焰剑 | 1.1-1.25 | 1.2-1.6 |

火山当前样板：

```csharp
lengthScale: 1.18f,
thicknessScale: 1.55f
```

## 6. 火山经验总结

### 6.1 粒子不跟随刀光的原因

错误做法：

- 在 `SlashChannelProjectile` 发出刀光时，用 `_aimRotation` 和 length 一次性生成挥舞粒子。

问题：

- channel projectile 只知道发刀光前的近似方向。
- 真实刀光由 `SlashArcProjectile` 的 `oldRot`、`Projectile.ai[1]`、YScale 和 profile transform 绘制。
- 两套坐标不一致，粒子会像独立火星团。

正确做法：

- 在 `SlashArcProjectile.AI()` 活动期内发挥舞粒子。
- 复用真实刀光的 owner center、rotation history 和变换。

### 6.2 先做 exact profile，不要一开始开 fallback

火山验证时只对 `TryGetExactProfile(...)` 命中的武器启用效果。这样调错时影响面很小。

新增武器也应该先 exact profile 验证，等多个武器稳定后再考虑 fallback 类别批量启用。

### 6.3 每轮只调一类效果

实践顺序建议：

1. 先只加 profile 数据和 resolver。
2. 再调挥舞粒子。
3. 再调命中粒子。
4. 再调长度/宽度。

不要同一轮同时改粒子、长宽、颜色、连招和 shader。否则进游戏看到问题时很难判断来源。

### 6.4 批量适配刀剑的经验

批量适配时不要复制几十份几乎一样的 profile。可用私有 helper 或共享 profile 组织低身份武器：

- 木剑族共用 `WoodSword`。
- 铜/锡/铁/铅共用 `EarlyMetalSword`。
- 银/钨/金/铂金共用 `NobleMetalSword`。
- Phaseblade/Phasesaber 用 helper 按颜色生成同结构 profile。
- 困难模式矿物剑用 helper 传颜色和长宽倍率。

但是 resolver 映射必须逐把写清楚。共享 profile 只是减少参数重复，不等于启用 fallback。

本轮暂时排除短剑、手套、长矛、鱼叉、Arkhalis/Terragrim、Zenith 等特殊动作武器。原因不是不能做，而是它们可能不走当前横斩刀光路径，或者原版 projectile 行为更特殊，需要单独验证。

## 7. 验证流程

每次新增或修改 profile 后：

1. 运行：

```powershell
dotnet build .\MeleeWeaponEffects.csproj
```

2. 如果构建失败且报 `.tmod` 被占用，先关闭 tModLoader 或禁用该 mod，再重新构建。

3. 进游戏测试：

- 挥舞时粒子是否贴着刀光。
- 命中时粒子是否只在命中点出现。
- 刀光长度和宽度是否符合武器重量。
- 非目标武器是否没有被误伤。
- 原版弹幕是否仍正常。

4. 如果要录屏复查，可以用 `ffmpeg` 导出关键帧对比粒子位置。

## 8. 常见问题

### Q: 粒子看起来不跟刀光走

检查是否有人把挥舞粒子放回了 `SlashChannelProjectile`。挥舞粒子应从 `SlashArcProjectile` 的真实刀光轨迹发出。

### Q: 改了 `ThicknessScale` 没效果

检查：

- combo 路径是否传入了 `step.ThicknessScale * profile.Shape.ThicknessScale`
- legacy `CreateSlash(...)` 是否使用了 `SlashScale * thickness`

### Q: 改了 `SlashColor` 没效果

当前阶段 `SlashColor` 不属于已验证能力，不应作为当前扩展目标。

### Q: 改了 `MinYScale/MaxYScale` 没效果

当前阶段 YScale 不接入武器 profile。不要在当前流程里依赖它。

### Q: 目标武器 profile 没生效

检查 `ItemID` 常量。游戏显示名和 tModLoader 常量名可能不同。

还要检查 `SlashProfileResolver.ExactProfiles` 是否真的有这把武器。当前阶段没有启用 fallback 批量效果，所以没有 exact 映射的武器不会有主题粒子和长宽差异。Blade of Grass 的经验就是：代码常量 `ItemID.BladeofGrass` 没有加入 resolver 前，游戏里看起来就是完全没有草系粒子。

### Q: 粒子看起来像白色拖带，不像目标主题

优先检查 `DustType`。如果主题需要点状、叶片、碎屑、星光或元素尘，不要使用基于 `Dusts/Spark.png` 的线状自定义 dust。Starfury 的经验是：用线状 dust 会出现白色带状火花，应该使用 `DustSoftStar = 278`，并随机粉色/金色。

如果 `DustSoftStar = 278` 看起来仍然只是小团，不像星星，可以像 Starfury 一样改用纯绘制 projectile：`StarSlashSparkleProjectile` 用 `VertexPositionColor`/`BasicEffect` 画四角星和小伴星，不依赖任何新增星形贴图。
