namespace WeaponEffects.Spears;

public static class SpearActionScheme
{
	public const int FirstEnabledStepIndex = 1;
	public const int LifetimeTicks = 26;

	public static readonly SpearActionStep[] Steps =
	{
		new(
			segmentIndex: 0,
			kind: SpearComboStepKind.ForwardThrust,
			name: "Forward Thrust",
			timing: new SpearActionTiming(
				activeStart: 0.2f,
				activeEnd: 0.55f,
				windupEnd: 0f,
				attackEnd: 1f,
				swingSoundProgress: 0.2f),
			gameplay: new SpearGameplayProfile(
				reachScale: 1.15f,
				collisionWidth: 22f,
				damageMultiplier: 0.85f,
				timeMultiplier: 0.65f,
				extraUpdates: 4),
			motion: default,
			afterimage: SpearSweepAfterimageProfile.Disabled(collisionWidth: 30f),
			tipGlow: new SpearTipGlowProfile(
				enabled: true,
				startProgress: 0f,
				centerOffset: 5f,
				baseSize: 10f,
				growthSize: 30f,
				widthScale: 0.4f,
				lengthScale: 1f,
				uniformScale: 1f,
				backExtentScale: 0.35f,
				collisionPadding: 6f)),

		new(
			segmentIndex: 1,
			kind: SpearComboStepKind.RisingLift,
			name: "Rising Lift",
			timing: new SpearActionTiming(
				activeStart: 0.42f,
				activeEnd: 0.76f,
				windupEnd: 0.48f,
				attackEnd: 0.76f,
				swingSoundProgress: 0.5f),
			gameplay: new SpearGameplayProfile(
				reachScale: 1.05f,
				collisionWidth: 24f,
				damageMultiplier: 2.3f,
				timeMultiplier: 1.8f,
				extraUpdates: 4),
			motion: SpearMotionProfile.Arc(
				startAngleDegrees: 60f,
				releaseAngleDegrees: 30f,
				endAngleDegrees: -210f,
				recoveryAngleDegrees: -210f,
				startRadiusScale: 0.86f,
				releaseRadiusScale: 0.9f,
				endRadiusScale: 0.9f,
				recoveryRadiusScale: 0.86f),
			afterimage: new SpearSweepAfterimageProfile(
				enabled: true,
				visualSampleCount: 18,
				visualProgressWindow: 0.34f,
				visualWidth: 22f,
				visualAlpha: 0.014f,
				visualFadeInEnd: 0.42f,
				visualFadeOutStart: 0.34f,
				visualInnerShaftAmount: 0.54f,
				visualReachScale: 1.6f,
				collisionSampleCount: 8,
				collisionSampleSpacing: 0.055f,
				collisionWidth: 32f,
				hitboxScale: 1.1f),
			tipGlow: SpearTipGlowProfile.Disabled),

		new(
			segmentIndex: 2,
			kind: SpearComboStepKind.Backsweep,
			name: "Around-Body Backsweep",
			timing: new SpearActionTiming(
				activeStart: 0.38f,
				activeEnd: 0.78f,
				windupEnd: 0.48f,
				attackEnd: 0.78f,
				swingSoundProgress: 0.46f),
			gameplay: new SpearGameplayProfile(
				reachScale: 1f,
				collisionWidth: 28f,
				damageMultiplier: 2.3f,
				timeMultiplier: 1.8f,
				extraUpdates: 4),
			motion: SpearMotionProfile.Arc(
				startAngleDegrees: 150f,
				releaseAngleDegrees: 180f,
				endAngleDegrees: 510f,
				recoveryAngleDegrees: 510f,
				startRadiusScale: 0.9f,
				releaseRadiusScale: 0.86f,
				endRadiusScale: 0.9f,
				recoveryRadiusScale: 0.84f),
			afterimage: new SpearSweepAfterimageProfile(
				enabled: true,
				visualSampleCount: 30,
				visualProgressWindow: 0.46f,
				visualWidth: 30f,
				visualAlpha: 0.014f,
				visualFadeInEnd: 0.38f,
				visualFadeOutStart: 0.36f,
				visualInnerShaftAmount: 0.16f,
				visualReachScale: 1.6f,
				collisionSampleCount: 8,
				collisionSampleSpacing: 0.06f,
				collisionWidth: 36f,
				hitboxScale: 1.1f),
			tipGlow: SpearTipGlowProfile.Disabled),

		new(
			segmentIndex: 3,
			kind: SpearComboStepKind.Finisher,
			name: "Branching Finisher",
			timing: new SpearActionTiming(
				activeStart: 0.37f,
				activeEnd: 0.86f,
				windupEnd: 0.48f,
				attackEnd: 0.7f,
				swingSoundProgress: 0.45f),
			gameplay: new SpearGameplayProfile(
				reachScale: 1.35f,
				collisionWidth: 32f,
				damageMultiplier: 3f,
				timeMultiplier: 2.3f,
				extraUpdates: 4,
				airborneTimeMultiplier: 1.2f),
			motion: SpearMotionProfile.Thrust(
				startAngleDegrees: 150f,
				releaseAngleDegrees: 90f,
				startRadiusScale: 0.9f,
				releaseRadiusScale: 0.82f,
				endXScale: 1.25f,
				endY: 8f),
			afterimage: SpearSweepAfterimageProfile.Disabled(collisionWidth: 42f),
			tipGlow: new SpearTipGlowProfile(
				enabled: true,
				startProgress: 0.62f,
				centerOffset: 50f,
				baseSize: 10f,
				growthSize: 10f,
				widthScale: 3.0f,
				lengthScale: 1.45f,
				uniformScale: 1.1f,
				backExtentScale: 0.35f,
				collisionPadding: 50f))
	};

	public static int Count => Steps.Length;

	public static ref readonly SpearActionStep GetStep(int index)
	{
		return ref Steps[index % Steps.Length];
	}
}
