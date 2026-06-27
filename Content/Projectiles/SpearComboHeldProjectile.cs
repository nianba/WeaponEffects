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

namespace WeaponEffects;

public class SpearComboHeldProjectile : ModProjectile
{
	private const int AimSyncIntervalTicks = 6;
	private const float AimSyncThreshold = 0.03f;
	private const int PoseHistoryLength = 30;
	private const int TransitionTicks = 3;
	private const Player.CompositeArmStretchAmount SpearArmStretch = Player.CompositeArmStretchAmount.Full;
	private static readonly Color SpearTipGlowColor = new(250, 236, 182, 0);

	private readonly PoseHistoryEntry[] _poseHistory = new PoseHistoryEntry[PoseHistoryLength];
	private int _weaponItemType;
	private int _useAnimation;
	private int _baseDamage;
	private float _baseKnockback;
	private float _weaponLength;
	private Vector2 _targetWorld;
	private float _aimRotation;
	private float _lastSyncedAimRotation;
	private float _stepAimRotation;
	private int _comboStepIndex;
	private SpearComboBranch _branch;
	private int _stepAgeUpdates;
	private int _stepDurationUpdates;
	private int _stepDurationTicks;
	private int _stepSerial;
	private int _poseHistoryHead;
	private int _poseHistoryCount;
	private bool _started;
	private bool _playedSwingSound;
	private bool _hasTransitionPose;
	private SpearPoseXna _transitionStartPose;
	private SpearPoseXna _currentPose;

	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		_weaponItemType = weaponItemType;
		_useAnimation = Math.Max(1, useAnimation);
		_baseDamage = Math.Max(1, Projectile.damage);
		_baseKnockback = Projectile.knockBack;
		_weaponLength = Math.Max(1f, weaponLength);
		_targetWorld = targetWorld;
		_aimRotation = (targetWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * Math.Sign(Projectile.ai[0])).ToRotation();
		_stepAimRotation = _aimRotation;
		_lastSyncedAimRotation = _aimRotation;

		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			BeginNextStep(Main.player[Projectile.owner], keepTransition: false);
		}

		Projectile.netUpdate = true;
	}

	public void RefreshWeaponState(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		_weaponItemType = weaponItemType;
		_useAnimation = Math.Max(1, useAnimation);
		_weaponLength = Math.Max(1f, weaponLength);
		_targetWorld = targetWorld;
		Projectile.timeLeft = 2;
		Projectile.netUpdate = true;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 420;
		Projectile.height = 420;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.timeLeft = 100;
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
		writer.Write(_useAnimation);
		writer.Write(_baseDamage);
		writer.Write(_baseKnockback);
		writer.Write(_weaponLength);
		writer.Write(_targetWorld.X);
		writer.Write(_targetWorld.Y);
		writer.Write(_aimRotation);
		writer.Write(_lastSyncedAimRotation);
		writer.Write(_stepAimRotation);
		writer.Write(_comboStepIndex);
		writer.Write((int)_branch);
		writer.Write(_stepAgeUpdates);
		writer.Write(_stepDurationUpdates);
		writer.Write(_stepDurationTicks);
		writer.Write(_stepSerial);
		writer.Write(_started);
		writer.Write(_playedSwingSound);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_weaponItemType = reader.ReadInt32();
		_useAnimation = reader.ReadInt32();
		_baseDamage = reader.ReadInt32();
		_baseKnockback = reader.ReadSingle();
		_weaponLength = reader.ReadSingle();
		_targetWorld = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		_aimRotation = reader.ReadSingle();
		_lastSyncedAimRotation = reader.ReadSingle();
		_stepAimRotation = reader.ReadSingle();
		_comboStepIndex = reader.ReadInt32();
		_branch = (SpearComboBranch)reader.ReadInt32();
		_stepAgeUpdates = reader.ReadInt32();
		_stepDurationUpdates = reader.ReadInt32();
		_stepDurationTicks = reader.ReadInt32();
		_stepSerial = reader.ReadInt32();
		_started = reader.ReadBoolean();
		_playedSwingSound = reader.ReadBoolean();
		if (_started)
		{
			ApplyStepDamage(in CurrentStep);
		}
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override bool? CanDamage()
	{
		return _started && IsCurrentStepActive(CurrentProgress);
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		return _started && CollisionAreaIntersects(targetHitbox);
	}

	public override void AI()
	{
		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			Projectile.Kill();
			return;
		}

		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead || !CanContinueAction(player))
		{
			Projectile.Kill();
			return;
		}

		if (!_started)
		{
			BeginNextStep(player, keepTransition: false);
		}

		if (Projectile.owner == Main.myPlayer)
		{
			UpdateLocalAim(player);
		}

		Projectile.Center = player.MountedCenter;
		Projectile.velocity = Vector2.Zero;
		Projectile.rotation = _stepAimRotation;
		Projectile.timeLeft = 2;

		_currentPose = EvaluateVisualPoseAt(CurrentProgress);
		PushPoseHistory(_currentPose, CurrentProgress);
		ApplyPlayerUsePose(player, _currentPose);
		PlaySwingSoundAtRelease(player, CurrentProgress);

		_stepAgeUpdates++;
		if (_stepAgeUpdates >= Math.Max(1, _stepDurationUpdates))
		{
			if (IsHoldingSpearUse(player))
			{
				BeginNextStep(player, keepTransition: true);
			}
			else
			{
				Projectile.Kill();
			}
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
			MeleeEffectAssets.NewProjectileDirect(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<SlashHitEffectProjectile>(), 0, 0f, Projectile.owner, Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
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
		if (Main.dedServ || !_started)
		{
			return false;
		}

		WeaponEffectsVisualConfig visualConfig = ModContent.GetInstance<WeaponEffectsVisualConfig>();
		if (visualConfig.DrawSpearShaftTrail || visualConfig.DrawSpearTipTrail)
		{
			DrawPoseHistoryTrail(visualConfig);
		}

		if (visualConfig.DrawHeldWeapon && visualConfig.DrawSpearHeldWeapon)
		{
			DrawHeldWeapon(_currentPose, lightColor);
		}

		return false;
	}

	private void BeginNextStep(Player player, bool keepTransition)
	{
		if (keepTransition && _poseHistoryCount > 0)
		{
			_transitionStartPose = _currentPose;
			_hasTransitionPose = true;
		}
		else
		{
			_hasTransitionPose = false;
		}

		WeaponEffectsPlayer effectsPlayer = player.GetModPlayer<WeaponEffectsPlayer>();
		_comboStepIndex = effectsPlayer.ConsumeNextSpearComboStep();
		ref readonly SpearComboStep step = ref TridentSpearComboScheme.GetStep(_comboStepIndex);
		_branch = step.Kind == SpearComboStepKind.Finisher
			? SpearMotion.SelectFinisherBranch(IsGrounded(player))
			: SpearComboBranch.None;
		_stepAimRotation = _aimRotation;
		Projectile.extraUpdates = step.ExtraUpdates;
		_stepDurationTicks = IntervalForStep(in step, _branch);
		_stepDurationUpdates = _stepDurationTicks * (Projectile.extraUpdates + 1);
		_stepAgeUpdates = 0;
		_stepSerial++;
		_started = true;
		_playedSwingSound = false;
		ApplyStepDamage(in step);
		ResetLocalNpcImmunity();
		Projectile.netUpdate = true;
	}

	private void ApplyStepDamage(in SpearComboStep step)
	{
		float damageMultiplier = MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().NormalSlashDamageMultiplier, 0.1f, 3f);
		float knockbackMultiplier = MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsGameplayConfig>().SlashKnockbackMultiplier, 0f, 3f);
		Projectile.damage = Math.Max(1, (int)MathF.Round(_baseDamage * damageMultiplier * step.DamageMultiplier));
		Projectile.knockBack = _baseKnockback * knockbackMultiplier;
	}

	private void ResetLocalNpcImmunity()
	{
		for (int i = 0; i < Projectile.localNPCImmunity.Length; i++)
		{
			Projectile.localNPCImmunity[i] = 0;
		}
	}

	private void UpdateLocalAim(Player player)
	{
		_targetWorld = Main.MouseWorld;
		Vector2 fallback = Vector2.UnitX * (player.direction == 0 ? 1 : player.direction);
		Vector2 direction = (_targetWorld - player.MountedCenter).SafeNormalize(fallback);
		_aimRotation = direction.ToRotation();
		if (Math.Abs(direction.X) > 0.001f)
		{
			player.direction = Math.Sign(direction.X);
		}

		int syncInterval = Math.Max(1, AimSyncIntervalTicks * (Projectile.extraUpdates + 1));
		float aimDelta = Math.Abs(MathHelper.WrapAngle(_aimRotation - _lastSyncedAimRotation));
		if (aimDelta >= AimSyncThreshold || _stepAgeUpdates % syncInterval == 0)
		{
			_lastSyncedAimRotation = _aimRotation;
			Projectile.netUpdate = true;
		}
	}

	private void ApplyPlayerUsePose(Player player, SpearPoseXna pose)
	{
		int itemUseTicks = Math.Max(Math.Max(1, _useAnimation), Math.Max(1, _stepDurationTicks));
		player.heldProj = Projectile.whoAmI;
		player.itemAnimation = itemUseTicks;
		player.itemTime = itemUseTicks;
		player.itemRotation = player.direction > 0 ? pose.Rotation : pose.Rotation + MathHelper.Pi;
		player.SetCompositeArmFront(true, SpearArmStretch, FrontArmRotation(pose.Rotation));
	}

	private bool CollisionAreaIntersects(Rectangle targetHitbox)
	{
		ref readonly SpearComboStep step = ref CurrentStep;
		float currentProgress = CurrentProgress;
		if (!SpearCollisionEnvelope.CanSampleCollisionAt(in step, currentProgress))
		{
			return false;
		}

		Vector2 targetPosition = targetHitbox.TopLeft();
		Vector2 targetSize = targetHitbox.Size();
		float sampleSpacing = SpearCollisionEnvelope.TrailSampleSpacing(in step);
		int sampleCount = SpearCollisionEnvelope.CollisionSampleCount(in step);

		int checkedSamples = 0;
		for (int i = 0; i < _poseHistoryCount && checkedSamples < sampleCount; i++)
		{
			PoseHistoryEntry entry = GetPoseHistoryFromNewest(i);
			if (entry.StepSerial != _stepSerial)
			{
				continue;
			}

			if (!SpearCollisionEnvelope.CanSampleCollisionAt(in step, entry.Progress))
			{
				continue;
			}

			checkedSamples++;
			if (PoseIntersectsTarget(targetPosition, targetSize, in step, entry.Pose, entry.Progress))
			{
				return true;
			}
		}

		if (checkedSamples == 0)
		{
			for (int i = 0; i < sampleCount; i++)
			{
				float progress = MathHelper.Clamp(currentProgress - i * sampleSpacing, 0f, 1f);
				if (!SpearCollisionEnvelope.CanSampleCollisionAt(in step, progress))
				{
					continue;
				}

				SpearPoseXna pose = EvaluatePoseAt(progress);
				if (PoseIntersectsTarget(targetPosition, targetSize, in step, pose, progress))
				{
					return true;
				}
			}
		}

		return false;
	}

	private bool PoseIntersectsTarget(Vector2 targetPosition, Vector2 targetSize, in SpearComboStep step, SpearPoseXna pose, float progress)
	{
		Vector2 collisionTip = CollisionTipForPose(in step, pose, progress, out float collisionWidth);
		float collisionPoint = 0f;
		return Collision.CheckAABBvLineCollision(targetPosition, targetSize, pose.Grip, collisionTip, collisionWidth, ref collisionPoint);
	}

	private Vector2 CollisionTipForPose(in SpearComboStep step, SpearPoseXna pose, float progress, out float collisionWidth)
	{
		Vector2 shaft = pose.Tip - pose.Grip;
		float spearLength = shaft.Length();
		collisionWidth = SpearCollisionEnvelope.CollisionWidth(in step);
		if (spearLength <= 1f)
		{
			return pose.Tip;
		}

		Vector2 direction = shaft / spearLength;
		Vector2 visibleTip = VisibleHeldSpearTip(pose, direction, spearLength);
		float reachScale = SpearCollisionEnvelope.CollisionReachScale(in step);
		Vector2 scaledTip = pose.Grip + (visibleTip - pose.Grip) * reachScale;
		float extension = SpearCollisionEnvelope.CollisionTipExtensionDistance(in step, progress);
		return scaledTip + direction * extension;
	}

	private SpearPoseXna EvaluateVisualPoseAt(float progress)
	{
		SpearPoseXna pose = EvaluatePoseAt(progress);
		if (!_hasTransitionPose)
		{
			return pose;
		}

		int transitionUpdates = Math.Max(1, TransitionTicks * (Projectile.extraUpdates + 1));
		if (_stepAgeUpdates >= transitionUpdates)
		{
			return pose;
		}

		float amount = Smooth01(_stepAgeUpdates / (float)transitionUpdates);
		return SpearPoseXna.Lerp(_transitionStartPose, pose, amount);
	}

	private SpearPoseXna EvaluatePoseAt(float progress)
	{
		SpearPoseSnapshot pose = SpearMotion.EvaluatePose(
			in CurrentStep,
			_branch,
			ToNumerics(OwnerCenterWorld()),
			_stepAimRotation,
			_weaponLength,
			progress);

		return AnchorPoseToPlayerFrontHand(new SpearPoseXna(ToXna(pose.Grip), ToXna(pose.Tip)));
	}

	private void PushPoseHistory(SpearPoseXna pose, float progress)
	{
		_poseHistory[_poseHistoryHead] = new PoseHistoryEntry(pose, _stepSerial, progress);
		_poseHistoryHead = (_poseHistoryHead + 1) % PoseHistoryLength;
		_poseHistoryCount = Math.Min(PoseHistoryLength, _poseHistoryCount + 1);
	}

	private void DrawPoseHistoryTrail(WeaponEffectsVisualConfig visualConfig)
	{
		if (_poseHistoryCount <= 0)
		{
			return;
		}

		Texture2D shaftTexture = visualConfig.DrawSpearShaftTrail
			? MeleeEffectAssets.GetTexture(MeleeEffectAssets.SlashTexture)
			: null;

		for (int i = _poseHistoryCount - 1; i >= 0; i--)
		{
			PoseHistoryEntry entry = GetPoseHistoryFromNewest(i);
			float age = i / (float)Math.Max(1, _poseHistoryCount - 1);
			float fade = (1f - age) * 0.22f;
			if (entry.StepSerial != _stepSerial)
			{
				fade *= 0.45f;
			}

			SpearPoseXna drawPose = entry.Pose.Translated(OwnerVisualOffset());
			if (visualConfig.DrawSpearShaftTrail && shaftTexture != null)
			{
				DrawShaftTrail(shaftTexture, drawPose, fade);
			}

			if (visualConfig.DrawSpearTipTrail && i < 8)
			{
				DrawSpearTipGlow(drawPose, fade, entry.Progress);
			}
		}
	}

	private PoseHistoryEntry GetPoseHistoryFromNewest(int newestOffset)
	{
		int index = _poseHistoryHead - 1 - newestOffset;
		while (index < 0)
		{
			index += PoseHistoryLength;
		}

		return _poseHistory[index % PoseHistoryLength];
	}

	private void DrawShaftTrail(Texture2D texture, SpearPoseXna pose, float fade)
	{
		Vector2 shaft = pose.Tip - pose.Grip;
		float length = shaft.Length();
		if (length <= 1f || fade <= 0f)
		{
			return;
		}

		float width = MathHelper.Lerp(3f, 8f, fade / 0.22f);
		Color color = new Color(245, 238, 205) * fade;
		Rectangle source = new(0, 0, texture.Width, texture.Height);
		Vector2 scale = new(length / Math.Max(1, texture.Width), width / Math.Max(1, texture.Height));

		Main.EntitySpriteDraw(
			texture,
			(pose.Grip + pose.Tip) * 0.5f - Main.screenPosition,
			source,
			color,
			pose.Rotation,
			new Vector2(texture.Width * 0.5f, texture.Height * 0.5f),
			scale,
			SpriteEffects.None,
			0f);
	}

	private void DrawSpearTipGlow(SpearPoseXna pose, float fade, float progress)
	{
		ref readonly SpearComboStep step = ref CurrentStep;
		SpearTipGlowProfile tipGlow = SpearTipGlowProfile.ForStep(in step);
		if (!tipGlow.Enabled || progress < tipGlow.StartProgress || fade <= 0f)
		{
			return;
		}

		Vector2 direction = pose.Tip - pose.Grip;
		float spearLength = direction.Length();
		if (spearLength <= 1f)
		{
			return;
		}

		direction /= spearLength;
		Vector2 visibleSpearTip = VisibleHeldSpearTip(pose, direction, spearLength);
		float centerDistance = Math.Max(0f, tipGlow.CenterOffset);
		float extensionSize = tipGlow.SizeAt(progress);
		float baseSpearHitboxDiagonal = MathF.Sqrt(22f * 22f + 2f * 2f);
		float extensionScale = extensionSize * MathF.Sqrt(2f) / baseSpearHitboxDiagonal;
		float glowStrength = Utils.Remap(progress, 0f, 0.3f, 0f, 1f) * Utils.Remap(progress, 0.3f, 1f, 1f, 0f);
		glowStrength = 1f - (1f - glowStrength) * (1f - glowStrength);
		glowStrength *= fade;
		if (glowStrength <= 0f)
		{
			return;
		}

		Vector2 glowCenter = visibleSpearTip + direction * centerDistance;
		Vector2 glowFront = visibleSpearTip + direction * tipGlow.FrontDistanceAt(progress);
		float rotation = pose.Rotation + MathHelper.PiOver2;
		Texture2D glowTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		Vector2 glowOrigin = glowTexture.Size() * 0.5f;
		Color glowColor = SpearTipGlowColor * glowStrength;
		Vector2 scale = new Vector2(glowStrength * extensionScale * tipGlow.WidthScale, extensionScale * tipGlow.LengthScale) * extensionScale * tipGlow.UniformScale;

		DrawSpearTipGlowSegment(glowTexture, glowOrigin, Vector2.Lerp(glowCenter, glowFront, 0.35f), glowColor, rotation, scale);
		DrawSpearTipGlowSegment(glowTexture, glowOrigin, glowCenter, glowColor * 0.85f, rotation, scale * new Vector2(0.8f, 1.35f));
	}

	private static void DrawSpearTipGlowSegment(Texture2D texture, Vector2 origin, Vector2 worldPosition, Color color, float rotation, Vector2 scale)
	{
		Main.EntitySpriteDraw(
			texture,
			worldPosition - Main.screenPosition,
			null,
			color,
			rotation,
			origin,
			scale,
			SpriteEffects.None,
			0f);
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

		Vector2 drawGrip = pose.Grip + OwnerVisualOffset();
		Vector2 drawTip = pose.Tip + OwnerVisualOffset();
		Vector2 shaft = drawTip - drawGrip;
		if (shaft.LengthSquared() <= 1f)
		{
			return;
		}

		Vector2 gripOrigin = SpearHeldVisualMetrics.GripOrigin(weaponTexture);
		Vector2 tipOrigin = SpearHeldVisualMetrics.TipOrigin(weaponTexture);
		Vector2 textureShaft = tipOrigin - gripOrigin;
		float textureGripToTipLength = SpearHeldVisualMetrics.TextureGripToTipLength(weaponTexture);
		float drawScale = SpearHeldVisualMetrics.DrawScale(shaft.Length(), textureGripToTipLength);
		float rotation = shaft.ToRotation() - textureShaft.ToRotation();

		Main.EntitySpriteDraw(
			weaponTexture,
			drawGrip - Main.screenPosition,
			null,
			lightColor,
			rotation,
			gripOrigin,
			drawScale,
			SpriteEffects.None,
			0f);
	}

	private Vector2 VisibleHeldSpearTip(SpearPoseXna pose, Vector2 direction, float shaftLength)
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

	private SpearPoseXna AnchorPoseToPlayerFrontHand(SpearPoseXna pose)
	{
		if (!TryGetOwner(out Player player))
		{
			return pose;
		}

		float armRotation = FrontArmRotation(pose.Rotation);
		Vector2 handPosition = player.GetFrontHandPosition(SpearArmStretch, armRotation);
		if (player.gravDir == -1f)
		{
			handPosition.Y = player.Bottom.Y + (player.position.Y - handPosition.Y);
		}

		return pose.Translated(handPosition - pose.Grip);
	}

	private Vector2 OwnerCenterWorld()
	{
		if (TryGetOwner(out Player player))
		{
			return player.MountedCenter;
		}

		return Projectile.Center;
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

	private Vector2 OwnerVisualOffset()
	{
		return TryGetOwner(out Player player) ? new Vector2(0f, player.gfxOffY) : Vector2.Zero;
	}

	private void PlaySwingSoundAtRelease(Player player, float progress)
	{
		if (_playedSwingSound)
		{
			return;
		}

		ref readonly SpearComboStep step = ref CurrentStep;
		float soundProgress = MathHelper.Clamp(step.ActiveStart + step.SwingSoundDelay, 0f, 1f);
		if (progress < soundProgress)
		{
			return;
		}

		_playedSwingSound = true;
		SoundStyle swingSound = new("WeaponEffects/Sounds/S2") { Volume = 0.32f };
		MeleeEffectAssets.PlaySound(in swingSound, player.Center);
		Projectile.netUpdate = true;
	}

	private bool CanContinueAction(Player player)
	{
		if (player.noItems || player.CCed)
		{
			return false;
		}

		Item item = player.HeldItem;
		return _weaponItemType > 0 && item != null && !item.IsAir && item.type == _weaponItemType;
	}

	private bool IsHoldingSpearUse(Player player)
	{
		return Projectile.owner != Main.myPlayer || player.channel;
	}

	private bool IsCurrentStepActive(float progress)
	{
		ref readonly SpearComboStep step = ref CurrentStep;
		if (!SpearCollisionEnvelope.CanSampleCollisionAt(in step, progress))
		{
			return false;
		}

		return true;
	}

	private int IntervalForStep(in SpearComboStep step, SpearComboBranch branch)
	{
		float interval = NormalSpearInterval * step.GetTimeMultiplier(branch);
		return Math.Max(1, (int)MathF.Round(interval));
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

	private void SpawnHitDust(NPC target, int dustType, int count, float minScale, float maxScale)
	{
		int scaledCount = MeleeEffectAssets.ScaleParticleCount(count);
		Vector2 tangent = _stepAimRotation.ToRotationVector2();
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

	private ref readonly SpearComboStep CurrentStep => ref TridentSpearComboScheme.GetStep(_comboStepIndex);

	private float CurrentProgress => MathHelper.Clamp(_stepAgeUpdates / (float)Math.Max(1, _stepDurationUpdates), 0f, 1f);

	private static bool IsGrounded(Player player)
	{
		return player.velocity.Y == 0f || player.sliding || player.mount.Active;
	}

	private static float FrontArmRotation(float spearRotation)
	{
		return spearRotation - MathHelper.PiOver2;
	}

	private static float Smooth01(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}

	private static NumericsVector2 ToNumerics(Vector2 value)
	{
		return new NumericsVector2(value.X, value.Y);
	}

	private static Vector2 ToXna(NumericsVector2 value)
	{
		return new Vector2(value.X, value.Y);
	}

	private readonly struct PoseHistoryEntry
	{
		public readonly SpearPoseXna Pose;
		public readonly int StepSerial;
		public readonly float Progress;

		public PoseHistoryEntry(SpearPoseXna pose, int stepSerial, float progress)
		{
			Pose = pose;
			StepSerial = stepSerial;
			Progress = progress;
		}
	}

	private readonly struct SpearPoseXna
	{
		public readonly Vector2 Grip;
		public readonly Vector2 Tip;

		public SpearPoseXna(Vector2 grip, Vector2 tip)
		{
			Grip = grip;
			Tip = tip;
		}

		public SpearPoseXna Translated(Vector2 offset)
		{
			return new SpearPoseXna(Grip + offset, Tip + offset);
		}

		public static SpearPoseXna Lerp(SpearPoseXna start, SpearPoseXna end, float amount)
		{
			return new SpearPoseXna(Vector2.Lerp(start.Grip, end.Grip, amount), Vector2.Lerp(start.Tip, end.Tip, amount));
		}

		public float Rotation => (Tip - Grip).ToRotation();
	}
}
