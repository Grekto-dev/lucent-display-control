using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using SD = System.Drawing;

namespace LucentControl {
    public static class NativeMethods {
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEVMODE {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void EnableDarkMode(IntPtr hwnd) {
            int trueVal = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueVal, sizeof(int));
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref trueVal, sizeof(int));
        }

        /// <summary>Forces the OS to page out unused memory. Used while the app idles in the tray.</summary>
        public static void TrimWorkingSet() {
            try {
                GC.Collect(2, GCCollectionMode.Forced, true);
                SetProcessWorkingSetSize(GetCurrentProcess(), new IntPtr(-1), new IntPtr(-1));
            } catch { }
        }
    }

    /// <summary>Central place for every path the app writes to (overridable for tests).</summary>
    public static class AppPaths {
        public static string DataDirOverride;

        public static string DataDir {
            get {
                if (DataDirOverride != null) return DataDirOverride;
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LucentControl");
            }
        }
        public static string ConfigFile { get { return Path.Combine(DataDir, "config.ini"); } }
        public static string ProfilesFile { get { return Path.Combine(DataDir, "profiles.ini"); } }

        public static void WriteAtomic(string path, string[] lines) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string tmp = path + ".tmp";
            File.WriteAllLines(tmp, lines, new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(tmp, path, null);
            else File.Move(tmp, path);
        }

        public static void DeleteAll() {
            try {
                if (Directory.Exists(DataDir)) Directory.Delete(DataDir, true);
            } catch {
                try { File.Delete(ConfigFile); } catch { }
                try { File.Delete(ProfilesFile); } catch { }
            }
        }
    }

    /// <summary>
    /// Applies contrast, gamma, brightness, color temperature and RGB gains with the DWM
    /// full-screen color matrix (works on every adapter, including DisplayLink). The matrix is
    /// reused between calls so slider drags do not allocate, and identical matrices are skipped.
    /// </summary>
    public static class DwmEngine {
        [DllImport("magnification.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MagInitialize();

        [DllImport("magnification.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MagUninitialize();

        [StructLayout(LayoutKind.Sequential)]
        private struct MAGCOLOREFFECT {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public float[] transform;
        }

        [DllImport("magnification.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MagSetFullscreenColorEffect(ref MAGCOLOREFFECT pEffect);

        private static bool isInitialized;
        private static bool hasLast;
        private static MAGCOLOREFFECT effect = new MAGCOLOREFFECT { transform = new float[25] };
        private static readonly float[] last = new float[25];

        /// <summary>Builds the 5x5 matrix for the given settings. Exposed for tests.</summary>
        public static void BuildMatrix(DisplaySettings s, bool includeBrightnessAndTemp, float[] m) {
            double brightness = includeBrightnessAndTemp ? s.Brightness : 1.0;
            double kelvin = includeBrightnessAndTemp ? s.Temperature : 6500.0;

            double tr, tg, tb;
            GammaMath.CalculateColorTemp(kelvin, out tr, out tg, out tb);

            float c = (float)s.Contrast;
            // Midtone lift: raises midtones when gamma > 1, crushes them when gamma < 1
            float gammaOffset = (float)(Math.Pow(0.5, 1.0 / Math.Max(0.1, s.Gamma)) - 0.5);
            float cOffset = 0.5f * (1.0f - c) + gammaOffset;

            float rGain = (float)(s.Red * tr * brightness);
            float gGain = (float)(s.Green * tg * brightness);
            float bGain = (float)(s.Blue * tb * brightness);

            Array.Clear(m, 0, 25);
            m[0] = c * rGain;
            m[6] = c * gGain;
            m[12] = c * bGain;
            m[18] = 1.0f;
            m[20] = cOffset * rGain;
            m[21] = cOffset * gGain;
            m[22] = cOffset * bGain;
            m[24] = 1.0f;
        }

        private static bool IsIdentity(float[] m) {
            for (int i = 0; i < 25; i++) {
                float expected = (i % 6 == 0) ? 1.0f : 0.0f;
                if (Math.Abs(m[i] - expected) > 0.0005f) return false;
            }
            return true;
        }

        /// <param name="full">true: brightness + temperature included. false: only the channels that are
        /// shared by every monitor (contrast, gamma, RGB) - used when brightness/temperature are per-monitor.</param>
        public static void Apply(DisplaySettings s, bool full) {
            float[] m = effect.transform;
            BuildMatrix(s, full, m);

            if (hasLast && SameAsLast(m)) return;
            if (!isInitialized) {
                if (IsIdentity(m)) return; // nothing to do, don't even load the Magnification runtime
                isInitialized = MagInitialize();
                if (!isInitialized) return;
            }
            if (MagSetFullscreenColorEffect(ref effect)) {
                Array.Copy(m, last, 25);
                hasLast = true;
            }
        }

        private static bool SameAsLast(float[] m) {
            for (int i = 0; i < 25; i++) {
                if (m[i] != last[i]) return false;
            }
            return true;
        }

        public static void Reset() {
            if (isInitialized) {
                float[] m = effect.transform;
                Array.Clear(m, 0, 25);
                m[0] = m[6] = m[12] = m[18] = m[24] = 1.0f;
                MagSetFullscreenColorEffect(ref effect);
                MagUninitialize();
                isInitialized = false;
            }
            hasLast = false;
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
    }

    public class ScreenDevice {
        public int Number { get; set; }
        public string DeviceName { get; set; }
        public string FriendlyName { get; set; }
        public bool IsPrimary { get; set; }
        public SD.Rectangle Bounds { get; set; }
        public int RefreshRate { get; set; }
        public int BitsPerPixel { get; set; }

        public ScreenDevice() {
            RefreshRate = 60;
            BitsPerPixel = 32;
        }

        public string DisplayLabel {
            get { return string.Format("{0} {1}", I18n.T("Monitor"), Number); }
        }

        public string ResolutionText {
            get { return string.Format("{0} × {1} · {2} Hz", Bounds.Width, Bounds.Height, RefreshRate); }
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

        public DisplaySettings() {
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

        public void CopyFrom(DisplaySettings o) {
            Brightness = o.Brightness;
            Contrast = o.Contrast;
            Gamma = o.Gamma;
            Temperature = o.Temperature;
            Red = o.Red;
            Green = o.Green;
            Blue = o.Blue;
        }

        public DisplaySettings Clone() {
            DisplaySettings s = new DisplaySettings();
            s.CopyFrom(this);
            return s;
        }

        public bool NearlyEquals(DisplaySettings o) {
            return Math.Abs(Brightness - o.Brightness) < 0.005
                && Math.Abs(Contrast - o.Contrast) < 0.005
                && Math.Abs(Gamma - o.Gamma) < 0.005
                && Math.Abs(Temperature - o.Temperature) < 5.0
                && Math.Abs(Red - o.Red) < 0.005
                && Math.Abs(Green - o.Green) < 0.005
                && Math.Abs(Blue - o.Blue) < 0.005;
        }

        public bool IsDefault() {
            return NearlyEquals(new DisplaySettings());
        }
    }

    public class AppConfig {
        public AppLanguage Language = AppLanguage.English;
        public bool StartWithWindows = false;
        public bool StartMinimized = false;
        public bool MinimizeOnClose = true;

        // Profile saving behaviour
        public bool AutoSave = false;
        public bool AutoApplyProfile = false;
        public string StartupProfile = "Standard";

        // Scheduler
        public bool SchedulerEnabled = false;
        public bool SchedulerNotify = true;
        public int SlotDayHour = 8;
        public int SlotDayMinute = 0;
        public string SlotDayProfile = "Work";
        public int SlotSunsetHour = 18;
        public int SlotSunsetMinute = 30;
        public string SlotSunsetProfile = "Warm";
        public int SlotNightHour = 22;
        public int SlotNightMinute = 0;
        public string SlotNightProfile = "Night";

        private static int Clamp(int v, int lo, int hi) {
            return v < lo ? lo : (v > hi ? hi : v);
        }

        public static AppConfig Load() {
            AppConfig cfg = new AppConfig();
            try {
                if (!File.Exists(AppPaths.ConfigFile)) return cfg;
                foreach (string line in File.ReadAllLines(AppPaths.ConfigFile)) {
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();
                    bool b;
                    int i;

                    switch (key) {
                        case "Language": if (int.TryParse(val, out i) && i >= 0 && i <= 6) cfg.Language = (AppLanguage)i; break;
                        case "StartWithWindows": if (bool.TryParse(val, out b)) cfg.StartWithWindows = b; break;
                        case "StartMinimized": if (bool.TryParse(val, out b)) cfg.StartMinimized = b; break;
                        case "MinimizeOnClose": if (bool.TryParse(val, out b)) cfg.MinimizeOnClose = b; break;
                        case "AutoSave": if (bool.TryParse(val, out b)) cfg.AutoSave = b; break;
                        case "AutoApplyProfile": if (bool.TryParse(val, out b)) cfg.AutoApplyProfile = b; break;
                        case "StartupProfile": if (val.Length > 0) cfg.StartupProfile = val; break;
                        case "SchedulerEnabled": if (bool.TryParse(val, out b)) cfg.SchedulerEnabled = b; break;
                        case "SchedulerNotify": if (bool.TryParse(val, out b)) cfg.SchedulerNotify = b; break;
                        case "SlotDayHour": if (int.TryParse(val, out i)) cfg.SlotDayHour = Clamp(i, 0, 23); break;
                        case "SlotDayMinute": if (int.TryParse(val, out i)) cfg.SlotDayMinute = Clamp(i, 0, 59); break;
                        case "SlotDayProfile": if (val.Length > 0) cfg.SlotDayProfile = val; break;
                        case "SlotSunsetHour": if (int.TryParse(val, out i)) cfg.SlotSunsetHour = Clamp(i, 0, 23); break;
                        case "SlotSunsetMinute": if (int.TryParse(val, out i)) cfg.SlotSunsetMinute = Clamp(i, 0, 59); break;
                        case "SlotSunsetProfile": if (val.Length > 0) cfg.SlotSunsetProfile = val; break;
                        case "SlotNightHour": if (int.TryParse(val, out i)) cfg.SlotNightHour = Clamp(i, 0, 23); break;
                        case "SlotNightMinute": if (int.TryParse(val, out i)) cfg.SlotNightMinute = Clamp(i, 0, 59); break;
                        case "SlotNightProfile": if (val.Length > 0) cfg.SlotNightProfile = val; break;
                    }
                }
            } catch { }
            return cfg;
        }

        public void Save() {
            try {
                List<string> lines = new List<string>();
                lines.Add("Language=" + (int)Language);
                lines.Add("StartWithWindows=" + StartWithWindows);
                lines.Add("StartMinimized=" + StartMinimized);
                lines.Add("MinimizeOnClose=" + MinimizeOnClose);
                lines.Add("AutoSave=" + AutoSave);
                lines.Add("AutoApplyProfile=" + AutoApplyProfile);
                lines.Add("StartupProfile=" + StartupProfile);
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
                AppPaths.WriteAtomic(AppPaths.ConfigFile, lines.ToArray());
            } catch { }
        }

        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValue = "LucentDisplayControl";

        public static void SetAutoStart(bool enable, bool minimized) {
            try {
                using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(RunKey)) {
                    if (enable) {
                        string exePath = System.Reflection.Assembly.GetEntryAssembly() != null
                            ? System.Reflection.Assembly.GetEntryAssembly().Location
                            : System.Reflection.Assembly.GetExecutingAssembly().Location;
                        string cmd = "\"" + exePath + "\"" + (minimized ? " /minimized" : "");
                        rk.SetValue(RunValue, cmd);
                    } else {
                        rk.DeleteValue(RunValue, false);
                    }
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
            return s;
        }

        public void SetFrom(DisplaySettings s) {
            Brightness = s.Brightness;
            Contrast = s.Contrast;
            Gamma = s.Gamma;
            Temperature = s.Temperature;
            Red = s.Red;
            Green = s.Green;
            Blue = s.Blue;
        }

        public bool Matches(DisplaySettings s) {
            return ToSettings().NearlyEquals(s);
        }

        /// <summary>Localization key of the one-line description (built-in presets only).</summary>
        public string DescriptionKey { get { return "Desc" + Name; } }
    }

    public class ProfileManager {
        public const int MaxNameLength = 40;
        private readonly List<UserProfile> profiles = new List<UserProfile>();

        public IList<UserProfile> Profiles { get { return profiles; } }

        public ProfileManager() {
            LoadAll();
        }

        public static List<UserProfile> CreateDefaults() {
            List<UserProfile> l = new List<UserProfile>();
            l.Add(new UserProfile("Standard", 1.00, 1.00, 1.00, 6500, 1.00, 1.00, 1.00, true));
            l.Add(new UserProfile("Warm", 0.85, 0.95, 1.05, 4500, 1.00, 0.95, 0.85, true));
            l.Add(new UserProfile("Cool", 1.00, 1.10, 1.00, 8000, 0.92, 0.96, 1.00, true));
            l.Add(new UserProfile("Movie", 0.90, 1.15, 1.15, 5800, 1.00, 0.95, 0.90, true));
            l.Add(new UserProfile("Work", 0.80, 1.00, 1.00, 6000, 1.00, 1.00, 0.96, true));
            l.Add(new UserProfile("Night", 0.65, 0.90, 1.00, 3400, 1.00, 0.84, 0.68, true));
            l.Add(new UserProfile("Gaming", 1.00, 1.20, 1.25, 7000, 1.00, 1.00, 1.00, true));
            l.Add(new UserProfile("Focus", 0.95, 1.30, 1.00, 6500, 1.00, 1.00, 1.00, true));
            return l;
        }

        private static double Clamp(double v, double lo, double hi) {
            return v < lo ? lo : (v > hi ? hi : v);
        }

        /// <summary>Removes characters that would break the file format and limits the length. Null if empty.</summary>
        public static string SanitizeName(string raw) {
            if (raw == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in raw) {
                if (ch == '|' || ch == '\r' || ch == '\n' || ch == '\t') sb.Append(' ');
                else sb.Append(ch);
            }
            string s = sb.ToString().Trim();
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            if (s.Length > MaxNameLength) s = s.Substring(0, MaxNameLength).Trim();
            return s.Length == 0 ? null : s;
        }

        public void LoadAll() {
            profiles.Clear();
            profiles.AddRange(CreateDefaults());

            try {
                if (!File.Exists(AppPaths.ProfilesFile)) return;
                CultureInfo inv = CultureInfo.InvariantCulture;
                foreach (string line in File.ReadAllLines(AppPaths.ProfilesFile)) {
                    if (line.Length == 0 || line[0] == '#') continue;
                    string[] p = line.Split('|');
                    if (p.Length < 8) continue;
                    string name = SanitizeName(p[0]);
                    if (name == null) continue;

                    double[] v = new double[7];
                    bool ok = true;
                    for (int i = 0; i < 7 && ok; i++) {
                        ok = double.TryParse(p[i + 1], NumberStyles.Float, inv, out v[i]);
                    }
                    if (!ok) continue;

                    DisplaySettings s = new DisplaySettings();
                    s.Brightness = Clamp(v[0], 0.00, 2.00);
                    s.Contrast = Clamp(v[1], 0.20, 2.00);
                    s.Gamma = Clamp(v[2], 0.50, 2.50);
                    s.Temperature = Clamp(v[3], 2500, 9500);
                    s.Red = Clamp(v[4], 0, 1);
                    s.Green = Clamp(v[5], 0, 1);
                    s.Blue = Clamp(v[6], 0, 1);

                    UserProfile existing = Find(name);
                    if (existing != null) existing.SetFrom(s);   // override of a built-in (or duplicate line)
                    else profiles.Add(new UserProfile(name, s.Brightness, s.Contrast, s.Gamma, s.Temperature, s.Red, s.Green, s.Blue, false));
                }
            } catch { }
        }

        /// <summary>Persists custom profiles plus any built-in preset the user changed.</summary>
        public void Save() {
            try {
                CultureInfo inv = CultureInfo.InvariantCulture;
                List<UserProfile> defaults = CreateDefaults();
                List<string> lines = new List<string>();
                foreach (UserProfile p in profiles) {
                    if (p.IsBuiltIn) {
                        UserProfile def = defaults.Find(delegate(UserProfile d) { return d.Name == p.Name; });
                        if (def != null && def.Matches(p.ToSettings())) continue;
                    }
                    lines.Add(string.Format(inv, "{0}|{1:F3}|{2:F3}|{3:F3}|{4:F0}|{5:F3}|{6:F3}|{7:F3}|{8}",
                        p.Name, p.Brightness, p.Contrast, p.Gamma, p.Temperature, p.Red, p.Green, p.Blue, p.IsBuiltIn ? "B" : "C"));
                }
                if (lines.Count == 0) {
                    if (File.Exists(AppPaths.ProfilesFile)) File.Delete(AppPaths.ProfilesFile);
                } else {
                    AppPaths.WriteAtomic(AppPaths.ProfilesFile, lines.ToArray());
                }
            } catch { }
        }

        public UserProfile Find(string name) {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (UserProfile p in profiles) {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return p;
            }
            return null;
        }

        public bool Exists(string name) {
            return Find(name) != null;
        }

        /// <summary>Creates the profile or overwrites the existing one (built-ins included).</summary>
        public UserProfile SaveProfile(string name, DisplaySettings s) {
            name = SanitizeName(name);
            if (name == null) return null;
            UserProfile p = Find(name);
            if (p != null) {
                p.SetFrom(s);
            } else {
                p = new UserProfile(name, s.Brightness, s.Contrast, s.Gamma, s.Temperature, s.Red, s.Green, s.Blue, false);
                profiles.Add(p);
            }
            Save();
            return p;
        }

        /// <summary>Deletes a custom profile. Built-in presets cannot be deleted.</summary>
        public bool DeleteProfile(string name) {
            UserProfile p = Find(name);
            if (p == null || p.IsBuiltIn) return false;
            profiles.Remove(p);
            Save();
            return true;
        }

        /// <summary>Resets every built-in preset to its factory values. Custom profiles are kept.</summary>
        public void RestoreDefaults() {
            foreach (UserProfile def in CreateDefaults()) {
                UserProfile p = Find(def.Name);
                if (p != null && p.IsBuiltIn) {
                    p.SetFrom(def.ToSettings());
                } else {
                    // a custom profile was using a built-in name - put the preset back in front
                    if (p != null) profiles.Remove(p);
                    profiles.Insert(0, def);
                }
            }
            Save();
        }

        public bool IsModified(UserProfile p) {
            if (p == null || !p.IsBuiltIn) return false;
            UserProfile def = CreateDefaults().Find(delegate(UserProfile d) { return d.Name == p.Name; });
            return def != null && !def.Matches(p.ToSettings());
        }

        public string UniqueName(string baseName) {
            string name = baseName;
            int n = 2;
            while (Exists(name)) {
                name = baseName + " " + n++;
            }
            return name;
        }
    }

    /// <summary>Pure scheduling logic (no UI) so it can be unit-tested.</summary>
    public static class SchedulerLogic {
        /// <summary>
        /// Returns the index of the slot whose start time passed most recently (wrapping past midnight),
        /// so any ordering of the slot times works, e.g. a night slot at 01:00.
        /// </summary>
        public static int CurrentSlot(TimeSpan now, TimeSpan[] starts) {
            int best = 0;
            double bestElapsed = double.MaxValue;
            for (int i = 0; i < starts.Length; i++) {
                double elapsed = (now - starts[i]).TotalMinutes;
                if (elapsed < 0) elapsed += 24 * 60;
                if (elapsed < bestElapsed) {
                    bestElapsed = elapsed;
                    best = i;
                }
            }
            return best;
        }
    }
}
