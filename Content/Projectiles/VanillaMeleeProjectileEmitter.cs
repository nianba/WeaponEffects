using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public static class VanillaMeleeProjectileEmitter
{
	private const int StarfuryItemType = 65;
	private const int StarWrathItemType = 3065;
	private const int ProjectileSwordItemTypeA = 674;
	private const int ProjectileSwordItemTypeB = 675;
	private const int ZenithItemType = 757;

	public static void Emit(ModProjectile sourceProjectile, bool charged, int itemType, Player player, Vector2 targetWorld)
	{
		if (player.whoAmI != Main.myPlayer)
		{
			return;
		}

		if (!ModContent.GetInstance<WeaponEffectsGameplayConfig>().EmitVanillaSwordProjectiles)
		{
			return;
		}

		Projectile source = sourceProjectile.Projectile;
		Vector2 playerCenter = player.Center;
		Vector2 aimDirection = SafeDirection(playerCenter, targetWorld, source.ai[1]);

		if (itemType == ZenithItemType)
		{
			int zenithMode = charged ? 3 : 1;
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 48f, ProjectileID.TerraBlade2Shot, source.damage, source.knockBack / 2f, source.owner, player.direction * player.gravDir, 45f, zenithMode);
			return;
		}

		if (!charged)
		{
			EmitNormal(source, itemType, player, targetWorld, playerCenter, aimDirection);
			return;
		}

		EmitCharged(source, itemType, player, targetWorld, playerCenter, aimDirection);
	}

	private static void EmitNormal(Projectile source, int itemType, Player player, Vector2 targetWorld, Vector2 playerCenter, Vector2 aimDirection)
	{
		if (itemType == StarfuryItemType)
		{
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection);
			return;
		}

		if (itemType == StarWrathItemType)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 start = targetWorld + new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-720, -500));
				Projectile.NewProjectile(source.GetSource_FromAI(), start, SafeDirection(start, targetWorld, source.ai[1]) * 19f, ProjectileID.StarWrath, source.damage * 2, source.knockBack, source.owner);
			}

			return;
		}

		switch (itemType)
		{
			case ProjectileSwordItemTypeA:
				Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 18f, ProjectileID.LightBeam, source.damage, source.knockBack / 2f, source.owner);
				return;
			case ProjectileSwordItemTypeB:
				Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 18f, ProjectileID.NightBeam, source.damage, source.knockBack / 2f, source.owner);
				return;
		}

		if (player.HeldItem.shoot > ProjectileID.None)
		{
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 30f, player.HeldItem.shoot, source.damage / 2, source.knockBack / 2f, source.owner);
		}
	}

	private static void EmitCharged(Projectile source, int itemType, Player player, Vector2 targetWorld, Vector2 playerCenter, Vector2 aimDirection)
	{
		if (itemType == StarfuryItemType)
		{
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection);
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection.RotatedBy(0.12f));
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection.RotatedBy(-0.12f));
			return;
		}

		if (itemType == StarWrathItemType)
		{
			for (int i = 0; i < 9; i++)
			{
				Vector2 start = targetWorld + new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-810, -450));
				Projectile.NewProjectile(source.GetSource_FromAI(), start, SafeDirection(start, targetWorld, source.ai[1]) * 19f, ProjectileID.StarWrath, source.damage * 2, source.knockBack, source.owner);
			}

			return;
		}

		switch (itemType)
		{
			case ProjectileSwordItemTypeA:
				EmitScaledSpread(source, playerCenter, aimDirection, 156);
				return;
			case ProjectileSwordItemTypeB:
				EmitScaledSpread(source, playerCenter, aimDirection, 157);
				return;
		}

		if (player.HeldItem.shoot <= ProjectileID.None)
		{
			return;
		}

		for (int i = 1; i < 3; i++)
		{
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection.RotatedByRandom(0.25) * 30f, player.HeldItem.shoot, source.damage / 2, source.knockBack / 2f, source.owner);
		}

		Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 30f, player.HeldItem.shoot, source.damage / 2, source.knockBack / 2f, source.owner);
	}

	private static void EmitScaledSpread(Projectile source, Vector2 playerCenter, Vector2 aimDirection, int projectileType)
	{
		for (int i = 1; i < 3; i++)
		{
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection.RotatedByRandom(0.25) * 30f, projectileType, source.damage / 2, source.knockBack / 2f, source.owner);
		}
	}

	private static void EmitBladeLaunchedStarfury(Projectile source, Vector2 playerCenter, Vector2 aimDirection)
	{
		Vector2 start = BladeEmissionPoint(source, playerCenter, aimDirection);
		Projectile.NewProjectile(source.GetSource_FromAI(), start, aimDirection * 13f, ProjectileID.Starfury, source.damage * 2, source.knockBack, source.owner);
	}

	private static Vector2 BladeEmissionPoint(Projectile source, Vector2 playerCenter, Vector2 aimDirection)
	{
		float weaponLength = MathHelper.Clamp(source.localAI[1], 36f, 120f);
		return playerCenter + aimDirection * (weaponLength * 0.58f);
	}

	private static Vector2 SafeDirection(Vector2 from, Vector2 to, float fallbackRotation)
	{
		Vector2 fallback = fallbackRotation.ToRotationVector2();
		if (fallback == Vector2.Zero)
		{
			fallback = Vector2.UnitX;
		}

		return (to - from).SafeNormalize(fallback);
	}
}
