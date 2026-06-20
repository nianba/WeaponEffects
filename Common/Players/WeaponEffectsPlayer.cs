using Terraria;
using Terraria.ModLoader;

namespace WeaponEffects;

public class WeaponEffectsPlayer : ModPlayer
{
	public int ScreenShakeTimer;
	public int SlashComboStepIndex;
	private int _slashComboResetTimer;

	public int ConsumeNextSlashComboStep()
	{
		int index = SlashComboStepIndex;
		SlashComboStepIndex = (SlashComboStepIndex + 1) % Compact3DComboSchemeA.Count;
		_slashComboResetTimer = ModContent.GetInstance<WeaponEffectsGameplayConfig>().ComboResetDelay;
		return index;
	}

	public override void ModifyScreenPosition()
	{
		if (Player.whoAmI != Main.myPlayer || ScreenShakeTimer <= 0)
		{
			return;
		}

		float strength = 15f * MeleeEffectAssets.ScreenShakeStrengthMultiplier;
		if (strength > 0f)
		{
			Main.screenPosition += Main.rand.NextVector2Circular(strength, strength);
		}

		ScreenShakeTimer--;
	}

	public override void PostUpdate()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			SlashGlobalItem.TryStartChargeInterrupt(Player);
		}

		if (_slashComboResetTimer <= 0)
		{
			SlashComboStepIndex = 0;
			return;
		}

		_slashComboResetTimer--;
	}
}
