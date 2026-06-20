using Microsoft.Xna.Framework;

namespace WeaponEffects;

public readonly struct SlashParticleProfile
{
	public readonly int DustType;
	public readonly Color DustColor;
	public readonly Color AlternateDustColor;
	public readonly int Count;
	public readonly float MinScale;
	public readonly float MaxScale;
	public readonly float VelocityScale;
	public readonly float SpreadRadians;
	public readonly bool NoGravity;
	public readonly int AlternateDustType;
	public readonly SlashParticleVisualStyle VisualStyle;

	public SlashParticleProfile(
		int dustType,
		Color dustColor,
		int count,
		float minScale,
		float maxScale,
		float velocityScale,
		float spreadRadians,
		Color alternateDustColor = default,
		bool noGravity = false,
		int alternateDustType = -1,
		SlashParticleVisualStyle visualStyle = SlashParticleVisualStyle.Dust)
	{
		DustType = dustType;
		DustColor = dustColor;
		AlternateDustColor = alternateDustColor;
		Count = count;
		MinScale = minScale;
		MaxScale = maxScale;
		VelocityScale = velocityScale;
		SpreadRadians = spreadRadians;
		NoGravity = noGravity;
		AlternateDustType = alternateDustType;
		VisualStyle = visualStyle;
	}
}
