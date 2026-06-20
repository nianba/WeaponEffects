using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace WeaponEffects;

internal static class ShadowFlameKnifeHelper
{
	public static void SpawnStuckKnife(IEntitySource source, Vector2 position, Vector2 direction, int owner)
	{
		KillOldestIfAtLimit(owner);

		Projectile projectile = MeleeEffectAssets.NewProjectileDirect(
			source,
			position,
			Vector2.Zero,
			ModContent.ProjectileType<ShadowFlameStuckKnifeProjectile>(),
			0,
			0f,
			owner);

		if (projectile.ModProjectile is ShadowFlameStuckKnifeProjectile stuck)
		{
			stuck.Initialize(direction);
		}
	}

	public static void RecallAll(Player player, IEntitySource source, int baseDamage, float knockback)
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

		Vector2 recallTarget = player.Center;
		int recallDamage = Math.Max(1, (int)MathF.Round(baseDamage * ShadowFlameKnifeTuning.RecallDamageMultiplier));
		int explosionDamage = Math.Max(1, (int)MathF.Round(baseDamage * ShadowFlameKnifeTuning.ExplosionDamageMultiplier));

		foreach (Projectile stuck in stuckKnives)
		{
			ShadowFlameRecallKnifeProjectile.Spawn(
				source,
				stuck.Center,
				recallTarget,
				player.whoAmI,
				recallDamage,
				explosionDamage,
				knockback * 0.5f);

			stuck.Kill();
		}

		SoundStyle recallSound = new("WeaponEffects/Sounds/Slashing")
		{
			Volume = 0.55f,
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

		int count = MeleeEffectAssets.ScaleParticleCount(3);
		for (int i = 0; i < count; i++)
		{
			Dust dust = Dust.NewDustDirect(
				player.position,
				player.width,
				player.height,
				ModContent.DustType<DarkSpark>(),
				Main.rand.NextFloat(-0.6f, 0.6f),
				Main.rand.NextFloat(-0.8f, 0.2f),
				0,
				new Color(80, 30, 150),
				0.45f);

			dust.noGravity = true;
		}
	}
}
