using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace MeleeWeaponEffects;

public class ExampleSlash : ModProjectile
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

	public void VanillaStyleShoots(bool charge, int ID, Vector2 position, Player player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_074b: Unknown result type (might be due to invalid IL or missing references)
		//IL_077f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0850: Unknown result type (might be due to invalid IL or missing references)
		//IL_0855: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		//IL_0866: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0879: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_090c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0932: Unknown result type (might be due to invalid IL or missing references)
		//IL_0937: Unknown result type (might be due to invalid IL or missing references)
		//IL_093e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_0948: Unknown result type (might be due to invalid IL or missing references)
		//IL_094d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0957: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e0: Unknown result type (might be due to invalid IL or missing references)
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
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 18f, 156, ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			return;
		case 675:
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 18f, 157, ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, 0f, 0f, 0f);
			return;
		case 757:
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 48f, 985, ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, (float)((Entity)player).direction * player.gravDir, 45f, 1f);
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
			Projectile.NewProjectile(((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), ((Entity)player).Center, Utils.SafeNormalize(Main.MouseWorld - ((Entity)player).Center, Vector2.Zero) * 48f, 985, ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack / 2f, ((ModProjectile)this).Projectile.owner, (float)((Entity)player).direction * player.gravDir, 45f, 2f);
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

	public override void AI()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)((ModProjectile)this).Projectile.localAI[0];
		((ModProjectile)this).Projectile.ai[1] += 1f;
		Player val = Main.player[((ModProjectile)this).Projectile.owner];
		int num2 = 2;
		val.itemAnimation = num;
		val.itemTime = num;
		val.heldProj = ((Entity)((ModProjectile)this).Projectile).whoAmI;
		((ModProjectile)this).Projectile.rotation = ((ModProjectile)this).Projectile.ai[0] + Utils.ToRotation(Main.MouseWorld - ((Entity)val).Center);
		if (((ModProjectile)this).Projectile.timeLeft > num2)
		{
			((ModProjectile)this).Projectile.timeLeft = num2;
		}
		if (val.channel)
		{
			((ModProjectile)this).Projectile.timeLeft = 2;
		}
		Projectile projectile = ((ModProjectile)this).Projectile;
		((Entity)projectile).velocity = ((Entity)projectile).velocity * 0f;
		((Entity)((ModProjectile)this).Projectile).Center = ((Entity)val).Center;
		if (((ModProjectile)this).Projectile.ai[1] % (float)num == 2f)
		{
			VanillaStyleShoots(charge: false, val.HeldItem.type, ((Entity)val).Center, val);
			float num3 = ((ModProjectile)this).Projectile.localAI[1];
			SoundStyle val2 = new SoundStyle("MeleeWeaponEffects/Sounds/S2") { Volume = 0.36f };
			SoundEngine.PlaySound(val2);
			((Entity)val).direction = Math.Sign(Main.MouseWorld.X - ((Entity)val).Center.X);
			HeroSlashMethod.Slash(IsPlayerProj: true, ((Entity)((ModProjectile)this).Projectile).GetSource_FromAI((string)null), Utils.ToRotation(Main.MouseWorld - ((Entity)val).Center) + Utils.NextFloat(Main.rand, -0.5f, 0.5f), Utils.NextBool(Main.rand, 2) ? (-2) : 2, (float)Main.rand.Next(160, 220) / 110f * num3, 0.5f, Utils.NextFloat(Main.rand, 0.36f, 0.8f), Main.rand.Next(4, 6), ((ModProjectile)this).Projectile.damage, ((ModProjectile)this).Projectile.knockBack, ((Entity)val).whoAmI, 0, Color.White, Weapon, 0, ((ModProjectile)this).Projectile.rotation - ((ModProjectile)this).Projectile.ai[0], num3);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false;
	}
}
