using System;
using System.Globalization;
using System.Windows.Data;

namespace MultiXIVLauncher.Services
{
    /// <summary>
    /// WPF value converter that dynamically binds a resource key to the currently active language.
    /// </summary>
    public class LanguageBinding : IValueConverter
    {
        /// <summary>
        /// Converts a localization key into its translated value using the current language.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            return LanguageManager.T(value.ToString());
        }

        /// <summary>
        /// Conversion back is not supported for language bindings.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}