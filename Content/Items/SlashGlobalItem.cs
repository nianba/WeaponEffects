using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

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
		return _usesSlashAction && ModContent.GetInstance<MeleeWeaponEffectsGameplayConfig>().CanCharge;
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

		float weaponLength = GetWeaponLength(item);
		Vector2 targetWorld = Main.MouseWorld;
		float startingRotation = Main.rand.NextBool() ? -1.9f : 1.9f;
		int projectileType = player.altFunctionUse == 2
			? ModContent.ProjectileType<ChargedSlashProjectile>()
			: ModContent.ProjectileType<SlashChannelProjectile>();

		if (player.altFunctionUse == 2 && ModContent.GetInstance<MeleeWeaponEffectsGameplayConfig>().CanCharge)
		{
			SoundEngine.PlaySound(new SoundStyle("MeleeWeaponEffects/Sounds/Xuli") { Volume = 0.25f }, player.Center);
		}
		else if (player.altFunctionUse == 2)
		{
			return true;
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

		return true;
	}

	private static bool ShouldUseSlashAction(Item item)
	{
		if (item.damage <= 0 || item.type == ItemID.Sickle || item.accessory || item.axe > 0 || item.pick > 0 || item.hammer > 0)
		{
			return false;
		}

		if (item.DamageType != DamageClass.Melee && item.DamageType != DamageClass.MeleeNoSpeed)
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
}
