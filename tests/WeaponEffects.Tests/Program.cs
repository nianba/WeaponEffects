using System.Numerics;
using WeaponEffects.Spears;

List<(string Name, Action Test)> tests =
[
	("Trident combo has four steps", TridentComboHasFourSteps),
	("Backsweep ends behind and low", BacksweepEndsBehindAndLow),
	("Ground finisher reaches farther than opener", GroundFinisherReachesFartherThanOpener),
	("Air finisher travels overhead then ends forward down", AirFinisherTravelsOverheadThenEndsForwardDown),
	("Branch selection maps grounded and airborne", BranchSelectionMapsGroundedAndAirborne),
	("Spear tip trail alpha stays narrow", SpearTipTrailAlphaStaysNarrow),
	("Spear shaft trail avoids stacked light panels", SpearShaftTrailAvoidsStackedLightPanels),
	("Spear debug draw toggles are wired", SpearDebugDrawTogglesAreWired)
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

static void BranchSelectionMapsGroundedAndAirborne()
{
	AssertEqual(SpearComboBranch.GroundedFinisher, SpearMotion.SelectFinisherBranch(isGrounded: true));
	AssertEqual(SpearComboBranch.AirborneFinisher, SpearMotion.SelectFinisherBranch(isGrounded: false));
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
