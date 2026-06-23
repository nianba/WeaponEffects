# Trident V1 Combo Effects Design

## Goal

Build a first playable Trident-only spear combo effect that proves the spear action pipeline works. V1 focuses on motion, collision, and reusable visual effects. It intentionally avoids a full visual theme.

The supported weapon is `ItemID.Trident` only.

## Scope

V1 includes:

- A four-step spear combo for `ItemID.Trident`.
- A spear-specific channel projectile that reads aim, keeps the item in use, and advances combo state.
- A spear strike projectile that owns real damage, hit detection, and hit feedback.
- A visual-only trail/glow projectile for shaft afterimages and tip trails.
- Ground and air variants for the fourth combo step.
- Reuse of the sword hit flash for enemy hits.
- Reuse of `Assets/Textures/SlashTex.png` for shaft afterimages and the air finisher's large semicircle trail.
- Use of `Assets/Textures/SpearTipTrail.png` for spear-tip trails.

V1 excludes:

- Support for all spear weapons.
- Weapon-specific spear profiles beyond Trident.
- Spark dust and charge bar visuals.
- A custom Trident visual theme.
- Multi-hit pierce logic or complex hit pause tuning.
- Reusing the sword fourth-hit finisher bonus as-is.

## Combo Behavior

The combo has four steps. Combo timing should follow the existing sword channel rhythm closely enough that melee speed and use animation still feel familiar, but the visual shapes and hit boxes must be spear-specific.

### Step 1: Forward Thrust

The player thrusts the spear forward in a short, fast line. The active collision is a narrow capsule from the hand/grip area toward the spear tip.

Initial timing:

- Duration: derived from the weapon use animation through the channel interval.
- Active window: 20% to 55% of the strike lifetime.
- Reach: about 1.15x the weapon's measured length.
- Width: narrow, roughly 18 to 24 pixels before profile scaling.

Visuals:

- Draw the held Trident aligned to aim.
- Draw a short `SpearTipTrail.png` streak at the moving tip.
- Optional faint shaft afterimage with `SlashTex.png`, kept thin and line-like.

### Step 2: Rising Lift

The spear tip moves from forward-low to forward-high. The attack reads as a lift or uppercut, not a crescent sword slash.

Initial timing:

- Active window: 22% to 60%.
- Reach: about 1.05x weapon length.
- Width: narrow to medium, roughly 20 to 28 pixels.

Visuals:

- Use `SpearTipTrail.png` along the tip path.
- Keep shaft trail restrained so the weapon still reads as a polearm.

### Step 3: Around-Body Backsweep

The spear moves around the player into a behind-low preparation pose. This is both an attack and the setup for the fourth step.

Hard pose requirement:

- At the end of the step, the spear tip is behind the player, low, and angled slightly downward.

Initial timing:

- Active window: 20% to 58%.
- Reach: about 1.0x weapon length.
- Width: medium, but still collision-aligned to the spear tip and shaft rather than a wide fan.

Visuals:

- `SlashTex.png` may appear as a restrained shaft smear.
- Avoid a broad moon-shaped slash. The player should read the weapon moving around the body, not cutting a sword arc.

### Step 4A: Grounded Advancing Thrust

If the player is grounded when step 4 starts, the branch is locked as the grounded finisher. The spear starts from the behind-low pose and drives forward into a long thrust.

Initial timing:

- Active window: 18% to 62%.
- Reach: about 1.35x weapon length.
- Width: narrow to medium, roughly 24 to 32 pixels.
- Damage multiplier: modest finisher bonus, initially 1.15x.

Visuals:

- Stronger `SpearTipTrail.png` at the tip.
- A longer, straight shaft afterimage using `SlashTex.png`.
- Reuse the sword hit flash on hit.

### Step 4B: Airborne Overhead Slam

If the player is airborne when step 4 starts, the branch is locked as the air finisher. The whole spear rotates over the player's head in a large semicircle using the grip area as the visual pivot, then slams forward and downward.

Initial timing:

- Active window: 28% to 72%.
- Reach: about 1.2x weapon length.
- Width: medium at the tip path, roughly 28 to 36 pixels.
- Damage multiplier: modest finisher bonus, initially 1.15x.

Visuals:

- Use `SlashTex.png` for the large overhead semicircle trajectory.
- Overlay `SpearTipTrail.png` near the tip so the damaging point remains readable.
- End pose should show the spear pressed forward/down, not suspended above the player.

## Branch Rules

The fourth step chooses its branch when the strike is fired, not every frame.

- Grounded at step-4 start: use grounded advancing thrust.
- Airborne at step-4 start: use airborne overhead slam.
- Once chosen, the branch does not change even if the player lands or leaves the ground during the strike.

## Architecture

Follow the sword system's organization, but do not use `SlashArcProjectile` as the spear's main attack.

Recommended classes:

- `SpearGlobalItem`: exact gate for `ItemID.Trident`, hides the vanilla use graphic, and starts the spear channel projectile.
- `SpearChannelProjectile`: reads local aim, keeps `itemAnimation` and `itemTime` alive, advances the spear combo index, and fires spear strikes.
- `SpearStrikeProjectile`: owns real damage, collision, hit cooldown, hit feedback, and networked strike parameters.
- `SpearTrailGlowProjectile`: visual-only drawing for shaft afterimages, tip trails, and the air finisher semicircle.
- `SpearComboStep`: immutable parameters for each combo step.
- `TridentSpearComboScheme`: the first and only V1 combo definition.

The existing player combo state can be extended only if the naming stays clear. If adding spear fields to `WeaponEffectsPlayer` makes the class unclear, create a small spear-specific state type instead.

## Data Flow

1. `SpearGlobalItem` detects `ItemID.Trident` use and spawns `SpearChannelProjectile`.
2. The channel projectile follows the player, updates aim from `Main.MouseWorld`, and keeps the item in channel-use state.
3. On each channel interval, the channel projectile consumes the next spear combo step.
4. The channel projectile spawns one `SpearStrikeProjectile` with step id, aim rotation, branch id, weapon item type, weapon length, damage, and knockback.
5. The strike projectile computes the current spear tip and shaft segment from normalized strike progress.
6. The strike projectile performs narrow line or capsule collision against NPCs.
7. On hit, the strike projectile triggers the existing sword hit flash behavior.
8. The strike projectile or channel projectile spawns `SpearTrailGlowProjectile` visuals with the same step and branch parameters.

## Collision Rules

Spear collision should be line or capsule based, not fan based.

- Steps 1 and 4A use a forward capsule from grip toward tip.
- Step 2 uses a swept tip capsule along the rising path.
- Step 3 uses a restrained swept capsule around the body.
- Step 4B uses a swept tip capsule along the overhead arc and final downward slam.

Each strike should avoid repeated uncontrolled hits. V1 should use a simple local NPC hit cooldown comparable to the sword behavior, with no special multi-hit piercing rules.

## Networking

The channel projectile should sync aim at the same kind of interval and threshold used by the sword channel projectile. The strike projectile must sync enough data for remote clients to reconstruct the same path:

- weapon item type
- combo step id
- branch id for step 4
- aim rotation
- weapon length
- damage and knockback inherited through normal projectile fields
- owner id

## Rendering

The spear weapon sprite should remain readable during every step. V1 should prefer clarity over dense effects.

Texture use:

- `SlashTex.png`: shaft afterimage and air finisher large semicircle.
- `SpearTipTrail.png`: bright tip trail for thrusts, lift, backsweep, and finishers.
- Existing sword hit flash: hit feedback.

The drawing code should keep `SlashTex.png` narrow for spear shaft visuals. A wide sword-like crescent is considered a V1 failure.

## Testing

Manual in-game verification is required because this is a visual and feel-heavy feature.

Minimum checks:

- Trident uses the spear combo instead of the sword slash combo.
- Non-Trident items are unchanged.
- Four combo steps advance in order and reset consistently.
- Step 4 chooses grounded or airborne branch only at strike start.
- Grounded finisher reaches farther than the first three steps.
- Air finisher draws the large overhead semicircle and ends forward/down.
- Enemy hits trigger damage and the reused hit flash.
- Multiplayer-relevant fields are written through `SendExtraAI` and restored through `ReceiveExtraAI`.

Code-level checks:

- Build succeeds.
- Existing sword behavior still compiles and remains isolated from spear projectiles.
- New spear classes avoid direct dependency on `SlashArcProjectile` for damage or collision.

## Acceptance Criteria

V1 is accepted when `ItemID.Trident` can execute the four-step combo with visible spear-tip trails, shaft afterimages, correct fourth-step branching, real enemy hits, and no visible fallback to sword crescent attacks.
