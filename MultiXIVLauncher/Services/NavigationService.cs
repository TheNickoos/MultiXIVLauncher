using System;
using System.Windows.Controls;

namespace MultiXIVLauncher.Services
{
    /// <summary>
    /// Provides a simple navigation system for switching views within a <see cref="ContentControl"/>.
    /// </summary>
    public static class NavigationService
    {
        private static ContentControl? _contentHost;

        /// <summary>
        /// Initializes the navigation service with a target <see cref="ContentControl"/> that hosts pages or views.
        /// </summary>
        /// <param name="contentHost">The control where views will be displayed.</param>
        public static void Initialize(ContentControl contentHost)
        {
            _contentHost = contentHost;
        }

        /// <summary>
        /// Navigates to the specified view by replacing the current content of the host.
        /// </summary>
        /// <param name="view">The new <see cref="UserControl"/> to display.</param>
        public static void Navigate(UserControl view)
        {
            if (_contentHost == null)
                throw new InvalidOperationException("NavigationService not initialized.");

            _contentHost.Content = view;
        }
    }
}
