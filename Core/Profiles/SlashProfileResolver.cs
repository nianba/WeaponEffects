using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace MeleeWeaponEffects;

public static class SlashProfileResolver
{
	private static readonly Dictionary<int, SlashProfileId> ExactProfiles = new()
	{
		[ItemID.FieryGreatsword] = SlashProfileId.Volcano
	};

	public static WeaponSlashProfile GetProfile(Item item)
	{
		if (item != null && ExactProfiles.TryGetValue(item.type, out SlashProfileId profileId))
		{
			return GetProfile(profileId);
		}

		return GetFallbackProfile(item);
	}

	public static WeaponSlashProfile GetProfile(SlashProfileId profileId)
	{
		return profileId switch
		{
			SlashProfileId.Volcano => SlashProfiles.Volcano,
			_ => SlashProfiles.BalancedSword
		};
	}

	public static bool TryGetExactProfile(int itemType, out WeaponSlashProfile profile)
	{
		if (ExactProfiles.TryGetValue(itemType, out SlashProfileId profileId))
		{
			profile = GetProfile(profileId);
			return true;
		}

		profile = default;
		return false;
	}

	private static WeaponSlashProfile GetFallbackProfile(Item item)
	{
		return SlashProfiles.BalancedSword;
	}
}
