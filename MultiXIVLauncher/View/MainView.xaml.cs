using MultiXIVLauncher;
using MultiXIVLauncher.Views.Headers;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new MainHeader());
        }
    }
}
