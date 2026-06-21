using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ExtendedEdgePendant : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterHeavySlash(1.00f, 2.20f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.MeteoriteBar, 10)
			.AddIngredient(ItemID.FallenStar, 3)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
