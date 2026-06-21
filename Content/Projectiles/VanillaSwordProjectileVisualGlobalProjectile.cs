using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class VanillaSwordProjectileVisualGlobalProjectile : GlobalProjectile
{
	private bool _hideTrueNightsEdgeOpening;
	private int _hiddenOpeningTicks;

	public override bool InstancePerEntity => true;

	public void HideTrueNightsEdgeOpening(int ticks)
	{
		_hideTrueNightsEdgeOpening = true;
		_hiddenOpeningTicks = ticks < 0 ? 0 : ticks;
	}

	public override bool PreDraw(Projectile projectile, ref Color lightColor)
	{
		if (_hideTrueNightsEdgeOpening &&
			projectile.type == ProjectileID.TrueNightsEdge &&
			projectile.localAI[0] < _hiddenOpeningTicks)
		{
			return false;
		}

		return true;
	}
}
