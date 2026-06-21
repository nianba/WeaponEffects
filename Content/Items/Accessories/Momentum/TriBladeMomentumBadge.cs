using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class TriBladeMomentumBadge : BladeAccessoryItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(gold: 6);
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		ApplyMomentumStats(player);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ModContent.ItemType<GaleSwordCharm>())
			.AddIngredient(ModContent.ItemType<ArmorBreakSwordCharm>())
			.AddIngredient(ModContent.ItemType<SharpSwordCharm>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
