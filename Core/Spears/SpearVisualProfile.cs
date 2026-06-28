using Microsoft.Xna.Framework;

namespace WeaponEffects.Spears;

public readonly struct SpearVisualProfile
{
	public SpearVisualProfile(string sweepColorTexturePath, Color tipGlowColor)
	{
		SweepColorTexturePath = sweepColorTexturePath;
		TipGlowColor = tipGlowColor;
	}

	public string SweepColorTexturePath { get; }

	public Color TipGlowColor { get; }
}
