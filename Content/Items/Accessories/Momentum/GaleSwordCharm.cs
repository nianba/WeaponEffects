using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class GaleSwordCharm : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterBladeMomentum(
			BladeMomentumGaleDuration,
			attackSpeedPerStack: 0.04f,
			attackSpeedMaxStacks: 8);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Feather, 5)
			.AddIngredient(ItemID.SilverBar, 8)
			.AddTile(TileID.Anvils)
			.Register();

		CreateRecipe()
			.AddIngredient(ItemID.Feather, 5)
			.AddIngredient(ItemID.TungstenBar, 8)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
