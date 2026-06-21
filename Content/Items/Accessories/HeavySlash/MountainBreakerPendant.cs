using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class MountainBreakerPendant : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterHeavySlash(1.30f, 1.00f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Obsidian, 12)
			.AddIngredient(ItemID.HellstoneBar, 8)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
