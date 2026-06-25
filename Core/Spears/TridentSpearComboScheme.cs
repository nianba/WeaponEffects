namespace WeaponEffects.Spears;

public static class TridentSpearComboScheme
{
	public static readonly SpearComboStep[] Steps =
	{
		new(
			segmentIndex: 0,
			kind: SpearComboStepKind.ForwardThrust,
			name: "Forward Thrust",
			activeStart: 0.2f,
			activeEnd: 0.55f,
			reachScale: 1.15f,
			collisionWidth: 22f,
			damageMultiplier: 0.85f,
			timeMultiplier: 0.75f,
			extraUpdates: 4),

		new(
			segmentIndex: 1,
			kind: SpearComboStepKind.RisingLift,
			name: "Rising Lift",
			activeStart: 0.22f,
			activeEnd: 0.6f,
			reachScale: 1.05f,
			collisionWidth: 24f,
			damageMultiplier: 1f,
			timeMultiplier: 0.9f,
			extraUpdates: 4),

		new(
			segmentIndex: 2,
			kind: SpearComboStepKind.Backsweep,
			name: "Around-Body Backsweep",
			activeStart: 0.2f,
			activeEnd: 0.58f,
			reachScale: 1f,
			collisionWidth: 28f,
			damageMultiplier: 0.85f,
			timeMultiplier: 1.05f,
			extraUpdates: 4),

		new(
			segmentIndex: 3,
			kind: SpearComboStepKind.Finisher,
			name: "Branching Finisher",
			activeStart: 0.18f,
			activeEnd: 0.72f,
			reachScale: 1.35f,
			collisionWidth: 32f,
			damageMultiplier: 1.3f,
			timeMultiplier: 1.15f,
			extraUpdates: 4,
			airborneTimeMultiplier: 1.2f)
	};

	public static int Count => Steps.Length;

	public static ref readonly SpearComboStep GetStep(int index)
	{
		return ref Steps[index % Steps.Length];
	}
}
