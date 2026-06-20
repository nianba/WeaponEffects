using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class SlashArcGlowProjectile : ModProjectile
{
	private readonly SlashVertex[] _vertices = new SlashVertex[80];
	private int _vertexCount;

	private bool _reverse;
	private bool _npcOwned;
	private Color _color = Color.White;

	public override string Texture => MeleeEffectAssets.SlashGlowTexture;

	public static void InitializeGlow(Projectile projectile, float startingRotation, float thickness, float yScale, int extraUpdates, bool npcOwned, Color color)
	{
		projectile.rotation = startingRotation;
		projectile.localAI[0] = thickness;
		projectile.localAI[1] = yScale;
		projectile.extraUpdates = extraUpdates;

		if (projectile.ModProjectile is SlashArcGlowProjectile glow)
		{
			glow._reverse = startingRotation > 0f;
			glow._npcOwned = npcOwned;
			glow._color = color == default ? Color.White : color;
			projectile.netUpdate = true;
		}
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.width = 24;
		Projectile.height = 24;
		Projectile.timeLeft = 100;
		Projectile.tileCollide = false;
		Projectile.penetrate = -1;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_reverse);
		writer.Write(_npcOwned);
		writer.Write(_color.PackedValue);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_reverse = reader.ReadBoolean();
		_npcOwned = reader.ReadBoolean();
		_color = new Color { PackedValue = reader.ReadUInt32() };
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override void AI()
	{
		if (Projectile.timeLeft > 40)
		{
			float rotationStep = MathHelper.Lerp(0.14f, 0f, 1f - (Projectile.timeLeft - 40) / 60f);
			Projectile.rotation += _reverse ? -rotationStep : rotationStep;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (ModContent.GetInstance<MeleeWeaponEffectsVisualConfig>().SlashStyle != 1)
		{
			return false;
		}

		BuildVertices(OwnerCenterWorld() - Main.screenPosition);
		if (_vertexCount < 3)
		{
			return false;
		}

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Projectile[Projectile.type].Value;
		Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, _vertexCount - 2);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		return false;
	}

	private Vector2 OwnerCenterWorld()
	{
		if (_npcOwned)
		{
			int npcIndex = (int)Projectile.ai[0];
			if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
			{
				return Main.npc[npcIndex].Center;
			}
		}

		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			return Main.player[Projectile.owner].Center;
		}

		return Projectile.Center;
	}

	private void BuildVertices(Vector2 ownerCenter)
	{
		_vertexCount = 0;
		float blink = ModContent.GetInstance<MeleeWeaponEffectsVisualConfig>().SlashBlink;

		for (int i = 0; i < Projectile.oldPos.Length; i++)
		{
			if (Projectile.oldRot[i] == 0f)
			{
				continue;
			}

			float factor = 1f - i / 40f;
			Vector2 outer = Projectile.oldRot[i].ToRotationVector2() * Projectile.velocity.Length();
			outer.Y *= Projectile.localAI[1];
			outer = outer.RotatedBy(Projectile.ai[1]);

			Vector2 inner = Projectile.oldRot[i].ToRotationVector2() * Projectile.velocity.Length() * (1f - Projectile.localAI[0] + Projectile.localAI[0] * i / 40f);
			inner.Y *= Projectile.localAI[1];
			inner = inner.RotatedBy(Projectile.ai[1]);

			if (_vertexCount + 2 > _vertices.Length)
			{
				break;
			}

			Color color = _color * blink * factor;
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + outer, new Vector3(factor, 0f, 1f), color);
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + inner, new Vector3(factor, 1f, 1f), color);
		}
	}
}
