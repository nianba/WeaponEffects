using Microsoft.Xna.Framework;

namespace MeleeWeaponEffects;

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
			lengthScale: 1.12f,
			thicknessScale: 1f,
			extraUpdates: 5,
			visual: new SlashArcVisualProfile(
				xScale: 1.05f,
				yScale: 0.82f,
				startDepth: -0.7f,
				hitDepth: 1f,
				endDepth: 0.2f,
				mainAlpha: 0.68f,
				coreAlpha: 0.8f,
				glowAlpha: 0.4f,
				trailCount: 1,
				trailDelayDegrees: 8f,
				tint: Color.White,
				nearEdgeAlpha: 0.28f,
				peakFlareAlpha: 0.18f,
				nearEdgeOffsetPixels: 4f)),

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
			lengthScale: 1.08f,
			thicknessScale: 1.12f,
			extraUpdates: 5,
			visual: new SlashArcVisualProfile(
				xScale: 1.05f,
				yScale: 1.12f,
				startDepth: 0.9f,
				hitDepth: 1.45f,
				endDepth: 0.1f,
				mainAlpha: 0.78f,
				coreAlpha: 1f,
				glowAlpha: 0.66f,
				trailCount: 1,
				trailDelayDegrees: 10f,
				tint: new Color(255, 245, 225),
				nearEdgeAlpha: 0.55f,
				peakFlareAlpha: 0.7f,
				nearEdgeOffsetPixels: 6f))
	};

	public static int Count => Steps.Length;

	public static ref readonly SlashComboStep GetStep(int index)
	{
		return ref Steps[index % Steps.Length];
	}
}
