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
	private int _weaponItemType;
	private int _useAnimation;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;

	public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		_weaponItemType = weaponItemType;
		_useAnimation = Math.Max(1, useAnimation);
		_weaponLength = Math.Max(1f, weaponLength);
		_targetWorld = targetWorld;
		_aimRotation = (targetWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Math.Sign(Projectile.ai[0])).ToRotation();
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

		if (player.channel)
		{
			Projectile.timeLeft = 2;
		}

		Projectile.velocity = Vector2.Zero;
		Projectile.Center = player.Center;

		int chargeReadyFrame = ChargeReadyFrame;
		if (Projectile.ai[1] == chargeReadyFrame)
		{
			SoundEngine.PlaySound(SoundID.Item4, player.Center);
			for (int i = 0; i < 30; i++)
			{
				Vector2 offset = Main.rand.NextVector2CircularEdge(50f, 50f);
				Dust.NewDust(player.Center + offset, 0, 0, DustID.GemDiamond, 0f, 0f, 0, default, 1.5f);
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (Projectile.ai[1] < ChargeReadyFrame)
		{
			return;
		}

		Player player = Main.player[Projectile.owner];
		SoundEngine.PlaySound(new SoundStyle("MeleeWeaponEffects/Sounds/Slashing") { Volume = 0.8f }, player.Center);

		if (Projectile.owner == Main.myPlayer)
		{
			VanillaMeleeProjectileEmitter.Emit(this, charged: true, player.HeldItem.type, player, _targetWorld);
			SlashArcProjectile.CreateSlash(
				isPlayerOwned: true,
				source: Projectile.GetSource_FromAI(),
				rotation: _aimRotation,
				startingRotation: Projectile.ai[0],
				length: _weaponLength * 3f,
				thickness: 0.45f,
				yScale: 0.35f,
				extraUpdates: 5,
				damage: Projectile.damage,
				knockback: 5f,
				owner: player.whoAmI,
				color: Color.White,
				weaponItemType: _weaponItemType,
				knockbackRotation: Projectile.rotation - Projectile.ai[0],
				weaponScale: _weaponLength);
		}

		player.GetModPlayer<MeleeEffectsPlayer>().ScreenShakeTimer = 15;
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
		DrawChargeBar(player);
		return false;
	}

	private int ChargeReadyFrame => Math.Max(1, _useAnimation) * 3;

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 direction = (_targetWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
		_aimRotation = direction.ToRotation();
		player.direction = Math.Sign(direction.X);
		Projectile.netUpdate = true;
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
		if (Projectile.ai[0] <= 0f)
		{
			Main.EntitySpriteDraw(weaponTexture, drawPosition, null, Color.White, Projectile.rotation + 0.785f, new Vector2(0f, weaponTexture.Height), 1f, SpriteEffects.None, 0f);
			return;
		}

		Main.EntitySpriteDraw(weaponTexture, drawPosition, null, Color.White, Projectile.rotation + 0.785f + 1.5707f, new Vector2(weaponTexture.Width, weaponTexture.Height), 1f, SpriteEffects.FlipHorizontally, 0f);
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
}
