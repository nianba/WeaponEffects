using Microsoft.Xna.Framework;

namespace WeaponEffects.Spears;

public readonly struct SpearHeldVisualProfile
{
	public SpearHeldVisualProfile(
		Vector2 gripOriginFactor,
		Vector2 tipOriginFactor,
		float minDrawScale,
		float maxDrawScale,
		float drawScaleMultiplier,
		float visibleTipDistanceMultiplier = 1f,
		SpearHeldTextureOverride textureOverride = default)
	{
		GripOriginFactor = gripOriginFactor;
		TipOriginFactor = tipOriginFactor;
		MinDrawScale = minDrawScale;
		MaxDrawScale = maxDrawScale;
		DrawScaleMultiplier = drawScaleMultiplier;
		VisibleTipDistanceMultiplier = visibleTipDistanceMultiplier;
		TextureOverride = textureOverride;
	}

	public Vector2 GripOriginFactor { get; }

	public Vector2 TipOriginFactor { get; }

	public float MinDrawScale { get; }

	public float MaxDrawScale { get; }

	public float DrawScaleMultiplier { get; }

	public float VisibleTipDistanceMultiplier { get; }

	public SpearHeldTextureOverride TextureOverride { get; }
}
