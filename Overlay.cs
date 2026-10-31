using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using SWF = System.Windows.Forms;

namespace LucentControl {
    public class ScreenOverlay : Window {
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_LAYERED = 0x00080000;

        private Rectangle dimRect;
        private Rectangle tintRect;
        private Rectangle rgbRect;

        public ScreenOverlay(int left, int top, int width, int height) {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Topmost = true;
            ShowInTaskbar = false;
            Focusable = false;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Background = Brushes.Transparent;

            Grid grid = new Grid();

            dimRect = new Rectangle();
            dimRect.Fill = Brushes.Black;
            dimRect.Opacity = 0.0;
            grid.Children.Add(dimRect);

            tintRect = new Rectangle();
            tintRect.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
            tintRect.Opacity = 0.0;
            grid.Children.Add(tintRect);

            rgbRect = new Rectangle();
            rgbRect.Fill = Brushes.Black;
            rgbRect.Opacity = 0.0;
            grid.Children.Add(rgbRect);

            Content = grid;

            SourceInitialized += delegate {
                try {
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED);
                } catch { }
            };
        }

        public void ApplySettings(DisplaySettings s) {
            // 1. Brightness overlay
            if (s.Brightness < 1.0) {
                dimRect.Fill = Brushes.Black;
                // Scale from 1.0 down to 0.10 -> Opacity from 0.0 up to 0.90
                dimRect.Opacity = Math.Min(0.92, Math.Max(0.0, (1.0 - s.Brightness) * 0.95));
            } else if (s.Brightness > 1.0) {
                dimRect.Fill = Brushes.White;
                dimRect.Opacity = Math.Min(0.35, (s.Brightness - 1.0) * 0.40);
            } else {
                dimRect.Opacity = 0.0;
            }

            // 2. Color Temperature overlay
            if (s.Temperature < 6400.0) {
                double factor = (6500.0 - s.Temperature) / (6500.0 - 2500.0);
                tintRect.Fill = new SolidColorBrush(Color.FromRgb(255, 135, 15));
                tintRect.Opacity = Math.Min(0.55, factor * 0.45);
            } else if (s.Temperature > 6600.0) {
                double factor = (s.Temperature - 6500.0) / (9500.0 - 6500.0);
                tintRect.Fill = new SolidColorBrush(Color.FromRgb(60, 150, 255));
                tintRect.Opacity = Math.Min(0.35, factor * 0.25);
            } else {
                tintRect.Opacity = 0.0;
            }

            // 3. RGB Channel Tinting
            double rLoss = Math.Max(0.0, 1.0 - s.Red);
            double gLoss = Math.Max(0.0, 1.0 - s.Green);
            double bLoss = Math.Max(0.0, 1.0 - s.Blue);

            if (rLoss > 0.05 || gLoss > 0.05 || bLoss > 0.05) {
                // Complementary tinting
                byte tintR = (byte)Math.Round(rLoss * 180.0);
                byte tintG = (byte)Math.Round(gLoss * 180.0);
                byte tintB = (byte)Math.Round(bLoss * 180.0);
                rgbRect.Fill = new SolidColorBrush(Color.FromRgb((byte)(255 - tintR), (byte)(255 - tintG), (byte)(255 - tintB)));
                rgbRect.Opacity = Math.Min(0.40, Math.Max(rLoss, Math.Max(gLoss, bLoss)) * 0.35);
            } else {
                rgbRect.Opacity = 0.0;
            }
        }
    }

    public class OverlayManager {
        private Dictionary<string, ScreenOverlay> overlays;

        public OverlayManager() {
            overlays = new Dictionary<string, ScreenOverlay>();
            InitOverlays();
        }

        public void InitOverlays() {
            CloseAll();
            try {
                foreach (SWF.Screen s in SWF.Screen.AllScreens) {
                    ScreenOverlay ov = new ScreenOverlay(s.Bounds.X, s.Bounds.Y, s.Bounds.Width, s.Bounds.Height);
                    ov.Show();
                    overlays[s.DeviceName] = ov;
                }
            } catch { }
        }

        public void UpdateDisplay(string deviceName, DisplaySettings s) {
            ScreenOverlay ov;
            if (overlays.TryGetValue(deviceName, out ov)) {
                ov.ApplySettings(s);
            }
        }

        public void UpdateAll(DisplaySettings s) {
            foreach (var kvp in overlays) {
                kvp.Value.ApplySettings(s);
            }
        }

        public void CloseAll() {
            foreach (var kvp in overlays) {
                try {
                    kvp.Value.Close();
                } catch { }
            }
            overlays.Clear();
        }
    }
}
