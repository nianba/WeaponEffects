using Terraria.ID;

namespace WeaponEffects.Spears;

public static class SpearItemIdResolver
{
	public static readonly int DreadOfTheRedSea = TryGetItemId("DreadoftheRedSea");
	public static readonly int SlimeSpear = TryGetItemId("SlimeSpear");

	public static bool Matches(int itemType, int resolvedItemType)
	{
		return resolvedItemType > 0 && itemType == resolvedItemType;
	}

	private static int TryGetItemId(string itemName)
	{
		try
		{
			return ItemID.Search.GetId(itemName);
		}
		catch
		{
			return 0;
		}
	}
}
