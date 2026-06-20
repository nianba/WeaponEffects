using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public static class SlashProfiles
{
	public static WeaponSlashProfile BalancedSword => new(
		SlashProfileId.BalancedSword,
		new Color(235, 238, 242),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 226, 160),
			count: 4,
			minScale: 0.8f,
			maxScale: 1.15f,
			velocityScale: 1f,
			spreadRadians: 0.35f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 236, 190),
			count: 10,
			minScale: 0.9f,
			maxScale: 1.25f,
			velocityScale: 1.15f,
			spreadRadians: 0.5f),
		new SlashShapeProfile(
			lengthScale: 1f,
			thicknessScale: 1f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile Volcano => new(
		SlashProfileId.Volcano,
		new Color(255, 112, 36),
		new SlashParticleProfile(
			DustID.Torch,
			new Color(255, 92, 24),
			count: 8,
			minScale: 1.1f,
			maxScale: 1.65f,
			velocityScale: 1.25f,
			spreadRadians: 0.45f),
		new SlashParticleProfile(
			DustID.Torch,
			new Color(255, 150, 54),
			count: 22,
			minScale: 1.2f,
			maxScale: 1.9f,
			velocityScale: 1.45f,
			spreadRadians: 0.75f),
		new SlashShapeProfile(
			lengthScale: 1.18f,
			thicknessScale: 1.3f,
			minYScale: 0.75f,
			maxYScale: 0.9f,
			angleRandomness: 0.34f,
			extraUpdates: 4));
}

