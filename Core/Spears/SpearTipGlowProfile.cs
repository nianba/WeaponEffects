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

	private SpearTipGlowProfile(
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

	public static SpearTipGlowProfile ForStep(in SpearComboStep step)
	{
		return step.Kind switch
		{
			SpearComboStepKind.ForwardThrust => new SpearTipGlowProfile(
				enabled: true,
				startProgress: 0f,
				centerOffset: 5f,
				baseSize: 10f,
				growthSize: 30f,
				widthScale: 0.4f,
				lengthScale: 1f,
				uniformScale: 1f,
				backExtentScale: 0.35f,
				collisionPadding: 6f),

			SpearComboStepKind.Finisher => new SpearTipGlowProfile(
				enabled: true,
				startProgress: 0.62f,
				centerOffset: 50f,
				baseSize: 10f,
				growthSize: 10f,
				widthScale: 3.0f,
				lengthScale: 1.45f,
				uniformScale: 1.1f,
				backExtentScale: 0.35f,
				collisionPadding: 50f),

			_ => default
		};
	}

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
