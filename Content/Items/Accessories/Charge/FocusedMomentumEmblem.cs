using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class FocusedMomentumEmblem : BladeAccessoryItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Pink;
		Item.value = Item.sellPrice(gold: 8);
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterChargeAccessory(-90, 0.50f, 1.00f, 0.10f, 0.25f, 0.10f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ModContent.ItemType<CondensedMomentumHourglass>())
			.AddIngredient(ModContent.ItemType<LongEdgeCrystalPendant>())
			.AddIngredient(ModContent.ItemType<RiftMomentumSigil>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
