using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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
		writer.Write(Projectile.rotation);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		Projectile.rotation = reader.ReadSingle();
	}

	public void Initialize(Vector2 direction)
	{
		Projectile.velocity = Vector2.Zero;
		Projectile.rotation = direction.SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver4;
		Projectile.netUpdate = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.velocity = Vector2.Zero;
		Lighting.AddLight(Projectile.Center, 0.1f, 0.02f, 0.18f);

		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			Projectile.Kill();
			return;
		}

		Player owner = Main.player[Projectile.owner];
		bool shouldFadeFast =
			!owner.active ||
			owner.dead ||
			owner.HeldItem == null ||
			owner.HeldItem.type != ItemID.ShadowFlameKnife;

		if (shouldFadeFast && Projectile.timeLeft > 15)
		{
			Projectile.timeLeft = 15;
		}

		if (!Main.dedServ && Main.rand.NextBool(10))
		{
			Dust dust = Dust.NewDustDirect(
				Projectile.position,
				Projectile.width,
				Projectile.height,
				ModContent.DustType<DarkSpark>(),
				0f,
				-0.2f,
				0,
				new Color(120, 40, 220),
				0.65f);

			dust.noGravity = true;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		float fade = Projectile.timeLeft > ShadowFlameKnifeTuning.FadeStartTicks
			? 0.68f
			: MathHelper.Clamp(Projectile.timeLeft / (float)ShadowFlameKnifeTuning.FadeStartTicks, 0f, 1f) * 0.68f;

		if (Projectile.timeLeft <= ShadowFlameKnifeTuning.FadeStartTicks)
		{
			fade *= Main.rand.NextBool(5) ? 0.65f : 1f;
		}

		Color drawColor = Color.Lerp(new Color(110, 40, 190), lightColor, 0.35f) * fade;
		Main.EntitySpriteDraw(
			texture,
			Projectile.Center - Main.screenPosition,
			null,
			drawColor,
			Projectile.rotation,
			texture.Size() / 2f,
			Projectile.scale,
			SpriteEffects.None,
			0f);

		return false;
	}
}
