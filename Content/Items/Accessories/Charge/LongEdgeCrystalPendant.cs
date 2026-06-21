using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class LongEdgeCrystalPendant : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterChargeAccessory(0, 0.50f, 1.00f, 0f, 0f, 0.10f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.CrystalShard, 12)
			.AddIngredient(ItemID.SoulofLight, 5)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
