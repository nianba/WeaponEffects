using System;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace WeaponEffects.Spears;

public static class SpearHeldVisualMetrics
{
	public static XnaVector2 GripOrigin(Texture2D weaponTexture, in SpearHeldVisualProfile profile)
	{
		return new XnaVector2(weaponTexture.Width * profile.GripOriginFactor.X, weaponTexture.Height * profile.GripOriginFactor.Y);
	}

	public static XnaVector2 TipOrigin(Texture2D weaponTexture, in SpearHeldVisualProfile profile)
	{
		return new XnaVector2(weaponTexture.Width * profile.TipOriginFactor.X, weaponTexture.Height * profile.TipOriginFactor.Y);
	}

	public static float TextureGripToTipLength(Texture2D weaponTexture, in SpearHeldVisualProfile profile)
	{
		return Math.Max(1f, (TipOrigin(weaponTexture, in profile) - GripOrigin(weaponTexture, in profile)).Length());
	}

	public static float DrawScale(float poseLength, float textureGripToTipLength, in SpearHeldVisualProfile profile)
	{
		float safeTextureLength = Math.Max(1f, textureGripToTipLength);
		float poseScale = Math.Clamp(poseLength / safeTextureLength, profile.MinDrawScale, profile.MaxDrawScale);
		return poseScale * profile.DrawScaleMultiplier;
	}

	public static float VisibleTipDistance(float poseLength, float textureGripToTipLength, in SpearHeldVisualProfile profile)
	{
		float safeTextureLength = Math.Max(1f, textureGripToTipLength);
		return safeTextureLength * DrawScale(poseLength, safeTextureLength, in profile) * profile.VisibleTipDistanceMultiplier;
	}

	public static XnaVector2 VisibleTip(XnaVector2 grip, XnaVector2 tip, Texture2D weaponTexture, in SpearHeldVisualProfile profile)
	{
		XnaVector2 shaft = tip - grip;
		float shaftLength = shaft.Length();
		if (shaftLength <= 1f)
		{
			return tip;
		}

		float visibleTipDistance = VisibleTipDistance(shaftLength, TextureGripToTipLength(weaponTexture, in profile), in profile);
		return grip + shaft / shaftLength * visibleTipDistance;
	}
}
