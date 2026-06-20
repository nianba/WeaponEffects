using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ShadowFlamePierceKnifeProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowFlameKnife;

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.timeLeft = ShadowFlameKnifeTuning.PierceLifetimeTicks;
		Projectile.tileCollide = true;
		Projectile.ignoreWater = true;
		Projectile.penetrate = 1;
		Projectile.aiStyle = -1;

		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 1000;
	}

	public override void AI()
	{
		if (Projectile.velocity.LengthSquared() > 0.01f)
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}

		Lighting.AddLight(Projectile.Center, 0.18f, 0.04f, 0.28f);

		if (!Main.dedServ && Main.rand.NextBool(4))
		{
			Dust dust = Dust.NewDustDirect(
				Projectile.position,
				Projectile.width,
				Projectile.height,
				ModContent.DustType<DarkSpark>(),
				-Projectile.velocity.X * 0.15f,
				-Projectile.velocity.Y * 0.15f,
				0,
				new Color(160, 60, 255),
				0.8f);

			dust.noGravity = true;
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

		if (Projectile.owner == Main.myPlayer)
		{
			float behindDistance = MathHelper.Max(target.width, target.height) * 0.55f + 18f;
			Vector2 stuckPosition = target.Center + direction * behindDistance;
			ShadowFlameKnifeHelper.SpawnStuckKnife(
				Projectile.GetSource_FromAI(),
				stuckPosition,
				direction,
				Projectile.owner);
		}

		target.AddBuff(BuffID.ShadowFlame, 120);
		EmitHitFeedback(target, direction);
		Projectile.Kill();
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		EmitTileCollideDust();
		Projectile.Kill();
		return false;
	}

	private void EmitHitFeedback(NPC target, Vector2 direction)
	{
		MeleeEffectAssets.NewProjectileDirect(
			Projectile.GetSource_FromAI(),
			target.Center,
			Vector2.Zero,
			ModContent.ProjectileType<SlashHitEffectProjectile>(),
			0,
			0f,
			Projectile.owner,
			Projectile.velocity.ToRotation());

		SoundStyle hitSound = new("WeaponEffects/Sounds/Onhit")
		{
			Volume = 0.32f,
			Pitch = Main.rand.NextFloat(-0.1f, 0.12f)
		};
		MeleeEffectAssets.PlaySound(in hitSound, target.Center);

		int count = MeleeEffectAssets.ScaleParticleCount(10);
		for (int i = 0; i < count; i++)
		{
			Vector2 velocity = direction.RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 4.5f);
			Dust dust = Dust.NewDustDirect(
				target.position,
				target.width,
				target.height,
				ModContent.DustType<DarkSpark>(),
				velocity.X,
				velocity.Y,
				0,
				new Color(155, 45, 230),
				Main.rand.NextFloat(0.65f, 1f));

			dust.noGravity = true;
		}
	}

	private void EmitTileCollideDust()
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(5);
		for (int i = 0; i < count; i++)
		{
			Dust dust = Dust.NewDustDirect(
				Projectile.position,
				Projectile.width,
				Projectile.height,
				ModContent.DustType<DarkSpark>(),
				-Projectile.velocity.X * Main.rand.NextFloat(0.03f, 0.16f),
				-Projectile.velocity.Y * Main.rand.NextFloat(0.03f, 0.16f),
				0,
				new Color(110, 35, 185),
				Main.rand.NextFloat(0.45f, 0.75f));

			dust.noGravity = true;
		}
	}
}
