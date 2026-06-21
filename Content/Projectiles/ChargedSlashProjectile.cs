using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ChargedSlashProjectile : ModProjectile
{
	private const float MaxChargeDamageScale = 5f;
	private const int AimSyncInterval = 6;
	private const float AimSyncThreshold = 0.03f;

	private int _weaponItemType;
	private int _useAnimation;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;
	private float _lastSyncedAimRotation;
	private bool _released;
	private bool _readyBurstEmitted;

	public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		_weaponItemType = weaponItemType;
		_useAnimation = Math.Max(1, useAnimation);
		_weaponLength = Math.Max(1f, weaponLength);
		_targetWorld = targetWorld;
		_aimRotation = (targetWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Math.Sign(Projectile.ai[0])).ToRotation();
		_lastSyncedAimRotation = _aimRotation;
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
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_weaponItemType);
		writer.Write(_useAnimation);
		writer.Write(_weaponLength);
		writer.Write(_targetWorld.X);
		writer.Write(_targetWorld.Y);
		writer.Write(_aimRotation);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_useAnimation = reader.ReadInt32();
		_weaponLength = reader.ReadSingle();
		_targetWorld = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		_aimRotation = reader.ReadSingle();
	}

	public override void AI()
	{
		Projectile.ai[1] += 1f;
		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead)
		{
			Projectile.Kill();
			return;
		}

		if (Projectile.owner == Main.myPlayer)
		{
			UpdateLocalAim(player);
		}

		player.itemAnimation = 3;
		player.itemTime = 3;
		player.heldProj = Projectile.whoAmI;
		Projectile.rotation = Projectile.ai[0] + _aimRotation;

		if (Projectile.timeLeft > 2)
		{
			Projectile.timeLeft = 2;
		}

		if (IsHoldingCharge())
		{
			Projectile.timeLeft = 2;
		}
		else if (Projectile.owner == Main.myPlayer)
		{
			ReleaseChargedSlash(player);
			Projectile.Kill();
			return;
		}

		Projectile.velocity = Vector2.Zero;
		Projectile.Center = player.Center;

		EmitChargingDust(player);

		if (Projectile.ai[1] >= EffectiveChargeReadyFrame)
		{
			EmitChargedReadyVisuals(player);
		}
	}

	public override void OnKill(int timeLeft)
	{
		Player player = Main.player[Projectile.owner];
		if (!_released && Projectile.owner == Main.myPlayer && Projectile.ai[1] > 0f && player.active && !player.dead)
		{
			ReleaseChargedSlash(player);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Player player = Main.player[Projectile.owner];
		Texture2D weaponTexture = GetWeaponTexture();
		if (weaponTexture == null)
		{
			return false;
		}

		if (ModContent.GetInstance<WeaponEffectsVisualConfig>().DrawHeldWeapon)
		{
			DrawHeldWeapon(weaponTexture);
		}

		if (ModContent.GetInstance<WeaponEffectsVisualConfig>().ShowChargeBar)
		{
			DrawChargeBar(player);
		}

		return false;
	}

	private int MinChargeFrame => Math.Max(1, _useAnimation) * Math.Max(1, ModContent.GetInstance<WeaponEffectsGameplayConfig>().ChargeMinDurationMultiplier);

	private int ChargeReadyFrame => Math.Max(MinChargeFrame, Math.Max(1, _useAnimation) * Math.Max(1, ModContent.GetInstance<WeaponEffectsGameplayConfig>().ChargeMaxDurationMultiplier));

	private int EffectiveChargeReadyFrame
	{
		get
		{
			if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
			{
				return ChargeReadyFrame;
			}

			Player player = Main.player[Projectile.owner];
			if (!player.active)
			{
				return ChargeReadyFrame;
			}

			WeaponEffectsPlayer effectsPlayer = player.GetModPlayer<WeaponEffectsPlayer>();
			return Math.Max(MinChargeFrame, (int)MathF.Round(ChargeReadyFrame * effectsPlayer.ChargeReadyFrameMultiplier));
		}
	}

	private float ChargeProgress => MathHelper.Clamp(Projectile.ai[1] / EffectiveChargeReadyFrame, 0f, 1f);

	private float CurrentLengthScale => MathHelper.Lerp(1f, MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().ChargeLengthScale, 1f, 4f), ChargeProgress);

	private float CurrentDamageScale => MathHelper.Lerp(1f, Math.Min(MaxChargeDamageScale, ModContent.GetInstance<WeaponEffectsGameplayConfig>().ChargeDamage), ChargeProgress);

	private bool IsHoldingCharge()
	{
		return Projectile.owner != Main.myPlayer || Main.mouseRight;
	}

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 direction = (_targetWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
		_aimRotation = direction.ToRotation();
		player.direction = Math.Sign(direction.X);

		float aimDelta = Math.Abs(MathHelper.WrapAngle(_aimRotation - _lastSyncedAimRotation));
		if (aimDelta >= AimSyncThreshold || Projectile.ai[1] % AimSyncInterval == 0f)
		{
			_lastSyncedAimRotation = _aimRotation;
			Projectile.netUpdate = true;
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

	private void DrawHeldWeapon(Texture2D weaponTexture)
	{
		Vector2 drawPosition = Projectile.Center - Main.screenPosition;
		Color drawColor = Color.White;
		float glowProgress = ChargeProgress;
		float glowPulse = 0.72f + 0.28f * (float)Math.Sin(Projectile.ai[1] * 0.22f);
		WeaponSlashProfile profile = GetChargeProfile(Main.player[Projectile.owner]);
		Color glowColor = profile.SlashColor;

		if (glowProgress > 0f)
		{
			float glowScale = MathHelper.Lerp(0.08f, 0.45f, glowProgress) * glowPulse;
			for (int i = 0; i < 4; i++)
			{
				Vector2 offset = (MathHelper.PiOver2 * i + Projectile.ai[1] * 0.05f).ToRotationVector2() * MathHelper.Lerp(1.5f, 4f, glowProgress);
				DrawWeaponSprite(weaponTexture, drawPosition + offset, glowColor * glowScale);
			}
		}

		DrawWeaponSprite(weaponTexture, drawPosition, drawColor);
	}

	private void DrawWeaponSprite(Texture2D weaponTexture, Vector2 drawPosition, Color color)
	{
		Main.EntitySpriteDraw(weaponTexture, drawPosition, null, color, _aimRotation + MathHelper.PiOver4 + MathHelper.Pi, new Vector2(0f, weaponTexture.Height), 1f, SpriteEffects.None, 0f);
	}

	private void DrawChargeBar(Player player)
	{
		Texture2D bar = MeleeEffectAssets.GetTexture(MeleeEffectAssets.ChargeBar);
		Texture2D fill = MeleeEffectAssets.GetTexture(MeleeEffectAssets.ChargeBarFill);
		int effectiveChargeReadyFrame = EffectiveChargeReadyFrame;
		float progress = Math.Min(Projectile.ai[1], effectiveChargeReadyFrame);
		float ratio = progress / effectiveChargeReadyFrame;
		Vector2 position = player.Center + new Vector2(-bar.Width / 2f, -60f) - Main.screenPosition;

		Main.EntitySpriteDraw(bar, position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		Color color = Color.Lerp(Color.GreenYellow, Color.Orange, ratio);
		if (ratio >= 1f)
		{
			color = Color.Lerp(Color.OrangeRed, Color.Yellow, 0.5f + 0.5f * (float)Math.Sin(Projectile.ai[1] * 0.3f));
		}

		int width = (int)(ratio * fill.Width);
		Main.EntitySpriteDraw(fill, position, new Rectangle(0, 0, width, fill.Height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
	}

	private void ReleaseChargedSlash(Player player)
	{
		if (_released)
		{
			return;
		}

		_released = true;
		if (Projectile.ai[1] < MinChargeFrame)
		{
			return;
		}

		WeaponEffectsPlayer effectsPlayer = player.GetModPlayer<WeaponEffectsPlayer>();
		float progress = ChargeProgress;
		float lengthBonus = EvaluateHalfFullBonus(progress, effectsPlayer.ChargeLengthBonusAtHalf, effectsPlayer.ChargeLengthBonusAtFull);
		float damageBonus = EvaluateHalfFullBonus(progress, effectsPlayer.ChargeDamageBonusAtHalf, effectsPlayer.ChargeDamageBonusAtFull);
		float widthBonus = effectsPlayer.ChargeWidthBonusAtFull * progress;
		float lengthScale = CurrentLengthScale * (1f + lengthBonus);
		int damage = Math.Max(1, (int)MathF.Round(Projectile.damage * CurrentDamageScale * (1f + damageBonus)));
		float thickness = 0.45f * (1f + widthBonus);
		Projectile.damage = damage;
		WeaponSlashProfile profile = GetChargeProfile(player);
		Color color = profile.SlashColor;

		SoundStyle releaseSound = new("WeaponEffects/Sounds/Slashing") { Volume = 0.8f };
		MeleeEffectAssets.PlaySound(in releaseSound, player.Center);
		VanillaMeleeProjectileEmitter.Emit(this, charged: true, player.HeldItem.type, player, _targetWorld);
		SlashArcProjectile.CreateSlash(
			isPlayerOwned: true,
			source: Projectile.GetSource_FromAI(),
			rotation: _aimRotation,
			startingRotation: Projectile.ai[0],
			length: _weaponLength * lengthScale,
			thickness: thickness,
			yScale: 0.35f,
			extraUpdates: 5,
			damage: damage,
			knockback: 5f * MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().SlashKnockbackMultiplier, 0f, 3f),
			owner: player.whoAmI,
			color: color,
			weaponItemType: _weaponItemType,
			knockbackRotation: Projectile.rotation - Projectile.ai[0],
			weaponScale: _weaponLength);

		player.GetModPlayer<WeaponEffectsPlayer>().ScreenShakeTimer = 15;
	}

	private static float EvaluateHalfFullBonus(float progress, float halfBonus, float fullBonus)
	{
		progress = MathHelper.Clamp(progress, 0f, 1f);
		if (progress <= 0.5f)
		{
			return MathHelper.Lerp(0f, halfBonus, progress / 0.5f);
		}

		return MathHelper.Lerp(halfBonus, fullBonus, (progress - 0.5f) / 0.5f);
	}

	private void EmitChargedReadyVisuals(Player player)
	{
		WeaponSlashProfile profile = GetChargeProfile(player);
		if (!_readyBurstEmitted)
		{
			_readyBurstEmitted = true;
			EmitReadyBurstParticles(in profile, player.Center);
		}

		if (Projectile.ai[1] % 5f != 0f)
		{
			return;
		}

		SlashParticleEmitter.EmitSwingParticles(in profile, player.Center, _aimRotation + MathHelper.Pi, _weaponLength * 0.55f, 0.25f, 0.45f);
	}

	private void EmitChargingDust(Player player)
	{
		if (Main.dedServ || Projectile.ai[1] % 4f != 0f || MeleeEffectAssets.ParticleDensityMultiplier <= 0f)
		{
			return;
		}

		WeaponSlashProfile profile = GetChargeProfile(player);
		SlashParticleProfile particles = profile.SwingParticles;
		Vector2 basePosition = player.Center + new Vector2(Main.rand.NextFloat(-18f, 18f), player.height * 0.45f);
		Vector2 velocity = new(Main.rand.NextFloat(-0.7f, 0.7f), Main.rand.NextFloat(1.8f, 3.4f));
		Color color = particles.AlternateDustColor != default && Main.rand.NextBool()
			? particles.AlternateDustColor
			: particles.DustColor;
		Dust dust = Dust.NewDustDirect(basePosition, 1, 1, particles.DustType, velocity.X, velocity.Y, 0, color, Main.rand.NextFloat(particles.MinScale, particles.MaxScale) * 0.75f);
		dust.noGravity = false;
		dust.fadeIn = particles.DustType == DustID.Torch ? 0.6f : 0f;
	}

	private void EmitReadyBurstParticles(in WeaponSlashProfile profile, Vector2 center)
	{
		if (Main.dedServ)
		{
			return;
		}

		SlashParticleProfile particles = profile.SwingParticles;
		int flashCount = MeleeEffectAssets.ScaleParticleCount(Math.Max(16, particles.Count * 3));
		for (int i = 0; i < flashCount; i++)
		{
			Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
			Vector2 position = center + Main.rand.NextVector2Circular(10f, 10f);
			Vector2 velocity = direction * Main.rand.NextFloat(0.8f, 3.2f) * particles.VelocityScale;
			Color color = Color.Lerp(ParticleColor(in particles), Color.White, 0.55f);
			Dust dust = Dust.NewDustDirect(position, 1, 1, particles.DustType, velocity.X, velocity.Y, 0, color, Main.rand.NextFloat(particles.MaxScale, particles.MaxScale * 1.8f));
			dust.noGravity = true;
			dust.fadeIn = particles.DustType == DustID.Torch ? 1f : 0.4f;
		}

		int burstCount = MeleeEffectAssets.ScaleParticleCount(Math.Max(64, particles.Count * 9));
		for (int i = 0; i < burstCount; i++)
		{
			Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
			float radius = Main.rand.NextFloat(18f, 74f);
			Vector2 position = center + direction * radius + Main.rand.NextVector2Circular(5f, 5f);
			Vector2 velocity = direction * Main.rand.NextFloat(4.5f, 11.5f) * particles.VelocityScale;
			Dust dust = Dust.NewDustDirect(position, 1, 1, particles.DustType, velocity.X, velocity.Y, 0, ParticleColor(in particles), Main.rand.NextFloat(particles.MinScale, particles.MaxScale) * 1.75f);
			dust.noGravity = true;
			dust.fadeIn = particles.DustType == DustID.Torch ? 1.1f : 0.25f;
		}

		int sparkCount = MeleeEffectAssets.ScaleParticleCount(Math.Max(14, particles.Count * 2));
		for (int i = 0; i < sparkCount; i++)
		{
			Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
			Dust spark = Dust.NewDustDirect(center + direction * Main.rand.NextFloat(38f, 90f), 1, 1, DustID.GemDiamond, direction.X * Main.rand.NextFloat(3f, 8f), direction.Y * Main.rand.NextFloat(3f, 8f), 0, Color.Lerp(profile.SlashColor, Color.White, 0.35f), Main.rand.NextFloat(1.15f, 1.9f));
			spark.noGravity = true;
		}
	}

	private static Color ParticleColor(in SlashParticleProfile particles)
	{
		return particles.AlternateDustColor != default && Main.rand.NextBool()
			? particles.AlternateDustColor
			: particles.DustColor;
	}

	private WeaponSlashProfile GetChargeProfile(Player player)
	{
		if (SlashProfileResolver.TryGetExactProfile(_weaponItemType, out WeaponSlashProfile profile))
		{
			return profile;
		}

		return SlashProfileResolver.GetProfile(player.HeldItem);
	}
}
