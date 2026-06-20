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

public class SlashChannelProjectile : ModProjectile
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
		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead)
		{
			Projectile.Kill();
			return;
		}

		Projectile.ai[1] += 1f;
		int useAnimation = Math.Max(1, _useAnimation);

		if (Projectile.owner == Main.myPlayer)
		{
			UpdateLocalAim(player);
		}

		player.itemAnimation = useAnimation;
		player.itemTime = useAnimation;
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

		if (Projectile.ai[1] % useAnimation == 2f && Projectile.owner == Main.myPlayer)
		{
			FireSlash(player);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 direction = (_targetWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
		_aimRotation = direction.ToRotation();
		player.direction = Math.Sign(direction.X);
		Projectile.netUpdate = true;
	}

	private void FireSlash(Player player)
	{
		VanillaMeleeProjectileEmitter.Emit(this, charged: false, player.HeldItem.type, player, _targetWorld);

		SoundEngine.PlaySound(new SoundStyle("MeleeWeaponEffects/Sounds/S2") { Volume = 0.36f }, player.Center);
		float randomizedRotation = _aimRotation + Main.rand.NextFloat(-0.5f, 0.5f);
		float length = Main.rand.Next(160, 220) / 110f * _weaponLength;
		int startRotation = Main.rand.NextBool() ? -2 : 2;

		SlashArcProjectile.CreateSlash(
			isPlayerOwned: true,
			source: Projectile.GetSource_FromAI(),
			rotation: randomizedRotation,
			startingRotation: startRotation,
			length: length,
			thickness: 0.5f,
			yScale: Main.rand.NextFloat(0.36f, 0.8f),
			extraUpdates: Main.rand.Next(4, 6),
			damage: Projectile.damage,
			knockback: Projectile.knockBack,
			owner: player.whoAmI,
			color: Color.White,
			weaponItemType: _weaponItemType,
			knockbackRotation: Projectile.rotation - Projectile.ai[0],
			weaponScale: _weaponLength);
	}
}
