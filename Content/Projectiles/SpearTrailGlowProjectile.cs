using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;
using NumericsVector2 = System.Numerics.Vector2;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace WeaponEffects;

public class SpearTrailGlowProjectile : ModProjectile
{
	private const int TrailLifetimeTicks = 26;
	private const int TrailSamples = 5;

	private int _comboStepIndex;
	private SpearComboBranch _branch;
	private float _aimRotation;
	private float _weaponLength;
	private int _totalLifetimeUpdates;
	private int _age;

	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WoodenArrowFriendly;

	public static void Spawn(
		IEntitySource source,
		XnaVector2 position,
		int owner,
		int comboStepIndex,
		SpearComboBranch branch,
		float aimRotation,
		float weaponLength)
	{
		Projectile projectile = Projectile.NewProjectileDirect(
			source,
			position,
			XnaVector2.Zero,
			ModContent.ProjectileType<SpearTrailGlowProjectile>(),
			0,
			0f,
			owner);

		if (projectile.ModProjectile is SpearTrailGlowProjectile trail)
		{
			trail.Initialize(comboStepIndex, branch, aimRotation, weaponLength);
		}

		MeleeEffectAssets.SyncProjectile(projectile);
	}

	public void Initialize(int comboStepIndex, SpearComboBranch branch, float aimRotation, float weaponLength)
	{
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

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = TrailLifetimeTicks;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_comboStepIndex);
		writer.Write((int)_branch);
		writer.Write(_aimRotation);
		writer.Write(_weaponLength);
		writer.Write(_totalLifetimeUpdates);
		writer.Write(_age);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
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

	public override void AI()
	{
		Player player = Main.player[Projectile.owner];
		if (!player.active || player.dead)
		{
			Projectile.Kill();
			return;
		}

		Projectile.Center = player.Center;
		Projectile.velocity = XnaVector2.Zero;
		_age++;

		if (_age >= TotalLifetimeUpdates)
		{
			Projectile.Kill();
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		WeaponEffectsVisualConfig visualConfig = ModContent.GetInstance<WeaponEffectsVisualConfig>();
		Texture2D shaftTexture = MeleeEffectAssets.GetTexture(MeleeEffectAssets.SlashTexture);
		float currentProgress = CurrentProgress;

		for (int i = TrailSamples - 1; i >= 0; i--)
		{
			float sampleProgress = MathHelper.Clamp(currentProgress - i * 0.035f, 0f, 1f);
			SpearPoseXna pose = EvaluatePoseAt(sampleProgress);
			float fade = (1f - i / (float)TrailSamples) * (1f - currentProgress * 0.35f);
			if (visualConfig.DrawSpearShaftTrail)
			{
				DrawShaftTrail(shaftTexture, pose, fade, i);
			}

			if (visualConfig.DrawSpearTipTrail)
			{
				DrawSpearTipGlow(pose, fade, sampleProgress, i);
			}
		}

		return false;
	}

	private void DrawShaftTrail(Texture2D texture, SpearPoseXna pose, float fade, int sampleIndex)
	{
		bool airFinisher = _branch == SpearComboBranch.AirborneFinisher;
		if (!airFinisher || sampleIndex > 1)
		{
			return;
		}

		XnaVector2 shaft = pose.Tip - pose.Grip;
		float length = shaft.Length();
		if (length <= 1f)
		{
			return;
		}

		float width = 5f;
		Color color = new Color(210, 235, 255) * (fade * 0.08f);
		Rectangle source = new(0, 0, texture.Width, texture.Height);
		XnaVector2 scale = new(length / texture.Width, width / texture.Height);

		Main.EntitySpriteDraw(
			texture,
			(pose.Grip + pose.Tip) * 0.5f - Main.screenPosition,
			source,
			color,
			pose.Rotation,
			new XnaVector2(texture.Width * 0.5f, texture.Height * 0.5f),
			scale,
			SpriteEffects.None,
			0f);
	}

	private void DrawSpearTipGlow(SpearPoseXna pose, float fade, float progress, int sampleIndex)
	{
		if (sampleIndex > 0)
		{
			return;
		}

		XnaVector2 direction = pose.Tip - pose.Grip;
		float spearLength = direction.Length();
		if (spearLength <= 1f)
		{
			return;
		}

		direction /= spearLength;

		float extensionProgress = MathHelper.Clamp(progress, 0f, 1f);
		float extensionDistance = 10f + 30f * extensionProgress;
		float extensionSize = 10f + 10f * extensionProgress;
		float baseSpearHitboxDiagonal = MathF.Sqrt(22f * 22f + 2f * 2f);
		float extensionScale = extensionSize * MathF.Sqrt(2f) / baseSpearHitboxDiagonal;
		float glowStrength = Utils.Remap(extensionProgress, 0f, 0.3f, 0f, 1f) * Utils.Remap(extensionProgress, 0.3f, 1f, 1f, 0f);
		glowStrength = 1f - (1f - glowStrength) * (1f - glowStrength);
		glowStrength *= fade;
		if (glowStrength <= 0f)
		{
			return;
		}

		XnaVector2 grip = pose.Grip;
		XnaVector2 spearTip = pose.Tip;
		XnaVector2 extensionCenter = spearTip + direction * extensionDistance;
		float rotation = pose.Rotation + MathHelper.PiOver2;
		Texture2D glowTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		XnaVector2 glowOrigin = glowTexture.Size() * 0.5f;
		Color glowColor = new Color(245, 250, 255, 0) * glowStrength;

		DrawSpearTipGlowSegment(glowTexture, glowOrigin, XnaVector2.Lerp(extensionCenter, spearTip, 0.5f), glowColor, rotation, new XnaVector2(glowStrength * extensionScale, extensionScale) * extensionScale);
		DrawSpearTipGlowSegment(glowTexture, glowOrigin, XnaVector2.Lerp(extensionCenter, spearTip, 1f), glowColor, rotation, new XnaVector2(glowStrength * extensionScale, extensionScale * 1.5f) * extensionScale);
		DrawSpearTipGlowSegment(glowTexture, glowOrigin, XnaVector2.Lerp(grip, spearTip, extensionProgress * 1.5f - 0.5f) + new XnaVector2(0f, 2f), glowColor, rotation, new XnaVector2(glowStrength * extensionScale * glowStrength, extensionScale * 2f * glowStrength) * extensionScale);

		for (float amount = 0.4f; amount <= 1f; amount += 0.1f)
		{
			XnaVector2 position = XnaVector2.Lerp(grip, extensionCenter, amount + 0.2f) + new XnaVector2(0f, 2f);
			Color segmentColor = glowColor * 0.75f * amount;
			XnaVector2 segmentScale = new XnaVector2(glowStrength * extensionScale * glowStrength, extensionScale * 2f * glowStrength) * extensionScale;
			DrawSpearTipGlowSegment(glowTexture, glowOrigin, position, segmentColor, rotation, segmentScale);
		}
	}

	private static void DrawSpearTipGlowSegment(Texture2D texture, XnaVector2 origin, XnaVector2 worldPosition, Color color, float rotation, XnaVector2 scale)
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

		return new SpearPoseXna(ToXna(pose.Grip), ToXna(pose.Tip));
	}

	private XnaVector2 OwnerCenterWorld()
	{
		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			Player player = Main.player[Projectile.owner];
			if (player.active)
			{
				return player.Center;
			}
		}

		return Projectile.Center;
	}

	private int TotalLifetimeUpdates => _totalLifetimeUpdates > 0
		? _totalLifetimeUpdates
		: TrailLifetimeTicks * (Projectile.extraUpdates + 1);

	private float CurrentProgress => MathHelper.Clamp(_age / (float)Math.Max(1, TotalLifetimeUpdates), 0f, 1f);

	private static int ScaledLifetimeUpdates(in SpearComboStep step, SpearComboBranch branch)
	{
		float scaledTicks = TrailLifetimeTicks * step.GetTimeMultiplier(branch);
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

		public SpearPoseXna(XnaVector2 grip, XnaVector2 tip)
		{
			Grip = grip;
			Tip = tip;
		}

		public float Rotation => (Tip - Grip).ToRotation();
	}
}
