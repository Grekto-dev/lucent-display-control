using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Shell;
using SD = System.Drawing;
using G2D = System.Drawing.Drawing2D;
using SWF = System.Windows.Forms;

namespace LucentControl {
    public class MainWindow : Window {
        private AppConfig config;
        private List<ScreenDevice> screenDevices;
        private Dictionary<string, DisplaySettings> settingsPerMonitor;
        private DisplaySettings globalSettings;
        private bool syncAllMonitors = false;
        private int selectedMonitorIndex = 0; // Default to Monitor 1 (index 0)

        private OverlayManager overlayManager;
        private SWF.NotifyIcon trayIcon;

        // Custom Slider Styles (Parsed from XAML)
        private Style sliderStyleBlue;
        private Style sliderStyleTemp;
        private Style sliderStyleRed;
        private Style sliderStyleGreen;
        private Style sliderStyleBlueChannel;

        // Sidebar Navigation Buttons
        private Button btnNavDashboard;
        private Button btnNavMonitors;
        private Button btnNavPresets;
        private Button btnNavProfiles;
        private Button btnNavScheduler;
        private Button btnNavSettings;

        // Main Views
        private Grid viewDashboard;
        private Grid viewSettings;

        // Visual Monitors Cards
        private StackPanel monitorCardsRow;
        private List<Border> monitorCardBorders;

        // Controls Sliders & Badges
        private Slider sliderBrightness;
        private Slider sliderContrast;
        private Slider sliderGamma;
        private Slider sliderColorTemp;
        private Slider sliderRed;
        private Slider sliderGreen;
        private Slider sliderBlue;

        private TextBlock badgeBrightness;
        private TextBlock badgeContrast;
        private TextBlock badgeGamma;
        private TextBlock badgeColorTemp;
        private TextBlock badgeRed;
        private TextBlock badgeGreen;
        private TextBlock badgeBlue;

        private TextBlock gammaTick18;
        private TextBlock gammaTick20;
        private Border gammaTick22Badge;
        private TextBlock gammaTick22Text;
        private TextBlock gammaTick24;
        private TextBlock gammaTick26;

        private Button btnPresetWarm;
        private Button btnPresetNeutral;
        private Button btnPresetCool;
        private Button btnPresetMovie;
        private Button btnPresetWork;
        private Button btnPresetNight;
        private List<Button> presetButtons;

        private ComboBox comboActiveProfile;
        private Button btnResetDefault;
        private Button btnApplyToMonitor;

        // Settings View Elements
        private TextBlock txtSettingsTitle;
        private TextBlock txtSettingsSubtitle;
        private TextBlock txtLangTitle;
        private TextBlock txtLangDesc;
        private ComboBox comboLanguage;

        private TextBlock txtStartWinTitle;
        private TextBlock txtStartWinDesc;
        private CheckBox chkStartWithWindows;

        private Border subCardStartMinimized;
        private TextBlock txtStartMinTitle;
        private TextBlock txtStartMinDesc;
        private CheckBox chkStartMinimized;

        private TextBlock txtMinCloseTitle;
        private TextBlock txtMinCloseDesc;
        private CheckBox chkMinimizeOnClose;

        private TextBlock txtAboutTitle;
        private TextBlock txtAboutVersion;
        private TextBlock txtAboutDesc;

        private Button btnSettingsApply;
        private Button btnSettingsOK;
        private Button btnSettingsCancel;
        private TextBlock lblSettingsSaveNotice;

        // Dynamic Texts for Localization
        private TextBlock headerTitle;
        private TextBlock headerStatus;
        private TextBlock sidebarMonitorsCount;
        private TextBlock mainTitle;
        private TextBlock mainSubtitle;
        private Button btnIdentify;
        private Button btnMinimizeToTray;

        private TextBlock lblBrightnessTitle;
        private TextBlock lblContrastTitle;
        private TextBlock lblGammaTitle;
        private TextBlock lblColorTempTitle;
        private TextBlock lblRedTitle;
        private TextBlock lblGreenTitle;
        private TextBlock lblBlueTitle;
        private TextBlock lblQuickPresetsTitle;
        private TextBlock lblQuickPresetsSubtitle;
        private TextBlock lblActiveProfileTitle;

        private bool isUpdatingUi = false;

        public MainWindow(bool startMinimized) {
            config = AppConfig.Load();
            I18n.CurrentLanguage = config.Language;
            I18n.CommittedLanguage = config.Language;

            screenDevices = new List<ScreenDevice>();
            settingsPerMonitor = new Dictionary<string, DisplaySettings>();
            globalSettings = new DisplaySettings();

            globalSettings.Brightness = config.Brightness;
            globalSettings.Contrast = config.Contrast;
            globalSettings.Gamma = config.Gamma;
            globalSettings.Temperature = config.Temperature;
            globalSettings.Red = config.Red;
            globalSettings.Green = config.Green;
            globalSettings.Blue = config.Blue;
            globalSettings.ProfileName = config.ActiveProfile;
            syncAllMonitors = config.SyncMonitors;

            DetectMonitors();
            overlayManager = new OverlayManager();

            LoadCustomStyles();
            InitializeComponent();
            SetupTrayIcon();

            Loaded += delegate {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.EnableDarkMode(hwnd);
                ApplyAllActiveSettings();

                if (startMinimized) {
                    Hide();
                }
            };
        }

        private void DetectMonitors() {
            screenDevices.Clear();
            int monNum = 1;

            foreach (SWF.Screen scr in SWF.Screen.AllScreens) {
                ScreenDevice dev = new ScreenDevice();
                dev.Number = monNum++;
                dev.DeviceName = scr.DeviceName;
                dev.Bounds = scr.Bounds;
                dev.IsPrimary = scr.Primary;
                dev.FriendlyName = scr.Primary ? "DisplayLink USB-DVI (Principal)" : "DisplayLink USB-DVI (Secundário)";
                dev.RefreshRate = 60;

                // Query EnumDisplayDevices for specific display model name
                uint i = 0;
                while (true) {
                    NativeMethods.DISPLAY_DEVICE d = new NativeMethods.DISPLAY_DEVICE();
                    d.cb = System.Runtime.InteropServices.Marshal.SizeOf(d);
                    if (!NativeMethods.EnumDisplayDevices(null, i, ref d, 0)) break;
                    if (d.DeviceName == scr.DeviceName && !string.IsNullOrEmpty(d.DeviceString)) {
                        dev.FriendlyName = d.DeviceString;
                        break;
                    }
                    i++;
                }

                // Backup hardware ramp
                IntPtr hdc = NativeMethods.CreateDC(null, dev.DeviceName, null, IntPtr.Zero);
                if (hdc != IntPtr.Zero) {
                    byte[] orig = new byte[1536];
                    if (NativeMethods.GetDeviceGammaRamp(hdc, orig)) {
                        dev.OriginalRamp = orig;
                    }
                    NativeMethods.DeleteDC(hdc);
                }

                screenDevices.Add(dev);
                if (!settingsPerMonitor.ContainsKey(dev.DeviceName)) {
                    DisplaySettings monS = globalSettings.Clone();
                    settingsPerMonitor[dev.DeviceName] = monS;
                }
            }
        }

        private void LoadCustomStyles() {
            try {
                string xaml = @"
                <ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                    <!-- Modern Blue Slider -->
                    <ControlTemplate x:Key='BlueSliderTemplate' TargetType='Slider'>
                        <Grid VerticalAlignment='Center' Height='28'>
                            <Border Height='6' CornerRadius='3' Background='#FF222634'/>
                            <Track x:Name='PART_Track'>
                                <Track.Thumb>
                                    <Thumb Width='20' Height='20' Cursor='Hand'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='Thumb'>
                                                <Grid>
                                                    <Ellipse Fill='#FFFFFFFF'/>
                                                    <Ellipse Stroke='#FF38BDF8' StrokeThickness='2'/>
                                                </Grid>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                    </ControlTemplate>

                    <!-- Gradient Color Temp Slider -->
                    <ControlTemplate x:Key='TempSliderTemplate' TargetType='Slider'>
                        <Grid VerticalAlignment='Center' Height='28'>
                            <Border Height='6' CornerRadius='3'>
                                <Border.Background>
                                    <LinearGradientBrush StartPoint='0,0' EndPoint='1,0'>
                                        <GradientStop Color='#FFFB923C' Offset='0.0'/>
                                        <GradientStop Color='#FFF8FAFC' Offset='0.5'/>
                                        <GradientStop Color='#FF38BDF8' Offset='1.0'/>
                                    </LinearGradientBrush>
                                </Border.Background>
                            </Border>
                            <Track x:Name='PART_Track'>
                                <Track.Thumb>
                                    <Thumb Width='20' Height='20' Cursor='Hand'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='Thumb'>
                                                <Grid>
                                                    <Ellipse Fill='#FFFFFFFF'/>
                                                    <Ellipse Stroke='#FF38BDF8' StrokeThickness='2'/>
                                                </Grid>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                    </ControlTemplate>

                    <!-- Red Channel Slider -->
                    <ControlTemplate x:Key='RedSliderTemplate' TargetType='Slider'>
                        <Grid VerticalAlignment='Center' Height='28'>
                            <Border Height='6' CornerRadius='3' Background='#FFEF4444'/>
                            <Track x:Name='PART_Track'>
                                <Track.Thumb>
                                    <Thumb Width='20' Height='20' Cursor='Hand'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='Thumb'>
                                                <Grid>
                                                    <Ellipse Fill='#FFFFFFFF'/>
                                                    <Ellipse Stroke='#FFEF4444' StrokeThickness='2'/>
                                                </Grid>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                    </ControlTemplate>

                    <!-- Green Channel Slider -->
                    <ControlTemplate x:Key='GreenSliderTemplate' TargetType='Slider'>
                        <Grid VerticalAlignment='Center' Height='28'>
                            <Border Height='6' CornerRadius='3' Background='#FF22C55E'/>
                            <Track x:Name='PART_Track'>
                                <Track.Thumb>
                                    <Thumb Width='20' Height='20' Cursor='Hand'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='Thumb'>
                                                <Grid>
                                                    <Ellipse Fill='#FFFFFFFF'/>
                                                    <Ellipse Stroke='#FF22C55E' StrokeThickness='2'/>
                                                </Grid>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                    </ControlTemplate>

                    <!-- Blue Channel Slider -->
                    <ControlTemplate x:Key='BlueChannelSliderTemplate' TargetType='Slider'>
                        <Grid VerticalAlignment='Center' Height='28'>
                            <Border Height='6' CornerRadius='3' Background='#FF3B82F6'/>
                            <Track x:Name='PART_Track'>
                                <Track.Thumb>
                                    <Thumb Width='20' Height='20' Cursor='Hand'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='Thumb'>
                                                <Grid>
                                                    <Ellipse Fill='#FFFFFFFF'/>
                                                    <Ellipse Stroke='#FF3B82F6' StrokeThickness='2'/>
                                                </Grid>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                    </ControlTemplate>
                </ResourceDictionary>";

                ResourceDictionary dict = (ResourceDictionary)XamlReader.Parse(xaml);
                sliderStyleBlue = new Style(typeof(Slider));
                sliderStyleBlue.Setters.Add(new Setter(Slider.TemplateProperty, dict["BlueSliderTemplate"]));

                sliderStyleTemp = new Style(typeof(Slider));
                sliderStyleTemp.Setters.Add(new Setter(Slider.TemplateProperty, dict["TempSliderTemplate"]));

                sliderStyleRed = new Style(typeof(Slider));
                sliderStyleRed.Setters.Add(new Setter(Slider.TemplateProperty, dict["RedSliderTemplate"]));

                sliderStyleGreen = new Style(typeof(Slider));
                sliderStyleGreen.Setters.Add(new Setter(Slider.TemplateProperty, dict["GreenSliderTemplate"]));

                sliderStyleBlueChannel = new Style(typeof(Slider));
                sliderStyleBlueChannel.Setters.Add(new Setter(Slider.TemplateProperty, dict["BlueChannelSliderTemplate"]));
            } catch { }
        }

        private void InitializeComponent() {
            Title = "Lucent Display Control";
            Width = 1120;
            Height = 720;
            MinWidth = 980;
            MinHeight = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(13, 16, 23));
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI, Segoe UI Variable Display");

            WindowChrome chrome = new WindowChrome();
            chrome.CaptionHeight = 48;
            chrome.CornerRadius = new CornerRadius(10);
            chrome.GlassFrameThickness = new Thickness(0);
            chrome.UseAeroCaptionButtons = false;
            WindowChrome.SetWindowChrome(this, chrome);

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(48) }); // Title Bar
            root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Main Body

            // 1. Custom Sleek Title Bar
            Grid titleBar = CreateTitleBar();
            Grid.SetRow(titleBar, 0);
            root.Children.Add(titleBar);

            // 2. Main Body: Left Sidebar + Right Content Area
            Grid mainBody = new Grid();
            mainBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(190) }); // Left Sidebar
            mainBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Content Area

            Border sidebar = CreateSidebar();
            Grid.SetColumn(sidebar, 0);
            mainBody.Children.Add(sidebar);

            Grid contentArea = new Grid();
            viewDashboard = CreateDashboardView();
            viewSettings = CreateSettingsView();
            viewSettings.Visibility = Visibility.Collapsed;

            contentArea.Children.Add(viewDashboard);
            contentArea.Children.Add(viewSettings);

            Grid.SetColumn(contentArea, 1);
            mainBody.Children.Add(contentArea);

            Grid.SetRow(mainBody, 1);
            root.Children.Add(mainBody);

            Content = root;
            Closing += MainWindow_Closing;

            UpdateAllTexts();
            UpdateVisualMonitorCards();
            UpdateUiFromSettings();
        }

        #region Title Bar & Sidebar
        private Grid CreateTitleBar() {
            Grid tb = new Grid();
            tb.Background = new SolidColorBrush(Color.FromRgb(13, 16, 23));
            tb.MouseDown += delegate(object s, MouseButtonEventArgs e) {
                if (e.ChangedButton == MouseButton.Left) DragMove();
            };

            tb.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            tb.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            tb.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Left: Logo + App Name
            StackPanel leftSp = new StackPanel();
            leftSp.Orientation = Orientation.Horizontal;
            leftSp.Margin = new Thickness(16, 0, 0, 0);
            leftSp.VerticalAlignment = VerticalAlignment.Center;

            // Sleek Diamond / Polygon Gradient Logo
            Border logoBorder = new Border();
            logoBorder.Width = 26;
            logoBorder.Height = 26;
            logoBorder.CornerRadius = new CornerRadius(6);
            logoBorder.Background = new LinearGradientBrush(
                Color.FromRgb(56, 189, 248),
                Color.FromRgb(99, 102, 241),
                new Point(0, 0),
                new Point(1, 1)
            );
            logoBorder.Margin = new Thickness(0, 0, 10, 0);

            TextBlock logoIcon = new TextBlock();
            logoIcon.Text = "❖";
            logoIcon.FontSize = 16;
            logoIcon.Foreground = Brushes.White;
            logoIcon.HorizontalAlignment = HorizontalAlignment.Center;
            logoIcon.VerticalAlignment = VerticalAlignment.Center;
            logoBorder.Child = logoIcon;
            leftSp.Children.Add(logoBorder);

            headerTitle = new TextBlock();
            headerTitle.Text = "Lucent Display Control";
            headerTitle.FontSize = 14;
            headerTitle.FontWeight = FontWeights.SemiBold;
            headerTitle.Foreground = Brushes.White;
            headerTitle.VerticalAlignment = VerticalAlignment.Center;
            leftSp.Children.Add(headerTitle);

            Grid.SetColumn(leftSp, 0);
            tb.Children.Add(leftSp);

            // Right: Status indicator + Window buttons
            StackPanel rightSp = new StackPanel();
            rightSp.Orientation = Orientation.Horizontal;
            rightSp.VerticalAlignment = VerticalAlignment.Center;
            rightSp.Margin = new Thickness(0, 0, 8, 0);

            // Green status dot
            Ellipse statusDot = new Ellipse();
            statusDot.Width = 8;
            statusDot.Height = 8;
            statusDot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            statusDot.Margin = new Thickness(0, 0, 6, 0);
            rightSp.Children.Add(statusDot);

            headerStatus = new TextBlock();
            headerStatus.Text = I18n.T("RunningInTray");
            headerStatus.FontSize = 12;
            headerStatus.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            headerStatus.Margin = new Thickness(0, 0, 20, 0);
            rightSp.Children.Add(headerStatus);

            // Bell icon
            TextBlock bell = new TextBlock();
            bell.Text = "🔔";
            bell.FontSize = 14;
            bell.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            bell.Margin = new Thickness(0, 0, 16, 0);
            bell.Cursor = Cursors.Hand;
            WindowChrome.SetIsHitTestVisibleInChrome(bell, true);
            rightSp.Children.Add(bell);

            // Window control buttons
            rightSp.Children.Add(CreateWindowBtn("─", delegate { WindowState = WindowState.Minimized; }));
            rightSp.Children.Add(CreateWindowBtn("□", delegate {
                WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            }));
            rightSp.Children.Add(CreateWindowBtn("✕", delegate { Close(); }));

            Grid.SetColumn(rightSp, 2);
            tb.Children.Add(rightSp);

            return tb;
        }

        private Button CreateWindowBtn(string glyph, RoutedEventHandler click) {
            Button b = new Button();
            WindowChrome.SetIsHitTestVisibleInChrome(b, true);
            b.Content = glyph;
            b.Width = 32;
            b.Height = 28;
            b.Background = Brushes.Transparent;
            b.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
            b.BorderThickness = new Thickness(0);
            b.FontSize = 12;
            b.Cursor = Cursors.Hand;
            b.Click += click;
            return b;
        }

        private Border CreateSidebar() {
            Border b = new Border();
            b.Background = new SolidColorBrush(Color.FromRgb(15, 18, 27));
            b.BorderBrush = new SolidColorBrush(Color.FromRgb(26, 31, 46));
            b.BorderThickness = new Thickness(0, 0, 1, 0);
            b.Padding = new Thickness(12, 14, 12, 16);

            Grid sbGrid = new Grid();
            sbGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Nav items
            sbGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Footer info

            StackPanel navList = new StackPanel();

            btnNavDashboard = CreateSidebarBtn("🏠", I18n.T("NavDashboard"), true, delegate { SwitchToDashboard(); });
            btnNavMonitors = CreateSidebarBtn("🖥", I18n.T("NavMonitors"), false, delegate { SwitchToDashboard(); });
            btnNavPresets = CreateSidebarBtn("⊞", I18n.T("NavPresets"), false, delegate { SwitchToDashboard(); });
            btnNavProfiles = CreateSidebarBtn("👤", I18n.T("NavProfiles"), false, delegate { SwitchToDashboard(); });
            btnNavScheduler = CreateSidebarBtn("🕒", I18n.T("NavScheduler"), false, delegate { SwitchToDashboard(); });
            btnNavSettings = CreateSidebarBtn("⚙", I18n.T("NavSettings"), false, delegate { SwitchToSettings(); });

            navList.Children.Add(btnNavDashboard);
            navList.Children.Add(btnNavMonitors);
            navList.Children.Add(btnNavPresets);
            navList.Children.Add(btnNavProfiles);
            navList.Children.Add(btnNavScheduler);
            navList.Children.Add(btnNavSettings);

            Grid.SetRow(navList, 0);
            sbGrid.Children.Add(navList);

            // Bottom Info: Green dot + monitor count + Version
            StackPanel foot = new StackPanel();

            StackPanel statusRow = new StackPanel();
            statusRow.Orientation = Orientation.Horizontal;
            statusRow.Margin = new Thickness(4, 0, 0, 4);

            Ellipse dot = new Ellipse();
            dot.Width = 6;
            dot.Height = 6;
            dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            dot.Margin = new Thickness(0, 0, 6, 0);
            statusRow.Children.Add(dot);

            sidebarMonitorsCount = new TextBlock();
            sidebarMonitorsCount.Text = I18n.T("MonitorsConnected", screenDevices.Count);
            sidebarMonitorsCount.FontSize = 11;
            sidebarMonitorsCount.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            statusRow.Children.Add(sidebarMonitorsCount);
            foot.Children.Add(statusRow);

            TextBlock verTb = new TextBlock();
            verTb.Text = "Lucent Display Control v0.1.0";
            verTb.FontSize = 10;
            verTb.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            verTb.Margin = new Thickness(4, 0, 0, 0);
            foot.Children.Add(verTb);

            Grid.SetRow(foot, 1);
            sbGrid.Children.Add(foot);

            b.Child = sbGrid;
            return b;
        }

        private Button CreateSidebarBtn(string icon, string label, bool isSelected, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 38;
            b.Margin = new Thickness(0, 0, 0, 4);
            b.BorderThickness = new Thickness(0);
            b.Cursor = Cursors.Hand;
            b.HorizontalContentAlignment = HorizontalAlignment.Left;
            b.Padding = new Thickness(10, 0, 10, 0);

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock iTb = new TextBlock();
            iTb.Text = icon;
            iTb.FontSize = 14;
            iTb.Width = 22;
            sp.Children.Add(iTb);

            TextBlock lTb = new TextBlock();
            lTb.Text = label;
            lTb.FontSize = 13;
            lTb.FontWeight = FontWeights.Medium;
            lTb.Margin = new Thickness(6, 0, 0, 0);
            sp.Children.Add(lTb);

            b.Content = sp;
            b.Click += click;
            ApplySidebarBtnStyle(b, isSelected);
            return b;
        }

        private void ApplySidebarBtnStyle(Button b, bool isSelected) {
            if (isSelected) {
                b.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
                b.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            } else {
                b.Background = Brushes.Transparent;
                b.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            }
        }

        private void SwitchToDashboard() {
            viewDashboard.Visibility = Visibility.Visible;
            viewSettings.Visibility = Visibility.Collapsed;
            ApplySidebarBtnStyle(btnNavDashboard, true);
            ApplySidebarBtnStyle(btnNavSettings, false);
        }

        private void SwitchToSettings() {
            viewDashboard.Visibility = Visibility.Collapsed;
            viewSettings.Visibility = Visibility.Visible;
            ApplySidebarBtnStyle(btnNavDashboard, false);
            ApplySidebarBtnStyle(btnNavSettings, true);
            lblSettingsSaveNotice.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Main Dashboard View (Exact Match to Screenshot)
        private Grid CreateDashboardView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 16, 24, 24);

            // 1. Dashboard Top Header Row
            Grid headGrid = new Grid();
            headGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            headGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            mainTitle = new TextBlock();
            mainTitle.Text = I18n.T("MonitorsTitle");
            mainTitle.FontSize = 20;
            mainTitle.FontWeight = FontWeights.Bold;
            mainTitle.Foreground = Brushes.White;
            titleSp.Children.Add(mainTitle);

            mainSubtitle = new TextBlock();
            mainSubtitle.Text = I18n.T("MonitorsSubtitle");
            mainSubtitle.FontSize = 12;
            mainSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            mainSubtitle.Margin = new Thickness(0, 2, 0, 0);
            titleSp.Children.Add(mainSubtitle);
            Grid.SetColumn(titleSp, 0);
            headGrid.Children.Add(titleSp);

            // Action buttons: Identify and Minimize to tray
            StackPanel actSp = new StackPanel();
            actSp.Orientation = Orientation.Horizontal;

            btnIdentify = CreateHeaderActionBtn("ⓘ", I18n.T("Identify"), delegate {
                overlayManager.ShowIdentifyBadges(screenDevices);
            });
            btnMinimizeToTray = CreateHeaderActionBtn("🖥", I18n.T("MinimizeToTray"), delegate {
                Hide();
            });

            actSp.Children.Add(btnIdentify);
            actSp.Children.Add(btnMinimizeToTray);
            Grid.SetColumn(actSp, 1);
            headGrid.Children.Add(actSp);
            sp.Children.Add(headGrid);

            // 2. Monitors Selection Cards Row
            ScrollViewer monScroll = new ScrollViewer();
            monScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            monScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            monScroll.Margin = new Thickness(0, 16, 0, 14);

            monitorCardsRow = new StackPanel();
            monitorCardsRow.Orientation = Orientation.Horizontal;
            monitorCardBorders = new List<Border>();

            for (int i = 0; i < screenDevices.Count; i++) {
                ScreenDevice sd = screenDevices[i];
                Border card = CreateMonitorCard(i, sd);
                monitorCardBorders.Add(card);
                monitorCardsRow.Children.Add(card);
            }

            // Also add All Displays Synced card
            Border syncCard = CreateAllMonitorsCard();
            monitorCardBorders.Add(syncCard);
            monitorCardsRow.Children.Add(syncCard);

            monScroll.Content = monitorCardsRow;
            sp.Children.Add(monScroll);

            // 3. Row 1: Brightness, Contrast, Gamma (3 equal columns)
            Grid row1 = new Grid();
            row1.Margin = new Thickness(0, 0, 0, 12);
            row1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            row1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            row1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            row1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            row1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Brightness Card
            Border brightCard = CreateDashboardCard();
            brightCard.Child = CreateBrightnessContent();
            Grid.SetColumn(brightCard, 0);
            row1.Children.Add(brightCard);

            // Contrast Card
            Border contrastCard = CreateDashboardCard();
            contrastCard.Child = CreateContrastContent();
            Grid.SetColumn(contrastCard, 2);
            row1.Children.Add(contrastCard);

            // Gamma Card
            Border gammaCard = CreateDashboardCard();
            gammaCard.Child = CreateGammaContent();
            Grid.SetColumn(gammaCard, 4);
            row1.Children.Add(gammaCard);

            sp.Children.Add(row1);

            // 4. Row 2: Color Temperature & RGB Channels (4 columns)
            Grid row2 = new Grid();
            row2.Margin = new Thickness(0, 0, 0, 12);
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.4, GridUnitType.Star) }); // Color Temp wider
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Red
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Green
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            row2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Blue

            // Color Temp Card
            Border tempCard = CreateDashboardCard();
            tempCard.Child = CreateColorTempContent();
            Grid.SetColumn(tempCard, 0);
            row2.Children.Add(tempCard);

            // Red Card
            Border redCard = CreateDashboardCard();
            redCard.Child = CreateChannelContent("●", "#EF4444", I18n.T("Red"), out lblRedTitle, out sliderRed, out badgeRed, sliderStyleRed);
            Grid.SetColumn(redCard, 2);
            row2.Children.Add(redCard);

            // Green Card
            Border greenCard = CreateDashboardCard();
            greenCard.Child = CreateChannelContent("●", "#22C55E", I18n.T("Green"), out lblGreenTitle, out sliderGreen, out badgeGreen, sliderStyleGreen);
            Grid.SetColumn(greenCard, 4);
            row2.Children.Add(greenCard);

            // Blue Card
            Border blueCard = CreateDashboardCard();
            blueCard.Child = CreateChannelContent("●", "#3B82F6", I18n.T("Blue"), out lblBlueTitle, out sliderBlue, out badgeBlue, sliderStyleBlueChannel);
            Grid.SetColumn(blueCard, 6);
            row2.Children.Add(blueCard);

            sp.Children.Add(row2);

            // 5. Row 3: Quick Presets Card
            Border presetsCard = CreateDashboardCard();
            presetsCard.Margin = new Thickness(0, 0, 0, 14);
            presetsCard.Child = CreateQuickPresetsContent();
            sp.Children.Add(presetsCard);

            // 6. Row 4: Bottom Action Bar (Active Profile, Reset, Apply)
            Grid footRow = CreateBottomActionBar();
            sp.Children.Add(footRow);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private Button CreateHeaderActionBtn(string icon, string label, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 34;
            b.Background = new SolidColorBrush(Color.FromRgb(26, 31, 46));
            b.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 48, 68));
            b.BorderThickness = new Thickness(1);
            b.Padding = new Thickness(12, 0, 12, 0);
            b.Margin = new Thickness(8, 0, 0, 0);
            b.Cursor = Cursors.Hand;

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock it = new TextBlock();
            it.Text = icon;
            it.FontSize = 13;
            it.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            it.Margin = new Thickness(0, 0, 6, 0);
            sp.Children.Add(it);

            TextBlock lt = new TextBlock();
            lt.Text = label;
            lt.FontSize = 12;
            lt.FontWeight = FontWeights.Medium;
            lt.Foreground = Brushes.White;
            sp.Children.Add(lt);

            b.Content = sp;
            b.Click += click;
            return b;
        }

        private Border CreateMonitorCard(int index, ScreenDevice sd) {
            Border b = new Border();
            b.Width = 240;
            b.Height = 110;
            b.CornerRadius = new CornerRadius(8);
            b.Margin = new Thickness(0, 0, 14, 0);
            b.Cursor = Cursors.Hand;
            b.Padding = new Thickness(14, 12, 14, 12);

            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            // Top: Badge circle (1) or (2) + Centered Monitor Icon
            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border badge = new Border();
            badge.Width = 22;
            badge.Height = 22;
            badge.CornerRadius = new CornerRadius(11);
            badge.Background = new SolidColorBrush(Color.FromRgb(40, 48, 68));

            TextBlock badgeText = new TextBlock();
            badgeText.Text = sd.Number.ToString();
            badgeText.FontSize = 11;
            badgeText.FontWeight = FontWeights.Bold;
            badgeText.Foreground = Brushes.White;
            badgeText.HorizontalAlignment = HorizontalAlignment.Center;
            badgeText.VerticalAlignment = VerticalAlignment.Center;
            badge.Child = badgeText;
            Grid.SetColumn(badge, 0);
            topRow.Children.Add(badge);

            TextBlock monIcon = new TextBlock();
            monIcon.Text = "🖥";
            monIcon.FontSize = 26;
            monIcon.HorizontalAlignment = HorizontalAlignment.Center;
            monIcon.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            Grid.SetColumn(monIcon, 1);
            topRow.Children.Add(monIcon);

            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            // Bottom: Green dot + Model name + Resolution
            StackPanel botSp = new StackPanel();
            botSp.VerticalAlignment = VerticalAlignment.Bottom;

            StackPanel nameSp = new StackPanel();
            nameSp.Orientation = Orientation.Horizontal;
            nameSp.HorizontalAlignment = HorizontalAlignment.Center;

            Ellipse dot = new Ellipse();
            dot.Width = 6;
            dot.Height = 6;
            dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            dot.Margin = new Thickness(0, 0, 6, 0);
            nameSp.Children.Add(dot);

            TextBlock nameTb = new TextBlock();
            nameTb.Text = sd.FriendlyName;
            nameTb.FontSize = 13;
            nameTb.FontWeight = FontWeights.SemiBold;
            nameTb.Foreground = Brushes.White;
            nameSp.Children.Add(nameTb);
            botSp.Children.Add(nameSp);

            TextBlock resTb = new TextBlock();
            resTb.Text = sd.ResolutionText;
            resTb.FontSize = 11;
            resTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            resTb.HorizontalAlignment = HorizontalAlignment.Center;
            resTb.Margin = new Thickness(0, 2, 0, 0);
            botSp.Children.Add(resTb);

            Grid.SetRow(botSp, 1);
            g.Children.Add(botSp);

            b.Child = g;

            int capturedIdx = index;
            b.MouseLeftButtonUp += delegate {
                selectedMonitorIndex = capturedIdx;
                syncAllMonitors = false;
                UpdateVisualMonitorCards();
                UpdateUiFromSettings();
            };

            return b;
        }

        private Border CreateAllMonitorsCard() {
            Border b = new Border();
            b.Width = 240;
            b.Height = 110;
            b.CornerRadius = new CornerRadius(8);
            b.Margin = new Thickness(0, 0, 14, 0);
            b.Cursor = Cursors.Hand;
            b.Padding = new Thickness(14, 12, 14, 12);

            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border badge = new Border();
            badge.Width = 22;
            badge.Height = 22;
            badge.CornerRadius = new CornerRadius(11);
            badge.Background = new SolidColorBrush(Color.FromRgb(40, 48, 68));

            TextBlock badgeText = new TextBlock();
            badgeText.Text = "⮂";
            badgeText.FontSize = 12;
            badgeText.FontWeight = FontWeights.Bold;
            badgeText.Foreground = Brushes.White;
            badgeText.HorizontalAlignment = HorizontalAlignment.Center;
            badgeText.VerticalAlignment = VerticalAlignment.Center;
            badge.Child = badgeText;
            Grid.SetColumn(badge, 0);
            topRow.Children.Add(badge);

            TextBlock monIcon = new TextBlock();
            monIcon.Text = "🖥🖥";
            monIcon.FontSize = 20;
            monIcon.HorizontalAlignment = HorizontalAlignment.Center;
            monIcon.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            Grid.SetColumn(monIcon, 1);
            topRow.Children.Add(monIcon);

            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            StackPanel botSp = new StackPanel();
            botSp.VerticalAlignment = VerticalAlignment.Bottom;

            TextBlock nameTb = new TextBlock();
            nameTb.Text = I18n.T("AllDisplays");
            nameTb.FontSize = 13;
            nameTb.FontWeight = FontWeights.SemiBold;
            nameTb.Foreground = Brushes.White;
            nameTb.HorizontalAlignment = HorizontalAlignment.Center;
            botSp.Children.Add(nameTb);

            TextBlock resTb = new TextBlock();
            resTb.Text = I18n.T("MonitorsConnected", screenDevices.Count);
            resTb.FontSize = 11;
            resTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            resTb.HorizontalAlignment = HorizontalAlignment.Center;
            resTb.Margin = new Thickness(0, 2, 0, 0);
            botSp.Children.Add(resTb);

            Grid.SetRow(botSp, 1);
            g.Children.Add(botSp);
            b.Child = g;

            b.MouseLeftButtonUp += delegate {
                syncAllMonitors = true;
                selectedMonitorIndex = -1;
                UpdateVisualMonitorCards();
                UpdateUiFromSettings();
            };

            return b;
        }

        private void UpdateVisualMonitorCards() {
            if (monitorCardBorders == null) return;
            for (int i = 0; i < monitorCardBorders.Count; i++) {
                Border b = monitorCardBorders[i];
                bool isAllCard = (i == screenDevices.Count);
                bool isSelected = isAllCard ? syncAllMonitors : (!syncAllMonitors && selectedMonitorIndex == i);

                if (isSelected) {
                    b.Background = new SolidColorBrush(Color.FromRgb(22, 28, 42));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    b.BorderThickness = new Thickness(2);
                } else {
                    b.Background = new SolidColorBrush(Color.FromRgb(18, 22, 32));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 36, 52));
                    b.BorderThickness = new Thickness(1);
                }
            }

            if (btnApplyToMonitor != null) {
                if (syncAllMonitors || selectedMonitorIndex == -1) {
                    btnApplyToMonitor.Content = "✔ " + I18n.T("ApplyToMonitor", I18n.T("AllDisplays"));
                } else if (selectedMonitorIndex >= 0 && selectedMonitorIndex < screenDevices.Count) {
                    btnApplyToMonitor.Content = "✔ " + I18n.T("ApplyToMonitor", screenDevices[selectedMonitorIndex].DisplayLabel);
                }
            }
        }

        private Border CreateDashboardCard() {
            Border b = new Border();
            b.Background = new SolidColorBrush(Color.FromRgb(18, 22, 32));
            b.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 36, 52));
            b.BorderThickness = new Thickness(1);
            b.CornerRadius = new CornerRadius(8);
            b.Padding = new Thickness(16, 14, 16, 14);
            return b;
        }

        private Grid CreateBrightnessContent() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Header: Sun icon + Brightness + 72%
            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock ic = new TextBlock();
            ic.Text = "☀";
            ic.FontSize = 14;
            ic.Foreground = new SolidColorBrush(Color.FromRgb(250, 204, 21));
            ic.Margin = new Thickness(0, 0, 6, 0);
            Grid.SetColumn(ic, 0);
            head.Children.Add(ic);

            lblBrightnessTitle = new TextBlock();
            lblBrightnessTitle.Text = I18n.T("Brightness");
            lblBrightnessTitle.FontSize = 13;
            lblBrightnessTitle.FontWeight = FontWeights.SemiBold;
            lblBrightnessTitle.Foreground = Brushes.White;
            Grid.SetColumn(lblBrightnessTitle, 1);
            head.Children.Add(lblBrightnessTitle);

            badgeBrightness = new TextBlock();
            badgeBrightness.Text = "100%";
            badgeBrightness.FontSize = 13;
            badgeBrightness.FontWeight = FontWeights.Bold;
            badgeBrightness.Foreground = Brushes.White;
            Grid.SetColumn(badgeBrightness, 2);
            head.Children.Add(badgeBrightness);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            // Body: Min sun icon, Slider, Max sun icon
            Grid body = new Grid();
            body.Margin = new Thickness(0, 10, 0, 0);
            body.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            body.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            body.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock minIc = new TextBlock();
            minIc.Text = "🔅";
            minIc.FontSize = 11;
            minIc.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            minIc.Margin = new Thickness(0, 0, 8, 0);
            minIc.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(minIc, 0);
            body.Children.Add(minIc);

            sliderBrightness = new Slider();
            sliderBrightness.Minimum = 10;
            sliderBrightness.Maximum = 150;
            sliderBrightness.Value = 100;
            if (sliderStyleBlue != null) sliderBrightness.Style = sliderStyleBlue;
            sliderBrightness.ValueChanged += delegate(object s, RoutedPropertyChangedEventArgs<double> e) {
                badgeBrightness.Text = string.Format("{0}%", (int)e.NewValue);
                if (!isUpdatingUi) OnControlChanged();
            };
            Grid.SetColumn(sliderBrightness, 1);
            body.Children.Add(sliderBrightness);

            TextBlock maxIc = new TextBlock();
            maxIc.Text = "🔆";
            maxIc.FontSize = 12;
            maxIc.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            maxIc.Margin = new Thickness(8, 0, 0, 0);
            maxIc.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(maxIc, 2);
            body.Children.Add(maxIc);

            Grid.SetRow(body, 1);
            g.Children.Add(body);
            return g;
        }

        private Grid CreateContrastContent() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock ic = new TextBlock();
            ic.Text = "◐";
            ic.FontSize = 14;
            ic.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            ic.Margin = new Thickness(0, 0, 6, 0);
            Grid.SetColumn(ic, 0);
            head.Children.Add(ic);

            lblContrastTitle = new TextBlock();
            lblContrastTitle.Text = I18n.T("Contrast");
            lblContrastTitle.FontSize = 13;
            lblContrastTitle.FontWeight = FontWeights.SemiBold;
            lblContrastTitle.Foreground = Brushes.White;
            Grid.SetColumn(lblContrastTitle, 1);
            head.Children.Add(lblContrastTitle);

            badgeContrast = new TextBlock();
            badgeContrast.Text = "100%";
            badgeContrast.FontSize = 13;
            badgeContrast.FontWeight = FontWeights.Bold;
            badgeContrast.Foreground = Brushes.White;
            Grid.SetColumn(badgeContrast, 2);
            head.Children.Add(badgeContrast);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            Grid body = new Grid();
            body.Margin = new Thickness(0, 10, 0, 0);
            body.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            body.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            body.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock minIc = new TextBlock();
            minIc.Text = "◑";
            minIc.FontSize = 12;
            minIc.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            minIc.Margin = new Thickness(0, 0, 8, 0);
            minIc.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(minIc, 0);
            body.Children.Add(minIc);

            sliderContrast = new Slider();
            sliderContrast.Minimum = 20;
            sliderContrast.Maximum = 200;
            sliderContrast.Value = 100;
            if (sliderStyleBlue != null) sliderContrast.Style = sliderStyleBlue;
            sliderContrast.ValueChanged += delegate(object s, RoutedPropertyChangedEventArgs<double> e) {
                badgeContrast.Text = string.Format("{0}%", (int)e.NewValue);
                if (!isUpdatingUi) OnControlChanged();
            };
            Grid.SetColumn(sliderContrast, 1);
            body.Children.Add(sliderContrast);

            TextBlock maxIc = new TextBlock();
            maxIc.Text = "●";
            maxIc.FontSize = 12;
            maxIc.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            maxIc.Margin = new Thickness(8, 0, 0, 0);
            maxIc.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(maxIc, 2);
            body.Children.Add(maxIc);

            Grid.SetRow(body, 1);
            g.Children.Add(body);
            return g;
        }

        private Grid CreateGammaContent() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock ic = new TextBlock();
            ic.Text = "📈";
            ic.FontSize = 13;
            ic.Foreground = new SolidColorBrush(Color.FromRgb(192, 132, 252));
            ic.Margin = new Thickness(0, 0, 6, 0);
            Grid.SetColumn(ic, 0);
            head.Children.Add(ic);

            lblGammaTitle = new TextBlock();
            lblGammaTitle.Text = I18n.T("Gamma");
            lblGammaTitle.FontSize = 13;
            lblGammaTitle.FontWeight = FontWeights.SemiBold;
            lblGammaTitle.Foreground = Brushes.White;
            Grid.SetColumn(lblGammaTitle, 1);
            head.Children.Add(lblGammaTitle);

            badgeGamma = new TextBlock();
            badgeGamma.Text = "1.0";
            badgeGamma.FontSize = 13;
            badgeGamma.FontWeight = FontWeights.Bold;
            badgeGamma.Foreground = Brushes.White;
            Grid.SetColumn(badgeGamma, 2);
            head.Children.Add(badgeGamma);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            sliderGamma = new Slider();
            sliderGamma.Minimum = 40;
            sliderGamma.Maximum = 260;
            sliderGamma.Value = 100;
            sliderGamma.Margin = new Thickness(0, 10, 0, 0);
            if (sliderStyleBlue != null) sliderGamma.Style = sliderStyleBlue;
            sliderGamma.ValueChanged += delegate(object s, RoutedPropertyChangedEventArgs<double> e) {
                double val = e.NewValue / 100.0;
                badgeGamma.Text = string.Format("{0:0.0}", val);
                gammaTick22Text.Text = string.Format("{0:0.0}", val);
                if (!isUpdatingUi) OnControlChanged();
            };
            Grid.SetRow(sliderGamma, 1);
            g.Children.Add(sliderGamma);

            // Scale tick row (1.8, 2.0, [2.2], 2.4, 2.6)
            Grid ticks = new Grid();
            ticks.Margin = new Thickness(0, 4, 0, 0);
            ticks.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticks.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticks.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticks.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticks.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            gammaTick18 = CreateTick("0.6");
            Grid.SetColumn(gammaTick18, 0);
            ticks.Children.Add(gammaTick18);

            gammaTick20 = CreateTick("0.8");
            Grid.SetColumn(gammaTick20, 1);
            ticks.Children.Add(gammaTick20);

            // Highlighted active tick badge
            gammaTick22Badge = new Border();
            gammaTick22Badge.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
            gammaTick22Badge.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            gammaTick22Badge.BorderThickness = new Thickness(1);
            gammaTick22Badge.CornerRadius = new CornerRadius(10);
            gammaTick22Badge.Padding = new Thickness(8, 2, 8, 2);
            gammaTick22Badge.HorizontalAlignment = HorizontalAlignment.Center;

            gammaTick22Text = new TextBlock();
            gammaTick22Text.Text = "1.0";
            gammaTick22Text.FontSize = 10;
            gammaTick22Text.FontWeight = FontWeights.Bold;
            gammaTick22Text.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            gammaTick22Badge.Child = gammaTick22Text;
            Grid.SetColumn(gammaTick22Badge, 2);
            ticks.Children.Add(gammaTick22Badge);

            gammaTick24 = CreateTick("1.4");
            Grid.SetColumn(gammaTick24, 3);
            ticks.Children.Add(gammaTick24);

            gammaTick26 = CreateTick("2.0");
            Grid.SetColumn(gammaTick26, 4);
            ticks.Children.Add(gammaTick26);

            Grid.SetRow(ticks, 2);
            g.Children.Add(ticks);
            return g;
        }

        private TextBlock CreateTick(string val) {
            TextBlock t = new TextBlock();
            t.Text = val;
            t.FontSize = 10;
            t.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            t.HorizontalAlignment = HorizontalAlignment.Center;
            t.VerticalAlignment = VerticalAlignment.Center;
            return t;
        }

        private Grid CreateColorTempContent() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock ic = new TextBlock();
            ic.Text = "🌡";
            ic.FontSize = 14;
            ic.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            ic.Margin = new Thickness(0, 0, 6, 0);
            Grid.SetColumn(ic, 0);
            head.Children.Add(ic);

            lblColorTempTitle = new TextBlock();
            lblColorTempTitle.Text = I18n.T("ColorTemp");
            lblColorTempTitle.FontSize = 13;
            lblColorTempTitle.FontWeight = FontWeights.SemiBold;
            lblColorTempTitle.Foreground = Brushes.White;
            Grid.SetColumn(lblColorTempTitle, 1);
            head.Children.Add(lblColorTempTitle);

            badgeColorTemp = new TextBlock();
            badgeColorTemp.Text = "6500 K";
            badgeColorTemp.FontSize = 13;
            badgeColorTemp.FontWeight = FontWeights.Bold;
            badgeColorTemp.Foreground = Brushes.White;
            Grid.SetColumn(badgeColorTemp, 2);
            head.Children.Add(badgeColorTemp);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            sliderColorTemp = new Slider();
            sliderColorTemp.Minimum = 2500;
            sliderColorTemp.Maximum = 9500;
            sliderColorTemp.Value = 6500;
            sliderColorTemp.Margin = new Thickness(0, 10, 0, 0);
            if (sliderStyleTemp != null) sliderColorTemp.Style = sliderStyleTemp;
            sliderColorTemp.ValueChanged += delegate(object s, RoutedPropertyChangedEventArgs<double> e) {
                badgeColorTemp.Text = string.Format("{0} K", (int)e.NewValue);
                if (!isUpdatingUi) OnControlChanged();
            };
            Grid.SetRow(sliderColorTemp, 1);
            g.Children.Add(sliderColorTemp);

            // Bottom row: Warm, [Neutral], Cool
            Grid bot = new Grid();
            bot.Margin = new Thickness(0, 4, 0, 0);
            bot.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            bot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            bot.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock warm = new TextBlock();
            warm.Text = "Warm";
            warm.FontSize = 11;
            warm.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            warm.Cursor = Cursors.Hand;
            warm.MouseLeftButtonUp += delegate { sliderColorTemp.Value = 3400; };
            Grid.SetColumn(warm, 0);
            bot.Children.Add(warm);

            Border neut = new Border();
            neut.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
            neut.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            neut.BorderThickness = new Thickness(1);
            neut.CornerRadius = new CornerRadius(12);
            neut.Padding = new Thickness(12, 2, 12, 2);
            neut.Cursor = Cursors.Hand;
            neut.HorizontalAlignment = HorizontalAlignment.Center;
            neut.MouseLeftButtonUp += delegate { sliderColorTemp.Value = 6500; };

            TextBlock neutText = new TextBlock();
            neutText.Text = "Neutral";
            neutText.FontSize = 11;
            neutText.FontWeight = FontWeights.SemiBold;
            neutText.Foreground = Brushes.White;
            neut.Child = neutText;
            Grid.SetColumn(neut, 1);
            bot.Children.Add(neut);

            TextBlock cool = new TextBlock();
            cool.Text = "Cool";
            cool.FontSize = 11;
            cool.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            cool.Cursor = Cursors.Hand;
            cool.MouseLeftButtonUp += delegate { sliderColorTemp.Value = 8500; };
            Grid.SetColumn(cool, 2);
            bot.Children.Add(cool);

            Grid.SetRow(bot, 2);
            g.Children.Add(bot);
            return g;
        }

        private Grid CreateChannelContent(string dot, string hex, string title, out TextBlock lblTitle, out Slider slider, out TextBlock badge, Style sStyle) {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock ic = new TextBlock();
            ic.Text = dot;
            ic.FontSize = 14;
            ic.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            ic.Margin = new Thickness(0, 0, 6, 0);
            Grid.SetColumn(ic, 0);
            head.Children.Add(ic);

            lblTitle = new TextBlock();
            lblTitle.Text = title;
            lblTitle.FontSize = 13;
            lblTitle.FontWeight = FontWeights.SemiBold;
            lblTitle.Foreground = Brushes.White;
            Grid.SetColumn(lblTitle, 1);
            head.Children.Add(lblTitle);

            badge = new TextBlock();
            badge.Text = "100";
            badge.FontSize = 13;
            badge.FontWeight = FontWeights.Bold;
            badge.Foreground = Brushes.White;
            Grid.SetColumn(badge, 2);
            head.Children.Add(badge);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            slider = new Slider();
            slider.Minimum = 0;
            slider.Maximum = 100;
            slider.Value = 100;
            slider.Margin = new Thickness(0, 10, 0, 0);
            if (sStyle != null) slider.Style = sStyle;
            TextBlock capturedBadge = badge;
            slider.ValueChanged += delegate(object s, RoutedPropertyChangedEventArgs<double> e) {
                capturedBadge.Text = string.Format("{0}", (int)e.NewValue);
                if (!isUpdatingUi) OnControlChanged();
            };
            Grid.SetRow(slider, 1);
            g.Children.Add(slider);

            Grid bot = new Grid();
            bot.Margin = new Thickness(0, 4, 0, 0);
            bot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            bot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock l0 = new TextBlock();
            l0.Text = "0";
            l0.FontSize = 10;
            l0.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            Grid.SetColumn(l0, 0);
            bot.Children.Add(l0);

            TextBlock l100 = new TextBlock();
            l100.Text = "100";
            l100.FontSize = 10;
            l100.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            l100.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(l100, 1);
            bot.Children.Add(l100);

            Grid.SetRow(bot, 2);
            g.Children.Add(bot);
            return g;
        }

        private Grid CreateQuickPresetsContent() {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Left Header: Sparkle + Quick Presets + Subtitle
            StackPanel left = new StackPanel();
            left.Margin = new Thickness(0, 0, 20, 0);
            left.VerticalAlignment = VerticalAlignment.Center;

            StackPanel titleRow = new StackPanel();
            titleRow.Orientation = Orientation.Horizontal;

            TextBlock star = new TextBlock();
            star.Text = "✦";
            star.FontSize = 14;
            star.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            star.Margin = new Thickness(0, 0, 6, 0);
            titleRow.Children.Add(star);

            lblQuickPresetsTitle = new TextBlock();
            lblQuickPresetsTitle.Text = I18n.T("QuickPresets");
            lblQuickPresetsTitle.FontSize = 13;
            lblQuickPresetsTitle.FontWeight = FontWeights.Bold;
            lblQuickPresetsTitle.Foreground = Brushes.White;
            titleRow.Children.Add(lblQuickPresetsTitle);
            left.Children.Add(titleRow);

            lblQuickPresetsSubtitle = new TextBlock();
            lblQuickPresetsSubtitle.Text = I18n.T("QuickPresetsDesc");
            lblQuickPresetsSubtitle.FontSize = 11;
            lblQuickPresetsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            lblQuickPresetsSubtitle.Margin = new Thickness(0, 2, 0, 0);
            left.Children.Add(lblQuickPresetsSubtitle);

            Grid.SetColumn(left, 0);
            g.Children.Add(left);

            // Right: Horizontal row of pill buttons
            WrapPanel wp = new WrapPanel();
            wp.VerticalAlignment = VerticalAlignment.Center;
            wp.HorizontalAlignment = HorizontalAlignment.Right;
            presetButtons = new List<Button>();

            btnPresetWarm = CreatePresetPill("☀ Warm", delegate { ApplyPreset(0.85, 1.0, 1.05, 3800, 1.0, 0.95, 0.80, "Warm"); });
            btnPresetNeutral = CreatePresetPill("🎯 Neutral", delegate { ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0, "Standard"); }, true);
            btnPresetCool = CreatePresetPill("❄ Cool", delegate { ApplyPreset(1.0, 1.05, 1.0, 8500, 0.95, 0.98, 1.0, "Cool"); });
            btnPresetMovie = CreatePresetPill("🎬 Movie", delegate { ApplyPreset(1.05, 1.25, 0.95, 6500, 1.0, 1.0, 1.0, "Movie"); });
            btnPresetWork = CreatePresetPill("🖥 Work", delegate { ApplyPreset(0.80, 1.05, 1.10, 5000, 1.0, 0.98, 0.90, "Work"); });
            btnPresetNight = CreatePresetPill("🌙 Night", delegate { ApplyPreset(0.60, 0.95, 1.10, 3000, 1.0, 0.88, 0.65, "Night"); });

            presetButtons.Add(btnPresetWarm);
            presetButtons.Add(btnPresetNeutral);
            presetButtons.Add(btnPresetCool);
            presetButtons.Add(btnPresetMovie);
            presetButtons.Add(btnPresetWork);
            presetButtons.Add(btnPresetNight);

            foreach (Button pb in presetButtons) wp.Children.Add(pb);

            Grid.SetColumn(wp, 1);
            g.Children.Add(wp);
            return g;
        }

        private Button CreatePresetPill(string label, RoutedEventHandler click, bool isSelected = false) {
            Button b = new Button();
            b.Content = label;
            b.Height = 32;
            b.Padding = new Thickness(14, 0, 14, 0);
            b.Margin = new Thickness(0, 0, 8, 4);
            b.FontSize = 12;
            b.FontWeight = FontWeights.Medium;
            b.Cursor = Cursors.Hand;
            b.BorderThickness = new Thickness(1);

            if (isSelected) {
                b.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                b.Foreground = Brushes.White;
            } else {
                b.Background = new SolidColorBrush(Color.FromRgb(22, 28, 40));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(36, 44, 62));
                b.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
            }

            b.Click += delegate(object s, RoutedEventArgs e) {
                foreach (Button other in presetButtons) {
                    other.Background = new SolidColorBrush(Color.FromRgb(22, 28, 40));
                    other.BorderBrush = new SolidColorBrush(Color.FromRgb(36, 44, 62));
                    other.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
                }
                b.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                b.Foreground = Brushes.White;
                click(s, e);
            };

            return b;
        }

        private Grid CreateBottomActionBar() {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Left: Active Profile dropdown
            Border profBorder = new Border();
            profBorder.Height = 36;
            profBorder.Background = new SolidColorBrush(Color.FromRgb(18, 22, 32));
            profBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 36, 52));
            profBorder.BorderThickness = new Thickness(1);
            profBorder.CornerRadius = new CornerRadius(6);
            profBorder.Padding = new Thickness(10, 0, 10, 0);

            StackPanel profSp = new StackPanel();
            profSp.Orientation = Orientation.Horizontal;
            profSp.VerticalAlignment = VerticalAlignment.Center;

            TextBlock uIc = new TextBlock();
            uIc.Text = "👤";
            uIc.FontSize = 13;
            uIc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            uIc.Margin = new Thickness(0, 0, 6, 0);
            profSp.Children.Add(uIc);

            lblActiveProfileTitle = new TextBlock();
            lblActiveProfileTitle.Text = I18n.T("ActiveProfile");
            lblActiveProfileTitle.FontSize = 12;
            lblActiveProfileTitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            lblActiveProfileTitle.Margin = new Thickness(0, 0, 8, 0);
            profSp.Children.Add(lblActiveProfileTitle);

            comboActiveProfile = new ComboBox();
            comboActiveProfile.Background = Brushes.Transparent;
            comboActiveProfile.BorderThickness = new Thickness(0);
            comboActiveProfile.Foreground = Brushes.White;
            comboActiveProfile.Width = 110;
            comboActiveProfile.Height = 28;
            comboActiveProfile.Items.Add("Standard");
            comboActiveProfile.Items.Add("Gaming");
            comboActiveProfile.Items.Add("Night Shift");
            comboActiveProfile.Items.Add("Cinema");
            comboActiveProfile.Items.Add("Reading");
            comboActiveProfile.SelectedIndex = 0;
            comboActiveProfile.SelectionChanged += delegate {
                if (comboActiveProfile.SelectedItem != null) {
                    config.ActiveProfile = comboActiveProfile.SelectedItem.ToString();
                    config.Save();
                }
            };
            profSp.Children.Add(comboActiveProfile);

            profBorder.Child = profSp;
            Grid.SetColumn(profBorder, 0);
            g.Children.Add(profBorder);

            // Right: Reset to Default + Apply to Monitor X
            StackPanel actRow = new StackPanel();
            actRow.Orientation = Orientation.Horizontal;

            btnResetDefault = new Button();
            btnResetDefault.Height = 36;
            btnResetDefault.Background = new SolidColorBrush(Color.FromRgb(26, 31, 46));
            btnResetDefault.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 48, 68));
            btnResetDefault.BorderThickness = new Thickness(1);
            btnResetDefault.Padding = new Thickness(16, 0, 16, 0);
            btnResetDefault.Margin = new Thickness(0, 0, 10, 0);
            btnResetDefault.Cursor = Cursors.Hand;
            btnResetDefault.Content = "🔄 " + I18n.T("ResetToDefault");
            btnResetDefault.Foreground = Brushes.White;
            btnResetDefault.FontSize = 12;
            btnResetDefault.FontWeight = FontWeights.Medium;
            btnResetDefault.Click += delegate { ResetCurrentOrAll(); };
            actRow.Children.Add(btnResetDefault);

            btnApplyToMonitor = new Button();
            btnApplyToMonitor.Height = 36;
            btnApplyToMonitor.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            btnApplyToMonitor.BorderThickness = new Thickness(0);
            btnApplyToMonitor.Padding = new Thickness(20, 0, 20, 0);
            btnApplyToMonitor.Cursor = Cursors.Hand;
            btnApplyToMonitor.Content = "✔ " + I18n.T("ApplyToMonitor", "Monitor 1");
            btnApplyToMonitor.Foreground = Brushes.White;
            btnApplyToMonitor.FontSize = 13;
            btnApplyToMonitor.FontWeight = FontWeights.SemiBold;
            btnApplyToMonitor.Click += delegate {
                ApplyAllActiveSettings();
                config.Save();
            };
            actRow.Children.Add(btnApplyToMonitor);

            Grid.SetColumn(actRow, 2);
            g.Children.Add(actRow);
            return g;
        }
        #endregion

        #region Settings View (Windows 11 Style)
        private Grid CreateSettingsView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(28, 20, 28, 28);

            // Settings Header
            StackPanel headerSp = new StackPanel();
            headerSp.Margin = new Thickness(0, 0, 0, 20);

            txtSettingsTitle = new TextBlock();
            txtSettingsTitle.Text = I18n.T("SettingsTitle");
            txtSettingsTitle.FontSize = 22;
            txtSettingsTitle.FontWeight = FontWeights.Bold;
            txtSettingsTitle.Foreground = Brushes.White;
            headerSp.Children.Add(txtSettingsTitle);

            txtSettingsSubtitle = new TextBlock();
            txtSettingsSubtitle.Text = I18n.T("SettingsSubtitle");
            txtSettingsSubtitle.FontSize = 12;
            txtSettingsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtSettingsSubtitle.Margin = new Thickness(0, 4, 0, 0);
            headerSp.Children.Add(txtSettingsSubtitle);
            sp.Children.Add(headerSp);

            // 1. Language Card
            Border langCard = CreateDashboardCard();
            langCard.Margin = new Thickness(0, 0, 0, 12);

            Grid langGrid = new Grid();
            langGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            langGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(220) });

            StackPanel langInfo = new StackPanel();
            txtLangTitle = new TextBlock();
            txtLangTitle.Text = I18n.T("LangTitle");
            txtLangTitle.FontSize = 14;
            txtLangTitle.FontWeight = FontWeights.SemiBold;
            txtLangTitle.Foreground = Brushes.White;
            langInfo.Children.Add(txtLangTitle);

            txtLangDesc = new TextBlock();
            txtLangDesc.Text = I18n.T("LangDesc");
            txtLangDesc.FontSize = 12;
            txtLangDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtLangDesc.TextWrapping = TextWrapping.Wrap;
            txtLangDesc.Margin = new Thickness(0, 4, 16, 0);
            langInfo.Children.Add(txtLangDesc);

            Grid.SetColumn(langInfo, 0);
            langGrid.Children.Add(langInfo);

            comboLanguage = new ComboBox();
            comboLanguage.Background = new SolidColorBrush(Color.FromRgb(30, 36, 50));
            comboLanguage.Foreground = Brushes.White;
            comboLanguage.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 68, 92));
            comboLanguage.Height = 36;
            comboLanguage.VerticalAlignment = VerticalAlignment.Center;
            comboLanguage.VerticalContentAlignment = VerticalAlignment.Center;
            comboLanguage.Padding = new Thickness(10, 4, 10, 4);

            foreach (string lName in I18n.LanguageNames) {
                comboLanguage.Items.Add(lName);
            }
            comboLanguage.SelectedIndex = (int)I18n.CurrentLanguage;

            // Immediate preview on language switch
            comboLanguage.SelectionChanged += delegate {
                if (comboLanguage.SelectedIndex >= 0) {
                    I18n.CurrentLanguage = (AppLanguage)comboLanguage.SelectedIndex;
                    UpdateAllTexts();
                }
            };

            Grid.SetColumn(comboLanguage, 1);
            langGrid.Children.Add(comboLanguage);
            langCard.Child = langGrid;
            sp.Children.Add(langCard);

            // 2. Startup Card with Conditional Subitem
            Border startupCard = CreateDashboardCard();
            startupCard.Margin = new Thickness(0, 0, 0, 12);
            StackPanel startupSp = new StackPanel();

            Grid startRow = new Grid();
            startRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            startRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel startInfo = new StackPanel();
            txtStartWinTitle = new TextBlock();
            txtStartWinTitle.Text = I18n.T("StartWinTitle");
            txtStartWinTitle.FontSize = 14;
            txtStartWinTitle.FontWeight = FontWeights.SemiBold;
            txtStartWinTitle.Foreground = Brushes.White;
            startInfo.Children.Add(txtStartWinTitle);

            txtStartWinDesc = new TextBlock();
            txtStartWinDesc.Text = I18n.T("StartWinDesc");
            txtStartWinDesc.FontSize = 12;
            txtStartWinDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            startInfo.Children.Add(txtStartWinDesc);

            Grid.SetColumn(startInfo, 0);
            startRow.Children.Add(startInfo);

            chkStartWithWindows = new CheckBox();
            chkStartWithWindows.IsChecked = config.StartWithWindows;
            chkStartWithWindows.VerticalAlignment = VerticalAlignment.Center;
            chkStartWithWindows.Cursor = Cursors.Hand;
            Grid.SetColumn(chkStartWithWindows, 1);
            startRow.Children.Add(chkStartWithWindows);

            startupSp.Children.Add(startRow);

            // Conditional Subitem: Start Minimized to Tray (Greyed out when parent unchecked!)
            subCardStartMinimized = new Border();
            subCardStartMinimized.Margin = new Thickness(0, 12, 0, 0);
            subCardStartMinimized.Padding = new Thickness(14, 10, 14, 10);
            subCardStartMinimized.Background = new SolidColorBrush(Color.FromRgb(26, 31, 44));
            subCardStartMinimized.CornerRadius = new CornerRadius(6);

            Grid subRow = new Grid();
            subRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            subRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel subInfo = new StackPanel();
            txtStartMinTitle = new TextBlock();
            txtStartMinTitle.Text = I18n.T("StartMinTitle");
            txtStartMinTitle.FontSize = 13;
            txtStartMinTitle.FontWeight = FontWeights.SemiBold;
            txtStartMinTitle.Foreground = Brushes.White;
            subInfo.Children.Add(txtStartMinTitle);

            txtStartMinDesc = new TextBlock();
            txtStartMinDesc.Text = I18n.T("StartMinDesc");
            txtStartMinDesc.FontSize = 11;
            txtStartMinDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            subInfo.Children.Add(txtStartMinDesc);

            Grid.SetColumn(subInfo, 0);
            subRow.Children.Add(subInfo);

            chkStartMinimized = new CheckBox();
            chkStartMinimized.IsChecked = config.StartMinimized;
            chkStartMinimized.VerticalAlignment = VerticalAlignment.Center;
            chkStartMinimized.Cursor = Cursors.Hand;
            Grid.SetColumn(chkStartMinimized, 1);
            subRow.Children.Add(chkStartMinimized);

            subCardStartMinimized.Child = subRow;
            startupSp.Children.Add(subCardStartMinimized);

            Action updateSubState = delegate {
                bool isParentChecked = (chkStartWithWindows.IsChecked == true);
                chkStartMinimized.IsEnabled = isParentChecked;
                subCardStartMinimized.Opacity = isParentChecked ? 1.0 : 0.45;
            };

            chkStartWithWindows.Checked += delegate { updateSubState(); };
            chkStartWithWindows.Unchecked += delegate { updateSubState(); };
            updateSubState();

            startupCard.Child = startupSp;
            sp.Children.Add(startupCard);

            // 3. Minimize to Tray on Close Card (Default True)
            Border closeCard = CreateDashboardCard();
            closeCard.Margin = new Thickness(0, 0, 0, 12);
            Grid closeGrid = new Grid();
            closeGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            closeGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel closeInfo = new StackPanel();
            txtMinCloseTitle = new TextBlock();
            txtMinCloseTitle.Text = I18n.T("MinCloseTitle");
            txtMinCloseTitle.FontSize = 14;
            txtMinCloseTitle.FontWeight = FontWeights.SemiBold;
            txtMinCloseTitle.Foreground = Brushes.White;
            closeInfo.Children.Add(txtMinCloseTitle);

            txtMinCloseDesc = new TextBlock();
            txtMinCloseDesc.Text = I18n.T("MinCloseDesc");
            txtMinCloseDesc.FontSize = 12;
            txtMinCloseDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            closeInfo.Children.Add(txtMinCloseDesc);

            Grid.SetColumn(closeInfo, 0);
            closeGrid.Children.Add(closeInfo);

            chkMinimizeOnClose = new CheckBox();
            chkMinimizeOnClose.IsChecked = config.MinimizeOnClose;
            chkMinimizeOnClose.VerticalAlignment = VerticalAlignment.Center;
            chkMinimizeOnClose.Cursor = Cursors.Hand;
            Grid.SetColumn(chkMinimizeOnClose, 1);
            closeGrid.Children.Add(chkMinimizeOnClose);

            closeCard.Child = closeGrid;
            sp.Children.Add(closeCard);

            // 4. About & Version Card (Version 0.1.0)
            Border aboutCard = CreateDashboardCard();
            aboutCard.Margin = new Thickness(0, 0, 0, 14);
            Grid aboutGrid = new Grid();
            aboutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            aboutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border aboutIcon = new Border();
            aboutIcon.Width = 42;
            aboutIcon.Height = 42;
            aboutIcon.CornerRadius = new CornerRadius(8);
            aboutIcon.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
            aboutIcon.Margin = new Thickness(0, 0, 14, 0);

            TextBlock iTb = new TextBlock();
            iTb.Text = "❖";
            iTb.FontSize = 22;
            iTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            iTb.HorizontalAlignment = HorizontalAlignment.Center;
            iTb.VerticalAlignment = VerticalAlignment.Center;
            aboutIcon.Child = iTb;
            Grid.SetColumn(aboutIcon, 0);
            aboutGrid.Children.Add(aboutIcon);

            StackPanel aboutInfo = new StackPanel();
            txtAboutTitle = new TextBlock();
            txtAboutTitle.Text = I18n.T("AboutTitle");
            txtAboutTitle.FontSize = 14;
            txtAboutTitle.FontWeight = FontWeights.SemiBold;
            txtAboutTitle.Foreground = Brushes.White;
            aboutInfo.Children.Add(txtAboutTitle);

            txtAboutVersion = new TextBlock();
            txtAboutVersion.Text = I18n.T("AboutVersion");
            txtAboutVersion.FontSize = 12;
            txtAboutVersion.FontWeight = FontWeights.Bold;
            txtAboutVersion.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            txtAboutVersion.Margin = new Thickness(0, 2, 0, 2);
            aboutInfo.Children.Add(txtAboutVersion);

            txtAboutDesc = new TextBlock();
            txtAboutDesc.Text = I18n.T("AboutDesc");
            txtAboutDesc.FontSize = 11;
            txtAboutDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            aboutInfo.Children.Add(txtAboutDesc);

            Grid.SetColumn(aboutInfo, 1);
            aboutGrid.Children.Add(aboutInfo);
            aboutCard.Child = aboutGrid;
            sp.Children.Add(aboutCard);

            // 5. Actions Bar (Apply, OK, Cancel)
            Grid actBar = new Grid();
            actBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            actBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            lblSettingsSaveNotice = new TextBlock();
            lblSettingsSaveNotice.Text = "✔ " + I18n.T("SavedNotice");
            lblSettingsSaveNotice.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            lblSettingsSaveNotice.FontSize = 13;
            lblSettingsSaveNotice.FontWeight = FontWeights.SemiBold;
            lblSettingsSaveNotice.VerticalAlignment = VerticalAlignment.Center;
            lblSettingsSaveNotice.Visibility = Visibility.Collapsed;
            Grid.SetColumn(lblSettingsSaveNotice, 0);
            actBar.Children.Add(lblSettingsSaveNotice);

            StackPanel bRow = new StackPanel();
            bRow.Orientation = Orientation.Horizontal;

            btnSettingsCancel = CreateSettingsButton(I18n.T("BtnCancel"), false, delegate {
                I18n.CurrentLanguage = I18n.CommittedLanguage;
                comboLanguage.SelectedIndex = (int)I18n.CommittedLanguage;
                chkStartWithWindows.IsChecked = config.StartWithWindows;
                chkStartMinimized.IsChecked = config.StartMinimized;
                chkMinimizeOnClose.IsChecked = config.MinimizeOnClose;
                UpdateAllTexts();
                SwitchToDashboard();
            });

            btnSettingsApply = CreateSettingsButton(I18n.T("BtnApply"), true, delegate {
                SaveSettingsState();
                lblSettingsSaveNotice.Visibility = Visibility.Visible;
            });

            btnSettingsOK = CreateSettingsButton(I18n.T("BtnOK"), true, delegate {
                SaveSettingsState();
                SwitchToDashboard();
            });

            bRow.Children.Add(btnSettingsCancel);
            bRow.Children.Add(btnSettingsApply);
            bRow.Children.Add(btnSettingsOK);

            Grid.SetColumn(bRow, 1);
            actBar.Children.Add(bRow);
            sp.Children.Add(actBar);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private Button CreateSettingsButton(string label, bool isAccent, RoutedEventHandler click) {
            Button b = new Button();
            b.Content = label;
            b.Height = 36;
            b.MinWidth = 90;
            b.Padding = new Thickness(16, 0, 16, 0);
            b.Margin = new Thickness(8, 0, 0, 0);
            b.FontSize = 13;
            b.FontWeight = FontWeights.SemiBold;
            b.Cursor = Cursors.Hand;
            b.BorderThickness = new Thickness(1);

            if (isAccent) {
                b.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                b.Foreground = Brushes.White;
            } else {
                b.Background = new SolidColorBrush(Color.FromRgb(26, 31, 46));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 48, 68));
                b.Foreground = Brushes.White;
            }
            b.Click += click;
            return b;
        }

        private void SaveSettingsState() {
            I18n.CommittedLanguage = I18n.CurrentLanguage;
            config.Language = I18n.CurrentLanguage;
            config.StartWithWindows = (chkStartWithWindows.IsChecked == true);
            config.StartMinimized = (chkStartMinimized.IsChecked == true);
            config.MinimizeOnClose = (chkMinimizeOnClose.IsChecked == true);

            AppConfig.SetAutoStart(config.StartWithWindows, config.StartMinimized);
            config.Save();
            UpdateTrayMenu();
        }
        #endregion

        #region Value Management & Real-time Output
        public void UpdateAllTexts() {
            if (mainTitle == null) return;

            headerStatus.Text = I18n.T("RunningInTray");
            sidebarMonitorsCount.Text = I18n.T("MonitorsConnected", screenDevices.Count);

            mainTitle.Text = I18n.T("MonitorsTitle");
            mainSubtitle.Text = I18n.T("MonitorsSubtitle");

            btnIdentify.Content = "ⓘ " + I18n.T("Identify");
            btnMinimizeToTray.Content = "🖥 " + I18n.T("MinimizeToTray");

            lblBrightnessTitle.Text = I18n.T("Brightness");
            lblContrastTitle.Text = I18n.T("Contrast");
            lblGammaTitle.Text = I18n.T("Gamma");
            lblColorTempTitle.Text = I18n.T("ColorTemp");
            lblRedTitle.Text = I18n.T("Red");
            lblGreenTitle.Text = I18n.T("Green");
            lblBlueTitle.Text = I18n.T("Blue");
            lblQuickPresetsTitle.Text = I18n.T("QuickPresets");
            lblQuickPresetsSubtitle.Text = I18n.T("QuickPresetsDesc");
            lblActiveProfileTitle.Text = I18n.T("ActiveProfile");
            btnResetDefault.Content = "🔄 " + I18n.T("ResetToDefault");

            btnPresetWarm.Content = I18n.T("PresetWarm");
            btnPresetNeutral.Content = I18n.T("PresetNeutral");
            btnPresetCool.Content = I18n.T("PresetCool");
            btnPresetMovie.Content = I18n.T("PresetMovie");
            btnPresetWork.Content = I18n.T("PresetWork");
            btnPresetNight.Content = I18n.T("PresetNight");

            // Settings view texts
            txtSettingsTitle.Text = I18n.T("SettingsTitle");
            txtSettingsSubtitle.Text = I18n.T("SettingsSubtitle");
            txtLangTitle.Text = I18n.T("LangTitle");
            txtLangDesc.Text = I18n.T("LangDesc");
            txtStartWinTitle.Text = I18n.T("StartWinTitle");
            txtStartWinDesc.Text = I18n.T("StartWinDesc");
            txtStartMinTitle.Text = I18n.T("StartMinTitle");
            txtStartMinDesc.Text = I18n.T("StartMinDesc");
            txtMinCloseTitle.Text = I18n.T("MinCloseTitle");
            txtMinCloseDesc.Text = I18n.T("MinCloseDesc");
            txtAboutTitle.Text = I18n.T("AboutTitle");
            txtAboutVersion.Text = I18n.T("AboutVersion");
            txtAboutDesc.Text = I18n.T("AboutDesc");

            btnSettingsApply.Content = I18n.T("BtnApply");
            btnSettingsOK.Content = I18n.T("BtnOK");
            btnSettingsCancel.Content = I18n.T("BtnCancel");

            UpdateVisualMonitorCards();
            UpdateTrayMenu();
        }

        private DisplaySettings GetCurrentTargetSettings() {
            if (syncAllMonitors || selectedMonitorIndex == -1) {
                return globalSettings;
            }
            if (selectedMonitorIndex >= 0 && selectedMonitorIndex < screenDevices.Count) {
                string dev = screenDevices[selectedMonitorIndex].DeviceName;
                return settingsPerMonitor[dev];
            }
            return globalSettings;
        }

        private void UpdateUiFromSettings() {
            isUpdatingUi = true;
            DisplaySettings s = GetCurrentTargetSettings();

            sliderBrightness.Value = s.Brightness * 100.0;
            sliderContrast.Value = s.Contrast * 100.0;
            sliderGamma.Value = s.Gamma * 100.0;
            sliderColorTemp.Value = s.Temperature;
            sliderRed.Value = s.Red * 100.0;
            sliderGreen.Value = s.Green * 100.0;
            sliderBlue.Value = s.Blue * 100.0;

            badgeBrightness.Text = string.Format("{0}%", (int)sliderBrightness.Value);
            badgeContrast.Text = string.Format("{0}%", (int)sliderContrast.Value);
            badgeGamma.Text = string.Format("{0:0.0}", sliderGamma.Value / 100.0);
            gammaTick22Text.Text = string.Format("{0:0.0}", sliderGamma.Value / 100.0);
            badgeColorTemp.Text = string.Format("{0} K", (int)sliderColorTemp.Value);
            badgeRed.Text = string.Format("{0}", (int)sliderRed.Value);
            badgeGreen.Text = string.Format("{0}", (int)sliderGreen.Value);
            badgeBlue.Text = string.Format("{0}", (int)sliderBlue.Value);

            isUpdatingUi = false;
        }

        private void OnControlChanged() {
            DisplaySettings s = GetCurrentTargetSettings();
            s.Brightness = sliderBrightness.Value / 100.0;
            s.Contrast = sliderContrast.Value / 100.0;
            s.Gamma = sliderGamma.Value / 100.0;
            s.Temperature = sliderColorTemp.Value;
            s.Red = sliderRed.Value / 100.0;
            s.Green = sliderGreen.Value / 100.0;
            s.Blue = sliderBlue.Value / 100.0;

            ApplyAllActiveSettings();

            config.Brightness = s.Brightness;
            config.Contrast = s.Contrast;
            config.Gamma = s.Gamma;
            config.Temperature = s.Temperature;
            config.Red = s.Red;
            config.Green = s.Green;
            config.Blue = s.Blue;
            config.SyncMonitors = syncAllMonitors;
            config.Save();
        }

        private void ApplyAllActiveSettings() {
            DisplaySettings current = GetCurrentTargetSettings();

            // 1. DWM Hardware Compositor Matrix (Affects Contrast, Gamma, Brightness, and Colors directly on DWM!)
            DwmEngine.Apply(current);

            // 2. Click-Through Per-Screen Overlays (For deep dimming and per-screen tinting)
            if (syncAllMonitors || selectedMonitorIndex == -1) {
                overlayManager.UpdateAll(globalSettings);
                byte[] ramp = GammaMath.BuildGammaRamp(globalSettings);
                foreach (ScreenDevice sd in screenDevices) {
                    ApplyRampToDevice(sd.DeviceName, ramp);
                }
            } else {
                if (selectedMonitorIndex >= 0 && selectedMonitorIndex < screenDevices.Count) {
                    ScreenDevice sd = screenDevices[selectedMonitorIndex];
                    DisplaySettings s = settingsPerMonitor[sd.DeviceName];
                    overlayManager.UpdateDisplay(sd.DeviceName, s);

                    byte[] ramp = GammaMath.BuildGammaRamp(s);
                    ApplyRampToDevice(sd.DeviceName, ramp);
                }
            }
        }

        private bool ApplyRampToDevice(string devName, byte[] ramp) {
            IntPtr hdc = NativeMethods.CreateDC(null, devName, null, IntPtr.Zero);
            if (hdc == IntPtr.Zero) return false;
            bool ok = NativeMethods.SetDeviceGammaRamp(hdc, ramp);
            NativeMethods.DeleteDC(hdc);
            return ok;
        }

        private void ApplyPreset(double b, double c, double g, double temp, double red, double green, double blue, string profName) {
            DisplaySettings s = GetCurrentTargetSettings();
            s.Brightness = b;
            s.Contrast = c;
            s.Gamma = g;
            s.Temperature = temp;
            s.Red = red;
            s.Green = green;
            s.Blue = blue;
            s.ProfileName = profName;

            UpdateUiFromSettings();
            OnControlChanged();
        }

        private void ResetCurrentOrAll() {
            if (syncAllMonitors || selectedMonitorIndex == -1) {
                globalSettings.ResetToDefaults();
                foreach (var kvp in settingsPerMonitor) {
                    kvp.Value.ResetToDefaults();
                }
                DwmEngine.Reset();
                overlayManager.UpdateAll(globalSettings);
                foreach (ScreenDevice sd in screenDevices) {
                    if (sd.OriginalRamp != null) ApplyRampToDevice(sd.DeviceName, sd.OriginalRamp);
                }
            } else {
                if (selectedMonitorIndex >= 0 && selectedMonitorIndex < screenDevices.Count) {
                    ScreenDevice sd = screenDevices[selectedMonitorIndex];
                    DisplaySettings s = settingsPerMonitor[sd.DeviceName];
                    s.ResetToDefaults();
                    overlayManager.UpdateDisplay(sd.DeviceName, s);
                    if (sd.OriginalRamp != null) ApplyRampToDevice(sd.DeviceName, sd.OriginalRamp);
                }
            }
            UpdateUiFromSettings();
            OnControlChanged();
        }
        #endregion

        #region System Tray & Lifecycle
        private void SetupTrayIcon() {
            trayIcon = new SWF.NotifyIcon();
            trayIcon.Text = "Lucent Display Control";
            trayIcon.Icon = GenerateTrayIcon();
            trayIcon.Visible = true;
            trayIcon.DoubleClick += delegate { RestoreWindow(); };

            UpdateTrayMenu();
        }

        private void UpdateTrayMenu() {
            if (trayIcon == null) return;
            SWF.ContextMenu menu = new SWF.ContextMenu();
            menu.MenuItems.Add(I18n.T("TrayOpen"), delegate { RestoreWindow(); });
            menu.MenuItems.Add(I18n.T("TraySettings"), delegate { RestoreWindow(); SwitchToSettings(); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(I18n.T("PresetWarm"), delegate { ApplyPreset(0.85, 1.0, 1.05, 3800, 1.0, 0.95, 0.80, "Warm"); });
            menu.MenuItems.Add(I18n.T("PresetNeutral"), delegate { ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0, "Standard"); });
            menu.MenuItems.Add(I18n.T("PresetCool"), delegate { ApplyPreset(1.0, 1.05, 1.0, 8500, 0.95, 0.98, 1.0, "Cool"); });
            menu.MenuItems.Add(I18n.T("PresetMovie"), delegate { ApplyPreset(1.05, 1.25, 0.95, 6500, 1.0, 1.0, 1.0, "Movie"); });
            menu.MenuItems.Add(I18n.T("PresetWork"), delegate { ApplyPreset(0.80, 1.05, 1.10, 5000, 1.0, 0.98, 0.90, "Work"); });
            menu.MenuItems.Add(I18n.T("PresetNight"), delegate { ApplyPreset(0.60, 0.95, 1.10, 3000, 1.0, 0.88, 0.65, "Night"); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(I18n.T("ResetToDefault"), delegate { ResetCurrentOrAll(); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(I18n.T("TrayExit"), delegate { RealExit(); });
            trayIcon.ContextMenu = menu;
        }

        private SD.Icon GenerateTrayIcon() {
            using (SD.Bitmap bmp = new SD.Bitmap(32, 32)) {
                using (SD.Graphics g = SD.Graphics.FromImage(bmp)) {
                    g.SmoothingMode = G2D.SmoothingMode.AntiAlias;

                    using (G2D.GraphicsPath path = new G2D.GraphicsPath()) {
                        int r = 6;
                        path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                        path.AddArc(32 - 2 * r, 0, r * 2, r * 2, 270, 90);
                        path.AddArc(32 - 2 * r, 32 - 2 * r, r * 2, r * 2, 0, 90);
                        path.AddArc(0, 32 - 2 * r, r * 2, r * 2, 90, 90);
                        path.CloseFigure();

                        using (G2D.LinearGradientBrush lgb = new G2D.LinearGradientBrush(new SD.Point(0, 0), new SD.Point(32, 32),
                            SD.Color.FromArgb(56, 189, 248), SD.Color.FromArgb(99, 102, 241))) {
                            g.FillPath(lgb, path);
                        }
                    }

                    using (SD.SolidBrush b = new SD.SolidBrush(SD.Color.FromArgb(254, 240, 138))) {
                        g.FillEllipse(b, 10, 10, 12, 12);
                    }

                    using (SD.Pen p = new SD.Pen(SD.Color.White, 2f)) {
                        g.DrawLine(p, 16, 4, 16, 8);
                        g.DrawLine(p, 16, 24, 16, 28);
                        g.DrawLine(p, 4, 16, 8, 16);
                        g.DrawLine(p, 24, 16, 28, 16);
                    }
                }
                IntPtr hIcon = bmp.GetHicon();
                return SD.Icon.FromHandle(hIcon);
            }
        }

        public void RestoreWindow() {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (config.MinimizeOnClose) {
                e.Cancel = true;
                Hide();
            } else {
                RealExit();
            }
        }

        private void RealExit() {
            if (trayIcon != null) {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            DwmEngine.Reset();
            if (overlayManager != null) {
                overlayManager.CloseAll();
            }
            Application.Current.Shutdown();
        }
        #endregion
    }
}
