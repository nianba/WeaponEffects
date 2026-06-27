using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;

namespace WeaponEffects;

public class SpearThrowProjectile : ModProjectile
{
	private readonly bool[] _hitNpcs = new bool[Main.maxNPCs];
	private int _weaponItemType;
	private float _aimRotation;
	private float _maxTravelDistance;
	private float _visualWidth;
	private float _collisionWidth;
	private float _chargeProgress;
	private float _distanceTravelled;

	public override string Texture => "Terraria/Images/Extra_" + ExtrasID.SharpTears;

	public static void Spawn(
		IEntitySource source,
		Vector2 position,
		int owner,
		int weaponItemType,
		float aimRotation,
		float maxTravelDistance,
		float visualWidth,
		float collisionWidth,
		float chargeProgress,
		int damage,
		float knockback)
	{
		Projectile projectile = Projectile.NewProjectileDirect(
			source,
			position,
			aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed,
			ModContent.ProjectileType<SpearThrowProjectile>(),
			damage,
			knockback,
			owner);

		if (projectile.ModProjectile is SpearThrowProjectile spearThrow)
		{
			spearThrow.Initialize(weaponItemType, aimRotation, maxTravelDistance, visualWidth, collisionWidth, chargeProgress);
		}

		MeleeEffectAssets.SyncProjectile(projectile);
	}

	public void Initialize(
		int weaponItemType,
		float aimRotation,
		float maxTravelDistance,
		float visualWidth,
		float collisionWidth,
		float chargeProgress)
	{
		_weaponItemType = Math.Max(0, weaponItemType);
		_aimRotation = aimRotation;
		_maxTravelDistance = Math.Max(1f, maxTravelDistance);
		_visualWidth = Math.Max(1f, visualWidth);
		_collisionWidth = Math.Max(_visualWidth + 2f, collisionWidth);
		_chargeProgress = MathHelper.Clamp(chargeProgress, 0f, 1f);
		_distanceTravelled = 0f;
		Projectile.rotation = _aimRotation;
		Projectile.velocity = _aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed;
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 300;
		Projectile.aiStyle = -1;
		Projectile.noEnchantmentVisuals = true;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_weaponItemType);
		writer.Write(_aimRotation);
		writer.Write(_maxTravelDistance);
		writer.Write(_visualWidth);
		writer.Write(_collisionWidth);
		writer.Write(_chargeProgress);
		writer.Write(_distanceTravelled);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_aimRotation = reader.ReadSingle();
		_maxTravelDistance = reader.ReadSingle();
		_visualWidth = reader.ReadSingle();
		_collisionWidth = reader.ReadSingle();
		_chargeProgress = reader.ReadSingle();
		_distanceTravelled = reader.ReadSingle();
		Projectile.rotation = _aimRotation;
		Projectile.velocity = _aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed;
	}

	public override void AI()
	{
		Projectile.rotation = _aimRotation;
		Projectile.velocity = _aimRotation.ToRotationVector2() * SpearThrowChargeMath.ThrowSpeed;
		_distanceTravelled += Projectile.velocity.Length();

		float lightStrength = MathHelper.Lerp(0.45f, 0.9f, _chargeProgress);
		Lighting.AddLight(Projectile.Center, 1f * lightStrength, 0.72f * lightStrength, 0.18f * lightStrength);

		if (_distanceTravelled >= _maxTravelDistance)
		{
			Projectile.Kill();
		}
	}

	public override bool? CanHitNPC(NPC target)
	{
		if (target == null || _hitNpcs[target.whoAmI])
		{
			return false;
		}

		return null;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		Vector2 direction = _aimRotation.ToRotationVector2();
		float length = SpindleLength;
		Vector2 start = Projectile.Center - direction * (length * 0.5f);
		Vector2 end = Projectile.Center + direction * (length * 0.5f);
		float collisionPoint = 0f;

		return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, _collisionWidth, ref collisionPoint);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		_hitNpcs[target.whoAmI] = true;

		SoundStyle hitSound = new("WeaponEffects/Sounds/Onhit")
		{
			Volume = 0.34f,
			Pitch = Main.rand.NextFloat(-0.04f, 0.18f)
		};
		MeleeEffectAssets.PlaySound(in hitSound, target.Center);
		SpawnGoldHitDust(target);

		if (ModContent.GetInstance<WeaponEffectsVisualConfig>().DrawSpearHitFlash)
		{
			MeleeEffectAssets.NewProjectileDirect(
				Projectile.GetSource_FromAI(),
				target.Center,
				Vector2.Zero,
				ModContent.ProjectileType<SlashHitEffectProjectile>(),
				0,
				0f,
				Projectile.owner,
				_aimRotation);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		DrawSpindle();
		return false;
	}

	private void DrawSpindle()
	{
		if (Main.dedServ)
		{
			return;
		}

		Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		Vector2 origin = texture.Size() * 0.5f;
		Vector2 drawPosition = Projectile.Center - Main.screenPosition;
		float length = SpindleLength;
		float width = Math.Max(1f, _visualWidth);
		float rotation = _aimRotation + MathHelper.PiOver2;
		Vector2 baseScale = new(width / texture.Width, length / texture.Height);
		float pulse = 0.92f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 18f);

		DrawSpindleLayer(texture, origin, drawPosition, rotation, baseScale * new Vector2(1.08f, 1.24f), new Color(255, 167, 34, 0) * 0.32f * pulse);
		DrawSpindleLayer(texture, origin, drawPosition, rotation, baseScale * new Vector2(0.92f, 0.84f), new Color(255, 211, 72, 0) * 0.72f);
		DrawSpindleLayer(texture, origin, drawPosition, rotation, baseScale * new Vector2(0.58f, 0.38f), new Color(255, 250, 190, 0) * 0.92f);
	}

	private static void DrawSpindleLayer(Texture2D texture, Vector2 origin, Vector2 drawPosition, float rotation, Vector2 scale, Color color)
	{
		Main.EntitySpriteDraw(
			texture,
			drawPosition,
			null,
			color,
			rotation,
			origin,
			scale,
			SpriteEffects.None,
			0f);
	}

	private void SpawnGoldHitDust(NPC target)
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(22, 0.9f + _chargeProgress * 0.9f);
		for (int i = 0; i < count; i++)
		{
			Vector2 velocity = Main.rand.NextVector2Circular(3.2f, 3.2f) + _aimRotation.ToRotationVector2() * Main.rand.NextFloat(0.8f, 2.8f);
			Color color = Color.Lerp(new Color(255, 170, 42), new Color(255, 235, 105), Main.rand.NextFloat());
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GemTopaz, velocity.X, velocity.Y, 0, color, Main.rand.NextFloat(0.9f, 1.45f));
			dust.noGravity = true;
			dust.fadeIn = 0.8f;
		}
	}

	private float SpindleLength => MathHelper.Lerp(296f, 528f, _chargeProgress);
}
