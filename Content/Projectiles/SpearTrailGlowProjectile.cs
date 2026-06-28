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
	private const int TrailSamples = 8;
	private const int SweepArcSamples = 30;
	private const int SweepArcMaxVertices = SweepArcSamples * 2;
	private const Player.CompositeArmStretchAmount SpearArmStretch = Player.CompositeArmStretchAmount.Full;
	private const float SweepTipEdgeInnerShaftAmount = 0.86f;
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
		ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(_comboStepIndex);
		Projectile.extraUpdates = step.Gameplay.ExtraUpdates;
		_totalLifetimeUpdates = ScaledLifetimeUpdates(in step, _branch);
		Projectile.timeLeft = TotalLifetimeUpdates + 2;
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = SpearActionScheme.LifetimeTicks;
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

		Projectile.Center = player.MountedCenter;
		Projectile.velocity = XnaVector2.Zero;
		_age++;

		if (_age >= TotalLifetimeUpdates)
		{
			Projectile.Kill();
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		SpearVisualProfile visualProfile = SpearVisualProfileResolver.Resolve(_weaponItemType);
		Texture2D shaftTexture = MeleeEffectAssets.GetTexture(MeleeEffectAssets.SlashTexture);
		float currentProgress = CurrentProgress;

		DrawSweepArc(currentProgress, in visualProfile);

		for (int i = TrailSamples - 1; i >= 0; i--)
		{
			float sampleProgress = MathHelper.Clamp(currentProgress - i * TrailSampleSpacing, 0f, 1f);
			SpearPoseXna pose = EvaluatePoseAt(sampleProgress);
			float fade = (1f - i / (float)TrailSamples) * (1f - currentProgress * 0.35f);
			DrawShaftTrail(shaftTexture, pose, fade, i);

			if (ShouldDrawSpearTipGlow())
			{
				DrawSpearTipGlow(pose, fade, sampleProgress, i, in visualProfile);
			}
		}

		return false;
	}

	private void DrawSweepArc(float currentProgress, in SpearVisualProfile visualProfile)
	{
		ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(_comboStepIndex);
		SpearSweepAfterimageProfile afterimage = step.Afterimage;
		if (!afterimage.Enabled)
		{
			return;
		}

		float motionFadeIn = Smooth01(MathHelper.Clamp(currentProgress / afterimage.VisualFadeInEnd, 0f, 1f));
		float motionFadeOut = Smooth01(MathHelper.Clamp((1f - currentProgress) / afterimage.VisualFadeOutStart, 0f, 1f));
		float motionAlpha = motionFadeIn * motionFadeOut * afterimage.VisualAlpha;
		if (motionAlpha <= 0.005f)
		{
			return;
		}

		DrawSweepArcVertices(currentProgress, in afterimage, motionAlpha, in visualProfile);
	}

	private bool ShouldDrawSpearTipGlow()
	{
		ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(_comboStepIndex);
		return SpearCollisionEnvelope.DrawsTipGlow(in step);
	}

	private XnaVector2 SweepTangentAt(float progress)
	{
		float delta = 0.018f;
		XnaVector2 previous = EvaluatePoseAt(MathHelper.Clamp(progress - delta, 0f, 1f)).Tip;
		XnaVector2 next = EvaluatePoseAt(MathHelper.Clamp(progress + delta, 0f, 1f)).Tip;
		return next - previous;
	}

	private int BuildSweepArcVertices(float currentProgress, in SpearSweepAfterimageProfile afterimage, float motionAlpha, SweepArcPass pass)
	{
		int vertexCount = 0;
		GetSweepArcPassSettings(pass, out float alphaScale, out float widthScale, out float offsetScale, out float progressLag, out Color passColor);

		int activeSamples = Math.Max(4, Math.Min(SweepArcSamples, afterimage.VisualSampleCount));
		for (int i = 0; i < activeSamples; i++)
		{
			float rawTrailPosition = activeSamples == 1 ? 0f : i / (float)(activeSamples - 1);
			float trailPosition = FrontOpenedTrailPosition(rawTrailPosition);
			float sampleProgress = MathHelper.Clamp(currentProgress - progressLag - afterimage.VisualProgressWindow * (1f - trailPosition), 0f, 1f);
			SpearPoseXna pose = EvaluatePoseAt(sampleProgress);
			XnaVector2 shaft = pose.Tip - pose.Grip;
			float shaftLength = shaft.Length();
			if (shaftLength <= 1f)
			{
				continue;
			}

			XnaVector2 tangent = SweepTangentAt(sampleProgress);
			if (tangent.LengthSquared() <= 0.001f)
			{
				continue;
			}

			tangent.Normalize();
			XnaVector2 normal = new(-tangent.Y, tangent.X);
			XnaVector2 shaftDirection = shaft / shaftLength;
			XnaVector2 visualTip = pose.Grip + shaft * afterimage.VisualReachScale;
			float widthFactor = SweepWidthFactor(trailPosition);
			float edgeOffset = afterimage.VisualWidth * widthFactor * offsetScale;
			float outerExtension = afterimage.VisualWidth * widthFactor * widthScale * 0.16f;
			float innerExtension = afterimage.VisualWidth * widthFactor * widthScale * 0.08f;
			float alpha = motionAlpha * alphaScale * SweepAlphaFactor(rawTrailPosition);
			Color color = passColor * alpha;
			float innerShaftAmount = pass == SweepArcPass.NearEdge
				? SweepTipEdgeInnerShaftAmount
				: afterimage.VisualInnerShaftAmount;
			XnaVector2 innerPoint = XnaVector2.Lerp(pose.Grip, visualTip, innerShaftAmount);
			XnaVector2 outer = visualTip + normal * edgeOffset + shaftDirection * outerExtension - Main.screenPosition;
			XnaVector2 inner = innerPoint + normal * edgeOffset - shaftDirection * innerExtension - Main.screenPosition;
			float texX = MathHelper.Clamp(1f - rawTrailPosition, 0.08f, 0.92f);

			if (vertexCount + 2 > _sweepArcVertices.Length)
			{
				break;
			}

			_sweepArcVertices[vertexCount++] = new SlashVertex(outer, new Vector3(texX, 0f, 1f), color);
			_sweepArcVertices[vertexCount++] = new SlashVertex(inner, new Vector3(texX, 1f, 1f), color);
		}

		return vertexCount;
	}

	private void DrawSweepArcVertices(float currentProgress, in SpearSweepAfterimageProfile afterimage, float motionAlpha, in SpearVisualProfile visualProfile)
	{
		GraphicsDevice device = Main.graphics.GraphicsDevice;
		Effect effect = GetSweepArcEffect();
		Texture2D sweepTexture = MeleeEffectAssets.GetTexture(MeleeEffectAssets.SlashTexture);
		Texture2D colorTexture = MeleeEffectAssets.GetTexture(visualProfile.SweepColorTexturePath);
		if (effect == null || sweepTexture == null || colorTexture == null)
		{
			return;
		}

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		device.Textures[0] = sweepTexture;
		device.Textures[1] = colorTexture;
		effect.CurrentTechnique.Passes[0].Apply();
		DrawSweepArcPass(device, currentProgress, in afterimage, motionAlpha, SweepArcPass.Main);
		DrawSweepArcPass(device, currentProgress, in afterimage, motionAlpha, SweepArcPass.Core);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	}

	private void DrawSweepArcPass(GraphicsDevice device, float currentProgress, in SpearSweepAfterimageProfile afterimage, float motionAlpha, SweepArcPass pass)
	{
		int vertexCount = BuildSweepArcVertices(currentProgress, in afterimage, motionAlpha, pass);
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

	private void DrawSpearTipGlow(SpearPoseXna pose, float fade, float progress, int sampleIndex, in SpearVisualProfile visualProfile)
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
		ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(_comboStepIndex);
		SpearTipGlowProfile tipGlow = step.TipGlow;
		if (!tipGlow.Enabled || progress < tipGlow.StartProgress)
		{
			return;
		}

		float centerDistance = Math.Max(0f, tipGlow.CenterOffset);
		float extensionSize = tipGlow.SizeAt(progress);
		float baseSpearHitboxDiagonal = MathF.Sqrt(22f * 22f + 2f * 2f);
		float extensionScale = extensionSize * MathF.Sqrt(2f) / baseSpearHitboxDiagonal;
		float glowStrength = Utils.Remap(extensionProgress, 0f, 0.3f, 0f, 1f) * Utils.Remap(extensionProgress, 0.3f, 1f, 1f, 0f);
		glowStrength = 1f - (1f - glowStrength) * (1f - glowStrength);
		glowStrength *= fade;
		if (glowStrength <= 0f)
		{
			return;
		}

		XnaVector2 visibleSpearTip = VisibleHeldSpearTip(pose);
		float forwardExtent = tipGlow.ForwardExtentAt(progress);
		float visualBackDistance = Math.Max(0f, centerDistance - forwardExtent * tipGlow.BackExtentScale);
		float visualFrontDistance = tipGlow.FrontDistanceAt(progress);
		XnaVector2 glowBack = visibleSpearTip + direction * visualBackDistance;
		XnaVector2 glowCenter = visibleSpearTip + direction * centerDistance;
		XnaVector2 glowFront = visibleSpearTip + direction * visualFrontDistance;
		float rotation = pose.Rotation + MathHelper.PiOver2;
		Texture2D glowTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		XnaVector2 glowOrigin = glowTexture.Size() * 0.5f;
		Color glowColor = visualProfile.TipGlowColor * glowStrength;

		DrawSpearTipGlowSegment(glowTexture, glowOrigin, XnaVector2.Lerp(glowCenter, glowFront, 0.35f), glowColor, rotation, new XnaVector2(glowStrength * extensionScale * tipGlow.WidthScale, extensionScale * tipGlow.LengthScale) * extensionScale * tipGlow.UniformScale);
		DrawSpearTipGlowSegment(glowTexture, glowOrigin, glowCenter, glowColor, rotation, new XnaVector2(glowStrength * extensionScale * tipGlow.WidthScale, extensionScale * 1.5f * tipGlow.LengthScale) * extensionScale * tipGlow.UniformScale);

		for (float amount = 0f; amount <= 1f; amount += 0.125f)
		{
			XnaVector2 position = XnaVector2.Lerp(glowBack, glowFront, amount) + new XnaVector2(0f, 2f);
			Color segmentColor = glowColor * 0.75f * amount;
			XnaVector2 segmentScale = new XnaVector2(glowStrength * extensionScale * glowStrength * tipGlow.WidthScale, extensionScale * 2f * glowStrength * tipGlow.LengthScale) * extensionScale * tipGlow.UniformScale;
			DrawSpearTipGlowSegment(glowTexture, glowOrigin, position, segmentColor, rotation, segmentScale);
		}
	}

	private XnaVector2 VisibleHeldSpearTip(SpearPoseXna pose)
	{
		SpearHeldVisualProfile heldVisualProfile = SpearHeldVisualProfileResolver.Resolve(_weaponItemType);
		Texture2D weaponTexture = GetHeldWeaponTexture(ref heldVisualProfile);
		if (weaponTexture == null)
		{
			return pose.Tip;
		}

		return SpearHeldVisualMetrics.VisibleTip(pose.Grip, pose.Tip, weaponTexture, in heldVisualProfile);
	}

	private Texture2D GetHeldWeaponTexture(ref SpearHeldVisualProfile heldVisualProfile)
	{
		if (heldVisualProfile.TextureOverride.HasSource
			&& heldVisualProfile.TextureOverride.ProjectileType > 0
			&& heldVisualProfile.TextureOverride.ProjectileType < TextureAssets.Projectile.Length)
		{
			Texture2D projectileTexture = TextureAssets.Projectile[heldVisualProfile.TextureOverride.ProjectileType].Value;
			if (projectileTexture != null)
			{
				return projectileTexture;
			}
		}

		heldVisualProfile = SpearHeldVisualProfileResolver.ResolveVanillaFallback(_weaponItemType);
		return GetWeaponTexture();
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
		ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(_comboStepIndex);
		SpearPoseSnapshot pose = SpearMotion.EvaluatePose(
			in step,
			_branch,
			ToNumerics(OwnerCenterWorld()),
			_aimRotation,
			_weaponLength,
			progress);

		return AnchorPoseToPlayerFrontHand(new SpearPoseXna(ToXna(pose.Grip), ToXna(pose.Tip))).Translated(OwnerVisualOffset());
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

	private int TotalLifetimeUpdates => _totalLifetimeUpdates > 0
		? _totalLifetimeUpdates
		: SpearActionScheme.LifetimeTicks * (Projectile.extraUpdates + 1);

	private float CurrentProgress => MathHelper.Clamp(_age / (float)Math.Max(1, TotalLifetimeUpdates), 0f, 1f);

	private float TrailSampleSpacing
	{
		get
		{
			ref readonly SpearActionStep step = ref SpearActionScheme.GetStep(_comboStepIndex);
			return SpearCollisionEnvelope.TrailSampleSpacing(in step);
		}
	}

	private static int ScaledLifetimeUpdates(in SpearActionStep step, SpearComboBranch branch)
	{
		float scaledTicks = SpearActionScheme.LifetimeTicks * step.Gameplay.GetTimeMultiplier(branch);
		return Math.Max(1, (int)MathF.Round(scaledTicks)) * (step.Gameplay.ExtraUpdates + 1);
	}

	private static float SweepWidthFactor(float position)
	{
		return 1f;
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
		alphaScale = 0.48f;
		widthScale = 1f;
		offsetScale = 0f;
		progressLag = 0f;
		passColor = new Color(245, 238, 205);

		switch (pass)
		{
			case SweepArcPass.Core:
				alphaScale = 0.2f;
				widthScale = 0.2f;
				offsetScale = 0.08f;
				passColor = new Color(255, 250, 232);
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

		public SpearPoseXna Translated(XnaVector2 offset)
		{
			return new SpearPoseXna(Grip + offset, Tip + offset);
		}

		public float Rotation => (Tip - Grip).ToRotation();
	}

	private enum SweepArcPass
	{
		TrailEcho,
		Main,
		Core,
		NearEdge
	}
}
