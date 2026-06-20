using Microsoft.Xna.Framework;
using Terraria.ID;

namespace MeleeWeaponEffects;

public static class SlashProfiles
{
	private const int DustSoftStar = 278;
	private const int DustGrassLeaf = 107;
	private const int DustFireEmber = 6;
	private const int DustIceShard = 135;
	private const int DustMetalSpark = 15;

	public static WeaponSlashProfile BalancedSword => new(
		SlashProfileId.BalancedSword,
		new Color(235, 238, 242),
		new SlashParticleProfile(
			DustMetalSpark,
			new Color(255, 226, 160),
			count: 4,
			minScale: 0.45f,
			maxScale: 0.78f,
			velocityScale: 1f,
			spreadRadians: 0.35f,
			alternateDustColor: new Color(214, 220, 230),
			noGravity: true),
		new SlashParticleProfile(
			DustMetalSpark,
			new Color(255, 236, 190),
			count: 10,
			minScale: 0.52f,
			maxScale: 0.9f,
			velocityScale: 1.15f,
			spreadRadians: 0.5f,
			alternateDustColor: new Color(214, 220, 230),
			noGravity: true),
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
			spreadRadians: 0.45f,
			alternateDustColor: new Color(255, 190, 58),
			noGravity: true),
		new SlashParticleProfile(
			DustID.Torch,
			new Color(255, 170, 64),
			count: 34,
			minScale: 1.35f,
			maxScale: 2.35f,
			velocityScale: 1.75f,
			spreadRadians: 0.95f,
			alternateDustColor: new Color(255, 104, 28),
			noGravity: true),
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
			DustSoftStar,
			new Color(112, 54, 188),
			count: 7,
			minScale: 0.5f,
			maxScale: 0.82f,
			velocityScale: 1.15f,
			spreadRadians: 0.42f,
			alternateDustColor: new Color(186, 60, 224),
			noGravity: true),
		new SlashParticleProfile(
			DustSoftStar,
			new Color(150, 60, 230),
			count: 20,
			minScale: 0.62f,
			maxScale: 1.05f,
			velocityScale: 1.45f,
			spreadRadians: 0.72f,
			alternateDustColor: new Color(56, 38, 108),
			noGravity: true),
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
			DustSoftStar,
			new Color(154, 70, 238),
			count: 9,
			minScale: 0.58f,
			maxScale: 0.95f,
			velocityScale: 1.25f,
			spreadRadians: 0.46f,
			alternateDustColor: new Color(224, 82, 255),
			noGravity: true),
		new SlashParticleProfile(
			DustSoftStar,
			new Color(205, 92, 255),
			count: 26,
			minScale: 0.72f,
			maxScale: 1.2f,
			velocityScale: 1.6f,
			spreadRadians: 0.82f,
			alternateDustColor: new Color(72, 42, 132),
			noGravity: true),
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
			DustMetalSpark,
			new Color(255, 222, 126),
			count: 7,
			minScale: 0.52f,
			maxScale: 0.88f,
			velocityScale: 1.1f,
			spreadRadians: 0.38f,
			alternateDustColor: new Color(255, 250, 212),
			noGravity: true),
		new SlashParticleProfile(
			DustMetalSpark,
			new Color(255, 244, 180),
			count: 22,
			minScale: 0.62f,
			maxScale: 1.08f,
			velocityScale: 1.45f,
			spreadRadians: 0.68f,
			alternateDustColor: new Color(255, 210, 80),
			noGravity: true),
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
			DustMetalSpark,
			new Color(255, 234, 142),
			count: 9,
			minScale: 0.58f,
			maxScale: 0.98f,
			velocityScale: 1.2f,
			spreadRadians: 0.42f,
			alternateDustColor: new Color(255, 250, 212),
			noGravity: true),
		new SlashParticleProfile(
			DustMetalSpark,
			new Color(255, 252, 208),
			count: 28,
			minScale: 0.7f,
			maxScale: 1.18f,
			velocityScale: 1.6f,
			spreadRadians: 0.78f,
			alternateDustColor: new Color(255, 218, 82),
			noGravity: true),
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
			DustGrassLeaf,
			new Color(90, 190, 64),
			count: 7,
			minScale: 0.62f,
			maxScale: 1.04f,
			velocityScale: 0.95f,
			spreadRadians: 0.4f,
			alternateDustColor: new Color(186, 234, 96),
			noGravity: true),
		new SlashParticleProfile(
			DustGrassLeaf,
			new Color(120, 222, 72),
			count: 20,
			minScale: 0.7f,
			maxScale: 1.15f,
			velocityScale: 1.25f,
			spreadRadians: 0.72f,
			alternateDustColor: new Color(210, 244, 118),
			noGravity: true),
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
			DustIceShard,
			new Color(102, 210, 255),
			count: 6,
			minScale: 0.44f,
			maxScale: 0.74f,
			velocityScale: 1.1f,
			spreadRadians: 0.32f,
			alternateDustColor: new Color(80, 160, 255),
			noGravity: true),
		new SlashParticleProfile(
			DustIceShard,
			new Color(150, 232, 255),
			count: 14,
			minScale: 0.54f,
			maxScale: 0.9f,
			velocityScale: 1.35f,
			spreadRadians: 0.58f,
			alternateDustColor: new Color(210, 250, 255),
			noGravity: true),
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
			DustIceShard,
			new Color(126, 216, 255),
			count: 6,
			minScale: 0.48f,
			maxScale: 0.82f,
			velocityScale: 1.05f,
			spreadRadians: 0.36f,
			alternateDustColor: new Color(185, 240, 255),
			noGravity: true),
		new SlashParticleProfile(
			DustIceShard,
			new Color(188, 244, 255),
			count: 16,
			minScale: 0.58f,
			maxScale: 0.98f,
			velocityScale: 1.35f,
			spreadRadians: 0.64f,
			alternateDustColor: new Color(152, 230, 255),
			noGravity: true),
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
			DustIceShard,
			new Color(144, 226, 255),
			count: 8,
			minScale: 0.52f,
			maxScale: 0.88f,
			velocityScale: 1.15f,
			spreadRadians: 0.4f,
			alternateDustColor: new Color(215, 250, 255),
			noGravity: true),
		new SlashParticleProfile(
			DustIceShard,
			new Color(210, 250, 255),
			count: 22,
			minScale: 0.66f,
			maxScale: 1.08f,
			velocityScale: 1.5f,
			spreadRadians: 0.72f,
			alternateDustColor: new Color(152, 230, 255),
			noGravity: true),
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
			DustSoftStar,
			new Color(255, 98, 206),
			count: 5,
			minScale: 0.58f,
			maxScale: 0.96f,
			velocityScale: 0.85f,
			spreadRadians: 0.32f,
			alternateDustColor: new Color(255, 218, 82),
			noGravity: true),
		new SlashParticleProfile(
			DustSoftStar,
			new Color(255, 98, 206),
			count: 14,
			minScale: 0.72f,
			maxScale: 1.18f,
			velocityScale: 1.15f,
			spreadRadians: 0.68f,
			alternateDustColor: new Color(255, 218, 82),
			noGravity: true),
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
			DustFireEmber,
			new Color(255, 78, 48),
			count: 7,
			minScale: 0.62f,
			maxScale: 1.08f,
			velocityScale: 1.15f,
			spreadRadians: 0.42f,
			alternateDustColor: new Color(255, 184, 48),
			noGravity: true),
		new SlashParticleProfile(
			DustID.Blood,
			new Color(156, 18, 40),
			count: 16,
			minScale: 0.62f,
			maxScale: 1.08f,
			velocityScale: 1.35f,
			spreadRadians: 0.68f,
			alternateDustColor: new Color(255, 184, 48)),
		new SlashShapeProfile(
			lengthScale: 1.05f,
			thicknessScale: 1.05f,
			minYScale: 0.45f,
			maxYScale: 0.65f,
			angleRandomness: 0.3f,
			extraUpdates: 5));
}
