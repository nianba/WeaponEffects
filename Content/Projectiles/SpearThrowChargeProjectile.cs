using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;

namespace WeaponEffects;

public class SpearThrowChargeProjectile : ModProjectile
{
	private const int AimSyncInterval = 6;
	private const float AimSyncThreshold = 0.03f;

	private int _weaponItemType;
	private int _useAnimation;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;
	private float _lastSyncedAimRotation;
	private int _chargeFrames;
	private int _effectiveFullChargeFrames;
	private bool _released;
	private bool _fullChargeBurstEmitted;

	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		_weaponItemType = weaponItemType;
		_useAnimation = Math.Max(1, useAnimation);
		_weaponLength = Math.Max(1f, weaponLength);
		_targetWorld = targetWorld;
		_aimRotation = (targetWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Math.Sign(Projectile.ai[0])).ToRotation();
		_lastSyncedAimRotation = _aimRotation;
		_effectiveFullChargeFrames = SpearThrowChargeMath.BaseFullChargeFrames;
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = 100;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_weaponItemType);
		writer.Write(_useAnimation);
		writer.Write(_weaponLength);
		writer.Write(_targetWorld.X);
		writer.Write(_targetWorld.Y);
		writer.Write(_aimRotation);
		writer.Write(_lastSyncedAimRotation);
		writer.Write(_chargeFrames);
		writer.Write(_effectiveFullChargeFrames);
		writer.Write(_released);
		writer.Write(_fullChargeBurstEmitted);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_useAnimation = reader.ReadInt32();
		_weaponLength = reader.ReadSingle();
		_targetWorld = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		_aimRotation = reader.ReadSingle();
		_lastSyncedAimRotation = reader.ReadSingle();
		_chargeFrames = reader.ReadInt32();
		_effectiveFullChargeFrames = reader.ReadInt32();
		_released = reader.ReadBoolean();
		_fullChargeBurstEmitted = reader.ReadBoolean();
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override void AI()
	{
		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			Projectile.Kill();
			return;
		}

		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead)
		{
			Projectile.Kill();
			return;
		}

		Projectile.Center = player.Center;
		Projectile.velocity = Vector2.Zero;
		Projectile.rotation = _aimRotation;

		if (Projectile.owner == Main.myPlayer)
		{
			UpdateLocalAim(player);
			_effectiveFullChargeFrames = SpearThrowChargeMath.EffectiveFullChargeFrames(player.GetAttackSpeed(DamageClass.Melee));
			if (!CanContinueCharge(player) || !IsHoldingCharge())
			{
				ReleaseOrCancel(player);
				Projectile.Kill();
				return;
			}

			_chargeFrames++;
			MaintainHeldUsePose(player);
		}
		else
		{
			_chargeFrames++;
		}

		Projectile.timeLeft = 2;
		EmitChargingParticles(player);
		if (!_fullChargeBurstEmitted && _chargeFrames >= EffectiveFullChargeFrames)
		{
			_fullChargeBurstEmitted = true;
			EmitFullChargeBurst(player);
			Projectile.netUpdate = true;
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			return;
		}

		Player player = Main.player[Projectile.owner];
		if (player.heldProj == Projectile.whoAmI)
		{
			player.heldProj = -1;
		}

		if (Projectile.owner == Main.myPlayer)
		{
			player.itemAnimation = 0;
			player.itemTime = 0;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (Main.dedServ)
		{
			return false;
		}

		WeaponEffectsVisualConfig visualConfig = ModContent.GetInstance<WeaponEffectsVisualConfig>();
		if (!visualConfig.DrawHeldWeapon || !visualConfig.DrawSpearHeldWeapon)
		{
			return false;
		}

		Player player = Main.player[Projectile.owner];
		Texture2D weaponTexture = GetWeaponTexture();
		if (player.active && weaponTexture != null)
		{
			DrawChargingSpear(player, weaponTexture, lightColor);
		}

		return false;
	}

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 direction = (_targetWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
		_aimRotation = direction.ToRotation();
		player.direction = Math.Sign(direction.X);

		float aimDelta = Math.Abs(MathHelper.WrapAngle(_aimRotation - _lastSyncedAimRotation));
		if (aimDelta >= AimSyncThreshold || _chargeFrames % AimSyncInterval == 0)
		{
			_lastSyncedAimRotation = _aimRotation;
			Projectile.netUpdate = true;
		}
	}

	private void MaintainHeldUsePose(Player player)
	{
		int animationFrames = Math.Max(2, Math.Min(Math.Max(1, _useAnimation), 12));
		player.heldProj = Projectile.whoAmI;
		player.itemAnimation = animationFrames;
		player.itemTime = animationFrames;
		player.itemRotation = player.direction > 0 ? _aimRotation : _aimRotation + MathHelper.Pi;
		player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _aimRotation - MathHelper.PiOver2);
	}

	private void ReleaseOrCancel(Player player)
	{
		if (_released)
		{
			return;
		}

		_released = true;
		Projectile.netUpdate = true;
		if (!SpearThrowChargeMath.IsChargeValid(_chargeFrames))
		{
			return;
		}

		float progress = SpearThrowChargeMath.ChargeProgress(_chargeFrames, EffectiveFullChargeFrames);
		int damage = Math.Max(1, (int)MathF.Round(Projectile.damage * SpearThrowChargeMath.DamageMultiplier(progress)));
		float distance = SpearThrowChargeMath.TravelDistancePixels(progress, Main.screenWidth);
		float visualWidth = SpearThrowChargeMath.VisualWidth(progress);
		float collisionWidth = SpearThrowChargeMath.CollisionWidth(progress);
		float knockback = Projectile.knockBack * MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().SlashKnockbackMultiplier, 0f, 3f);

		SoundStyle releaseSound = new("WeaponEffects/Sounds/S2")
		{
			Volume = 0.42f,
			Pitch = MathHelper.Lerp(-0.08f, 0.18f, progress)
		};
		MeleeEffectAssets.PlaySound(in releaseSound, player.Center);

		SpearThrowProjectile.Spawn(
			Projectile.GetSource_FromAI(),
			player.Center,
			player.whoAmI,
			_weaponItemType,
			_aimRotation,
			distance,
			visualWidth,
			collisionWidth,
			progress,
			damage,
			knockback);
	}

	private bool IsHoldingCharge()
	{
		return Projectile.owner != Main.myPlayer || Main.mouseRight;
	}

	private bool CanContinueCharge(Player player)
	{
		if (!player.active || player.dead || player.noItems || player.CCed)
		{
			return false;
		}

		Item item = player.HeldItem;
		return _weaponItemType > 0 && item != null && !item.IsAir && item.type == _weaponItemType;
	}

	private void DrawChargingSpear(Player player, Texture2D weaponTexture, Color lightColor)
	{
		Vector2 direction = _aimRotation.ToRotationVector2();
		Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
		float progress = ChargeProgress;
		float pullBack = MathHelper.Lerp(12f, 42f, progress);
		float readyLift = MathHelper.Lerp(6f, 18f, progress);
		Vector2 grip = player.MountedCenter - direction * pullBack - normal * player.direction * 5f - Vector2.UnitY * readyLift;
		Vector2 tip = grip + direction * (_weaponLength * MathHelper.Lerp(0.78f, 0.98f, progress));
		Vector2 shaft = tip - grip;
		if (shaft.LengthSquared() <= 1f)
		{
			return;
		}

		Vector2 gripOrigin = HeldSpearGripOrigin(weaponTexture);
		Vector2 tipOrigin = HeldSpearTipOrigin(weaponTexture);
		Vector2 textureShaft = tipOrigin - gripOrigin;
		float scale = MathHelper.Clamp(shaft.Length() / Math.Max(1f, textureShaft.Length()), 0.65f, 1.18f);
		float rotation = shaft.ToRotation() - textureShaft.ToRotation();
		Color heatColor = progress < 0.5f
			? Color.Lerp(lightColor, new Color(255, 125, 32), progress / 0.5f)
			: Color.Lerp(new Color(255, 125, 32), new Color(255, 226, 96), (progress - 0.5f) / 0.5f);

		if (progress > 0f)
		{
			float pulse = 0.55f + 0.45f * (float)Math.Sin(_chargeFrames * 0.22f);
			for (int i = 0; i < 3; i++)
			{
				Vector2 offset = (MathHelper.TwoPi * i / 3f + _chargeFrames * 0.08f).ToRotationVector2() * MathHelper.Lerp(1f, 4f, progress);
				Main.EntitySpriteDraw(
					weaponTexture,
					grip - Main.screenPosition + offset,
					null,
					heatColor * (0.18f * progress * pulse),
					rotation,
					gripOrigin,
					scale,
					SpriteEffects.None,
					0f);
			}
		}

		Main.EntitySpriteDraw(
			weaponTexture,
			grip - Main.screenPosition,
			null,
			heatColor,
			rotation,
			gripOrigin,
			scale,
			SpriteEffects.None,
			0f);
	}

	private void EmitChargingParticles(Player player)
	{
		if (Main.dedServ || _chargeFrames % 5 != 0 || MeleeEffectAssets.ParticleDensityMultiplier <= 0f)
		{
			return;
		}

		float progress = ChargeProgress;
		int count = MeleeEffectAssets.ScaleParticleCount(progress >= 1f ? 3 : 1);
		Vector2 direction = _aimRotation.ToRotationVector2();
		Vector2 basePosition = player.Center - direction * MathHelper.Lerp(10f, 34f, progress);
		for (int i = 0; i < count; i++)
		{
			Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
			Vector2 velocity = direction.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * Main.rand.NextFloat(0.35f, 1.8f);
			Color color = Color.Lerp(new Color(255, 145, 40), new Color(255, 228, 90), progress);
			Dust dust = Dust.NewDustDirect(basePosition + offset, 1, 1, DustID.GoldFlame, velocity.X, velocity.Y, 0, color, Main.rand.NextFloat(0.75f, 1.35f));
			dust.noGravity = true;
			dust.fadeIn = 0.5f + progress;
		}
	}

	private void EmitFullChargeBurst(Player player)
	{
		if (Main.dedServ)
		{
			return;
		}

		int count = MeleeEffectAssets.ScaleParticleCount(42);
		for (int i = 0; i < count; i++)
		{
			Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
			Vector2 position = player.Center + direction * Main.rand.NextFloat(12f, 48f);
			Vector2 velocity = direction * Main.rand.NextFloat(2.2f, 8f);
			Color color = Color.Lerp(new Color(255, 168, 48), Color.Gold, Main.rand.NextFloat());
			Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.GoldFlame, velocity.X, velocity.Y, 0, color, Main.rand.NextFloat(1f, 1.8f));
			dust.noGravity = true;
			dust.fadeIn = 1f;
		}
	}

	private Texture2D GetWeaponTexture()
	{
		if (_weaponItemType <= 0 || _weaponItemType >= TextureAssets.Item.Length)
		{
			return null;
		}

		return TextureAssets.Item[_weaponItemType].Value;
	}

	private int EffectiveFullChargeFrames => _effectiveFullChargeFrames > 0
		? _effectiveFullChargeFrames
		: SpearThrowChargeMath.BaseFullChargeFrames;

	private float ChargeProgress => SpearThrowChargeMath.ChargeProgress(_chargeFrames, EffectiveFullChargeFrames);

	private static Vector2 HeldSpearGripOrigin(Texture2D weaponTexture)
	{
		return new Vector2(weaponTexture.Width * 0.1f, weaponTexture.Height * 0.9f);
	}

	private static Vector2 HeldSpearTipOrigin(Texture2D weaponTexture)
	{
		return new Vector2(weaponTexture.Width, 0f);
	}
}
