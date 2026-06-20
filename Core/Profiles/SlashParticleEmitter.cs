using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace WeaponEffects;

public static class SlashParticleEmitter
{
	public static void EmitSwingParticles(in WeaponSlashProfile profile, Vector2 center, float rotation, float length, float yScale, float strength = 1f)
	{
		if (Main.dedServ)
		{
			return;
		}

		SlashParticleProfile particles = profile.SwingParticles;
		int count = ScaledCount(particles.Count, strength);
		if (count <= 0)
		{
			return;
		}

		Vector2 forward = rotation.ToRotationVector2();
		Vector2 normal = new(-forward.Y, forward.X);
		float outerRadius = length * Main.rand.NextFloat(0.55f, 1f);

		for (int i = 0; i < count; i++)
		{
			float along = Main.rand.NextFloat(0.35f, 1f);
			float side = Main.rand.NextFloat(-0.28f, 0.28f) * length * yScale;
			Vector2 position = center + forward * outerRadius * along + normal * side;
			Vector2 velocity = forward.RotatedBy(Main.rand.NextFloat(-particles.SpreadRadians, particles.SpreadRadians)) * Main.rand.NextFloat(1.2f, 4.2f) * particles.VelocityScale;
			if (particles.VisualStyle == SlashParticleVisualStyle.DrawnStar)
			{
				SpawnDrawnStar(position, velocity, in particles, cluster: Main.rand.NextFloat() < 0.25f);
				continue;
			}

			SpawnDust(position, velocity, in particles);
		}
	}

	public static void EmitSwingTrailParticles(in WeaponSlashProfile profile, Vector2 position, Vector2 tangent, Vector2 outward, float strength = 0.16f)
	{
		if (Main.dedServ)
		{
			return;
		}

		SlashParticleProfile particles = profile.SwingParticles;
		int count = ScaledCount(particles.Count, strength);
		if (count <= 0)
		{
			return;
		}

		Vector2 tangentDirection = tangent.SafeNormalize(Vector2.UnitX);
		Vector2 outwardDirection = outward.SafeNormalize(Vector2.Zero);
		for (int i = 0; i < count; i++)
		{
			Vector2 velocity = tangentDirection.RotatedBy(Main.rand.NextFloat(-particles.SpreadRadians * 0.45f, particles.SpreadRadians * 0.45f)) * Main.rand.NextFloat(0.8f, 2.8f) * particles.VelocityScale;
			velocity += outwardDirection * Main.rand.NextFloat(0.15f, 1.15f) * particles.VelocityScale;
			Vector2 spawnPosition = position + Main.rand.NextVector2Circular(4f, 4f);
			if (particles.VisualStyle == SlashParticleVisualStyle.DrawnStar)
			{
				SpawnDrawnStar(spawnPosition, velocity, in particles, cluster: Main.rand.NextFloat() < 0.18f);
				continue;
			}

			SpawnDust(spawnPosition, velocity, in particles);
		}
	}

	public static void EmitHitParticles(in WeaponSlashProfile profile, Vector2 position, float rotation, float strength = 1f)
	{
		if (Main.dedServ)
		{
			return;
		}

		SlashParticleProfile particles = profile.HitParticles;
		int count = ScaledCount(particles.Count, strength);
		if (count <= 0)
		{
			return;
		}

		Vector2 forward = rotation.ToRotationVector2();
		for (int i = 0; i < count; i++)
		{
			Vector2 velocity = forward.RotatedBy(Main.rand.NextFloat(-particles.SpreadRadians, particles.SpreadRadians)) * Main.rand.NextFloat(1.6f, 6.5f) * particles.VelocityScale;
			velocity += Main.rand.NextVector2Circular(1.2f, 1.2f);
			Vector2 spawnPosition = position + Main.rand.NextVector2Circular(8f, 8f);
			if (particles.VisualStyle == SlashParticleVisualStyle.DrawnStar)
			{
				SpawnDrawnStar(spawnPosition, velocity * 0.72f, in particles, cluster: Main.rand.NextFloat() < 0.65f);
				continue;
			}

			SpawnDust(spawnPosition, velocity, in particles);
		}
	}

	private static int ScaledCount(int baseCount, float strength)
	{
		return MeleeEffectAssets.ScaleParticleCount(baseCount, strength);
	}

	private static void SpawnDust(Vector2 position, Vector2 velocity, in SlashParticleProfile particles)
	{
		int dustType = particles.AlternateDustType >= 0 && Main.rand.NextBool()
			? particles.AlternateDustType
			: particles.DustType;
		Color color = particles.AlternateDustColor != default && Main.rand.NextBool()
			? particles.AlternateDustColor
			: particles.DustColor;
		Dust dust = Dust.NewDustDirect(position, 1, 1, dustType, velocity.X, velocity.Y, 0, color, Main.rand.NextFloat(particles.MinScale, particles.MaxScale));
		dust.noGravity = particles.NoGravity || dustType == DustID.Torch;
		dust.fadeIn = dustType == DustID.Torch ? 0.8f : 0f;
	}

	private static void SpawnDrawnStar(Vector2 position, Vector2 velocity, in SlashParticleProfile particles, bool cluster)
	{
		Color color = particles.AlternateDustColor != default && Main.rand.NextBool()
			? particles.AlternateDustColor
			: particles.DustColor;
		float scale = Main.rand.NextFloat(particles.MinScale, particles.MaxScale);
		StarSlashSparkleProjectile.Spawn(position, velocity, color, scale, cluster);
	}
}
