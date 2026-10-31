using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
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
            // Brightness overlay (black for dimming, white for boost)
            if (s.Brightness < 1.0) {
                dimRect.Fill = Brushes.Black;
                dimRect.Opacity = Math.Min(0.92, Math.Max(0.0, (1.0 - s.Brightness) * 0.95));
            } else if (s.Brightness > 1.0) {
                dimRect.Fill = Brushes.White;
                dimRect.Opacity = Math.Min(0.35, (s.Brightness - 1.0) * 0.40);
            } else {
                dimRect.Opacity = 0.0;
            }

            // Color Temperature tint
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
        }
    }

    public class IdentifyOverlay : Window {
        public IdentifyOverlay(int monNumber, string resolution, SWF.Screen screen) {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Topmost = true;
            ShowInTaskbar = false;
            Focusable = false;
            Background = Brushes.Transparent;

            Width = 240;
            Height = 240;
            Left = screen.Bounds.X + (screen.Bounds.Width - Width) / 2;
            Top = screen.Bounds.Y + (screen.Bounds.Height - Height) / 2;

            Border card = new Border();
            card.CornerRadius = new CornerRadius(20);
            card.Background = new SolidColorBrush(Color.FromArgb(235, 15, 23, 42));
            card.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            card.BorderThickness = new Thickness(3);
            card.Padding = new Thickness(16);

            StackPanel sp = new StackPanel();
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock numTb = new TextBlock();
            numTb.Text = monNumber.ToString();
            numTb.FontSize = 84;
            numTb.FontWeight = FontWeights.Bold;
            numTb.Foreground = Brushes.White;
            numTb.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Children.Add(numTb);

            TextBlock resTb = new TextBlock();
            resTb.Text = resolution;
            resTb.FontSize = 14;
            resTb.FontWeight = FontWeights.SemiBold;
            resTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            resTb.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Children.Add(resTb);

            card.Child = sp;
            Content = card;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2.5);
            timer.Tick += delegate {
                timer.Stop();
                Close();
            };
            timer.Start();
        }
    }

    public enum CalibrationPatternType {
        White,
        Black,
        Grayscale,
        ColorBars
    }

    public class CalibrationWindow : Window {
        public CalibrationWindow(CalibrationPatternType pattern, SWF.Screen screen) {
            WindowStyle = WindowStyle.None;
            Topmost = true;
            ShowInTaskbar = false;
            Left = screen != null ? screen.Bounds.X : 0;
            Top = screen != null ? screen.Bounds.Y : 0;
            Width = screen != null ? screen.Bounds.Width : SystemParameters.PrimaryScreenWidth;
            Height = screen != null ? screen.Bounds.Height : SystemParameters.PrimaryScreenHeight;
            Cursor = System.Windows.Input.Cursors.Hand;

            Grid root = new Grid();

            if (pattern == CalibrationPatternType.White) {
                root.Background = Brushes.White;
            } else if (pattern == CalibrationPatternType.Black) {
                root.Background = Brushes.Black;
            } else if (pattern == CalibrationPatternType.Grayscale) {
                root.Background = new SolidColorBrush(Color.FromRgb(20, 20, 20));

                Grid grayGrid = new Grid();
                grayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grayGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) });

                // 16 discrete steps
                UniformGrid stepsGrid = new UniformGrid();
                stepsGrid.Rows = 1;
                stepsGrid.Columns = 16;
                for (int i = 0; i < 16; i++) {
                    byte val = (byte)Math.Round((i / 15.0) * 255.0);
                    Border b = new Border();
                    b.Background = new SolidColorBrush(Color.FromRgb(val, val, val));

                    TextBlock tb = new TextBlock();
                    tb.Text = string.Format("{0}%", (int)Math.Round((i / 15.0) * 100));
                    tb.Foreground = val > 128 ? Brushes.Black : Brushes.White;
                    tb.FontSize = 14;
                    tb.FontWeight = FontWeights.Bold;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    tb.Margin = new Thickness(0, 0, 0, 20);
                    b.Child = tb;

                    stepsGrid.Children.Add(b);
                }
                Grid.SetRow(stepsGrid, 0);
                grayGrid.Children.Add(stepsGrid);

                // Continuous gradient
                Border contGrad = new Border();
                LinearGradientBrush lgb = new LinearGradientBrush();
                lgb.StartPoint = new Point(0, 0.5);
                lgb.EndPoint = new Point(1, 0.5);
                lgb.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                lgb.GradientStops.Add(new GradientStop(Colors.White, 1.0));
                contGrad.Background = lgb;
                Grid.SetRow(contGrad, 1);
                grayGrid.Children.Add(contGrad);

                root.Children.Add(grayGrid);
            } else if (pattern == CalibrationPatternType.ColorBars) {
                // SMPTE 8 color bars
                UniformGrid barGrid = new UniformGrid();
                barGrid.Rows = 1;
                barGrid.Columns = 8;
                Color[] colors = new Color[] {
                    Color.FromRgb(255, 255, 255), // White 100%
                    Color.FromRgb(255, 255, 0),   // Yellow
                    Color.FromRgb(0, 255, 255),   // Cyan
                    Color.FromRgb(0, 255, 0),     // Green
                    Color.FromRgb(255, 0, 255),   // Magenta
                    Color.FromRgb(255, 0, 0),     // Red
                    Color.FromRgb(0, 0, 255),     // Blue
                    Color.FromRgb(0, 0, 0)        // Black
                };
                string[] names = new string[] { "White", "Yellow", "Cyan", "Green", "Magenta", "Red", "Blue", "Black" };
                for (int i = 0; i < 8; i++) {
                    Border b = new Border();
                    b.Background = new SolidColorBrush(colors[i]);
                    TextBlock tb = new TextBlock();
                    tb.Text = names[i];
                    tb.Foreground = (i == 0 || i == 1 || i == 2 || i == 3) ? Brushes.Black : Brushes.White;
                    tb.FontSize = 16;
                    tb.FontWeight = FontWeights.Bold;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    tb.Margin = new Thickness(0, 0, 0, 40);
                    b.Child = tb;
                    barGrid.Children.Add(b);
                }
                root.Children.Add(barGrid);
            }

            // Floating dismiss instruction banner at top
            Border banner = new Border();
            banner.HorizontalAlignment = HorizontalAlignment.Center;
            banner.VerticalAlignment = VerticalAlignment.Top;
            banner.Margin = new Thickness(0, 30, 0, 0);
            banner.Background = new SolidColorBrush(Color.FromArgb(200, 15, 23, 42));
            banner.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            banner.BorderThickness = new Thickness(1.5);
            banner.CornerRadius = new CornerRadius(20);
            banner.Padding = new Thickness(24, 10, 24, 10);

            TextBlock bannerText = new TextBlock();
            bannerText.Text = I18n.T("TestPatternNotice");
            bannerText.Foreground = Brushes.White;
            bannerText.FontSize = 13;
            bannerText.FontWeight = FontWeights.SemiBold;
            banner.Child = bannerText;
            root.Children.Add(banner);

            Content = root;

            KeyDown += delegate(object sender, System.Windows.Input.KeyEventArgs e) {
                if (e.Key == System.Windows.Input.Key.Escape) {
                    Close();
                }
            };
            MouseDown += delegate {
                Close();
            };
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

        public void ShowIdentifyBadges(List<ScreenDevice> devices) {
            try {
                foreach (ScreenDevice dev in devices) {
                    foreach (SWF.Screen scr in SWF.Screen.AllScreens) {
                        if (scr.DeviceName == dev.DeviceName) {
                            IdentifyOverlay idOv = new IdentifyOverlay(dev.Number, dev.ResolutionText, scr);
                            idOv.Show();
                            break;
                        }
                    }
                }
            } catch { }
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
