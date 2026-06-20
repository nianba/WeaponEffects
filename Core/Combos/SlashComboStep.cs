namespace WeaponEffects;

public readonly struct SlashComboStep
{
	public readonly int SegmentIndex;
	public readonly string Name;
	public readonly float StartAngleDegrees;
	public readonly float HitAngleDegrees;
	public readonly float EndAngleDegrees;
	public readonly float ActiveStart;
	public readonly float ActiveEnd;
	public readonly float LengthScale;
	public readonly float ThicknessScale;
	public readonly int ExtraUpdates;
	public readonly SlashArcVisualProfile Visual;

	public SlashComboStep(
		int segmentIndex,
		string name,
		float startAngleDegrees,
		float hitAngleDegrees,
		float endAngleDegrees,
		float activeStart,
		float activeEnd,
		float lengthScale,
		float thicknessScale,
		int extraUpdates,
		SlashArcVisualProfile visual)
	{
		SegmentIndex = segmentIndex;
		Name = name;
		StartAngleDegrees = startAngleDegrees;
		HitAngleDegrees = hitAngleDegrees;
		EndAngleDegrees = endAngleDegrees;
		ActiveStart = activeStart;
		ActiveEnd = activeEnd;
		LengthScale = lengthScale;
		ThicknessScale = thicknessScale;
		ExtraUpdates = extraUpdates;
		Visual = visual;
	}
}
