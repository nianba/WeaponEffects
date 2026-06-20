using Terraria;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class MeleeEffectsPlayer : ModPlayer
{
	private const int ComboResetDelay = 45;

	public int ScreenShakeTimer;
	public int SlashComboStepIndex;
	private int _slashComboResetTimer;

	public int ConsumeNextSlashComboStep()
	{
		int index = SlashComboStepIndex;
		SlashComboStepIndex = (SlashComboStepIndex + 1) % Compact3DComboSchemeA.Count;
		_slashComboResetTimer = ComboResetDelay;
		return index;
	}

	public override void ModifyScreenPosition()
	{
		if (Player.whoAmI != Main.myPlayer || ScreenShakeTimer <= 0)
		{
			return;
		}

		Main.screenPosition += Main.rand.NextVector2Circular(15f, 15f);
		ScreenShakeTimer--;
	}

	public override void PostUpdate()
	{
		if (_slashComboResetTimer <= 0)
		{
			SlashComboStepIndex = 0;
			return;
		}

		_slashComboResetTimer--;
	}
}
