// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Multi-value converter that compares two objects by reference.
/// </summary>
public sealed class ReferenceEqualsMultiConverter : IMultiValueConverter
{
    /// <summary>
    /// Shared <see cref="ReferenceEqualsMultiConverter"/> instance.
    /// </summary>
    public static readonly ReferenceEqualsMultiConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return false;
        }

        return ReferenceEquals(values[0], values[1]);
    }

    /// <inheritdoc/>
    public object? ConvertBack(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
