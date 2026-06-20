using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class SlashArcProjectile : ModProjectile
{
	private readonly SlashVertex[] _vertices = new SlashVertex[80];
	private readonly Effect _slashEffect = ModContent.Request<Effect>("MeleeWeaponEffects/Effects/Mhd", AssetRequestMode.ImmediateLoad).Value;
	private int _vertexCount;

	private bool _reverse;
	private bool _npcOwned;
	private Color _color = Color.White;
	private int _weaponItemType;
	private int _nextSlashCount;
	private float _targetRotation;
	private bool _usesVisualProfile;
	private int _profileAge;
	private int _profileTrailCount;
	private float _profileXScale = 1f;
	private float _profileStartDepth;
	private float _profileHitDepth;
	private float _profileEndDepth;
	private float _profileHitProgress = 0.5f;
	private float _profileMainAlpha = 1f;
	private float _profileCoreAlpha;
	private float _profileFarRimAlpha;
	private float _profileNearEdgeAlpha;
	private float _profilePeakFlareAlpha;
	private float _profileNearEdgeOffsetPixels;
	private float _profileFarRimOffsetPixels;
	private float _profileTrailDelayRadians;

	public override string Texture => "Terraria/Images/Extra_193";

	public static void CreateSlash(
		bool isPlayerOwned,
		IEntitySource source,
		float rotation,
		float startingRotation,
		float length,
		float thickness,
		float yScale,
		int extraUpdates = 0,
		int damage = 50,
		float knockback = 0f,
		int owner = 0,
		int ownerNPC = 0,
		Color color = default,
		int weaponItemType = 0,
		int nextSlashCount = 0,
		float knockbackRotation = 0f,
		float weaponScale = 1f)
	{
		float configuredThickness = ModContent.GetInstance<MeleeWeaponEffectsGameplayConfig>().SlashScale * Math.Max(0.1f, thickness);
		Vector2 velocity = knockbackRotation.ToRotationVector2() * length;
		Vector2 position = isPlayerOwned ? Main.player[owner].Center : Main.npc[ownerNPC].Center;
		int projectileOwner = isPlayerOwned ? owner : Main.myPlayer;
		float ai0 = isPlayerOwned ? 0f : ownerNPC;

		Projectile slash = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<SlashArcProjectile>(), damage, knockback, projectileOwner, ai0, rotation);
		InitializeSlash(slash, startingRotation, configuredThickness, yScale, extraUpdates, !isPlayerOwned, color, weaponItemType, nextSlashCount, knockbackRotation);

		Projectile glow = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<SlashArcGlowProjectile>(), damage, knockback, projectileOwner, ai0, rotation);
		SlashArcGlowProjectile.InitializeGlow(glow, startingRotation, configuredThickness, yScale, extraUpdates, !isPlayerOwned, color);
	}

	public static void CreateProfiledSlash(
		bool isPlayerOwned,
		IEntitySource source,
		float rotation,
		float startingRotation,
		float length,
		float thicknessScale,
		float yScale,
		int extraUpdates,
		int damage,
		float knockback,
		int owner,
		int ownerNPC,
		int weaponItemType,
		float knockbackRotation,
		in SlashArcVisualProfile visual,
		float hitProgress)
	{
		float configuredThickness = ModContent.GetInstance<MeleeWeaponEffectsGameplayConfig>().SlashScale * Math.Max(0.1f, thicknessScale);
		Vector2 velocity = knockbackRotation.ToRotationVector2() * length;
		Vector2 position = isPlayerOwned ? Main.player[owner].Center : Main.npc[ownerNPC].Center;
		int projectileOwner = isPlayerOwned ? owner : Main.myPlayer;
		float ai0 = isPlayerOwned ? 0f : ownerNPC;
		Color color = visual.Tint == default ? Color.White : visual.Tint;

		Projectile slash = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<SlashArcProjectile>(), damage, knockback, projectileOwner, ai0, rotation);
		InitializeSlash(slash, startingRotation, configuredThickness, yScale, extraUpdates, !isPlayerOwned, color, weaponItemType, nextSlashCount: 0, knockbackRotation);
		InitializeVisualProfile(slash, in visual, hitProgress);

		Projectile glow = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<SlashArcGlowProjectile>(), damage, knockback, projectileOwner, ai0, rotation);
		SlashArcGlowProjectile.InitializeProfiledGlow(glow, startingRotation, configuredThickness, yScale, extraUpdates, !isPlayerOwned, color, in visual, hitProgress);
	}

	private static void InitializeSlash(Projectile projectile, float startingRotation, float thickness, float yScale, int extraUpdates, bool npcOwned, Color color, int weaponItemType, int nextSlashCount, float targetRotation)
	{
		projectile.rotation = startingRotation;
		projectile.localAI[0] = thickness;
		projectile.localAI[1] = yScale;
		projectile.extraUpdates = extraUpdates;

		if (projectile.ModProjectile is SlashArcProjectile slash)
		{
			slash._reverse = startingRotation > 0f;
			slash._npcOwned = npcOwned;
			slash._color = color == default ? Color.White : color;
			slash._weaponItemType = weaponItemType;
			slash._nextSlashCount = nextSlashCount;
			slash._targetRotation = targetRotation;
			projectile.netUpdate = true;
		}
	}

	private static void InitializeVisualProfile(Projectile projectile, in SlashArcVisualProfile visual, float hitProgress)
	{
		if (projectile.ModProjectile is SlashArcProjectile slash)
		{
			slash._usesVisualProfile = true;
			slash._profileAge = 0;
			slash._profileXScale = MathHelper.Clamp(visual.XScale, 0.2f, 2f);
			slash._profileStartDepth = visual.StartDepth;
			slash._profileHitDepth = visual.HitDepth;
			slash._profileEndDepth = visual.EndDepth;
			slash._profileHitProgress = MathHelper.Clamp(hitProgress, 0.08f, 0.92f);
			slash._profileMainAlpha = MathHelper.Clamp(visual.MainAlpha, 0f, 1.5f);
			slash._profileCoreAlpha = MathHelper.Clamp(visual.CoreAlpha, 0f, 1.5f);
			slash._profileFarRimAlpha = MathHelper.Clamp(visual.FarRimAlpha, 0f, 1.5f);
			slash._profileNearEdgeAlpha = MathHelper.Clamp(visual.NearEdgeAlpha, 0f, 1.5f);
			slash._profilePeakFlareAlpha = MathHelper.Clamp(visual.PeakFlareAlpha, 0f, 1.5f);
			slash._profileNearEdgeOffsetPixels = MathHelper.Clamp(visual.NearEdgeOffsetPixels, 0f, 24f);
			slash._profileFarRimOffsetPixels = MathHelper.Clamp(visual.FarRimOffsetPixels, 0f, 24f);
			slash._profileTrailCount = Math.Min(Math.Max(visual.TrailCount, 0), 2);
			slash._profileTrailDelayRadians = MathHelper.ToRadians(MathHelper.Clamp(visual.TrailDelayDegrees, 0f, 30f));
			projectile.netUpdate = true;
		}
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.width = 24;
		Projectile.height = 24;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.timeLeft = 100;
		Projectile.tileCollide = false;
		Projectile.penetrate = -1;
		Projectile.ignoreWater = true;
		Projectile.aiStyle = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 1000;
		Projectile.noEnchantmentVisuals = true;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(_reverse);
		writer.Write(_npcOwned);
		writer.Write(_color.PackedValue);
		writer.Write(_weaponItemType);
		writer.Write(_nextSlashCount);
		writer.Write(_targetRotation);
		writer.Write(_usesVisualProfile);
		writer.Write(_profileAge);
		writer.Write(_profileTrailCount);
		writer.Write(_profileXScale);
		writer.Write(_profileStartDepth);
		writer.Write(_profileHitDepth);
		writer.Write(_profileEndDepth);
		writer.Write(_profileHitProgress);
		writer.Write(_profileMainAlpha);
		writer.Write(_profileCoreAlpha);
		writer.Write(_profileFarRimAlpha);
		writer.Write(_profileNearEdgeAlpha);
		writer.Write(_profilePeakFlareAlpha);
		writer.Write(_profileNearEdgeOffsetPixels);
		writer.Write(_profileFarRimOffsetPixels);
		writer.Write(_profileTrailDelayRadians);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		_reverse = reader.ReadBoolean();
		_npcOwned = reader.ReadBoolean();
		_color = new Color { PackedValue = reader.ReadUInt32() };
		_weaponItemType = reader.ReadInt32();
		_nextSlashCount = reader.ReadInt32();
		_targetRotation = reader.ReadSingle();
		_usesVisualProfile = reader.ReadBoolean();
		_profileAge = reader.ReadInt32();
		_profileTrailCount = reader.ReadInt32();
		_profileXScale = reader.ReadSingle();
		_profileStartDepth = reader.ReadSingle();
		_profileHitDepth = reader.ReadSingle();
		_profileEndDepth = reader.ReadSingle();
		_profileHitProgress = reader.ReadSingle();
		_profileMainAlpha = reader.ReadSingle();
		_profileCoreAlpha = reader.ReadSingle();
		_profileFarRimAlpha = reader.ReadSingle();
		_profileNearEdgeAlpha = reader.ReadSingle();
		_profilePeakFlareAlpha = reader.ReadSingle();
		_profileNearEdgeOffsetPixels = reader.ReadSingle();
		_profileFarRimOffsetPixels = reader.ReadSingle();
		_profileTrailDelayRadians = reader.ReadSingle();
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override void CutTiles()
	{
		Vector2 center = OwnerCenterWorld();
		Vector2 end = center + Projectile.velocity.Length() * Projectile.ai[1].ToRotationVector2();
		float width = Projectile.velocity.Length() * Projectile.localAI[1];
		DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
		Utils.PlotTileLine(center, end, width, DelegateMethods.CutTiles);
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		float _ = 0f;
		Vector2 center = OwnerCenterWorld();
		Vector2 end = center + Projectile.velocity.Length() * Projectile.ai[1].ToRotationVector2();
		return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), center, end, Projectile.velocity.Length() * Projectile.localAI[1], ref _);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Player player = Main.player[Projectile.owner];
		if (!_npcOwned && player.active)
		{
			ItemLoader.OnHitNPC(player.HeldItem, player, target, in hit, damageDone);
		}

		MeleeEffectAssets.NewProjectileDirect(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<SlashHitEffectProjectile>(), 0, 0f, Projectile.owner, Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));

		SoundStyle? targetHitSound = target.HitSound;
		if (targetHitSound.HasValue && targetHitSound.Value == SoundID.NPCHit4)
		{
			SoundStyle blockSound = new("MeleeWeaponEffects/Sounds/Block") { Volume = 0.25f };
			MeleeEffectAssets.PlaySound(in blockSound, target.Center);
			SpawnHitDust(target, DustID.Torch, 22, 1.2f, 3f);
		}
		else
		{
			SoundStyle hitSound = new("MeleeWeaponEffects/Sounds/Onhit")
			{
				Volume = 0.45f,
				Pitch = Main.rand.NextFloat(-0.15f, 0.15f)
			};
			MeleeEffectAssets.PlaySound(in hitSound, target.Center);
			SpawnHitDust(target, DustID.Blood, 32, 0.9f, 1.7f);
		}

		EmitExactProfileHitParticles(target);
		EmitOnHitProjectiles(target);
	}

	public override void AI()
	{
		if (_usesVisualProfile)
		{
			_profileAge++;
		}

		Vector2 aimVector = Projectile.rotation.ToRotationVector2();
		aimVector.Y *= Projectile.localAI[1];
		aimVector = aimVector.RotatedBy(Projectile.ai[1]);
		float itemRotation = aimVector.ToRotation();

		if (!_npcOwned)
		{
			Player player = Main.player[Projectile.owner];
			if (player.active && !player.dead)
			{
				player.itemRotation = player.direction > 0 ? itemRotation : MathHelper.Pi + itemRotation;
				Projectile.friendly = true;
				Projectile.DamageType = DamageClass.Melee;
			}
		}
		else
		{
			Projectile.friendly = false;
			Projectile.hostile = true;
		}

		if (Projectile.timeLeft > 40)
		{
			float rotationStep = MathHelper.Lerp(0.14f, 0f, 1f - (Projectile.timeLeft - 40) / 60f);
			Projectile.rotation += _reverse ? -rotationStep : rotationStep;
			EmitExactProfileSwingParticles();
		}

		if (Projectile.timeLeft == 60 && !_npcOwned)
		{
			HandleProjectileBlocking();
			QueueNextSlash();
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Vector2 ownerCenter = OwnerCenterWorld() - Main.screenPosition;
		float weaponRotation = CurrentWeaponRotation();
		int style = ModContent.GetInstance<MeleeWeaponEffectsVisualConfig>().SlashStyle;

		if (_usesVisualProfile)
		{
			DrawProfiledSlash(ownerCenter, style);
		}
		else
		{
			BuildVertices(ownerCenter);
			if (_vertexCount >= 3)
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, style == 1 ? BlendState.AlphaBlend : BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				Main.graphics.GraphicsDevice.Textures[0] = style == 1 ? TextureAssets.Projectile[Projectile.type].Value : MeleeEffectAssets.GetTexture(MeleeEffectAssets.SlashTexture);
				Main.graphics.GraphicsDevice.Textures[1] = GetWeaponTexture();
				_slashEffect.CurrentTechnique.Passes[0].Apply();
				Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, _vertexCount - 2);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		DrawWeapon(ownerCenter, weaponRotation);
		return false;
	}

	private Vector2 OwnerCenterWorld()
	{
		if (_npcOwned)
		{
			int npcIndex = (int)Projectile.ai[0];
			if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
			{
				return Main.npc[npcIndex].Center;
			}
		}

		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			return Main.player[Projectile.owner].Center;
		}

		return Projectile.Center;
	}

	private void DrawProfiledSlash(Vector2 ownerCenter, int style)
	{
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, style == 1 ? BlendState.AlphaBlend : BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		Main.graphics.GraphicsDevice.Textures[0] = style == 1 ? TextureAssets.Projectile[Projectile.type].Value : MeleeEffectAssets.GetTexture(MeleeEffectAssets.SlashTexture);
		Main.graphics.GraphicsDevice.Textures[1] = GetWeaponTexture();
		_slashEffect.CurrentTechnique.Passes[0].Apply();

		if (_profileFarRimAlpha > 0f)
		{
			DrawProfilePass(ownerCenter, style, ProfileVisualPass.FarRim, 0f);
		}

		for (int trail = _profileTrailCount; trail >= 1; trail--)
		{
			DrawProfilePass(ownerCenter, style, ProfileVisualPass.TrailEcho, -_profileTrailDelayRadians * trail);
		}

		DrawProfilePass(ownerCenter, style, ProfileVisualPass.Main, 0f);

		if (_profileCoreAlpha > 0f)
		{
			DrawProfilePass(ownerCenter, style, ProfileVisualPass.Core, 0f);
		}

		if (_profilePeakFlareAlpha > 0f)
		{
			DrawProfilePass(ownerCenter, style, ProfileVisualPass.PeakFlare, 0f);
		}

		if (_profileNearEdgeAlpha > 0f)
		{
			DrawProfilePass(ownerCenter, style, ProfileVisualPass.NearEdge, 0f);
		}

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	}

	private void DrawProfilePass(Vector2 ownerCenter, int style, ProfileVisualPass pass, float angleOffset)
	{
		BuildProfileVertices(ownerCenter, style, pass, angleOffset);
		if (_vertexCount >= 3)
		{
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, _vertexCount - 2);
		}
	}

	private void BuildVertices(Vector2 ownerCenter)
	{
		_vertexCount = 0;
		int style = ModContent.GetInstance<MeleeWeaponEffectsVisualConfig>().SlashStyle;

		for (int i = 0; i < Projectile.oldPos.Length; i++)
		{
			if (Projectile.oldRot[i] == 0f)
			{
				continue;
			}

			float factor = 1f - i / 40f;
			Vector2 outer = Projectile.oldRot[i].ToRotationVector2() * Projectile.velocity.Length();
			outer.Y *= Projectile.localAI[1];
			outer = outer.RotatedBy(Projectile.ai[1]);

			Vector2 inner;
			if (style == 1)
			{
				inner = Projectile.oldRot[i].ToRotationVector2() * Projectile.velocity.Length() * (1f - Projectile.localAI[0] + Projectile.localAI[0] * i / 40f);
			}
			else
			{
				inner = Projectile.oldRot[i].ToRotationVector2() * 10f;
			}

			inner.Y *= Projectile.localAI[1];
			inner = inner.RotatedBy(Projectile.ai[1]);

			if (_vertexCount + 2 > _vertices.Length)
			{
				break;
			}

			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + outer, new Vector3(factor, 0f, 1f), _color * factor);
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + inner, new Vector3(factor, 1f, 1f), _color * factor);
		}
	}

	private void BuildProfileVertices(Vector2 ownerCenter, int style, ProfileVisualPass pass, float angleOffset)
	{
		_vertexCount = 0;
		int activeTrailCount = CountActiveTrailSamples();
		if (activeTrailCount < 2)
		{
			return;
		}

		int activeIndex = 0;
		for (int i = 0; i < Projectile.oldPos.Length; i++)
		{
			if (Projectile.oldRot[i] == 0f)
			{
				continue;
			}

			float trailPosition = activeIndex / Math.Max(1f, activeTrailCount - 1f);
			float trailFactor = 1f - trailPosition;
			float progress = ProfileProgressAtTrailIndex(i);
			float depth = EvaluateProfileDepth(progress);
			float nearAmount = MathHelper.Clamp((depth + 0.9f) / 2.4f, 0f, 1f);
			float farAmount = 1f - nearAmount;
			float hitPeak = EvaluateHitPeak(progress);
			float crescent = CrescentWidthFactor(trailPosition);
			float tipVisibility = TipVisibilityFactor(trailPosition);

			GetProfilePassSettings(pass, nearAmount, farAmount, hitPeak, out float alpha, out float outerScale, out float widthScale, out float offsetPixels, out Color passColor);
			if (alpha <= 0f)
			{
				activeIndex++;
				continue;
			}

			float tipConvergence = 1f - tipVisibility;
			float rotation = Projectile.oldRot[i] + angleOffset * tipConvergence;
			float depthScale = 1f + MathHelper.Clamp(depth, -1.2f, 1.5f) * 0.08f;
			float length = Projectile.velocity.Length();
			outerScale = MathHelper.Lerp(1f, outerScale, tipConvergence);
			offsetPixels *= tipConvergence;
			float outerRadius = length * outerScale * depthScale;
			float innerRadius;
			if (style == 1)
			{
				float width = MathHelper.Clamp(Projectile.localAI[0] * widthScale * crescent, 0f, 0.95f);
				float innerTaper = 0.18f + 0.82f * crescent;
				innerRadius = length * (outerScale - width + width * trailPosition * innerTaper) * depthScale;
			}
			else
			{
				innerRadius = 10f * outerScale * depthScale;
			}

			Vector2 outer = ProfileVector(rotation, outerRadius);
			Vector2 inner = ProfileVector(rotation, innerRadius);
			Vector2 offset = ProfileScreenOffset(outer, offsetPixels);
			float depthAlpha = 1f + MathHelper.Clamp(depth, -1f, 1.4f) * 0.1f;
			float tipAlpha = MathHelper.Lerp(0.58f, CrescentAlphaFactor(crescent), tipConvergence);
			float trailAlpha = MathHelper.Lerp(0.32f, trailFactor, tipConvergence);
			Color color = passColor * alpha * trailAlpha * depthAlpha * tipAlpha;

			if (_vertexCount + 2 > _vertices.Length)
			{
				break;
			}

			float texX = MathHelper.Clamp(trailFactor, 0.08f, 0.92f);
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + outer + offset, new Vector3(texX, 0f, 1f), color);
			_vertices[_vertexCount++] = new SlashVertex(ownerCenter + inner + offset, new Vector3(texX, 1f, 1f), color);
			activeIndex++;
		}
	}

	private Vector2 ProfileVector(float rotation, float radius)
	{
		Vector2 direction = rotation.ToRotationVector2();
		direction.X *= _profileXScale;
		direction.Y *= Projectile.localAI[1];
		return (direction * radius).RotatedBy(Projectile.ai[1]);
	}

	private int CountActiveTrailSamples()
	{
		int count = 0;
		for (int i = 0; i < Projectile.oldRot.Length; i++)
		{
			if (Projectile.oldRot[i] != 0f)
			{
				count++;
			}
		}

		return count;
	}

	private static float CrescentWidthFactor(float position)
	{
		position = MathHelper.Clamp(position, 0f, 1f);
		float centerWeight = MathF.Sin(position * MathHelper.Pi);
		float leadingTip = Smooth01(MathHelper.Clamp(position / 0.18f, 0f, 1f));
		float trailingTip = Smooth01(MathHelper.Clamp((1f - position) / 0.24f, 0f, 1f));
		float tipWeight = Math.Min(leadingTip, trailingTip);
		return MathHelper.Clamp(centerWeight * tipWeight, 0f, 1f);
	}

	private static float CrescentAlphaFactor(float crescent)
	{
		return MathHelper.Lerp(0.18f, 1f, MathF.Sqrt(MathHelper.Clamp(crescent, 0f, 1f)));
	}

	private static float TipVisibilityFactor(float position)
	{
		float edgeDistance = Math.Min(position, 1f - position);
		return 1f - Smooth01(MathHelper.Clamp(edgeDistance / 0.12f, 0f, 1f));
	}

	private Vector2 ProfileScreenOffset(Vector2 outer, float offsetPixels)
	{
		if (offsetPixels == 0f)
		{
			return Vector2.Zero;
		}

		Vector2 normal = new(-outer.Y, outer.X);
		if (normal.LengthSquared() < 0.001f)
		{
			return Vector2.Zero;
		}

		normal.Normalize();
		if (_reverse)
		{
			normal *= -1f;
		}

		return normal * offsetPixels;
	}

	private void GetProfilePassSettings(ProfileVisualPass pass, float nearAmount, float farAmount, float hitPeak, out float alpha, out float outerScale, out float widthScale, out float offsetPixels, out Color passColor)
	{
		alpha = _profileMainAlpha;
		outerScale = 1f;
		widthScale = 1f;
		offsetPixels = 0f;
		passColor = _color;

		switch (pass)
		{
			case ProfileVisualPass.FarRim:
				alpha = _profileFarRimAlpha * (0.35f + farAmount * 0.55f);
				outerScale = 0.985f;
				widthScale = 0.14f;
				offsetPixels = -_profileFarRimOffsetPixels * (0.4f + farAmount * 0.6f);
				passColor = Color.Lerp(_color, Color.Black, 0.28f);
				break;
			case ProfileVisualPass.TrailEcho:
				alpha = _profileMainAlpha * 0.12f;
				outerScale = 0.96f;
				widthScale = 0.36f;
				offsetPixels = -_profileFarRimOffsetPixels * 0.35f;
				passColor = Color.Lerp(_color, Color.White, 0.2f);
				break;
			case ProfileVisualPass.Core:
				alpha = _profileCoreAlpha * (0.72f + hitPeak * 0.38f);
				outerScale = 0.965f + nearAmount * 0.025f;
				widthScale = 0.16f;
				passColor = Color.Lerp(_color, Color.White, 0.78f);
				break;
			case ProfileVisualPass.PeakFlare:
				alpha = _profilePeakFlareAlpha * hitPeak * 0.82f;
				outerScale = 1.02f + nearAmount * 0.06f;
				widthScale = 0.16f;
				offsetPixels = _profileNearEdgeOffsetPixels * nearAmount * 0.45f;
				passColor = Color.Lerp(_color, Color.White, 0.82f);
				break;
			case ProfileVisualPass.NearEdge:
				alpha = _profileNearEdgeAlpha * (0.55f + nearAmount * 0.85f);
				outerScale = 1.025f + nearAmount * 0.035f;
				widthScale = 0.12f;
				offsetPixels = _profileNearEdgeOffsetPixels * nearAmount;
				passColor = Color.Lerp(_color, Color.White, 0.62f);
				break;
		}
	}

	private float ProfileProgressAtTrailIndex(int trailIndex)
	{
		return MathHelper.Clamp((_profileAge - trailIndex) / 60f, 0f, 1f);
	}

	private float EvaluateProfileDepth(float progress)
	{
		if (progress <= _profileHitProgress)
		{
			return MathHelper.Lerp(_profileStartDepth, _profileHitDepth, Smooth01(progress / _profileHitProgress));
		}

		return MathHelper.Lerp(_profileHitDepth, _profileEndDepth, Smooth01((progress - _profileHitProgress) / (1f - _profileHitProgress)));
	}

	private float EvaluateHitPeak(float progress)
	{
		float distance = Math.Abs(progress - _profileHitProgress);
		float peak = 1f - MathHelper.Clamp(distance / 0.16f, 0f, 1f);
		return peak * peak;
	}

	private static float Smooth01(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}

	private void DrawWeapon(Vector2 ownerCenter, float weaponRotation)
	{
		Texture2D weaponTexture = GetWeaponTexture();
		if (weaponTexture == null)
		{
			return;
		}

		Vector2 position = ownerCenter + weaponRotation.ToRotationVector2() * weaponTexture.Size() / 2f;
		SpriteEffects effects = SpriteEffects.None;
		float rotation = weaponRotation + 0.785f;

		if (_reverse && Projectile.ai[1] > -MathHelper.PiOver2 && Projectile.ai[1] < MathHelper.PiOver2)
		{
			effects = SpriteEffects.FlipHorizontally;
			rotation += 1.5707f;
		}
		else if ((_reverse && Projectile.ai[1] <= -MathHelper.PiOver2) || Projectile.ai[1] >= MathHelper.PiOver2)
		{
			effects = SpriteEffects.FlipHorizontally;
			rotation += 1.5707f;
		}

		Main.EntitySpriteDraw(weaponTexture, position, null, Color.White, rotation, weaponTexture.Size() / 2f, Projectile.scale, effects, 0f);
	}

	private float CurrentWeaponRotation()
	{
		Vector2 direction = Projectile.rotation.ToRotationVector2();
		direction.Y *= Projectile.localAI[1];
		direction = direction.RotatedBy(Projectile.ai[1]);
		return direction.ToRotation();
	}

	private Texture2D GetWeaponTexture()
	{
		if (_weaponItemType <= 0 || _weaponItemType >= TextureAssets.Item.Length)
		{
			return null;
		}

		return TextureAssets.Item[_weaponItemType].Value;
	}

	private bool CanBlock(Projectile target)
	{
		float _ = 0f;
		Rectangle hitbox = target.Hitbox;
		Vector2 center = OwnerCenterWorld();
		Vector2 end = center + Projectile.velocity.Length() * Projectile.ai[1].ToRotationVector2();
		return Collision.CheckAABBvLineCollision(hitbox.TopLeft(), hitbox.Size(), center, end, Projectile.velocity.Length() * Projectile.localAI[1], ref _);
	}

	private void HandleProjectileBlocking()
	{
		if (!ModContent.GetInstance<MeleeWeaponEffectsGameplayConfig>().SlashCanKillProjectiles)
		{
			return;
		}

		bool blocked = false;
		foreach (Projectile target in Main.projectile)
		{
			if (!target.active || !target.hostile || target.damage <= 0 || !CanBlock(target))
			{
				continue;
			}

			target.Kill();
			blocked = true;
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.position, Vector2.Zero, ProjectileID.DaybreakExplosion, 0, 0f, Projectile.owner);
		}

		if (blocked)
		{
			SoundStyle style = new("MeleeWeaponEffects/Sounds/Block") { Volume = 0.4f };
			MeleeEffectAssets.PlaySound(in style, Projectile.Center);
		}
	}

	private void QueueNextSlash()
	{
		if (_nextSlashCount < 1 || Projectile.owner != Main.myPlayer)
		{
			return;
		}

		Player player = Main.player[Projectile.owner];
		float randomOffset = Main.rand.NextFloat(-0.4f, 0.4f);
		float startingRotation = _reverse ? -1.9f + randomOffset : 1.9f + randomOffset;
		CreateSlash(true, Projectile.GetSource_FromAI(), _targetRotation, startingRotation, 353f, 0.45f, 0.35f, 5, Projectile.damage, 5f, player.whoAmI, color: _color, weaponItemType: _weaponItemType, nextSlashCount: _nextSlashCount - 1);
	}

	private void SpawnHitDust(NPC target, int dustType, int count, float minScale, float maxScale)
	{
		for (int i = 0; i < count; i++)
		{
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, dustType, 0f, 0f, 0, default, Main.rand.NextFloat(minScale, maxScale));
			dust.velocity = (Projectile.ai[1] + Main.rand.NextFloat(-0.45f, 0.45f)).ToRotationVector2() * Main.rand.NextFloat(0.4f, 10f);
		}
	}

	private void EmitExactProfileHitParticles(NPC target)
	{
		if (SlashProfileResolver.TryGetExactProfile(_weaponItemType, out WeaponSlashProfile profile))
		{
			float strength = _usesVisualProfile ? MathHelper.Clamp(1f + _profilePeakFlareAlpha * 0.4f, 1f, 1.4f) : 1f;
			SlashParticleEmitter.EmitHitParticles(in profile, target.Center, Projectile.ai[1], strength);
		}
	}

	private void EmitExactProfileSwingParticles()
	{
		if (Projectile.timeLeft % 3 != 0 || !SlashProfileResolver.TryGetExactProfile(_weaponItemType, out WeaponSlashProfile profile))
		{
			return;
		}

		int maxTrailIndex = Math.Min(8, Projectile.oldRot.Length - 1);
		int trailIndex = maxTrailIndex > 0 ? Main.rand.Next(maxTrailIndex + 1) : 0;
		float rotation = Projectile.oldRot[trailIndex] == 0f ? Projectile.rotation : Projectile.oldRot[trailIndex];
		float radius = Projectile.velocity.Length() * Main.rand.NextFloat(0.72f, 1.02f);
		Vector2 offset = SlashOffsetForRotation(rotation, radius);
		Vector2 radial = offset.SafeNormalize(Vector2.UnitX);
		Vector2 tangent = new(-radial.Y, radial.X);
		if (_reverse)
		{
			tangent *= -1f;
		}

		float strength = _usesVisualProfile ? MathHelper.Clamp(0.13f + _profilePeakFlareAlpha * 0.11f, 0.13f, 0.24f) : 0.16f;
		SlashParticleEmitter.EmitSwingTrailParticles(in profile, OwnerCenterWorld() + offset, tangent, radial, strength);
	}

	private Vector2 SlashOffsetForRotation(float rotation, float radius)
	{
		if (_usesVisualProfile)
		{
			return ProfileVector(rotation, radius);
		}

		Vector2 direction = rotation.ToRotationVector2();
		direction.Y *= Projectile.localAI[1];
		return (direction * radius).RotatedBy(Projectile.ai[1]);
	}

	private void EmitOnHitProjectiles(NPC target)
	{
		if (!_npcOwned && Main.player[Projectile.owner].HeldItem.type == ItemID.Bladetongue)
		{
			for (int i = 0; i < 3; i++)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Main.rand.NextVector2Unit(0f, MathHelper.TwoPi) * 5f, ProjectileID.IchorSplash, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
			}
		}
	}

	private enum ProfileVisualPass
	{
		FarRim,
		TrailEcho,
		Main,
		Core,
		PeakFlare,
		NearEdge
	}
}
