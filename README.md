# ☼ Lucent Display Control

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Release](https://img.shields.io/badge/Release-v0.1.0-brightgreen.svg)](https://github.com/Grekto-dev/lucent-display-control/releases)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)](https://www.microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4.svg)](https://dotnet.microsoft.com/)

A modern, ultra-lightweight Windows 11 native desktop application designed for fine-grained control over **Brightness (0% to 200%), Contrast, Gamma, Color Temperature (Kelvin), and Individual RGB Channels** across all monitors.

Engineered specifically for configurations where standard GPU utilities (NVIDIA Control Panel, Intel Graphics Command Center, AMD Software) fail or are unavailable—such as **DisplayLink USB docks, external USB-to-HDMI/DisplayPort adapters, integrated laptop panels, and monitors lacking physical hardware buttons**.

---

## 📸 Overview

```
┌────────────────────────────────────────────────────────────────────────┐
│ ☼ Lucent Display Control                    ● Running in system tray  │
├───────────────┬────────────────────────────────────────────────────────┤
│ ⌂ Dashboard   │  Monitors                                              │
│ ⎚ Monitors    │  [1] DisplayLink USB   [2] DisplayLink USB   [⇄ Synced]│
│ ⊞ Presets     ├────────────────────────────────────────────────────────┤
│ 👤 Profiles   │  ☼ Brightness          ◑ Contrast         📈 Gamma     │
│ ⏱ Scheduler   │  [0% ──●── 200%]       [50% ──●── 150%]   [0.6 ──●── 2]│
│ ⚙ Settings    ├────────────────────────────────────────────────────────┤
│               │  🌡 Color Temp (6500K)  🔴 Red  🟢 Green  🔵 Blue       │
│               ├────────────────────────────────────────────────────────┤
│               │  ✦ Quick Presets: Warm · Neutral · Cool · Movie · Work │
└───────────────┴────────────────────────────────────────────────────────┘
```

---

## ✨ Key Features

### 🔌 Universal DisplayLink & GPU Bypass
- **Hybrid Composition Engine:** Utilizes the high-performance Windows Desktop Window Manager (DWM) 5×5 color transformation matrix (`MagSetFullscreenColorEffect`) combined with zero-overhead click-through layered overlays (`WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE`).
- Works on **any** display output, bypassing the requirement for DDC/CI protocol support or proprietary GPU drivers.

### 🎚 Fine-Grained Picture & Color Tuning
- **Brightness (0% to 200%):** Standard 100% level sits at the exact center of the slider track. Slide left to dim down to complete darkness (0%), or slide right to boost luminance up to **200%**.
- **Contrast (50% to 150%):** Midtone-preserving contrast control.
- **Gamma (0.60 to 2.00):** Correct gamma curves with calibrated tick markers.
- **Color Temperature (3000 K to 9000 K):** Circadian daylight simulation from warm amber (anti-blue light eye protection) to crisp cool daylight, with neutral balanced at 6500 K.
- **Independent RGB Gain (0 to 100):** Direct channel balance sliders for Red, Green, and Blue.

### 🖥 Multi-Monitor Topology & Synchronization
- Interactive monitor cards detailing model names, active resolutions (e.g., `1920 × 1080 · 60 Hz`), and display hierarchy.
- Toggle between adjusting each screen individually or synchronizing all screens simultaneously with **All Displays (Synced)**.
- **Identify Displays:** Fullscreen high-visibility overlay badge identifying display numbers in physical space.

### 🎨 Profiles & Intelligent Saving
- **8 Built-in Presets:** *Standard, Warm, Cool, Movie, Work, Night, Gaming, and Focus*.
- **Auto-Save vs. Manual Save Modes:** Configurable in Settings.
  - In Auto-Save mode, modifying settings from a default profile triggers a one-time profile naming prompt; subsequent adjustments persist immediately.
  - In Manual Save mode, live screen adjustments stay in preview until clicking **Save** or **Save as New**.
- Create, rename, delete, and manage unlimited custom display profiles.

### ⏱ Circadian Scheduler
- Automatic circadian lighting adjustments based on local time.
- Configurable time slots: **Day**, **Sunset**, and **Night** transitions that automatically apply your preferred display profiles.

### ⚙ Windows 11 Native Settings Page
- **7 Languages Supported:** English (default), Português (Brasil), Español, Français, Italiano, 中文 (Simplified Chinese), and 日本語 (Japanese) with instant live preview.
- **Startup Integration:** Launch at Windows boot with optional indented toggle to start minimized directly to the tray.
- **Tray Minimization:** Minimize to system tray when the window is closed, keeping the app accessible via tray icon and notification menu.
- **Auto-Apply on Boot:** Automatically activate a chosen profile upon app launch.
- **Reset Options:**
  - *Restore Default Presets:* Restores modified built-in presets while safely preserving all custom profiles.
  - *Erase All Data:* Complete factory reset with confirmation dialog.

### ⚡ Ultra-Low Resource Usage
- **Working Set Trimming:** Automatically frees inactive memory (`SetProcessWorkingSetSize(-1, -1)`) when minimized to tray.
- **Memory Footprint:** Drops to **~48 MB RAM** when idling in the tray (over 61% reduction).
- **CPU Overhead:** **0.00% background CPU** usage—event-driven architecture with zero polling loops.

---

## 📥 Download

Pre-compiled, standalone, portable executables are available on the [Releases](https://github.com/Grekto-dev/lucent-display-control/releases) page:

1. Download **`LucentControl.exe`** from the latest release.
2. Run it directly—no installation or administrative privileges required.
3. (Optional) In **Settings**, enable *Start with Windows* and *Start minimized to tray*.

---

## 🔨 Building from Source

Lucent Display Control is built in C# with WPF on the standard **.NET Framework 4.8**. It has zero external package dependencies and can be compiled natively using the Microsoft Visual C# Compiler (`csc.exe`) included with Windows:

### Prerequisites
- Windows 10 or Windows 11
- .NET Framework 4.8 (pre-installed on all modern Windows installations)
- Git

### Build Instructions
Clone the repository and run the build script:

```cmd
git clone https://github.com/Grekto-dev/lucent-display-control.git
cd lucent-display-control
build.bat
```

The compiled binary will be generated at `./LucentControl.exe`.

### Running the Automated Test Suite
To execute the comprehensive unit test suite covering icon loading, config persistence, profile management, matrix calculations, and overlay logic:

```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /out:RunTests.exe /r:LucentControl.exe /r:System.Xaml.dll /r:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll /r:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll /r:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll tools\RunTests.cs
RunTests.exe
del RunTests.exe
```

---

## 🏛 Architecture

- **`MainWindow.cs` / `MainWindow.Views.cs`:** WPF Fluent-styled interface, dark mode palette (`#0B0E14` base, `#141924` cards, `#38BDF8` accent), custom control templates, and high-DPI assets.
- **`MainWindow.Logic.cs`:** Monitor enumeration (`EnumDisplayMonitors`, `GetMonitorInfo`), synchronization logic, preset transitions, tray icon integration, and memory optimization.
- **`MainWindow.Settings.cs`:** Windows 11 Settings UI layout, language dynamic switches, startup registry integration (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`), and dialog flows.
- **`Models.cs`:** 
  - `DwmEngine`: Computes 5×5 hardware color transformation matrices applying contrast offset, gamma midtone lift, Kelvin color temperature gains, and brightness scaling.
  - `ProfileManager`: Thread-safe persistence of user presets with atomic writes.
- **`Overlay.cs`:** Transparent, hit-test invisible layered WPF overlay windows supporting per-monitor luminance boosts and color tinting on non-DWM displays.
- **`AppIcon.cs`:** Embedded vector multi-resolution icon renderer (`16x16`, `24x24`, `32x32`, `48x48`, `64x64`, `128x128`, `256x256`).

---

## 📄 License

Licensed under the **Apache License, Version 2.0**. See the [LICENSE](LICENSE) file for the full license text.

```
Copyright 2026 Lucent Display Control Contributors

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0
```
