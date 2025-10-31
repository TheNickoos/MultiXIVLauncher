using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MultiXIVLauncher.View
{
    public partial class LoadingWindow : Window, INotifyPropertyChanged
    {
        private string _statusText = "Démarrage du jeu…";

        public event PropertyChangedEventHandler PropertyChanged;

        public LoadingWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Texte affiché sous la barre de progression.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Méthode appelée depuis le code externe pour mettre à jour le statut.
        /// Thread-safe (marshal vers le Dispatcher si besoin).
        /// </summary>
        public void SetStatus(string text)
        {
            if (Dispatcher.CheckAccess())
            {
                StatusText = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            }
            else
            {
                Dispatcher.Invoke(() => StatusText = string.IsNullOrWhiteSpace(text) ? string.Empty : text);
            }
        }

        /// <summary>
        /// Si tu veux éventuellement bloquer/débloquer l’UI (optionnel).
        /// </summary>
        public void SetIsBusy(bool isBusy)
        {
            if (Dispatcher.CheckAccess())
            {
                IsHitTestVisible = !isBusy;
                Opacity = isBusy ? 0.98 : 1.0;
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    IsHitTestVisible = !isBusy;
                    Opacity = isBusy ? 0.98 : 1.0;
                });
            }
        }

        public System.Threading.CancellationTokenSource CancellationSource { get; set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Si l’utilisateur ferme la fenêtre pendant que ça charge, on annule
            if (IsLoaded && CancellationSource != null && !CancellationSource.IsCancellationRequested)
            {
                CancellationSource.Cancel();
            }
            base.OnClosing(e);
        }


        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
