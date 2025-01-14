using System;
using System.IO; 
using System.ComponentModel;
using System.Windows.Input;
using System.Xml;
using ASCOM.DriverAccess; // Ensure ASCOM is referenced
using PMC_Eight_Mount_Control.Helpers;
using PMC_Eight_Mount_Control.Models;
using Newtonsoft.Json;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

namespace PMC_Eight_Mount_Control.ViewModels
{
    public class LocationSettingsViewModel : INotifyPropertyChanged
    {
        private string _latitude;
        private string _longitude;
        private string _elevation;

        private readonly string ConfigFilePath = ("C:\\Users\\kyleb\\source\\repos\\PMC-Eight Mount Control\\PMC-Eight Mount Control\\bin\\DebuglocationConfig.json"); //json wouldn't instantiate so I had to specify the location. 

        public ObservableCollection<string> TelescopeModels { get; } = new ObservableCollection<string>
   {
       "Explore Scientific EXOS II",
       "Losmandy G-11",
       "Explore Scientific iEXOS-100",
       "Explore Scientific iEXOS-200",
       "Explore Scientific iEXOS-300",
       "Losmandy Titan",
       "Scotty",
       "MSREQ"
   };

        private string _selectedTelescopeModel;
        public string SelectedTelescopeModel
        {
            get => _selectedTelescopeModel;
            set
            {
                _selectedTelescopeModel = value;
                OnPropertyChanged();
                UpdateTelescopeModel(); // Trigger update when changed
            }
        }

        public string Latitude
        {
            get => _latitude;
            set
            {
                _latitude = value;
                OnPropertyChanged();
            }
        }

        public string Longitude
        {
            get => _longitude;
            set
            {
                _longitude = value;
                OnPropertyChanged();
            }
        }

        public string Elevation
        {
            get => _elevation;
            set
            {
                _elevation = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveLocationCommand { get; }

        public LocationSettingsViewModel()
        {
            LoadLocationData(); // Load previously saved location data
            SaveLocationCommand = new RelayCommand(SaveLocation);
        }

        private void LoadLocationData()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var locationData = JsonConvert.DeserializeObject<LocationData>(json);

                    Latitude = locationData.Latitude.ToString();
                    Longitude = locationData.Longitude.ToString();
                    Elevation = locationData.Elevation.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading location data: {ex.Message}");
                }
            }
        }

        private void SaveLocation()
        {
            try
            {
                // Save to the ASCOM driver
                using (var telescope = new Telescope("ASCOM.DeviceHub.Telescope"))
                {
                    telescope.Connected = true;
                    telescope.SiteLatitude = double.Parse(Latitude);
                    telescope.SiteLongitude = double.Parse(Longitude);
                    telescope.SiteElevation = double.Parse(Elevation);
                    telescope.Connected = false;
                }

                // Save location data to a JSON file for future use
                var locationData = new
                {
                    Latitude = double.Parse(Latitude),
                    Longitude = double.Parse(Longitude),
                    Elevation = double.Parse(Elevation),
                };

                string json = JsonConvert.SerializeObject(locationData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);

                //I'm struggling to get the JSON to instantiate so I'm going to add this to make sure I know where it's saving. 
                Console.WriteLine($"Location updated and saved successfully to: {Path.GetFullPath(ConfigFilePath)}");
            }
            catch (Exception ex)
            {
                // Handle any errors here (e.g., invalid input or driver errors)
                Console.WriteLine($"Error updating location: {ex.Message}");
            }
        }

        private void UpdateTelescopeModel()
        {
            try
            {
                using (var telescope = new Telescope("ASCOM.DeviceHub.Telescope"))
                {
                    telescope.Connected = true;
                    telescope.CommandString($":SET_MODEL_{SelectedTelescopeModel.Replace(" ", "_")}", false); // Example ASCOM format
                    Console.WriteLine($"Telescope model set to {SelectedTelescopeModel}");
                    telescope.Connected = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating telescope model: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
