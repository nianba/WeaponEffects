using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace MeleeWeaponEffects;

public static class SlashProfileResolver
{
	private static readonly Dictionary<int, SlashProfileId> ExactProfiles = new()
	{
		[ItemID.FieryGreatsword] = SlashProfileId.Volcano,
		[ItemID.NightsEdge] = SlashProfileId.NightsEdge,
		[ItemID.TrueNightsEdge] = SlashProfileId.TrueNightsEdge,
		[ItemID.Excalibur] = SlashProfileId.Excalibur,
		[ItemID.TrueExcalibur] = SlashProfileId.TrueExcalibur,
		[ItemID.BladeofGrass] = SlashProfileId.BladeOfGrass,
		[ItemID.Muramasa] = SlashProfileId.Muramasa,
		[ItemID.IceBlade] = SlashProfileId.IceBlade,
		[ItemID.Frostbrand] = SlashProfileId.Frostbrand,
		[ItemID.Starfury] = SlashProfileId.Starfury,
		[ItemID.Bladetongue] = SlashProfileId.Bladetongue
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
			SlashProfileId.NightsEdge => SlashProfiles.NightsEdge,
			SlashProfileId.TrueNightsEdge => SlashProfiles.TrueNightsEdge,
			SlashProfileId.Excalibur => SlashProfiles.Excalibur,
			SlashProfileId.TrueExcalibur => SlashProfiles.TrueExcalibur,
			SlashProfileId.BladeOfGrass => SlashProfiles.BladeOfGrass,
			SlashProfileId.Muramasa => SlashProfiles.Muramasa,
			SlashProfileId.IceBlade => SlashProfiles.IceBlade,
			SlashProfileId.Frostbrand => SlashProfiles.Frostbrand,
			SlashProfileId.Starfury => SlashProfiles.Starfury,
			SlashProfileId.Bladetongue => SlashProfiles.Bladetongue,
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
