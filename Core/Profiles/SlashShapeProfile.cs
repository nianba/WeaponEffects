namespace MeleeWeaponEffects;

public readonly struct SlashShapeProfile
{
	public readonly float LengthScale;
	public readonly float ThicknessScale;
	public readonly float MinYScale;
	public readonly float MaxYScale;
	public readonly float AngleRandomness;
	public readonly int ExtraUpdates;

	public SlashShapeProfile(
		float lengthScale,
		float thicknessScale,
		float minYScale,
		float maxYScale,
		float angleRandomness,
		int extraUpdates)
	{
		LengthScale = lengthScale;
		ThicknessScale = thicknessScale;
		MinYScale = minYScale;
		MaxYScale = maxYScale;
		AngleRandomness = angleRandomness;
		ExtraUpdates = extraUpdates;
	}
}

