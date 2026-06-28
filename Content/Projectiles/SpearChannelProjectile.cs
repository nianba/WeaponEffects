using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;

namespace WeaponEffects;

public class SpearChannelProjectile : ModProjectile
{
	private const int AimSyncInterval = 6;
	private const float AimSyncThreshold = 0.03f;

	private int _weaponItemType;
	private int _useAnimation;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;
	private float _lastSyncedAimRotation;
	private int _nextSpearTimer;

	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

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
		writer.Write(_nextSpearTimer);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_useAnimation = reader.ReadInt32();
		_weaponLength = reader.ReadSingle();
		_targetWorld = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		_aimRotation = reader.ReadSingle();
		_nextSpearTimer = reader.ReadInt32();
		_lastSyncedAimRotation = _aimRotation;
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
		int currentSpearInterval = Math.Max(1, _nextSpearTimer > 0 ? _nextSpearTimer : NormalSpearInterval);

		if (Projectile.owner == Main.myPlayer)
		{
			UpdateLocalAim(player);
		}

		player.itemAnimation = Math.Max(useAnimation, currentSpearInterval);
		player.itemTime = Math.Max(useAnimation, currentSpearInterval);
		player.heldProj = Projectile.whoAmI;
		Projectile.rotation = _aimRotation;

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

		if (_nextSpearTimer > 0)
		{
			_nextSpearTimer--;
		}

		if (Projectile.owner == Main.myPlayer)
		{
			if (_nextSpearTimer <= 0)
			{
				FireSpearStrike(player);
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 direction = (_targetWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
		_aimRotation = direction.ToRotation();
		if (Math.Abs(direction.X) > 0.001f)
		{
			player.direction = Math.Sign(direction.X);
		}

		float aimDelta = Math.Abs(MathHelper.WrapAngle(_aimRotation - _lastSyncedAimRotation));
		if (aimDelta >= AimSyncThreshold || Projectile.ai[1] % AimSyncInterval == 0f)
		{
			_lastSyncedAimRotation = _aimRotation;
			Projectile.netUpdate = true;
		}
	}

	private void FireSpearStrike(Player player)
	{
		WeaponEffectsPlayer effectsPlayer = player.GetModPlayer<WeaponEffectsPlayer>();
		int comboStepIndex = effectsPlayer.ConsumeNextSpearComboStep();
		ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(comboStepIndex);
		SpearComboBranch branch = step.Kind == SpearComboStepKind.Finisher
			? SpearMotion.SelectFinisherBranch(IsGrounded(player))
			: SpearComboBranch.None;

		int damage = Math.Max(1, (int)MathF.Round(NormalSpearDamage * step.Gameplay.DamageMultiplier));
		float knockback = SpearKnockback;

		SpearStrikeProjectile.Spawn(
			Projectile.GetSource_FromAI(),
			player.Center,
			player.whoAmI,
			_weaponItemType,
			comboStepIndex,
			branch,
			_aimRotation,
			_weaponLength,
			damage,
			knockback);

		SpearTrailGlowProjectile.Spawn(
			Projectile.GetSource_FromAI(),
			player.Center,
			player.whoAmI,
			_weaponItemType,
			comboStepIndex,
			branch,
			_aimRotation,
			_weaponLength);

		_nextSpearTimer = IntervalForStep(in step, branch);
		Projectile.netUpdate = true;
	}

	private int NormalSpearInterval
	{
		get
		{
			float multiplier = MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().NormalSlashIntervalMultiplier, 0.25f, 3f);
			float interval = Math.Max(1, _useAnimation) * multiplier;

			if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
			{
				Player player = Main.player[Projectile.owner];
				if (player.active)
				{
					float meleeSpeed = MathHelper.Clamp(player.GetAttackSpeed(DamageClass.Melee), 0.25f, 4f);
					interval /= meleeSpeed;
				}
			}

			return Math.Max(1, (int)MathF.Round(interval));
		}
	}

	private int IntervalForStep(in SpearActionStep step, SpearComboBranch branch)
	{
		float interval = NormalSpearInterval * step.Gameplay.GetTimeMultiplier(branch);
		return Math.Max(1, (int)MathF.Round(interval));
	}

	private int NormalSpearDamage
	{
		get
		{
			float multiplier = MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().NormalSlashDamageMultiplier, 0.1f, 3f);
			return Math.Max(1, (int)MathF.Round(Projectile.damage * multiplier));
		}
	}

	private float SpearKnockback => Projectile.knockBack * MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().SlashKnockbackMultiplier, 0f, 3f);

	private static bool IsGrounded(Player player)
	{
		return player.velocity.Y == 0f || player.sliding || player.mount.Active;
	}
}
