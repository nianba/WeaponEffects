using System;

namespace WeaponEffects.Spears;

public readonly struct SpearTipGlowProfile
{
	public readonly bool Enabled;
	public readonly float StartProgress;
	public readonly float CenterOffset;
	public readonly float BaseSize;
	public readonly float GrowthSize;
	public readonly float WidthScale;
	public readonly float LengthScale;
	public readonly float UniformScale;
	public readonly float BackExtentScale;
	public readonly float CollisionPadding;

	public SpearTipGlowProfile(
		bool enabled,
		float startProgress,
		float centerOffset,
		float baseSize,
		float growthSize,
		float widthScale,
		float lengthScale,
		float uniformScale,
		float backExtentScale,
		float collisionPadding)
	{
		Enabled = enabled;
		StartProgress = startProgress;
		CenterOffset = centerOffset;
		BaseSize = baseSize;
		GrowthSize = growthSize;
		WidthScale = widthScale;
		LengthScale = lengthScale;
		UniformScale = uniformScale;
		BackExtentScale = backExtentScale;
		CollisionPadding = collisionPadding;
	}

	public static SpearTipGlowProfile Disabled => default;

	public float SizeAt(float progress)
	{
		return BaseSize + GrowthSize * Math.Clamp(progress, 0f, 1f);
	}

	public float ForwardExtentAt(float progress)
	{
		return SizeAt(progress) * LengthScale * UniformScale;
	}

	public float FrontDistanceAt(float progress)
	{
		return Math.Max(0f, CenterOffset) + ForwardExtentAt(progress);
	}

	public float CollisionExtensionAt(float progress)
	{
		if (!Enabled)
		{
			return 0f;
		}

		return FrontDistanceAt(progress) + CollisionPadding;
	}
}
