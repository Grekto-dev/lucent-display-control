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
    /// <summary>Click-through, per-monitor dimming / tint layer (only used when monitors are controlled independently).</summary>
    public class ScreenOverlay : Window {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_LAYERED = 0x00080000;

        private static readonly Brush Black = Freeze(new SolidColorBrush(Colors.Black));
        private static readonly Brush White = Freeze(new SolidColorBrush(Colors.White));
        private static readonly Brush Warm = Freeze(new SolidColorBrush(Color.FromRgb(255, 135, 15)));
        private static readonly Brush Cool = Freeze(new SolidColorBrush(Color.FromRgb(60, 150, 255)));

        private readonly Rectangle dimRect;
        private readonly Rectangle tintRect;
        private readonly System.Drawing.Rectangle pixelBounds;

        private static Brush Freeze(SolidColorBrush b) {
            b.Freeze();
            return b;
        }

        /// <summary>True when the settings need a visible overlay at all.</summary>
        public static bool NeedsOverlay(DisplaySettings s) {
            return Math.Abs(s.Brightness - 1.0) > 0.005 || s.Temperature < 6400.0 || s.Temperature > 6600.0;
        }

        public ScreenOverlay(System.Drawing.Rectangle bounds) {
            pixelBounds = bounds;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            Focusable = false;
            IsHitTestVisible = false;
            Background = Brushes.Transparent;
            Left = bounds.X;
            Top = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;

            Grid grid = new Grid();
            dimRect = new Rectangle();
            dimRect.Fill = Black;
            dimRect.Opacity = 0.0;
            grid.Children.Add(dimRect);

            tintRect = new Rectangle();
            tintRect.Fill = Warm;
            tintRect.Opacity = 0.0;
            grid.Children.Add(tintRect);
            Content = grid;

            SourceInitialized += delegate {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                int ex = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, ex | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED);
                // Position in physical pixels: WPF's Left/Width are DIPs and drift on scaled displays.
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, pixelBounds.X, pixelBounds.Y, pixelBounds.Width, pixelBounds.Height, NativeMethods.SWP_NOACTIVATE);
            };
        }

        public void ApplySettings(DisplaySettings s) {
            if (s.Brightness < 1.0) {
                dimRect.Fill = Black;
                dimRect.Opacity = Math.Min(0.92, (1.0 - s.Brightness) * 0.95);
            } else if (s.Brightness > 1.0) {
                dimRect.Fill = White;
                dimRect.Opacity = Math.Min(0.35, (s.Brightness - 1.0) * 0.40);
            } else {
                dimRect.Opacity = 0.0;
            }

            if (s.Temperature < 6400.0) {
                tintRect.Fill = Warm;
                tintRect.Opacity = Math.Min(0.55, (6500.0 - s.Temperature) / 4000.0 * 0.45);
            } else if (s.Temperature > 6600.0) {
                tintRect.Fill = Cool;
                tintRect.Opacity = Math.Min(0.35, (s.Temperature - 6500.0) / 3000.0 * 0.25);
            } else {
                tintRect.Opacity = 0.0;
            }
        }
    }

    public class IdentifyOverlay : Window {
        private readonly System.Drawing.Rectangle bounds;

        public IdentifyOverlay(int monNumber, string resolution, SWF.Screen screen) {
            bounds = screen.Bounds;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            Focusable = false;
            IsHitTestVisible = false;
            Background = Brushes.Transparent;
            Width = 240;
            Height = 240;

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

            SourceInitialized += delegate {
                PresentationSource src = PresentationSource.FromVisual(this);
                double scale = src != null && src.CompositionTarget != null ? src.CompositionTarget.TransformToDevice.M11 : 1.0;
                int px = (int)Math.Round(240 * scale);
                NativeMethods.SetWindowPos(new WindowInteropHelper(this).Handle, NativeMethods.HWND_TOPMOST,
                    bounds.X + (bounds.Width - px) / 2, bounds.Y + (bounds.Height - px) / 2, px, px, NativeMethods.SWP_NOACTIVATE);
            };

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
        private readonly System.Drawing.Rectangle bounds;

        public CalibrationWindow(CalibrationPatternType pattern, SWF.Screen screen) {
            bounds = screen != null ? screen.Bounds : SWF.Screen.PrimaryScreen.Bounds;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = false;
            Icon = AppIcon.Wpf();
            Left = bounds.X;
            Top = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
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
            } else {
                UniformGrid barGrid = new UniformGrid();
                barGrid.Rows = 1;
                barGrid.Columns = 8;
                Color[] colors = new Color[] {
                    Color.FromRgb(255, 255, 255), Color.FromRgb(255, 255, 0), Color.FromRgb(0, 255, 255), Color.FromRgb(0, 255, 0),
                    Color.FromRgb(255, 0, 255), Color.FromRgb(255, 0, 0), Color.FromRgb(0, 0, 255), Color.FromRgb(0, 0, 0)
                };
                string[] names = new string[] { "White", "Yellow", "Cyan", "Green", "Magenta", "Red", "Blue", "Black" };
                for (int i = 0; i < 8; i++) {
                    Border b = new Border();
                    b.Background = new SolidColorBrush(colors[i]);
                    TextBlock tb = new TextBlock();
                    tb.Text = names[i];
                    tb.Foreground = i < 4 ? Brushes.Black : Brushes.White;
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

            SourceInitialized += delegate {
                NativeMethods.SetWindowPos(new WindowInteropHelper(this).Handle, NativeMethods.HWND_TOPMOST,
                    bounds.X, bounds.Y, bounds.Width, bounds.Height, 0);
            };
            Loaded += delegate { Activate(); Focus(); };
            KeyDown += delegate(object sender, System.Windows.Input.KeyEventArgs e) {
                if (e.Key == System.Windows.Input.Key.Escape) Close();
            };
            MouseDown += delegate { Close(); };
        }
    }

    /// <summary>
    /// Owns the per-monitor overlays. Windows are created only while a monitor actually needs one
    /// and are destroyed as soon as it returns to neutral, so the idle cost is zero.
    /// </summary>
    public class OverlayManager {
        private readonly Dictionary<string, ScreenOverlay> overlays = new Dictionary<string, ScreenOverlay>();

        public int ActiveCount { get { return overlays.Count; } }

        public void Update(SWF.Screen screen, DisplaySettings s) {
            ScreenOverlay ov;
            bool has = overlays.TryGetValue(screen.DeviceName, out ov);
            if (!ScreenOverlay.NeedsOverlay(s)) {
                if (has) {
                    ov.Close();
                    overlays.Remove(screen.DeviceName);
                }
                return;
            }
            if (!has) {
                ov = new ScreenOverlay(screen.Bounds);
                overlays[screen.DeviceName] = ov;
                ov.Show();
            }
            ov.ApplySettings(s);
        }

        public void ShowPattern(CalibrationPatternType pattern) {
            foreach (SWF.Screen scr in SWF.Screen.AllScreens) {
                new CalibrationWindow(pattern, scr).Show();
            }
        }

        public void Apply(List<ScreenDevice> devices, Dictionary<string, DisplaySettings> settings) {
            SWF.Screen[] screens = SWF.Screen.AllScreens;
            foreach (SWF.Screen scr in screens) {
                DisplaySettings s;
                if (settings.TryGetValue(scr.DeviceName, out s)) {
                    Update(scr, s);
                }
            }
        }

        public void ShowIdentifyBadges(List<ScreenDevice> devices) {
            SWF.Screen[] screens = SWF.Screen.AllScreens;
            foreach (ScreenDevice dev in devices) {
                foreach (SWF.Screen scr in screens) {
                    if (scr.DeviceName == dev.DeviceName) {
                        new IdentifyOverlay(dev.Number, dev.ResolutionText, scr).Show();
                        break;
                    }
                }
            }
        }

        public void CloseAll() {
            foreach (KeyValuePair<string, ScreenOverlay> kvp in overlays) {
                try { kvp.Value.Close(); } catch { }
            }
            overlays.Clear();
        }
    }
}
