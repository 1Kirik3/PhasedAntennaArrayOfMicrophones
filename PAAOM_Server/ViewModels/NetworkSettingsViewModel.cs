using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAAOM_Server.ViewModels
{
    public class NetworkSettingsViewModel : INotifyPropertyChanged
    {
        private string _localIP = "127.0.0.1";
        private string _localPort = "5000";
        private string _remoteIP = "127.0.0.1";
        private string _remotePort = "5001";

        public ObservableCollection<string> AvailableIPs { get; } = new ObservableCollection<string>();

        public string LocalIP
        {
            get => _localIP;
            set
            {
                _localIP = value;
                OnPropertyChanged();
            }
        }

        public string LocalPort
        {
            get => _localPort;
            set
            {
                _localPort = value;
                OnPropertyChanged();
            }
        }

        public string RemoteIP
        {
            get => _remoteIP;
            set
            {
                _remoteIP = value;
                OnPropertyChanged();
            }
        }

        public string RemotePort
        {
            get => _remotePort;
            set
            {
                _remotePort = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}