using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SpearThrowChargeProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Item_" + ItemID.Trident;

	public void Initialize(int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		Projectile.netUpdate = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = 2;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
	}
}
