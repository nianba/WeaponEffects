using Microsoft.Xna.Framework;
using Terraria.ID;

namespace WeaponEffects.Spears;

public static class SpearHeldVisualProfileResolver
{
	private static readonly SpearHeldVisualProfile DefaultProfile = DiagonalProfile(
		gripAlongShaft: 0.34f,
		maxDrawScale: 2.25f,
		drawScaleMultiplier: 1.05f);

	public static SpearHeldVisualProfile Resolve(int weaponItemType)
	{
		return Resolve(weaponItemType, allowTextureOverride: true);
	}

	public static SpearHeldVisualProfile ResolveVanillaFallback(int weaponItemType)
	{
		return Resolve(weaponItemType, allowTextureOverride: false);
	}

	private static SpearHeldVisualProfile Resolve(int weaponItemType, bool allowTextureOverride)
	{
		if (SpearItemIdResolver.Matches(weaponItemType, SpearItemIdResolver.DreadOfTheRedSea))
		{
			return DiagonalProfile(0.36f, maxDrawScale: 2.2f, drawScaleMultiplier: 1.04f);
		}

		if (SpearItemIdResolver.Matches(weaponItemType, SpearItemIdResolver.SlimeSpear))
		{
			return DiagonalProfile(0.32f, maxDrawScale: 2.15f, drawScaleMultiplier: 1.04f);
		}

		return weaponItemType switch
		{
			ItemID.Trident when allowTextureOverride => DiagonalProfile(
				0.36f,
				minDrawScale: 0.8f,
				maxDrawScale: 1.25f,
				drawScaleMultiplier: 1f,
				visibleTipDistanceMultiplier: 1.3f,
				textureOverride: ProjectileTexture(ProjectileID.Trident, 70f, 70f)),
			ItemID.Spear when allowTextureOverride => ProjectileProfile(ProjectileID.Spear, 54f, 54f),
			ItemID.ThunderSpear when allowTextureOverride => ProjectileProfile(ProjectileID.ThunderSpear, 70f, 70f),
			ItemID.TheRottedFork when allowTextureOverride => ProjectileProfile(ProjectileID.TheRottedFork, 84f, 84f),
			ItemID.DarkLance when allowTextureOverride => ProjectileProfile(ProjectileID.DarkLance, 100f, 100f),
			ItemID.CobaltNaginata when allowTextureOverride => ProjectileProfile(ProjectileID.CobaltNaginata, 100f, 100f),
			ItemID.PalladiumPike when allowTextureOverride => ProjectileProfile(ProjectileID.PalladiumPike, 100f, 100f),
			ItemID.MythrilHalberd when allowTextureOverride => ProjectileProfile(ProjectileID.MythrilHalberd, 100f, 100f),
			ItemID.OrichalcumHalberd when allowTextureOverride => ProjectileProfile(ProjectileID.OrichalcumHalberd, 100f, 100f),
			ItemID.AdamantiteGlaive when allowTextureOverride => ProjectileProfile(ProjectileID.AdamantiteGlaive, 110f, 110f),
			ItemID.TitaniumTrident when allowTextureOverride => ProjectileProfile(ProjectileID.TitaniumTrident, 110f, 110f),
			ItemID.Gungnir when allowTextureOverride => ProjectileProfile(ProjectileID.Gungnir, 116f, 116f),
			ItemID.ChlorophytePartisan when allowTextureOverride => ProjectileProfile(ProjectileID.ChlorophytePartisan, 116f, 116f),
			ItemID.MushroomSpear when allowTextureOverride => ProjectileProfile(ProjectileID.MushroomSpear, 100f, 100f),
			ItemID.Swordfish when allowTextureOverride => ProjectileProfile(ProjectileID.Swordfish, 64f, 64f),
			ItemID.ObsidianSwordfish when allowTextureOverride => ProjectileProfile(ProjectileID.ObsidianSwordfish, 82f, 82f),
			ItemID.NorthPole when allowTextureOverride => ProjectileProfile(ProjectileID.NorthPoleWeapon, 116f, 116f),
			ItemID.Swordfish => DiagonalProfile(0.28f, tipAlongShaft: 0.94f, maxDrawScale: 2.05f, drawScaleMultiplier: 1.02f),
			ItemID.ObsidianSwordfish => DiagonalProfile(0.28f, tipAlongShaft: 0.94f, maxDrawScale: 2.05f, drawScaleMultiplier: 1.02f),
			ItemID.NorthPole => DiagonalProfile(0.38f, maxDrawScale: 2.3f, drawScaleMultiplier: 1.05f),
			_ => DefaultProfile,
		};
	}

	private static SpearHeldVisualProfile DiagonalProfile(
		float gripAlongShaft,
		float tipAlongShaft = 1f,
		float minDrawScale = 1f,
		float maxDrawScale = 2.25f,
		float drawScaleMultiplier = 1.05f,
		float visibleTipDistanceMultiplier = 1f,
		SpearHeldTextureOverride textureOverride = default)
	{
		Vector2 butt = new(0.04f, 0.96f);
		Vector2 tip = new(0.97f, 0.03f);
		Vector2 gripOrigin = Vector2.Lerp(butt, tip, gripAlongShaft);
		Vector2 tipOrigin = Vector2.Lerp(butt, tip, tipAlongShaft);

		return new SpearHeldVisualProfile(
			gripOrigin,
			tipOrigin,
			minDrawScale,
			maxDrawScale,
			drawScaleMultiplier,
			visibleTipDistanceMultiplier,
			textureOverride);
	}

	private static SpearHeldVisualProfile ProjectileProfile(int projectileType, float textureWidth, float textureHeight)
	{
		return DiagonalProfile(
			0.36f,
			minDrawScale: 0.8f,
			maxDrawScale: 1.25f,
			drawScaleMultiplier: 1f,
			visibleTipDistanceMultiplier: 1.3f,
			textureOverride: ProjectileTexture(projectileType, textureWidth, textureHeight));
	}

	private static SpearHeldTextureOverride ProjectileTexture(int projectileType, float textureWidth, float textureHeight)
	{
		return new SpearHeldTextureOverride(
			projectileType,
			new Vector2(textureWidth * 0.53f, textureHeight * 0.53f),
			new Vector2(textureWidth * 0.12f, textureHeight * 0.12f));
	}
}
