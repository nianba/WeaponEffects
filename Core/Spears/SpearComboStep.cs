namespace WeaponEffects.Spears;

public readonly struct SpearComboStep
{
	public readonly int SegmentIndex;
	public readonly SpearComboStepKind Kind;
	public readonly string Name;
	public readonly float ActiveStart;
	public readonly float ActiveEnd;
	public readonly float ReachScale;
	public readonly float CollisionWidth;
	public readonly float DamageMultiplier;
	public readonly float TimeMultiplier;
	public readonly float AirborneTimeMultiplier;
	public readonly int ExtraUpdates;

	public SpearComboStep(
		int segmentIndex,
		SpearComboStepKind kind,
		string name,
		float activeStart,
		float activeEnd,
		float reachScale,
		float collisionWidth,
		float damageMultiplier,
		float timeMultiplier,
		int extraUpdates,
		float airborneTimeMultiplier = 0f)
	{
		SegmentIndex = segmentIndex;
		Kind = kind;
		Name = name;
		ActiveStart = activeStart;
		ActiveEnd = activeEnd;
		ReachScale = reachScale;
		CollisionWidth = collisionWidth;
		DamageMultiplier = damageMultiplier;
		TimeMultiplier = timeMultiplier;
		AirborneTimeMultiplier = airborneTimeMultiplier;
		ExtraUpdates = extraUpdates;
	}

	public float GetTimeMultiplier(SpearComboBranch branch)
	{
		float multiplier = branch == SpearComboBranch.AirborneFinisher && AirborneTimeMultiplier > 0f
			? AirborneTimeMultiplier
			: TimeMultiplier;

		return System.Math.Clamp(multiplier, 0.2f, 3.0f);
	}
}
