using System;
using System.Collections.Generic;
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

public class HeroSlashMethod : ModProjectile
{
	private Effect SlashEff = ModContent.Request<Effect>("MeleeWeaponEffects/Effects/Mhd", (AssetRequestMode)2).Value;

	public bool Reverse;

	public bool IsNPCproj;

	public Color c = Color.White;

	public Texture2D t;

	public int ShouldDoNextSlash;

	public float Scale1 = 1f;

	public override string Texture => "Terraria/Images/Extra_193";

	public static void Slash(bool IsPlayerProj, IEntitySource source, float rotation, float Startingrot, float Length, float Thick, float YScale, int ExtraSpeed = 0, int damage = 50, float Knockback = 0f, int owner = 0, int ownerNPC = 0, Color color = default(Color), Texture2D weaponTex = null, int ShouldDoNextSlash = 0, float KnockBackRotation = 0f, float WeaponScale = 1f)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		Thick = ModContent.GetInstance<Mconfig>().SlashScale;
		if (IsPlayerProj)
		{
			Projectile val = Projectile.NewProjectileDirect(source, ((Entity)Main.player[owner]).Center, Utils.ToRotationVector2(KnockBackRotation) * Length, ModContent.ProjectileType<HeroSlashMethod>(), damage, Knockback, owner, 0f, rotation, 0f);
			val.rotation = Startingrot;
			(val.ModProjectile as HeroSlashMethod).Reverse = Startingrot > 0f;
			val.localAI[0] = Thick;
			val.localAI[1] = YScale;
			if (color != default(Color))
			{
				(val.ModProjectile as HeroSlashMethod).c = color;
			}
			val.extraUpdates = ExtraSpeed;
			(val.ModProjectile as HeroSlashMethod).Scale1 = WeaponScale;
			(val.ModProjectile as HeroSlashMethod).ShouldDoNextSlash = ShouldDoNextSlash;
			if (weaponTex != null)
			{
				(val.ModProjectile as HeroSlashMethod).t = weaponTex;
			}
			Projectile val2 = Projectile.NewProjectileDirect(source, ((Entity)Main.player[owner]).Center, Utils.ToRotationVector2(KnockBackRotation) * Length, ModContent.ProjectileType<HeroSlash2>(), damage, Knockback, owner, 0f, rotation, 0f);
			val2.rotation = Startingrot;
			(val2.ModProjectile as HeroSlash2).Reverse = Startingrot > 0f;
			val2.localAI[0] = Thick;
			val2.localAI[1] = YScale;
			if (color != default(Color))
			{
				(val2.ModProjectile as HeroSlash2).c = color;
			}
			val2.extraUpdates = ExtraSpeed;
			(val2.ModProjectile as HeroSlash2).T = weaponTex;
		}
		if (!IsPlayerProj)
		{
			Projectile val3 = Projectile.NewProjectileDirect(source, ((Entity)Main.npc[ownerNPC]).Center, Vector2.UnitX * Length, ModContent.ProjectileType<HeroSlashMethod>(), damage, Knockback, 0, (float)ownerNPC, rotation, 0f);
			val3.rotation = Startingrot;
			(val3.ModProjectile as HeroSlashMethod).Reverse = Startingrot > 0f;
			(val3.ModProjectile as HeroSlashMethod).IsNPCproj = true;
			val3.localAI[0] = Thick;
			val3.localAI[1] = YScale;
			if (color != default(Color))
			{
				(val3.ModProjectile as HeroSlashMethod).c = color;
			}
			val3.extraUpdates = ExtraSpeed;
			Projectile val4 = Projectile.NewProjectileDirect(source, ((Entity)Main.npc[ownerNPC]).Center, Vector2.UnitX * Length, ModContent.ProjectileType<HeroSlash2>(), damage, Knockback, 0, (float)ownerNPC, rotation, 0f);
			val4.rotation = Startingrot;
			(val4.ModProjectile as HeroSlash2).Reverse = Startingrot > 0f;
			(val4.ModProjectile as HeroSlash2).IsNPCproj = true;
			val4.localAI[0] = Thick;
			val4.localAI[1] = YScale;
			if (color != default(Color))
			{
				(val4.ModProjectile as HeroSlash2).c = color;
			}
			val4.extraUpdates = ExtraSpeed;
			(val4.ModProjectile as HeroSlash2).T = weaponTex;
		}
	}

	public override void CutTiles()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		DelegateMethods.tilecut_0 = (TileCuttingContext)2;
		_ = ((Entity)((ModProjectile)this).Projectile).velocity;
		Vector2 center = ((Entity)((ModProjectile)this).Projectile).Center;
		Vector2 val = ((Entity)Main.player[((ModProjectile)this).Projectile.owner]).Center + ((Entity)((ModProjectile)this).Projectile).velocity.Length() * Utils.ToRotationVector2(((ModProjectile)this).Projectile.ai[1]);
		float num = ((Entity)((ModProjectile)this).Projectile).velocity.Length() * ((ModProjectile)this).Projectile.localAI[1];
		Utils.PlotTileLine(center, val, num, DelegateMethods.CutTiles);
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[((ModProjectile)this).Projectile.type] = 40;
		ProjectileID.Sets.TrailingMode[((ModProjectile)this).Projectile.type] = 2;
	}

	public override void SetDefaults()
	{
		((Entity)((ModProjectile)this).Projectile).width = (((Entity)((ModProjectile)this).Projectile).height = 24);
		((ModProjectile)this).Projectile.DamageType = DamageClass.Melee;
		((ModProjectile)this).Projectile.timeLeft = 100;
		((ModProjectile)this).Projectile.tileCollide = false;
		((ModProjectile)this).Projectile.penetrate = -1;
		((ModProjectile)this).Projectile.ignoreWater = true;
		((ModProjectile)this).Projectile.aiStyle = -11;
		((ModProjectile)this).Projectile.usesLocalNPCImmunity = true;
		((ModProjectile)this).Projectile.localNPCHitCooldown = 1000;
		((ModProjectile)this).Projectile.noEnchantmentVisuals = true;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Player val = Main.player[((ModProjectile)this).Projectile.owner];
		float num = 0f;
		Vector2 val2 = ((Entity)val).Center + ((Entity)((ModProjectile)this).Projectile).velocity.Length() * Utils.ToRotationVector2(((ModProjectile)this).Projectile.ai[1]);
		return Collision.CheckAABBvLineCollision(Utils.TopLeft(targetHitbox), Utils.Size(targetHitbox), ((Entity)val).Center, val2, ((Entity)((ModProjectile)this).Projectile).velocity.Length() * ((ModProjectile)this).Projectile.localAI[1], ref num);
	}

	private bool CanBlock(Projectile target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Rectangle hitbox = ((Entity)target).Hitbox;
		Player val = Main.player[((ModProjectile)this).Projectile.owner];
		float num = 0f;
		Vector2 val2 = ((Entity)val).Center + ((Entity)((ModProjectile)this).Projectile).velocity.Length() * Utils.ToRotationVector2(((ModProjectile)this).Projectile.ai[1]);
		return Collision.CheckAABBvLineCollision(Utils.TopLeft(hitbox), Utils.Size(hitbox), ((Entity)val).Center, val2, ((Entity)((ModProjectile)this).Projectile).velocity.Length() * ((ModProjectile)this).Projectile.localAI[1], ref num);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		Player val = Main.player[((ModProjectile)this).Projectile.owner];
		ItemLoader.OnHitNPC(val.HeldItem, val, target, ref hit, damageDone);
		Tetrad.ShootProjectileDir(((Entity)target).Center, Vector2.Zero, ModContent.ProjectileType<ExampleSlashingEffect>(), 0, 0f, 0, Utils.NextFloat(Main.rand, -3.14f, 3.14f));
		SoundStyle? hitSound = target.HitSound;
		SoundStyle nPCHit = SoundID.NPCHit4;
		if (hitSound.HasValue && (!hitSound.HasValue || hitSound.GetValueOrDefault() == nPCHit))
		{
			for (int i = 0; i < 22; i++)
			{
				SoundStyle style = new SoundStyle("MeleeWeaponEffects/Sounds/Block") { Volume = 0.25f };
				Tetrad.PlaySound(in style);
				Dust.NewDustDirect(((Entity)target).position, ((Entity)target).width, ((Entity)target).height, 6, 0f, 0f, 0, default(Color), Utils.NextFloat(Main.rand, 1.2f, 3f)).velocity = Utils.ToRotationVector2(((ModProjectile)this).Projectile.ai[1] + Utils.NextFloat(Main.rand, -0.45f, 0.45f)) * Utils.NextFloat(Main.rand, 0.4f, 10f);
			}
		}
		else
		{
			SoundStyle style2 = new SoundStyle("MeleeWeaponEffects/Sounds/Onhit")
			{
				Volume = 0.45f,
				Pitch = Utils.NextFloat(Main.rand, -0.15f, 0.15f)
			};
			Tetrad.PlaySound(in style2);
			for (int j = 0; j < 32; j++)
			{
				Dust.NewDustDirect(((Entity)target).position, ((Entity)target).width, ((Entity)target).height, 5, 0f, 0f, 0, default(Color), Utils.NextFloat(Main.rand, 0.9f, 1.7f)).velocity = Utils.ToRotationVector2(((ModProjectile)this).Projectile.ai[1] + Utils.NextFloat(Main.rand, -0.45f, 0.45f)) * Utils.NextFloat(Main.rand, 0.4f, 10f);
			}
		}
		VanillaOnHit(val.HeldItem.type, target);
	}

	public override void AI()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Utils.ToRotationVector2(((ModProjectile)this).Projectile.rotation);
		val.Y *= ((ModProjectile)this).Projectile.localAI[1];
		val = Utils.RotatedBy(val, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
		float num = Utils.ToRotation(val);
		if (((Entity)Main.player[((ModProjectile)this).Projectile.owner]).direction > 0)
		{
			Main.player[((ModProjectile)this).Projectile.owner].itemRotation = num;
		}
		else
		{
			Main.player[((ModProjectile)this).Projectile.owner].itemRotation = 3.1416f + num;
		}
		if (((ModProjectile)this).Projectile.timeLeft > 40)
		{
			float num2 = MathHelper.Lerp(0.14f, 0f, 1f - (float)(((ModProjectile)this).Projectile.timeLeft - 40) / 60f);
			if (!Reverse)
			{
				Projectile projectile = ((ModProjectile)this).Projectile;
				projectile.rotation += num2;
			}
			else
			{
				Projectile projectile2 = ((ModProjectile)this).Projectile;
				projectile2.rotation -= num2;
			}
		}
		if (!IsNPCproj)
		{
			((ModProjectile)this).Projectile.friendly = true;
			((ModProjectile)this).Projectile.DamageType = DamageClass.Melee;
		}
		if (((ModProjectile)this).Projectile.timeLeft == 60 && !IsNPCproj)
		{
			bool flag = false;
			if (ModContent.GetInstance<Mconfig>().SlashCanKillProjectiles)
			{
				Projectile[] projectile3 = Main.projectile;
				foreach (Projectile val2 in projectile3)
				{
					if (val2.hostile && val2.damage > 0 && CanBlock(val2) && ((Entity)val2).active)
					{
						val2.Kill();
						flag = true;
						Projectile.NewProjectile((IEntitySource)null, ((Entity)val2).position, Vector2.Zero, 953, 0, 0f, 0, 0f, 0f, 0f);
					}
				}
			}
			if (flag)
			{
				SoundStyle style = new SoundStyle("MeleeWeaponEffects/Sounds/Block") { Volume = 0.4f };
				Tetrad.PlaySound(in style);
			}
			Player val3 = Main.player[((ModProjectile)this).Projectile.owner];
			if (ShouldDoNextSlash >= 1)
			{
				float num3 = Utils.NextFloat(Main.rand, -0.4f, 0.4f);
				Slash(IsPlayerProj: true, ((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), Utils.ToRotation(Main.MouseWorld - ((Entity)val3).Center), Reverse ? (-1.9f + num3) : (1.9f + num3), 353f, 0.45f, 0.35f, 5, ((ModProjectile)this).Projectile.damage, 5f, ((Entity)val3).whoAmI, 0, c, t, ShouldDoNextSlash - 1);
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_0643: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_0753: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_0781: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Utils.ToRotationVector2(((ModProjectile)this).Projectile.rotation);
		val.Y *= ((ModProjectile)this).Projectile.localAI[1];
		val = Utils.RotatedBy(val, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
		float num = Utils.ToRotation(val);
		Vector2 zero = Vector2.Zero;
		zero = ((!IsNPCproj) ? (((Entity)Main.player[((ModProjectile)this).Projectile.owner]).Center - Main.screenPosition) : (((Entity)Main.npc[(int)((ModProjectile)this).Projectile.ai[0]]).Center - Main.screenPosition));
		List<VertexInfo2> list = new List<VertexInfo2>();
		for (int i = 0; i < ((ModProjectile)this).Projectile.oldPos.Length; i++)
		{
			if (((ModProjectile)this).Projectile.oldRot[i] != 0f)
			{
				if (ModContent.GetInstance<Mconfig>().Style == 0)
				{
					Vector2 val2 = Utils.ToRotationVector2(((ModProjectile)this).Projectile.oldRot[i]) * ((Entity)((ModProjectile)this).Projectile).velocity.Length();
					val2.Y *= ((ModProjectile)this).Projectile.localAI[1];
					val2 = Utils.RotatedBy(val2, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
					list.Add(new VertexInfo2(zero + val2, new Vector3(1f - (float)i / 40f, 0f, 1f), c * (1f - (float)i / 40f)));
					Vector2 val3 = Utils.ToRotationVector2(((ModProjectile)this).Projectile.oldRot[i]) * ((Entity)((ModProjectile)this).Projectile).velocity.Length() * (1f - ((ModProjectile)this).Projectile.localAI[0] + ((ModProjectile)this).Projectile.localAI[0] * (float)i / 40f);
					val3.Y *= ((ModProjectile)this).Projectile.localAI[1];
					val3 = Utils.RotatedBy(val3, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
					list.Add(new VertexInfo2(zero + val3, new Vector3(1f - (float)i / 40f, 1f, 1f), c * (1f - (float)i / 40f)));
				}
				else if (ModContent.GetInstance<Mconfig>().Style == 1)
				{
					Vector2 val4 = Utils.ToRotationVector2(((ModProjectile)this).Projectile.oldRot[i]) * ((Entity)((ModProjectile)this).Projectile).velocity.Length();
					val4.Y *= ((ModProjectile)this).Projectile.localAI[1];
					val4 = Utils.RotatedBy(val4, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
					list.Add(new VertexInfo2(zero + val4, new Vector3(1f - (float)i / 40f, 0f, 1f), c * (1f - (float)i / 40f)));
					Vector2 val5 = Utils.ToRotationVector2(((ModProjectile)this).Projectile.oldRot[i]) * 10f;
					val5.Y *= ((ModProjectile)this).Projectile.localAI[1];
					val5 = Utils.RotatedBy(val5, (double)((ModProjectile)this).Projectile.ai[1], default(Vector2));
					list.Add(new VertexInfo2(zero + val5, new Vector3(1f - (float)i / 40f, 1f, 1f), c * (1f - (float)i / 40f)));
				}
			}
		}
		Main.spriteBatch.End();
		Main.spriteBatch.Begin((SpriteSortMode)1, (ModContent.GetInstance<Mconfig>().Style == 0) ? BlendState.AlphaBlend : BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, Main.GameViewMatrix.TransformationMatrix);
		Main.graphics.GraphicsDevice.Textures[0] = (Texture)(object)((ModContent.GetInstance<Mconfig>().Style == 0) ? TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value : ModContent.Request<Texture2D>("MeleeWeaponEffects/SlashTex", (AssetRequestMode)2).Value);
		Main.graphics.GraphicsDevice.Textures[1] = (Texture)(object)t;
		SlashEff.CurrentTechnique.Passes[0].Apply();
		if (list.Count >= 3)
		{
			Main.graphics.GraphicsDevice.DrawUserPrimitives<VertexInfo2>((PrimitiveType)1, list.ToArray(), 0, list.Count - 2);
		}
		Main.spriteBatch.End();
		Main.spriteBatch.Begin((SpriteSortMode)1, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, Main.GameViewMatrix.TransformationMatrix);
		if (t != null)
		{
			float num2 = 1f;
			if (Reverse && ((ModProjectile)this).Projectile.ai[1] > -(float)Math.PI / 2f && ((ModProjectile)this).Projectile.ai[1] < (float)Math.PI / 2f)
			{
				Main.EntitySpriteDraw(t, zero + Utils.ToRotationVector2(num) * Utils.Size(t) / 2f, (Rectangle?)null, Color.White, num + 0.785f + 1.5707f, Utils.Size(t) / 2f, ((ModProjectile)this).Projectile.scale * num2, (SpriteEffects)1, 0f);
			}
			if (!Reverse && ((ModProjectile)this).Projectile.ai[1] > -(float)Math.PI / 2f && ((ModProjectile)this).Projectile.ai[1] < (float)Math.PI / 2f)
			{
				Main.EntitySpriteDraw(t, zero + Utils.ToRotationVector2(num) * Utils.Size(t) / 2f, (Rectangle?)null, Color.White, num + 0.785f, Utils.Size(t) / 2f, ((ModProjectile)this).Projectile.scale * num2, (SpriteEffects)0, 0f);
			}
			else if ((Reverse && ((ModProjectile)this).Projectile.ai[1] <= -(float)Math.PI / 2f) || ((ModProjectile)this).Projectile.ai[1] >= (float)Math.PI / 2f)
			{
				Main.EntitySpriteDraw(t, zero + Utils.ToRotationVector2(num) * Utils.Size(t) / 2f, (Rectangle?)null, Color.White, num + 0.785f + 1.5707f, Utils.Size(t) / 2f, ((ModProjectile)this).Projectile.scale * num2, (SpriteEffects)1, 0f);
			}
			else if ((!Reverse && ((ModProjectile)this).Projectile.ai[1] <= -(float)Math.PI / 2f) || ((ModProjectile)this).Projectile.ai[1] >= (float)Math.PI / 2f)
			{
				Main.EntitySpriteDraw(t, zero + Utils.ToRotationVector2(num) * Utils.Size(t) / 2f, (Rectangle?)null, Color.White, num + 0.785f, Utils.Size(t) / 2f, ((ModProjectile)this).Projectile.scale * num2, (SpriteEffects)0, 0f);
			}
		}
		return false;
	}

	private void VanillaOnHit(int ID, NPC target)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (ID == 3211)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 center = ((Entity)target).Center;
				Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), center, Utils.NextVector2Unit(Main.rand, 0f, (float)Math.PI * 2f) * 5f, 524, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			}
		}
	}
}
