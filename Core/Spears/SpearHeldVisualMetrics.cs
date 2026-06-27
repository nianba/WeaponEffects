using System;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace WeaponEffects.Spears;

public static class SpearHeldVisualMetrics
{
	private const float GripOriginXFactor = 0.15f;
	private const float GripOriginYFactor = 0.9f;
	private const float TipOriginXFactor = 1f;
	private const float TipOriginYFactor = 0f;
	private const float MinDrawScale = 0.65f;
	private const float MaxDrawScale = 1.18f;
	private const float DrawScaleMultiplier = 1.12f;

	public static XnaVector2 GripOrigin(Texture2D weaponTexture)
	{
		return new XnaVector2(weaponTexture.Width * GripOriginXFactor, weaponTexture.Height * GripOriginYFactor);
	}

	public static XnaVector2 TipOrigin(Texture2D weaponTexture)
	{
		return new XnaVector2(weaponTexture.Width * TipOriginXFactor, weaponTexture.Height * TipOriginYFactor);
	}

	public static float TextureGripToTipLength(Texture2D weaponTexture)
	{
		return Math.Max(1f, (TipOrigin(weaponTexture) - GripOrigin(weaponTexture)).Length());
	}

	public static float DrawScale(float poseLength, float textureGripToTipLength)
	{
		float safeTextureLength = Math.Max(1f, textureGripToTipLength);
		float poseScale = Math.Clamp(poseLength / safeTextureLength, MinDrawScale, MaxDrawScale);
		return poseScale * DrawScaleMultiplier;
	}

	public static float VisibleTipDistance(float poseLength, float textureGripToTipLength)
	{
		float safeTextureLength = Math.Max(1f, textureGripToTipLength);
		return safeTextureLength * DrawScale(poseLength, safeTextureLength);
	}

	public static XnaVector2 VisibleTip(XnaVector2 grip, XnaVector2 tip, Texture2D weaponTexture)
	{
		XnaVector2 shaft = tip - grip;
		float shaftLength = shaft.Length();
		if (shaftLength <= 1f)
		{
			return tip;
		}

		float visibleTipDistance = VisibleTipDistance(shaftLength, TextureGripToTipLength(weaponTexture));
		return grip + shaft / shaftLength * visibleTipDistance;
	}
}
