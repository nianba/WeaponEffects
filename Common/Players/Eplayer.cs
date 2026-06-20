using Terraria;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class Eplayer : ModPlayer
{
	public int screentimer = -1;

	public override void ModifyScreenPosition()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (screentimer > 0)
		{
			Main.screenPosition += Utils.NextVector2Circular(Main.rand, 15f, 15f);
			screentimer--;
		}
	}
}
