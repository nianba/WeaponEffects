namespace WeaponEffects.Spears;

public readonly struct SpearMotionProfile
{
	public readonly SpearMotionStyle Style;
	public readonly float StartAngleDegrees;
	public readonly float ReleaseAngleDegrees;
	public readonly float EndAngleDegrees;
	public readonly float RecoveryAngleDegrees;
	public readonly float StartRadiusScale;
	public readonly float ReleaseRadiusScale;
	public readonly float EndRadiusScale;
	public readonly float RecoveryRadiusScale;
	public readonly float EndXScale;
	public readonly float EndY;

	private SpearMotionProfile(
		SpearMotionStyle style,
		float startAngleDegrees,
		float releaseAngleDegrees,
		float endAngleDegrees,
		float recoveryAngleDegrees,
		float startRadiusScale,
		float releaseRadiusScale,
		float endRadiusScale,
		float recoveryRadiusScale,
		float endXScale,
		float endY)
	{
		Style = style;
		StartAngleDegrees = startAngleDegrees;
		ReleaseAngleDegrees = releaseAngleDegrees;
		EndAngleDegrees = endAngleDegrees;
		RecoveryAngleDegrees = recoveryAngleDegrees;
		StartRadiusScale = startRadiusScale;
		ReleaseRadiusScale = releaseRadiusScale;
		EndRadiusScale = endRadiusScale;
		RecoveryRadiusScale = recoveryRadiusScale;
		EndXScale = endXScale;
		EndY = endY;
	}

	public static SpearMotionProfile Arc(
		float startAngleDegrees,
		float releaseAngleDegrees,
		float endAngleDegrees,
		float recoveryAngleDegrees,
		float startRadiusScale,
		float releaseRadiusScale,
		float endRadiusScale,
		float recoveryRadiusScale)
	{
		return new SpearMotionProfile(
			SpearMotionStyle.Arc,
			startAngleDegrees,
			releaseAngleDegrees,
			endAngleDegrees,
			recoveryAngleDegrees,
			startRadiusScale,
			releaseRadiusScale,
			endRadiusScale,
			recoveryRadiusScale,
			0f,
			0f);
	}

	public static SpearMotionProfile Thrust(
		float startAngleDegrees,
		float releaseAngleDegrees,
		float startRadiusScale,
		float releaseRadiusScale,
		float endXScale,
		float endY)
	{
		return new SpearMotionProfile(
			SpearMotionStyle.Thrust,
			startAngleDegrees,
			releaseAngleDegrees,
			0f,
			0f,
			startRadiusScale,
			releaseRadiusScale,
			0f,
			0f,
			endXScale,
			endY);
	}
}
