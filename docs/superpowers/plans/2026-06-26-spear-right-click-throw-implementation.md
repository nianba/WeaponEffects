# Spear Right-Click Throw Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Trident right-click charged throw described in `docs/superpowers/specs/2026-06-26-spear-right-click-throw-design.md`.

**Architecture:** Add a spear-only right-click chain: `SpearGlobalItem` starts or interrupts into `SpearThrowChargeProjectile`, which releases `SpearThrowProjectile`. Pure charge math lives in `Core/Spears` so scaling rules can be tested without tModLoader.

**Tech Stack:** C# latest, tModLoader `GlobalItem`/`ModProjectile`, XNA/FNA drawing, existing `MeleeEffectAssets`, existing `tests/WeaponEffects.Tests` .NET 8 source/logic test harness.

---

## File Structure

- Create `Core/Spears/SpearThrowChargeMath.cs`: pure constants and linear charge scaling.
- Modify `tests/WeaponEffects.Tests/Program.cs`: add charge math tests and source-wiring checks.
- Modify `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`: copy source files used by source-wiring checks.
- Modify `Common/Players/WeaponEffectsPlayer.cs`: add `ResetSpearCombo()` and call spear right-click interrupt from `PostUpdate`.
- Modify `Content/Items/SpearGlobalItem.cs`: add right-click entry, interrupt helpers, and charge-controller spawn.
- Create `Content/Projectiles/SpearThrowChargeProjectile.cs`: hold-to-charge controller, pose, heat visuals, full-charge particles, release/cancel behavior.
- Create `Content/Projectiles/SpearThrowProjectile.cs`: fixed-speed spindle light projectile with wall piercing, multi-target piercing, one hit per NPC, generous collision.

## Constants Chosen For V1

- Minimum valid charge: `60` frames.
- Base full charge: `300` frames.
- Effective full-charge lower bound: `120` frames.
- Damage multiplier: linear `1f` to `10f`.
- Range multiplier by screen width: linear `1.5f` to `5f`.
- Fixed throw speed: `42f` pixels per tick.
- Visible spindle width: linear `18f` to `34f`.
- Collision width: linear `34f` to `58f`.

---

### Task 1: Pure Charge Math And Tests

**Files:**
- Create: `Core/Spears/SpearThrowChargeMath.cs`
- Modify: `tests/WeaponEffects.Tests/Program.cs`

- [ ] **Step 1: Write failing tests for charge math**

Add the test registrations after the existing `Spear debug draw toggles are wired` registration in `tests/WeaponEffects.Tests/Program.cs`:

```csharp
("Spear throw charge rejects sub-minimum release", SpearThrowChargeRejectsSubMinimumRelease),
("Spear throw damage scales linearly", SpearThrowDamageScalesLinearly),
("Spear throw range scales linearly by screen width", SpearThrowRangeScalesLinearlyByScreenWidth),
("Spear throw attack speed compresses only full charge", SpearThrowAttackSpeedCompressesOnlyFullCharge)
```

Add these test methods before `ReadPng`:

```csharp
static void SpearThrowChargeRejectsSubMinimumRelease()
{
	AssertEqual(60, SpearThrowChargeMath.MinimumChargeFrames);
	AssertTrue(!SpearThrowChargeMath.IsChargeValid(59), "59 frames should cancel the throw");
	AssertTrue(SpearThrowChargeMath.IsChargeValid(60), "60 frames should be the first valid release frame");
}

static void SpearThrowDamageScalesLinearly()
{
	int fullChargeFrames = SpearThrowChargeMath.BaseFullChargeFrames;
	AssertApproximately(0f, SpearThrowChargeMath.ChargeProgress(60, fullChargeFrames), 0.0001f);
	AssertApproximately(0.5f, SpearThrowChargeMath.ChargeProgress(180, fullChargeFrames), 0.0001f);
	AssertApproximately(1f, SpearThrowChargeMath.ChargeProgress(300, fullChargeFrames), 0.0001f);
	AssertApproximately(1f, SpearThrowChargeMath.DamageMultiplier(0f), 0.0001f);
	AssertApproximately(5.5f, SpearThrowChargeMath.DamageMultiplier(0.5f), 0.0001f);
	AssertApproximately(10f, SpearThrowChargeMath.DamageMultiplier(1f), 0.0001f);
}

static void SpearThrowRangeScalesLinearlyByScreenWidth()
{
	const float screenWidth = 1920f;
	AssertApproximately(2880f, SpearThrowChargeMath.TravelDistancePixels(0f, screenWidth), 0.001f);
	AssertApproximately(6240f, SpearThrowChargeMath.TravelDistancePixels(0.5f, screenWidth), 0.001f);
	AssertApproximately(9600f, SpearThrowChargeMath.TravelDistancePixels(1f, screenWidth), 0.001f);
}

static void SpearThrowAttackSpeedCompressesOnlyFullCharge()
{
	AssertEqual(300, SpearThrowChargeMath.EffectiveFullChargeFrames(1f));
	AssertEqual(150, SpearThrowChargeMath.EffectiveFullChargeFrames(2f));
	AssertEqual(120, SpearThrowChargeMath.EffectiveFullChargeFrames(4f));
	AssertEqual(60, SpearThrowChargeMath.MinimumChargeFrames);
}
```

Add this assertion helper near `AssertEqual`:

```csharp
static void AssertApproximately(float expected, float actual, float tolerance)
{
	if (MathF.Abs(expected - actual) > tolerance)
	{
		throw new InvalidOperationException($"expected {expected}, got {actual}");
	}
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
```

Expected: build fails with missing `SpearThrowChargeMath`.

- [ ] **Step 3: Add pure charge math implementation**

Create `Core/Spears/SpearThrowChargeMath.cs`:

```csharp
using System;

namespace WeaponEffects.Spears;

public static class SpearThrowChargeMath
{
	public const int MinimumChargeFrames = 60;
	public const int BaseFullChargeFrames = 300;
	public const int MinimumFullChargeFrames = 120;
	public const float MinimumDamageMultiplier = 1f;
	public const float MaximumDamageMultiplier = 10f;
	public const float MinimumScreenRange = 1.5f;
	public const float MaximumScreenRange = 5f;
	public const float ThrowSpeed = 42f;
	public const float MinimumVisualWidth = 18f;
	public const float MaximumVisualWidth = 34f;
	public const float MinimumCollisionWidth = 34f;
	public const float MaximumCollisionWidth = 58f;

	public static bool IsChargeValid(int chargeFrames)
	{
		return chargeFrames >= MinimumChargeFrames;
	}

	public static int EffectiveFullChargeFrames(float meleeAttackSpeed)
	{
		float speed = Math.Clamp(meleeAttackSpeed, 0.25f, 4f);
		int scaled = (int)MathF.Round(BaseFullChargeFrames / speed);
		return Math.Max(MinimumFullChargeFrames, scaled);
	}

	public static float ChargeProgress(int chargeFrames, int fullChargeFrames)
	{
		int clampedFull = Math.Max(MinimumChargeFrames + 1, fullChargeFrames);
		float progress = (chargeFrames - MinimumChargeFrames) / (float)(clampedFull - MinimumChargeFrames);
		return Math.Clamp(progress, 0f, 1f);
	}

	public static float DamageMultiplier(float chargeProgress)
	{
		return Lerp(MinimumDamageMultiplier, MaximumDamageMultiplier, Math.Clamp(chargeProgress, 0f, 1f));
	}

	public static float TravelDistancePixels(float chargeProgress, float screenWidth)
	{
		float width = Math.Max(1f, screenWidth);
		float screens = Lerp(MinimumScreenRange, MaximumScreenRange, Math.Clamp(chargeProgress, 0f, 1f));
		return width * screens;
	}

	public static float VisualWidth(float chargeProgress)
	{
		return Lerp(MinimumVisualWidth, MaximumVisualWidth, Math.Clamp(chargeProgress, 0f, 1f));
	}

	public static float CollisionWidth(float chargeProgress)
	{
		return Lerp(MinimumCollisionWidth, MaximumCollisionWidth, Math.Clamp(chargeProgress, 0f, 1f));
	}

	private static float Lerp(float start, float end, float amount)
	{
		return start + (end - start) * amount;
	}
}
```

- [ ] **Step 4: Run pure tests and verify they pass**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
```

Expected: all tests pass, including the four new spear throw math tests.

- [ ] **Step 5: Commit charge math**

```powershell
git add Core\Spears\SpearThrowChargeMath.cs tests\WeaponEffects.Tests\Program.cs
git commit -m "feat: add spear throw charge math"
```

---

### Task 2: Player State And Item Entry

**Files:**
- Modify: `Common/Players/WeaponEffectsPlayer.cs`
- Modify: `Content/Items/SpearGlobalItem.cs`
- Create: `Content/Projectiles/SpearThrowChargeProjectile.cs`
- Modify: `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`
- Modify: `tests/WeaponEffects.Tests/Program.cs`

- [ ] **Step 1: Add source-wiring tests**

In `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`, add these `None` entries:

```xml
<None Include="..\..\Content\Items\SpearGlobalItem.cs" Link="Content\Items\SpearGlobalItem.cs" CopyToOutputDirectory="PreserveNewest" />
<None Include="..\..\Common\Players\WeaponEffectsPlayer.cs" Link="Common\Players\WeaponEffectsPlayer.cs" CopyToOutputDirectory="PreserveNewest" />
```

In `tests/WeaponEffects.Tests/Program.cs`, add registrations:

```csharp
("Spear throw right-click item entry is wired", SpearThrowRightClickItemEntryIsWired),
("Spear combo reset method is exposed", SpearComboResetMethodIsExposed)
```

Add methods:

```csharp
static void SpearThrowRightClickItemEntryIsWired()
{
	string itemPath = Path.Combine(AppContext.BaseDirectory, "Content", "Items", "SpearGlobalItem.cs");
	string source = File.ReadAllText(itemPath);

	AssertTrue(source.Contains("public override bool AltFunctionUse(Item item, Player player)"), "SpearGlobalItem must expose right-click use");
	AssertTrue(source.Contains("player.altFunctionUse == 2"), "SpearGlobalItem.UseItem must branch right-click use");
	AssertTrue(source.Contains("StartSpearThrowCharge(item, player);"), "right-click use must start the spear throw charge controller");
	AssertTrue(source.Contains("KillOwnedSpearChannels(player);"), "right-click charge must interrupt active spear channels");
	AssertTrue(source.Contains("ResetSpearCombo()"), "right-click charge must reset the spear combo");
}

static void SpearComboResetMethodIsExposed()
{
	string playerPath = Path.Combine(AppContext.BaseDirectory, "Common", "Players", "WeaponEffectsPlayer.cs");
	string source = File.ReadAllText(playerPath);

	AssertTrue(source.Contains("public void ResetSpearCombo()"), "WeaponEffectsPlayer should expose a spear combo reset method");
	AssertTrue(source.Contains("SpearGlobalItem.TryStartThrowChargeInterrupt(Player);"), "local player update should allow right-click interruption during active spear channels");
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
```

Expected: source-wiring tests fail because right-click entry and reset method are not present.

- [ ] **Step 3: Add spear combo reset method**

In `Common/Players/WeaponEffectsPlayer.cs`, add this method immediately after `ConsumeNextSpearComboStep()`:

```csharp
public void ResetSpearCombo()
{
	SpearComboStepIndex = 0;
	_spearComboResetTimer = 0;
}
```

In `PostUpdate()`, after `SlashGlobalItem.TryStartChargeInterrupt(Player);`, add:

```csharp
SpearGlobalItem.TryStartThrowChargeInterrupt(Player);
```

- [ ] **Step 4: Add right-click entry in `SpearGlobalItem`**

In `Content/Items/SpearGlobalItem.cs`, add:

```csharp
public override bool AltFunctionUse(Item item, Player player)
{
	return _usesSpearAction;
}
```

Replace the local-player part of `UseItem` with:

```csharp
if (player.altFunctionUse == 2)
{
	StartSpearThrowCharge(item, player);
	return true;
}

StartSpearAction(item, player);
return true;
```

Add these helper methods before `StartSpearAction`:

```csharp
internal static bool TryStartThrowChargeInterrupt(Player player)
{
	if (!CanStartThrowChargeFromInput(player))
	{
		return false;
	}

	Item item = player.HeldItem;
	if (!ShouldUseSpearAction(item) || HasOwnedProjectile(player, ModContent.ProjectileType<SpearThrowChargeProjectile>()))
	{
		return false;
	}

	bool interruptingSpearChannel = HasOwnedProjectile(player, ModContent.ProjectileType<SpearChannelProjectile>());
	if (!interruptingSpearChannel && player.itemAnimation <= 0 && player.itemTime <= 0)
	{
		return false;
	}

	StartSpearThrowCharge(item, player);
	return true;
}

private static void StartSpearThrowCharge(Item item, Player player)
{
	KillOwnedSpearChannels(player);
	player.GetModPlayer<WeaponEffectsPlayer>().ResetSpearCombo();
	player.itemAnimation = 0;
	player.itemTime = 0;

	float weaponLength = GetWeaponLength(item);
	Vector2 targetWorld = Main.MouseWorld;

	Projectile projectile = Projectile.NewProjectileDirect(
		player.GetSource_ItemUse(item),
		player.Center,
		Vector2.Zero,
		ModContent.ProjectileType<SpearThrowChargeProjectile>(),
		player.GetWeaponDamage(item),
		player.GetWeaponKnockback(item),
		player.whoAmI,
		player.direction);

	if (projectile.ModProjectile is SpearThrowChargeProjectile charge)
	{
		charge.Initialize(item.type, item.useAnimation, weaponLength, targetWorld);
	}

	MeleeEffectAssets.SyncProjectile(projectile);
}
```

Add these helpers near the bottom of `SpearGlobalItem`:

```csharp
private static bool CanStartThrowChargeFromInput(Player player)
{
	if (player.whoAmI != Main.myPlayer || !Main.mouseRight)
	{
		return false;
	}

	if (!player.active || player.dead || player.noItems || player.CCed)
	{
		return false;
	}

	if (player.mouseInterface || Main.playerInventory || Main.mapFullscreen || Main.drawingPlayerChat || Main.editSign || Main.editChest || Main.blockInput)
	{
		return false;
	}

	Item item = player.HeldItem;
	return item != null && !item.IsAir && ShouldUseSpearAction(item);
}

private static bool HasOwnedProjectile(Player player, int projectileType)
{
	for (int i = 0; i < Main.maxProjectiles; i++)
	{
		Projectile projectile = Main.projectile[i];
		if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
		{
			return true;
		}
	}

	return false;
}

private static void KillOwnedSpearChannels(Player player)
{
	int spearChannelType = ModContent.ProjectileType<SpearChannelProjectile>();
	for (int i = 0; i < Main.maxProjectiles; i++)
	{
		Projectile projectile = Main.projectile[i];
		if (projectile.active && projectile.owner == player.whoAmI && projectile.type == spearChannelType)
		{
			projectile.Kill();
		}
	}
}
```

- [ ] **Step 5: Run source tests and note expected compile status**

Create a temporary compile-safe charge controller shell at `Content/Projectiles/SpearThrowChargeProjectile.cs`. This shell is replaced with the real controller in Task 3:

```csharp
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SpearThrowChargeProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = 2;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
	}
}
```

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
dotnet build WeaponEffects.csproj -v:minimal
```

Expected: tests pass and the main mod project builds with the temporary charge-controller shell.

- [ ] **Step 6: Commit item entry and player state**

```powershell
git add Common\Players\WeaponEffectsPlayer.cs Content\Items\SpearGlobalItem.cs Content\Projectiles\SpearThrowChargeProjectile.cs tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj tests\WeaponEffects.Tests\Program.cs
git commit -m "feat: wire spear throw right-click entry"
```

---

### Task 3: Charge Controller Projectile

**Files:**
- Modify: `Content/Projectiles/SpearThrowChargeProjectile.cs`
- Create: `Content/Projectiles/SpearThrowProjectile.cs`
- Modify: `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`
- Modify: `tests/WeaponEffects.Tests/Program.cs`

- [ ] **Step 1: Add source tests for charge controller behavior**

In `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`, add:

```xml
<None Include="..\..\Content\Projectiles\SpearThrowChargeProjectile.cs" Link="Content\Projectiles\SpearThrowChargeProjectile.cs" CopyToOutputDirectory="PreserveNewest" />
```

In `tests/WeaponEffects.Tests/Program.cs`, add registration:

```csharp
("Spear throw charge projectile follows cancellation and release rules", SpearThrowChargeProjectileFollowsCancellationAndReleaseRules)
```

Add method:

```csharp
static void SpearThrowChargeProjectileFollowsCancellationAndReleaseRules()
{
	string path = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearThrowChargeProjectile.cs");
	string source = File.ReadAllText(path);

	AssertTrue(source.Contains("SpearThrowChargeMath.IsChargeValid(_chargeFrames)"), "charge projectile must cancel releases below the 1 second minimum");
	AssertTrue(source.Contains("SpearThrowProjectile.Spawn("), "valid releases must spawn the thrown spear-light");
	AssertTrue(source.Contains("Main.mouseRight"), "charge should hold while right click remains down");
	AssertTrue(source.Contains("EmitFullChargeBurst(player);"), "full charge should emit a burst once");
	AssertTrue(source.Contains("DrawChargingSpear("), "charge projectile should draw the held pre-throw spear");
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
```

Expected: the new source test fails because the charge-controller shell does not contain release, cancellation, full-charge, or draw logic.

- [ ] **Step 3: Replace the charge-controller shell**

Replace all contents of `Content/Projectiles/SpearThrowChargeProjectile.cs`:

```csharp
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;

namespace WeaponEffects;

public class SpearThrowChargeProjectile : ModProjectile
{
	private const int AimSyncInterval = 6;
	private const float AimSyncThreshold = 0.03f;

	private int _weaponItemType;
	private int _useAnimation;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;
	private float _lastSyncedAimRotation;
	private int _chargeFrames;
	private int _effectiveFullChargeFrames;
	private bool _released;
	private bool _fullChargeBurstEmitted;

	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		_weaponItemType = weaponItemType;
		_useAnimation = Math.Max(1, useAnimation);
		_weaponLength = Math.Max(1f, weaponLength);
		_targetWorld = targetWorld;
		_aimRotation = (targetWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Math.Sign(Projectile.ai[0])).ToRotation();
		_lastSyncedAimRotation = _aimRotation;
		_effectiveFullChargeFrames = SpearThrowChargeMath.BaseFullChargeFrames;
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = 100;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_weaponItemType);
		writer.Write(_useAnimation);
		writer.Write(_weaponLength);
		writer.Write(_targetWorld.X);
		writer.Write(_targetWorld.Y);
		writer.Write(_aimRotation);
		writer.Write(_chargeFrames);
		writer.Write(_effectiveFullChargeFrames);
		writer.Write(_fullChargeBurstEmitted);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_useAnimation = reader.ReadInt32();
		_weaponLength = reader.ReadSingle();
		_targetWorld = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		_aimRotation = reader.ReadSingle();
		_chargeFrames = reader.ReadInt32();
		_effectiveFullChargeFrames = reader.ReadInt32();
		_fullChargeBurstEmitted = reader.ReadBoolean();
		_lastSyncedAimRotation = _aimRotation;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override void AI()
	{
		Player player = Main.player[Projectile.owner];
		if (!CanContinueCharge(player))
		{
			Projectile.Kill();
			return;
		}

		if (Projectile.owner == Main.myPlayer)
		{
			UpdateLocalAim(player);
			_effectiveFullChargeFrames = SpearThrowChargeMath.EffectiveFullChargeFrames(player.GetAttackSpeed(DamageClass.Melee));
		}

		Projectile.Center = player.Center;
		Projectile.velocity = Vector2.Zero;
		Projectile.rotation = _aimRotation;
		Projectile.timeLeft = 2;
		player.heldProj = Projectile.whoAmI;
		player.itemAnimation = 3;
		player.itemTime = 3;
		Vector2 aimDirection = _aimRotation.ToRotationVector2();
		player.direction = aimDirection.X >= 0f ? 1 : -1;
		player.itemRotation = player.direction > 0 ? _aimRotation : _aimRotation + MathHelper.Pi;

		if (!IsHoldingCharge())
		{
			if (Projectile.owner == Main.myPlayer)
			{
				ReleaseOrCancel(player);
			}

			Projectile.Kill();
			return;
		}

		_chargeFrames++;
		EmitChargingParticles(player);

		if (ChargeProgress >= 1f && !_fullChargeBurstEmitted)
		{
			_fullChargeBurstEmitted = true;
			EmitFullChargeBurst(player);
			Projectile.netUpdate = true;
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (_released || Projectile.owner != Main.myPlayer)
		{
			return;
		}

		Player player = Main.player[Projectile.owner];
		if (player.active && !player.dead && !IsHoldingCharge())
		{
			ReleaseOrCancel(player);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (Main.dedServ)
		{
			return false;
		}

		Texture2D weaponTexture = GetWeaponTexture();
		if (weaponTexture != null && ModContent.GetInstance<WeaponEffectsVisualConfig>().DrawSpearHeldWeapon)
		{
			DrawChargingSpear(weaponTexture, lightColor);
		}

		return false;
	}

	private float ChargeProgress => SpearThrowChargeMath.ChargeProgress(_chargeFrames, _effectiveFullChargeFrames);

	private bool IsHoldingCharge()
	{
		return Projectile.owner != Main.myPlayer || Main.mouseRight;
	}

	private bool CanContinueCharge(Player player)
	{
		if (!player.active || player.dead || player.noItems || player.CCed)
		{
			return false;
		}

		Item heldItem = player.HeldItem;
		return heldItem != null && !heldItem.IsAir && heldItem.type == _weaponItemType;
	}

	private void ReleaseOrCancel(Player player)
	{
		if (_released)
		{
			return;
		}

		_released = true;
		if (!SpearThrowChargeMath.IsChargeValid(_chargeFrames))
		{
			return;
		}

		float progress = ChargeProgress;
		int damage = Math.Max(1, (int)MathF.Round(Projectile.damage * SpearThrowChargeMath.DamageMultiplier(progress)));
		float distance = SpearThrowChargeMath.TravelDistancePixels(progress, Main.screenWidth);
		float visualWidth = SpearThrowChargeMath.VisualWidth(progress);
		float collisionWidth = SpearThrowChargeMath.CollisionWidth(progress);
		SoundStyle releaseSound = new("WeaponEffects/Sounds/Slashing") { Volume = 0.65f };
		MeleeEffectAssets.PlaySound(in releaseSound, player.Center);

		SpearThrowProjectile.Spawn(
			Projectile.GetSource_FromAI(),
			player.Center,
			player.whoAmI,
			_weaponItemType,
			_aimRotation,
			distance,
			visualWidth,
			collisionWidth,
			progress,
			damage,
			Projectile.knockBack);
	}

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 direction = (_targetWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
		_aimRotation = direction.ToRotation();
		player.direction = direction.X >= 0f ? 1 : -1;

		float aimDelta = Math.Abs(MathHelper.WrapAngle(_aimRotation - _lastSyncedAimRotation));
		if (aimDelta >= AimSyncThreshold || _chargeFrames % AimSyncInterval == 0)
		{
			_lastSyncedAimRotation = _aimRotation;
			Projectile.netUpdate = true;
		}
	}

	private void DrawChargingSpear(Texture2D weaponTexture, Color lightColor)
	{
		Player player = Main.player[Projectile.owner];
		Vector2 direction = _aimRotation.ToRotationVector2();
		Vector2 grip = player.Center - direction * 34f + new Vector2(0f, 8f);
		Vector2 tip = player.Center + direction * MathHelper.Clamp(_weaponLength * 0.72f, 72f, 150f);
		Vector2 shaft = tip - grip;
		float rotation = shaft.ToRotation();
		float heat = ChargeProgress;
		float pulse = heat >= 1f ? 0.75f + 0.25f * (float)Math.Sin(_chargeFrames * 0.28f) : 1f;
		Color heated = Color.Lerp(lightColor, new Color(255, 138, 38), MathHelper.Clamp(heat * 1.4f, 0f, 1f));
		Color final = Color.Lerp(heated, new Color(255, 238, 110), MathHelper.Clamp((heat - 0.55f) / 0.45f, 0f, 1f)) * pulse;
		Vector2 origin = new(weaponTexture.Width * 0.1f, weaponTexture.Height * 0.9f);
		Vector2 textureTip = new(weaponTexture.Width, 0f);
		float textureLength = Math.Max(1f, (textureTip - origin).Length());
		float scale = MathHelper.Clamp(shaft.Length() / textureLength, 0.75f, 1.2f);

		if (heat > 0f)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 offset = (MathHelper.PiOver2 * i + _chargeFrames * 0.05f).ToRotationVector2() * MathHelper.Lerp(1.5f, 4f, heat);
				Main.EntitySpriteDraw(weaponTexture, grip + offset - Main.screenPosition, null, new Color(255, 190, 55) * (0.18f + heat * 0.28f), rotation - (textureTip - origin).ToRotation(), origin, scale, SpriteEffects.None, 0f);
			}
		}

		Main.EntitySpriteDraw(weaponTexture, grip - Main.screenPosition, null, final, rotation - (textureTip - origin).ToRotation(), origin, scale, SpriteEffects.None, 0f);
	}

	private void EmitChargingParticles(Player player)
	{
		if (Main.dedServ || _chargeFrames % 5 != 0 || MeleeEffectAssets.ParticleDensityMultiplier <= 0f)
		{
			return;
		}

		float progress = ChargeProgress;
		Vector2 direction = _aimRotation.ToRotationVector2();
		Vector2 basePosition = player.Center + direction * Main.rand.NextFloat(18f, 62f) + Main.rand.NextVector2Circular(8f, 8f);
		Dust dust = Dust.NewDustDirect(basePosition, 1, 1, DustID.GemTopaz, 0f, 0f, 0, new Color(255, 198, 64), Main.rand.NextFloat(0.65f, 1.25f + progress * 0.8f));
		dust.noGravity = true;
		dust.velocity = -direction * Main.rand.NextFloat(0.2f, 1.1f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
	}

	private void EmitFullChargeBurst(Player player)
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(72);
		for (int i = 0; i < count; i++)
		{
			Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
			Dust dust = Dust.NewDustDirect(player.Center + Main.rand.NextVector2Circular(12f, 12f), 1, 1, DustID.GemTopaz, direction.X * Main.rand.NextFloat(2.5f, 8f), direction.Y * Main.rand.NextFloat(2.5f, 8f), 0, new Color(255, 221, 93), Main.rand.NextFloat(1f, 1.8f));
			dust.noGravity = true;
		}
	}

	private Texture2D GetWeaponTexture()
	{
		if (_weaponItemType <= 0 || _weaponItemType >= TextureAssets.Item.Length)
		{
			return null;
		}

		return TextureAssets.Item[_weaponItemType].Value;
	}
}
```

- [ ] **Step 4: Create a temporary thrown-projectile shell**

Create `Content/Projectiles/SpearThrowProjectile.cs`. This shell is replaced with the real projectile in Task 4:

```csharp
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SpearThrowProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Extra_" + ExtrasID.SharpTears;

	public static void Spawn(
		IEntitySource source,
		Vector2 position,
		int owner,
		int weaponItemType,
		float aimRotation,
		float maxTravelDistance,
		float visualWidth,
		float collisionWidth,
		float chargeProgress,
		int damage,
		float knockback)
	{
		Projectile projectile = Projectile.NewProjectileDirect(
			source,
			position,
			aimRotation.ToRotationVector2() * 42f,
			ModContent.ProjectileType<SpearThrowProjectile>(),
			damage,
			knockback,
			owner);

		projectile.rotation = aimRotation;
		MeleeEffectAssets.SyncProjectile(projectile);
	}

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 120;
	}
}
```

- [ ] **Step 5: Run source tests and build**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
dotnet build WeaponEffects.csproj -v:minimal
```

Expected: source tests pass for the charge controller and the main mod project builds with the temporary thrown-projectile shell.

- [ ] **Step 6: Commit charge controller**

```powershell
git add Content\Projectiles\SpearThrowChargeProjectile.cs Content\Projectiles\SpearThrowProjectile.cs tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj tests\WeaponEffects.Tests\Program.cs
git commit -m "feat: add spear throw charge controller"
```

---

### Task 4: Thrown Spear-Light Projectile

**Files:**
- Modify: `Content/Projectiles/SpearThrowProjectile.cs`
- Modify: `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`
- Modify: `tests/WeaponEffects.Tests/Program.cs`

- [ ] **Step 1: Add source tests for thrown projectile rules**

In `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`, add:

```xml
<None Include="..\..\Content\Projectiles\SpearThrowProjectile.cs" Link="Content\Projectiles\SpearThrowProjectile.cs" CopyToOutputDirectory="PreserveNewest" />
```

In `tests/WeaponEffects.Tests/Program.cs`, add registration:

```csharp
("Spear throw projectile implements piercing wall-pass rules", SpearThrowProjectileImplementsPiercingWallPassRules)
```

Add method:

```csharp
static void SpearThrowProjectileImplementsPiercingWallPassRules()
{
	string path = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearThrowProjectile.cs");
	string source = File.ReadAllText(path);

	AssertTrue(source.Contains("Projectile.tileCollide = false;"), "thrown spear-light must pass through walls");
	AssertTrue(source.Contains("Projectile.penetrate = -1;"), "thrown spear-light must pierce multiple enemies");
	AssertTrue(source.Contains("private readonly bool[] _hitNpcs = new bool[Main.maxNPCs];"), "thrown spear-light must track one hit per NPC");
	AssertTrue(source.Contains("return target != null && !_hitNpcs[target.whoAmI];"), "CanHitNPC must prevent repeat hits per throw");
	AssertTrue(source.Contains("_hitNpcs[target.whoAmI] = true;"), "OnHitNPC must mark the NPC as hit");
	AssertTrue(source.Contains("DrawSpindle("), "PreDraw must use the spindle light visual");
	AssertTrue(!source.Contains("SlashArcProjectile"), "spear throw must not route damage through SlashArcProjectile");
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
```

Expected: source test fails because the thrown-projectile shell does not contain one-hit-per-NPC state, custom collision, or spindle drawing.

- [ ] **Step 3: Replace the thrown-projectile shell**

Replace all contents of `Content/Projectiles/SpearThrowProjectile.cs`:

```csharp
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;

namespace WeaponEffects;

public class SpearThrowProjectile : ModProjectile
{
	private readonly bool[] _hitNpcs = new bool[Main.maxNPCs];
	private int _weaponItemType;
	private float _aimRotation;
	private float _maxTravelDistance;
	private float _visualWidth;
	private float _collisionWidth;
	private float _chargeProgress;
	private float _distanceTravelled;

	public override string Texture => "Terraria/Images/Extra_" + ExtrasID.SharpTears;

	public static void Spawn(
		IEntitySource source,
		Vector2 position,
		int owner,
		int weaponItemType,
		float aimRotation,
		float maxTravelDistance,
		float visualWidth,
		float collisionWidth,
		float chargeProgress,
		int damage,
		float knockback)
	{
		Vector2 velocity = aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed;
		Projectile projectile = Projectile.NewProjectileDirect(
			source,
			position,
			velocity,
			ModContent.ProjectileType<SpearThrowProjectile>(),
			damage,
			knockback,
			owner);

		if (projectile.ModProjectile is SpearThrowProjectile spearThrow)
		{
			spearThrow.Initialize(weaponItemType, aimRotation, maxTravelDistance, visualWidth, collisionWidth, chargeProgress);
		}

		MeleeEffectAssets.SyncProjectile(projectile);
	}

	public void Initialize(int weaponItemType, float aimRotation, float maxTravelDistance, float visualWidth, float collisionWidth, float chargeProgress)
	{
		_weaponItemType = weaponItemType;
		_aimRotation = aimRotation;
		_maxTravelDistance = Math.Max(1f, maxTravelDistance);
		_visualWidth = Math.Max(1f, visualWidth);
		_collisionWidth = Math.Max(1f, collisionWidth);
		_chargeProgress = MathHelper.Clamp(chargeProgress, 0f, 1f);
		Projectile.rotation = aimRotation;
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 300;
		Projectile.extraUpdates = 1;
		Projectile.aiStyle = -1;
		Projectile.noEnchantmentVisuals = true;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_weaponItemType);
		writer.Write(_aimRotation);
		writer.Write(_maxTravelDistance);
		writer.Write(_visualWidth);
		writer.Write(_collisionWidth);
		writer.Write(_chargeProgress);
		writer.Write(_distanceTravelled);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_aimRotation = reader.ReadSingle();
		_maxTravelDistance = reader.ReadSingle();
		_visualWidth = reader.ReadSingle();
		_collisionWidth = reader.ReadSingle();
		_chargeProgress = reader.ReadSingle();
		_distanceTravelled = reader.ReadSingle();
		Projectile.rotation = _aimRotation;
		Projectile.velocity = _aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed;
	}

	public override void AI()
	{
		Projectile.rotation = _aimRotation;
		Projectile.velocity = _aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed;
		_distanceTravelled += Projectile.velocity.Length();
		Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.72f, 0.18f) * (0.45f + _chargeProgress * 0.45f));

		if (_distanceTravelled >= _maxTravelDistance)
		{
			Projectile.Kill();
		}
	}

	public override bool? CanHitNPC(NPC target)
	{
		return target != null && !_hitNpcs[target.whoAmI];
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		Vector2 direction = _aimRotation.ToRotationVector2();
		Vector2 start = Projectile.Center - direction * 52f;
		Vector2 end = Projectile.Center + direction * 70f;
		float collisionPoint = 0f;
		return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, _collisionWidth, ref collisionPoint);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		_hitNpcs[target.whoAmI] = true;
		SoundStyle hitSound = new("WeaponEffects/Sounds/Onhit")
		{
			Volume = 0.36f,
			Pitch = Main.rand.NextFloat(-0.08f, 0.12f)
		};
		MeleeEffectAssets.PlaySound(in hitSound, target.Center);
		SpawnGoldHitDust(target);

		if (ModContent.GetInstance<WeaponEffectsVisualConfig>().DrawSpearHitFlash)
		{
			MeleeEffectAssets.NewProjectileDirect(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<SlashHitEffectProjectile>(), 0, 0f, Projectile.owner, _aimRotation);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		DrawSpindle();
		return false;
	}

	private void DrawSpindle()
	{
		Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		Vector2 origin = texture.Size() * 0.5f;
		float length = MathHelper.Lerp(96f, 172f, _chargeProgress);
		float width = _visualWidth;
		Color outer = new Color(255, 176, 30, 0) * 0.58f;
		Color inner = new Color(255, 246, 164, 0) * 0.92f;
		float rotation = _aimRotation + MathHelper.PiOver2;
		Vector2 scaleOuter = new(width / texture.Width, length / texture.Height);
		Vector2 scaleInner = new((width * 0.42f) / texture.Width, (length * 0.82f) / texture.Height);

		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, outer, rotation, origin, scaleOuter, SpriteEffects.None, 0f);
		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, inner, rotation, origin, scaleInner, SpriteEffects.None, 0f);
	}

	private void SpawnGoldHitDust(NPC target)
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(22, 0.9f + _chargeProgress * 0.9f);
		Vector2 tangent = _aimRotation.ToRotationVector2();
		for (int i = 0; i < count; i++)
		{
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GemTopaz, 0f, 0f, 0, new Color(255, 214, 91), Main.rand.NextFloat(0.8f, 1.7f));
			dust.noGravity = true;
			dust.velocity = (tangent + Main.rand.NextVector2Circular(0.65f, 0.65f)).SafeNormalize(tangent) * Main.rand.NextFloat(1.2f, 7.5f);
		}
	}
}
```

- [ ] **Step 4: Run tests and build**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
dotnet build WeaponEffects.csproj -v:minimal
```

Expected: tests pass and the main mod project builds.

- [ ] **Step 5: Commit thrown projectile**

```powershell
git add Content\Projectiles\SpearThrowProjectile.cs tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj tests\WeaponEffects.Tests\Program.cs
git commit -m "feat: add spear throw projectile"
```

---

### Task 5: Integration Verification And Tuning Pass

**Files:**
- Modify: `Content/Projectiles/SpearThrowChargeProjectile.cs`
- Modify: `Content/Projectiles/SpearThrowProjectile.cs`
- Modify: `Content/Items/SpearGlobalItem.cs`
- Modify: `tests/WeaponEffects.Tests/Program.cs`

- [ ] **Step 1: Add final isolation tests**

In `tests/WeaponEffects.Tests/Program.cs`, add registration:

```csharp
("Spear throw remains isolated from sword slash projectile", SpearThrowRemainsIsolatedFromSwordSlashProjectile)
```

Add method:

```csharp
static void SpearThrowRemainsIsolatedFromSwordSlashProjectile()
{
	string chargePath = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearThrowChargeProjectile.cs");
	string throwPath = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearThrowProjectile.cs");
	string itemPath = Path.Combine(AppContext.BaseDirectory, "Content", "Items", "SpearGlobalItem.cs");
	string combined = File.ReadAllText(chargePath) + File.ReadAllText(throwPath) + File.ReadAllText(itemPath);

	AssertTrue(!combined.Contains("ChargedSlashProjectile"), "spear throw should not reuse sword charge projectile");
	AssertTrue(!combined.Contains("SlashArcProjectile"), "spear throw should not reuse sword slash projectile");
	AssertTrue(combined.Contains("SpearThrowChargeMath.MinimumChargeFrames"), "spear throw should use the pure charge constants");
}
```

- [ ] **Step 2: Run full verification**

Run:

```powershell
dotnet run --project tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
dotnet build WeaponEffects.csproj -v:minimal
rg -n "SlashArcProjectile|ChargedSlashProjectile" Content\Items\SpearGlobalItem.cs Content\Projectiles\SpearThrowChargeProjectile.cs Content\Projectiles\SpearThrowProjectile.cs
git status --short --untracked-files=all
```

Expected:

- Tests pass.
- Build passes.
- `rg` returns no matches.
- `git status` shows only files intentionally changed for the current task.

- [ ] **Step 3: Manual in-game verification checklist**

Run the mod in tModLoader and verify:

- Trident right click starts the backward pre-throw charge pose.
- Right-click charge interrupts active left-click spear combo.
- Starting right-click charge resets the next left-click attack to combo step 1.
- Releasing before 1 second cancels with no light projectile.
- Releasing after 1 second fires a golden spindle-shaped projectile.
- Full charge emits a clear gold burst and pulse.
- The projectile passes through blocks.
- The projectile can hit multiple NPCs.
- The same NPC is not damaged twice by one throw.
- Player movement and jumping remain normal during charge.
- Existing sword charge still works on sword-profile weapons.

- [ ] **Step 4: Commit integration verification changes**

```powershell
git add Content\Items\SpearGlobalItem.cs Content\Projectiles\SpearThrowChargeProjectile.cs Content\Projectiles\SpearThrowProjectile.cs Common\Players\WeaponEffectsPlayer.cs Core\Spears\SpearThrowChargeMath.cs tests\WeaponEffects.Tests\Program.cs tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj
git commit -m "test: verify spear throw integration"
```

---

## Self-Review Notes

- Spec coverage: right-click input, interruption, reset, cancellation, linear damage, linear distance, fixed speed, wall piercing, multi-target piercing, one hit per NPC, visuals, networking fields, and isolation are covered by tasks.
- No config entries are planned for V1, matching the spec.
- The plan keeps the throw independent of `SlashArcProjectile` and `ChargedSlashProjectile`.
- The implementation steps use test-first source/logic checks where pure tModLoader runtime tests are not practical.
