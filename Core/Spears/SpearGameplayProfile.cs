namespace WeaponEffects.Spears;

public readonly struct SpearGameplayProfile
{
	public readonly float ReachScale;
	public readonly float CollisionWidth;
	public readonly float DamageMultiplier;
	public readonly float TimeMultiplier;
	public readonly float AirborneTimeMultiplier;
	public readonly int ExtraUpdates;

	public SpearGameplayProfile(
		float reachScale,
		float collisionWidth,
		float damageMultiplier,
		float timeMultiplier,
		int extraUpdates,
		float airborneTimeMultiplier = 0f)
	{
		ReachScale = reachScale;
		CollisionWidth = collisionWidth;
		DamageMultiplier = damageMultiplier;
		TimeMultiplier = timeMultiplier;
		ExtraUpdates = extraUpdates;
		AirborneTimeMultiplier = airborneTimeMultiplier;
	}

	public float GetTimeMultiplier(SpearComboBranch branch)
	{
		float multiplier = branch == SpearComboBranch.AirborneFinisher && AirborneTimeMultiplier > 0f
			? AirborneTimeMultiplier
			: TimeMultiplier;

		return System.Math.Clamp(multiplier, 0.2f, 3.0f);
	}
}
