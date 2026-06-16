namespace IntegratedStrategyEvents.Events;

internal static class IntegratedStrategyRichText
{
	private const int PageDescriptionFontSize = 28;
	private const int OptionDescriptionFontSize = 22;
	private const string DescriptionSuffix = ".description";
	private const string OptionKeySegment = ".options.";

	public static List<(string, string)>? ApplyFontSizes(List<(string, string)>? localization)
	{
		if (localization == null)
		{
			return null;
		}

		List<(string, string)> adjusted = new(localization.Count);
		foreach ((string key, string value) in localization)
		{
			adjusted.Add((key, ApplyFontSize(key, value)));
		}

		return adjusted;
	}

	private static string ApplyFontSize(string key, string value)
	{
		if (!key.EndsWith(DescriptionSuffix, StringComparison.Ordinal))
		{
			return value;
		}

		int fontSize = key.Contains(OptionKeySegment, StringComparison.Ordinal)
			? OptionDescriptionFontSize
			: PageDescriptionFontSize;
		return WrapFontSize(value, fontSize);
	}

	private static string WrapFontSize(string value, int fontSize)
	{
		if (string.IsNullOrWhiteSpace(value) || value.StartsWith("[font_size=", StringComparison.Ordinal))
		{
			return value;
		}

		return $"[font_size={fontSize}]{value}[/font_size]";
	}
}
