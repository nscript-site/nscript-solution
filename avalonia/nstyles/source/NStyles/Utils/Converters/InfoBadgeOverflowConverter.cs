﻿using Avalonia.Data.Converters;
using System.Globalization;

namespace NStyles.Utils.Converters;

public class InfoBadgeOverflowConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var overflowSuffix = parameter as string ?? "+";
        if (!double.TryParse(values[0]?.ToString(), out var headerValue) || values[1] is not (int overflowValue and > 0))
        {
            return values[0];
        }

        if (headerValue > overflowValue)
        {
            return overflowValue + overflowSuffix;
        }

        return values[0];
    }
}
