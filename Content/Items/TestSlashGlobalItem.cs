using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class TestSlashGlobalItem : GlobalItem
{
	private bool s1;

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Item item)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldUseSlashAction(item))
		{
			s1 = true;
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

	public override bool AltFunctionUse(Item item, Player player)
	{
		if (!ModContent.GetInstance<Mconfig>().CanCharge)
		{
			return false;
		}
		if (s1)
		{
			return true;
		}
		return false;
	}

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (s1)
		{
			return false;
		}
		return true;
	}

	public override bool? UseItem(Item item, Player player)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		if (s1)
		{
			float scale = item.scale;
			Vector2 val = Utils.Size(TextureAssets.Item[item.type].Value);
			float num = scale * val.Length();
			if (player.altFunctionUse != 2)
			{
				Projectile obj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item, (string)null), ((Entity)player).Center, Vector2.Zero, ModContent.ProjectileType<ExampleSlash>(), player.GetWeaponDamage(item, false), player.GetWeaponKnockback(item), ((Entity)player).whoAmI, Utils.NextBool(Main.rand, 2) ? (-1.9f) : 1.9f, 0f, 0f);
				(obj.ModProjectile as ExampleSlash).Weapon = TextureAssets.Item[item.type].Value;
				obj.localAI[0] = item.useAnimation;
				obj.localAI[1] = num;
				obj.rotation = Utils.ToRotation(Main.MouseWorld - ((Entity)player).Center);
			}
			else if (ModContent.GetInstance<Mconfig>().CanCharge)
			{
				SoundStyle val2 = new SoundStyle("MeleeWeaponEffects/Sounds/Xuli") { Volume = 0.25f };
				SoundEngine.PlaySound(val2);
				Projectile obj2 = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item, (string)null), ((Entity)player).Center, Vector2.Zero, ModContent.ProjectileType<ExampleSlashWeapon>(), player.GetWeaponDamage(item, false) * ModContent.GetInstance<Mconfig>().ChargeDamage, player.GetWeaponKnockback(item), ((Entity)player).whoAmI, Utils.NextBool(Main.rand, 2) ? (-1.9f) : 1.9f, 0f, 0f);
				(obj2.ModProjectile as ExampleSlashWeapon).Weapon = TextureAssets.Item[item.type].Value;
				obj2.localAI[0] = item.useAnimation;
				obj2.localAI[1] = num;
				obj2.rotation = Utils.ToRotation(Main.MouseWorld - ((Entity)player).Center);
			}
			return true;
		}
		return null;
	}
}
