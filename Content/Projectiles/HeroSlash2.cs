using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class HeroSlash2 : ModProjectile
{
	private Effect Mhd = ModContent.Request<Effect>("MeleeWeaponEffects/Effects/Mhd", (AssetRequestMode)2).Value;

	public bool Reverse;

	public bool IsNPCproj;

	public Color c = Color.White;

	public Texture2D T;

	public override string Texture => "MeleeWeaponEffects/EX112";

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[((ModProjectile)this).Projectile.type] = 40;
		ProjectileID.Sets.TrailingMode[((ModProjectile)this).Projectile.type] = 2;
	}

	public override void SetDefaults()
	{
		((Entity)((ModProjectile)this).Projectile).width = (((Entity)((ModProjectile)this).Projectile).height = 24);
		((ModProjectile)this).Projectile.timeLeft = 100;
		((ModProjectile)this).Projectile.tileCollide = false;
		((ModProjectile)this).Projectile.penetrate = -1;
		((ModProjectile)this).Projectile.ignoreWater = true;
		((ModProjectile)this).Projectile.aiStyle = -11;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override void AI()
	{
		if (((ModProjectile)this).Projectile.timeLeft > 40)
		{
			float num = MathHelper.Lerp(0.14f, 0f, 1f - (float)(((ModProjectile)this).Projectile.timeLeft - 40) / 60f);
			if (!Reverse)
			{
				Projectile projectile = ((ModProjectile)this).Projectile;
				projectile.rotation += num;
			}
			else
			{
				Projectile projectile2 = ((ModProjectile)this).Projectile;
				projectile2.rotation -= num;
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.Zero;
		zero = ((!IsNPCproj) ? (((Entity)Main.player[((ModProjectile)this).Projectile.owner]).Center - Main.screenPosition) : (((Entity)Main.npc[(int)((ModProjectile)this).Projectile.ai[0]]).Center - Main.screenPosition));
		List<VertexInfo2> list = new List<VertexInfo2>();
		for (int i = 0; i < ((ModProjectile)this).Projectile.oldPos.Length; i++)
		{
			if (((ModProjectile)this).Projectile.oldRot[i] != 0f && ModContent.GetInstance<Mconfig>().Style == 0)
			{
				Vector2 val = Utils.ToRotationVector2(((ModProjectile)this).Projectile.oldRot[i]) * ((Entity)((ModProjectile)this).Projectile).velocity.Length();
				val.Y *= ((ModProjectile)this).Projectile.localAI[1];
				val = Utils.RotatedBy(val, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
				list.Add(new VertexInfo2(zero + val, new Vector3(1f - (float)i / 40f, 0f, 1f), Color.White * ModContent.GetInstance<Mconfig>().SlashBlink));
				Vector2 val2 = Utils.ToRotationVector2(((ModProjectile)this).Projectile.oldRot[i]) * ((Entity)((ModProjectile)this).Projectile).velocity.Length() * (1f - ((ModProjectile)this).Projectile.localAI[0] + ((ModProjectile)this).Projectile.localAI[0] * (float)i / 40f);
				val2.Y *= ((ModProjectile)this).Projectile.localAI[1];
				val2 = Utils.RotatedBy(val2, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
				list.Add(new VertexInfo2(zero + val2, new Vector3(1f - (float)i / 40f, 1f, 1f), Color.White * ModContent.GetInstance<Mconfig>().SlashBlink));
			}
		}
		Main.spriteBatch.End();
		Main.spriteBatch.Begin((SpriteSortMode)1, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, Main.GameViewMatrix.TransformationMatrix);
		Main.graphics.GraphicsDevice.Textures[0] = (Texture)(object)TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value;
		if (list.Count >= 3)
		{
			Main.graphics.GraphicsDevice.DrawUserPrimitives<VertexInfo2>((PrimitiveType)1, list.ToArray(), 0, list.Count - 2);
		}
		Main.spriteBatch.End();
		Main.spriteBatch.Begin((SpriteSortMode)1, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, Main.GameViewMatrix.TransformationMatrix);
		return false;
	}
}
