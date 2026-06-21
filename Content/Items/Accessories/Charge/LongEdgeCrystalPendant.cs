using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class LongEdgeCrystalPendant : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterChargeAccessory(1f, 0.40f, 0.80f, 0f, 0f, 0.10f);
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
