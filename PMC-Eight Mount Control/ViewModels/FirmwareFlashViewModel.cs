using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using PMC_Eight_Mount_Control.Helpers;

namespace PMC_Eight_Mount_Control.ViewModels
{
    public class FirmwareFlashViewModel
    {
        public string FirmwareFilePath { get; set; }
        public string FlashStatus { get; private set; }
        public ICommand BrowseCommand { get; }
        public ICommand FlashFirmwareCommand { get; }

        public FirmwareFlashViewModel()
        {
            BrowseCommand = new RelayCommand(BrowseForFile);
            FlashFirmwareCommand = new RelayCommand(FlashFirmware);
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
            }
        }

        private void FlashFirmware()
        {
            if (!File.Exists(FirmwareFilePath))
            {
                FlashStatus = "Error: File not found.";
                return;
            }

            // Logic to send binary to mount
            FlashStatus = "Firmware flashing started...";
            // Example flashing code
            FlashStatus = "Firmware flashed successfully!";
        }
    }
}
