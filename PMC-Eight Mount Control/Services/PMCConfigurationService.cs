using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace PMC_Eight_Mount_Control.Services
{
    public class PMCConfigurationService
    {
        private const int BaudRate = 115200; // Typical baud rate for PMC-Eight mount

        // Expose the selected COM port so the ViewModel can use it
        public string SelectedComPort { get; private set; }

        public async Task<string> ConnectToMountAsync()
        {
            try
            {
                // Step 1: Detect available COM ports
                string[] ports = SerialPort.GetPortNames();
                if (ports.Length == 0)
                {
                    return "No COM ports detected. Please check the connection.";
                }

                // Step 2: Automatically select the correct port (prioritize COM 3 if it exists)
                SelectedComPort = DetectCOMPort();
                if (string.IsNullOrEmpty(SelectedComPort))
                {
                    return "Unable to detect the correct COM port.";
                }

                // Step 3: Try to connect to the mount
                using (SerialPort port = new SerialPort(SelectedComPort, BaudRate))
                {
                    port.Open();

                    // Send a command to check the connection
                    string response = await SendCommandAsync(port, "GET_CURRENT_CONFIGURATION");

                    if (response.Contains("EXPECTED_CONFIG"))
                    {
                        return $"Mount connected on {SelectedComPort}. Configuration is up to date.";
                    }
                    else
                    {
                        // Perform configuration if needed
                        string configureResponse = await SendCommandAsync(port, "SET_NEW_CONFIGURATION");
                        return $"Mount connected on {SelectedComPort}. Configuration updated.";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error connecting to mount: {ex.Message}";
            }
        }

        private string DetectCOMPort()
        {
            string[] ports = SerialPort.GetPortNames();

            // Prioritize COM 3 if it's available
            if (ports.Contains("COM3"))
            {
                return "COM3";
            }

            // Otherwise, return the first available port
            return ports.FirstOrDefault();
        }

        private Task<string> SendCommandAsync(SerialPort port, string command)
        {
            return Task.Run(() =>
            {
                port.WriteLine(command);
                string response = port.ReadLine();
                return response;
            });
        }

    private Task<string> RetrieveAndConfigureMountAsync(SerialPort port)
        {
            return Task.Run(() =>
            {
                // Send a command to retrieve the current configuration
                string response = SendCommandAsync(port, "GET_CURRENT_CONFIGURATION").Result;

                // Parse the response (in reality, this would be based on the actual format of the response)
                if (response.Contains("iEXOS100"))
                {
                    // Set the mount type to iEXOS100
                    SendCommandAsync(port, "SET_MOUNT_TYPE iEXOS100").Wait();
                }
                else if (response.Contains("EXOS2"))
                {
                    // Set the mount type to EXOS2
                    SendCommandAsync(port, "SET_MOUNT_TYPE EXOS2").Wait();
                }

                // Set other parameters, like Wi-Fi settings
                SendCommandAsync(port, "SET_WIFI_ENABLED true").Wait();

                // Finalize the configuration
                return "Mount configured successfully.";
            });
        }

    }
}