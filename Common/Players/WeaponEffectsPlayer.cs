using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WeaponEffects.Spears;

namespace WeaponEffects;

public class WeaponEffectsPlayer : ModPlayer
{
	private const int MaxBladeMomentumStacks = 8;
	private const int BladeMomentumDecayInterval = 30;
	private const int BladeMomentumNpcCooldown = 12;

	public int ScreenShakeTimer;
	public int SlashComboStepIndex;
	public int SpearComboStepIndex;
	public int BladeMomentumStacks;
	public float FourthSlashDamageMultiplier = 1f;
	public float FourthSlashLengthMultiplier = 1f;
	public float ChargeReadyFrameMultiplier = 1f;
	public float ChargeLengthBonusAtHalf;
	public float ChargeLengthBonusAtFull;
	public float ChargeDamageBonusAtHalf;
	public float ChargeDamageBonusAtFull;
	public float ChargeWidthBonusAtFull;
	private int _bladeMomentumDuration;
	private int _bladeMomentumDecayTimer;
	private bool _bladeMomentumAccessoryActive;
	private int _bladeMomentumDurationFrames;
	private float _bladeMomentumAttackSpeedPerStack;
	private float _bladeMomentumCritPerStack;
	private float _bladeMomentumDamagePerStack;
	private int _bladeMomentumAttackSpeedMaxStacks;
	private int _bladeMomentumCritMaxStacks;
	private int _bladeMomentumDamageMaxStacks;
	private int _slashComboResetTimer;
	private int _spearComboResetTimer;
	private readonly int[] _bladeMomentumNpcCooldown = new int[Main.maxNPCs];
	private readonly int[] _shadowFlameRecallHitCount = new int[Main.maxNPCs];
	private readonly int[] _shadowFlameRecallWindowTimer = new int[Main.maxNPCs];
	private readonly bool[] _shadowFlameExplodedThisRecall = new bool[Main.maxNPCs];

	public override void ResetEffects()
	{
		FourthSlashDamageMultiplier = 1f;
		FourthSlashLengthMultiplier = 1f;
		ChargeReadyFrameMultiplier = 1f;
		ChargeLengthBonusAtHalf = 0f;
		ChargeLengthBonusAtFull = 0f;
		ChargeDamageBonusAtHalf = 0f;
		ChargeDamageBonusAtFull = 0f;
		ChargeWidthBonusAtFull = 0f;
		_bladeMomentumAccessoryActive = false;
		_bladeMomentumDurationFrames = 0;
		_bladeMomentumAttackSpeedPerStack = 0f;
		_bladeMomentumCritPerStack = 0f;
		_bladeMomentumDamagePerStack = 0f;
		_bladeMomentumAttackSpeedMaxStacks = 0;
		_bladeMomentumCritMaxStacks = 0;
		_bladeMomentumDamageMaxStacks = 0;
	}

	public int ConsumeNextSlashComboStep()
	{
		int index = SlashComboStepIndex;
		SlashComboStepIndex = (SlashComboStepIndex + 1) % Compact3DComboSchemeA.Count;
		_slashComboResetTimer = ModContent.GetInstance<WeaponEffectsGameplayConfig>().ComboResetDelay;
		return index;
	}

	public int ConsumeNextSpearComboStep()
	{
		int index = SpearComboStepIndex;
		SpearComboStepIndex = (SpearComboStepIndex + 1) % TridentSpearComboScheme.Count;
		_spearComboResetTimer = ModContent.GetInstance<WeaponEffectsGameplayConfig>().ComboResetDelay;
		return index;
	}

	public void ResetSpearCombo()
	{
		SpearComboStepIndex = 0;
		_spearComboResetTimer = 0;
	}

	public void StartShadowFlameRecallSession()
	{
		Array.Clear(_shadowFlameRecallHitCount);
		Array.Clear(_shadowFlameRecallWindowTimer);
		Array.Clear(_shadowFlameExplodedThisRecall);
	}

	public void RegisterHeavySlash(float damageMultiplier, float lengthMultiplier)
	{
		FourthSlashDamageMultiplier = Math.Max(FourthSlashDamageMultiplier, damageMultiplier);
		FourthSlashLengthMultiplier = Math.Max(FourthSlashLengthMultiplier, lengthMultiplier);
	}

	public void RegisterBladeMomentum(
		int durationFrames,
		float attackSpeedPerStack = 0f,
		int attackSpeedMaxStacks = 0,
		float critPerStack = 0f,
		int critMaxStacks = 0,
		float damagePerStack = 0f,
		int damageMaxStacks = 0)
	{
		_bladeMomentumAccessoryActive = true;
		_bladeMomentumDurationFrames = Math.Max(_bladeMomentumDurationFrames, durationFrames);

		if (attackSpeedPerStack > 0f && attackSpeedMaxStacks > 0)
		{
			_bladeMomentumAttackSpeedPerStack = Math.Max(_bladeMomentumAttackSpeedPerStack, attackSpeedPerStack);
			_bladeMomentumAttackSpeedMaxStacks = Math.Max(_bladeMomentumAttackSpeedMaxStacks, attackSpeedMaxStacks);
		}

		if (critPerStack > 0f && critMaxStacks > 0)
		{
			_bladeMomentumCritPerStack = Math.Max(_bladeMomentumCritPerStack, critPerStack);
			_bladeMomentumCritMaxStacks = Math.Max(_bladeMomentumCritMaxStacks, critMaxStacks);
		}

		if (damagePerStack > 0f && damageMaxStacks > 0)
		{
			_bladeMomentumDamagePerStack = Math.Max(_bladeMomentumDamagePerStack, damagePerStack);
			_bladeMomentumDamageMaxStacks = Math.Max(_bladeMomentumDamageMaxStacks, damageMaxStacks);
		}
	}

	public bool TryGainBladeMomentum(Projectile sourceProjectile, NPC target)
	{
		if (!_bladeMomentumAccessoryActive || sourceProjectile == null || target == null)
		{
			return false;
		}

		int npcIndex = target.whoAmI;
		if (npcIndex < 0 || npcIndex >= Main.maxNPCs || _bladeMomentumNpcCooldown[npcIndex] > 0)
		{
			return false;
		}

		BladeMomentumStacks = Math.Min(MaxBladeMomentumStacks, BladeMomentumStacks + 1);
		_bladeMomentumDuration = Math.Max(_bladeMomentumDuration, _bladeMomentumDurationFrames);
		_bladeMomentumDecayTimer = BladeMomentumDecayInterval;
		_bladeMomentumNpcCooldown[npcIndex] = BladeMomentumNpcCooldown;
		return true;
	}

	public void RegisterChargeAccessory(
		float readyFrameMultiplier,
		float lengthBonusAtHalf,
		float lengthBonusAtFull,
		float damageBonusAtHalf,
		float damageBonusAtFull,
		float widthBonusAtFull)
	{
		ChargeReadyFrameMultiplier = Math.Min(ChargeReadyFrameMultiplier, Math.Clamp(readyFrameMultiplier, 0.01f, 1f));
		ChargeLengthBonusAtHalf = Math.Max(ChargeLengthBonusAtHalf, lengthBonusAtHalf);
		ChargeLengthBonusAtFull = Math.Max(ChargeLengthBonusAtFull, lengthBonusAtFull);
		ChargeDamageBonusAtHalf = Math.Max(ChargeDamageBonusAtHalf, damageBonusAtHalf);
		ChargeDamageBonusAtFull = Math.Max(ChargeDamageBonusAtFull, damageBonusAtFull);
		ChargeWidthBonusAtFull = Math.Max(ChargeWidthBonusAtFull, widthBonusAtFull);
	}

	public override void UpdateEquips()
	{
		ApplyBladeMomentumStats();
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
			SpearGlobalItem.TryStartThrowChargeInterrupt(Player);
		}

		if (_slashComboResetTimer <= 0)
		{
			SlashComboStepIndex = 0;
		}
		else
		{
			_slashComboResetTimer--;
		}

		if (_spearComboResetTimer <= 0)
		{
			SpearComboStepIndex = 0;
		}
		else
		{
			_spearComboResetTimer--;
		}

		UpdateBladeMomentum();
		UpdateShadowFlameRecallWindows();
	}

	private void ApplyBladeMomentumStats()
	{
		if (!_bladeMomentumAccessoryActive || BladeMomentumStacks <= 0)
		{
			return;
		}

		if (_bladeMomentumAttackSpeedPerStack > 0f && _bladeMomentumAttackSpeedMaxStacks > 0)
		{
			int stacks = Math.Min(BladeMomentumStacks, _bladeMomentumAttackSpeedMaxStacks);
			Player.GetAttackSpeed(DamageClass.Melee) += _bladeMomentumAttackSpeedPerStack * stacks;
		}

		if (_bladeMomentumCritPerStack > 0f && _bladeMomentumCritMaxStacks > 0)
		{
			int stacks = Math.Min(BladeMomentumStacks, _bladeMomentumCritMaxStacks);
			Player.GetCritChance(DamageClass.Melee) += _bladeMomentumCritPerStack * stacks;
		}

		if (_bladeMomentumDamagePerStack > 0f && _bladeMomentumDamageMaxStacks > 0)
		{
			int stacks = Math.Min(BladeMomentumStacks, _bladeMomentumDamageMaxStacks);
			Player.GetDamage(DamageClass.Melee) += _bladeMomentumDamagePerStack * stacks;
		}
	}

	private void UpdateBladeMomentum()
	{
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			if (_bladeMomentumNpcCooldown[i] > 0)
			{
				_bladeMomentumNpcCooldown[i]--;
			}
		}

		if (!_bladeMomentumAccessoryActive)
		{
			BladeMomentumStacks = 0;
			_bladeMomentumDuration = 0;
			_bladeMomentumDecayTimer = 0;
			return;
		}

		if (BladeMomentumStacks <= 0)
		{
			_bladeMomentumDuration = 0;
			_bladeMomentumDecayTimer = 0;
			return;
		}

		if (_bladeMomentumDuration > 0)
		{
			_bladeMomentumDuration--;
			return;
		}

		if (_bladeMomentumDecayTimer > 0)
		{
			_bladeMomentumDecayTimer--;
			return;
		}

		BladeMomentumStacks--;
		_bladeMomentumDecayTimer = BladeMomentumDecayInterval;
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
