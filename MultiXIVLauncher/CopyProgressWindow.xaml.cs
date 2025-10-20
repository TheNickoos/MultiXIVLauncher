using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MultiXIVLauncher
{
    public partial class CopyProgressWindow : Window
    {
        public CopyProgressWindow()
        {
            InitializeComponent();
        }

        public async Task CopyWithProgressAsync(string sourceDir, string destDir)
        {
            StatusText.Text = "Copying files...";
            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            int total = files.Length;
            int copied = 0;

            await Task.Run(() =>
            {
                CopyRecursive(sourceDir, destDir, ref copied, total);
            });
        }

        private void CopyRecursive(string sourceDir, string destDir, ref int copied, int total)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);

                copied++;
                double progress = ((double)copied / total) * 100;
                Dispatcher.Invoke(() => ProgressBar.Value = progress);
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string subDest = Path.Combine(destDir, Path.GetFileName(dir));
                CopyRecursive(dir, subDest, ref copied, total);
            }
        }
    }
}
