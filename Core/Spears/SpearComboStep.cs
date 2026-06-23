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
		int extraUpdates)
	{
		SegmentIndex = segmentIndex;
		Kind = kind;
		Name = name;
		ActiveStart = activeStart;
		ActiveEnd = activeEnd;
		ReachScale = reachScale;
		CollisionWidth = collisionWidth;
		DamageMultiplier = damageMultiplier;
		ExtraUpdates = extraUpdates;
	}
}
