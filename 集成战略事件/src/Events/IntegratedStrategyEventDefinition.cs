namespace IntegratedStrategyEvents.Events;

public sealed record IntegratedStrategyEventDefinition(
	string PortraitPath,
	Func<List<(string, string)>?> CreateLocalization,
	IntegratedStrategyEventLayoutProfile? Layout = null,
	bool AlignHoverTipsRight = false)
{
	private const string EventPortraitPathPrefix = "res://IntegratedStrategyEvents/images/events/";

	public static IntegratedStrategyEventDefinition ForEventPortrait(
		string fileName,
		Func<List<(string, string)>?> createLocalization,
		IntegratedStrategyEventLayoutProfile? Layout = null,
		bool AlignHoverTipsRight = false)
	{
		return new IntegratedStrategyEventDefinition(
			EventPortraitPathPrefix + fileName,
			createLocalization,
			Layout,
			AlignHoverTipsRight);
	}
}

public readonly record struct IntegratedStrategyEventLayoutProfile(
	bool LeftAligned,
	float ContentWidthScale,
	float VerticalOffset = 0f,
	int? VerticalOffsetOptionCount = null,
	float HorizontalOffset = 0f)
{
	public static readonly IntegratedStrategyEventLayoutProfile Standard = new(false, 1f);
	public static readonly IntegratedStrategyEventLayoutProfile StandardNarrow = new(false, 0.92f);
	public static readonly IntegratedStrategyEventLayoutProfile StandardNarrowSlightlyShiftedRight = new(false, 0.92f, HorizontalOffset: 80f);
	public static readonly IntegratedStrategyEventLayoutProfile StandardMediumNarrowSlightlyShiftedRight = new(false, 0.86f, HorizontalOffset: 80f);
	public static readonly IntegratedStrategyEventLayoutProfile StandardNarrowShiftedRight = new(false, 0.92f, HorizontalOffset: 220f);
	public static readonly IntegratedStrategyEventLayoutProfile StandardLowered = new(false, 1f, 80f);
	public static readonly IntegratedStrategyEventLayoutProfile StandardRaised = new(false, 1f, -95f, 5);
	public static readonly IntegratedStrategyEventLayoutProfile StandardSlightlyRaisedForFourOptions = new(false, 1f, -70f, 4);
	public static readonly IntegratedStrategyEventLayoutProfile LeftNarrow = Left(0.86f);
	public static readonly IntegratedStrategyEventLayoutProfile LeftWide = Left(0.9f);
	public static readonly IntegratedStrategyEventLayoutProfile LeftMedium = Left(0.82f);
	public static readonly IntegratedStrategyEventLayoutProfile LeftMediumSlightlyRaisedForFourOptions = new(true, 0.82f, -70f, 4);
	public static readonly IntegratedStrategyEventLayoutProfile LeftCompact = Left(0.78f);
	public static readonly IntegratedStrategyEventLayoutProfile LeftVeryCompact = Left(0.62f);

	public static IntegratedStrategyEventLayoutProfile Left(float contentWidthScale, float verticalOffset = 0f)
	{
		return new IntegratedStrategyEventLayoutProfile(true, contentWidthScale, verticalOffset);
	}
}
