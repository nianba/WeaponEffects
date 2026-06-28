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
		float visibleTipDistanceMultiplier = 1f)
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
			visibleTipDistanceMultiplier);
	}
}
