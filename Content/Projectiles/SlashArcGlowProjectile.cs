using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class SlashArcGlowProjectile : ModProjectile
{
	private readonly SlashVertex[] _vertices = new SlashVertex[96];
	private int _vertexCount;

	private bool _reverse;
	private bool _npcOwned;
	private Color _color = Color.White;
	private bool _usesVisualProfile;
	private int _profileAge;
	private float _profileXScale = 1f;
	private float _profileStartDepth;
	private float _profileHitDepth;
	private float _profileEndDepth;
	private float _profileHitProgress = 0.5f;
	private float _profileGlowAlpha = 1f;
	private float _profilePeakFlareAlpha;
	private float _profileNearEdgeOffsetPixels;

	public override string Texture => MeleeEffectAssets.SlashGlowTexture;

	public static void InitializeGlow(Projectile projectile, float startingRotation, float thickness, float yScale, int extraUpdates, bool npcOwned, Color color)
	{
		projectile.rotation = startingRotation;
		projectile.localAI[0] = thickness;
		projectile.localAI[1] = yScale;
		projectile.extraUpdates = extraUpdates;

		if (projectile.ModProjectile is SlashArcGlowProjectile glow)
		{
			glow._reverse = startingRotation > 0f;
			glow._npcOwned = npcOwned;
			glow._color = color == default ? Color.White : color;
			projectile.netUpdate = true;
		}
	}

	public static void InitializeProfiledGlow(Projectile projectile, float startingRotation, float thickness, float yScale, int extraUpdates, bool npcOwned, Color color, in SlashArcVisualProfile visual, float hitProgress)
	{
		InitializeGlow(projectile, startingRotation, thickness, yScale, extraUpdates, npcOwned, color);

		if (projectile.ModProjectile is SlashArcGlowProjectile glow)
		{
			glow._usesVisualProfile = true;
			glow._profileAge = 0;
			glow._profileXScale = MathHelper.Clamp(visual.XScale, 0.2f, 2f);
			glow._profileStartDepth = visual.StartDepth;
			glow._profileHitDepth = visual.HitDepth;
			glow._profileEndDepth = visual.EndDepth;
			glow._profileHitProgress = MathHelper.Clamp(hitProgress, 0.08f, 0.92f);
			glow._profileGlowAlpha = MathHelper.Clamp(visual.GlowAlpha, 0f, 1.5f);
			glow._profilePeakFlareAlpha = MathHelper.Clamp(visual.PeakFlareAlpha, 0f, 1.5f);
			glow._profileNearEdgeOffsetPixels = MathHelper.Clamp(visual.NearEdgeOffsetPixels, 0f, 24f);
			projectile.netUpdate = true;
		}
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.width = 24;
		Projectile.height = 24;
		Projectile.timeLeft = 100;
		Projectile.tileCollide = false;
		Projectile.penetrate = -1;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_reverse);
		writer.Write(_npcOwned);
		writer.Write(_color.PackedValue);
		writer.Write(_usesVisualProfile);
		writer.Write(_profileAge);
		writer.Write(_profileXScale);
		writer.Write(_profileStartDepth);
		writer.Write(_profileHitDepth);
		writer.Write(_profileEndDepth);
		writer.Write(_profileHitProgress);
		writer.Write(_profileGlowAlpha);
		writer.Write(_profilePeakFlareAlpha);
		writer.Write(_profileNearEdgeOffsetPixels);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_reverse = reader.ReadBoolean();
		_npcOwned = reader.ReadBoolean();
		_color = new Color { PackedValue = reader.ReadUInt32() };
		_usesVisualProfile = reader.ReadBoolean();
		_profileAge = reader.ReadInt32();
		_profileXScale = reader.ReadSingle();
		_profileStartDepth = reader.ReadSingle();
		_profileHitDepth = reader.ReadSingle();
		_profileEndDepth = reader.ReadSingle();
		_profileHitProgress = reader.ReadSingle();
		_profileGlowAlpha = reader.ReadSingle();
		_profilePeakFlareAlpha = reader.ReadSingle();
		_profileNearEdgeOffsetPixels = reader.ReadSingle();
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override void AI()
	{
		if (_usesVisualProfile)
		{
			_profileAge++;
		}

		if (Projectile.timeLeft > 40)
		{
			float rotationStep = MathHelper.Lerp(0.14f, 0f, 1f - (Projectile.timeLeft - 40) / 60f);
			Projectile.rotation += _reverse ? -rotationStep : rotationStep;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (ModContent.GetInstance<MeleeWeaponEffectsVisualConfig>().SlashStyle != 1)
		{
			return false;
		}

		BuildVertices(OwnerCenterWorld() - Main.screenPosition);
		if (_vertexCount < 3)
		{
			return false;
		}

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Projectile[Projectile.type].Value;
		Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, _vertexCount - 2);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		return false;
	}

	private Vector2 OwnerCenterWorld()
	{
		if (_npcOwned)
		{
			int npcIndex = (int)Projectile.ai[0];
			if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
			{
				return Main.npc[npcIndex].Center;
			}
		}

		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			return Main.player[Projectile.owner].Center;
		}

		return Projectile.Center;
	}

	private void BuildVertices(Vector2 ownerCenter)
	{
		_vertexCount = 0;
		float blink = ModContent.GetInstance<MeleeWeaponEffectsVisualConfig>().SlashBlink;
		int activeTrailCount = _usesVisualProfile ? CountActiveTrailSamples() : Projectile.oldPos.Length;
		if (_usesVisualProfile && activeTrailCount < 2)
		{
			return;
		}

		AddFrontTip(ownerCenter, blink);

		int activeIndex = 0;
		for (int i = 0; i < Projectile.oldPos.Length; i++)
		{
			if (Projectile.oldRot[i] == 0f)
			{
				continue;
			}

			float rawTrailPosition = _usesVisualProfile ? (activeIndex + 1f) / activeTrailCount : i / 40f;
			float trailPosition = _usesVisualProfile ? FrontOpenedTrailPosition(rawTrailPosition) : rawTrailPosition;
			float factor = 1f - rawTrailPosition;
			float progress = ProfileProgressAtTrailIndex(i);
			float depth = _usesVisualProfile ? EvaluateProfileDepth(progress) : 0f;
			float hitPeak = _usesVisualProfile ? EvaluateHitPeak(progress) : 0f;
			float nearAmount = MathHelper.Clamp((depth + 0.9f) / 2.4f, 0f, 1f);
			float depthScale = _usesVisualProfile ? 1f + MathHelper.Clamp(depth, -1.2f, 1.5f) * 0.1f : 1f;
			float glowScale = _usesVisualProfile ? 1.04f + nearAmount * 0.08f + hitPeak * 0.05f : 1f;
			float widthScale = _usesVisualProfile ? 1.08f + nearAmount * 0.16f : 1f;
			float crescent = _usesVisualProfile ? GlowCrescentWidthFactor(trailPosition) : 1f;
			float tipVisibility = _usesVisualProfile ? TipVisibilityFactor(trailPosition) : 0f;
			float tipConvergence = 1f - tipVisibility;
			glowScale = MathHelper.Lerp(1f, glowScale, tipConvergence);
			Vector2 outer = ProfileVector(Projectile.oldRot[i], Projectile.velocity.Length() * glowScale * depthScale);
			outer = outer.RotatedBy(Projectile.ai[1]);

			float width = MathHelper.Clamp(Projectile.localAI[0] * widthScale * crescent, 0f, 0.95f);
			float innerTaper = 0.25f + 0.75f * crescent;
			Vector2 inner = ProfileVector(Projectile.oldRot[i], Projectile.velocity.Length() * (glowScale - width + width * trailPosition * innerTaper) * depthScale);
			inner = inner.RotatedBy(Projectile.ai[1]);
			Vector2 offset = ProfileScreenOffset(outer, _usesVisualProfile ? _profileNearEdgeOffsetPixels * nearAmount * 0.55f * tipConvergence : 0f);

			if (_vertexCount + 2 > _vertices.Length)
			{
				break;
			}

			float alpha = _usesVisualProfile ? _profileGlowAlpha * (0.42f + nearAmount * 0.24f + _profilePeakFlareAlpha * hitPeak * 0.18f) : 1f;
			float crescentAlpha = _usesVisualProfile ? MathHelper.Lerp(0.16f, GlowCrescentAlphaFactor(crescent), tipConvergence) : 1f;
			float trailAlpha = _usesVisualProfile ? MathHelper.Lerp(0.18f, factor, tipConvergence) : factor;
			float frontAlpha = _usesVisualProfile ? Smooth01(MathHelper.Clamp(rawTrailPosition / 0.16f, 0f, 1f)) : 1f;
			Color color = _color * blink * trailAlpha * alpha * crescentAlpha * frontAlpha;
			float texX = _usesVisualProfile ? MathHelper.Clamp(factor, 0.08f, 0.92f) : factor;
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + outer + offset, new Vector3(texX, 0f, 1f), color);
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + inner + offset, new Vector3(texX, 1f, 1f), color);
			activeIndex++;
		}
	}

	private void AddFrontTip(Vector2 ownerCenter, float blink)
	{
		if (!_usesVisualProfile || _profileGlowAlpha <= 0f || _vertexCount + 2 > _vertices.Length)
		{
			return;
		}

		float progress = ProfileProgressAtTrailIndex(0);
		float depth = EvaluateProfileDepth(progress);
		float nearAmount = MathHelper.Clamp((depth + 0.9f) / 2.4f, 0f, 1f);
		float hitPeak = EvaluateHitPeak(progress);
		float depthScale = 1f + MathHelper.Clamp(depth, -1.2f, 1.5f) * 0.1f;
		float glowScale = 1.04f + nearAmount * 0.08f + hitPeak * 0.05f;
		Vector2 tip = ProfileVector(Projectile.rotation, Projectile.velocity.Length() * glowScale * depthScale).RotatedBy(Projectile.ai[1]);
		if (TryGetCurrentFrontTipDirection(Projectile.rotation, Projectile.velocity.Length() * glowScale * depthScale, out Vector2 frontTipDirection))
		{
			tip += frontTipDirection * Projectile.velocity.Length() * 0.045f;
		}

		float alpha = _profileGlowAlpha * (0.34f + nearAmount * 0.18f + _profilePeakFlareAlpha * hitPeak * 0.12f);
		Color color = _color * blink * alpha * 0.28f;
		_vertices[_vertexCount++] = new SlashVertex(ownerCenter + tip, new Vector3(0.92f, 0f, 1f), color);
		_vertices[_vertexCount++] = new SlashVertex(ownerCenter + tip, new Vector3(0.92f, 1f, 1f), color);
	}

	private bool TryGetCurrentFrontTipDirection(float currentRotation, float currentRadius, out Vector2 direction)
	{
		Vector2 current = ProfileVector(currentRotation, currentRadius).RotatedBy(Projectile.ai[1]);
		for (int i = 0; i < Projectile.oldRot.Length; i++)
		{
			if (Projectile.oldRot[i] == 0f)
			{
				continue;
			}

			Vector2 next = ProfileVector(Projectile.oldRot[i], currentRadius).RotatedBy(Projectile.ai[1]);
			direction = current - next;
			if (direction.LengthSquared() > 0.001f)
			{
				direction.Normalize();
				return true;
			}
		}

		direction = new Vector2(-current.Y, current.X);
		if (_reverse)
		{
			direction *= -1f;
		}

		if (direction.LengthSquared() <= 0.001f)
		{
			return false;
		}

		direction.Normalize();
		return true;
	}

	private static float FrontOpenedTrailPosition(float rawPosition)
	{
		rawPosition = MathHelper.Clamp(rawPosition, 0f, 1f);
		return MathHelper.Clamp(0.12f + rawPosition * 0.88f, 0f, 1f);
	}

	private Vector2 ProfileVector(float rotation, float radius)
	{
		Vector2 direction = rotation.ToRotationVector2();
		if (_usesVisualProfile)
		{
			direction.X *= _profileXScale;
		}

		direction.Y *= Projectile.localAI[1];
		return direction * radius;
	}

	private int CountActiveTrailSamples()
	{
		int count = 0;
		for (int i = 0; i < Projectile.oldRot.Length; i++)
		{
			if (Projectile.oldRot[i] != 0f)
			{
				count++;
			}
		}

		return count;
	}

	private static float GlowCrescentWidthFactor(float position)
	{
		position = MathHelper.Clamp(position, 0f, 1f);
		float centerWeight = System.MathF.Sin(position * MathHelper.Pi);
		float leadingTip = Smooth01(MathHelper.Clamp(position / 0.24f, 0f, 1f));
		float trailingTip = Smooth01(MathHelper.Clamp((1f - position) / 0.32f, 0f, 1f));
		float tipWeight = System.Math.Min(leadingTip, trailingTip);
		return MathHelper.Clamp(centerWeight * tipWeight, 0f, 1f);
	}

	private static float GlowCrescentAlphaFactor(float crescent)
	{
		return MathHelper.Lerp(0.24f, 0.86f, System.MathF.Sqrt(MathHelper.Clamp(crescent, 0f, 1f)));
	}

	private static float TipVisibilityFactor(float position)
	{
		float edgeDistance = System.Math.Min(position, 1f - position);
		return 1f - Smooth01(MathHelper.Clamp(edgeDistance / 0.14f, 0f, 1f));
	}

	private Vector2 ProfileScreenOffset(Vector2 outer, float offsetPixels)
	{
		if (offsetPixels == 0f)
		{
			return Vector2.Zero;
		}

		Vector2 normal = new(-outer.Y, outer.X);
		if (normal.LengthSquared() < 0.001f)
		{
			return Vector2.Zero;
		}

		normal.Normalize();
		if (_reverse)
		{
			normal *= -1f;
		}

		return normal * offsetPixels;
	}

	private float ProfileProgressAtTrailIndex(int trailIndex)
	{
		return MathHelper.Clamp((_profileAge - trailIndex) / 60f, 0f, 1f);
	}

	private float EvaluateProfileDepth(float progress)
	{
		if (!_usesVisualProfile)
		{
			return 0f;
		}

		if (progress <= _profileHitProgress)
		{
			return MathHelper.Lerp(_profileStartDepth, _profileHitDepth, Smooth01(progress / _profileHitProgress));
		}

		return MathHelper.Lerp(_profileHitDepth, _profileEndDepth, Smooth01((progress - _profileHitProgress) / (1f - _profileHitProgress)));
	}

	private float EvaluateHitPeak(float progress)
	{
		float distance = System.Math.Abs(progress - _profileHitProgress);
		float peak = 1f - MathHelper.Clamp(distance / 0.16f, 0f, 1f);
		return peak * peak;
	}

	private static float Smooth01(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}
}
