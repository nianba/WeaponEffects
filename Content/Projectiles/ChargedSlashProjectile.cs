using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class ChargedSlashProjectile : ModProjectile
{
	private const int ChargeDurationMultiplier = 5;
	private const float MaxChargeLengthScale = 2f;
	private const float MaxChargeDamageScale = 5f;
	private const int AimSyncInterval = 6;
	private const float AimSyncThreshold = 0.03f;
	private static readonly bool DrawChargeBarEnabled = false;

	private int _weaponItemType;
	private int _useAnimation;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;
	private float _lastSyncedAimRotation;
	private bool _released;

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

		if (Projectile.ai[1] >= ChargeReadyFrame)
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

		DrawHeldWeapon(weaponTexture);
		if (DrawChargeBarEnabled)
		{
			DrawChargeBar(player);
		}

		return false;
	}

	private int ChargeReadyFrame => Math.Max(1, _useAnimation) * ChargeDurationMultiplier;

	private float ChargeProgress => MathHelper.Clamp(Projectile.ai[1] / ChargeReadyFrame, 0f, 1f);

	private float CurrentLengthScale => MathHelper.Lerp(1f, MaxChargeLengthScale, ChargeProgress);

	private float CurrentDamageScale => MathHelper.Lerp(1f, Math.Min(MaxChargeDamageScale, ModContent.GetInstance<MeleeWeaponEffectsGameplayConfig>().ChargeDamage), ChargeProgress);

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
		float progress = Math.Min(Projectile.ai[1], ChargeReadyFrame);
		float ratio = progress / ChargeReadyFrame;
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
		float lengthScale = CurrentLengthScale;
		int damage = Math.Max(1, (int)MathF.Round(Projectile.damage * CurrentDamageScale));
		Projectile.damage = damage;
		WeaponSlashProfile profile = GetChargeProfile(player);
		Color color = profile.SlashColor;

		SoundEngine.PlaySound(new SoundStyle("MeleeWeaponEffects/Sounds/Slashing") { Volume = 0.8f }, player.Center);
		VanillaMeleeProjectileEmitter.Emit(this, charged: true, player.HeldItem.type, player, _targetWorld);
		SlashArcProjectile.CreateSlash(
			isPlayerOwned: true,
			source: Projectile.GetSource_FromAI(),
			rotation: _aimRotation,
			startingRotation: Projectile.ai[0],
			length: _weaponLength * lengthScale,
			thickness: 0.45f,
			yScale: 0.35f,
			extraUpdates: 5,
			damage: damage,
			knockback: 5f,
			owner: player.whoAmI,
			color: color,
			weaponItemType: _weaponItemType,
			knockbackRotation: Projectile.rotation - Projectile.ai[0],
			weaponScale: _weaponLength);

		player.GetModPlayer<MeleeEffectsPlayer>().ScreenShakeTimer = 15;
	}

	private void EmitChargedReadyVisuals(Player player)
	{
		if (Projectile.ai[1] % 5f != 0f)
		{
			return;
		}

		WeaponSlashProfile profile = GetChargeProfile(player);
		SlashParticleEmitter.EmitSwingParticles(in profile, player.Center, _aimRotation + MathHelper.Pi, _weaponLength * 0.55f, 0.25f, 0.45f);
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
