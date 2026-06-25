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
	private const int SweepArcSamples = 14;
	private const int SweepArcMaxVertices = SweepArcSamples * 2;
	private static BasicEffect _sweepArcEffect;
	private static GraphicsDevice _sweepArcEffectDevice;

	private readonly VertexPositionColor[] _sweepArcVertices = new VertexPositionColor[SweepArcMaxVertices];
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

		if (visualConfig.DrawSpearSweepArc)
		{
			DrawSweepArc(currentProgress);
		}

		for (int i = TrailSamples - 1; i >= 0; i--)
		{
			float sampleProgress = MathHelper.Clamp(currentProgress - i * TrailSampleSpacing, 0f, 1f);
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

	private void DrawSweepArc(float currentProgress)
	{
		ref readonly SpearComboStep step = ref TridentSpearComboScheme.GetStep(_comboStepIndex);
		SweepArcSettings settings = SweepArcSettings.ForStep(step.Kind);
		if (!settings.Enabled)
		{
			return;
		}

		float motionFadeIn = Smooth01(MathHelper.Clamp(currentProgress / settings.FadeInEnd, 0f, 1f));
		float motionFadeOut = Smooth01(MathHelper.Clamp((1f - currentProgress) / settings.FadeOutStart, 0f, 1f));
		float motionAlpha = motionFadeIn * motionFadeOut * settings.Alpha;
		if (motionAlpha <= 0.01f)
		{
			return;
		}

		int vertexCount = 0;
		int activeSamples = Math.Max(4, Math.Min(SweepArcSamples, settings.SampleCount));
		for (int i = 0; i < activeSamples; i++)
		{
			float trailPosition = activeSamples == 1 ? 0f : i / (float)(activeSamples - 1);
			float sampleProgress = MathHelper.Clamp(currentProgress - settings.ProgressWindow * (1f - trailPosition), 0f, 1f);
			SpearPoseXna pose = EvaluatePoseAt(sampleProgress);
			XnaVector2 point = pose.Tip;

			XnaVector2 tangent = SweepTangentAt(sampleProgress);
			if (tangent.LengthSquared() <= 0.001f)
			{
				continue;
			}

			tangent.Normalize();
			XnaVector2 normal = new(-tangent.Y, tangent.X);
			float widthFactor = SweepWidthFactor(trailPosition);
			float width = settings.Width * widthFactor;
			float alpha = motionAlpha * SweepAlphaFactor(trailPosition);
			Color color = Color.Lerp(settings.Color, Color.White, settings.WhiteMix * widthFactor) * alpha;
			XnaVector2 screenPoint = point - Main.screenPosition;

			if (vertexCount + 2 > _sweepArcVertices.Length)
			{
				break;
			}

			_sweepArcVertices[vertexCount++] = new VertexPositionColor(new Vector3(screenPoint + normal * width, 0f), color);
			_sweepArcVertices[vertexCount++] = new VertexPositionColor(new Vector3(screenPoint - normal * width, 0f), color * 0.78f);
		}

		if (vertexCount < 4)
		{
			return;
		}

		DrawSweepArcVertices(vertexCount);
	}

	private XnaVector2 SweepTangentAt(float progress)
	{
		float delta = 0.018f;
		XnaVector2 previous = EvaluatePoseAt(MathHelper.Clamp(progress - delta, 0f, 1f)).Tip;
		XnaVector2 next = EvaluatePoseAt(MathHelper.Clamp(progress + delta, 0f, 1f)).Tip;
		return next - previous;
	}

	private void DrawSweepArcVertices(int vertexCount)
	{
		GraphicsDevice device = Main.graphics.GraphicsDevice;
		BasicEffect effect = GetSweepArcEffect(device);
		effect.World = Matrix.Identity;
		effect.View = Main.GameViewMatrix.TransformationMatrix;
		effect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);

		Main.spriteBatch.End();
		device.BlendState = BlendState.Additive;
		device.DepthStencilState = DepthStencilState.None;
		device.RasterizerState = RasterizerState.CullNone;

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, _sweepArcVertices, 0, vertexCount - 2);
		}

		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	}

	private static BasicEffect GetSweepArcEffect(GraphicsDevice device)
	{
		if (_sweepArcEffect == null || _sweepArcEffectDevice != device)
		{
			_sweepArcEffect?.Dispose();
			_sweepArcEffect = new BasicEffect(device)
			{
				VertexColorEnabled = true,
				TextureEnabled = false
			};
			_sweepArcEffectDevice = device;
		}

		return _sweepArcEffect;
	}

	private void DrawShaftTrail(Texture2D texture, SpearPoseXna pose, float fade, int sampleIndex)
	{
		bool airFinisher = _branch == SpearComboBranch.AirborneFinisher;
		bool sweepStep = _comboStepIndex == 1 || _comboStepIndex == 2;
		int maxSampleIndex = airFinisher ? 1 : 3;
		if ((!airFinisher && !sweepStep) || sampleIndex > maxSampleIndex)
		{
			return;
		}

		XnaVector2 shaft = pose.Tip - pose.Grip;
		float length = shaft.Length();
		if (length <= 1f)
		{
			return;
		}

		float width = airFinisher ? 5f : (_comboStepIndex == 2 ? 4.5f : 4f);
		float alpha = airFinisher ? 0.08f : (_comboStepIndex == 2 ? 0.195f : 0.185f);
		Color color = new Color(210, 235, 255) * (fade * alpha);
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

	private float TrailSampleSpacing => _comboStepIndex == 1 || _comboStepIndex == 2 ? 0.12f : 0.035f;

	private static int ScaledLifetimeUpdates(in SpearComboStep step, SpearComboBranch branch)
	{
		float scaledTicks = TrailLifetimeTicks * step.GetTimeMultiplier(branch);
		return Math.Max(1, (int)MathF.Round(scaledTicks)) * (step.ExtraUpdates + 1);
	}

	private static float SweepWidthFactor(float position)
	{
		position = MathHelper.Clamp(position, 0f, 1f);
		float center = MathF.Sin(position * MathHelper.Pi);
		float leadingTip = Smooth01(MathHelper.Clamp(position / 0.16f, 0f, 1f));
		float trailingTip = Smooth01(MathHelper.Clamp((1f - position) / 0.2f, 0f, 1f));
		return MathHelper.Clamp(center * Math.Min(leadingTip, trailingTip), 0f, 1f);
	}

	private static float SweepAlphaFactor(float position)
	{
		position = MathHelper.Clamp(position, 0f, 1f);
		float trailFade = MathHelper.Lerp(0.2f, 1f, position);
		float endFade = Smooth01(MathHelper.Clamp((1f - position) / 0.12f, 0f, 1f));
		return trailFade * endFade;
	}

	private static float Smooth01(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
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

	private readonly struct SweepArcSettings
	{
		public readonly bool Enabled;
		public readonly int SampleCount;
		public readonly float ProgressWindow;
		public readonly float Width;
		public readonly float Alpha;
		public readonly float FadeInEnd;
		public readonly float FadeOutStart;
		public readonly float WhiteMix;
		public readonly Color Color;

		private SweepArcSettings(int sampleCount, float progressWindow, float width, float alpha, float fadeInEnd, float fadeOutStart, float whiteMix, Color color)
		{
			Enabled = true;
			SampleCount = sampleCount;
			ProgressWindow = progressWindow;
			Width = width;
			Alpha = alpha;
			FadeInEnd = fadeInEnd;
			FadeOutStart = fadeOutStart;
			WhiteMix = whiteMix;
			Color = color;
		}

		public static SweepArcSettings ForStep(SpearComboStepKind kind)
		{
			return kind switch
			{
				SpearComboStepKind.RisingLift => new SweepArcSettings(
					sampleCount: 10,
					progressWindow: 0.28f,
					width: 7.5f,
					alpha: 0.48f,
					fadeInEnd: 0.24f,
					fadeOutStart: 0.38f,
					whiteMix: 0.5f,
					color: new Color(150, 225, 255)),

				SpearComboStepKind.Backsweep => new SweepArcSettings(
					sampleCount: 14,
					progressWindow: 0.42f,
					width: 9.5f,
					alpha: 0.38f,
					fadeInEnd: 0.18f,
					fadeOutStart: 0.45f,
					whiteMix: 0.38f,
					color: new Color(120, 210, 255)),

				_ => default
			};
		}
	}
}
