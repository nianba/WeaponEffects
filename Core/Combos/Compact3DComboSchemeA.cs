using Microsoft.Xna.Framework;

namespace WeaponEffects;

public static class Compact3DComboSchemeA
{
	public static readonly SlashComboStep[] Steps =
	{
		new(
			segmentIndex: 0,
			name: "Falling Diagonal Opener",
			startAngleDegrees: -125f,
			hitAngleDegrees: -35f,
			endAngleDegrees: 35f,
			activeStart: 0.24f,
			activeEnd: 0.58f,
			lengthScale: 1.16f,
			thicknessScale: 0.82f,
			extraUpdates: 5,
			visual: new SlashArcVisualProfile(
				xScale: 1.18f,
				yScale: 0.58f,
				startDepth: -0.95f,
				hitDepth: 0.65f,
				endDepth: -0.15f,
				mainAlpha: 0.56f,
				coreAlpha: 0.62f,
				glowAlpha: 0.28f,
				trailCount: 1,
				trailDelayDegrees: 11f,
				tint: new Color(220, 235, 255),
				farRimAlpha: 0.16f,
				nearEdgeAlpha: 0.16f,
				peakFlareAlpha: 0.08f,
				nearEdgeOffsetPixels: 2f,
				farRimOffsetPixels: 4f)),

		new(
			segmentIndex: 1,
			name: "Reverse Rising Cut",
			startAngleDegrees: 40f,
			hitAngleDegrees: -70f,
			endAngleDegrees: -135f,
			activeStart: 0.22f,
			activeEnd: 0.6f,
			lengthScale: 1.02f,
			thicknessScale: 0.92f,
			extraUpdates: 5,
			visual: new SlashArcVisualProfile(
				xScale: 0.92f,
				yScale: 1.08f,
				startDepth: 0.8f,
				hitDepth: -0.9f,
				endDepth: -0.3f,
				mainAlpha: 0.6f,
				coreAlpha: 0.72f,
				glowAlpha: 0.34f,
				trailCount: 1,
				trailDelayDegrees: 7f,
				tint: new Color(225, 230, 255),
				farRimAlpha: 0.18f,
				nearEdgeAlpha: 0.2f,
				farRimOffsetPixels: 4f)),

		new(
			segmentIndex: 2,
			name: "Front-Plane Horizontal Slice",
			startAngleDegrees: -155f,
			hitAngleDegrees: 0f,
			endAngleDegrees: 28f,
			activeStart: 0.2f,
			activeEnd: 0.62f,
			lengthScale: 1.18f,
			thicknessScale: 1f,
			extraUpdates: 5,
			visual: new SlashArcVisualProfile(
				xScale: 1.3f,
				yScale: 0.68f,
				startDepth: -0.4f,
				hitDepth: 1.35f,
				endDepth: -0.2f,
				mainAlpha: 0.64f,
				coreAlpha: 0.88f,
				glowAlpha: 0.5f,
				trailCount: 2,
				trailDelayDegrees: 8f,
				tint: Color.White,
				farRimAlpha: 0.22f,
				nearEdgeAlpha: 0.48f,
				peakFlareAlpha: 0.42f,
				nearEdgeOffsetPixels: 7f,
				farRimOffsetPixels: 5f)),

		new(
			segmentIndex: 3,
			name: "Compact Downward Finisher",
			startAngleDegrees: -95f,
			hitAngleDegrees: -25f,
			endAngleDegrees: 55f,
			activeStart: 0.26f,
			activeEnd: 0.68f,
			lengthScale: 0.92f,
			thicknessScale: 1.42f,
			extraUpdates: 5,
			visual: new SlashArcVisualProfile(
				xScale: 0.82f,
				yScale: 1.38f,
				startDepth: 1.05f,
				hitDepth: 1.65f,
				endDepth: 0.35f,
				mainAlpha: 0.86f,
				coreAlpha: 1.18f,
				glowAlpha: 0.82f,
				trailCount: 1,
				trailDelayDegrees: 4f,
				tint: new Color(255, 230, 185),
				farRimAlpha: 0.1f,
				nearEdgeAlpha: 0.82f,
				peakFlareAlpha: 1.05f,
				nearEdgeOffsetPixels: 11f,
				farRimOffsetPixels: 2f))
	};

	public static int Count => Steps.Length;

	public static ref readonly SlashComboStep GetStep(int index)
	{
		return ref Steps[index % Steps.Length];
	}
}
