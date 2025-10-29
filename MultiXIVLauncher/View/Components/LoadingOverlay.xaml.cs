using System.Windows.Controls;

namespace MultiXIVLauncher.View.Components
{
    public partial class LoadingOverlay : UserControl
    {
        public LoadingOverlay()
        {
            InitializeComponent();
        }

        public void UpdateStatus(string text)
        {
            LblStatus.Text = text;
        }
    }
}
