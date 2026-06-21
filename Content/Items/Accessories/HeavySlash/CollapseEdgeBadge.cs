using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class CollapseEdgeBadge : BladeAccessoryItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.LightRed;
		Item.value = Item.sellPrice(gold: 6);
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterHeavySlash(1.35f, 2.20f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ModContent.ItemType<MountainBreakerPendant>())
			.AddIngredient(ModContent.ItemType<ExtendedEdgePendant>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
