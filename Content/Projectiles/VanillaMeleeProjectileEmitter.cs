using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public static class VanillaMeleeProjectileEmitter
{
	private const float TrueExcaliburShootSpeed = 11f;
	private const float TrueNightsEdgeShootSpeed = 14f;

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

		if (itemType == ItemID.TerraBlade)
		{
			int terraBladeMode = charged ? 3 : 1;
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 48f, ProjectileID.TerraBlade2Shot, source.damage, source.knockBack / 2f, source.owner, player.direction * player.gravDir, 45f, terraBladeMode);
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
		if (itemType == ItemID.Starfury)
		{
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection);
			return;
		}

		if (itemType == ItemID.StarWrath)
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
			case ItemID.TrueExcalibur:
				Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * TrueExcaliburShootSpeed, ProjectileID.TrueExcalibur, source.damage, source.knockBack, source.owner);
				return;
			case ItemID.TrueNightsEdge:
				Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * TrueNightsEdgeShootSpeed, ProjectileID.TrueNightsEdge, source.damage, source.knockBack, source.owner);
				return;
		}

		if (player.HeldItem.shoot > ProjectileID.None)
		{
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection * 30f, player.HeldItem.shoot, source.damage / 2, source.knockBack / 2f, source.owner);
		}
	}

	private static void EmitCharged(Projectile source, int itemType, Player player, Vector2 targetWorld, Vector2 playerCenter, Vector2 aimDirection)
	{
		if (itemType == ItemID.Starfury)
		{
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection);
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection.RotatedBy(0.12f));
			EmitBladeLaunchedStarfury(source, playerCenter, aimDirection.RotatedBy(-0.12f));
			return;
		}

		if (itemType == ItemID.StarWrath)
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
			case ItemID.TrueExcalibur:
				EmitScaledSpread(source, playerCenter, aimDirection, ProjectileID.TrueExcalibur, TrueExcaliburShootSpeed);
				return;
			case ItemID.TrueNightsEdge:
				EmitScaledSpread(source, playerCenter, aimDirection, ProjectileID.TrueNightsEdge, TrueNightsEdgeShootSpeed);
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

	private static void EmitScaledSpread(Projectile source, Vector2 playerCenter, Vector2 aimDirection, int projectileType, float shootSpeed)
	{
		for (int i = 1; i < 3; i++)
		{
			Projectile.NewProjectile(source.GetSource_FromAI(), playerCenter, aimDirection.RotatedByRandom(0.25) * shootSpeed, projectileType, source.damage / 2, source.knockBack / 2f, source.owner);
		}
	}

	private static void EmitBladeLaunchedStarfury(Projectile source, Vector2 playerCenter, Vector2 aimDirection)
	{
		Vector2 start = BladeEmissionPoint(source, playerCenter, aimDirection);
		Projectile star = Projectile.NewProjectileDirect(source.GetSource_FromAI(), start, aimDirection * 13f, ProjectileID.Starfury, source.damage * 2, source.knockBack, source.owner);
		star.tileCollide = false;
		star.netUpdate = true;
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
