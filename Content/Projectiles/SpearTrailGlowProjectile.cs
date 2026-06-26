using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
	private const int TrailSamples = 8;
	private const int SweepArcSamples = 18;
	private const int SweepArcMaxVertices = SweepArcSamples * 2;
	private static Asset<Effect> _sweepArcEffect;

	private readonly SlashVertex[] _sweepArcVertices = new SlashVertex[SweepArcMaxVertices];
	private int _weaponItemType;
	private int _comboStepIndex;
	private SpearComboBranch _branch;
	private float _aimRotation;
	private float _weaponLength;
	private int _totalLifetimeUpdates;
	private int _age;

	public override string Texture => "Terraria/Images/Extra_193";

	public static void Spawn(
		IEntitySource source,
		XnaVector2 position,
		int owner,
		int weaponItemType,
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
			trail.Initialize(weaponItemType, comboStepIndex, branch, aimRotation, weaponLength);
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

			if (visualConfig.DrawSpearTipTrail && ShouldDrawSpearTipGlow())
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

		DrawSweepArcVertices(currentProgress, in settings, motionAlpha);
	}

	private bool ShouldDrawSpearTipGlow()
	{
		ref readonly SpearComboStep step = ref TridentSpearComboScheme.GetStep(_comboStepIndex);
		return step.Kind is not SpearComboStepKind.RisingLift and not SpearComboStepKind.Backsweep;
	}

	private XnaVector2 SweepTangentAt(float progress)
	{
		float delta = 0.018f;
		XnaVector2 previous = EvaluatePoseAt(MathHelper.Clamp(progress - delta, 0f, 1f)).Tip;
		XnaVector2 next = EvaluatePoseAt(MathHelper.Clamp(progress + delta, 0f, 1f)).Tip;
		return next - previous;
	}

	private int BuildSweepArcVertices(float currentProgress, in SweepArcSettings settings, float motionAlpha, SweepArcPass pass)
	{
		int vertexCount = 0;
		GetSweepArcPassSettings(pass, out float alphaScale, out float widthScale, out float offsetScale, out float progressLag, out Color passColor);

		int activeSamples = Math.Max(4, Math.Min(SweepArcSamples, settings.SampleCount));
		for (int i = 0; i < activeSamples; i++)
		{
			float rawTrailPosition = activeSamples == 1 ? 0f : i / (float)(activeSamples - 1);
			float trailPosition = FrontOpenedTrailPosition(rawTrailPosition);
			float sampleProgress = MathHelper.Clamp(currentProgress - progressLag - settings.ProgressWindow * (1f - trailPosition), 0f, 1f);
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
			float width = settings.Width * widthFactor * widthScale;
			float alpha = motionAlpha * alphaScale * SweepAlphaFactor(rawTrailPosition);
			Color color = passColor * alpha;
			XnaVector2 screenPoint = point - Main.screenPosition + normal * settings.Width * widthFactor * offsetScale;
			float texX = MathHelper.Clamp(1f - rawTrailPosition, 0.08f, 0.92f);

			if (vertexCount + 2 > _sweepArcVertices.Length)
			{
				break;
			}

			_sweepArcVertices[vertexCount++] = new SlashVertex(screenPoint + normal * width, new Vector3(texX, 0f, 1f), color);
			_sweepArcVertices[vertexCount++] = new SlashVertex(screenPoint - normal * width, new Vector3(texX, 1f, 1f), color);
		}

		return vertexCount;
	}

	private void DrawSweepArcVertices(float currentProgress, in SweepArcSettings settings, float motionAlpha)
	{
		GraphicsDevice device = Main.graphics.GraphicsDevice;
		Effect effect = GetSweepArcEffect();
		Texture2D weaponTexture = GetWeaponTexture();
		if (effect == null || weaponTexture == null)
		{
			return;
		}

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		device.Textures[0] = TextureAssets.Projectile[Projectile.type].Value;
		device.Textures[1] = weaponTexture;
		effect.CurrentTechnique.Passes[0].Apply();
		DrawSweepArcPass(device, currentProgress, in settings, motionAlpha, SweepArcPass.TrailEcho);
		DrawSweepArcPass(device, currentProgress, in settings, motionAlpha, SweepArcPass.Main);
		DrawSweepArcPass(device, currentProgress, in settings, motionAlpha, SweepArcPass.Core);
		DrawSweepArcPass(device, currentProgress, in settings, motionAlpha, SweepArcPass.NearEdge);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	}

	private void DrawSweepArcPass(GraphicsDevice device, float currentProgress, in SweepArcSettings settings, float motionAlpha, SweepArcPass pass)
	{
		int vertexCount = BuildSweepArcVertices(currentProgress, in settings, motionAlpha, pass);
		if (vertexCount >= 4)
		{
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, _sweepArcVertices, 0, vertexCount - 2);
		}
	}

	private static Effect GetSweepArcEffect()
	{
		if (Main.dedServ)
		{
			return null;
		}

		_sweepArcEffect ??= ModContent.Request<Effect>("WeaponEffects/Effects/Mhd", AssetRequestMode.ImmediateLoad);
		return _sweepArcEffect.Value;
	}

	private Texture2D GetWeaponTexture()
	{
		if (_weaponItemType <= 0 || _weaponItemType >= TextureAssets.Item.Length)
		{
			return null;
		}

		return TextureAssets.Item[_weaponItemType].Value;
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
		float alpha = 0.08f;
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

	private float TrailSampleSpacing => _comboStepIndex == 1 || _comboStepIndex == 2 ? 0.09f : 0.035f;

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

	private static float FrontOpenedTrailPosition(float position)
	{
		return 0.12f + MathHelper.Clamp(position, 0f, 1f) * 0.88f;
	}

	private static float SweepAlphaFactor(float position)
	{
		position = MathHelper.Clamp(position, 0f, 1f);
		float trailFade = MathHelper.Lerp(0.2f, 1f, position);
		float endFade = Smooth01(MathHelper.Clamp((1f - position) / 0.12f, 0f, 1f));
		return trailFade * endFade;
	}

	private static void GetSweepArcPassSettings(SweepArcPass pass, out float alphaScale, out float widthScale, out float offsetScale, out float progressLag, out Color passColor)
	{
		alphaScale = 1f;
		widthScale = 1f;
		offsetScale = 0f;
		progressLag = 0f;
		passColor = Color.White;

		switch (pass)
		{
			case SweepArcPass.TrailEcho:
				alphaScale = 0.16f;
				widthScale = 0.62f;
				offsetScale = -0.16f;
				progressLag = 0.06f;
				passColor = Color.Lerp(Color.White, Color.Black, 0.12f);
				break;
			case SweepArcPass.Core:
				alphaScale = 0.52f;
				widthScale = 0.24f;
				offsetScale = 0.08f;
				break;
			case SweepArcPass.NearEdge:
				alphaScale = 0.42f;
				widthScale = 0.14f;
				offsetScale = 0.58f;
				break;
		}
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

		private SweepArcSettings(int sampleCount, float progressWindow, float width, float alpha, float fadeInEnd, float fadeOutStart)
		{
			Enabled = true;
			SampleCount = sampleCount;
			ProgressWindow = progressWindow;
			Width = width;
			Alpha = alpha;
			FadeInEnd = fadeInEnd;
			FadeOutStart = fadeOutStart;
		}

		public static SweepArcSettings ForStep(SpearComboStepKind kind)
		{
			return kind switch
			{
				SpearComboStepKind.RisingLift => new SweepArcSettings(
					sampleCount: 14,
					progressWindow: 0.42f,
					width: 22f,
					alpha: 0.68f,
					fadeInEnd: 0.24f,
					fadeOutStart: 0.46f),

				SpearComboStepKind.Backsweep => new SweepArcSettings(
					sampleCount: 18,
					progressWindow: 0.72f,
					width: 30f,
					alpha: 0.62f,
					fadeInEnd: 0.18f,
					fadeOutStart: 0.56f),

				_ => default
			};
		}
	}

	private enum SweepArcPass
	{
		TrailEcho,
		Main,
		Core,
		NearEdge
	}
}
