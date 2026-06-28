namespace WeaponEffects.Spears;

public readonly struct SpearActionStep
{
	public readonly int SegmentIndex;
	public readonly SpearComboStepKind Kind;
	public readonly string Name;
	public readonly SpearActionTiming Timing;
	public readonly SpearGameplayProfile Gameplay;
	public readonly SpearMotionProfile Motion;
	public readonly SpearSweepAfterimageProfile Afterimage;
	public readonly SpearTipGlowProfile TipGlow;

	public SpearActionStep(
		int segmentIndex,
		SpearComboStepKind kind,
		string name,
		SpearActionTiming timing,
		SpearGameplayProfile gameplay,
		SpearMotionProfile motion,
		SpearSweepAfterimageProfile afterimage,
		SpearTipGlowProfile tipGlow)
	{
		SegmentIndex = segmentIndex;
		Kind = kind;
		Name = name;
		Timing = timing;
		Gameplay = gameplay;
		Motion = motion;
		Afterimage = afterimage;
		TipGlow = tipGlow;
	}
}
