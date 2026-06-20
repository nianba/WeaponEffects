using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class StarSlashSparkleProjectile : ModProjectile
{
	private const int Lifetime = 22;
	private const int MaxVertices = 144;
	private static BasicEffect _effect;
	private static GraphicsDevice _effectDevice;

	private readonly VertexPositionColor[] _vertices = new VertexPositionColor[MaxVertices];
	private int _vertexCount;
	private Color _color = Color.White;
	private float _baseRotation;
	private bool _cluster;

	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FallingStar;

	public static void Spawn(Vector2 position, Vector2 velocity, Color color, float scale, bool cluster)
	{
		if (Main.dedServ)
		{
			return;
		}

		Projectile projectile = Projectile.NewProjectileDirect(
			new Terraria.DataStructures.EntitySource_Misc("WeaponEffects:StarSlashSparkle"),
			position,
			velocity,
			ModContent.ProjectileType<StarSlashSparkleProjectile>(),
			0,
			0f,
			Main.myPlayer,
			MathHelper.Clamp(scale, 0.2f, 2.2f),
			cluster ? 1f : 0f);

		if (projectile.ModProjectile is StarSlashSparkleProjectile sparkle)
		{
			sparkle.Initialize(color, Main.rand.NextFloat(MathHelper.TwoPi), cluster);
		}
	}

	public override void SetDefaults()
	{
		Projectile.width = 4;
		Projectile.height = 4;
		Projectile.timeLeft = Lifetime;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.aiStyle = -1;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_color.PackedValue);
		writer.Write(_baseRotation);
		writer.Write(_cluster);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_color = new Color { PackedValue = reader.ReadUInt32() };
		_baseRotation = reader.ReadSingle();
		_cluster = reader.ReadBoolean();
	}

	public void Initialize(Color color, float rotation, bool cluster)
	{
		_color = color == default ? Color.White : color;
		_baseRotation = rotation;
		_cluster = cluster;
		Projectile.netUpdate = true;
	}

	public override void AI()
	{
		Projectile.velocity *= 0.92f;
		Projectile.rotation += 0.035f;
		Lighting.AddLight(Projectile.Center, _color.ToVector3() * 0.22f);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		float progress = 1f - Projectile.timeLeft / (float)Lifetime;
		float fadeIn = Smooth01(MathHelper.Clamp(progress / 0.14f, 0f, 1f));
		float fadeOut = Smooth01(MathHelper.Clamp(Projectile.timeLeft / (float)Lifetime, 0f, 1f));
		float alpha = fadeIn * fadeOut;
		if (alpha <= 0.01f)
		{
			return false;
		}

		_vertexCount = 0;
		Vector2 center = Projectile.Center - Main.screenPosition;
		float scale = Projectile.ai[0] * (0.9f + MathF.Sin(progress * MathHelper.Pi) * 0.22f);
		float rotation = _baseRotation + Projectile.rotation;

		AddStar(center, scale * 1.45f, rotation, _color, alpha * 0.35f, solidCore: false);
		AddStar(center, scale, rotation, _color, alpha, solidCore: true);

		if (_cluster)
		{
			Vector2 right = new Vector2(18f, 3f).RotatedBy(rotation) * scale;
			Vector2 left = new Vector2(-16f, 10f).RotatedBy(rotation) * scale;
			AddStar(center + right, scale * 0.42f, rotation - 0.35f, _color, alpha * 0.85f, solidCore: true);
			AddStar(center + left, scale * 0.34f, rotation + 0.42f, Color.Lerp(_color, Color.White, 0.35f), alpha * 0.7f, solidCore: true);
		}

		if (_vertexCount < 3)
		{
			return false;
		}

		DrawVertices();
		return false;
	}

	private void AddStar(Vector2 center, float scale, float rotation, Color color, float alpha, bool solidCore)
	{
		float longRadius = 16f * scale;
		float sideRadius = 12f * scale;
		float notchRadius = 2.8f * scale;
		Color centerColor = Color.Lerp(color, Color.White, solidCore ? 0.72f : 0.42f) * alpha;
		Color tipColor = color * alpha * (solidCore ? 0.9f : 0.18f);
		Color notchColor = color * alpha * (solidCore ? 0.72f : 0.05f);

		Vector2[] points =
		{
			Point(center, rotation - MathHelper.PiOver2, longRadius),
			Point(center, rotation - MathHelper.PiOver4, notchRadius),
			Point(center, rotation, sideRadius),
			Point(center, rotation + MathHelper.PiOver4, notchRadius),
			Point(center, rotation + MathHelper.PiOver2, longRadius),
			Point(center, rotation + MathHelper.Pi * 0.75f, notchRadius),
			Point(center, rotation + MathHelper.Pi, sideRadius),
			Point(center, rotation - MathHelper.Pi * 0.75f, notchRadius)
		};

		for (int i = 0; i < points.Length; i++)
		{
			Color first = i % 2 == 0 ? tipColor : notchColor;
			Color second = (i + 1) % 2 == 0 ? tipColor : notchColor;
			AddTriangle(center, centerColor, points[i], first, points[(i + 1) % points.Length], second);
		}

		if (solidCore)
		{
			AddDiamond(center, rotation, scale * 3.1f, Color.Lerp(color, Color.White, 0.9f) * alpha);
		}
	}

	private void AddDiamond(Vector2 center, float rotation, float radius, Color color)
	{
		Vector2 top = Point(center, rotation - MathHelper.PiOver2, radius);
		Vector2 right = Point(center, rotation, radius * 0.78f);
		Vector2 bottom = Point(center, rotation + MathHelper.PiOver2, radius);
		Vector2 left = Point(center, rotation + MathHelper.Pi, radius * 0.78f);
		AddTriangle(top, color, right, color, bottom, color);
		AddTriangle(top, color, bottom, color, left, color);
	}

	private static Vector2 Point(Vector2 center, float rotation, float radius)
	{
		return center + rotation.ToRotationVector2() * radius;
	}

	private void AddTriangle(Vector2 a, Color colorA, Vector2 b, Color colorB, Vector2 c, Color colorC)
	{
		if (_vertexCount + 3 > _vertices.Length)
		{
			return;
		}

		_vertices[_vertexCount++] = new VertexPositionColor(new Vector3(a, 0f), colorA);
		_vertices[_vertexCount++] = new VertexPositionColor(new Vector3(b, 0f), colorB);
		_vertices[_vertexCount++] = new VertexPositionColor(new Vector3(c, 0f), colorC);
	}

	private void DrawVertices()
	{
		GraphicsDevice device = Main.graphics.GraphicsDevice;
		BasicEffect effect = GetEffect(device);
		effect.World = Matrix.Identity;
		effect.View = Main.GameViewMatrix.TransformationMatrix;
		effect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);

		Main.spriteBatch.End();
		device.BlendState = BlendState.Additive;
		device.DepthStencilState = DepthStencilState.None;
		device.RasterizerState = RasterizerState.CullNone;

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertexCount / 3);
		}

		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	}

	private static BasicEffect GetEffect(GraphicsDevice device)
	{
		if (_effect == null || _effectDevice != device)
		{
			_effect?.Dispose();
			_effect = new BasicEffect(device)
			{
				VertexColorEnabled = true,
				TextureEnabled = false
			};
			_effectDevice = device;
		}

		return _effect;
	}

	private static float Smooth01(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}
}
