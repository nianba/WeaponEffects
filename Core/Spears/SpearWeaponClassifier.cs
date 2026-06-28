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

		return item.type == ItemID.Trident;
	}
}
