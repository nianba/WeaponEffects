using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace WeaponEffects;

public class StarSpark : ModDust
{
	public override string Texture => "WeaponEffects/Dusts/Spark";

	public override bool Update(Dust dust)
	{
		dust.velocity *= 0.9f;
		dust.position += dust.velocity;
		dust.rotation += 0.16f;
		dust.scale -= 0.035f;

		if (dust.scale <= 0f)
		{
			dust.active = false;
		}

		return false;
	}

	public override bool PreDraw(Dust dust)
	{
		Texture2D texture = Texture2D.Value;
		Vector2 position = dust.position - Main.screenPosition;
		Vector2 origin = texture.Size() / 2f;
		Color color = dust.color;
		color.A = 0;

		DrawBeam(texture, position, origin, color, dust.rotation, dust.scale);
		DrawBeam(texture, position, origin, color, dust.rotation + MathHelper.PiOver2, dust.scale * 0.82f);
		DrawBeam(texture, position, origin, Color.White * 0.55f, dust.rotation + MathHelper.PiOver4, dust.scale * 0.42f);
		DrawBeam(texture, position, origin, Color.White * 0.55f, dust.rotation - MathHelper.PiOver4, dust.scale * 0.42f);
		return false;
	}

	private static void DrawBeam(Texture2D texture, Vector2 position, Vector2 origin, Color color, float rotation, float scale)
	{
		Main.EntitySpriteDraw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
	}
}
