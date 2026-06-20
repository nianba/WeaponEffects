using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class DarkSpark : ModDust
{
	public override string Texture => "MeleeWeaponEffects/Dusts/Spark";

	public override void SetStaticDefaults()
	{
	}

	public override bool Update(Dust dust)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		float num = dust.velocity.Length();
		dust.velocity += Utils.NextVector2Circular(Main.rand, num / 5f, num / 5f);
		dust.velocity *= 0.967f;
		dust.position += dust.velocity;
		if (dust.scale < 0f)
		{
			dust.active = false;
		}
		dust.scale -= 0.018f;
		return false;
	}

	public override bool PreDraw(Dust dust)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		Texture2D value = ((ModDust)this).Texture2D.Value;
		Color color = dust.color;
		color.A = 0;
		Main.EntitySpriteDraw(value, dust.position - Main.screenPosition, (Rectangle?)null, color, Utils.ToRotation(dust.velocity), Utils.Size(value) / 2f, dust.scale, (SpriteEffects)0, 0f);
		Main.EntitySpriteDraw(value, dust.position - Main.screenPosition, (Rectangle?)null, new Color(55, 55, 55, 0), Utils.ToRotation(dust.velocity), Utils.Size(value) / 2f, dust.scale, (SpriteEffects)0, 0f);
		return false;
	}
}
