﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAdjuvatorTransductoris.Helpers;

public class PluginViewBoolToVisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Visibility)value == Visibility.Visible;
    }
}