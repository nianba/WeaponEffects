using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ShadowFlameExplosionProjectile : ModProjectile
{
	private bool _played;

	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowFlameKnife;

	public static void Spawn(IEntitySource source, Vector2 center, int owner, int damage, float knockback)
	{
		Projectile projectile = MeleeEffectAssets.NewProjectileDirect(
			source,
			center,
			Vector2.Zero,
			ModContent.ProjectileType<ShadowFlameExplosionProjectile>(),
			damage,
			knockback,
			owner);

		projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = ShadowFlameKnifeTuning.ExplosionRadius;
		Projectile.height = ShadowFlameKnifeTuning.ExplosionRadius;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 8;
		Projectile.aiStyle = -1;

		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 1000;
	}

	public override bool? CanDamage()
	{
		return Projectile.timeLeft >= 6;
	}

	public override void AI()
	{
		if (_played)
		{
			return;
		}

		_played = true;

		SoundStyle sound = new("WeaponEffects/Sounds/Slashing")
		{
			Volume = 0.75f,
			Pitch = -0.35f
		};
		MeleeEffectAssets.PlaySound(in sound, Projectile.Center);

		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Projectile.owner == Main.myPlayer)
		{
			Main.player[Projectile.owner].GetModPlayer<WeaponEffectsPlayer>().ScreenShakeTimer = 6;
		}

		EmitExplosionDust();
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}

	private void EmitExplosionDust()
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(52);
		for (int i = 0; i < count; i++)
		{
			Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.6f, 8.4f);
			Dust dust = Dust.NewDustDirect(
				Projectile.Center - new Vector2(4f),
				8,
				8,
				Main.rand.NextBool(3) ? DustID.Demonite : DustID.Shadowflame,
				velocity.X,
				velocity.Y,
				0,
				new Color(175, 55, 255),
				Main.rand.NextFloat(1f, 1.65f));

			dust.noGravity = true;
		}

		ShadowFlameKnifeHelper.EmitShadowFlameImpactParticles(Projectile.Center, Main.rand.NextVector2Unit(), 28, 1.5f);
	}
}
