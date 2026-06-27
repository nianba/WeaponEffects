using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SpearGlobalItem : GlobalItem
{
	private bool _usesSpearAction;

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Item item)
	{
		if (!ShouldUseSpearAction(item))
		{
			return;
		}

		_usesSpearAction = true;
		item.noUseGraphic = true;
		item.noMelee = true;
		item.channel = true;
		item.shootSpeed = 40f;
		item.useStyle = ItemUseStyleID.Rapier;
		item.UseSound = SoundID.Item1;
		item.shoot = ProjectileID.None;
	}

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return !_usesSpearAction;
	}

	public override bool AltFunctionUse(Item item, Player player)
	{
		return _usesSpearAction;
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (!_usesSpearAction || player.whoAmI != Main.myPlayer)
		{
			return _usesSpearAction ? true : null;
		}

		if (player.altFunctionUse == 2)
		{
			StartSpearThrowCharge(item, player);
			return true;
		}

		StartSpearAction(item, player);
		return true;
	}

	internal static bool TryStartThrowChargeInterrupt(Player player)
	{
		if (!CanStartThrowChargeFromInput(player))
		{
			return false;
		}

		Item item = player.HeldItem;
		if (!ShouldUseSpearAction(item) || HasOwnedProjectile(player, ModContent.ProjectileType<SpearThrowChargeProjectile>()))
		{
			return false;
		}

		bool interruptingSpearChannel = HasOwnedProjectile(player, ModContent.ProjectileType<SpearChannelProjectile>())
			|| HasOwnedProjectile(player, ModContent.ProjectileType<SpearComboHeldProjectile>());
		if (!interruptingSpearChannel && player.itemAnimation <= 0 && player.itemTime <= 0)
		{
			return false;
		}

		StartSpearThrowCharge(item, player);
		return true;
	}

	private static void StartSpearThrowCharge(Item item, Player player)
	{
		if (HasOwnedProjectile(player, ModContent.ProjectileType<SpearThrowChargeProjectile>()))
		{
			return;
		}

		KillOwnedSpearChannels(player);
		player.GetModPlayer<WeaponEffectsPlayer>().ResetSpearCombo();
		player.itemAnimation = 0;
		player.itemTime = 0;

		float weaponLength = GetWeaponLength(item);
		Vector2 targetWorld = Main.MouseWorld;

		Projectile projectile = Projectile.NewProjectileDirect(
			player.GetSource_ItemUse(item),
			player.Center,
			Vector2.Zero,
			ModContent.ProjectileType<SpearThrowChargeProjectile>(),
			player.GetWeaponDamage(item),
			player.GetWeaponKnockback(item),
			player.whoAmI,
			player.direction);

		if (projectile.ModProjectile is SpearThrowChargeProjectile charge)
		{
			charge.Initialize(item.type, item.useAnimation, weaponLength, targetWorld);
		}

		MeleeEffectAssets.SyncProjectile(projectile);
	}

	private static void StartSpearAction(Item item, Player player)
	{
		float weaponLength = GetWeaponLength(item);
		Vector2 targetWorld = Main.MouseWorld;

		if (TryRefreshOwnedSpearCombo(player, item.type, item.useAnimation, weaponLength, targetWorld))
		{
			return;
		}

		Projectile projectile = Projectile.NewProjectileDirect(
			player.GetSource_ItemUse(item),
			player.Center,
			Vector2.Zero,
			ModContent.ProjectileType<SpearComboHeldProjectile>(),
			player.GetWeaponDamage(item),
			player.GetWeaponKnockback(item),
			player.whoAmI,
			player.direction);

		if (projectile.ModProjectile is SpearComboHeldProjectile spear)
		{
			spear.Initialize(item.type, item.useAnimation, weaponLength, targetWorld);
		}

		MeleeEffectAssets.SyncProjectile(projectile);
	}

	private static bool ShouldUseSpearAction(Item item)
	{
		return ModContent.GetInstance<WeaponEffectsGameplayConfig>().EnableSlashRework
			&& item != null
			&& !item.IsAir
			&& item.type == ItemID.Trident;
	}

	private static float GetWeaponLength(Item item)
	{
		Texture2D texture = TextureAssets.Item[item.type].Value;
		return item.scale * new Vector2(texture.Width, texture.Height).Length();
	}

	private static bool CanStartThrowChargeFromInput(Player player)
	{
		if (player.whoAmI != Main.myPlayer || !Main.mouseRight)
		{
			return false;
		}

		if (!player.active || player.dead || player.noItems || player.CCed)
		{
			return false;
		}

		if (player.mouseInterface || Main.playerInventory || Main.mapFullscreen || Main.drawingPlayerChat || Main.editSign || Main.editChest || Main.blockInput)
		{
			return false;
		}

		Item item = player.HeldItem;
		return item != null && !item.IsAir && ShouldUseSpearAction(item);
	}

	private static bool HasOwnedProjectile(Player player, int projectileType)
	{
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryRefreshOwnedSpearCombo(Player player, int weaponItemType, int useAnimation, float weaponLength, Vector2 targetWorld)
	{
		int spearComboType = ModContent.ProjectileType<SpearComboHeldProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active && projectile.owner == player.whoAmI && projectile.type == spearComboType)
			{
				if (projectile.ModProjectile is SpearComboHeldProjectile spear)
				{
					spear.RefreshWeaponState(weaponItemType, useAnimation, weaponLength, targetWorld);
				}

				return true;
			}
		}

		return false;
	}

	private static void KillOwnedSpearChannels(Player player)
	{
		int spearChannelType = ModContent.ProjectileType<SpearChannelProjectile>();
		int spearComboType = ModContent.ProjectileType<SpearComboHeldProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active
				&& projectile.owner == player.whoAmI
				&& (projectile.type == spearChannelType || projectile.type == spearComboType))
			{
				projectile.Kill();
			}
		}
	}
}
