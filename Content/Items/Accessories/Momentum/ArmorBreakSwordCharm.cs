using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ArmorBreakSwordCharm : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterBladeMomentum(
			BladeMomentumDefaultDuration,
			critPerStack: 2f,
			critMaxStacks: 4);
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
