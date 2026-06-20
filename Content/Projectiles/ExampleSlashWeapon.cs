using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class ExampleSlashWeapon : ModProjectile
{
	public Texture2D Weapon;

	public override string Texture => "Terraria/Images/Item_" + (short)3063;

	public override void SetDefaults()
	{
		((Entity)((ModProjectile)this).Projectile).width = (((Entity)((ModProjectile)this).Projectile).height = 12);
		((ModProjectile)this).Projectile.timeLeft = 100;
		((ModProjectile)this).Projectile.friendly = false;
		((ModProjectile)this).Projectile.tileCollide = false;
		((ModProjectile)this).Projectile.ignoreWater = true;
	}

	public override void AI()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		((ModProjectile)this).Projectile.ai[1] += 1f;
		Player val = Main.player[((ModProjectile)this).Projectile.owner];
		int num = 2;
		val.itemAnimation = 3;
		val.itemTime = 3;
		val.heldProj = ((Entity)((ModProjectile)this).Projectile).whoAmI;
		((ModProjectile)this).Projectile.rotation = ((ModProjectile)this).Projectile.ai[0] + Utils.ToRotation(Main.MouseWorld - ((Entity)val).Center);
		if (((ModProjectile)this).Projectile.timeLeft > num)
		{
			((ModProjectile)this).Projectile.timeLeft = num;
		}
		if (Main.mouseRight)
		{
			((ModProjectile)this).Projectile.timeLeft = 2;
		}
		Projectile projectile = ((ModProjectile)this).Projectile;
		((Entity)projectile).velocity = ((Entity)projectile).velocity * 0f;
		((Entity)((ModProjectile)this).Projectile).Center = ((Entity)val).Center;
		int num2 = (int)((ModProjectile)this).Projectile.localAI[0] * 3;
		if (((ModProjectile)this).Projectile.ai[1] == (float)num2)
		{
			SoundEngine.PlaySound(SoundID.Item4);
			for (int i = 0; i < 30; i++)
			{
				Vector2 val2 = Utils.NextVector2CircularEdge(Main.rand, 50f, 50f);
				Dust.NewDust(((Entity)val).Center + val2, 0, 0, 173, 0f, 0f, 0, default(Color), 1.5f);
			}
		}
		((Entity)val).direction = Math.Sign(Main.MouseWorld.X - ((Entity)val).Center.X);
	}

	public override void OnKill(int timeLeft)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (((ModProjectile)this).Projectile.ai[1] >= (float)((int)((ModProjectile)this).Projectile.localAI[0] * 3))
		{
			SoundStyle val = new SoundStyle("MeleeWeaponEffects/Sounds/Slashing") { Volume = 0.8f };
			SoundEngine.PlaySound(val);
			Player val2 = Main.player[((ModProjectile)this).Projectile.owner];
			VanillaStyleShoots(charge: true, val2.HeldItem.type, ((Entity)val2).Center, val2);
			HeroSlashMethod.Slash(IsPlayerProj: true, ((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), Utils.ToRotation(Main.MouseWorld - ((Entity)val2).Center), ((ModProjectile)this).Projectile.ai[0], ((ModProjectile)this).Projectile.localAI[1] * 3f, 0.45f, 0.35f, 5, ((ModProjectile)this).Projectile.damage, 5f, ((Entity)val2).whoAmI, 0, Color.White, Weapon, 0, ((ModProjectile)this).Projectile.rotation - ((ModProjectile)this).Projectile.ai[0], ((ModProjectile)this).Projectile.localAI[1]);
			val2.GetModPlayer<Eplayer>().screentimer = 15;
		}
	}

	public void VanillaStyleShoots(bool charge, int ID, Vector2 position, Player player)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_065a: Unknown result type (might be due to invalid IL or missing references)
		//IL_065f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0700: Unknown result type (might be due to invalid IL or missing references)
		//IL_0749: Unknown result type (might be due to invalid IL or missing references)
		//IL_074e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0782: Unknown result type (might be due to invalid IL or missing references)
		//IL_0853: Unknown result type (might be due to invalid IL or missing references)
		//IL_0858: Unknown result type (might be due to invalid IL or missing references)
		//IL_085f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0864: Unknown result type (might be due to invalid IL or missing references)
		//IL_0869: Unknown result type (might be due to invalid IL or missing references)
		//IL_086e: Unknown result type (might be due to invalid IL or missing references)
		//IL_087c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0886: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08db: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0935: Unknown result type (might be due to invalid IL or missing references)
		//IL_093a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0941: Unknown result type (might be due to invalid IL or missing references)
		//IL_0946: Unknown result type (might be due to invalid IL or missing references)
		//IL_094b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0950: Unknown result type (might be due to invalid IL or missing references)
		//IL_095a: Unknown result type (might be due to invalid IL or missing references)
		//IL_09aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09af: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e3: Unknown result type (might be due to invalid IL or missing references)
		if (ID == 757)
		{
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 48f, 985, ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, (float)((Entity)player).direction * player.gravDir, 45f, 3f);
		}
		if (ID == 65 && !charge)
		{
			Vector2 val = Main.MouseWorld + new Vector2((float)Main.rand.Next(-500, 500), (float)Main.rand.Next(-520, -450));
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), val, Utils.SafeNormalize(Main.MouseWorld - val, Vector2.Zero) * 13f, 9, ((ModProjectile)this).Projectile.damage * 2, ((ModProjectile)this).Projectile.knockBack, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			return;
		}
		if (ID == 3065 && !charge)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 val2 = Main.MouseWorld + new Vector2((float)Main.rand.Next(-200, 200), (float)Main.rand.Next(-720, -500));
				Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), val2, Utils.SafeNormalize(Main.MouseWorld - val2, Vector2.Zero) * 19f, 503, ((ModProjectile)this).Projectile.damage * 2, ((ModProjectile)this).Projectile.knockBack, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			}
			return;
		}
		switch (ID)
		{
		case 674:
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 18f, 156, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			return;
		case 675:
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 18f, 157, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			return;
		}
		if (!charge)
		{
			if (player.HeldItem.shoot > 1)
			{
				Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 30f, player.HeldItem.shoot, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			}
			return;
		}
		if (!charge)
		{
			return;
		}
		switch (ID)
		{
		case 65:
		{
			for (int l = 0; l < 3; l++)
			{
				Vector2 val4 = Main.MouseWorld + new Vector2((float)Main.rand.Next(-500, 500), (float)Main.rand.Next(-550, -430));
				Projectile obj3 = Projectile.NewProjectileDirect(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), val4, Utils.SafeNormalize(Main.MouseWorld - val4, Vector2.Zero) * 13f, 9, ((ModProjectile)this).Projectile.damage * 2, ((ModProjectile)this).Projectile.knockBack, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
				((Entity)obj3).position = ((Entity)obj3).Center;
				obj3.scale *= 2f;
				((Entity)obj3).width = ((Entity)obj3).width * 2;
				((Entity)obj3).height = ((Entity)obj3).height * 2;
				((Entity)obj3).Center = ((Entity)obj3).position;
			}
			return;
		}
		case 3065:
		{
			for (int j = 0; j < 9; j++)
			{
				Vector2 val3 = Main.MouseWorld + new Vector2((float)Main.rand.Next(-200, 200), (float)Main.rand.Next(-810, -450));
				Projectile obj = Projectile.NewProjectileDirect(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), val3, Utils.SafeNormalize(Main.MouseWorld - val3, Vector2.Zero) * 19f, 503, ((ModProjectile)this).Projectile.damage * 2, ((ModProjectile)this).Projectile.knockBack, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
				((Entity)obj).position = ((Entity)obj).Center;
				obj.scale *= 2f;
				((Entity)obj).width = ((Entity)obj).width * 2;
				((Entity)obj).height = ((Entity)obj).height * 2;
				((Entity)obj).Center = ((Entity)obj).position;
			}
			return;
		}
		case 674:
		{
			for (int k = 1; k < 3; k++)
			{
				Projectile obj2 = Projectile.NewProjectileDirect(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.RotatedByRandom(Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero), 0.25) * 30f, 156, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
				((Entity)obj2).position = ((Entity)obj2).Center;
				obj2.scale *= 2f;
				((Entity)obj2).width = ((Entity)obj2).width * 2;
				((Entity)obj2).height = ((Entity)obj2).height * 2;
				((Entity)obj2).Center = ((Entity)obj2).position;
			}
			return;
		}
		case 675:
		{
			for (int m = 1; m < 3; m++)
			{
				Projectile obj4 = Projectile.NewProjectileDirect(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.RotatedByRandom(Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero), 0.25) * 30f, 157, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
				((Entity)obj4).position = ((Entity)obj4).Center;
				obj4.scale *= 2f;
				((Entity)obj4).width = ((Entity)obj4).width * 2;
				((Entity)obj4).height = ((Entity)obj4).height * 2;
				((Entity)obj4).Center = ((Entity)obj4).position;
			}
			return;
		}
		case 757:
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 48f, 985, ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, (float)((Entity)player).direction * player.gravDir, 45f, 1f);
			return;
		}
		if (player.HeldItem.shoot > 1)
		{
			for (int n = 1; n < 3; n++)
			{
				Projectile obj5 = Projectile.NewProjectileDirect(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.RotatedByRandom(Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero), 0.25) * 30f, player.HeldItem.shoot, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
				((Entity)obj5).position = ((Entity)obj5).Center;
				obj5.scale *= 2f;
				((Entity)obj5).width = ((Entity)obj5).width * 2;
				((Entity)obj5).height = ((Entity)obj5).height * 2;
				((Entity)obj5).Center = ((Entity)obj5).position;
			}
			Projectile obj6 = Projectile.NewProjectileDirect(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 30f, player.HeldItem.shoot, ((ModProjectile)this).Projectile.damage / 2, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			((Entity)obj6).position = ((Entity)obj6).Center;
			obj6.scale *= 2f;
			((Entity)obj6).width = ((Entity)obj6).width * 2;
			((Entity)obj6).height = ((Entity)obj6).height * 2;
			((Entity)obj6).Center = ((Entity)obj6).position;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		Player val = Main.player[((ModProjectile)this).Projectile.owner];
		Texture2D textureMySelf = Tetrad.GetTextureMySelf("MeleeWeaponEffects/Bar");
		Texture2D textureMySelf2 = Tetrad.GetTextureMySelf("MeleeWeaponEffects/BarInside");
		float num = ((((ModProjectile)this).Projectile.ai[1] > ((ModProjectile)this).Projectile.localAI[0] * 3f) ? (((ModProjectile)this).Projectile.localAI[0] * 3f) : ((ModProjectile)this).Projectile.ai[1]);
		if (Weapon != null)
		{
			if (!(((ModProjectile)this).Projectile.ai[0] > 0f))
			{
				Main.EntitySpriteDraw(Weapon, ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition, (Rectangle?)null, Color.White, ((ModProjectile)this).Projectile.rotation + 0.785f, new Vector2(0f, (float)TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value.Height), 1f, (SpriteEffects)0, 0f);
			}
			else
			{
				Main.EntitySpriteDraw(Weapon, ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition, (Rectangle?)null, Color.White, ((ModProjectile)this).Projectile.rotation + 0.785f + 1.5707f, new Vector2((float)TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value.Width, (float)TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value.Height), 1f, (SpriteEffects)1, 0f);
			}
			Main.EntitySpriteDraw(textureMySelf, ((Entity)val).Center + new Vector2((float)(-textureMySelf.Width / 2), -60f) - Main.screenPosition, (Rectangle?)null, Color.White, 0f, new Vector2(0f, 0f), 1f, (SpriteEffects)0, 0f);
			int num2 = (int)(num / (((ModProjectile)this).Projectile.localAI[0] * 3f) * (float)textureMySelf2.Width);
			Color val2 = Color.Lerp(Color.GreenYellow, Color.Orange, num / (((ModProjectile)this).Projectile.localAI[0] * 3f));
			if (num == ((ModProjectile)this).Projectile.localAI[0] * 3f)
			{
				val2 = Color.Lerp(Color.OrangeRed, Color.Yellow, (float)(0.5 + 0.5 * Math.Sin(((ModProjectile)this).Projectile.ai[1] * 0.3f)));
			}
			Main.EntitySpriteDraw(textureMySelf2, ((Entity)val).Center + new Vector2((float)(-textureMySelf.Width / 2), -60f) - Main.screenPosition, (Rectangle?)new Rectangle(0, 0, num2, textureMySelf2.Height), val2, 0f, new Vector2(0f, 0f), 1f, (SpriteEffects)0, 0f);
		}
		return false;
	}
}
