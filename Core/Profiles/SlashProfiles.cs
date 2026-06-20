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
			new Color(255, 170, 64),
			count: 34,
			minScale: 1.35f,
			maxScale: 2.35f,
			velocityScale: 1.75f,
			spreadRadians: 0.95f),
		new SlashShapeProfile(
			lengthScale: 1.18f,
			thicknessScale: 1.55f,
			minYScale: 0.75f,
			maxYScale: 0.9f,
			angleRandomness: 0.34f,
			extraUpdates: 4));

	public static WeaponSlashProfile NightsEdge => new(
		SlashProfileId.NightsEdge,
		new Color(118, 68, 178),
		new SlashParticleProfile(
			ModContent.DustType<DarkSpark>(),
			new Color(150, 76, 220),
			count: 7,
			minScale: 0.9f,
			maxScale: 1.35f,
			velocityScale: 1.15f,
			spreadRadians: 0.42f),
		new SlashParticleProfile(
			ModContent.DustType<DarkSpark>(),
			new Color(188, 92, 255),
			count: 20,
			minScale: 1.0f,
			maxScale: 1.85f,
			velocityScale: 1.45f,
			spreadRadians: 0.72f),
		new SlashShapeProfile(
			lengthScale: 1.08f,
			thicknessScale: 0.95f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile TrueNightsEdge => new(
		SlashProfileId.TrueNightsEdge,
		new Color(152, 82, 232),
		new SlashParticleProfile(
			ModContent.DustType<DarkSpark>(),
			new Color(185, 86, 255),
			count: 9,
			minScale: 1.0f,
			maxScale: 1.55f,
			velocityScale: 1.25f,
			spreadRadians: 0.46f),
		new SlashParticleProfile(
			ModContent.DustType<DarkSpark>(),
			new Color(225, 118, 255),
			count: 26,
			minScale: 1.15f,
			maxScale: 2.05f,
			velocityScale: 1.6f,
			spreadRadians: 0.82f),
		new SlashShapeProfile(
			lengthScale: 1.15f,
			thicknessScale: 1.05f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile Excalibur => new(
		SlashProfileId.Excalibur,
		new Color(255, 224, 118),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 222, 126),
			count: 7,
			minScale: 0.9f,
			maxScale: 1.4f,
			velocityScale: 1.1f,
			spreadRadians: 0.38f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 244, 180),
			count: 22,
			minScale: 1.05f,
			maxScale: 1.9f,
			velocityScale: 1.45f,
			spreadRadians: 0.68f),
		new SlashShapeProfile(
			lengthScale: 1.08f,
			thicknessScale: 1.18f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile TrueExcalibur => new(
		SlashProfileId.TrueExcalibur,
		new Color(255, 238, 148),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 234, 142),
			count: 9,
			minScale: 1.0f,
			maxScale: 1.55f,
			velocityScale: 1.2f,
			spreadRadians: 0.42f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 252, 208),
			count: 28,
			minScale: 1.15f,
			maxScale: 2.1f,
			velocityScale: 1.6f,
			spreadRadians: 0.78f),
		new SlashShapeProfile(
			lengthScale: 1.15f,
			thicknessScale: 1.25f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile BladeOfGrass => new(
		SlashProfileId.BladeOfGrass,
		new Color(118, 220, 70),
		new SlashParticleProfile(
			DustID.Grass,
			new Color(118, 224, 76),
			count: 7,
			minScale: 0.8f,
			maxScale: 1.25f,
			velocityScale: 0.95f,
			spreadRadians: 0.4f),
		new SlashParticleProfile(
			DustID.Grass,
			new Color(168, 244, 92),
			count: 20,
			minScale: 0.95f,
			maxScale: 1.7f,
			velocityScale: 1.25f,
			spreadRadians: 0.72f),
		new SlashShapeProfile(
			lengthScale: 1.08f,
			thicknessScale: 1.1f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile Muramasa => new(
		SlashProfileId.Muramasa,
		new Color(90, 190, 255),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(102, 210, 255),
			count: 6,
			minScale: 0.75f,
			maxScale: 1.15f,
			velocityScale: 1.1f,
			spreadRadians: 0.32f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(150, 232, 255),
			count: 14,
			minScale: 0.85f,
			maxScale: 1.45f,
			velocityScale: 1.35f,
			spreadRadians: 0.58f),
		new SlashShapeProfile(
			lengthScale: 1f,
			thicknessScale: 0.7f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile IceBlade => new(
		SlashProfileId.IceBlade,
		new Color(132, 220, 255),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(126, 216, 255),
			count: 6,
			minScale: 0.8f,
			maxScale: 1.2f,
			velocityScale: 1.05f,
			spreadRadians: 0.36f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(188, 244, 255),
			count: 16,
			minScale: 0.9f,
			maxScale: 1.55f,
			velocityScale: 1.35f,
			spreadRadians: 0.64f),
		new SlashShapeProfile(
			lengthScale: 1f,
			thicknessScale: 0.8f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile Frostbrand => new(
		SlashProfileId.Frostbrand,
		new Color(150, 232, 255),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(144, 226, 255),
			count: 8,
			minScale: 0.9f,
			maxScale: 1.4f,
			velocityScale: 1.15f,
			spreadRadians: 0.4f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(210, 250, 255),
			count: 22,
			minScale: 1.0f,
			maxScale: 1.85f,
			velocityScale: 1.5f,
			spreadRadians: 0.72f),
		new SlashShapeProfile(
			lengthScale: 1.12f,
			thicknessScale: 0.9f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile Starfury => new(
		SlashProfileId.Starfury,
		new Color(190, 220, 255),
		new SlashParticleProfile(
			ModContent.DustType<StarSpark>(),
			new Color(196, 226, 255),
			count: 5,
			minScale: 0.55f,
			maxScale: 0.9f,
			velocityScale: 0.85f,
			spreadRadians: 0.32f),
		new SlashParticleProfile(
			ModContent.DustType<StarSpark>(),
			new Color(255, 244, 190),
			count: 14,
			minScale: 0.7f,
			maxScale: 1.2f,
			velocityScale: 1.15f,
			spreadRadians: 0.68f),
		new SlashShapeProfile(
			lengthScale: 1.12f,
			thicknessScale: 0.85f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));

	public static WeaponSlashProfile Bladetongue => new(
		SlashProfileId.Bladetongue,
		new Color(255, 90, 44),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 78, 48),
			count: 7,
			minScale: 0.9f,
			maxScale: 1.35f,
			velocityScale: 1.15f,
			spreadRadians: 0.42f),
		new SlashParticleProfile(
			ModContent.DustType<CommonSpark>(),
			new Color(255, 184, 48),
			count: 16,
			minScale: 1.0f,
			maxScale: 1.75f,
			velocityScale: 1.35f,
			spreadRadians: 0.68f),
		new SlashShapeProfile(
			lengthScale: 1.05f,
			thicknessScale: 1.05f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));
}
