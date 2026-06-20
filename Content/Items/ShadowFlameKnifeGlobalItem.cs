using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class ShadowFlameKnifeGlobalItem : GlobalItem
{
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return entity.type == ItemID.ShadowFlameKnife;
	}

	public override void SetDefaults(Item item)
	{
		if (item.type != ItemID.ShadowFlameKnife)
		{
			return;
		}

		item.shoot = ModContent.ProjectileType<ShadowFlamePierceKnifeProjectile>();
		item.shootSpeed = ShadowFlameKnifeTuning.PierceSpeed;
		item.noMelee = true;
		item.noUseGraphic = true;
	}

	public override bool AltFunctionUse(Item item, Player player)
	{
		return item.type == ItemID.ShadowFlameKnife;
	}

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (item.type != ItemID.ShadowFlameKnife)
		{
			return true;
		}

		if (player.altFunctionUse == 2)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				ShadowFlameKnifeHelper.RecallAll(player, source, damage, knockback);
			}

			return false;
		}

		return true;
	}
}
