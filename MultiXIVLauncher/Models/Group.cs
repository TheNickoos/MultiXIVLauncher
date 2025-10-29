using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiXIVLauncher.Models
{
    public class Group : INotifyPropertyChanged
    {
        private int _id;
        private string _name = "";

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
