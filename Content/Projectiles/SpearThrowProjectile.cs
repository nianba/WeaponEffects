using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SpearThrowProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Extra_" + ExtrasID.SharpTears;

	public static void Spawn(
		IEntitySource source,
		Vector2 position,
		int owner,
		int weaponItemType,
		float aimRotation,
		float maxTravelDistance,
		float visualWidth,
		float collisionWidth,
		float chargeProgress,
		int damage,
		float knockback)
	{
		Projectile projectile = Projectile.NewProjectileDirect(
			source,
			position,
			aimRotation.ToRotationVector2() * 42f,
			ModContent.ProjectileType<SpearThrowProjectile>(),
			damage,
			knockback,
			owner);

		projectile.rotation = aimRotation;
		MeleeEffectAssets.SyncProjectile(projectile);
	}

	public override void SetDefaults()
	{
		Projectile.width = 18;
		Projectile.height = 18;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 120;
	}
}
