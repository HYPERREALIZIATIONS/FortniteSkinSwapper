using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FortniteSwapper.Converters;

public class RarityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var r = (value as string)?.ToLowerInvariant() ?? string.Empty;
        return r switch
        {
            "legendary" => new SolidColorBrush(Color.FromRgb(0xF2, 0xC9, 0x4C)),
            "epic" or "marvel" or "dc" or "icon" => new SolidColorBrush(Color.FromRgb(0x9B, 0x4D, 0xCA)),
            "rare" => new SolidColorBrush(Color.FromRgb(0x4F, 0xB9, 0xF6)),
            "uncommon" => new SolidColorBrush(Color.FromRgb(0x5D, 0xD6, 0x57)),
            "common" => new SolidColorBrush(Color.FromRgb(0xB0, 0xB8, 0xC4)),
            _ => new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x98))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
