using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ShadowFlameStuckKnifeProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowFlameKnife;

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = ShadowFlameKnifeTuning.StuckLifetimeTicks;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(Projectile.ai[0]);
		writer.Write(Projectile.rotation);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Projectile.ai[0] = reader.ReadSingle();
		Projectile.rotation = reader.ReadSingle();
	}

	public void Initialize(Vector2 direction)
	{
		Vector2 normalizedDirection = direction.SafeNormalize(Vector2.UnitX);
		Projectile.velocity = Vector2.Zero;
		Projectile.ai[0] = normalizedDirection.ToRotation();
		Projectile.rotation = ShadowFlameKnifeHelper.KnifeDrawRotation(normalizedDirection);
		Projectile.netUpdate = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.velocity = Vector2.Zero;
		Projectile.rotation = ShadowFlameKnifeHelper.KnifeDrawRotation(Projectile.ai[0].ToRotationVector2());
		Lighting.AddLight(Projectile.Center, 0.1f, 0.02f, 0.18f);

		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			Projectile.Kill();
			return;
		}

		Player owner = Main.player[Projectile.owner];
		Vector2 directionToPlayer = (owner.Center - Projectile.Center).SafeNormalize(Projectile.ai[0].ToRotationVector2());
		Projectile.ai[0] = directionToPlayer.ToRotation();
		Projectile.rotation = ShadowFlameKnifeHelper.KnifeDrawRotation(directionToPlayer);

		bool shouldFadeFast =
			!owner.active ||
			owner.dead ||
			owner.HeldItem == null ||
			owner.HeldItem.type != ItemID.ShadowFlameKnife;

		if (shouldFadeFast && Projectile.timeLeft > 15)
		{
			Projectile.timeLeft = 15;
		}

		if (!Main.dedServ && Main.rand.NextBool(4))
		{
			ShadowFlameKnifeHelper.EmitShadowFlameTrailParticle(Projectile.Center, directionToPlayer, 0.55f);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		float opacity = Projectile.timeLeft > ShadowFlameKnifeTuning.FadeStartTicks
			? 0.48f
			: MathHelper.Clamp(Projectile.timeLeft / (float)ShadowFlameKnifeTuning.FadeStartTicks, 0f, 1f) * 0.48f;
		ShadowFlameKnifeHelper.DrawShadowKnife(Projectile, lightColor, opacity);
		return false;
	}
}
