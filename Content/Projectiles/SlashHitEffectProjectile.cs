using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace WeaponEffects;

public class SlashHitEffectProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Extra_89";

	public override void SetDefaults()
	{
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.timeLeft = 14;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Main.EntitySpriteDraw(
			texture,
			Projectile.Center - Main.screenPosition,
			null,
			Color.White,
			Projectile.ai[0],
			new Vector2(texture.Width, texture.Height) / 2f,
			new Vector2(Projectile.timeLeft / 14f, 8.8f),
			SpriteEffects.None,
			0f);
		return false;
	}
}
