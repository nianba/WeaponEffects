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

	public override bool? UseItem(Item item, Player player)
	{
		if (!_usesSpearAction || player.whoAmI != Main.myPlayer)
		{
			return _usesSpearAction ? true : null;
		}

		StartSpearAction(item, player);
		return true;
	}

	private static void StartSpearAction(Item item, Player player)
	{
		float weaponLength = GetWeaponLength(item);
		Vector2 targetWorld = Main.MouseWorld;

		Projectile projectile = Projectile.NewProjectileDirect(
			player.GetSource_ItemUse(item),
			player.Center,
			Vector2.Zero,
			ModContent.ProjectileType<SpearChannelProjectile>(),
			player.GetWeaponDamage(item),
			player.GetWeaponKnockback(item),
			player.whoAmI,
			player.direction);

		if (projectile.ModProjectile is SpearChannelProjectile spear)
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
}
