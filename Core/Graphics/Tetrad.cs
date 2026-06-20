using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class Tetrad
{
	public static void PlaySound(in SoundStyle style, Vector2? position = null)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		SoundEngine.PlaySound(style, position);
	}

	public static Texture2D GetTextureMySelf(string path)
	{
		return ModContent.Request<Texture2D>(path, (AssetRequestMode)2).Value;
	}

	public static int ShootProjectile(Vector2 position, Vector2 velocity, int ID, int dmg, float knb, int owner = 0, float ai0 = 0f, float ai1 = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Projectile.NewProjectile((IEntitySource)null, position, velocity, ID, dmg, knb, owner, ai0, ai1, 0f);
	}

	public static Projectile ShootProjectileDir(Vector2 position, Vector2 velocity, int ID, int dmg, float knb, int owner = 0, float ai0 = 0f, float ai1 = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Projectile.NewProjectileDirect((IEntitySource)null, position, velocity, ID, dmg, knb, owner, ai0, ai1, 0f);
	}
}
