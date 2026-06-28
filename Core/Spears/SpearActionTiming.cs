namespace WeaponEffects.Spears;

public readonly struct SpearActionTiming
{
	public readonly float ActiveStart;
	public readonly float ActiveEnd;
	public readonly float WindupEnd;
	public readonly float AttackEnd;
	public readonly float SwingSoundProgress;

	public SpearActionTiming(
		float activeStart,
		float activeEnd,
		float windupEnd,
		float attackEnd,
		float swingSoundProgress)
	{
		ActiveStart = activeStart;
		ActiveEnd = activeEnd;
		WindupEnd = windupEnd;
		AttackEnd = attackEnd;
		SwingSoundProgress = swingSoundProgress;
	}
}
