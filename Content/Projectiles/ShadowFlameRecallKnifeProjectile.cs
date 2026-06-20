using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ShadowFlameRecallKnifeProjectile : ModProjectile
{
	private int _explosionDamage;

	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowFlameKnife;

	public static void Spawn(IEntitySource source, Vector2 start, Vector2 target, int owner, int damage, int explosionDamage, float knockback)
	{
		Vector2 direction = (target - start).SafeNormalize(Vector2.UnitX);
		Vector2 velocity = direction * ShadowFlameKnifeTuning.RecallSpeed;

		Projectile projectile = MeleeEffectAssets.NewProjectileDirect(
			source,
			start,
			velocity,
			ModContent.ProjectileType<ShadowFlameRecallKnifeProjectile>(),
			damage,
			knockback,
			owner,
			target.X,
			target.Y);

		if (projectile.ModProjectile is ShadowFlameRecallKnifeProjectile recall)
		{
			recall._explosionDamage = explosionDamage;
			projectile.netUpdate = true;
		}
	}

	public override void SetStaticDefaults()
	{
	}

	public override void SetDefaults()
	{
		Projectile.width = 20;
		Projectile.height = 20;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = ShadowFlameKnifeTuning.RecallLifetimeTicks;
		Projectile.aiStyle = -1;

		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 1000;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_explosionDamage);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_explosionDamage = reader.ReadInt32();
	}

	public override void AI()
	{
		Vector2 target = new(Projectile.ai[0], Projectile.ai[1]);

		if (Projectile.velocity.LengthSquared() > 0.01f)
		{
			Projectile.rotation = ShadowFlameKnifeHelper.KnifeDrawRotation(Projectile.velocity);
		}

		Lighting.AddLight(Projectile.Center, 0.2f, 0.04f, 0.32f);

		if (Vector2.DistanceSquared(Projectile.Center, target) < 28f * 28f)
		{
			Projectile.Kill();
			return;
		}

		if (!Main.dedServ)
		{
			ShadowFlameKnifeHelper.EmitShadowFlameTrailParticle(Projectile.Center, Projectile.velocity, 1.8f);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		target.AddBuff(BuffID.ShadowFlame, ShadowFlameKnifeTuning.ShadowFlameDebuffTicks);

		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner == Main.myPlayer)
		{
			Main.player[Projectile.owner]
				.GetModPlayer<WeaponEffectsPlayer>()
				.RegisterShadowFlameRecallHit(target, Projectile, _explosionDamage);
		}

		ShadowFlameKnifeHelper.EmitSlashHitEffect(
			Projectile.GetSource_FromAI(),
			target.Center,
			Projectile.owner,
			Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
		ShadowFlameKnifeHelper.EmitShadowFlameImpactParticles(target.Center, Projectile.velocity, 14, 1f);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		ShadowFlameKnifeHelper.DrawShadowKnife(Projectile, lightColor, 0.96f);
		return false;
	}
}
