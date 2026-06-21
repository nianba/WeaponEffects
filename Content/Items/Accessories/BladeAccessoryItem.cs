using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public abstract class BladeAccessoryItem : ModItem
{
	private const string TextureRoot = "WeaponEffects/Content/Items/Accessories";
	protected const int BladeMomentumGaleDuration = 120;
	protected const int BladeMomentumDefaultDuration = 180;

	public override string Texture => Type switch
	{
		int type when type == ModContent.ItemType<GaleSwordCharm>() => TextureRoot + "/Momentum/GaleSwordCharm",
		int type when type == ModContent.ItemType<ArmorBreakSwordCharm>() => TextureRoot + "/Momentum/ArmorBreakSwordCharm",
		int type when type == ModContent.ItemType<SharpSwordCharm>() => TextureRoot + "/Momentum/SharpSwordCharm",
		int type when type == ModContent.ItemType<TriBladeMomentumBadge>() => TextureRoot + "/Momentum/TriBladeMomentumBadge",
		int type when type == ModContent.ItemType<MountainBreakerPendant>() => TextureRoot + "/HeavySlash/MountainBreakerPendant",
		int type when type == ModContent.ItemType<ExtendedEdgePendant>() => TextureRoot + "/HeavySlash/ExtendedEdgePendant",
		int type when type == ModContent.ItemType<CollapseEdgeBadge>() => TextureRoot + "/HeavySlash/CollapseEdgeBadge",
		int type when type == ModContent.ItemType<CondensedMomentumHourglass>() => TextureRoot + "/Charge/CondensedMomentumHourglass",
		int type when type == ModContent.ItemType<LongEdgeCrystalPendant>() => TextureRoot + "/Charge/LongEdgeCrystalPendant",
		int type when type == ModContent.ItemType<RiftMomentumSigil>() => TextureRoot + "/Charge/RiftMomentumSigil",
		int type when type == ModContent.ItemType<FocusedMomentumEmblem>() => TextureRoot + "/Charge/FocusedMomentumEmblem",
		int type when type == ModContent.ItemType<BladeHeart>() => TextureRoot + "/BladeHeart/BladeHeart",
		_ => base.Texture
	};

	public override void SetDefaults()
	{
		Item.width = 32;
		Item.height = 32;
		Item.accessory = true;
		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 2);
	}

	public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
	{
		return !ConflictsWith(equippedItem.type, incomingItem.type);
	}

	protected static void ApplyMomentumStats(Player player)
	{
		player.GetModPlayer<WeaponEffectsPlayer>().RegisterBladeMomentum(
			BladeMomentumDefaultDuration,
			attackSpeedPerStack: 0.03f,
			attackSpeedMaxStacks: 8,
			critPerStack: 2f,
			critMaxStacks: 4,
			damagePerStack: 0.02f,
			damageMaxStacks: 5);
	}

	protected static bool ConflictsWith(int firstItemType, int secondItemType)
	{
		BladeAccessoryTier firstTier = GetAccessoryTier(firstItemType);
		BladeAccessoryTier secondTier = GetAccessoryTier(secondItemType);

		if (firstTier == BladeAccessoryTier.None || secondTier == BladeAccessoryTier.None)
		{
			return false;
		}

		if (firstTier == BladeAccessoryTier.BladeHeart || secondTier == BladeAccessoryTier.BladeHeart)
		{
			return true;
		}

		return GetAccessoryGroup(firstItemType) == GetAccessoryGroup(secondItemType)
			&& (firstTier == BladeAccessoryTier.LineFinal || secondTier == BladeAccessoryTier.LineFinal);
	}

	private static BladeAccessoryGroup GetAccessoryGroup(int itemType)
	{
		if (itemType == ModContent.ItemType<GaleSwordCharm>()
			|| itemType == ModContent.ItemType<ArmorBreakSwordCharm>()
			|| itemType == ModContent.ItemType<SharpSwordCharm>()
			|| itemType == ModContent.ItemType<TriBladeMomentumBadge>())
		{
			return BladeAccessoryGroup.Momentum;
		}

		if (itemType == ModContent.ItemType<MountainBreakerPendant>()
			|| itemType == ModContent.ItemType<ExtendedEdgePendant>()
			|| itemType == ModContent.ItemType<CollapseEdgeBadge>())
		{
			return BladeAccessoryGroup.HeavySlash;
		}

		if (itemType == ModContent.ItemType<CondensedMomentumHourglass>()
			|| itemType == ModContent.ItemType<LongEdgeCrystalPendant>()
			|| itemType == ModContent.ItemType<RiftMomentumSigil>()
			|| itemType == ModContent.ItemType<FocusedMomentumEmblem>())
		{
			return BladeAccessoryGroup.Charge;
		}

		if (itemType == ModContent.ItemType<BladeHeart>())
		{
			return BladeAccessoryGroup.BladeHeart;
		}

		return BladeAccessoryGroup.None;
	}

	[Flags]
	private enum BladeAccessoryGroup
	{
		None = 0,
		Momentum = 1,
		HeavySlash = 2,
		Charge = 4,
		BladeHeart = Momentum | HeavySlash | Charge
	}

	private static BladeAccessoryTier GetAccessoryTier(int itemType)
	{
		if (itemType == ModContent.ItemType<BladeHeart>())
		{
			return BladeAccessoryTier.BladeHeart;
		}

		if (itemType == ModContent.ItemType<TriBladeMomentumBadge>()
			|| itemType == ModContent.ItemType<CollapseEdgeBadge>()
			|| itemType == ModContent.ItemType<FocusedMomentumEmblem>())
		{
			return BladeAccessoryTier.LineFinal;
		}

		if (GetAccessoryGroup(itemType) != BladeAccessoryGroup.None)
		{
			return BladeAccessoryTier.Component;
		}

		return BladeAccessoryTier.None;
	}

	private enum BladeAccessoryTier
	{
		None,
		Component,
		LineFinal,
		BladeHeart
	}
}
