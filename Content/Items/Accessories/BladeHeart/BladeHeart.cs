using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class BladeHeart : BladeAccessoryItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Yellow;
		Item.value = Item.sellPrice(gold: 20);
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		ApplyMomentumStats(player);
		WeaponEffectsPlayer effectsPlayer = player.GetModPlayer<WeaponEffectsPlayer>();
		effectsPlayer.RegisterHeavySlash(1.35f, 2.20f);
		effectsPlayer.RegisterChargeAccessory(-90, 0.50f, 1.00f, 0.10f, 0.25f, 0.10f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ModContent.ItemType<TriBladeMomentumBadge>())
			.AddIngredient(ModContent.ItemType<CollapseEdgeBadge>())
			.AddIngredient(ModContent.ItemType<FocusedMomentumEmblem>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
