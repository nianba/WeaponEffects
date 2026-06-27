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
			timeMultiplier: 0.65f,
			extraUpdates: 4),

		new(
			segmentIndex: 1,
			kind: SpearComboStepKind.RisingLift,
			name: "Rising Lift",
			activeStart: 0.42f,
			activeEnd: 0.76f,
			reachScale: 1.05f,
			collisionWidth: 24f,
			damageMultiplier: 2.3f,
			timeMultiplier: 1.8f,
			extraUpdates: 4,
			swingSoundDelay: 0.08f),

		new(
			segmentIndex: 2,
			kind: SpearComboStepKind.Backsweep,
			name: "Around-Body Backsweep",
			activeStart: 0.38f,
			activeEnd: 0.78f,
			reachScale: 1f,
			collisionWidth: 28f,
			damageMultiplier: 2.3f,
			timeMultiplier: 1.8f,
			extraUpdates: 4,
			swingSoundDelay: 0.08f),

		new(
			segmentIndex: 3,
			kind: SpearComboStepKind.Finisher,
			name: "Branching Finisher",
			activeStart: 0.37f,
			activeEnd: 0.86f,
			reachScale: 1.35f,
			collisionWidth: 32f,
			damageMultiplier: 3f,
			timeMultiplier: 2.3f,
			extraUpdates: 4,
			swingSoundDelay: 0.08f,
			airborneTimeMultiplier: 1.2f)
	};

	public static int Count => Steps.Length;

	public static ref readonly SpearComboStep GetStep(int index)
	{
		return ref Steps[index % Steps.Length];
	}
}
