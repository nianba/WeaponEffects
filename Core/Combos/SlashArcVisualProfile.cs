using Microsoft.Xna.Framework;

namespace MeleeWeaponEffects;

public readonly struct SlashArcVisualProfile
{
	public readonly float XScale;
	public readonly float YScale;
	public readonly float StartDepth;
	public readonly float HitDepth;
	public readonly float EndDepth;
	public readonly float MainAlpha;
	public readonly float CoreAlpha;
	public readonly float GlowAlpha;
	public readonly int TrailCount;
	public readonly float TrailDelayDegrees;
	public readonly Color Tint;
	public readonly float FarRimAlpha;
	public readonly float NearEdgeAlpha;
	public readonly float PeakFlareAlpha;
	public readonly float NearEdgeOffsetPixels;
	public readonly float FarRimOffsetPixels;

	public SlashArcVisualProfile(
		float xScale,
		float yScale,
		float startDepth,
		float hitDepth,
		float endDepth,
		float mainAlpha,
		float coreAlpha,
		float glowAlpha,
		int trailCount,
		float trailDelayDegrees,
		Color tint,
		float farRimAlpha = 0f,
		float nearEdgeAlpha = 0f,
		float peakFlareAlpha = 0f,
		float nearEdgeOffsetPixels = 0f,
		float farRimOffsetPixels = 0f)
	{
		XScale = xScale;
		YScale = yScale;
		StartDepth = startDepth;
		HitDepth = hitDepth;
		EndDepth = endDepth;
		MainAlpha = mainAlpha;
		CoreAlpha = coreAlpha;
		GlowAlpha = glowAlpha;
		TrailCount = trailCount;
		TrailDelayDegrees = trailDelayDegrees;
		Tint = tint;
		FarRimAlpha = farRimAlpha;
		NearEdgeAlpha = nearEdgeAlpha;
		PeakFlareAlpha = peakFlareAlpha;
		NearEdgeOffsetPixels = nearEdgeOffsetPixels;
		FarRimOffsetPixels = farRimOffsetPixels;
	}
}
