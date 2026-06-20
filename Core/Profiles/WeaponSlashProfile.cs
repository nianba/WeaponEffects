using Microsoft.Xna.Framework;

namespace MeleeWeaponEffects;

public readonly struct WeaponSlashProfile
{
	public readonly SlashProfileId Id;
	public readonly Color SlashColor;
	public readonly SlashParticleProfile SwingParticles;
	public readonly SlashParticleProfile HitParticles;
	public readonly SlashShapeProfile Shape;

	public WeaponSlashProfile(
		SlashProfileId id,
		Color slashColor,
		SlashParticleProfile swingParticles,
		SlashParticleProfile hitParticles,
		SlashShapeProfile shape)
	{
		Id = id;
		SlashColor = slashColor;
		SwingParticles = swingParticles;
		HitParticles = hitParticles;
		Shape = shape;
	}
}

