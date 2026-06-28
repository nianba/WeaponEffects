using System;

namespace WeaponEffects.Spears;

public static class SpearCollisionEnvelope
{
	public static float CollisionWidth(in SpearActionStep step)
	{
		return step.Afterimage.CollisionWidth;
	}

	public static float HitboxScale(in SpearActionStep step)
	{
		return step.Afterimage.HitboxScale;
	}

	public static float TrailSampleSpacing(in SpearActionStep step)
	{
		return step.Afterimage.CollisionSampleSpacing;
	}

	public static int CollisionSampleCount(in SpearActionStep step)
	{
		return step.Afterimage.CollisionSampleCount;
	}

	public static bool CanSampleCollisionAt(in SpearActionStep step, float progress)
	{
		return progress >= step.Timing.ActiveStart && progress <= step.Timing.ActiveEnd;
	}

	public static bool DrawsTipGlow(in SpearActionStep step)
	{
		return step.TipGlow.Enabled;
	}

	public static float CollisionTipExtensionDistance(
		in SpearActionStep step,
		float progress)
	{
		return step.TipGlow.CollisionExtensionAt(progress);
	}
}
