namespace WeaponEffects.Spears;

public readonly struct SpearSweepAfterimageProfile
{
	public readonly bool Enabled;
	public readonly int VisualSampleCount;
	public readonly float VisualProgressWindow;
	public readonly float VisualWidth;
	public readonly float VisualAlpha;
	public readonly float VisualFadeInEnd;
	public readonly float VisualFadeOutStart;
	public readonly float VisualInnerShaftAmount;
	public readonly float VisualReachScale;
	public readonly int CollisionSampleCount;
	public readonly float CollisionSampleSpacing;
	public readonly float CollisionWidth;
	public readonly float CollisionReachScale;

	public SpearSweepAfterimageProfile(
		bool enabled,
		int visualSampleCount,
		float visualProgressWindow,
		float visualWidth,
		float visualAlpha,
		float visualFadeInEnd,
		float visualFadeOutStart,
		float visualInnerShaftAmount,
		float visualReachScale,
		int collisionSampleCount,
		float collisionSampleSpacing,
		float collisionWidth,
		float collisionReachScale)
	{
		Enabled = enabled;
		VisualSampleCount = visualSampleCount;
		VisualProgressWindow = visualProgressWindow;
		VisualWidth = visualWidth;
		VisualAlpha = visualAlpha;
		VisualFadeInEnd = visualFadeInEnd;
		VisualFadeOutStart = visualFadeOutStart;
		VisualInnerShaftAmount = visualInnerShaftAmount;
		VisualReachScale = visualReachScale;
		CollisionSampleCount = collisionSampleCount;
		CollisionSampleSpacing = collisionSampleSpacing;
		CollisionWidth = collisionWidth;
		CollisionReachScale = collisionReachScale;
	}

	public static SpearSweepAfterimageProfile Disabled(float collisionWidth)
	{
		return new SpearSweepAfterimageProfile(
			enabled: false,
			visualSampleCount: 0,
			visualProgressWindow: 0f,
			visualWidth: 0f,
			visualAlpha: 0f,
			visualFadeInEnd: 1f,
			visualFadeOutStart: 1f,
			visualInnerShaftAmount: 1f,
			visualReachScale: 1f,
			collisionSampleCount: 8,
			collisionSampleSpacing: 0.035f,
			collisionWidth: collisionWidth,
			collisionReachScale: 1f);
	}
}
