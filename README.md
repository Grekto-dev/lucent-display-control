# ☀ Lucent Display Control

**Version 0.1.0**

A modern Windows 11-style native desktop application designed for precise control over **Brightness, Contrast, Gamma, Color Temperature, and Individual RGB Channels** across multiple monitors, built specifically to support **DisplayLink USB** adapters, external docks, and integrated screens without physical adjustment buttons.

---

## ✨ Features

* **Universal DisplayLink & Video Output Compatibility:**
  * Uses a high-performance hybrid engine combining **Hardware DWM Fullscreen Composition Matrix** (`MagSetFullscreenColorEffect`) and **WPF Layered Click-Through Overlays** (`WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE`).
  * Solves the issue where DisplayLink USB video adapters ignore hardware DDC/CI or GPU control panels.
* **Unified Visual Branding & Multi-Resolution Icon:**
  * Consistent high-DPI diamond-aperture icon across the window chrome, taskbar, Alt+Tab, native dark dialogs, and system tray.
* **Interactive Display Topology:**
  * Interactive monitor cards displaying active screens, resolutions (e.g. `1920 × 1080 · 60 Hz`), and primary/secondary designations.
  * Adjust displays independently or synchronize all monitors with **All Displays (Synced)**.
* **Fine-Grained Picture & Color Controls:**
  * 💡 **Brightness:** 20% to 100% with live percentage badge.
  * ⚡ **Contrast:** 50% to 150%.
  * 🌀 **Gamma:** 0.60 to 2.00 with visual reference ticks.
  * 🌡 **Color Temperature:** 3000 K (warm/amber anti-fatigue) to 9000 K (cool/blue), with neutral at 6500 K.
  * 🔴🟢🔵 **RGB Channels:** Independent Red, Green, and Blue sliders (0 to 100).
* **Presets & Profile Management:**
  * 8 built-in factory presets: *Standard, Warm, Cool, Movie, Work, Night, Gaming, Focus*.
  * Built-in vs. Custom tags, modification tracking, and creation of unlimited custom profiles.
* **Intelligent Auto-Save System:**
  * Configurable in Settings: choose between **Auto-Save** or **Manual Save**.
  * **First-Time Save Flow:** When modifying settings from a default profile, prompts the user to name their new profile; subsequent changes persist directly to that profile without repetitive prompts.
  * **Manual Save Mode:** Real-time screen preview with uncommitted changes badge and explicit Save / Save as New buttons.
* **Circadian Automated Scheduler:**
  * Automatic circadian transitions across Day, Sunset, and Night slots with customizable start times and target profiles.
* **Windows 11 Settings:**
  * 🌐 **Language Selector:** Supports 7 languages with instant preview:
    * English (Default), Português (Brasil), Español, Français, Italiano, 中文, 日本語.
  * 🚀 **Start with Windows:** Auto-start on boot with indented **Start minimized to system tray** toggle.
  * 📥 **Minimize to Tray on Close:** Keeps app running in background when window is closed.
  * ⚡ **Auto-Apply Profile on Startup:** Loads user-selected profile on startup; profile dropdown is greyed out and disabled unless auto-apply is enabled.
  * 🔄 **Restore Default Presets:** Restores factory built-in presets if modified (with confirmation dialog; custom profiles preserved).
  * 🗑 **Erase All Data (Factory Reset):** Safely deletes configuration, schedules, and custom profiles, returning app to out-of-the-box state (with danger confirmation dialog).
* **High Efficiency & Near-Zero Resource Usage:**
  * Automatically forces working set trimming (`SetProcessWorkingSetSize(-1, -1)`) when minimized to tray.
  * **Memory Footprint:** Drops to ~48 MB in tray (61.2% memory savings over baseline).
  * **CPU Utilization:** 0.00% background idle usage with no polling loops.

---

## 🛠 Compilation & Architecture

Built with C# 5 and WPF on .NET Framework 4.8. Can be compiled natively on any Windows machine using `csc.exe` without external SDKs:

```powershell
.\build.bat
```

To run:
```powershell
.\start.bat
# or launch LucentControl.exe directly
```

### Running Automated Tests
```powershell
.\build.bat
# Runs the full unit test suite (AppIcon, Config, Profiles, RestoreDefaults, EraseData, DwmEngine, Scheduler, Overlays)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:exe /r:LucentControl.exe /r:System.Xaml.dll /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll" /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" /r:System.Drawing.dll /r:System.Windows.Forms.dll /out:"RunTests.exe" tools\RunTests.cs
.\RunTests.exe
```

---

## 📦 Git Repository

Version control is initialized in this repository:
* `main` branch tracks the project history.
