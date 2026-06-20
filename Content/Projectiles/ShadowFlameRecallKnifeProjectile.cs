using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
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
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}

		Lighting.AddLight(Projectile.Center, 0.2f, 0.04f, 0.32f);

		if (Vector2.DistanceSquared(Projectile.Center, target) < 28f * 28f)
		{
			Projectile.Kill();
			return;
		}

		if (!Main.dedServ)
		{
			Dust dust = Dust.NewDustDirect(
				Projectile.position,
				Projectile.width,
				Projectile.height,
				ModContent.DustType<DarkSpark>(),
				-Projectile.velocity.X * 0.08f,
				-Projectile.velocity.Y * 0.08f,
				0,
				new Color(180, 70, 255),
				0.75f);

			dust.noGravity = true;
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner == Main.myPlayer)
		{
			Main.player[Projectile.owner]
				.GetModPlayer<WeaponEffectsPlayer>()
				.RegisterShadowFlameRecallHit(target, Projectile, _explosionDamage);
		}

		MeleeEffectAssets.NewProjectileDirect(
			Projectile.GetSource_FromAI(),
			target.Center,
			Vector2.Zero,
			ModContent.ProjectileType<SlashHitEffectProjectile>(),
			0,
			0f,
			Projectile.owner,
			Projectile.velocity.ToRotation());
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 origin = texture.Size() / 2f;

		for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
		{
			Vector2 oldPosition = Projectile.oldPos[i];
			if (oldPosition == Vector2.Zero)
			{
				continue;
			}

			float factor = 1f - i / (float)Projectile.oldPos.Length;
			Color trailColor = new Color(120, 40, 220, 0) * factor * 0.45f;
			Main.EntitySpriteDraw(
				texture,
				oldPosition + Projectile.Size / 2f - Main.screenPosition,
				null,
				trailColor,
				Projectile.oldRot[i] == 0f ? Projectile.rotation : Projectile.oldRot[i],
				origin,
				Projectile.scale * (0.75f + factor * 0.2f),
				SpriteEffects.None,
				0f);
		}

		Main.EntitySpriteDraw(
			texture,
			Projectile.Center - Main.screenPosition,
			null,
			Color.Lerp(lightColor, Color.White, 0.35f),
			Projectile.rotation,
			origin,
			Projectile.scale,
			SpriteEffects.None,
			0f);

		return false;
	}
}
