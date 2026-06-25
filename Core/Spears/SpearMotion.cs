using System;
using System.Numerics;

namespace WeaponEffects.Spears;

public static class SpearMotion
{
	public static SpearComboBranch SelectFinisherBranch(bool isGrounded)
	{
		return isGrounded ? SpearComboBranch.GroundedFinisher : SpearComboBranch.AirborneFinisher;
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
		float reach = Math.Max(1f, weaponLength) * step.ReachScale;
		Vector2 localTip = LocalTipFor(step.Kind, branch, reach, clampedProgress);
		float facing = MathF.Cos(aimRotation) < 0f ? -1f : 1f;
		Vector2 grip = ownerCenter + new Vector2(12f * facing, 4f);
		Vector2 tip = grip + Rotate(localTip, aimRotation);
		bool active = clampedProgress >= step.ActiveStart && clampedProgress <= step.ActiveEnd;

		return new SpearPoseSnapshot(grip, tip, step.CollisionWidth, active);
	}

	private static Vector2 LocalTipFor(SpearComboStepKind kind, SpearComboBranch branch, float reach, float progress)
	{
		return kind switch
		{
			SpearComboStepKind.ForwardThrust => Lerp(new Vector2(reach * 0.45f, 5f), new Vector2(reach, 0f), Smooth01(progress)),
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
		float eased = Smooth01(progress);
		Vector2 start = new(reach * 0.62f, 34f);
		Vector2 end = new(reach * 0.92f, -62f);
		return Lerp(start, end, eased);
	}

	private static Vector2 BacksweepTip(float reach, float progress)
	{
		float angle = LerpRadians(-0.12f, 2.82f, Smooth01(progress));
		float radius = reach * LerpFloat(0.78f, 0.86f, progress);
		Vector2 arc = new(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
		arc.Y += LerpFloat(0f, 28f, Smooth01(progress));
		return arc;
	}

	private static Vector2 GroundedFinisherTip(float reach, float progress)
	{
		float eased = Smooth01(progress);
		Vector2 start = new(-reach * 0.52f, 38f);
		Vector2 end = new(reach, 8f);
		return Lerp(start, end, eased);
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

	private static Vector2 Rotate(Vector2 value, float radians)
	{
		float cos = MathF.Cos(radians);
		float sin = MathF.Sin(radians);
		return new Vector2(value.X * cos - value.Y * sin, value.X * sin + value.Y * cos);
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

	private static float Smooth01(float value)
	{
		value = Math.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}
}
