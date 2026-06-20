using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public static class MeleeEffectAssets
{
	public const string TextureRoot = "MeleeWeaponEffects/Assets/Textures";
	public const string ChargeBar = TextureRoot + "/Bar";
	public const string ChargeBarFill = TextureRoot + "/BarInside";
	public const string SlashTexture = TextureRoot + "/SlashTex";
	public const string SlashGlowTexture = TextureRoot + "/EX112";

	public static Texture2D GetTexture(string path)
	{
		return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
	}

	public static void PlaySound(in SoundStyle style, Vector2? position = null)
	{
		SoundEngine.PlaySound(style, position);
	}

	public static int NewProjectile(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack, int owner = 0, float ai0 = 0f, float ai1 = 0f)
	{
		return Projectile.NewProjectile(source, position, velocity, type, damage, knockBack, owner, ai0, ai1);
	}

	public static Projectile NewProjectileDirect(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack, int owner = 0, float ai0 = 0f, float ai1 = 0f)
	{
		return Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockBack, owner, ai0, ai1);
	}
}
