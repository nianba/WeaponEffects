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
using NumericsVector2 = System.Numerics.Vector2;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace WeaponEffects;

public class SpearStrikeProjectile : ModProjectile
{
	private const int CollisionSamples = 8;
	private const Player.CompositeArmStretchAmount SpearArmStretch = Player.CompositeArmStretchAmount.Full;

	private int _weaponItemType;
	private int _comboStepIndex;
	private SpearComboBranch _branch;
	private float _aimRotation;
	private float _weaponLength;
	private int _totalLifetimeUpdates;
	private int _age;

	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

	public static void Spawn(
		IEntitySource source,
		XnaVector2 position,
		int owner,
		int weaponItemType,
		int comboStepIndex,
		SpearComboBranch branch,
		float aimRotation,
		float weaponLength,
		int damage,
		float knockback)
	{
		Projectile projectile = Projectile.NewProjectileDirect(
			source,
			position,
			XnaVector2.Zero,
			ModContent.ProjectileType<SpearStrikeProjectile>(),
			damage,
			knockback,
			owner);

		if (projectile.ModProjectile is SpearStrikeProjectile spear)
		{
			spear.Initialize(weaponItemType, comboStepIndex, branch, aimRotation, weaponLength);
		}

		MeleeEffectAssets.SyncProjectile(projectile);
	}

	public void Initialize(int weaponItemType, int comboStepIndex, SpearComboBranch branch, float aimRotation, float weaponLength)
	{
		_weaponItemType = weaponItemType;
		_comboStepIndex = comboStepIndex;
		_branch = branch;
		_aimRotation = aimRotation;
		_weaponLength = Math.Max(1f, weaponLength);
		ref readonly SpearComboStep step = ref TridentSpearComboScheme.GetStep(_comboStepIndex);
		Projectile.extraUpdates = step.ExtraUpdates;
		_totalLifetimeUpdates = ScaledLifetimeUpdates(in step, _branch);
		Projectile.timeLeft = TotalLifetimeUpdates + 2;
		Projectile.netUpdate = true;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 420;
		Projectile.height = 420;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.timeLeft = SpearCollisionEnvelope.LifetimeTicks;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.penetrate = -1;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 1000;
		Projectile.noEnchantmentVisuals = true;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_weaponItemType);
		writer.Write(_comboStepIndex);
		writer.Write((int)_branch);
		writer.Write(_aimRotation);
		writer.Write(_weaponLength);
		writer.Write(_totalLifetimeUpdates);
		writer.Write(_age);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_comboStepIndex = reader.ReadInt32();
		_branch = (SpearComboBranch)reader.ReadInt32();
		_aimRotation = reader.ReadSingle();
		_weaponLength = reader.ReadSingle();
		_totalLifetimeUpdates = reader.ReadInt32();
		_age = reader.ReadInt32();
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override bool? CanDamage()
	{
		return true;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		return CollisionAreaIntersects(targetHitbox);
	}

	public override void AI()
	{
		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead)
		{
			Projectile.Kill();
			return;
		}

		Projectile.Center = player.MountedCenter;
		Projectile.velocity = XnaVector2.Zero;
		Projectile.rotation = _aimRotation;

		SpearPoseXna pose = EvaluatePoseAt(CurrentProgress);
		ApplyPlayerUsePose(player, pose);

		_age++;
		if (_age >= TotalLifetimeUpdates)
		{
			Projectile.Kill();
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Player player = Main.player[Projectile.owner];
		if (player.active && TryGetHeldSourceWeapon(player, out Item sourceItem))
		{
			ItemLoader.OnHitNPC(sourceItem, player, target, in hit, damageDone);
		}

		if (ModContent.GetInstance<WeaponEffectsVisualConfig>().DrawSpearHitFlash)
		{
			MeleeEffectAssets.NewProjectileDirect(Projectile.GetSource_FromAI(), target.Center, XnaVector2.Zero, ModContent.ProjectileType<SlashHitEffectProjectile>(), 0, 0f, Projectile.owner, Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
		}

		SoundStyle? targetHitSound = target.HitSound;
		if (targetHitSound.HasValue && targetHitSound.Value == SoundID.NPCHit4)
		{
			SoundStyle blockSound = new("WeaponEffects/Sounds/Block") { Volume = 0.24f };
			MeleeEffectAssets.PlaySound(in blockSound, target.Center);
			SpawnHitDust(target, DustID.Torch, 18, 1.1f, 2.6f);
		}
		else
		{
			SoundStyle hitSound = new("WeaponEffects/Sounds/Onhit")
			{
				Volume = 0.4f,
				Pitch = Main.rand.NextFloat(-0.12f, 0.12f)
			};
			MeleeEffectAssets.PlaySound(in hitSound, target.Center);
			SpawnHitDust(target, DustID.Blood, 26, 0.85f, 1.5f);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		WeaponEffectsVisualConfig visualConfig = ModContent.GetInstance<WeaponEffectsVisualConfig>();
		if (visualConfig.DrawHeldWeapon && visualConfig.DrawSpearHeldWeapon)
		{
			DrawHeldWeapon(EvaluatePoseAt(CurrentProgress), lightColor);
		}

		return false;
	}

	private bool CollisionAreaIntersects(Rectangle targetHitbox)
	{
		XnaVector2 targetPosition = targetHitbox.TopLeft();
		XnaVector2 targetSize = targetHitbox.Size();
		float collisionPoint = 0f;
		ref readonly SpearComboStep step = ref TridentSpearComboScheme.GetStep(_comboStepIndex);
		float sampleSpacing = SpearCollisionEnvelope.TrailSampleSpacing(in step);

		for (int i = 0; i < CollisionSamples; i++)
		{
			float progress = MathHelper.Clamp(CurrentProgress - i * sampleSpacing, 0f, 1f);
			SpearPoseXna pose = EvaluatePoseAt(progress);
			XnaVector2 collisionTip = CollisionTipForPose(in step, pose, progress, out float collisionWidth);

			collisionPoint = 0f;
			if (Collision.CheckAABBvLineCollision(targetPosition, targetSize, pose.Grip, collisionTip, collisionWidth, ref collisionPoint))
			{
				return true;
			}
		}

		return false;
	}

	private XnaVector2 CollisionTipForPose(in SpearComboStep step, SpearPoseXna pose, float progress, out float collisionWidth)
	{
		XnaVector2 shaft = pose.Tip - pose.Grip;
		float spearLength = shaft.Length();
		collisionWidth = SpearCollisionEnvelope.CollisionWidth(in step);
		if (spearLength <= 1f)
		{
			return pose.Tip;
		}

		XnaVector2 direction = shaft / spearLength;
		XnaVector2 visibleTip = VisibleHeldSpearTip(pose, direction, spearLength);
		float extension = SpearCollisionEnvelope.CollisionTipExtensionDistance(in step, progress);
		return visibleTip + direction * extension;
	}

	private SpearPoseXna EvaluatePoseAt(float progress)
	{
		ref readonly SpearComboStep step = ref TridentSpearComboScheme.GetStep(_comboStepIndex);
		SpearPoseSnapshot pose = SpearMotion.EvaluatePose(
			in step,
			_branch,
			ToNumerics(OwnerCenterWorld()),
			_aimRotation,
			_weaponLength,
			progress);

		return AnchorPoseToPlayerFrontHand(new SpearPoseXna(ToXna(pose.Grip), ToXna(pose.Tip), pose.CollisionWidth, pose.Active));
	}

	private XnaVector2 OwnerCenterWorld()
	{
		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			Player player = Main.player[Projectile.owner];
			if (player.active)
			{
				return player.MountedCenter;
			}
		}

		return Projectile.Center;
	}

	private SpearPoseXna AnchorPoseToPlayerFrontHand(SpearPoseXna pose)
	{
		if (!TryGetOwner(out Player player))
		{
			return pose;
		}

		float armRotation = FrontArmRotation(pose.Rotation);
		XnaVector2 handPosition = player.GetFrontHandPosition(SpearArmStretch, armRotation);
		if (player.gravDir == -1f)
		{
			handPosition.Y = player.Bottom.Y + (player.position.Y - handPosition.Y);
		}

		return pose.Translated(handPosition - pose.Grip);
	}

	private void ApplyPlayerUsePose(Player player, SpearPoseXna pose)
	{
		player.heldProj = Projectile.whoAmI;
		player.itemRotation = player.direction > 0 ? pose.Rotation : pose.Rotation + MathHelper.Pi;
		player.SetCompositeArmFront(true, SpearArmStretch, FrontArmRotation(pose.Rotation));
	}

	private static float FrontArmRotation(float spearRotation)
	{
		return spearRotation - MathHelper.PiOver2;
	}

	private bool TryGetOwner(out Player player)
	{
		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			player = Main.player[Projectile.owner];
			return player.active;
		}

		player = null;
		return false;
	}

	private XnaVector2 OwnerVisualOffset()
	{
		return TryGetOwner(out Player player) ? new XnaVector2(0f, player.gfxOffY) : XnaVector2.Zero;
	}

	private void DrawHeldWeapon(SpearPoseXna pose, Color lightColor)
	{
		if (_weaponItemType <= 0 || _weaponItemType >= TextureAssets.Item.Length)
		{
			return;
		}

		Texture2D weaponTexture = TextureAssets.Item[_weaponItemType].Value;
		if (weaponTexture == null)
		{
			return;
		}

		XnaVector2 shaft = pose.Tip - pose.Grip;
		if (shaft.LengthSquared() <= 1f)
		{
			return;
		}

		XnaVector2 gripOrigin = SpearHeldVisualMetrics.GripOrigin(weaponTexture);
		XnaVector2 tipOrigin = SpearHeldVisualMetrics.TipOrigin(weaponTexture);
		XnaVector2 textureShaft = tipOrigin - gripOrigin;
		float textureGripToTipLength = SpearHeldVisualMetrics.TextureGripToTipLength(weaponTexture);
		float drawScale = SpearHeldVisualMetrics.DrawScale(shaft.Length(), textureGripToTipLength);
		XnaVector2 drawPosition = pose.Grip + OwnerVisualOffset() - Main.screenPosition;
		float rotation = pose.Rotation - textureShaft.ToRotation();

		Main.EntitySpriteDraw(
			weaponTexture,
			drawPosition,
			null,
			lightColor,
			rotation,
			gripOrigin,
			drawScale,
			SpriteEffects.None,
			0f);
	}

	private XnaVector2 VisibleHeldSpearTip(SpearPoseXna pose, XnaVector2 direction, float shaftLength)
	{
		if (_weaponItemType <= 0 || _weaponItemType >= TextureAssets.Item.Length)
		{
			return pose.Tip;
		}

		Texture2D weaponTexture = TextureAssets.Item[_weaponItemType].Value;
		if (weaponTexture == null)
		{
			return pose.Tip;
		}

		return SpearHeldVisualMetrics.VisibleTip(pose.Grip, pose.Tip, weaponTexture);
	}

	private void SpawnHitDust(NPC target, int dustType, int count, float minScale, float maxScale)
	{
		int scaledCount = MeleeEffectAssets.ScaleParticleCount(count);
		XnaVector2 tangent = _aimRotation.ToRotationVector2();
		for (int i = 0; i < scaledCount; i++)
		{
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType, 0f, 0f, 0, default, Main.rand.NextFloat(minScale, maxScale));
			dust.velocity = (tangent + Main.rand.NextVector2Circular(0.45f, 0.45f)).SafeNormalize(tangent) * Main.rand.NextFloat(0.4f, 8f);
		}
	}

	private bool TryGetHeldSourceWeapon(Player player, out Item sourceItem)
	{
		sourceItem = player.HeldItem;
		return sourceItem != null && !sourceItem.IsAir && sourceItem.type == _weaponItemType;
	}

	private int TotalLifetimeUpdates => _totalLifetimeUpdates > 0
		? _totalLifetimeUpdates
		: SpearCollisionEnvelope.LifetimeTicks * (Projectile.extraUpdates + 1);

	private float CurrentProgress => MathHelper.Clamp(_age / (float)Math.Max(1, TotalLifetimeUpdates), 0f, 1f);

	private static int ScaledLifetimeUpdates(in SpearComboStep step, SpearComboBranch branch)
	{
		float scaledTicks = SpearCollisionEnvelope.LifetimeTicks * step.GetTimeMultiplier(branch);
		return Math.Max(1, (int)MathF.Round(scaledTicks)) * (step.ExtraUpdates + 1);
	}

	private static NumericsVector2 ToNumerics(XnaVector2 value)
	{
		return new NumericsVector2(value.X, value.Y);
	}

	private static XnaVector2 ToXna(NumericsVector2 value)
	{
		return new XnaVector2(value.X, value.Y);
	}

	private readonly struct SpearPoseXna
	{
		public readonly XnaVector2 Grip;
		public readonly XnaVector2 Tip;
		public readonly float CollisionWidth;
		public readonly bool Active;

		public SpearPoseXna(XnaVector2 grip, XnaVector2 tip, float collisionWidth, bool active)
		{
			Grip = grip;
			Tip = tip;
			CollisionWidth = collisionWidth;
			Active = active;
		}

		public SpearPoseXna Translated(XnaVector2 offset)
		{
			return new SpearPoseXna(Grip + offset, Tip + offset, CollisionWidth, Active);
		}

		public float Rotation => (Tip - Grip).ToRotation();
	}
}
