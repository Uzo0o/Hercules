using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Hercules.Converters;

/// <summary>
/// Colors a player's personal-foul count so it's readable at a glance during
/// a broadcast: normal text under 4 fouls, yellow at 4 (one away from fouling
/// out in a standard 5-foul competition), red at 5+ (fouled out). Not aware
/// of a competition's actual foul limit (that lives in the "setup" message,
/// which isn't wired up yet) - 5 is the FIBA default, close enough for a
/// visual cue rather than a rules engine.
/// </summary>
public class FoulCountToBrushConverter : IValueConverter
{
    public static readonly FoulCountToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int fouls = value is int i ? i : 0;

        return fouls switch
        {
            >= 5 => new SolidColorBrush(Color.Parse("#EF4444")), // matches AccentDanger
            4 => new SolidColorBrush(Color.Parse("#F2C94C")),    // matches AccentYellow
            _ => new SolidColorBrush(Color.Parse("#F0EBE6"))     // matches TextPrimary
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}