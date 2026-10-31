using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SD = System.Drawing;

namespace LucentControl {
    public static class NativeMethods {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool GetDeviceGammaRamp(IntPtr hDC, byte[] lpRamp);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool SetDeviceGammaRamp(IntPtr hDC, byte[] lpRamp);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hDC);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void EnableDarkMode(IntPtr hwnd) {
            int trueVal = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueVal, sizeof(int));
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref trueVal, sizeof(int));
        }
    }

    public static class DwmEngine {
        [DllImport("magnification.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool MagInitialize();

        [DllImport("magnification.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool MagUninitialize();

        [StructLayout(LayoutKind.Sequential)]
        public struct MAGCOLOREFFECT {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public float[] transform;
        }

        [DllImport("magnification.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool MagSetFullscreenColorEffect(ref MAGCOLOREFFECT pEffect);

        private static bool isInitialized = false;

        public static void Apply(DisplaySettings s) {
            if (!isInitialized) {
                isInitialized = MagInitialize();
                if (!isInitialized) return;
            }

            double tr, tg, tb;
            GammaMath.CalculateColorTemp(s.Temperature, out tr, out tg, out tb);

            float c = (float)s.Contrast;
            float b = (float)s.Brightness;

            // Accurate gamma midtone offset: lifts midtones when gamma > 1.0, crushes when gamma < 1.0
            float gammaOffset = (float)(Math.Pow(0.5, 1.0 / Math.Max(0.1, s.Gamma)) - 0.5);
            float cOffset = 0.5f * (1.0f - c) + gammaOffset;

            float rGain = (float)(s.Red * tr * b);
            float gGain = (float)(s.Green * tg * b);
            float bGain = (float)(s.Blue * tb * b);

            MAGCOLOREFFECT eff = new MAGCOLOREFFECT();
            eff.transform = new float[25] {
                c * rGain,   0.0f,        0.0f,        0.0f, 0.0f,
                0.0f,        c * gGain,   0.0f,        0.0f, 0.0f,
                0.0f,        0.0f,        c * bGain,   0.0f, 0.0f,
                0.0f,        0.0f,        0.0f,        1.0f, 0.0f,
                cOffset * rGain, cOffset * gGain, cOffset * bGain, 0.0f, 1.0f
            };

            MagSetFullscreenColorEffect(ref eff);
        }

        public static void Reset() {
            if (isInitialized) {
                MAGCOLOREFFECT id = new MAGCOLOREFFECT();
                id.transform = new float[25] {
                    1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                    0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                    0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                    0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                    0.0f, 0.0f, 0.0f, 0.0f, 1.0f
                };
                MagSetFullscreenColorEffect(ref id);
                MagUninitialize();
                isInitialized = false;
            }
        }
    }

    public class ScreenDevice {
        public int Number { get; set; }
        public string DeviceName { get; set; }
        public string FriendlyName { get; set; }
        public bool IsPrimary { get; set; }
        public SD.Rectangle Bounds { get; set; }
        public int RefreshRate { get; set; }
        public byte[] OriginalRamp { get; set; }

        public ScreenDevice() {
            RefreshRate = 60;
        }

        public string DisplayLabel {
            get {
                return string.Format("{0} {1}", I18n.T("Monitor"), Number);
            }
        }

        public string ResolutionText {
            get {
                return string.Format("{0} × {1} · {2} Hz", Bounds.Width, Bounds.Height, RefreshRate);
            }
        }
    }

    public class DisplaySettings {
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        public double Gamma { get; set; }
        public double Temperature { get; set; }
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
        public string ProfileName { get; set; }

        public DisplaySettings() {
            ProfileName = "Standard";
            ResetToDefaults();
        }

        public void ResetToDefaults() {
            Brightness = 1.0;
            Contrast = 1.0;
            Gamma = 1.0;
            Temperature = 6500.0;
            Red = 1.0;
            Green = 1.0;
            Blue = 1.0;
        }

        public DisplaySettings Clone() {
            DisplaySettings s = new DisplaySettings();
            s.Brightness = this.Brightness;
            s.Contrast = this.Contrast;
            s.Gamma = this.Gamma;
            s.Temperature = this.Temperature;
            s.Red = this.Red;
            s.Green = this.Green;
            s.Blue = this.Blue;
            s.ProfileName = this.ProfileName;
            return s;
        }
    }

    public class AppConfig {
        public AppLanguage Language = AppLanguage.English;
        public bool StartWithWindows = false;
        public bool StartMinimized = false;
        public bool MinimizeOnClose = true;
        public bool SyncMonitors = true;
        public double Brightness = 1.0;
        public double Contrast = 1.0;
        public double Gamma = 1.0;
        public double Temperature = 6500.0;
        public double Red = 1.0;
        public double Green = 1.0;
        public double Blue = 1.0;
        public string ActiveProfile = "Standard";

        private static string GetConfigPath() {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LucentControl");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "config.ini");
        }

        public static AppConfig Load() {
            AppConfig cfg = new AppConfig();
            string path = GetConfigPath();
            if (!File.Exists(path)) return cfg;

            try {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines) {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2) {
                        string key = parts[0].Trim();
                        string val = parts[1].Trim();
                        double d;
                        bool b;
                        int i;

                        if (key == "Language" && int.TryParse(val, out i)) {
                            if (i >= 0 && i <= 6) cfg.Language = (AppLanguage)i;
                        }
                        else if (key == "StartWithWindows" && bool.TryParse(val, out b)) cfg.StartWithWindows = b;
                        else if (key == "StartMinimized" && bool.TryParse(val, out b)) cfg.StartMinimized = b;
                        else if (key == "MinimizeOnClose" && bool.TryParse(val, out b)) cfg.MinimizeOnClose = b;
                        else if (key == "SyncMonitors" && bool.TryParse(val, out b)) cfg.SyncMonitors = b;
                        else if (key == "ActiveProfile") cfg.ActiveProfile = val;
                        else if (key == "Brightness" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Brightness = d;
                        else if (key == "Contrast" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Contrast = d;
                        else if (key == "Gamma" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Gamma = d;
                        else if (key == "Temperature" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Temperature = d;
                        else if (key == "Red" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Red = d;
                        else if (key == "Green" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Green = d;
                        else if (key == "Blue" && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) cfg.Blue = d;
                    }
                }
            } catch { }
            return cfg;
        }

        public void Save() {
            try {
                string path = GetConfigPath();
                List<string> lines = new List<string>();
                lines.Add("Language=" + (int)Language);
                lines.Add("StartWithWindows=" + StartWithWindows);
                lines.Add("StartMinimized=" + StartMinimized);
                lines.Add("MinimizeOnClose=" + MinimizeOnClose);
                lines.Add("SyncMonitors=" + SyncMonitors);
                lines.Add("ActiveProfile=" + ActiveProfile);
                lines.Add("Brightness=" + Brightness.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Contrast=" + Contrast.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Gamma=" + Gamma.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Temperature=" + Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Red=" + Red.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Green=" + Green.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Blue=" + Blue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                File.WriteAllLines(path, lines.ToArray());
            } catch { }
        }

        public static void SetAutoStart(bool enable, bool minimized) {
            try {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (enable) {
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string cmd = "\"" + exePath + "\"";
                    if (minimized) {
                        cmd += " /minimized";
                    }
                    rk.SetValue("LucentDisplayControl", cmd);
                } else {
                    rk.DeleteValue("LucentDisplayControl", false);
                }
            } catch { }
        }
    }

    public static class GammaMath {
        public static void CalculateColorTemp(double kelvin, out double r, out double g, out double b) {
            if (Math.Abs(kelvin - 6500.0) < 50.0) {
                r = 1.0;
                g = 1.0;
                b = 1.0;
                return;
            }

            double temp = kelvin / 100.0;
            double red, green, blue;

            if (temp <= 66.0) {
                red = 255.0;
            } else {
                red = temp - 60.0;
                red = 329.698727446 * Math.Pow(red, -0.1332047592);
                if (red < 0.0) red = 0.0;
                if (red > 255.0) red = 255.0;
            }

            if (temp <= 66.0) {
                green = temp;
                green = 99.4708025861 * Math.Log(green) - 161.1195681661;
                if (green < 0.0) green = 0.0;
                if (green > 255.0) green = 255.0;
            } else {
                green = temp - 60.0;
                green = 288.1221695283 * Math.Pow(green, -0.0755148492);
                if (green < 0.0) green = 0.0;
                if (green > 255.0) green = 255.0;
            }

            if (temp >= 66.0) {
                blue = 255.0;
            } else if (temp <= 19.0) {
                blue = 0.0;
            } else {
                blue = temp - 10.0;
                blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;
                if (blue < 0.0) blue = 0.0;
                if (blue > 255.0) blue = 255.0;
            }

            r = red / 255.0;
            g = green / 255.0;
            b = blue / 255.0;
        }

        public static byte[] BuildGammaRamp(DisplaySettings settings) {
            byte[] ramp = new byte[1536];

            double tR, tG, tB;
            CalculateColorTemp(settings.Temperature, out tR, out tG, out tB);

            double[] channelGains = new double[] {
                settings.Red * tR,
                settings.Green * tG,
                settings.Blue * tB
            };

            for (int ch = 0; ch < 3; ch++) {
                double gain = channelGains[ch];
                for (int i = 0; i < 256; i++) {
                    double x = (double)i / 255.0;

                    if (settings.Gamma > 0.05) {
                        x = Math.Pow(x, 1.0 / settings.Gamma);
                    }

                    x = (x - 0.5) * settings.Contrast + 0.5;
                    x = x * settings.Brightness * gain;

                    if (x < 0.0) x = 0.0;
                    if (x > 1.0) x = 1.0;

                    ushort val = (ushort)Math.Round(x * 65535.0);
                    int offset = (ch * 512) + (i * 2);
                    ramp[offset] = (byte)(val & 0xFF);
                    ramp[offset + 1] = (byte)((val >> 8) & 0xFF);
                }
            }
            return ramp;
        }
    }
}
