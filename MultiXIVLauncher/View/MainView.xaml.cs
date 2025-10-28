using MultiXIVLauncher.Views.Headers;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the main launcher view.
    /// This is the default view displayed when the application starts.
    /// </summary>
    public partial class MainView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainView"/> class.
        /// Sets the dynamic header content to the main header.
        /// </summary>
        public MainView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new MainHeader());
        }
    }
}
