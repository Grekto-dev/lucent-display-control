# ☀ Lucent Display Control

**Version 0.1.0**

A modern Windows 11-style native desktop application designed for precise control over **Brightness, Contrast, Gamma, Color Temperature, and Individual RGB Channels** across multiple monitors, built specifically to support **DisplayLink USB** adapters, external docks, and integrated screens without physical adjustment buttons.

---

## ✨ Features

* **Universal DisplayLink & Video Output Compatibility:**
  * Uses a high-performance hybrid engine combining **WPF Layered Click-Through Overlays** (`WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE`) and **Windows GDI Gamma Ramps**.
  * Solves the issue where DisplayLink USB video adapters ignore hardware DDC/CI or GPU control panels.
* **Windows 11 Visual Monitor Layout:**
  * Interactive monitor cards displaying active screens, resolutions (e.g. `1920 × 1080`), and primary/secondary designations.
  * Click any monitor card to adjust it independently, or select **All Displays (Synced)** to synchronize everything.
* **Fine-Grained Picture & Color Controls:**
  * 💡 **Brightness:** 10% to 150% with live percentage badge.
  * ⚡ **Contrast:** 20% to 200%.
  * 🌀 **Gamma:** 0.40 to 2.50.
  * 🌡 **Color Temperature:** 2500 K (warm/amber anti-fatigue) to 9500 K (cool/blue), with neutral at 6500 K.
  * 🎨 **Live Color Preview:** Real-time visual indicator of the resulting color mix.
  * 🔴🟢🔵 **RGB Channels:** Independent Red, Green, and Blue sliders (0% to 100%).
* **Quick Presets:**
  * ☀ **Default:** 100% neutral values.
  * 🌙 **Night:** Warm light, reduced brightness (65%), and blue light filtration.
  * 📖 **Reading:** Soft contrast and cozy paper tone.
  * 🎬 **Cinema:** Deep blacks and increased contrast (120%).
  * 🔄 **Reset:** Instantly resets to factory baseline.
* **Windows 11 Settings Page:**
  * 🌐 **Language Selector:** Supports 7 languages:
    * English (Default)
    * Português (Brasil)
    * Español
    * Français
    * Italiano
    * 中文 (Chinese)
    * 日本語 (Japanese)
    * *Instant preview on language selection; permanently saved only when clicking Apply or OK.*
  * 🚀 **Start with Windows:** Toggle option to start automatically on PC boot.
    * Includes an indented sub-item **Start minimized to system tray** (disabled/greyed out unless autostart is enabled).
  * 📥 **Minimize to Tray on Close:** Enabled by default. If unchecked, clicking the window close button exits the program.
  * ℹ **About Section:** Displays app version `0.1.0` and details.
  * 💾 **Action Buttons:** `Apply`, `OK`, and `Cancel` (with rollback to committed state).
* **System Tray:**
  * Custom luminous icon with right-click context menu (localized).
  * Double-click to restore window.

---

## 🛠 Compilation & Architecture

Built with C# and WPF on .NET Framework 4.8. Can be compiled natively on any Windows machine using `csc.exe` without installing external SDKs:

```powershell
.\build.bat
```

To run:
```powershell
.\start.bat
# or launch LucentControl.exe directly
```

---

## 📦 Git Repository

Version control is initialized in this repository:
* `main` branch tracks the project history.
