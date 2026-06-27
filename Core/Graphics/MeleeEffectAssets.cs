using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public static class MeleeEffectAssets
{
	public const string TextureRoot = "WeaponEffects/Assets/Textures";
	public const string ChargeBar = TextureRoot + "/Bar";
	public const string ChargeBarFill = TextureRoot + "/BarInside";
	public const string SlashTexture = TextureRoot + "/SlashTex";
	public const string SlashGlowTexture = TextureRoot + "/EX112";
	public const string SpearTipTrailTexture = TextureRoot + "/SpearTipTrail";
	public const string SpearSweepColorTexture = TextureRoot + "/SpearSweepColor";

	public static Texture2D GetTexture(string path)
	{
		return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
	}

	public static float ParticleDensityMultiplier => MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsVisualConfig>().ParticleDensity, 0f, 3f);

	public static float ScreenShakeStrengthMultiplier => MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsVisualConfig>().ScreenShakeStrength, 0f, 2f);

	public static float EffectVolumeMultiplier => MathHelper.Clamp(ModContent.GetInstance<WeaponEffectsVisualConfig>().EffectVolume, 0f, 2f);

	public static int ScaleParticleCount(int baseCount, float strength = 1f)
	{
		float scaledCount = MathHelper.Max(0f, baseCount * strength * ParticleDensityMultiplier);
		return (int)System.MathF.Round(scaledCount);
	}

	public static void PlaySound(in SoundStyle style, Vector2? position = null)
	{
		float volumeMultiplier = EffectVolumeMultiplier;
		if (volumeMultiplier <= 0f)
		{
			return;
		}

		SoundStyle scaledStyle = style;
		scaledStyle.Volume *= volumeMultiplier;
		SoundEngine.PlaySound(scaledStyle, position);
	}

	public static int NewProjectile(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack, int owner = 0, float ai0 = 0f, float ai1 = 0f)
	{
		return Projectile.NewProjectile(source, position, velocity, type, damage, knockBack, owner, ai0, ai1);
	}

	public static Projectile NewProjectileDirect(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack, int owner = 0, float ai0 = 0f, float ai1 = 0f)
	{
		return Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockBack, owner, ai0, ai1);
	}

	public static void SyncProjectile(Projectile projectile)
	{
		if (Main.netMode == NetmodeID.SinglePlayer || projectile == null || !projectile.active)
		{
			return;
		}

		NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
	}
}
