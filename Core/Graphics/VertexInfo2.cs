using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MeleeWeaponEffects;

public struct VertexInfo2 : IVertexType
{
	private static VertexDeclaration _vertexDeclaration = new VertexDeclaration((VertexElement[])(object)new VertexElement[3]
	{
		new VertexElement(0, (VertexElementFormat)1, (VertexElementUsage)0, 0),
		new VertexElement(8, (VertexElementFormat)4, (VertexElementUsage)1, 0),
		new VertexElement(12, (VertexElementFormat)2, (VertexElementUsage)2, 0)
	});

	public Vector2 Position;

	public Color Color;

	public Vector3 TexCoord;

	public VertexDeclaration VertexDeclaration => _vertexDeclaration;

	public VertexInfo2(Vector2 position, Vector3 texCoord, Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
		TexCoord = texCoord;
		Color = color;
	}
}
