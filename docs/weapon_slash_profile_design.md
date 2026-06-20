# Weapon Slash Profile Design

## 1. 目标和范围

目标是在不重写当前刀光渲染系统的前提下，让不同武器获得更清晰的视觉身份。第一阶段优先做两类能力：

- 粒子特色：挥砍过程、刀尖、命中点、蓄力释放时的 Dust / 自定义粒子。
- 刀光形状特色：长度、宽度、Y 轴压缩、弧度、随机幅度、残影密度和挥砍方向。

第一阶段不建议做每把武器一张独立刀光纹理，也不建议先改 Shader。当前代码已经可以比较容易接入粒子和形状参数，这两项投入低、风险小、玩家感知明显。

## 2. 当前工程约束

当前普通攻击链路是：

1. `SlashGlobalItem.UseItem(...)` 根据武器生成 `SlashChannelProjectile`。
2. `SlashChannelProjectile.FireSlash(...)` 每个 `useAnimation` 周期发出一次刀光。
3. `SlashArcProjectile.CreateSlash(...)` 创建主刀光和辉光层。
4. `SlashArcProjectile.BuildVertices(...)` 根据 `oldRot` 生成 triangle strip。
5. `SlashArcProjectile.OnHitNPC(...)` 处理命中特效、声音、命中粒子和部分原版武器弹幕。

第一阶段最适合接入 profile 的位置：

- `SlashChannelProjectile.FireSlash(...)`：控制挥砍粒子、长度、宽度、Y 缩放、颜色、角度随机。
- `SlashArcProjectile.OnHitNPC(...)`：控制命中粒子、命中颜色、特殊命中反馈。
- `SlashArcProjectile.CreateSlash(...)`：接收 profile 后把形状参数传给主刀光和辉光层。

当前一个需要修正的实现细节：

- `SlashArcProjectile.CreateSlash(...)` 有 `thickness` 参数，但现在实际使用的是配置里的 `SlashScale`，传入的 `thickness` 没有生效。做武器宽度差异时，应改成 `config.SlashScale * profile.Shape.ThicknessScale`，或者让 `thickness` 真正参与计算。

## 3. 推荐架构

不要为所有武器写一个巨大分支。推荐使用“精确武器 profile + 类别兜底 profile”。

```csharp
public readonly struct WeaponSlashProfile
{
	public readonly int ProfileId;
	public readonly Color SlashColor;
	public readonly SlashParticleProfile SwingParticles;
	public readonly SlashParticleProfile HitParticles;
	public readonly SlashShapeProfile Shape;
}

public readonly struct SlashParticleProfile
{
	public readonly int DustType;
	public readonly Color DustColor;
	public readonly int SwingCount;
	public readonly int HitCount;
	public readonly float MinScale;
	public readonly float MaxScale;
	public readonly float VelocityScale;
}

public readonly struct SlashShapeProfile
{
	public readonly float LengthScale;
	public readonly float ThicknessScale;
	public readonly float MinYScale;
	public readonly float MaxYScale;
	public readonly float AngleRandomness;
	public readonly int ExtraUpdates;
}
```

查询方式：

```csharp
public static class SlashProfileResolver
{
	public static WeaponSlashProfile GetProfile(Item item)
	{
		if (ExactProfiles.TryGetValue(item.type, out WeaponSlashProfile exact))
		{
			return exact;
		}

		return GetFallbackProfile(item);
	}
}
```

设计原则：

- 重点武器用 `ItemID` 精确指定。
- 普通武器按剑型、材质、元素、稀有度、武器尺寸走兜底。
- projectile 内部只同步 `ProfileId` 或 `weaponItemType`，不要同步一整套复杂对象。
- `SlashArcProjectile` 继续负责绘制，不直接写“某把武器应该是什么风格”的逻辑。

## 4. 设计分类

| 分类 | 参数 | 第一阶段用途 | 难度 |
| --- | --- | --- | --- |
| 挥砍粒子 | DustType、颜色、数量、速度、缩放 | 每把武器最直接的主题差异 | 低 |
| 命中粒子 | DustType、数量、喷射方向、颜色 | 强化打击反馈 | 低 |
| 刀光长度 | LengthScale | 大剑更长，短剑更短 | 低 |
| 刀光宽度 | ThicknessScale | 重武器更厚，细剑更薄 | 低到中 |
| Y 轴缩放 | MinYScale、MaxYScale | 控制刀光扁平、圆润、竖向拉伸 | 低 |
| 挥砍随机 | AngleRandomness | 暗影/血色更不稳定，圣光更稳定 | 低 |
| 更新密度 | ExtraUpdates | 影响轨迹平滑度和速度感 | 低 |
| 刀光颜色 | SlashColor | 不换纹理也能形成元素差异 | 低 |
| 连段段落 | ComboStepProfile | 让同一把武器在不同段落有不同形状 | 中 |
| 专属纹理 | SlashTexturePath | 每把武器专属刀光贴图 | 高 |
| Shader 参数 | Effect 参数、混合模式 | 高级发光、扭曲、渐变 | 高 |

第一阶段建议只实现表中“低”和“低到中”的内容。

## 5. 武器类别设计

| 类别 | 适用对象 | 粒子设计 | 刀光形状 | 实现难度 |
| --- | --- | --- | --- | --- |
| 基础金属剑 | 铜、铁、银、金等普通剑 | 少量白黄金属火星，命中时小火花 | 标准长度，标准宽度，低随机 | 低 |
| 短剑/细剑 | 短剑、刺剑、速度快的小型剑 | 少量细碎火星，集中在刀尖 | 长度 0.75 到 0.9，宽度 0.55 到 0.75，YScale 偏低 | 低 |
| 标准长剑 | 大多数普通剑 | 中等金属火星 | 长度 1.0，宽度 1.0，YScale 0.45 到 0.65 | 低 |
| 重剑/大剑 | Breaker Blade、Hellstone/Volcano 类重武器 | 较大颗粒，命中爆发更多 | 长度 1.1 到 1.25，宽度 1.15 到 1.35，YScale 0.65 到 0.9 | 低 |
| 火焰类 | Volcano、火焰主题武器 | 橙红火星和余烬，命中有火焰爆点 | 宽刀光，随机略高，尾部较厚 | 低 |
| 冰霜类 | Ice Blade、Frostbrand 等 | 浅蓝冰晶和白色碎屑 | 窄而亮，YScale 0.35 到 0.55，长度略长 | 低 |
| 暗影类 | Light's Bane、Night's Edge、True Night's Edge | 紫黑 DarkSpark，粒子速度不稳定 | 宽度略窄，随机较高，残影更尖 | 低 |
| 血色/猩红类 | Blood Butcherer、Bladetongue 等 | 红色液滴、暗红火星，命中喷射更散 | 宽度中等，角度随机较高，命中粒子多 | 低到中 |
| 神圣/圣光类 | Excalibur、True Excalibur、Keybrand | 金白光点，命中有圆形爆闪感 | 宽度略大，YScale 稳定，随机低 | 低 |
| 自然/丛林类 | Blade of Grass、Seedler 等 | 绿色孢子、叶状视觉可以后续加自定义 Dust | 刀光较宽，YScale 偏高，弧线柔和 | 中 |
| 星辰类 | Starfury、Star Wrath、Enchanted Sword | 白蓝星点，尾迹更细长 | 长度较长，宽度中等偏窄，YScale 偏低 | 中 |
| 科技/能量类 | Influx Waver 等 | 青蓝短促能量点，粒子沿切线排列 | 极薄长弧，随机低，可做双层错位 | 中 |
| 事件/特殊类 | The Horseman's Blade、Flying Dragon 等 | 根据武器主题做橙色、红色或风压粒子 | 形状差异更明显，但先不改纹理 | 中 |
| 终局特殊武器 | Zenith 等当前未必被本 mod 接管的武器 | 暂不第一阶段处理 | 需要先确认兼容路径 | 高 |

## 6. 重点武器设计

这些武器适合先做精确 profile。原因是玩家辨识度高，且大部分能用现有粒子和形状参数表达。

| 武器 | 粒子设计 | 刀光形状 | 工程备注 | 难度 |
| --- | --- | --- | --- | --- |
| Terra Blade | 绿白能量粒子，挥砍中段有少量亮点，命中绿色爆点 | 长度 1.15 到 1.25，宽度 1.0，YScale 0.45 到 0.6 | 保留现有原版剑气发射逻辑 | 中 |
| Night's Edge | 紫黑 DarkSpark，粒子速度轻微扰动 | 长度 1.05，宽度 0.9，YScale 0.55 到 0.7，角度随机较高 | 可直接使用已有 `DarkSpark` | 低 |
| True Night's Edge | 更亮的紫黑粒子，命中时加深色爆点 | 长度 1.15，宽度 0.95，YScale 0.5 到 0.68 | 保留原版夜刃弹幕 | 中 |
| Excalibur | 金白光点，粒子少但亮 | 长度 1.05，宽度 1.12，YScale 0.5 到 0.62 | 第一阶段只改颜色和粒子即可 | 低 |
| True Excalibur | 金白粒子更多，命中有短爆闪 | 长度 1.15，宽度 1.18，YScale 0.48 到 0.6 | 保留原版圣剑弹幕 | 中 |
| Muramasa | 蓝白细碎光点，刀尖粒子明显 | 长度 1.0，宽度 0.65，YScale 0.3 到 0.45，ExtraUpdates 略高 | 很适合展示“窄快刀光” | 低 |
| Blade of Grass | 绿色孢子，命中时散开 | 长度 1.08，宽度 1.15，YScale 0.65 到 0.8 | 自定义叶片 Dust 放到后续，第一阶段用绿色 Dust | 中 |
| Seedler | 绿色圆点和孢子感粒子 | 长度 1.05，宽度 1.1，YScale 0.6 到 0.78 | 保留原版射出物，命中粒子不要过多 | 中 |
| Volcano / Fiery Greatsword | 橙红余烬，挥砍末端火星更多 | 长度 1.18，宽度 1.3，YScale 0.75 到 0.9 | 粒子很好做，形状也只需 profile 参数 | 低 |
| Blood Butcherer | 暗红粒子，命中喷射方向更散 | 长度 1.0，宽度 1.08，YScale 0.55 到 0.75，随机较高 | 可用血色 hit dust 表现 | 低 |
| Bladetongue | 红黄命中喷射，带 Ichor 感 | 长度 1.05，宽度 1.0，YScale 0.55 到 0.7 | 代码已有额外 Ichor projectile，需要避免重复过量 | 中 |
| Ice Blade | 浅蓝碎冰粒子，挥砍粒子少 | 长度 1.0，宽度 0.78，YScale 0.35 到 0.52 | 第一阶段用蓝白 vanilla Dust | 低 |
| Frostbrand | 更亮的冰晶和命中碎裂 | 长度 1.12，宽度 0.85，YScale 0.4 到 0.55 | 比 Ice Blade 粒子数量略高 | 低 |
| Starfury | 白蓝星点，挥砍尾部细长 | 长度 1.12，宽度 0.82，YScale 0.35 到 0.5 | 保留落星逻辑，粒子数量要克制 | 中 |
| Star Wrath | 更高亮的星点和命中爆闪 | 长度 1.2，宽度 0.9，YScale 0.35 到 0.52 | 原版落星已经很强，刀光不要过宽 | 中 |
| Enchanted Sword | 淡蓝魔法粒子 | 长度 1.06，宽度 0.88，YScale 0.4 到 0.56 | 适合作为魔法剑 fallback 样板 | 低 |
| Influx Waver | 青蓝短线粒子，沿刀光切线排列 | 长度 1.2，宽度 0.7，YScale 0.32 到 0.45，随机很低 | 双层错位刀光属于后续，中等难度 | 中 |
| The Horseman's Blade | 橙色火星和烟尘感粒子 | 长度 1.12，宽度 1.18，YScale 0.65 到 0.82 | 原版召唤效果保留，粒子别盖住原效果 | 中 |
| Breaker Blade | 大颗粒金属火星，命中重 | 长度 1.25，宽度 1.35，YScale 0.8 到 0.95 | 低成本体现重量感 | 低 |
| Cutlass | 银白细火星，挥砍轻快 | 长度 0.95，宽度 0.72，YScale 0.38 到 0.55 | 作为快速剑 fallback 样板 | 低 |
| Keybrand | 金色细粒子，命中亮点集中 | 长度 1.0，宽度 0.95，YScale 0.5 到 0.65 | 粒子和颜色足够表达 | 低 |
| Flying Dragon | 红橙风压粒子，尾部较长 | 长度 1.18，宽度 0.95，YScale 0.45 到 0.62 | 需要确认原版弹幕保留路径 | 中 |
| Zenith | 暂不做第一阶段 | 暂不做第一阶段 | 当前全局接管规则可能不会覆盖它，先不要强接 | 高 |

## 7. 形状 profile 建议

| 形状 profile | 用途 | LengthScale | ThicknessScale | YScale | AngleRandomness | ExtraUpdates |
| --- | --- | ---: | ---: | --- | ---: | ---: |
| `NeedleFast` | 短剑、细剑、Muramasa | 0.8 到 1.0 | 0.55 到 0.75 | 0.3 到 0.48 | 0.15 | 6 |
| `BalancedSword` | 标准剑 | 1.0 | 1.0 | 0.45 到 0.65 | 0.3 | 5 |
| `HeavyGreatsword` | 重剑、大剑 | 1.15 到 1.3 | 1.18 到 1.4 | 0.7 到 0.95 | 0.25 | 4 |
| `ThinEnergy` | 魔法剑、星辰剑、科技剑 | 1.1 到 1.25 | 0.7 到 0.95 | 0.32 到 0.55 | 0.12 | 6 |
| `UnstableDark` | 暗影、血色 | 1.0 到 1.15 | 0.85 到 1.1 | 0.5 到 0.75 | 0.45 | 5 |
| `OrganicWide` | 自然、孢子主题 | 1.05 到 1.15 | 1.08 到 1.25 | 0.62 到 0.82 | 0.28 | 5 |
| `HolyBroad` | 神圣、金色主题 | 1.05 到 1.18 | 1.1 到 1.25 | 0.48 到 0.65 | 0.12 | 5 |

这些数值应作为初始调参范围，不是最终平衡值。第一版实现时建议把每类 profile 做成常量，进游戏后再微调。

## 8. 粒子 profile 建议

| 粒子 profile | 视觉主题 | 挥砍粒子 | 命中粒子 | 生成位置 | 难度 |
| --- | --- | --- | --- | --- | --- |
| `MetalSpark` | 普通金属 | 少量黄白火星 | 中等火星爆点 | 刀尖和命中点 | 低 |
| `FireEmber` | 火焰 | 橙红余烬，速度慢衰减 | 较多火星和火焰点 | 刀光外沿、命中点 | 低 |
| `IceShard` | 冰霜 | 蓝白小碎屑 | 向外散开的浅蓝碎屑 | 刀尖、命中点 | 低 |
| `DarkSpark` | 暗影 | 使用现有 `DarkSpark` | 紫黑爆点 | 刀光外沿、命中点 | 低 |
| `BloodSpray` | 血色 | 暗红小点 | 较散的红色喷射 | 命中点为主 | 低 |
| `HolyGlint` | 神圣 | 金白细光点 | 集中的亮点爆发 | 刀光中段、命中点 | 低 |
| `NatureSpore` | 自然 | 绿色孢子点 | 绿色散射 | 刀光外沿 | 中 |
| `StarMote` | 星辰 | 白蓝小星点 | 短爆闪 | 刀尖、命中点 | 中 |
| `EnergyLine` | 科技/能量 | 青蓝短线，沿切线飞出 | 小范围亮点 | 刀尖和外沿 | 中 |

性能限制建议：

- 每次普通挥砍的挥砍粒子控制在 3 到 10 个。
- 每次命中的命中粒子控制在 8 到 24 个。
- 重武器和终局武器可以稍多，但不要每帧沿整条 `oldRot` 都生成粒子。
- 添加一个客户端视觉配置项，例如 `ParticleIntensity`，用于整体调节粒子数量。

## 9. 推荐实现阶段

### 阶段 1：profile 数据结构和 resolver

新增文件建议：

- `Core/Profiles/WeaponSlashProfile.cs`
- `Core/Profiles/SlashParticleProfile.cs`
- `Core/Profiles/SlashShapeProfile.cs`
- `Core/Profiles/SlashProfileResolver.cs`
- `Core/Profiles/SlashProfiles.cs`

实现内容：

- 定义数据结构。
- 加入 10 到 15 个重点武器 exact profile。
- 加入基础 fallback：短剑、标准剑、重剑、魔法剑、火焰、冰霜、暗影、神圣。

难度：低。

### 阶段 2：普通挥砍接入形状 profile

修改 `SlashChannelProjectile.FireSlash(...)`：

- `length = baseLength * profile.Shape.LengthScale`
- `thickness = config.SlashScale * profile.Shape.ThicknessScale`
- `yScale = Main.rand.NextFloat(profile.Shape.MinYScale, profile.Shape.MaxYScale)`
- `randomizedRotation = aim + Main.rand.NextFloat(-profile.Shape.AngleRandomness, profile.Shape.AngleRandomness)`
- `color = profile.SlashColor`

同时修正 `SlashArcProjectile.CreateSlash(...)` 中 `thickness` 参数不生效的问题。

难度：低到中。

### 阶段 3：普通挥砍接入粒子 profile

新增 `SlashParticleEmitter` helper：

- `EmitSwingParticles(profile, center, rotation, length, yScale)`
- `EmitHitParticles(profile, target, slashRotation)`

挥砍粒子放在 `SlashChannelProjectile.FireSlash(...)`，命中粒子放在 `SlashArcProjectile.OnHitNPC(...)`。

难度：低。

### 阶段 4：命中特效和原版弹幕协调

重点处理已有特殊逻辑：

- `Bladetongue` 已在命中时发射 Ichor projectile，粒子不要重复过量。
- `Terra Blade`、`Night's Edge`、`Excalibur`、`Starfury`、`Star Wrath` 等保留 `VanillaMeleeProjectileEmitter.Emit(...)`。
- 命中粒子只做视觉，不改变伤害和判定。

难度：中。

### 阶段 5：和连段 profile 合并

当前 `Core/Combos/` 已有 `SlashComboStep` 和 `SlashArcVisualProfile` 草稿。后续可以让武器 profile 和连段 profile 叠加：

- 武器 profile 决定主题：颜色、粒子、基础宽度、基础长度。
- 连段 profile 决定动作：第几段、角度、深度、段落强弱。

最终形状参数：

```text
finalLength = weaponLength * weaponProfile.Shape.LengthScale * comboStep.LengthScale
finalThickness = config.SlashScale * weaponProfile.Shape.ThicknessScale * comboStep.ThicknessScale
finalColor = weaponProfile.SlashColor * comboStep.Visual.MainAlpha
```

难度：中到高。建议等第一阶段粒子和形状稳定后再做。

## 10. 第一批推荐实现清单

第一批不要追求覆盖所有武器。建议先做这些：

| 优先级 | 内容 | 原因 |
| --- | --- | --- |
| P0 | 基础金属、短剑、标准剑、重剑 fallback | 立刻覆盖大量武器 |
| P0 | Terra Blade、Night's Edge、Excalibur、Muramasa、Volcano | 辨识度高，效果容易明显 |
| P1 | True Night's Edge、True Excalibur、Blade of Grass、Blood Butcherer、Ice Blade、Frostbrand | 元素差异清晰，工程量低 |
| P1 | Starfury、Star Wrath、Enchanted Sword、Bladetongue | 需要和原版弹幕协调 |
| P2 | Seedler、Influx Waver、The Horseman's Blade、Flying Dragon | 主题明显，但调参成本更高 |
| P3 | Zenith 和其他复杂终局武器 | 先确认是否被当前全局接管规则覆盖 |

## 11. 验证标准

实现后建议这样验证：

- `dotnet build .\MeleeWeaponEffects.csproj` 必须通过。
- 每个 fallback 类至少测试一把武器。
- 每个 exact profile 至少测试一次普通挥砍和一次命中。
- 快速武器不能因为粒子太多掉帧。
- 重武器应该看起来更厚更重，但不能遮住角色和敌人。
- 星辰、原版弹幕类武器不能重复发射或视觉过载。
- 多人环境下 profile 颜色和粒子风格应保持一致，至少不能因为未同步字段导致刀光形状不同步。

## 12. 结论

最推荐的工程路线是：

1. 先做 `SlashProfileResolver`。
2. 先接入粒子和形状参数。
3. 先覆盖 fallback 类别和少量重点武器。
4. 再和四段连斩 / combo profile 结合。
5. 最后才考虑每把武器专属纹理或 Shader。

这样能在较小改动下让大部分武器立刻产生差异，同时保留后续扩展空间。

