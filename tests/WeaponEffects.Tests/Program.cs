using System.Numerics;
using WeaponEffects.Spears;

List<(string Name, Action Test)> tests =
[
	("Trident combo has four steps", TridentComboHasFourSteps),
	("Grip stays on facing hand side while aiming up", GripStaysOnFacingHandSideWhileAimingUp),
	("Backsweep ends behind and low", BacksweepEndsBehindAndLow),
	("Ground finisher reaches farther than opener", GroundFinisherReachesFartherThanOpener),
	("Air finisher travels overhead then ends forward down", AirFinisherTravelsOverheadThenEndsForwardDown),
	("Branch selection uses grounded finisher for grounded and airborne", BranchSelectionUsesGroundedFinisherForGroundedAndAirborne),
	("Spear tip trail alpha stays narrow", SpearTipTrailAlphaStaysNarrow),
	("Spear shaft trail avoids stacked light panels", SpearShaftTrailAvoidsStackedLightPanels),
	("Held spear draw anchors at grip", HeldSpearDrawAnchorsAtGrip),
	("Spear debug draw toggles are wired", SpearDebugDrawTogglesAreWired),
	("Spear throw charge rejects sub-minimum release", SpearThrowChargeRejectsSubMinimumRelease),
	("Spear throw damage scales linearly", SpearThrowDamageScalesLinearly),
	("Spear throw range scales linearly by screen width", SpearThrowRangeScalesLinearlyByScreenWidth),
	("Spear throw attack speed compresses only full charge", SpearThrowAttackSpeedCompressesOnlyFullCharge),
	("Spear throw right-click item entry is wired", SpearThrowRightClickItemEntryIsWired),
	("Spear combo reset method is exposed", SpearComboResetMethodIsExposed)
];

int failed = 0;
foreach ((string name, Action test) in tests)
{
	try
	{
		test();
		Console.WriteLine($"PASS {name}");
	}
	catch (Exception ex)
	{
		failed++;
		Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
	}
}

if (failed > 0)
{
	Environment.Exit(1);
}

static void TridentComboHasFourSteps()
{
	AssertEqual(4, TridentSpearComboScheme.Count);
	AssertEqual(SpearComboStepKind.ForwardThrust, TridentSpearComboScheme.GetStep(0).Kind);
	AssertEqual(SpearComboStepKind.RisingLift, TridentSpearComboScheme.GetStep(1).Kind);
	AssertEqual(SpearComboStepKind.Backsweep, TridentSpearComboScheme.GetStep(2).Kind);
	AssertEqual(SpearComboStepKind.Finisher, TridentSpearComboScheme.GetStep(3).Kind);
}

static void GripStaysOnFacingHandSideWhileAimingUp()
{
	SpearComboStep step = TridentSpearComboScheme.GetStep(0);
	Vector2 ownerCenter = Vector2.Zero;
	float aimUpRight = -MathF.PI * 0.42f;
	float aimUpLeft = MathF.PI + MathF.PI * 0.42f;

	SpearPoseSnapshot rightFacingPose = SpearMotion.EvaluatePose(step, SpearComboBranch.None, ownerCenter, aimUpRight, weaponLength: 100f, progress: 0.5f);
	SpearPoseSnapshot leftFacingPose = SpearMotion.EvaluatePose(step, SpearComboBranch.None, ownerCenter, aimUpLeft, weaponLength: 100f, progress: 0.5f);

	AssertTrue(rightFacingPose.Grip.X > ownerCenter.X + 6f, $"right-facing grip should stay on the player's right hand side while aiming up, got grip={rightFacingPose.Grip}");
	AssertTrue(leftFacingPose.Grip.X < ownerCenter.X - 6f, $"left-facing grip should stay on the player's left hand side while aiming up, got grip={leftFacingPose.Grip}");
}

static void BacksweepEndsBehindAndLow()
{
	SpearComboStep step = TridentSpearComboScheme.GetStep(2);
	SpearPoseSnapshot pose = SpearMotion.EvaluatePose(step, SpearComboBranch.None, Vector2.Zero, 0f, weaponLength: 100f, progress: 1f);

	AssertTrue(pose.Tip.X < pose.Grip.X - 40f, $"expected tip behind grip, got grip={pose.Grip}, tip={pose.Tip}");
	AssertTrue(pose.Tip.Y > pose.Grip.Y + 15f, $"expected tip below grip in screen coordinates, got grip={pose.Grip}, tip={pose.Tip}");
}

static void GroundFinisherReachesFartherThanOpener()
{
	SpearPoseSnapshot opener = SpearMotion.EvaluatePose(TridentSpearComboScheme.GetStep(0), SpearComboBranch.None, Vector2.Zero, 0f, 100f, 1f);
	SpearPoseSnapshot finisher = SpearMotion.EvaluatePose(TridentSpearComboScheme.GetStep(3), SpearComboBranch.GroundedFinisher, Vector2.Zero, 0f, 100f, 1f);

	AssertTrue(finisher.Tip.X > opener.Tip.X + 15f, $"expected grounded finisher reach beyond opener, opener={opener.Tip}, finisher={finisher.Tip}");
}

static void AirFinisherTravelsOverheadThenEndsForwardDown()
{
	SpearComboStep finisherStep = TridentSpearComboScheme.GetStep(3);
	SpearPoseSnapshot mid = SpearMotion.EvaluatePose(finisherStep, SpearComboBranch.AirborneFinisher, Vector2.Zero, 0f, 100f, 0.45f);
	SpearPoseSnapshot end = SpearMotion.EvaluatePose(finisherStep, SpearComboBranch.AirborneFinisher, Vector2.Zero, 0f, 100f, 1f);

	AssertTrue(mid.Tip.Y < mid.Grip.Y - 45f, $"expected mid-air finisher over grip, got grip={mid.Grip}, tip={mid.Tip}");
	AssertTrue(end.Tip.X > end.Grip.X + 45f, $"expected air finisher to end forward, got grip={end.Grip}, tip={end.Tip}");
	AssertTrue(end.Tip.Y > end.Grip.Y + 20f, $"expected air finisher to end down, got grip={end.Grip}, tip={end.Tip}");
}

static void BranchSelectionUsesGroundedFinisherForGroundedAndAirborne()
{
	AssertEqual(SpearComboBranch.GroundedFinisher, SpearMotion.SelectFinisherBranch(isGrounded: true));
	AssertEqual(SpearComboBranch.GroundedFinisher, SpearMotion.SelectFinisherBranch(isGrounded: false));
}

static void SpearTipTrailAlphaStaysNarrow()
{
	string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Textures", "SpearTipTrail.png");
	using FileStream stream = File.OpenRead(path);
	byte[] signature = new byte[8];
	ReadExact(stream, signature);
	byte[] pngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
	AssertTrue(signature.SequenceEqual(pngSignature), "SpearTipTrail.png must be a PNG file");

	(byte colorType, byte bitDepth, byte[] imageData, int width, int height) = ReadPng(stream);
	AssertEqual(6, (int)colorType);
	AssertEqual(8, (int)bitDepth);

	byte[] raw = DecompressZlib(imageData);
	int stride = width * 4;
	int bytesPerRow = stride + 1;
	byte[] previous = new byte[stride];
	byte[] current = new byte[stride];
	int alphaOver8 = 0;
	int alphaOver128 = 0;
	int minY128 = height;
	int maxY128 = -1;

	for (int y = 0; y < height; y++)
	{
		int rowStart = y * bytesPerRow;
		byte filter = raw[rowStart];
		Array.Copy(raw, rowStart + 1, current, 0, stride);
		UnfilterRow(current, previous, filter, bytesPerPixel: 4);

		for (int x = 0; x < width; x++)
		{
			byte alpha = current[x * 4 + 3];
			if (alpha > 8)
			{
				alphaOver8++;
			}

			if (alpha > 128)
			{
				alphaOver128++;
				minY128 = Math.Min(minY128, y);
				maxY128 = Math.Max(maxY128, y);
			}
		}

		(previous, current) = (current, previous);
	}

	int totalPixels = width * height;
	AssertTrue(alphaOver8 < totalPixels / 3, $"low alpha coverage is too broad: {alphaOver8}/{totalPixels}");
	AssertTrue(alphaOver128 > 0, "expected a visible high-alpha spear-tip streak");
	AssertTrue(maxY128 - minY128 + 1 <= 40, $"high-alpha trail is too tall: {maxY128 - minY128 + 1}px");
}

static void SpearShaftTrailAvoidsStackedLightPanels()
{
	string path = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearTrailGlowProjectile.cs");
	string source = File.ReadAllText(path);

	AssertTrue(!source.Contains("private const int TrailSamples = 9;"), "nine full trail samples stack into a rectangular light panel");
	AssertTrue(!source.Contains("fade * (airFinisher ? 0.34f : 0.22f)"), "shaft trail opacity is too high for a full-length rectangular texture");
	AssertTrue(!source.Contains("float width = airFinisher ? 18f : 9f;"), "shaft trail is too wide when SlashTex is stretched over the whole spear");
}

static void HeldSpearDrawAnchorsAtGrip()
{
	string path = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearStrikeProjectile.cs");
	string source = File.ReadAllText(path);

	AssertTrue(!source.Contains("XnaVector2 drawPosition = (pose.Grip + pose.Tip) * 0.5f - Main.screenPosition;"), "held spear must not be drawn from the shaft midpoint because backsweep visually detaches from the player's hand");
	AssertTrue(source.Contains("XnaVector2 drawPosition = pose.Grip - Main.screenPosition;"), "held spear draw position should use the evaluated grip as the visual anchor");
	AssertTrue(source.Contains("HeldSpearGripOrigin(weaponTexture)"), "held spear sprite origin should represent the texture grip point, not the texture center");
}

static void SpearDebugDrawTogglesAreWired()
{
	string configPath = Path.Combine(AppContext.BaseDirectory, "Common", "Configs", "WeaponEffectsConfig.cs");
	string trailPath = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearTrailGlowProjectile.cs");
	string strikePath = Path.Combine(AppContext.BaseDirectory, "Content", "Projectiles", "SpearStrikeProjectile.cs");

	string config = File.ReadAllText(configPath);
	string trail = File.ReadAllText(trailPath);
	string strike = File.ReadAllText(strikePath);

	foreach (string toggle in new[] { "DrawSpearTipTrail", "DrawSpearShaftTrail", "DrawSpearHeldWeapon", "DrawSpearHitFlash" })
	{
		AssertTrue(config.Contains($"public bool {toggle} = true;"), $"missing visual config toggle {toggle}");
	}

	AssertTrue(trail.Contains("DrawSpearTipTrail"), "tip trail toggle must be read by SpearTrailGlowProjectile");
	AssertTrue(trail.Contains("DrawSpearShaftTrail"), "shaft trail toggle must be read by SpearTrailGlowProjectile");
	AssertTrue(strike.Contains("DrawSpearHeldWeapon"), "held weapon toggle must be read by SpearStrikeProjectile");
	AssertTrue(strike.Contains("DrawSpearHitFlash"), "hit flash toggle must be read by SpearStrikeProjectile");
}

static void SpearThrowChargeRejectsSubMinimumRelease()
{
	AssertEqual(60, SpearThrowChargeMath.MinimumChargeFrames);
	AssertTrue(!SpearThrowChargeMath.IsChargeValid(59), "59 frames should cancel the throw");
	AssertTrue(SpearThrowChargeMath.IsChargeValid(60), "60 frames should be the first valid release frame");
}

static void SpearThrowDamageScalesLinearly()
{
	int fullChargeFrames = SpearThrowChargeMath.BaseFullChargeFrames;
	AssertApproximately(0f, SpearThrowChargeMath.ChargeProgress(60, fullChargeFrames), 0.0001f);
	AssertApproximately(0.5f, SpearThrowChargeMath.ChargeProgress(180, fullChargeFrames), 0.0001f);
	AssertApproximately(1f, SpearThrowChargeMath.ChargeProgress(300, fullChargeFrames), 0.0001f);
	AssertApproximately(1f, SpearThrowChargeMath.DamageMultiplier(0f), 0.0001f);
	AssertApproximately(5.5f, SpearThrowChargeMath.DamageMultiplier(0.5f), 0.0001f);
	AssertApproximately(10f, SpearThrowChargeMath.DamageMultiplier(1f), 0.0001f);
}

static void SpearThrowRangeScalesLinearlyByScreenWidth()
{
	const float screenWidth = 1920f;
	AssertApproximately(2880f, SpearThrowChargeMath.TravelDistancePixels(0f, screenWidth), 0.001f);
	AssertApproximately(6240f, SpearThrowChargeMath.TravelDistancePixels(0.5f, screenWidth), 0.001f);
	AssertApproximately(9600f, SpearThrowChargeMath.TravelDistancePixels(1f, screenWidth), 0.001f);
}

static void SpearThrowAttackSpeedCompressesOnlyFullCharge()
{
	AssertEqual(300, SpearThrowChargeMath.EffectiveFullChargeFrames(1f));
	AssertEqual(150, SpearThrowChargeMath.EffectiveFullChargeFrames(2f));
	AssertEqual(120, SpearThrowChargeMath.EffectiveFullChargeFrames(4f));
	AssertEqual(60, SpearThrowChargeMath.MinimumChargeFrames);
}

static void SpearThrowRightClickItemEntryIsWired()
{
	string itemPath = Path.Combine(AppContext.BaseDirectory, "Content", "Items", "SpearGlobalItem.cs");
	string source = File.ReadAllText(itemPath);

	AssertTrue(source.Contains("public override bool AltFunctionUse(Item item, Player player)"), "SpearGlobalItem must expose right-click use");
	AssertTrue(source.Contains("player.altFunctionUse == 2"), "SpearGlobalItem.UseItem must branch right-click use");
	AssertTrue(source.Contains("StartSpearThrowCharge(item, player);"), "right-click use must start the spear throw charge controller");
	AssertTrue(source.Contains("KillOwnedSpearChannels(player);"), "right-click charge must interrupt active spear channels");
	AssertTrue(source.Contains("ResetSpearCombo()"), "right-click charge must reset the spear combo");
}

static void SpearComboResetMethodIsExposed()
{
	string playerPath = Path.Combine(AppContext.BaseDirectory, "Common", "Players", "WeaponEffectsPlayer.cs");
	string source = File.ReadAllText(playerPath);

	AssertTrue(source.Contains("public void ResetSpearCombo()"), "WeaponEffectsPlayer should expose a spear combo reset method");
	AssertTrue(source.Contains("SpearGlobalItem.TryStartThrowChargeInterrupt(Player);"), "local player update should allow right-click interruption during active spear channels");
}

static (byte ColorType, byte BitDepth, byte[] ImageData, int Width, int Height) ReadPng(Stream stream)
{
	int width = 0;
	int height = 0;
	byte colorType = 0;
	byte bitDepth = 0;
	List<byte> imageData = [];

	while (stream.Position < stream.Length)
	{
		int length = ReadBigEndianInt32FromStream(stream);
		byte[] typeBytes = new byte[4];
		ReadExact(stream, typeBytes);
		string type = System.Text.Encoding.ASCII.GetString(typeBytes);
		byte[] data = new byte[length];
		ReadExact(stream, data);
		stream.Position += 4;

		if (type == "IHDR")
		{
			width = ReadBigEndianInt32FromBytes(data, 0);
			height = ReadBigEndianInt32FromBytes(data, 4);
			bitDepth = data[8];
			colorType = data[9];
		}
		else if (type == "IDAT")
		{
			imageData.AddRange(data);
		}
		else if (type == "IEND")
		{
			break;
		}
	}

	return (colorType, bitDepth, imageData.ToArray(), width, height);
}

static byte[] DecompressZlib(byte[] data)
{
	using MemoryStream input = new(data);
	input.Position = 2;
	using System.IO.Compression.DeflateStream deflate = new(input, System.IO.Compression.CompressionMode.Decompress);
	using MemoryStream output = new();
	deflate.CopyTo(output);
	return output.ToArray();
}

static void UnfilterRow(byte[] row, byte[] previous, byte filter, int bytesPerPixel)
{
	for (int i = 0; i < row.Length; i++)
	{
		int left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
		int up = previous[i];
		int upLeft = i >= bytesPerPixel ? previous[i - bytesPerPixel] : 0;
		int value = filter switch
		{
			0 => row[i],
			1 => row[i] + left,
			2 => row[i] + up,
			3 => row[i] + ((left + up) >> 1),
			4 => row[i] + Paeth(left, up, upLeft),
			_ => throw new InvalidOperationException($"unsupported PNG filter {filter}")
		};
		row[i] = unchecked((byte)value);
	}
}

static int Paeth(int left, int up, int upLeft)
{
	int p = left + up - upLeft;
	int pa = Math.Abs(p - left);
	int pb = Math.Abs(p - up);
	int pc = Math.Abs(p - upLeft);
	if (pa <= pb && pa <= pc)
	{
		return left;
	}

	return pb <= pc ? up : upLeft;
}

static int ReadBigEndianInt32FromStream(Stream stream)
{
	byte[] bytes = new byte[4];
	ReadExact(stream, bytes);
	return ReadBigEndianInt32FromBytes(bytes, 0);
}

static int ReadBigEndianInt32FromBytes(byte[] bytes, int offset)
{
	return bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 3];
}

static void ReadExact(Stream stream, byte[] buffer)
{
	int offset = 0;
	while (offset < buffer.Length)
	{
		int read = stream.Read(buffer, offset, buffer.Length - offset);
		if (read == 0)
		{
			throw new EndOfStreamException();
		}

		offset += read;
	}
}

static void AssertTrue(bool condition, string message)
{
	if (!condition)
	{
		throw new InvalidOperationException(message);
	}
}

static void AssertEqual<T>(T expected, T actual)
{
	if (!EqualityComparer<T>.Default.Equals(expected, actual))
	{
		throw new InvalidOperationException($"expected {expected}, got {actual}");
	}
}

static void AssertApproximately(float expected, float actual, float tolerance)
{
	if (MathF.Abs(expected - actual) > tolerance)
	{
		throw new InvalidOperationException($"expected {expected}, got {actual}");
	}
}
