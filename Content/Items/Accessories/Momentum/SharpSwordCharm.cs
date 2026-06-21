using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SharpSwordCharm : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetDamage(DamageClass.Melee) += 0.10f;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Stinger, 4)
			.AddIngredient(ItemID.JungleSpores, 6)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
