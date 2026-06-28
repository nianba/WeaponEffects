using Microsoft.Xna.Framework;
using Terraria.ID;

namespace WeaponEffects.Spears;

public static class SpearVisualProfileResolver
{
	private const string SpearSweepColorRoot = MeleeEffectAssets.TextureRoot + "/SpearSweepColors";

	private static readonly SpearVisualProfile FallbackProfile = Profile("Trident", new Color(250, 236, 182, 0));

	public static SpearVisualProfile Resolve(int weaponItemType)
	{
		if (SpearItemIdResolver.Matches(weaponItemType, SpearItemIdResolver.DreadOfTheRedSea))
		{
			return Profile("GhastlyGlaive", new Color(120, 245, 205, 0));
		}

		if (SpearItemIdResolver.Matches(weaponItemType, SpearItemIdResolver.SlimeSpear))
		{
			return Profile("SlimeSpear", new Color(118, 236, 132, 0));
		}

		return weaponItemType switch
		{
			ItemID.Spear => Profile(nameof(ItemID.Spear), new Color(224, 228, 220, 0)),
			ItemID.Trident => FallbackProfile,
			ItemID.ThunderSpear => Profile("StormSpear", new Color(245, 218, 118, 0)),
			ItemID.Swordfish => Profile(nameof(ItemID.Swordfish), new Color(152, 218, 246, 0)),
			ItemID.TheRottedFork => Profile(nameof(ItemID.TheRottedFork), new Color(188, 122, 82, 0)),
			ItemID.DarkLance => Profile(nameof(ItemID.DarkLance), new Color(180, 78, 226, 0)),
			ItemID.CobaltNaginata => Profile(nameof(ItemID.CobaltNaginata), new Color(95, 186, 255, 0)),
			ItemID.PalladiumPike => Profile(nameof(ItemID.PalladiumPike), new Color(255, 150, 92, 0)),
			ItemID.MythrilHalberd => Profile(nameof(ItemID.MythrilHalberd), new Color(93, 246, 210, 0)),
			ItemID.OrichalcumHalberd => Profile(nameof(ItemID.OrichalcumHalberd), new Color(245, 98, 210, 0)),
			ItemID.AdamantiteGlaive => Profile(nameof(ItemID.AdamantiteGlaive), new Color(255, 82, 98, 0)),
			ItemID.TitaniumTrident => Profile(nameof(ItemID.TitaniumTrident), new Color(230, 238, 248, 0)),
			ItemID.Gungnir => Profile(nameof(ItemID.Gungnir), new Color(255, 228, 112, 0)),
			ItemID.ChlorophytePartisan => Profile(nameof(ItemID.ChlorophytePartisan), new Color(128, 244, 98, 0)),
			ItemID.MushroomSpear => Profile(nameof(ItemID.MushroomSpear), new Color(104, 178, 255, 0)),
			ItemID.ObsidianSwordfish => Profile(nameof(ItemID.ObsidianSwordfish), new Color(238, 92, 70, 0)),
			ItemID.NorthPole => Profile(nameof(ItemID.NorthPole), new Color(164, 228, 255, 0)),
			_ => FallbackProfile,
		};
	}

	private static SpearVisualProfile Profile(string itemName, Color tipGlowColor)
	{
		return new SpearVisualProfile($"{SpearSweepColorRoot}/{itemName}", tipGlowColor);
	}
}
