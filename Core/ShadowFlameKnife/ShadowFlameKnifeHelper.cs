using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

internal static class ShadowFlameKnifeHelper
{
	// Vanilla ShadowFlameKnife uses velocity.ToRotation() + PiOver2 while flying.
	private const float KnifeSpriteRotationOffset = MathHelper.PiOver2;

	public static float KnifeDrawRotation(Vector2 direction)
	{
		return direction.SafeNormalize(Vector2.UnitX).ToRotation() + KnifeSpriteRotationOffset;
	}

	public static void DrawShadowKnife(Projectile projectile, Color lightColor, float opacity = 1f)
	{
		Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
		Color drawColor = Color.Lerp(lightColor, Color.White, 0.25f) * opacity;

		Main.EntitySpriteDraw(
			texture,
			projectile.Center - Main.screenPosition,
			null,
			drawColor,
			projectile.rotation,
			texture.Size() / 2f,
			projectile.scale,
			SpriteEffects.None,
			0f);
	}

	public static void SpawnStuckKnife(IEntitySource source, Vector2 position, Vector2 direction, int owner, int sourceDamage)
	{
		Vector2 normalizedDirection = direction.SafeNormalize(Vector2.UnitX);
		KillOldestIfAtLimit(owner);

		Projectile projectile = MeleeEffectAssets.NewProjectileDirect(
			source,
			position,
			Vector2.Zero,
			ModContent.ProjectileType<ShadowFlameStuckKnifeProjectile>(),
			0,
			0f,
			owner,
			normalizedDirection.ToRotation());

		if (projectile.ModProjectile is ShadowFlameStuckKnifeProjectile stuck)
		{
			stuck.Initialize(normalizedDirection, sourceDamage);
		}
	}

	public static void EmitShadowFlameImpactParticles(Vector2 center, Vector2 direction, int baseCount, float speedScale = 1f)
	{
		if (Main.dedServ)
		{
			return;
		}

		Vector2 forward = direction.SafeNormalize(Vector2.UnitX);
		Vector2 normal = new(-forward.Y, forward.X);
		int count = MeleeEffectAssets.ScaleParticleCount((int)MathF.Ceiling(baseCount * 1.6f));
		for (int i = 0; i < count; i++)
		{
			float forwardSpeed = Main.rand.NextFloat(0.75f, 4.8f) * speedScale;
			float sideSpeed = Main.rand.NextFloat(-2.5f, 2.5f) * speedScale;
			Vector2 velocity = forward * forwardSpeed + normal * sideSpeed + Main.rand.NextVector2Circular(0.85f, 0.85f);
			Color color = Main.rand.NextBool(3)
				? new Color(70, 18, 115)
				: new Color(135, 42, 215);
			Dust dust = Dust.NewDustDirect(
				center + Main.rand.NextVector2Circular(13f, 13f),
				1,
				1,
				Main.rand.NextBool(4) ? DustID.Demonite : DustID.Shadowflame,
				velocity.X,
				velocity.Y,
				0,
				color,
				Main.rand.NextFloat(0.8f, 1.35f));

			dust.noGravity = true;
		}

		int emberCount = MeleeEffectAssets.ScaleParticleCount(Math.Max(2, (int)MathF.Ceiling(baseCount / 3f)));
		for (int i = 0; i < emberCount; i++)
		{
			Vector2 velocity = forward.RotatedByRandom(0.9f) * Main.rand.NextFloat(0.65f, 3.1f) * speedScale;
			Dust ember = Dust.NewDustDirect(
				center + Main.rand.NextVector2Circular(10f, 10f),
				1,
				1,
				DustID.Shadowflame,
				velocity.X,
				velocity.Y,
				0,
				new Color(95, 30, 170),
				Main.rand.NextFloat(0.75f, 1.15f));

			ember.noGravity = true;
		}
	}

	public static void EmitSlashHitEffect(IEntitySource source, Vector2 center, int owner, float rotation)
	{
		MeleeEffectAssets.NewProjectileDirect(
			source,
			center,
			Vector2.Zero,
			ModContent.ProjectileType<SlashHitEffectProjectile>(),
			0,
			0f,
			owner,
			rotation);
	}

	public static void EmitShadowFlameTrailParticle(Vector2 center, Vector2 direction, float speedScale = 1f)
	{
		if (Main.dedServ)
		{
			return;
		}

		Vector2 forward = direction.SafeNormalize(Vector2.UnitX);
		int count = MeleeEffectAssets.ScaleParticleCount(2);
		for (int i = 0; i < count; i++)
		{
			Vector2 velocity = -forward.RotatedByRandom(0.42f) * Main.rand.NextFloat(0.45f, 2.2f) * speedScale;
			int dustType = Main.rand.NextBool(3) ? DustID.Demonite : DustID.Shadowflame;
			Color color = Main.rand.NextBool()
				? new Color(110, 42, 178)
				: new Color(48, 20, 92);
			Dust dust = Dust.NewDustDirect(
				center + Main.rand.NextVector2Circular(6f, 6f),
				1,
				1,
				dustType,
				velocity.X,
				velocity.Y,
				0,
				color,
				Main.rand.NextFloat(0.64f, 1.05f));

			dust.noGravity = true;
		}
	}

	public static void RecallAll(Player player, IEntitySource source, float knockback)
	{
		List<Projectile> stuckKnives = new();
		int stuckType = ModContent.ProjectileType<ShadowFlameStuckKnifeProjectile>();

		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active && projectile.owner == player.whoAmI && projectile.type == stuckType)
			{
				stuckKnives.Add(projectile);
			}
		}

		if (stuckKnives.Count <= 0)
		{
			EmitEmptyRecallFeedback(player);
			return;
		}

		player.GetModPlayer<WeaponEffectsPlayer>().StartShadowFlameRecallSession();

		foreach (Projectile stuck in stuckKnives)
		{
			int sourceDamage = Math.Max(1, stuck.damage);
			if (stuck.ModProjectile is ShadowFlameStuckKnifeProjectile stuckKnife)
			{
				sourceDamage = Math.Max(1, stuckKnife.SourceDamage);
			}

			int recallDamage = Math.Max(1, (int)MathF.Round(sourceDamage * ShadowFlameKnifeTuning.RecallDamageMultiplier));
			int explosionDamage = Math.Max(1, (int)MathF.Round(sourceDamage * ShadowFlameKnifeTuning.ExplosionDamageMultiplier));

			ShadowFlameRecallKnifeProjectile.Spawn(
				source,
				stuck.Center,
				player.whoAmI,
				recallDamage,
				explosionDamage,
				knockback * 0.5f);

			stuck.Kill();
		}

		SoundStyle recallSound = new("WeaponEffects/Sounds/Slashing")
		{
			Volume = 0.10f,
			Pitch = -0.2f
		};
		MeleeEffectAssets.PlaySound(in recallSound, player.Center);
	}

	private static void KillOldestIfAtLimit(int owner)
	{
		int count = 0;
		Projectile oldest = null;
		int stuckType = ModContent.ProjectileType<ShadowFlameStuckKnifeProjectile>();

		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (!projectile.active || projectile.owner != owner || projectile.type != stuckType)
			{
				continue;
			}

			count++;

			if (oldest == null || projectile.timeLeft < oldest.timeLeft)
			{
				oldest = projectile;
			}
		}

		if (count >= ShadowFlameKnifeTuning.MaxStuckKnives && oldest != null)
		{
			oldest.Kill();
		}
	}

	private static void EmitEmptyRecallFeedback(Player player)
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(6);
		for (int i = 0; i < count; i++)
		{
			Dust dust = Dust.NewDustDirect(
				player.position,
				player.width,
				player.height,
				Main.rand.NextBool() ? DustID.Demonite : DustID.Shadowflame,
				Main.rand.NextFloat(-0.6f, 0.6f),
				Main.rand.NextFloat(-0.8f, 0.2f),
				0,
				new Color(80, 30, 150),
				0.75f);

			dust.noGravity = true;
		}
	}
}
