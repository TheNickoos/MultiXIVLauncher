using MultiXIVLauncher.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiXIVLauncher
{
    /// <summary>
    /// Main application window of MultiXIVLauncher.
    /// Handles navigation between core sections such as Settings, Characters, Presets, and Groups.
    /// </summary>
    public partial class LauncherWindow : Window
    {
        /// <summary>
        /// Initializes the main window and sets the default view.
        /// </summary>
        public LauncherWindow()
        {
            InitializeComponent();
            MainContent.Content = new MainView();
        }

        /// <summary>
        /// Sets dynamic content in the window header (e.g. SettingsHeader, MainHeader).
        /// </summary>
        /// <param name="element">The header UI element to display.</param>
        public void SetHeaderContent(UIElement element)
        {
            HeaderDynamicArea.Content = element;
        }

        /// <summary>
        /// Displays the Settings view.
        /// </summary>
        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView();
        }

        /// <summary>
        /// Displays the Characters view.
        /// </summary>
        private void ShowCharacters(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CharactersView();
        }

        /// <summary>
        /// Displays the Presets view.
        /// </summary>
        private void ShowPresets(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new PresetsView();
        }

        /// <summary>
        /// Displays the Groups view.
        /// </summary>
        private void ShowGroups(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new GroupsView();
        }

        /// <summary>
        /// Opens the official Mog Station website.
        /// </summary>
        private void OpenMogStation(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.mogstation.com");
        }

        /// <summary>
        /// Opens the XIVISUP website.
        /// </summary>
        private void OpenXIVISUP(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://is.xivup.com/");
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Sets the active page inside the main content area.
        /// </summary>
        /// <param name="view">The view to display inside the main content frame.</param>
        public void SetPage(UserControl view)
        {
            MainContent.Content = view;
        }

        /// <summary>
        /// Allows the window to be dragged by clicking and holding anywhere on its surface.
        /// </summary>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        public void SetLauncherInteractivity(bool enable)
        {
            this.IsEnabled = enable;
        }
    }
}
