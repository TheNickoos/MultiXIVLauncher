using System;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Services
{
    /// <summary>
    /// Provides attached properties that automatically localize text, titles, and tooltips
    /// based on language keys managed by <see cref="LanguageManager"/>.
    /// </summary>
    public static class LanguageBehavior
    {

        /// <summary>
        /// Attached property for binding a localization key to a control's text or content.
        /// </summary>
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.RegisterAttached(
                "Key",
                typeof(string),
                typeof(LanguageBehavior),
                new PropertyMetadata(null, OnKeyChanged));

        /// <summary>
        /// Gets the localization key for a control.
        /// </summary>
        public static string GetKey(DependencyObject obj)
        {
            return (string)obj.GetValue(KeyProperty);
        }

        /// <summary>
        /// Sets the localization key for a control.
        /// </summary>
        public static void SetKey(DependencyObject obj, string value)
        {
            obj.SetValue(KeyProperty, value);
        }

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateText(d, e.NewValue as string);

            LanguageManager.LanguageChanged += delegate
            {
                UpdateText(d, e.NewValue as string);
            };
        }

        /// <summary>
        /// Updates the localized text or content of a control.
        /// </summary>
        private static void UpdateText(DependencyObject d, string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (d is ContentControl contentControl)
                contentControl.Content = LanguageManager.T(key);
            else if (d is TextBlock textBlock)
                textBlock.Text = LanguageManager.T(key);
        }

        /// <summary>
        /// Attached property for binding a localization key to a control's tooltip.
        /// </summary>
        public static readonly DependencyProperty ToolTipKeyProperty =
            DependencyProperty.RegisterAttached(
                "ToolTipKey",
                typeof(string),
                typeof(LanguageBehavior),
                new PropertyMetadata(null, OnToolTipKeyChanged));

        /// <summary>
        /// Gets the localization key for a tooltip.
        /// </summary>
        public static string GetToolTipKey(DependencyObject obj)
        {
            return (string)obj.GetValue(ToolTipKeyProperty);
        }

        /// <summary>
        /// Sets the localization key for a tooltip.
        /// </summary>
        public static void SetToolTipKey(DependencyObject obj, string value)
        {
            obj.SetValue(ToolTipKeyProperty, value);
        }

        private static void OnToolTipKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateToolTip(d, e.NewValue as string);

            LanguageManager.LanguageChanged += delegate
            {
                UpdateToolTip(d, e.NewValue as string);
            };
        }

        /// <summary>
        /// Updates the localized tooltip of a control.
        /// </summary>
        private static void UpdateToolTip(DependencyObject d, string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            ((FrameworkElement)d).ToolTip = LanguageManager.T(key);
        }

        /// <summary>
        /// Attached property for binding a localization key to a window's title.
        /// </summary>
        public static readonly DependencyProperty TitleKeyProperty =
            DependencyProperty.RegisterAttached(
                "TitleKey",
                typeof(string),
                typeof(LanguageBehavior),
                new PropertyMetadata(null, OnTitleKeyChanged));

        /// <summary>
        /// Gets the localization key for a window title.
        /// </summary>
        public static string GetTitleKey(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleKeyProperty);
        }

        /// <summary>
        /// Sets the localization key for a window title.
        /// </summary>
        public static void SetTitleKey(DependencyObject obj, string value)
        {
            obj.SetValue(TitleKeyProperty, value);
        }

        private static void OnTitleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateTitle(d, e.NewValue as string);

            LanguageManager.LanguageChanged += delegate
            {
                UpdateTitle(d, e.NewValue as string);
            };
        }

        /// <summary>
        /// Updates the localized title of a window.
        /// </summary>
        private static void UpdateTitle(DependencyObject d, string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var window = d as Window;
            if (window != null)
                window.Title = LanguageManager.T(key);
        }
    }
}
