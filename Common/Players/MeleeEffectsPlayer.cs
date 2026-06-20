using Terraria;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class MeleeEffectsPlayer : ModPlayer
{
	public int ScreenShakeTimer;

	public override void ModifyScreenPosition()
	{
		if (Player.whoAmI != Main.myPlayer || ScreenShakeTimer <= 0)
		{
			return;
		}

		Main.screenPosition += Main.rand.NextVector2Circular(15f, 15f);
		ScreenShakeTimer--;
	}
}
