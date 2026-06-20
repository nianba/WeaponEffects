using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class SlashTest : ModItem
{
	public override string Texture => "Terraria/Images/Item_" + (short)3063;

	public override void SetDefaults()
	{
		((ModItem)this).Item.CloneDefaults(757);
		((ModItem)this).Item.noUseGraphic = true;
		((ModItem)this).Item.noMelee = true;
		((ModItem)this).Item.damage = 5000;
		((ModItem)this).Item.channel = true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Projectile.NewProjectile((IEntitySource)(object)source, ((Entity)player).Center, Vector2.Zero, ModContent.ProjectileType<ExampleSlashWeapon>(), damage, 0f, ((Entity)player).whoAmI, Utils.NextBool(Main.rand, 2) ? (-1.9f) : 1.9f, 0f, 0f);
		return false;
	}
}
