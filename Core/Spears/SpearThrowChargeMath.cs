using System;

namespace WeaponEffects.Spears;

public static class SpearThrowChargeMath
{
	public const int MinimumChargeFrames = 60;
	public const int BaseFullChargeFrames = 300;
	public const int MinimumFullChargeFrames = 120;
	public const float MinimumDamageMultiplier = 1f;
	public const float MaximumDamageMultiplier = 10f;
	public const float MinimumScreenRange = 1.5f;
	public const float MaximumScreenRange = 5f;
	public const float ThrowSpeed = 42f;
	public const float MinimumVisualWidth = 18f;
	public const float MaximumVisualWidth = 34f;
	public const float MinimumCollisionWidth = 34f;
	public const float MaximumCollisionWidth = 58f;

	public static bool IsChargeValid(int chargeFrames)
	{
		return chargeFrames >= MinimumChargeFrames;
	}

	public static int EffectiveFullChargeFrames(float meleeAttackSpeed)
	{
		float speed = Math.Clamp(meleeAttackSpeed, 0.25f, 4f);
		int scaled = (int)MathF.Round(BaseFullChargeFrames / speed);
		return Math.Max(MinimumFullChargeFrames, scaled);
	}

	public static float ChargeProgress(int chargeFrames, int fullChargeFrames)
	{
		int clampedFull = Math.Max(MinimumChargeFrames + 1, fullChargeFrames);
		float progress = (chargeFrames - MinimumChargeFrames) / (float)(clampedFull - MinimumChargeFrames);
		return Math.Clamp(progress, 0f, 1f);
	}

	public static float DamageMultiplier(float chargeProgress)
	{
		return Lerp(MinimumDamageMultiplier, MaximumDamageMultiplier, Math.Clamp(chargeProgress, 0f, 1f));
	}

	public static float TravelDistancePixels(float chargeProgress, float screenWidth)
	{
		float width = Math.Max(1f, screenWidth);
		float screens = Lerp(MinimumScreenRange, MaximumScreenRange, Math.Clamp(chargeProgress, 0f, 1f));
		return width * screens;
	}

	public static float VisualWidth(float chargeProgress)
	{
		return Lerp(MinimumVisualWidth, MaximumVisualWidth, Math.Clamp(chargeProgress, 0f, 1f));
	}

	public static float CollisionWidth(float chargeProgress)
	{
		return Lerp(MinimumCollisionWidth, MaximumCollisionWidth, Math.Clamp(chargeProgress, 0f, 1f));
	}

	private static float Lerp(float start, float end, float amount)
	{
		return start + (end - start) * amount;
	}
}
