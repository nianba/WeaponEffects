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
	private const float LegacyAverageLengthScale = 190f / 110f;
	private static readonly SlashEmissionMode EmissionMode = SlashEmissionMode.Compact3DComboSchemeA;

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
			FireSlash(player, useAnimation);
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

	private void FireSlash(Player player, int useAnimation)
	{
		switch (EmissionMode)
		{
			case SlashEmissionMode.LegacyRandom:
				FireRandomSlash(player);
				break;
			case SlashEmissionMode.Compact3DComboSchemeA:
				FireComboSchemeASlash(player, useAnimation);
				break;
		}
	}

	private void FireRandomSlash(Player player)
	{
		VanillaMeleeProjectileEmitter.Emit(this, charged: false, player.HeldItem.type, player, _targetWorld);

		SoundEngine.PlaySound(new SoundStyle("MeleeWeaponEffects/Sounds/S2") { Volume = 0.36f }, player.Center);
		float randomizedRotation = _aimRotation + Main.rand.NextFloat(-0.5f, 0.5f);
		float length = Main.rand.Next(160, 220) / 110f * _weaponLength;
		float thicknessScale = 1f;
		float yScale = Main.rand.NextFloat(0.36f, 0.8f);
		int startRotation = Main.rand.NextBool() ? -2 : 2;
		ApplyExactProfileLengthAndWidth(ref length, ref thicknessScale);

		SlashArcProjectile.CreateSlash(
			isPlayerOwned: true,
			source: Projectile.GetSource_FromAI(),
			rotation: randomizedRotation,
			startingRotation: startRotation,
			length: length,
			thickness: thicknessScale,
			yScale: yScale,
			extraUpdates: Main.rand.Next(4, 6),
			damage: Projectile.damage,
			knockback: Projectile.knockBack,
			owner: player.whoAmI,
			color: Color.White,
			weaponItemType: _weaponItemType,
			knockbackRotation: Projectile.rotation - Projectile.ai[0],
			weaponScale: _weaponLength);
	}

	private void FireComboSchemeASlash(Player player, int useAnimation)
	{
		VanillaMeleeProjectileEmitter.Emit(this, charged: false, player.HeldItem.type, player, _targetWorld);

		int comboStepIndex = player.GetModPlayer<MeleeEffectsPlayer>().ConsumeNextSlashComboStep();
		ref readonly SlashComboStep step = ref Compact3DComboSchemeA.GetStep(comboStepIndex);
		SoundEngine.PlaySound(new SoundStyle("MeleeWeaponEffects/Sounds/S2") { Volume = 0.36f }, player.Center);

		float hitAngle = MathHelper.ToRadians(step.HitAngleDegrees);
		float baseRotation = _aimRotation - hitAngle;
		float startingRotation = MathHelper.ToRadians(step.StartAngleDegrees);
		float length = _weaponLength * LegacyAverageLengthScale * step.LengthScale;
		float thicknessScale = step.ThicknessScale;
		float yScale = RuntimeYScaleForStep(in step);
		ApplyExactProfileLengthAndWidth(ref length, ref thicknessScale);

		SlashArcProjectile.CreateProfiledSlash(
			isPlayerOwned: true,
			source: Projectile.GetSource_FromAI(),
			rotation: baseRotation,
			startingRotation: startingRotation,
			length: length,
			thicknessScale: thicknessScale,
			yScale: yScale,
			extraUpdates: step.ExtraUpdates,
			damage: Projectile.damage,
			knockback: Projectile.knockBack,
			owner: player.whoAmI,
			ownerNPC: 0,
			weaponItemType: _weaponItemType,
			knockbackRotation: _aimRotation,
			visual: in step.Visual,
			hitProgress: (step.ActiveStart + step.ActiveEnd) * 0.5f);

		Projectile.netUpdate = true;
	}

	private static float RuntimeYScaleForStep(in SlashComboStep step)
	{
		return MathHelper.Clamp(step.Visual.YScale * 0.65f, 0.36f, 0.8f);
	}

	private void ApplyExactProfileLengthAndWidth(ref float length, ref float thicknessScale)
	{
		if (!SlashProfileResolver.TryGetExactProfile(_weaponItemType, out WeaponSlashProfile profile))
		{
			return;
		}

		length *= profile.Shape.LengthScale;
		thicknessScale *= profile.Shape.ThicknessScale;
	}

	private enum SlashEmissionMode
	{
		LegacyRandom,
		Compact3DComboSchemeA
	}
}
