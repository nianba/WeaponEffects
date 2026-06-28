using System;

namespace WeaponEffects.Spears;

public readonly struct SpearTipGlowProfile
{
	public readonly bool Enabled;
	public readonly float StartProgress;
	public readonly float BaseLengthRatio;
	public readonly float GrowthLengthRatio;
	public readonly float VisualScale;
	public readonly float WidthScale;
	public readonly float BackOverlap;
	public readonly float HitboxScale;

	public SpearTipGlowProfile(
		bool enabled,
		float startProgress,
		float baseLengthRatio,
		float growthLengthRatio,
		float visualScale,
		float widthScale,
		float backOverlap,
		float hitboxScale)
	{
		Enabled = enabled;
		StartProgress = startProgress;
		BaseLengthRatio = baseLengthRatio;
		GrowthLengthRatio = growthLengthRatio;
		VisualScale = visualScale;
		WidthScale = widthScale;
		BackOverlap = backOverlap;
		HitboxScale = hitboxScale;
	}

	public static SpearTipGlowProfile Disabled => default;

	public float LocalProgress(float progress)
	{
		float duration = Math.Max(0.001f, 1f - StartProgress);
		return Math.Clamp((progress - StartProgress) / duration, 0f, 1f);
	}

	public float VisualLengthAt(float progress, float spearLength)
	{
		float ratio = Math.Max(0f, BaseLengthRatio + GrowthLengthRatio * LocalProgress(progress));
		return Math.Max(0f, spearLength) * ratio * Math.Max(0f, VisualScale);
	}

	public float HitboxExtensionAt(float progress, float spearLength)
	{
		if (!Enabled)
		{
			return 0f;
		}

		return VisualLengthAt(progress, spearLength) * Math.Max(0f, HitboxScale);
	}
}
