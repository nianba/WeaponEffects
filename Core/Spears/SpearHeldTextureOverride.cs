using Microsoft.Xna.Framework;

namespace WeaponEffects.Spears;

public readonly struct SpearHeldTextureOverride
{
	public SpearHeldTextureOverride(int projectileType, Vector2 gripOrigin, Vector2 tipOrigin)
	{
		ProjectileType = projectileType;
		GripOrigin = gripOrigin;
		TipOrigin = tipOrigin;
	}

	public int ProjectileType { get; }

	public Vector2 GripOrigin { get; }

	public Vector2 TipOrigin { get; }

	public bool HasSource => ProjectileType > 0;
}
