using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public static class VanillaSpearProjectileEmitter
{
	private const float StepTwoSpread = 0.16f;
	private const float StepThreeSpread = 0.05f;
	private const float MinimumShootSpeed = 1f;
	private const int TravelDistanceMultiplier = 2;

	public static void Emit(
		Projectile source,
		Player player,
		int weaponItemType,
		int comboStepIndex,
		float aimRotation,
		float weaponLength,
		int damage,
		float knockback)
	{
		if (player.whoAmI != Main.myPlayer)
		{
			return;
		}

		if (!ModContent.GetInstance<WeaponEffectsGameplayConfig>().EmitVanillaSwordProjectiles)
		{
			return;
		}

		if (!TryGetProfile(weaponItemType, out SpearProjectileProfile profile))
		{
			return;
		}

		int count = ProjectileCount(comboStepIndex);
		if (count <= 0)
		{
			return;
		}

		Vector2 origin = EmissionPoint(player, aimRotation, weaponLength);
		float speed = Math.Max(MinimumShootSpeed, HeldShootSpeed(player, weaponItemType)) * profile.SpeedMultiplier;
		int projectileDamage = Math.Max(1, damage);
		Vector2 fallbackDirection = aimRotation.ToRotationVector2();

		for (int i = 0; i < count; i++)
		{
			float offset = SpreadOffset(comboStepIndex, count, i);
			Vector2 velocity = fallbackDirection.RotatedBy(offset) * speed;
			Projectile projectile = Projectile.NewProjectileDirect(source.GetSource_FromAI(), origin, velocity, profile.ProjectileType, projectileDamage, knockback, source.owner);
			projectile.timeLeft *= TravelDistanceMultiplier;
			projectile.netUpdate = true;
		}
	}

	private static bool TryGetProfile(int itemType, out SpearProjectileProfile profile)
	{
		switch (itemType)
		{
			case ItemID.ThunderSpear:
				profile = new SpearProjectileProfile(ProjectileID.ThunderSpearShot, 4f);
				return true;
			case ItemID.MushroomSpear:
				profile = new SpearProjectileProfile(ProjectileID.Mushroom, 1f);
				return true;
			case ItemID.ChlorophytePartisan:
				profile = new SpearProjectileProfile(ProjectileID.SporeCloud, 1f);
				return true;
			default:
				profile = default;
				return false;
		}
	}

	private static int ProjectileCount(int comboStepIndex)
	{
		return comboStepIndex switch
		{
			1 => 1,
			2 => 2,
			3 => 3,
			_ => 0
		};
	}

	private static float SpreadOffset(int comboStepIndex, int count, int index)
	{
		return comboStepIndex switch
		{
			2 => (index - (count - 1) * 0.5f) * StepTwoSpread,
			3 => (index - (count - 1) * 0.5f) * StepThreeSpread,
			_ => 0f
		};
	}

	private static Vector2 EmissionPoint(Player player, float aimRotation, float weaponLength)
	{
		Vector2 direction = aimRotation.ToRotationVector2().SafeNormalize(Vector2.UnitX * Math.Sign(player.direction));
		float distance = MathHelper.Clamp(weaponLength * 0.36f, 28f, 64f);
		return player.MountedCenter + direction * distance;
	}

	private static float HeldShootSpeed(Player player, int weaponItemType)
	{
		Item item = player.HeldItem;
		if (item != null && !item.IsAir && item.type == weaponItemType && item.shootSpeed > 0f)
		{
			return item.shootSpeed;
		}

		return MinimumShootSpeed;
	}

	private readonly struct SpearProjectileProfile
	{
		public readonly int ProjectileType;
		public readonly float SpeedMultiplier;

		public SpearProjectileProfile(int projectileType, float speedMultiplier)
		{
			ProjectileType = projectileType;
			SpeedMultiplier = speedMultiplier;
		}
	}
}
