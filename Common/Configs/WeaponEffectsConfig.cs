using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WeaponEffects;

internal class WeaponEffectsGameplayConfig : ModConfig
{
	[DefaultValue(true)]
	public bool EnableSlashRework = true;

	[Increment(0.01f)]
	[Range(0, 1)]
	[DefaultValue(0.5f)]
	[SliderColor(255, 0, 127, 255)]
	[Slider]
	public float SlashScale = 0.5f;

	[Increment(0.05f)]
	[Range(0.1f, 3f)]
	[DefaultValue(1f)]
	[SliderColor(255, 96, 96, 255)]
	[Slider]
	public float NormalSlashDamageMultiplier = 1f;

	[Increment(0.05f)]
	[Range(0.25f, 3f)]
	[DefaultValue(1f)]
	[SliderColor(96, 180, 255, 255)]
	[Slider]
	public float NormalSlashIntervalMultiplier = 1f;

	[Increment(0.05f)]
	[Range(0f, 3f)]
	[DefaultValue(1f)]
	[SliderColor(180, 255, 96, 255)]
	[Slider]
	public float SlashKnockbackMultiplier = 1f;

	[Increment(1)]
	[Range(1, 5)]
	[DefaultValue(5)]
	[SliderColor(255, 244, 1, 255)]
	[Slider]
	public int ChargeDamage = 5;

	[Increment(1)]
	[Range(1, 5)]
	[DefaultValue(1)]
	[Slider]
	public int ChargeMinDurationMultiplier = 1;

	[Increment(1)]
	[Range(1, 10)]
	[DefaultValue(5)]
	[Slider]
	public int ChargeMaxDurationMultiplier = 5;

	[Increment(0.1f)]
	[Range(1f, 4f)]
	[DefaultValue(4f)]
	[SliderColor(255, 160, 60, 255)]
	[Slider]
	public float ChargeLengthScale = 4f;

	[DefaultValue(true)]
	public bool SlashCanKillProjectiles = true;

	[DefaultValue(true)]
	public bool CanCharge = true;

	[DefaultValue(true)]
	public bool EmitVanillaSwordProjectiles = true;

	[Increment(1)]
	[Range(1, 180)]
	[DefaultValue(120)]
	[Slider]
	public int ComboResetDelay = 120;

	public override ConfigScope Mode => ConfigScope.ServerSide;
}

internal class WeaponEffectsVisualConfig : ModConfig
{
	[Increment(0.01f)]
	[Range(0, 1)]
	[DefaultValue(0.5f)]
	[SliderColor(0, 244, 127, 255)]
	[Slider]
	public float SlashBlink = 0.5f;

	[Increment(1)]
	[Range(1, 2)]
	[DefaultValue(1)]
	[Slider]
	public int SlashStyle = 1;

	[DefaultValue(false)]
	public bool ShowChargeBar = false;

	[DefaultValue(true)]
	public bool DrawHeldWeapon = true;

	[DefaultValue(true)]
	public bool DrawSpearTipTrail = true;

	[DefaultValue(true)]
	public bool DrawSpearShaftTrail = true;

	[DefaultValue(true)]
	public bool DrawSpearSweepArc = true;

	[DefaultValue(true)]
	public bool DrawSpearHeldWeapon = true;

	[DefaultValue(true)]
	public bool DrawSpearHitFlash = true;

	[Increment(0.05f)]
	[Range(0f, 3f)]
	[DefaultValue(1f)]
	[SliderColor(255, 190, 80, 255)]
	[Slider]
	public float ParticleDensity = 1f;

	[Increment(0.05f)]
	[Range(0f, 2f)]
	[DefaultValue(1f)]
	[SliderColor(160, 120, 255, 255)]
	[Slider]
	public float ScreenShakeStrength = 1f;

	[Increment(0.05f)]
	[Range(0f, 2f)]
	[DefaultValue(1f)]
	[SliderColor(120, 220, 255, 255)]
	[Slider]
	public float EffectVolume = 1f;

	public override ConfigScope Mode => ConfigScope.ClientSide;
}
