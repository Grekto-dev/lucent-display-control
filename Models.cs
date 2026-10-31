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

        // Scheduler Settings
        public bool SchedulerEnabled = false;
        public bool SchedulerNotify = true;
        public int SlotDayHour = 8;
        public int SlotDayMinute = 0;
        public string SlotDayProfile = "Office / Work";
        public int SlotSunsetHour = 18;
        public int SlotSunsetMinute = 30;
        public string SlotSunsetProfile = "Warm / Reading";
        public int SlotNightHour = 22;
        public int SlotNightMinute = 0;
        public string SlotNightProfile = "Night Shift";

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
                        // Scheduler
                        else if (key == "SchedulerEnabled" && bool.TryParse(val, out b)) cfg.SchedulerEnabled = b;
                        else if (key == "SchedulerNotify" && bool.TryParse(val, out b)) cfg.SchedulerNotify = b;
                        else if (key == "SlotDayHour" && int.TryParse(val, out i)) cfg.SlotDayHour = i;
                        else if (key == "SlotDayMinute" && int.TryParse(val, out i)) cfg.SlotDayMinute = i;
                        else if (key == "SlotDayProfile") cfg.SlotDayProfile = val;
                        else if (key == "SlotSunsetHour" && int.TryParse(val, out i)) cfg.SlotSunsetHour = i;
                        else if (key == "SlotSunsetMinute" && int.TryParse(val, out i)) cfg.SlotSunsetMinute = i;
                        else if (key == "SlotSunsetProfile") cfg.SlotSunsetProfile = val;
                        else if (key == "SlotNightHour" && int.TryParse(val, out i)) cfg.SlotNightHour = i;
                        else if (key == "SlotNightMinute" && int.TryParse(val, out i)) cfg.SlotNightMinute = i;
                        else if (key == "SlotNightProfile") cfg.SlotNightProfile = val;
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
                // Scheduler
                lines.Add("SchedulerEnabled=" + SchedulerEnabled);
                lines.Add("SchedulerNotify=" + SchedulerNotify);
                lines.Add("SlotDayHour=" + SlotDayHour);
                lines.Add("SlotDayMinute=" + SlotDayMinute);
                lines.Add("SlotDayProfile=" + SlotDayProfile);
                lines.Add("SlotSunsetHour=" + SlotSunsetHour);
                lines.Add("SlotSunsetMinute=" + SlotSunsetMinute);
                lines.Add("SlotSunsetProfile=" + SlotSunsetProfile);
                lines.Add("SlotNightHour=" + SlotNightHour);
                lines.Add("SlotNightMinute=" + SlotNightMinute);
                lines.Add("SlotNightProfile=" + SlotNightProfile);
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

    public class UserProfile {
        public string Name { get; set; }
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        public double Gamma { get; set; }
        public double Temperature { get; set; }
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
        public bool IsBuiltIn { get; set; }

        public UserProfile() {
            Brightness = 1.0;
            Contrast = 1.0;
            Gamma = 1.0;
            Temperature = 6500.0;
            Red = 1.0;
            Green = 1.0;
            Blue = 1.0;
            IsBuiltIn = false;
        }

        public UserProfile(string name, double b, double c, double g, double t, double red, double green, double blue, bool isBuiltIn) {
            Name = name;
            Brightness = b;
            Contrast = c;
            Gamma = g;
            Temperature = t;
            Red = red;
            Green = green;
            Blue = blue;
            IsBuiltIn = isBuiltIn;
        }

        public DisplaySettings ToSettings() {
            DisplaySettings s = new DisplaySettings();
            s.Brightness = Brightness;
            s.Contrast = Contrast;
            s.Gamma = Gamma;
            s.Temperature = Temperature;
            s.Red = Red;
            s.Green = Green;
            s.Blue = Blue;
            s.ProfileName = Name;
            return s;
        }
    }

    public class ProfileManager {
        public List<UserProfile> Profiles { get; private set; }

        public ProfileManager() {
            Profiles = new List<UserProfile>();
            LoadAll();
        }

        public void LoadAll() {
            Profiles.Clear();
            // 1. Built-in standard profiles
            Profiles.Add(new UserProfile("Standard", 1.0, 1.0, 1.0, 6500.0, 1.0, 1.0, 1.0, true));
            Profiles.Add(new UserProfile("Warm", 0.85, 0.95, 1.05, 4500.0, 1.0, 0.95, 0.85, true));
            Profiles.Add(new UserProfile("Cool", 1.0, 1.10, 1.0, 8000.0, 0.92, 0.96, 1.0, true));
            Profiles.Add(new UserProfile("Movie", 0.90, 1.15, 1.15, 5800.0, 1.0, 0.95, 0.90, true));
            Profiles.Add(new UserProfile("Work", 0.80, 1.0, 1.0, 6000.0, 1.0, 1.0, 0.96, true));
            Profiles.Add(new UserProfile("Night", 0.65, 0.90, 1.0, 3400.0, 1.0, 0.84, 0.68, true));
            Profiles.Add(new UserProfile("Gaming", 1.0, 1.20, 1.25, 7000.0, 1.0, 1.0, 1.0, true));
            Profiles.Add(new UserProfile("Focus", 0.95, 1.30, 1.0, 6500.0, 1.0, 1.0, 1.0, true));

            // 2. Load custom user profiles from profiles.ini
            try {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LucentControl");
                string path = Path.Combine(dir, "profiles.ini");
                if (File.Exists(path)) {
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines) {
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                        string[] p = line.Split('|');
                        if (p.Length >= 8) {
                            UserProfile up = new UserProfile();
                            up.Name = p[0];
                            up.Brightness = double.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture);
                            up.Contrast = double.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture);
                            up.Gamma = double.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture);
                            up.Temperature = double.Parse(p[4], System.Globalization.CultureInfo.InvariantCulture);
                            up.Red = double.Parse(p[5], System.Globalization.CultureInfo.InvariantCulture);
                            up.Green = double.Parse(p[6], System.Globalization.CultureInfo.InvariantCulture);
                            up.Blue = double.Parse(p[7], System.Globalization.CultureInfo.InvariantCulture);
                            up.IsBuiltIn = false;
                            Profiles.Add(up);
                        }
                    }
                }
            } catch { }
        }

        public void SaveCustom() {
            try {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LucentControl");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "profiles.ini");
                List<string> lines = new List<string>();
                foreach (UserProfile p in Profiles) {
                    if (!p.IsBuiltIn) {
                        lines.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "{0}|{1:F2}|{2:F2}|{3:F2}|{4:F0}|{5:F2}|{6:F2}|{7:F2}",
                            p.Name, p.Brightness, p.Contrast, p.Gamma, p.Temperature, p.Red, p.Green, p.Blue));
                    }
                }
                File.WriteAllLines(path, lines.ToArray());
            } catch { }
        }

        public void AddOrUpdate(string name, DisplaySettings s) {
            UserProfile existing = Profiles.Find(delegate(UserProfile p) { return p.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
            if (existing != null) {
                existing.Brightness = s.Brightness;
                existing.Contrast = s.Contrast;
                existing.Gamma = s.Gamma;
                existing.Temperature = s.Temperature;
                existing.Red = s.Red;
                existing.Green = s.Green;
                existing.Blue = s.Blue;
            } else {
                UserProfile up = new UserProfile(name, s.Brightness, s.Contrast, s.Gamma, s.Temperature, s.Red, s.Green, s.Blue, false);
                Profiles.Add(up);
            }
            SaveCustom();
        }

        public bool DeleteProfile(string name) {
            UserProfile existing = Profiles.Find(delegate(UserProfile p) { return p.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
            if (existing != null && !existing.IsBuiltIn) {
                Profiles.Remove(existing);
                SaveCustom();
                return true;
            }
            return false;
        }

        public UserProfile Find(string name) {
            return Profiles.Find(delegate(UserProfile p) { return p.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
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
