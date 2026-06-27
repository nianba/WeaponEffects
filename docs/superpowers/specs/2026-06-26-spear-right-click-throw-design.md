# Spear Right-Click Charged Throw Design

## Goal

Add a Trident right-click special attack that charges while the player holds right click, then throws a golden spindle-shaped spear-light projectile on release.

The feature is a spear-specific special attack. It should not reuse sword slash damage projectiles, and it should keep the existing left-click Trident combo isolated.

## Scope

Included:

- Trident right-click entry through `SpearGlobalItem`.
- Hold-to-charge control projectile.
- Release-to-throw spear-light damage projectile.
- Charge cancellation before the minimum charge time.
- Right-click interruption of the current left-click spear channel.
- Golden heating visuals during charge.
- Golden full-charge particle feedback.
- Narrow spindle-shaped spear-light projectile on release.
- Multi-enemy piercing, wall-piercing travel, and one hit per NPC per throw.

Excluded from V1:

- Cooldown after release.
- Release recovery or player movement lockout.
- Config menu entries for tuning values.
- Recall behavior after throwing.
- Weapon support beyond `ItemID.Trident`.
- Reusing `SlashArcProjectile` for spear throw damage.

## Gameplay And Input

Right click starts the charged throw. Holding right click keeps the charge active. Releasing right click resolves the charge:

- If charge time is below 1 second, cancel the special with no throw and no damage.
- If charge time is at least 1 second, fire the spear-light projectile.

Right-click charge can interrupt the current left-click spear combo. Starting charge should kill any owned `SpearChannelProjectile`, clear the active spear left-click controller state, and reset the left-click spear combo index. This makes the right-click throw a standalone special action rather than a combo continuation.

The player can move and jump normally while charging. V1 intentionally has no release recovery and no right-click cooldown. This favors feel first; balance risk is accepted for playtesting.

## Charge Scaling

Charge timing:

- Minimum valid charge: 1 second.
- Base full charge: 5 seconds.
- Attack speed reduces full-charge time only.
- The 1 second minimum valid charge time stays fixed.
- Implementation should clamp effective full-charge time to a reasonable lower bound so extreme attack speed cannot collapse the charge window below the minimum.

Damage scaling is linear from minimum valid charge to full charge:

- 1 second charge: 1x weapon damage.
- Full charge: 10x weapon damage.
- Values between those endpoints interpolate linearly.

Travel distance scaling is also linear:

- 1 second charge: about 1.5 screen widths.
- Full charge: about 5 screen widths.

The spear-light projectile has fixed high velocity. Charge affects damage and maximum travel distance, not projectile speed.

## Hit Rules

The thrown spear-light pierces enemies and tiles:

- `tileCollide` should be false.
- The projectile continues through walls.
- The projectile continues through enemies.
- Each NPC can be damaged at most once by a single throw.

The collision width should be wider than the visible spindle body. This is deliberate: the visual stays spear-like and narrow, while the hit feel is generous.

This combination is strong: normal movement, wall piercing, multi-target piercing, no cooldown, and 10x full-charge damage may be too efficient. The first implementation should preserve the chosen feel, then tune damage, range, width, or full-charge time after playtesting.

## Charge Visuals

During charge, the player holds the spear in a backward pre-throw pose:

- The player grips the spear handle.
- The spear is pulled backward into a throwing stance.
- The spear tip still points toward the mouse position.
- The pose should not read as a left-click stab, sweep, or sword-like slash.

The spear heats up over time:

- Starts from the normal weapon color.
- Progresses through heated orange.
- Reaches bright golden yellow near full charge.
- Full charge emits a golden variant of the existing sword full-charge particle feel.
- After full charge, golden pulsing should continue so the player knows the charge is capped.

## Release Visuals

On release, the thrown attack does not need to preserve the original Trident sprite silhouette. It should become a narrow piercing spear-light.

The projectile shape should be spindle-like:

- Tapered at both ends.
- Slightly wider and brightest at the center.
- Long, straight, and narrow enough to read as a spear or lance.
- Not a simple line segment.
- Not a broad sword beam or crescent.

The preferred color is golden, matching the charged spear heat. Hit feedback can reuse or adapt existing hit flash logic, but the color should read as gold rather than the generic sword slash effect.

## Architecture

Use an independent right-click chain:

- `SpearGlobalItem`: owns the right-click entry point. Adds `AltFunctionUse`, branches `UseItem` for right click, interrupts existing spear channels, resets spear combo state, and starts the charge controller.
- `SpearThrowChargeProjectile`: new control projectile for charging. Owns aim tracking, charge timer, held spear pose, heating visuals, full-charge particles, release, and cancellation.
- `SpearThrowProjectile`: new damage projectile for the released spear-light. Owns fixed-speed travel, max distance, wall piercing, multi-enemy piercing, one-hit-per-NPC state, collision width, damage, and spindle drawing.
- `WeaponEffectsPlayer`: should expose a clear method for resetting spear combo state instead of requiring outside code to mutate fields directly.
- `Core/Spears`: may contain a small pure helper for charge math, such as full-charge time, progress, damage multiplier, and travel distance calculations.

Do not route spear throw damage through `SlashArcProjectile`. Avoid coupling this feature to sword charge internals.

## Networking

The charge controller should sync enough data for remote clients to reproduce visible charge state:

- Weapon item type.
- Weapon length.
- Aim rotation or target direction.
- Charge frame count.
- Effective full-charge frame count.
- Whether full-charge burst has already been emitted.

The thrown spear-light should sync enough data for remote clients to reconstruct travel and visuals:

- Aim rotation.
- Charge progress or resolved damage/range inputs.
- Maximum travel distance.
- Visual width.
- Collision width.
- Weapon item type.
- Owner id.

Damage and knockback should use normal projectile fields where possible.

## Cancellation And Cleanup

The charge controller should end cleanly when:

- Right click is released before 1 second.
- The owner dies.
- The owner becomes inactive.
- The owner can no longer use items.
- The held item is no longer the Trident.

Cancellation before 1 second must not spawn the thrown projectile. Cleanup must avoid stuck `heldProj`, stuck item animation, or lingering charge visuals.

## Acceptance Criteria

- Trident right click starts a visible charge pose.
- Starting right-click charge interrupts any active Trident left-click channel and resets the spear combo index.
- Releasing before 1 second cancels with no throw and no damage.
- Releasing after 1 second fires a golden spindle-shaped spear-light.
- Full charge produces clear golden charge feedback.
- Damage scales linearly from 1x at 1 second to 10x at full charge.
- Travel distance scales linearly from about 1.5 screen widths to about 5 screen widths.
- Projectile speed is fixed regardless of charge.
- The spear-light passes through walls.
- The spear-light pierces multiple NPCs.
- Each NPC is damaged at most once per throw.
- The visible spear-light is narrower than its generous hit width.
- Player movement and jumping remain normal during charge.
- Existing left-click Trident combo still works after the throw special is cancelled or released.
- Existing sword slash and sword charge behavior remains isolated.

## Playtest Risks

The feature is intentionally tuned for feel first. The highest-risk combination is:

- Normal movement during charge.
- No cooldown.
- No release recovery.
- Wall piercing.
- Multi-target piercing.
- Generous collision width.
- 10x full-charge damage.

If playtesting shows the special dominates combat, tune in this order:

1. Reduce collision width.
2. Increase effective full-charge time floor.
3. Reduce full-charge damage from 10x.
4. Reduce full-charge range.
5. Add a small right-click cooldown only if numeric tuning cannot preserve the desired feel.
