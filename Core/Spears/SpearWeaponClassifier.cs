using Terraria;
using Terraria.ID;

namespace WeaponEffects.Spears;

public static class SpearWeaponClassifier
{
	public static bool IsSupportedSpear(Item item)
	{
		if (item == null || item.IsAir)
		{
			return false;
		}

		if (SpearItemIdResolver.Matches(item.type, SpearItemIdResolver.DreadOfTheRedSea)
			|| SpearItemIdResolver.Matches(item.type, SpearItemIdResolver.SlimeSpear))
		{
			return true;
		}

		return item.type switch
		{
			ItemID.Spear
				or ItemID.Trident
				or ItemID.ThunderSpear
				or ItemID.Swordfish
				or ItemID.TheRottedFork
				or ItemID.DarkLance
				or ItemID.CobaltNaginata
				or ItemID.PalladiumPike
				or ItemID.MythrilHalberd
				or ItemID.OrichalcumHalberd
				or ItemID.AdamantiteGlaive
				or ItemID.TitaniumTrident
				or ItemID.Gungnir
				or ItemID.ChlorophytePartisan
				or ItemID.MushroomSpear
				or ItemID.ObsidianSwordfish
				or ItemID.NorthPole => true,
			_ => false,
		};
	}
}
