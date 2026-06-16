using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GasStationIS.Core;

namespace GasStationIS.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c) =>
            value is true ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type t, object p, CultureInfo c) =>
            value is Visibility.Visible;
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c) =>
            value is false ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type t, object p, CultureInfo c) =>
            !(value is Visibility.Visible);
    }

    /// <summary>
    /// Процент заполненности → цвет индикатора.
    /// &lt;15% красный, &lt;30% оранжевый, иначе зелёный.
    /// </summary>
    [ValueConversion(typeof(double), typeof(Brush))]
    public class FuelLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is double pct)
            {
                if (pct < 15) return new SolidColorBrush(Color.FromRgb(220, 53, 69));   // #DC3545 красный
                if (pct < 30) return new SolidColorBrush(Color.FromRgb(255, 149, 0));   // #FF9500 оранжевый
                return new SolidColorBrush(Color.FromRgb(25, 135, 84));                  // #198754 зелёный
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Процент заполненности → ширина полоски (0–200 px).
    /// </summary>
    public class FuelLevelToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is double pct)
                return Math.Max(0, Math.Min(200, pct * 2));   // 100% → 200px
            return 0.0;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Текущая роль пользователя — скрываем элементы не для администратора.
    /// </summary>
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c) =>
            SessionManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Число > 0 → Visible, иначе Collapsed.
    /// </summary>
    public class PositiveToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            try { return System.Convert.ToDouble(value) > 0 ? Visibility.Visible : Visibility.Collapsed; }
            catch { return Visibility.Collapsed; }
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Непустая строка / ненулевой объект → Visible, иначе Collapsed.
    /// Используется для StatusMessage и ErrorMessage.
    /// </summary>
    public class NotNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is string s) return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }
}
