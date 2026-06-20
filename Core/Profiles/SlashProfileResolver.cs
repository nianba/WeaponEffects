using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace WeaponEffects;

public static class SlashProfileResolver
{
	private static readonly Dictionary<int, SlashProfileId> ExactProfiles = new()
	{
		[ItemID.WoodenSword] = SlashProfileId.WoodSword,
		[ItemID.BorealWoodSword] = SlashProfileId.WoodSword,
		[ItemID.PalmWoodSword] = SlashProfileId.WoodSword,
		[ItemID.EbonwoodSword] = SlashProfileId.WoodSword,
		[ItemID.ShadewoodSword] = SlashProfileId.WoodSword,
		[ItemID.RichMahoganySword] = SlashProfileId.WoodSword,
		[ItemID.PearlwoodSword] = SlashProfileId.WoodSword,
		[ItemID.AshWoodSword] = SlashProfileId.WoodSword,

		[ItemID.CopperBroadsword] = SlashProfileId.EarlyMetalSword,
		[ItemID.TinBroadsword] = SlashProfileId.EarlyMetalSword,
		[ItemID.IronBroadsword] = SlashProfileId.EarlyMetalSword,
		[ItemID.LeadBroadsword] = SlashProfileId.EarlyMetalSword,
		[ItemID.SilverBroadsword] = SlashProfileId.NobleMetalSword,
		[ItemID.TungstenBroadsword] = SlashProfileId.NobleMetalSword,
		[ItemID.GoldBroadsword] = SlashProfileId.NobleMetalSword,
		[ItemID.PlatinumBroadsword] = SlashProfileId.NobleMetalSword,

		[ItemID.CactusSword] = SlashProfileId.CactusSword,
		[ItemID.BoneSword] = SlashProfileId.BoneSword,
		[ItemID.ZombieArm] = SlashProfileId.ZombieArm,
		[ItemID.CandyCaneSword] = SlashProfileId.CandyCaneSword,
		[ItemID.Katana] = SlashProfileId.Katana,
		[ItemID.FalconBlade] = SlashProfileId.FalconBlade,

		[ItemID.LightsBane] = SlashProfileId.LightsBane,
		[ItemID.BloodButcherer] = SlashProfileId.BloodButcherer,
		[ItemID.BeeKeeper] = SlashProfileId.BeeKeeper,
		[ItemID.EnchantedSword] = SlashProfileId.EnchantedSword,

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
		[ItemID.Bladetongue] = SlashProfileId.Bladetongue,

		[ItemID.BluePhaseblade] = SlashProfileId.BluePhaseblade,
		[ItemID.RedPhaseblade] = SlashProfileId.RedPhaseblade,
		[ItemID.GreenPhaseblade] = SlashProfileId.GreenPhaseblade,
		[ItemID.PurplePhaseblade] = SlashProfileId.PurplePhaseblade,
		[ItemID.WhitePhaseblade] = SlashProfileId.WhitePhaseblade,
		[ItemID.YellowPhaseblade] = SlashProfileId.YellowPhaseblade,
		[ItemID.OrangePhaseblade] = SlashProfileId.OrangePhaseblade,

		[ItemID.BluePhasesaber] = SlashProfileId.BluePhasesaber,
		[ItemID.RedPhasesaber] = SlashProfileId.RedPhasesaber,
		[ItemID.GreenPhasesaber] = SlashProfileId.GreenPhasesaber,
		[ItemID.PurplePhasesaber] = SlashProfileId.PurplePhasesaber,
		[ItemID.WhitePhasesaber] = SlashProfileId.WhitePhasesaber,
		[ItemID.YellowPhasesaber] = SlashProfileId.YellowPhasesaber,
		[ItemID.OrangePhasesaber] = SlashProfileId.OrangePhasesaber,

		[ItemID.CobaltSword] = SlashProfileId.CobaltSword,
		[ItemID.PalladiumSword] = SlashProfileId.PalladiumSword,
		[ItemID.MythrilSword] = SlashProfileId.MythrilSword,
		[ItemID.OrichalcumSword] = SlashProfileId.OrichalcumSword,
		[ItemID.AdamantiteSword] = SlashProfileId.AdamantiteSword,
		[ItemID.TitaniumSword] = SlashProfileId.TitaniumSword,
		[ItemID.ChlorophyteSaber] = SlashProfileId.ChlorophyteSaber,
		[ItemID.ChlorophyteClaymore] = SlashProfileId.ChlorophyteClaymore,

		[ItemID.BreakerBlade] = SlashProfileId.BreakerBlade,
		[ItemID.Cutlass] = SlashProfileId.Cutlass,
		[ItemID.Keybrand] = SlashProfileId.Keybrand,
		[ItemID.BeamSword] = SlashProfileId.BeamSword,
		[ItemID.TerraBlade] = SlashProfileId.TerraBlade,
		[ItemID.TheHorsemansBlade] = SlashProfileId.TheHorsemansBlade,
		[ItemID.ChristmasTreeSword] = SlashProfileId.ChristmasTreeSword,
		[ItemID.Seedler] = SlashProfileId.Seedler,
		[ItemID.InfluxWaver] = SlashProfileId.InfluxWaver,
		[ItemID.StarWrath] = SlashProfileId.StarWrath,
		[ItemID.Meowmere] = SlashProfileId.Meowmere,
		[ItemID.PsychoKnife] = SlashProfileId.PsychoKnife,
		[ItemID.DD2SquireDemonSword] = SlashProfileId.BrandOfTheInferno,
		[ItemID.DD2SquireBetsySword] = SlashProfileId.FlyingDragon
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
			SlashProfileId.WoodSword => SlashProfiles.WoodSword,
			SlashProfileId.EarlyMetalSword => SlashProfiles.EarlyMetalSword,
			SlashProfileId.NobleMetalSword => SlashProfiles.NobleMetalSword,
			SlashProfileId.CactusSword => SlashProfiles.CactusSword,
			SlashProfileId.BoneSword => SlashProfiles.BoneSword,
			SlashProfileId.ZombieArm => SlashProfiles.ZombieArm,
			SlashProfileId.CandyCaneSword => SlashProfiles.CandyCaneSword,
			SlashProfileId.Katana => SlashProfiles.Katana,
			SlashProfileId.FalconBlade => SlashProfiles.FalconBlade,
			SlashProfileId.LightsBane => SlashProfiles.LightsBane,
			SlashProfileId.BloodButcherer => SlashProfiles.BloodButcherer,
			SlashProfileId.BeeKeeper => SlashProfiles.BeeKeeper,
			SlashProfileId.EnchantedSword => SlashProfiles.EnchantedSword,
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
			SlashProfileId.BluePhaseblade => SlashProfiles.BluePhaseblade,
			SlashProfileId.RedPhaseblade => SlashProfiles.RedPhaseblade,
			SlashProfileId.GreenPhaseblade => SlashProfiles.GreenPhaseblade,
			SlashProfileId.PurplePhaseblade => SlashProfiles.PurplePhaseblade,
			SlashProfileId.WhitePhaseblade => SlashProfiles.WhitePhaseblade,
			SlashProfileId.YellowPhaseblade => SlashProfiles.YellowPhaseblade,
			SlashProfileId.OrangePhaseblade => SlashProfiles.OrangePhaseblade,
			SlashProfileId.BluePhasesaber => SlashProfiles.BluePhasesaber,
			SlashProfileId.RedPhasesaber => SlashProfiles.RedPhasesaber,
			SlashProfileId.GreenPhasesaber => SlashProfiles.GreenPhasesaber,
			SlashProfileId.PurplePhasesaber => SlashProfiles.PurplePhasesaber,
			SlashProfileId.WhitePhasesaber => SlashProfiles.WhitePhasesaber,
			SlashProfileId.YellowPhasesaber => SlashProfiles.YellowPhasesaber,
			SlashProfileId.OrangePhasesaber => SlashProfiles.OrangePhasesaber,
			SlashProfileId.CobaltSword => SlashProfiles.CobaltSword,
			SlashProfileId.PalladiumSword => SlashProfiles.PalladiumSword,
			SlashProfileId.MythrilSword => SlashProfiles.MythrilSword,
			SlashProfileId.OrichalcumSword => SlashProfiles.OrichalcumSword,
			SlashProfileId.AdamantiteSword => SlashProfiles.AdamantiteSword,
			SlashProfileId.TitaniumSword => SlashProfiles.TitaniumSword,
			SlashProfileId.ChlorophyteSaber => SlashProfiles.ChlorophyteSaber,
			SlashProfileId.ChlorophyteClaymore => SlashProfiles.ChlorophyteClaymore,
			SlashProfileId.BreakerBlade => SlashProfiles.BreakerBlade,
			SlashProfileId.Cutlass => SlashProfiles.Cutlass,
			SlashProfileId.Keybrand => SlashProfiles.Keybrand,
			SlashProfileId.BeamSword => SlashProfiles.BeamSword,
			SlashProfileId.TerraBlade => SlashProfiles.TerraBlade,
			SlashProfileId.TheHorsemansBlade => SlashProfiles.TheHorsemansBlade,
			SlashProfileId.ChristmasTreeSword => SlashProfiles.ChristmasTreeSword,
			SlashProfileId.Seedler => SlashProfiles.Seedler,
			SlashProfileId.InfluxWaver => SlashProfiles.InfluxWaver,
			SlashProfileId.StarWrath => SlashProfiles.StarWrath,
			SlashProfileId.Meowmere => SlashProfiles.Meowmere,
			SlashProfileId.PsychoKnife => SlashProfiles.PsychoKnife,
			SlashProfileId.BrandOfTheInferno => SlashProfiles.BrandOfTheInferno,
			SlashProfileId.FlyingDragon => SlashProfiles.FlyingDragon,
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
