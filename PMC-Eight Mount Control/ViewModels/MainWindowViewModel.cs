using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PMC_Eight_Mount_Control.Helpers;
using PMC_Eight_Mount_Control.Services;

namespace PMC_Eight_Mount_Control.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly PMCConfigurationService _pmcService;
        private readonly StellariumService _stellariumService;

        public MainWindowViewModel()
        {
            _pmcService = new PMCConfigurationService();
            _stellariumService = new StellariumService();
            AvailableComPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            _selectedComPort = AvailableComPorts.FirstOrDefault(); // Default to first available port
            ConnectCommand = new RelayCommand(ConnectToMount);
            LaunchStellariumCommand = new RelayCommand(LaunchStellarium);

            // Initialize the rate and movement commands
            SelectedRate = "1 deg/s"; // Default rate
            MoveNorthCommand = new RelayCommand(MoveNorth);
            MoveSouthCommand = new RelayCommand(MoveSouth);
            MoveEastCommand = new RelayCommand(MoveEast);
            MoveWestCommand = new RelayCommand(MoveWest);
        }

        public ObservableCollection<string> AvailableComPorts { get; }
        private string _selectedComPort;

        public string SelectedComPort
        {
            get => _selectedComPort;
            set
            {
                _selectedComPort = value;
                OnPropertyChanged();
            }
        }

        private string _connectionStatus;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        private string _mountStatus;
        public string MountStatus
        {
            get => _mountStatus;
            set { _mountStatus = value; OnPropertyChanged(); }
        }

        private string _stellariumStatus;
        public string StellariumStatus
        {
            get => _stellariumStatus;
            set { _stellariumStatus = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // New properties and commands for movement and rate selection
        private string _selectedRate;
        public string SelectedRate
        {
            get => _selectedRate;
            set
            {
                _selectedRate = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; }
        public ICommand MoveNorthCommand { get; }
        public ICommand MoveSouthCommand { get; }
        public ICommand MoveEastCommand { get; }
        public ICommand MoveWestCommand { get; }
        public ICommand LaunchStellariumCommand { get; }

        private async void ConnectToMount()
        {
            try
            {
                // Call the asynchronous connection method
                ConnectionStatus = "Connecting to mount...";

                // Connect to the mount
                string connectionResult = await _pmcService.ConnectToMountAsync();
                ConnectionStatus = connectionResult;

                // Update the SelectedComPort in the ViewModel so that the ComboBox shows the correct port
                SelectedComPort = _pmcService.SelectedComPort;

                // Refresh the list of available COM ports to make sure the ComboBox reflects all options
                RefreshAvailableComPorts();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error connecting to mount: {ex.Message}";
            }
        }

        private void RefreshAvailableComPorts()
        {
            // Clear and refresh the available COM ports in the ComboBox
            AvailableComPorts.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                AvailableComPorts.Add(port);
            }
        }

        private void MoveNorth() => MoveTelescope("North");
        private void MoveSouth() => MoveTelescope("South");
        private void MoveEast() => MoveTelescope("East");
        private void MoveWest() => MoveTelescope("West");

        private void MoveTelescope(string direction)
        {
            // Implement actual movement logic based on the SelectedRate and direction
            Console.WriteLine($"Moving {direction} at {SelectedRate}");
        }

        private void LaunchStellarium()
        {
            try
            {
                StellariumStatus = _stellariumService.LaunchStellarium();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error launching Stellarium: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
