using System;

namespace WeaponEffects.Spears;

public static class SpearCollisionEnvelope
{
	public const int LifetimeTicks = 26;

	private const float ForwardThrustCollisionWidth = 30f;
	private const float RisingLiftCollisionWidth = 32f;
	private const float BacksweepCollisionWidth = 36f;
	private const float FinisherCollisionWidth = 42f;
	private const float QuickTrailSampleSpacing = 0.035f;
	private const float SweepTrailSampleSpacing = 0.09f;

	public static float CollisionWidth(in SpearComboStep step)
	{
		return step.Kind switch
		{
			SpearComboStepKind.ForwardThrust => ForwardThrustCollisionWidth,
			SpearComboStepKind.RisingLift => RisingLiftCollisionWidth,
			SpearComboStepKind.Backsweep => BacksweepCollisionWidth,
			SpearComboStepKind.Finisher => FinisherCollisionWidth,
			_ => Math.Max(step.CollisionWidth, ForwardThrustCollisionWidth)
		};
	}

	public static float TrailSampleSpacing(in SpearComboStep step)
	{
		return step.Kind is SpearComboStepKind.RisingLift or SpearComboStepKind.Backsweep
			? SweepTrailSampleSpacing
			: QuickTrailSampleSpacing;
	}

	public static bool DrawsTipGlow(in SpearComboStep step)
	{
		return SpearTipGlowProfile.ForStep(in step).Enabled;
	}

	public static float CollisionTipExtensionDistance(
		in SpearComboStep step,
		float progress)
	{
		return SpearTipGlowProfile.ForStep(in step).CollisionExtensionAt(progress);
	}
}
