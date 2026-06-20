using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MeleeWeaponEffects;

internal class MeleeWeaponEffectsGameplayConfig : ModConfig
{
	[Increment(0.01f)]
	[Range(0, 1)]
	[DefaultValue(0.5f)]
	[SliderColor(255, 0, 127, 255)]
	[Slider]
	public float SlashScale = 0.5f;

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

	public override ConfigScope Mode => ConfigScope.ServerSide;
}

internal class MeleeWeaponEffectsVisualConfig : ModConfig
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

	public override ConfigScope Mode => ConfigScope.ClientSide;
}
