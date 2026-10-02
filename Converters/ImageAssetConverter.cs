using Avalonia.Data.Converters;
using StageFlow.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StageFlow.Converters
{
    internal class ImageAssetConverter : IValueConverter
    {
        public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        {
            return value is MediaAssetType type &&
                   type == MediaAssetType.Image;
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
