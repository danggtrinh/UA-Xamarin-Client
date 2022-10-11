using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Thesis
{
    public class AddOrEditConnectionViewModel : INotifyPropertyChanged
    {
        private ConnectType _connectType;

        public ConnectType ConnectType
        {
            get { return _connectType; }
            set
            {
                _connectType = value;
                OnPropertyChanged();
            }
        }

        public AddOrEditConnectionViewModel()
        {
            ConnectType = new ConnectType();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}