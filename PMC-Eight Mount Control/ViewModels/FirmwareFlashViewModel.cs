using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using PMC_Eight_Mount_Control.Helpers;

namespace PMC_Eight_Mount_Control.ViewModels
{
    public class FirmwareFlashViewModel : INotifyPropertyChanged
    {
        private string _firmwareFilePath;
        private string _flashStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FirmwareFilePath
        {
            get => _firmwareFilePath;
            set
            {
                _firmwareFilePath = value;
                OnPropertyChanged(nameof(FirmwareFilePath));
            }
        }

        public string FlashStatus
        {
            get => _flashStatus;
            set
            {
                _flashStatus = value;
                OnPropertyChanged(nameof(FlashStatus));
            }
        }

        public ICommand BrowseCommand { get; }
        public ICommand FlashFirmwareCommand { get; }

        public FirmwareFlashViewModel()
        {
            BrowseCommand = new RelayCommand(BrowseForFile);
            FlashFirmwareCommand = new RelayCommand(async () => await FlashFirmwareAsync());
        }

        private void BrowseForFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Binary Files (*.binary)|*.binary",
                Title = "Select Firmware File"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FirmwareFilePath = openFileDialog.FileName;
                FlashStatus = "Firmware file loaded.";
            }
        }

        private async Task FlashFirmwareAsync()
        {
            if (string.IsNullOrEmpty(FirmwareFilePath) || !File.Exists(FirmwareFilePath))
            {
                FlashStatus = "Error: Please load a valid firmware file.";
                return;
            }

            FlashStatus = "Flashing firmware...";

            try
            {
                using (SerialPort port = new SerialPort("COM3", 115200))
                {
                    port.Open();

                    byte[] firmwareData = File.ReadAllBytes(FirmwareFilePath);

                    // Send data in chunks manually
                    const int chunkSize = 1024;
                    for (int i = 0; i < firmwareData.Length; i += chunkSize)
                    {
                        int size = chunkSize;
                        if (i + chunkSize > firmwareData.Length)
                        {
                            size = firmwareData.Length - i;
                        }

                        byte[] chunk = new byte[size];
                        System.Array.Copy(firmwareData, i, chunk, 0, size);
                        port.Write(chunk, 0, chunk.Length);
                        await Task.Delay(50); // Add delay if necessary
                    }

                    port.WriteLine("END_FLASH");
                    FlashStatus = "Firmware flashed successfully!";
                }
            }
            catch (IOException ex)
            {
                FlashStatus = $"Flashing failed: {ex.Message}";
            }
        }
    }
}
