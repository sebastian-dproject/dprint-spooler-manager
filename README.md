# DPrint Spooler Manager

<p align="center">

A lightweight Windows utility for monitoring and managing the Windows Print Spooler service and print queues.

</p>

<p align="center">

<img src="https://img.shields.io/badge/Latest-v1.0.1-blue" alt="Latest Version">
<img src="https://img.shields.io/badge/Platform-Windows%20x64-blue" alt="Platform">
<img src="https://img.shields.io/badge/Framework-WPF%20%7C%20.NET%2010-purple" alt="Framework">
<img src="https://img.shields.io/badge/License-Apache%202.0-green" alt="License">

</p>

<p align="center">

<a href="https://github.com/sebastian-dproject/dprint-spooler-manager/releases/tag/v1.0.1">
<img src="https://img.shields.io/badge/Download-DPrint%20Spooler%20Manager%20v1.0.1-brightgreen?style=for-the-badge&logo=windows" alt="Download DPrint Spooler Manager">
</a>

</p>

## Features

- View the Windows Print Spooler service status.
- Start, stop, and restart the Print Spooler service.
- Monitor print jobs with automatic refresh (3-second interval).
- Manually refresh the print queue.
- View printer, document, user, status, page count, and submission time.
- Remove only the selected print job.
- Clear all jobs for the selected printer.
- Clear all print queues.
- View the activity log in a separate window.
- Switch between Dark and Light themes.
- Sort print queue columns.
- View application version, author credits, acknowledgements, and license in the About window.

## Screenshots

### Main Window

<p align="center">
  <img src="screenshots/main-dark.png" width="40%" alt="Main Window - Dark Mode">
  <img src="screenshots/main-light.png" width="40%" alt="Main Window - Light Mode">
</p>

### Activity Log

<p align="center">
  <img src="screenshots/activity-log-dark.png" width="40%" alt="Activity Log - Dark Mode">
  <img src="screenshots/activity-log-light.png" width="40%" alt="Activity Log - Light Mode">
</p>

## Requirements

### To run

- Windows 10 or later.
- Windows x64 architecture.
- Administrator privileges for service control and print-job deletion.

The portable release is self-contained and does not require a separate .NET Runtime installation.

### To build from source

- .NET 10 SDK.
- Windows environment.

## Build and run

Clone the repository, open a terminal in the project directory, then run:

```powershell
dotnet restore
dotnet build
dotnet run
```

The application requests Administrator permission at startup because some operations require elevation.

## Important notes

- The application uses Windows Management Instrumentation (WMI) to inspect and manage print jobs.
- The application uses `ServiceController` to control the Windows Print Spooler service.
- Clearing or deleting print jobs is destructive: pending jobs may be permanently removed. Review the selected action and confirmation prompt before proceeding.
- The sort arrow indicator in the queue table header is a known visual issue in this baseline. Column sorting itself is available; the indicator may not be visible.

## Project structure

```text
.
├── Models/
├── Services/
├── App.xaml
├── MainWindow.xaml
├── LogWindow.xaml
├── DPrintSpoolerManager.csproj
└── DPrintSpoolerManager.ico
```

The project and C# namespace use the `DPrintSpoolerManager` identity. The user-facing product and executable name are **DPrint Spooler Manager**.

## Disclaimer

DPrint Spooler Manager is an independent project and is not affiliated with, endorsed by, or supported by Microsoft or any printer manufacturer. Windows is a trademark of Microsoft Corporation.

## License

This project is licensed under the **Apache License 2.0**. See the [`LICENSE`](LICENSE) file for the complete license text.

Unless a file or component states otherwise, the project source code is distributed under Apache-2.0. Third-party dependencies and assets remain subject to their respective licenses and terms. The DPrint name and branding are not granted as trademarks by this software license.

## Credits

Published under the **DProject** identity.

Developed by **Sebastian**, with assistance from **OpenAI ChatGPT** during learning, design, and development. DPrint is an independent project; this acknowledgement does not imply endorsement by OpenAI.

Copyright © 2026 Sebastian.

## Download

**Latest version: v1.0.1**

[Download DPrint Spooler Manager for Windows x64](https://github.com/sebastian-dproject/dprint-spooler-manager/releases/tag/v1.0.1)

Download the ZIP package, extract it, and run `DPrint Spooler Manager.exe`.

No installer or separate .NET Runtime installation required.

For previous versions and source code, visit [GitHub Releases](https://github.com/sebastian-dproject/dprint-spooler-manager/releases).
