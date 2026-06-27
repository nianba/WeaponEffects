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
		in SpearComboStep step,
		SpearComboBranch branch,
		Vector2 ownerCenter,
		float aimRotation,
		float weaponLength,
		float progress)
	{
		float clampedProgress = Math.Clamp(progress, 0f, 1f);
		float reach = ResolveReach(weaponLength, step.ReachScale);
		Vector2 localTip = LocalTipFor(step.Kind, branch, reach, clampedProgress);
		float facing = MathF.Cos(aimRotation) < 0f ? -1f : 1f;
		Vector2 grip = ownerCenter + new Vector2(12f * facing, 4f);
		Vector2 tip = grip + TransformLocalTip(localTip, aimRotation, facing);
		bool active = clampedProgress >= step.ActiveStart && clampedProgress <= step.ActiveEnd;

		return new SpearPoseSnapshot(grip, tip, step.CollisionWidth, active);
	}

	public static float ResolveReach(float weaponLength, float reachScale)
	{
		return Math.Max(MinimumSpearReach, Math.Max(1f, weaponLength)) * reachScale;
	}

	private static Vector2 LocalTipFor(SpearComboStepKind kind, SpearComboBranch branch, float reach, float progress)
	{
		return kind switch
		{
			SpearComboStepKind.ForwardThrust => Lerp(new Vector2(reach * 0.45f, 5f), new Vector2(reach * 0.82f, 0f), Smooth01(progress)),
			SpearComboStepKind.RisingLift => RisingLiftTip(reach, progress),
			SpearComboStepKind.Backsweep => BacksweepTip(reach, progress),
			SpearComboStepKind.Finisher => branch == SpearComboBranch.AirborneFinisher
				? AirborneFinisherTip(reach, progress)
				: GroundedFinisherTip(reach, progress),
			_ => new Vector2(reach, 0f)
		};
	}

	private static Vector2 RisingLiftTip(float reach, float progress)
	{
		const float windupEnd = 0.48f;
		const float liftEnd = 0.76f;
		float liftEndAngle = DegreesToRadians(-210f);
		float liftEndRadius = reach * 0.9f;
		Vector2 liftEndTip = ArcTip(liftEndAngle, liftEndRadius, 0f);
		if (progress <= windupEnd)
		{
			float windupProgress = Smooth01(progress / windupEnd);
			float angle = LerpRadians(DegreesToRadians(60f), DegreesToRadians(30f), windupProgress);
			float radius = reach * LerpFloat(0.86f, 0.9f, windupProgress);
			return ArcTip(angle, radius, 0f);
		}

		if (progress <= liftEnd)
		{
			float liftProgress = Smooth01((progress - windupEnd) / (liftEnd - windupEnd));
			float liftAngle = LerpRadians(DegreesToRadians(30f), liftEndAngle, liftProgress);
			float liftRadius = reach * LerpFloat(0.9f, 0.9f, liftProgress);
			return ArcTip(liftAngle, liftRadius, 0f);
		}

		float recoveryProgress = Smooth01((progress - liftEnd) / (1f - liftEnd));
		Vector2 recoveryEnd = ArcTip(DegreesToRadians(-210f), reach * 0.86f, 0f);
		return Lerp(liftEndTip, recoveryEnd, recoveryProgress);
	}

	private static Vector2 BacksweepTip(float reach, float progress)
	{
		const float windupEnd = 0.48f;
		const float sweepEnd = 0.78f;
		float sweepEndAngle = DegreesToRadians(510f);
		float sweepEndRadius = reach * 0.9f;
		Vector2 sweepEndTip = ArcTip(sweepEndAngle, sweepEndRadius, 0f);
		if (progress <= windupEnd)
		{
			float windupProgress = Smooth01(progress / windupEnd);
			float angle = LerpRadians(DegreesToRadians(150f), DegreesToRadians(180f), windupProgress);
			float radius = reach * LerpFloat(0.9f, 0.86f, windupProgress);
			return ArcTip(angle, radius, 0f);
		}

		if (progress <= sweepEnd)
		{
			float sweepProgress = Smooth01((progress - windupEnd) / (sweepEnd - windupEnd));
			float sweepAngle = LerpRadians(DegreesToRadians(180f), sweepEndAngle, sweepProgress);
			float sweepRadius = reach * LerpFloat(0.86f, sweepEndRadius / reach, sweepProgress);
			return ArcTip(sweepAngle, sweepRadius, 0f);
		}

		float recoveryProgress = Smooth01((progress - sweepEnd) / (1f - sweepEnd));
		Vector2 recoveryEnd = ArcTip(DegreesToRadians(510f), reach * 0.84f, 0f);
		return Lerp(sweepEndTip, recoveryEnd, recoveryProgress);
	}

	private static Vector2 GroundedFinisherTip(float reach, float progress)
	{
		Vector2 held = ArcTip(DegreesToRadians(150f), reach * 0.82f, 0f);
		Vector2 end = new(reach * 1.25f, 8f);
		const float windupEnd = 0.48f;
		const float thrustEnd = 0.7f;
		if (progress <= windupEnd)
		{
			float windupProgress = Smooth01(progress / windupEnd);
			float angle = LerpRadians(DegreesToRadians(150f), DegreesToRadians(120f), windupProgress);
			float radius = reach * LerpFloat(0.9f, 0.82f, windupProgress);
			return ArcTip(angle, radius, 0f);
		}

		if (progress <= thrustEnd)
		{
			float thrustProgress = Smooth01((progress - windupEnd) / (thrustEnd - windupEnd));
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
