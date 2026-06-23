# Trident V1 Combo Effects Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Implement a first playable `ItemID.Trident` spear combo with spear-specific motion, collision, trails, grounded/airborne fourth-step branching, and reused hit feedback.

**Architecture:** A pure spear-motion layer defines combo data and deterministic pose evaluation. `SpearGlobalItem` gates only Trident and starts `SpearChannelProjectile`; `SpearStrikeProjectile` owns real damage and capsule-style collision; `SpearTrailGlowProjectile` owns shaft and tip visuals.

**Tech Stack:** C# latest, .NET 8 pure logic tests, tModLoader ModItem/ModProjectile APIs, XNA/FNA drawing primitives, existing `MeleeEffectAssets` helpers.

---

## Execution Notes

This plan was executed in the current workspace because `Assets/Textures/SpearTipTrail.png` was already present and required by the feature. The user's long-spear reference material under `docs/长矛/` remains untouched and untracked.

The pure logic test project lives under `tests/`, so the main mod project excludes `tests/**/*.cs` from compilation. This keeps test `Program.cs` out of the tModLoader mod build.

## Completed Tasks

- [x] **Task 1: Pure spear combo data and motion**
  - Added `Core/Spears/SpearComboBranch.cs`.
  - Added `Core/Spears/SpearComboStepKind.cs`.
  - Added `Core/Spears/SpearComboStep.cs`.
  - Added `Core/Spears/SpearPoseSnapshot.cs`.
  - Added `Core/Spears/SpearMotion.cs`.
  - Added `Core/Spears/TridentSpearComboScheme.cs`.
  - Added `tests/WeaponEffects.Tests/WeaponEffects.Tests.csproj`.
  - Added `tests/WeaponEffects.Tests/Program.cs`.

- [x] **Task 2: Player spear combo state and assets**
  - Extended `WeaponEffectsPlayer` with spear combo index and reset timer.
  - Added `MeleeEffectAssets.SpearTipTrailTexture`.

- [x] **Task 3: Trident item gate and channel projectile**
  - Added `Content/Items/SpearGlobalItem.cs`.
  - Added `Content/Projectiles/SpearChannelProjectile.cs`.

- [x] **Task 4: Spear strike projectile**
  - Added `Content/Projectiles/SpearStrikeProjectile.cs`.
  - Uses `SpearMotion.EvaluatePose` for spear-specific path and collision.
  - Reuses `SlashHitEffectProjectile` for hit flash.
  - Does not use `SlashArcProjectile`.

- [x] **Task 5: Spear trail glow projectile**
  - Added `Content/Projectiles/SpearTrailGlowProjectile.cs`.
  - Uses `SlashTex.png` for shaft afterimages.
  - Uses `SpearTipTrail.png` for spear-tip trail.

- [x] **Task 6: Integration verification**
  - Pure logic tests pass.
  - Main mod project builds successfully.
  - Source checks confirm Trident gate, strike spawn, tip texture usage, and no `SlashArcProjectile` dependency in spear strike.

## Verification Commands

```powershell
dotnet run --project "tests\WeaponEffects.Tests\WeaponEffects.Tests.csproj"
dotnet build "WeaponEffects.csproj" -v:minimal
Select-String -Path "Content\Projectiles\SpearStrikeProjectile.cs" -Pattern "SlashArcProjectile"
git status --short --untracked-files=all
```
