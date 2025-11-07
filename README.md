# pomodoro-tray-app
A lightweight Pomodoro timer that lives in your system tray. Built with WPF and Material Design, this app makes it easy to start/stop your timer, switch intervals, and get notifications—all without needing to open a main window.

## About This Project
I used it as a small project to learn more about a few things:

* **Material Design in XAML:** Bringing a modern, material look and feel to a WPF application.
* **Single-File Executable:** Packaging the entire application, including the .NET runtime and all dependencies, into a single `.exe` file for easy distribution.
* **GitHub Actions/Workflows:** Automating the process of building the application and creating releases.

## Future Plans
Since this is something that is purposefully simplistic, I don't expect any major features to be added. However, there are plenty of enhancements and improvements that could be made/added, including:
1. Styling improvements
2. Animations
3. Cross platform (Avalonia?)
4. Persistent user settings:
	* Configurable intervals
	* Notification customizations (sounds, text size, etc.)
	* Hotkeys (system-wide)
	* Register a Windows Startup shortcut or registry entry so the tray app starts with the OS.
