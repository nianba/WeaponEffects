using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MeleeWeaponEffects;

internal class Mconfig : ModConfig
{
	[Increment(0.01f)]
	[Range(0, 1)]
	[Tooltip("Slash Scale (0~1):This config can modify the effect's width")]
	[DefaultValue(0.5f)]
	[SliderColor(255, 0, 127, 255)]
	[Slider]
	public float SlashScale;

	[Increment(0.01f)]
	[Range(0, 1)]
	[Tooltip("Slash Blink (0~1):This config can modify the effect's edge's glow")]
	[DefaultValue(0.5f)]
	[SliderColor(0, 244, 127, 255)]
	[Slider]
	public float SlashBlink;

	[Increment(1)]
	[Range(2, 10)]
	[Tooltip("Charge (2~10 times):This config can modify the charged damage")]
	[DefaultValue(5)]
	[SliderColor(255, 244, 1, 255)]
	[Slider]
	public int ChargeDamage;

	[DefaultValue(true)]
	[Tooltip("Determine whether your slash can destroy hostile projectiles")]
	public bool SlashCanKillProjectiles;

	[DefaultValue(true)]
	[Tooltip("Determine whether the function that 'Right click to charge' is enable.")]
	public bool CanCharge;

	[Range(1, 2)]
	[DefaultValue(1)]
	public int Style;

	public override ConfigScope Mode => (ConfigScope)1;
}
