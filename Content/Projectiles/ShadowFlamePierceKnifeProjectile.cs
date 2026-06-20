using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ShadowFlamePierceKnifeProjectile : ModProjectile
{
	private bool _placedStuckKnife;

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
			Projectile.rotation = ShadowFlameKnifeHelper.KnifeDrawRotation(Projectile.velocity);
		}

		Lighting.AddLight(Projectile.Center, 0.18f, 0.04f, 0.28f);

		if (!Main.dedServ)
		{
			ShadowFlameKnifeHelper.EmitShadowFlameTrailParticle(Projectile.Center, Projectile.velocity, 1.3f);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		ShadowFlameKnifeHelper.DrawShadowKnife(Projectile, lightColor);
		return false;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

		if (Projectile.owner == Main.myPlayer)
		{
			float behindDistance = MathHelper.Max(target.width, target.height) * 0.55f + 18f;
			Vector2 stuckPosition = target.Center + direction * behindDistance;
			PlaceStuckKnife(stuckPosition, direction);
		}

		target.AddBuff(BuffID.ShadowFlame, ShadowFlameKnifeTuning.ShadowFlameDebuffTicks);
		EmitHitFeedback(target, direction);
		Projectile.Kill();
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Vector2 direction = oldVelocity.SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
		if (Projectile.owner == Main.myPlayer)
		{
			PlaceStuckKnife(Projectile.Center, direction);
		}

		EmitTileCollideDust(direction);
		Projectile.Kill();
		return false;
	}

	public override void OnKill(int timeLeft)
	{
		if (_placedStuckKnife || timeLeft > 0 || Projectile.owner != Main.myPlayer)
		{
			return;
		}

		Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
		PlaceStuckKnife(Projectile.Center, direction);
		ShadowFlameKnifeHelper.EmitShadowFlameImpactParticles(Projectile.Center, -direction, 10, 0.8f);
	}

	private void EmitHitFeedback(NPC target, Vector2 direction)
	{
		SoundStyle hitSound = new("WeaponEffects/Sounds/Onhit")
		{
			Volume = 0.32f,
			Pitch = Main.rand.NextFloat(-0.1f, 0.12f)
		};
		MeleeEffectAssets.PlaySound(in hitSound, target.Center);

		ShadowFlameKnifeHelper.EmitSlashHitEffect(
			Projectile.GetSource_FromAI(),
			target.Center,
			Projectile.owner,
			Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
		ShadowFlameKnifeHelper.EmitShadowFlameImpactParticles(target.Center, direction, 20, 1.2f);
	}

	private void EmitTileCollideDust(Vector2 direction)
	{
		if (Main.dedServ)
		{
			return;
		}

		ShadowFlameKnifeHelper.EmitShadowFlameImpactParticles(Projectile.Center, -direction, 14, 0.9f);
	}

	private void PlaceStuckKnife(Vector2 position, Vector2 direction)
	{
		if (_placedStuckKnife)
		{
			return;
		}

		_placedStuckKnife = true;
		ShadowFlameKnifeHelper.SpawnStuckKnife(
			Projectile.GetSource_FromAI(),
			position,
			direction,
			Projectile.owner,
			Projectile.damage);
	}
}
