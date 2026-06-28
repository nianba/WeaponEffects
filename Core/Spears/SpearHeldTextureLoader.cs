using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace WeaponEffects.Spears;

public static class SpearHeldTextureLoader
{
	public static bool TryGetProjectileTexture(SpearHeldTextureOverride textureOverride, out Texture2D texture)
	{
		texture = null;

		int projectileType = textureOverride.ProjectileType;
		if (!textureOverride.HasSource
			|| projectileType <= 0
			|| projectileType >= TextureAssets.Projectile.Length)
		{
			return false;
		}

		Main.instance.LoadProjectile(projectileType);
		texture = TextureAssets.Projectile[projectileType].Value;
		return texture != null && texture.Width > 1 && texture.Height > 1;
	}
}
