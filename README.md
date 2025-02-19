# PMC-Eight Mount Control

## Overview
The **PMC-Eight Mount Control** application is a C#-based tool designed to simplify the connection and control of **Explore Scientific's PMC-Eight Go-To mount**. This application provides a streamlined interface to:

- Detect and connect to the PMC-Eight mount automatically
- Manage location settings
- Launch and configure **Stellarium** for seamless sky navigation
- Execute telescope movement commands
- Flash firmware updates

## Features
- **Auto-Detect Mount**: Scans available COM ports and connects to the mount.
- **Stellarium Integration**: Automatically modifies Stellarium configuration to use the ASCOM telescope connection.
- **Location Management**: Saves and loads location settings for accurate tracking.
- **Mount Control**: Allows manual movement of the telescope in all directions.
- **Firmware Flashing**: Provides a dedicated interface for flashing firmware to the PMC-Eight mount.

## Installation
### Prerequisites
Before installing the software, ensure you have:
- **Windows 10 or later**
- **.NET Framework 4.8 or later**
- **ASCOM Platform** (Required for telescope communication)
- **Stellarium** (Optional, but recommended for sky navigation)

### Steps
1. **Download the latest release** from the [GitHub Releases](https://github.com/kylebrozek/PMC-Eight-Mount-Control/releases) page.
2. **Extract the files** and run `PMC-Eight Mount Control.exe`.
3. If prompted, install **ASCOM Platform** and **Stellarium**.
4. Connect your **PMC-Eight mount** via USB.
5. Open the application and follow the **setup wizard**.

## Usage
### Connecting to the Mount
1. Launch the application.
2. Select the **COM Port** (or use Auto-Detect).
3. Click **Connect** to establish a connection.

### Setting Location Data
1. Open **Location Settings**.
2. Enter latitude, longitude, and elevation.
3. Click **Save** to update your mount settings.

### Controlling the Mount
- Use the movement buttons to **adjust telescope positioning**.
- Select a movement rate (e.g., `1 deg/s`).
- Click **Stop** to halt movement.

### Launching Stellarium
1. Click **Launch Stellarium**.
2. The application will modify **Stellarium’s config** to enable ASCOM control.

### Flashing Firmware
1. Open **Firmware Update**.
2. Select the `20A02.0.binary` firmware file.
3. Click **Flash Firmware**.

## Development
### Cloning the Repository
```sh
git clone https://github.com/kylebrozek/PMC-Eight-Mount-Control.git
```

### Building the Application
1. Open the solution `PMC-Eight Mount Control.sln` in **Visual Studio**.
2. Restore NuGet packages.
3. Build the project and run the application.

## Contributions
Contributions are welcome! To contribute:
- Fork the repository.
- Create a feature branch.
- Submit a **pull request** with a detailed explanation.

## Author
This project was developed by **Kyle Brozek**.

## Contact
For questions, open an [issue](https://github.com/kylebrozek/PMC-Eight-Mount-Control/issues) or reach out via [LinkedIn](https://www.linkedin.com/in/kylebrozek/).

