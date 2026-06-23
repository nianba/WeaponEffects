using System.Numerics;

namespace WeaponEffects.Spears;

public readonly struct SpearPoseSnapshot
{
	public readonly Vector2 Grip;
	public readonly Vector2 Tip;
	public readonly Vector2 ShaftDirection;
	public readonly float CollisionWidth;
	public readonly bool Active;

	public SpearPoseSnapshot(Vector2 grip, Vector2 tip, float collisionWidth, bool active)
	{
		Grip = grip;
		Tip = tip;
		CollisionWidth = collisionWidth;
		Active = active;
		Vector2 shaft = tip - grip;
		ShaftDirection = shaft.LengthSquared() > 0.001f ? Vector2.Normalize(shaft) : Vector2.UnitX;
	}
}
