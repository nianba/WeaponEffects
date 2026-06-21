using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponEffects;

public class WeaponEffectsPlayer : ModPlayer
{
	public int ScreenShakeTimer;
	public int SlashComboStepIndex;
	private int _slashComboResetTimer;
	private readonly int[] _shadowFlameRecallHitCount = new int[Main.maxNPCs];
	private readonly int[] _shadowFlameRecallWindowTimer = new int[Main.maxNPCs];
	private readonly bool[] _shadowFlameExplodedThisRecall = new bool[Main.maxNPCs];

	public int ConsumeNextSlashComboStep()
	{
		int index = SlashComboStepIndex;
		SlashComboStepIndex = (SlashComboStepIndex + 1) % Compact3DComboSchemeA.Count;
		_slashComboResetTimer = ModContent.GetInstance<WeaponEffectsGameplayConfig>().ComboResetDelay;
		return index;
	}

	public void StartShadowFlameRecallSession()
	{
		Array.Clear(_shadowFlameRecallHitCount);
		Array.Clear(_shadowFlameRecallWindowTimer);
		Array.Clear(_shadowFlameExplodedThisRecall);
	}

	public void RegisterShadowFlameRecallHit(NPC target, Projectile recallProjectile, int explosionDamage)
	{
		int npcIndex = target.whoAmI;
		if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
		{
			return;
		}

		if (_shadowFlameRecallWindowTimer[npcIndex] <= 0)
		{
			_shadowFlameRecallHitCount[npcIndex] = 0;
		}

		_shadowFlameRecallHitCount[npcIndex]++;
		_shadowFlameRecallWindowTimer[npcIndex] = ShadowFlameKnifeTuning.ExplosionWindowTicks;

		if (_shadowFlameRecallHitCount[npcIndex] < ShadowFlameKnifeTuning.ExplosionRequiredHits)
		{
			return;
		}

		if (_shadowFlameExplodedThisRecall[npcIndex])
		{
			return;
		}

		_shadowFlameExplodedThisRecall[npcIndex] = true;
		ShadowFlameExplosionProjectile.Spawn(
			recallProjectile.GetSource_FromAI(),
			target.Center,
			recallProjectile.owner,
			explosionDamage,
			recallProjectile.knockBack * 1.2f);
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
			TryRecallShadowFlameKnifeInterrupt();
			SlashGlobalItem.TryStartChargeInterrupt(Player);
		}

		if (_slashComboResetTimer <= 0)
		{
			SlashComboStepIndex = 0;
		}
		else
		{
			_slashComboResetTimer--;
		}

		UpdateShadowFlameRecallWindows();
	}

	private bool TryRecallShadowFlameKnifeInterrupt()
	{
		if (!CanRecallShadowFlameKnifeFromInput())
		{
			return false;
		}

		Item item = Player.HeldItem;
		ShadowFlameKnifeHelper.RecallAll(Player, Player.GetSource_ItemUse(item), Player.GetWeaponKnockback(item));
		Main.mouseRightRelease = false;
		Player.itemAnimation = 0;
		Player.itemTime = 0;
		return true;
	}

	private bool CanRecallShadowFlameKnifeFromInput()
	{
		if (!Main.mouseRight || !Main.mouseRightRelease)
		{
			return false;
		}

		if (!Player.active || Player.dead || Player.noItems || Player.CCed)
		{
			return false;
		}

		if (Player.mouseInterface || Main.playerInventory || Main.mapFullscreen || Main.drawingPlayerChat || Main.editSign || Main.editChest || Main.blockInput)
		{
			return false;
		}

		Item item = Player.HeldItem;
		if (item == null || item.IsAir || item.type != ItemID.ShadowFlameKnife)
		{
			return false;
		}

		return Player.itemAnimation > 0 || Player.itemTime > 0;
	}

	private void UpdateShadowFlameRecallWindows()
	{
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			if (_shadowFlameRecallWindowTimer[i] <= 0)
			{
				continue;
			}

			_shadowFlameRecallWindowTimer[i]--;
			if (_shadowFlameRecallWindowTimer[i] <= 0)
			{
				_shadowFlameRecallHitCount[i] = 0;
			}
		}
	}
}
