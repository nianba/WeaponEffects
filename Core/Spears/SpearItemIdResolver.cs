using System;
using System.Reflection;
using Terraria.ID;

namespace WeaponEffects.Spears;

public static class SpearItemIdResolver
{
	public static readonly int DreadOfTheRedSea = TryGetVanillaItemId("DreadoftheRedSea");
	public static readonly int SlimeSpear = TryGetVanillaItemId("SlimeSpear");

	public static bool Matches(int itemType, int resolvedItemType)
	{
		return resolvedItemType > 0 && itemType == resolvedItemType;
	}

	public static int TryGetVanillaProjectileId(string projectileName)
	{
		return TryGetVanillaId(typeof(ProjectileID), projectileName);
	}

	private static int TryGetVanillaItemId(string itemName)
	{
		return TryGetVanillaId(typeof(ItemID), itemName);
	}

	private static int TryGetVanillaId(Type idType, string name)
	{
		FieldInfo field = idType.GetField(name, BindingFlags.Public | BindingFlags.Static);
		return field?.FieldType == typeof(int) && field.GetValue(null) is int id ? id : 0;
	}
}
