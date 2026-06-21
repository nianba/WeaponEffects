using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class RiftMomentumSigil : BladeAccessoryItem
{
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterChargeAccessory(1f, 0f, 0f, 0.10f, 0.25f, 0f);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofNight, 6)
			.AddIngredient(ItemID.SoulofLight, 6)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
