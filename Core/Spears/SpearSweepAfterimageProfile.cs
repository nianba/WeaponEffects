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

	private SpearSweepAfterimageProfile(
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

	public static SpearSweepAfterimageProfile ForStep(in SpearComboStep step)
	{
		return step.Kind switch
		{
			SpearComboStepKind.RisingLift => new SpearSweepAfterimageProfile(
				enabled: true,
				visualSampleCount: 18,
				visualProgressWindow: 0.58f,
				visualWidth: 22f,
				visualAlpha: 0.014f,
				visualFadeInEnd: 0.24f,
				visualFadeOutStart: 0.46f,
				visualInnerShaftAmount: 0.54f,
				visualReachScale: 1.6f,
				collisionSampleCount: 8,
				collisionSampleSpacing: 0.09f,
				collisionWidth: 32f,
				collisionReachScale: 2.0f),

			SpearComboStepKind.Backsweep => new SpearSweepAfterimageProfile(
				enabled: true,
				visualSampleCount: 30,
				visualProgressWindow: 1.12f,
				visualWidth: 30f,
				visualAlpha: 0.014f,
				visualFadeInEnd: 0.18f,
				visualFadeOutStart: 0.56f,
				visualInnerShaftAmount: 0.16f,
				visualReachScale: 1.6f,
				collisionSampleCount: 8,
				collisionSampleSpacing: 0.09f,
				collisionWidth: 36f,
				collisionReachScale: 2.0f),

			_ => new SpearSweepAfterimageProfile(
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
				collisionWidth: step.Kind switch
				{
					SpearComboStepKind.ForwardThrust => 30f,
					SpearComboStepKind.Finisher => 42f,
					_ => step.CollisionWidth
				},
				collisionReachScale: 1f)
		};
	}
}
