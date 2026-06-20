using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SlashGlobalItem : GlobalItem
{
	private bool _usesSlashAction;

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Item item)
	{
		if (!ShouldUseSlashAction(item))
		{
			return;
		}

		_usesSlashAction = true;
		item.noUseGraphic = true;
		item.noMelee = true;
		item.channel = true;
		item.shootSpeed = 40f;
		item.useStyle = ItemUseStyleID.Rapier;
		item.UseSound = SoundID.Item1;

		if (ShouldClearOriginalShoot(item.shoot))
		{
			item.shoot = ProjectileID.None;
		}
	}

	public override bool AltFunctionUse(Item item, Player player)
	{
		return _usesSlashAction && ModContent.GetInstance<WeaponEffectsGameplayConfig>().CanCharge;
	}

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return !_usesSlashAction;
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (!_usesSlashAction || player.whoAmI != Main.myPlayer)
		{
			return _usesSlashAction ? true : null;
		}

		bool charged = player.altFunctionUse == 2;
		if (charged && !ModContent.GetInstance<WeaponEffectsGameplayConfig>().CanCharge)
		{
			return true;
		}

		StartSlashAction(item, player, charged);
		return true;
	}

	internal static bool TryStartChargeInterrupt(Player player)
	{
		if (!CanStartChargeFromInput(player))
		{
			return false;
		}

		Item item = player.HeldItem;
		if (!ShouldUseSlashAction(item) || HasOwnedProjectile(player, ModContent.ProjectileType<ChargedSlashProjectile>()))
		{
			return false;
		}

		int slashChannelType = ModContent.ProjectileType<SlashChannelProjectile>();
		bool interruptingSlashChannel = HasOwnedProjectile(player, slashChannelType);
		if (!interruptingSlashChannel && player.itemAnimation <= 0 && player.itemTime <= 0)
		{
			return false;
		}

		KillOwnedSlashChannels(player);
		player.itemAnimation = 0;
		player.itemTime = 0;
		StartSlashAction(item, player, charged: true);
		return true;
	}

	private static void StartSlashAction(Item item, Player player, bool charged)
	{
		float weaponLength = GetWeaponLength(item);
		Vector2 targetWorld = Main.MouseWorld;
		float startingRotation = Main.rand.NextBool() ? -1.9f : 1.9f;
		int projectileType = charged
			? ModContent.ProjectileType<ChargedSlashProjectile>()
			: ModContent.ProjectileType<SlashChannelProjectile>();

		if (charged)
		{
			SoundStyle chargeSound = new("WeaponEffects/Sounds/Xuli") { Volume = 0.25f };
			MeleeEffectAssets.PlaySound(in chargeSound, player.Center);
		}

		int projectileDamage = player.GetWeaponDamage(item);

		Projectile projectile = Projectile.NewProjectileDirect(
			player.GetSource_ItemUse(item),
			player.Center,
			Vector2.Zero,
			projectileType,
			projectileDamage,
			player.GetWeaponKnockback(item),
			player.whoAmI,
			startingRotation);

		projectile.localAI[0] = item.useAnimation;
		projectile.localAI[1] = weaponLength;
		projectile.rotation = (targetWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction).ToRotation();

		if (projectile.ModProjectile is SlashChannelProjectile slash)
		{
			slash.Initialize(item.type, item.useAnimation, weaponLength, targetWorld);
		}
		else if (projectile.ModProjectile is ChargedSlashProjectile chargedSlash)
		{
			chargedSlash.Initialize(item.type, item.useAnimation, weaponLength, targetWorld);
		}
	}

	private static bool ShouldUseSlashAction(Item item)
	{
		if (!ModContent.GetInstance<WeaponEffectsGameplayConfig>().EnableSlashRework)
		{
			return false;
		}

		if (item != null && item.type == ItemID.ShadowFlameKnife)
		{
			return false;
		}

		if (item == null || item.IsAir || item.damage <= 0 || item.type == ItemID.Sickle || item.accessory || item.axe > 0 || item.pick > 0 || item.hammer > 0)
		{
			return false;
		}

		if (item.DamageType != DamageClass.Melee && item.DamageType != DamageClass.MeleeNoSpeed)
		{
			return false;
		}

		if (!SlashProfileResolver.TryGetExactProfile(item.type, out _))
		{
			return false;
		}

		bool supportedShoot = item.shoot == ProjectileID.None || IsVanillaSwordSlashProjectile(item.shoot);
		return (!item.noMelee || supportedShoot) && (!item.channel || supportedShoot);
	}

	private static bool IsVanillaSwordSlashProjectile(int projectileType)
	{
		return projectileType == ProjectileID.NightsEdge
			|| projectileType == ProjectileID.Excalibur
			|| projectileType == ProjectileID.TrueExcalibur
			|| projectileType == ProjectileID.TrueNightsEdge
			|| projectileType == ProjectileID.TerraBlade2
			|| projectileType == ProjectileID.TerraBlade2Shot
			|| projectileType == ProjectileID.TheHorsemansBlade;
	}

	private static bool ShouldClearOriginalShoot(int projectileType)
	{
		return projectileType == ProjectileID.TrueNightsEdge
			|| projectileType == ProjectileID.TrueExcalibur
			|| projectileType == ProjectileID.TerraBlade2Shot;
	}

	private static float GetWeaponLength(Item item)
	{
		Texture2D texture = TextureAssets.Item[item.type].Value;
		return item.scale * new Vector2(texture.Width, texture.Height).Length();
	}

	private static bool CanStartChargeFromInput(Player player)
	{
		if (player.whoAmI != Main.myPlayer || !Main.mouseRight || !ModContent.GetInstance<WeaponEffectsGameplayConfig>().CanCharge)
		{
			return false;
		}

		if (!player.active || player.dead || player.noItems || player.CCed)
		{
			return false;
		}

		Item item = player.HeldItem;
		return item != null && !item.IsAir;
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

	private static void KillOwnedSlashChannels(Player player)
	{
		int slashChannelType = ModContent.ProjectileType<SlashChannelProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active && projectile.owner == player.whoAmI && projectile.type == slashChannelType)
			{
				projectile.Kill();
			}
		}
	}
}
