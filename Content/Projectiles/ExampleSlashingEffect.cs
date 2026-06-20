using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class ExampleSlashingEffect : ModProjectile
{
	public override string Texture => "Terraria/Images/Extra_89";

	public override void SetDefaults()
	{
		((Entity)((ModProjectile)this).Projectile).width = (((Entity)((ModProjectile)this).Projectile).height = 12);
		((ModProjectile)this).Projectile.timeLeft = 14;
		((ModProjectile)this).Projectile.friendly = false;
		((ModProjectile)this).Projectile.tileCollide = false;
		((ModProjectile)this).Projectile.ignoreWater = true;
	}

	public override void AI()
	{
	}

	public override bool PreDraw(ref Color lightColor)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Main.EntitySpriteDraw(TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value, ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition, (Rectangle?)null, Color.White, ((ModProjectile)this).Projectile.ai[0], Utils.Size(TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value) / 2f, new Vector2((float)((ModProjectile)this).Projectile.timeLeft / 14f, 8.8f), (SpriteEffects)0, 0f);
		return false;
	}
}
