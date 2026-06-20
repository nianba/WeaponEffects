using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace WeaponEffects;

public class DarkSpark : ModDust
{
	public override string Texture => "WeaponEffects/Dusts/Spark";

	public override bool Update(Dust dust)
	{
		float speed = dust.velocity.Length();
		dust.velocity += Main.rand.NextVector2Circular(speed / 5f, speed / 5f);
		dust.velocity *= 0.967f;
		dust.position += dust.velocity;
		dust.scale -= 0.018f;

		if (dust.scale < 0f)
		{
			dust.active = false;
		}

		return false;
	}

	public override bool PreDraw(Dust dust)
	{
		Texture2D texture = Texture2D.Value;
		Color color = dust.color;
		color.A = 0;

		Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, null, color, dust.velocity.ToRotation(), texture.Size() / 2f, dust.scale, SpriteEffects.None, 0f);
		Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, null, new Color(55, 55, 55, 0), dust.velocity.ToRotation(), texture.Size() / 2f, dust.scale, SpriteEffects.None, 0f);
		return false;
	}
}
