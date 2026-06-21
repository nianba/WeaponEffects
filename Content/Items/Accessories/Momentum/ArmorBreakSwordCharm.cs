using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ArmorBreakSwordCharm : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetCritChance(DamageClass.Melee) += 8f;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Bone, 12)
			.AddIngredient(ItemID.DemoniteBar, 8)
			.AddTile(TileID.Anvils)
			.Register();

		CreateRecipe()
			.AddIngredient(ItemID.Bone, 12)
			.AddIngredient(ItemID.CrimtaneBar, 8)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
