using System;

namespace WeaponEffects.Spears;

public static class SpearCollisionEnvelope
{
	public const int LifetimeTicks = 26;

	public static float CollisionWidth(in SpearComboStep step)
	{
		return SpearSweepAfterimageProfile.ForStep(in step).CollisionWidth;
	}

	public static float CollisionReachScale(in SpearComboStep step)
	{
		return SpearSweepAfterimageProfile.ForStep(in step).CollisionReachScale;
	}

	public static float TrailSampleSpacing(in SpearComboStep step)
	{
		return SpearSweepAfterimageProfile.ForStep(in step).CollisionSampleSpacing;
	}

	public static int CollisionSampleCount(in SpearComboStep step)
	{
		return SpearSweepAfterimageProfile.ForStep(in step).CollisionSampleCount;
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
