using Microsoft.Xna.Framework;

namespace MeleeWeaponEffects;

public readonly struct SlashParticleProfile
{
	public readonly int DustType;
	public readonly Color DustColor;
	public readonly int Count;
	public readonly float MinScale;
	public readonly float MaxScale;
	public readonly float VelocityScale;
	public readonly float SpreadRadians;

	public SlashParticleProfile(
		int dustType,
		Color dustColor,
		int count,
		float minScale,
		float maxScale,
		float velocityScale,
		float spreadRadians)
	{
		DustType = dustType;
		DustColor = dustColor;
		Count = count;
		MinScale = minScale;
		MaxScale = maxScale;
		VelocityScale = velocityScale;
		SpreadRadians = spreadRadians;
	}
}

