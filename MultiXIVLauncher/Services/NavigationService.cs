using System;
using System.Windows.Controls;

namespace MultiXIVLauncher.Services
{
    public static class NavigationService
    {
        private static ContentControl _contentHost;

        public static void Initialize(ContentControl contentHost)
        {
            _contentHost = contentHost;
        }

        public static void Navigate(UserControl view)
        {
            if (_contentHost == null)
                throw new InvalidOperationException("NavigationService not initialized.");
            _contentHost.Content = view;
        }
    }
}
