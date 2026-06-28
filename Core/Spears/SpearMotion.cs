using System;
using System.Numerics;

namespace WeaponEffects.Spears;

public static class SpearMotion
{
	public const float MinimumSpearReach = 118f;

	public static SpearComboBranch SelectFinisherBranch(bool isGrounded)
	{
		return SpearComboBranch.GroundedFinisher;
	}

	public static SpearPoseSnapshot EvaluatePose(
		in SpearActionStep step,
		SpearComboBranch branch,
		Vector2 ownerCenter,
		float aimRotation,
		float weaponLength,
		float progress)
	{
		float clampedProgress = Math.Clamp(progress, 0f, 1f);
		float reach = ResolveReach(weaponLength, step.Gameplay.ReachScale);
		Vector2 localTip = LocalTipFor(in step, branch, reach, clampedProgress);
		float facing = MathF.Cos(aimRotation) < 0f ? -1f : 1f;
		Vector2 grip = ownerCenter + new Vector2(12f * facing, 4f);
		Vector2 tip = grip + TransformLocalTip(localTip, aimRotation, facing);
		bool active = clampedProgress >= step.Timing.ActiveStart && clampedProgress <= step.Timing.ActiveEnd;

		return new SpearPoseSnapshot(grip, tip, step.Gameplay.CollisionWidth, active);
	}

	public static float ResolveReach(float weaponLength, float reachScale)
	{
		return Math.Max(MinimumSpearReach, Math.Max(1f, weaponLength)) * reachScale;
	}

	private static Vector2 LocalTipFor(in SpearActionStep step, SpearComboBranch branch, float reach, float progress)
	{
		if (step.Kind == SpearComboStepKind.Finisher && branch == SpearComboBranch.AirborneFinisher)
		{
			return AirborneFinisherTip(reach, progress);
		}

		return step.Kind switch
		{
			SpearComboStepKind.ForwardThrust => Lerp(new Vector2(reach * 0.45f, 5f), new Vector2(reach * 0.82f, 0f), Smooth01(progress)),
			SpearComboStepKind.RisingLift => ArcTip(in step, reach, progress),
			SpearComboStepKind.Backsweep => ArcTip(in step, reach, progress),
			SpearComboStepKind.Finisher => GroundedFinisherTip(in step, reach, progress),
			_ => new Vector2(reach, 0f)
		};
	}

	private static Vector2 ArcTip(in SpearActionStep step, float reach, float progress)
	{
		SpearActionTiming timing = step.Timing;
		SpearMotionProfile motion = step.Motion;
		float endAngle = DegreesToRadians(motion.EndAngleDegrees);
		float endRadius = reach * motion.EndRadiusScale;
		Vector2 endTip = ArcTip(endAngle, endRadius, 0f);
		if (progress <= timing.WindupEnd)
		{
			float windupProgress = Smooth01(progress / timing.WindupEnd);
			float angle = LerpRadians(DegreesToRadians(motion.StartAngleDegrees), DegreesToRadians(motion.ReleaseAngleDegrees), windupProgress);
			float radius = reach * LerpFloat(motion.StartRadiusScale, motion.ReleaseRadiusScale, windupProgress);
			return ArcTip(angle, radius, 0f);
		}

		if (progress <= timing.AttackEnd)
		{
			float attackProgress = Smooth01((progress - timing.WindupEnd) / (timing.AttackEnd - timing.WindupEnd));
			float attackAngle = LerpRadians(DegreesToRadians(motion.ReleaseAngleDegrees), endAngle, attackProgress);
			float attackRadius = reach * LerpFloat(motion.ReleaseRadiusScale, motion.EndRadiusScale, attackProgress);
			return ArcTip(attackAngle, attackRadius, 0f);
		}

		float recoveryProgress = Smooth01((progress - timing.AttackEnd) / (1f - timing.AttackEnd));
		Vector2 recoveryEnd = ArcTip(DegreesToRadians(motion.RecoveryAngleDegrees), reach * motion.RecoveryRadiusScale, 0f);
		return Lerp(endTip, recoveryEnd, recoveryProgress);
	}

	private static Vector2 GroundedFinisherTip(in SpearActionStep step, float reach, float progress)
	{
		SpearActionTiming timing = step.Timing;
		SpearMotionProfile motion = step.Motion;
		Vector2 held = ArcTip(DegreesToRadians(motion.StartAngleDegrees), reach * motion.ReleaseRadiusScale, 0f);
		Vector2 end = new(reach * motion.EndXScale, motion.EndY);
		if (progress <= timing.WindupEnd)
		{
			float windupProgress = Smooth01(progress / timing.WindupEnd);
			float angle = LerpRadians(DegreesToRadians(motion.StartAngleDegrees), DegreesToRadians(motion.ReleaseAngleDegrees), windupProgress);
			float radius = reach * LerpFloat(motion.StartRadiusScale, motion.ReleaseRadiusScale, windupProgress);
			return ArcTip(angle, radius, 0f);
		}

		if (progress <= timing.AttackEnd)
		{
			float thrustProgress = Smooth01((progress - timing.WindupEnd) / (timing.AttackEnd - timing.WindupEnd));
			return Lerp(held, end, thrustProgress);
		}

		return end;
	}

	private static Vector2 AirborneFinisherTip(float reach, float progress)
	{
		Vector2 start = new(-reach * 0.72f, reach * 0.48f);
		Vector2 overheadControl = new(-reach * 0.22f, -reach * 1.28f);
		Vector2 overheadExit = new(reach * 0.58f, -reach * 0.64f);
		Vector2 slamEnd = new(reach * 0.86f, reach * 0.52f);

		if (progress <= 0.68f)
		{
			float arcProgress = Smooth01(progress / 0.68f);
			return QuadraticBezier(start, overheadControl, overheadExit, arcProgress);
		}

		float slamProgress = Smooth01((progress - 0.68f) / 0.32f);
		return Lerp(overheadExit, slamEnd, slamProgress);
	}

	private static Vector2 TransformLocalTip(Vector2 value, float aimRotation, float facing)
	{
		Vector2 forward = new(MathF.Cos(aimRotation), MathF.Sin(aimRotation));
		Vector2 screenDownPerpendicular = new Vector2(-forward.Y, forward.X) * facing;
		return forward * value.X + screenDownPerpendicular * value.Y;
	}

	private static Vector2 Lerp(Vector2 start, Vector2 end, float amount)
	{
		return start + (end - start) * amount;
	}

	private static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float amount)
	{
		Vector2 first = Lerp(start, control, amount);
		Vector2 second = Lerp(control, end, amount);
		return Lerp(first, second, amount);
	}

	private static float LerpRadians(float start, float end, float amount)
	{
		return start + (end - start) * amount;
	}

	private static float LerpFloat(float start, float end, float amount)
	{
		return start + (end - start) * amount;
	}

	private static float DegreesToRadians(float degrees)
	{
		return degrees * MathF.PI / 180f;
	}

	private static Vector2 ArcTip(float angle, float radius, float yOffset)
	{
		Vector2 arc = new(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
		arc.Y += yOffset;
		return arc;
	}

	private static float Smooth01(float value)
	{
		value = Math.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}
}
