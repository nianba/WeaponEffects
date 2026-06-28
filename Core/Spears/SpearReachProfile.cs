using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace WeaponEffects.Spears;

public static class SpearReachProfile
{
	private const float VanillaReachScale = 1f;

	private static readonly Dictionary<int, VanillaSpearMotionProfile> MotionProfiles = CreateMotionProfiles();

	public static float ResolveBaseLength(Item item, float fallbackLength)
	{
		if (item == null || item.IsAir)
		{
			return fallbackLength;
		}

		if (!MotionProfiles.TryGetValue(item.shoot, out VanillaSpearMotionProfile motion))
		{
			return fallbackLength;
		}

		int useAnimation = Math.Max(1, item.useAnimation);
		float vanillaReachScore = Math.Max(0f, item.shootSpeed) * motion.MaxExtensionFactor(useAnimation);
		if (vanillaReachScore <= 0f)
		{
			return fallbackLength;
		}

		return Math.Max(SpearMotion.MinimumSpearReach, vanillaReachScore * VanillaReachScale);
	}

	private static Dictionary<int, VanillaSpearMotionProfile> CreateMotionProfiles()
	{
		Dictionary<int, VanillaSpearMotionProfile> profiles = new()
		{
			[ProjectileID.DarkLance] = new(3f, 1.4f, 1.6f),
			[ProjectileID.Trident] = new(4f, 0.9f, 1.2f),
			[ProjectileID.Spear] = new(4f, 0.85f, 1.1f),
			[ProjectileID.MythrilHalberd] = new(3f, 1.7f, 1.9f),
			[ProjectileID.AdamantiteGlaive] = new(3f, 1.9f, 2.1f),
			[ProjectileID.CobaltNaginata] = new(3f, 1.9f, 2.1f),
			[ProjectileID.Gungnir] = new(3f, 2.1f, 2.4f),
			[ProjectileID.MushroomSpear] = new(3f, 1f, 1.3f),
			[ProjectileID.PalladiumPike] = new(3f, 1.9f, 2.1f),
			[ProjectileID.OrichalcumHalberd] = new(3f, 1.7f, 1.9f),
			[ProjectileID.TheRottedFork] = new(4f, 1.3f, 1.5f),
			[ProjectileID.TitaniumTrident] = new(3f, 1.9f, 2.1f),
			[ProjectileID.ChlorophytePartisan] = new(3f, 2.1f, 2.4f),
			[ProjectileID.NorthPoleWeapon] = new(3f, 2.1f, 2.4f),
			[ProjectileID.ObsidianSwordfish] = new(3f, 1.5f, 1.6f),
			[ProjectileID.Swordfish] = new(3f, 1.4f, 1.5f),
			[ProjectileID.ThunderSpear] = new(8f, 1.2f, 1.5f)
		};

		TryAddProjectileProfile(profiles, "SlimeSpear", new VanillaSpearMotionProfile(4f, 0.85f, 1.1f));

		return profiles;
	}

	private static void TryAddProjectileProfile(
		Dictionary<int, VanillaSpearMotionProfile> profiles,
		string projectileName,
		VanillaSpearMotionProfile profile)
	{
		int projectileType = SpearItemIdResolver.TryGetVanillaProjectileId(projectileName);
		if (projectileType > ProjectileID.None)
		{
			profiles[projectileType] = profile;
		}
	}

	private readonly struct VanillaSpearMotionProfile
	{
		private readonly float _baseOffset;
		private readonly float _forwardFactor;
		private readonly float _retractFactor;

		public VanillaSpearMotionProfile(float baseOffset, float forwardFactor, float retractFactor)
		{
			_baseOffset = baseOffset;
			_forwardFactor = forwardFactor;
			_retractFactor = retractFactor;
		}

		public float MaxExtensionFactor(int useAnimation)
		{
			int windBackTicks = useAnimation / 3;
			int forwardTicks = Math.Max(0, useAnimation - windBackTicks);
			return _baseOffset + _forwardFactor * forwardTicks;
		}
	}
}
