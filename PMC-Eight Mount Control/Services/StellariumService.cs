using System;
using System.Diagnostics;
using System.IO;

namespace PMC_Eight_Mount_Control.Services
{
    public class StellariumService
    {
        public string LaunchStellarium()
        {
            try
            {
                // Modify Stellarium config to use the ASCOM telescope connection
                string configFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Stellarium", "config.ini");

                ModifyStellariumConfig(configFilePath);

                // Launch Stellarium
                // Launch Stellarium
                Process.Start(new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\Stellarium\stellarium.exe",
                    Arguments = "--single-buffer",
                    UseShellExecute = true
                });


                return "Stellarium launched successfully.";
            }
            catch (Exception ex)
            {
                return $"Error launching Stellarium: {ex.Message}";
            }
        }

        private void ModifyStellariumConfig(string configFilePath)
        {
            // Logic to modify Stellarium's config file to use the ASCOM device
        }
    }
}
