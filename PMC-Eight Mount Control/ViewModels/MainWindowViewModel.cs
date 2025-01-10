﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ASCOM.DriverAccess;
using PMC_Eight_Mount_Control.Helpers;
using PMC_Eight_Mount_Control.Services;
using PMC_Eight_Mount_Control.Views;
using PMC_Eight_Mount_Control.Models;
using Newtonsoft.Json;

namespace PMC_Eight_Mount_Control.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly PMCConfigurationService _pmcService;
        private readonly StellariumService _stellariumService;
        public ICommand OpenLocationSettingsCommand { get; }
        public ICommand OpenFirmwareFlashCommand { get; }

        public MainWindowViewModel()
        {
            LoadLocationData();
            OpenLocationSettingsCommand = new RelayCommand(OpenLocationSettings);
            OpenFirmwareFlashCommand = new RelayCommand(OpenFirmwareFlashWindow);
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
            StopCommand = new RelayCommand(StopTelescope);
        }

        public ObservableCollection<string> AvailableComPorts { get; }
        private string _selectedComPort;
        private readonly string ConfigFilePath = "locationConfig.json";
        private ASCOM.DriverAccess.Telescope telescope; // I had to persist the telescope object here or else I couldn't get the scope to stop moving. 

        private void OpenLocationSettings()
        {
            var locationSettingsWindow = new LocationSettingsWindow
            {
                DataContext = new LocationSettingsViewModel()
            };
            locationSettingsWindow.Show();
        }

        private void LoadLocationData()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var locationData = JsonConvert.DeserializeObject<LocationData>(json);

                    // Apply this data to the telescope driver
                    using (var telescope = new Telescope("ASCOM.ES_PMC8.Telescope"))
                    {
                        telescope.Connected = true;
                        telescope.SiteLatitude = locationData.Latitude;
                        telescope.SiteLongitude = locationData.Longitude;
                        telescope.SiteElevation = locationData.Elevation;
                        telescope.Connected = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading location data: {ex.Message}");
                }
            }
        }

        // Class to hold location data
        private class LocationData
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Elevation { get; set; }
        }

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
        public ICommand StopCommand { get; }
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

        private void TestTelescopeCommunication()
        {
            try
            {
                if (telescope == null)
                {
                    telescope = new ASCOM.DriverAccess.Telescope("ASCOM.ES_PMC8.Telescope");
                }

                if (!telescope.Connected)
                {
                    telescope.Connected = true;
                }

                // Test a simple stop command
                telescope.CommandBlind(":Q#", true); // Test stop command

                Console.WriteLine("Command sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error communicating with telescope: {ex.Message}");
            }
        }

        private async void MoveTelescope(string direction)
        {
            try
            {
                if (telescope == null)
                {
                    telescope = new ASCOM.DriverAccess.Telescope("ASCOM.ES_PMC8.Telescope");
                }

                if (!telescope.Connected)
                {
                    telescope.Connected = true;
                }

                double moveRate = 0.2083333; // Rate to mimic Device Hub's movement rate

                // Move the telescope based on the direction
                switch (direction)
                {
                    case "North":
                        // Move the telescope in the North direction by increasing the Declination
                        telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisSecondary, moveRate);
                        break;
                    case "South":
                        // Move the telescope in the South direction by decreasing the Declination
                        telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisSecondary, -moveRate);
                        break;
                    case "East":
                        // Move the telescope in the East direction by increasing the Right Ascension
                        telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisPrimary, moveRate);
                        break;
                    case "West":
                        // Move the telescope in the West direction by decreasing the Right Ascension
                        telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisPrimary, -moveRate);
                        break;
                }

                // Move for a short duration
                await Task.Delay(1000); // Adjust delay to control movement duration

                // Stop the movement
                //telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisSecondary, 0.001); // Stop the Secondary axis
                //await Task.Delay(100);
                telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisSecondary, 0);
                telescope.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisPrimary, 0); // Stop the Primary axis if necessary
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving telescope: {ex.Message}");
            }
        }

        private void StopTelescope()
        {
            try
            {
                if (telescope != null && telescope.Connected)
                {
                    telescope.AbortSlew(); // This stops all motion of the telescope
                    Console.WriteLine("Telescope motion stopped.");
                    telescope.Connected = false; // Disconnect the telescope
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping telescope: {ex.Message}");
            }
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

        private void OpenFirmwareFlashWindow()
        {
            var firmwareFlashWindow = new FirmwareFlashWindow
            {
                DataContext = new FirmwareFlashViewModel()
            };
            firmwareFlashWindow.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
