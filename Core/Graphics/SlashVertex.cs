using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WeaponEffects;

public struct SlashVertex : IVertexType
{
	private static readonly VertexDeclaration Declaration = new(
		new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
		new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
		new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));

	public Vector2 Position;
	public Color Color;
	public Vector3 TexCoord;

	public readonly VertexDeclaration VertexDeclaration => Declaration;

	public SlashVertex(Vector2 position, Vector3 texCoord, Color color)
	{
		Position = position;
		TexCoord = texCoord;
		Color = color;
	}
}
