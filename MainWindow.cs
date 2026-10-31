using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using SD = System.Drawing;
using G2D = System.Drawing.Drawing2D;
using SWF = System.Windows.Forms;

namespace LucentControl {
    public class MainWindow : Window {
        private AppConfig config;
        private List<ScreenDevice> screenDevices;
        private Dictionary<string, DisplaySettings> settingsPerMonitor;
        private DisplaySettings globalSettings;
        private bool syncAllMonitors = true;
        private int selectedMonitorIndex = -1; // -1 means All Displays

        private OverlayManager overlayManager;
        private SWF.NotifyIcon trayIcon;

        // Navigation
        private Button btnNavDisplays;
        private Button btnNavSettings;
        private Grid viewDisplays;
        private Grid viewSettings;

        // UI elements that need dynamic translation update
        private TextBlock headerTitle;
        private TextBlock headerSubtitle;
        private TextBlock lblVisualMonitorsTitle;
        private TextBlock secLuminanceTitle;
        private TextBlock secColorTitle;
        private TextBlock secPresetsTitle;

        private TextBlock lblBrightness;
        private TextBlock lblContrast;
        private TextBlock lblGamma;
        private TextBlock lblColorTemp;
        private TextBlock lblRed;
        private TextBlock lblGreen;
        private TextBlock lblBlue;

        private TextBlock badgeBrightness;
        private TextBlock badgeContrast;
        private TextBlock badgeGamma;
        private TextBlock badgeColorTemp;
        private TextBlock badgeRed;
        private TextBlock badgeGreen;
        private TextBlock badgeBlue;

        private Slider sliderBrightness;
        private Slider sliderContrast;
        private Slider sliderGamma;
        private Slider sliderColorTemp;
        private Slider sliderRed;
        private Slider sliderGreen;
        private Slider sliderBlue;

        private Border colorPreviewBox;
        private StackPanel monitorCardsPanel;
        private List<Border> visualMonitorCards;

        // Presets Buttons
        private Button btnPresetDefault;
        private Button btnPresetNight;
        private Button btnPresetReading;
        private Button btnPresetCinema;
        private Button btnPresetReset;

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

        private Button btnApply;
        private Button btnOK;
        private Button btnCancel;
        private TextBlock lblSaveNotice;

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
            syncAllMonitors = config.SyncMonitors;

            DetectMonitors();
            overlayManager = new OverlayManager();

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
                dev.FriendlyName = "DisplayLink USB Device";

                // Query EnumDisplayDevices for specific model or string
                uint i = 0;
                while (true) {
                    NativeMethods.DISPLAY_DEVICE d = new NativeMethods.DISPLAY_DEVICE();
                    d.cb = System.Runtime.InteropServices.Marshal.SizeOf(d);
                    if (!NativeMethods.EnumDisplayDevices(null, i, ref d, 0)) break;
                    if (d.DeviceName == scr.DeviceName) {
                        if (!string.IsNullOrEmpty(d.DeviceString)) {
                            dev.FriendlyName = d.DeviceString;
                        }
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
                    settingsPerMonitor[dev.DeviceName] = new DisplaySettings();
                }
            }
        }

        private void InitializeComponent() {
            Title = "Lucent Display Control";
            Width = 620;
            Height = 780;
            MinWidth = 540;
            MinHeight = 680;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(20, 22, 28));
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI, Segoe UI Variable Display");

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Header & Nav
            root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Views

            // 1. TOP HEADER & NAVIGATION
            Grid topSection = CreateTopHeaderAndNav();
            Grid.SetRow(topSection, 0);
            root.Children.Add(topSection);

            // 2. MAIN VIEWS CONTAINER
            Grid viewsContainer = new Grid();

            viewDisplays = CreateDisplaysView();
            viewSettings = CreateSettingsView();
            viewSettings.Visibility = Visibility.Collapsed;

            viewsContainer.Children.Add(viewDisplays);
            viewsContainer.Children.Add(viewSettings);

            Grid.SetRow(viewsContainer, 1);
            root.Children.Add(viewsContainer);

            Content = root;
            Closing += MainWindow_Closing;

            UpdateAllTexts();
            UpdateVisualMonitorSelection();
            UpdateUiFromSettings();
        }

        #region Navigation & Header
        private Grid CreateTopHeaderAndNav() {
            Grid g = new Grid();
            g.Margin = new Thickness(24, 20, 24, 12);
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Brand Header Row
            Grid brandRow = new Grid();
            brandRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            brandRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Modern glowing sun badge
            Border badge = new Border();
            badge.Width = 44;
            badge.Height = 44;
            badge.CornerRadius = new CornerRadius(12);
            badge.Background = new LinearGradientBrush(
                Color.FromRgb(0, 120, 212),
                Color.FromRgb(99, 102, 241),
                new Point(0, 0),
                new Point(1, 1)
            );
            badge.Margin = new Thickness(0, 0, 14, 0);

            TextBlock iconTb = new TextBlock();
            iconTb.Text = "☀";
            iconTb.FontSize = 24;
            iconTb.FontWeight = FontWeights.Bold;
            iconTb.Foreground = Brushes.White;
            iconTb.HorizontalAlignment = HorizontalAlignment.Center;
            iconTb.VerticalAlignment = VerticalAlignment.Center;
            badge.Child = iconTb;
            Grid.SetColumn(badge, 0);
            brandRow.Children.Add(badge);

            StackPanel brandInfo = new StackPanel();
            brandInfo.VerticalAlignment = VerticalAlignment.Center;

            headerTitle = new TextBlock();
            headerTitle.Text = "Lucent Display Control";
            headerTitle.FontSize = 20;
            headerTitle.FontWeight = FontWeights.SemiBold;
            headerTitle.Foreground = Brushes.White;
            brandInfo.Children.Add(headerTitle);

            headerSubtitle = new TextBlock();
            headerSubtitle.Text = I18n.T("HeaderSubtitle", screenDevices.Count);
            headerSubtitle.FontSize = 12;
            headerSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            brandInfo.Children.Add(headerSubtitle);

            Grid.SetColumn(brandInfo, 1);
            brandRow.Children.Add(brandInfo);

            Grid.SetRow(brandRow, 0);
            g.Children.Add(brandRow);

            // Windows 11 Segmented Navigation Bar
            Border navBorder = new Border();
            navBorder.Margin = new Thickness(0, 16, 0, 0);
            navBorder.Background = new SolidColorBrush(Color.FromRgb(28, 31, 40));
            navBorder.CornerRadius = new CornerRadius(8);
            navBorder.Padding = new Thickness(4);

            UniformGrid navGrid = new UniformGrid();
            navGrid.Columns = 2;

            btnNavDisplays = CreateNavTabButton("🖥 " + I18n.T("NavDisplays"), true, delegate { SwitchTab(true); });
            btnNavSettings = CreateNavTabButton("⚙ " + I18n.T("NavSettings"), false, delegate { SwitchTab(false); });

            navGrid.Children.Add(btnNavDisplays);
            navGrid.Children.Add(btnNavSettings);
            navBorder.Child = navGrid;

            Grid.SetRow(navBorder, 1);
            g.Children.Add(navBorder);

            return g;
        }

        private Button CreateNavTabButton(string text, bool isActive, RoutedEventHandler click) {
            Button b = new Button();
            b.Content = text;
            b.Height = 36;
            b.FontSize = 13;
            b.FontWeight = FontWeights.SemiBold;
            b.Cursor = Cursors.Hand;
            b.BorderThickness = new Thickness(0);
            b.Click += click;
            ApplyNavStyle(b, isActive);
            return b;
        }

        private void ApplyNavStyle(Button b, bool isActive) {
            if (isActive) {
                b.Background = new SolidColorBrush(Color.FromRgb(46, 52, 70));
                b.Foreground = Brushes.White;
            } else {
                b.Background = Brushes.Transparent;
                b.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            }
        }

        private void SwitchTab(bool showDisplays) {
            if (showDisplays) {
                viewDisplays.Visibility = Visibility.Visible;
                viewSettings.Visibility = Visibility.Collapsed;
                ApplyNavStyle(btnNavDisplays, true);
                ApplyNavStyle(btnNavSettings, false);
            } else {
                viewDisplays.Visibility = Visibility.Collapsed;
                viewSettings.Visibility = Visibility.Visible;
                ApplyNavStyle(btnNavDisplays, false);
                ApplyNavStyle(btnNavSettings, true);
                lblSaveNotice.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Displays & Controls View
        private Grid CreateDisplaysView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 8, 24, 24);

            // Visual Windows 11 Monitor Layout Cards
            Border monSection = CreateCard();
            StackPanel monSp = new StackPanel();

            lblVisualMonitorsTitle = CreateSectionHeader("🖥 " + I18n.T("AllDisplays").ToUpper());
            monSp.Children.Add(lblVisualMonitorsTitle);

            ScrollViewer monScroll = new ScrollViewer();
            monScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            monScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            monScroll.Margin = new Thickness(0, 8, 0, 0);

            monitorCardsPanel = new StackPanel();
            monitorCardsPanel.Orientation = Orientation.Horizontal;
            visualMonitorCards = new List<Border>();

            // All Displays Card
            Border allCard = CreateVisualMonitorCard(-1, I18n.T("AllDisplays"), "Sincronizado", "Todas");
            visualMonitorCards.Add(allCard);
            monitorCardsPanel.Children.Add(allCard);

            // Individual monitor cards
            for (int i = 0; i < screenDevices.Count; i++) {
                ScreenDevice sd = screenDevices[i];
                string subtitle = string.Format("{0} • {1}", sd.ResolutionText, sd.DeviceName);
                Border monCard = CreateVisualMonitorCard(i, string.Format("{0} {1}", I18n.T("Monitor"), sd.Number), subtitle, sd.IsPrimary ? I18n.T("Primary") : I18n.T("Secondary"));
                visualMonitorCards.Add(monCard);
                monitorCardsPanel.Children.Add(monCard);
            }

            monScroll.Content = monitorCardsPanel;
            monSp.Children.Add(monScroll);
            monSection.Child = monSp;
            sp.Children.Add(monSection);

            // Luminance Section
            Border lumCard = CreateCard();
            StackPanel lumSp = new StackPanel();
            secLuminanceTitle = CreateSectionHeader("💡 " + I18n.T("SecLuminance"));
            lumSp.Children.Add(secLuminanceTitle);

            lumSp.Children.Add(CreateSliderRow(I18n.T("Brightness"), "100%", "#F59E0B", 10, 150, 100, out sliderBrightness, out badgeBrightness, out lblBrightness, delegate(double v) { return string.Format("{0}%", (int)v); }));
            lumSp.Children.Add(CreateSliderRow(I18n.T("Contrast"), "100%", "#0EA5E9", 20, 200, 100, out sliderContrast, out badgeContrast, out lblContrast, delegate(double v) { return string.Format("{0}%", (int)v); }));
            lumSp.Children.Add(CreateSliderRow(I18n.T("Gamma"), "1.00", "#A855F7", 40, 250, 100, out sliderGamma, out badgeGamma, out lblGamma, delegate(double v) { return string.Format("{0:0.00}", v / 100.0); }));
            lumCard.Child = lumSp;
            sp.Children.Add(lumCard);

            // Color Section
            Border colorCard = CreateCard();
            StackPanel colorSp = new StackPanel();
            secColorTitle = CreateSectionHeader("🎨 " + I18n.T("SecColor"));
            colorSp.Children.Add(secColorTitle);

            colorSp.Children.Add(CreateSliderRow(I18n.T("ColorTemp"), "6500 K (" + I18n.T("Neutral") + ")", "#FB923C", 2500, 9500, 6500, out sliderColorTemp, out badgeColorTemp, out lblColorTemp, delegate(double v) {
                int k = (int)v;
                if (k <= 4000) return string.Format("{0} K ({1})", k, I18n.T("Warm"));
                if (k >= 8000) return string.Format("{0} K ({1})", k, I18n.T("Cold"));
                if (Math.Abs(k - 6500) < 100) return string.Format("6500 K ({0})", I18n.T("Neutral"));
                return string.Format("{0} K", k);
            }));

            colorPreviewBox = new Border();
            colorPreviewBox.Height = 12;
            colorPreviewBox.CornerRadius = new CornerRadius(6);
            colorPreviewBox.Margin = new Thickness(0, 4, 0, 14);
            colorPreviewBox.Background = Brushes.White;
            colorSp.Children.Add(colorPreviewBox);

            colorSp.Children.Add(CreateSliderRow(I18n.T("Red"), "100%", "#EF4444", 0, 100, 100, out sliderRed, out badgeRed, out lblRed, delegate(double v) { return string.Format("{0}%", (int)v); }));
            colorSp.Children.Add(CreateSliderRow(I18n.T("Green"), "100%", "#10B981", 0, 100, 100, out sliderGreen, out badgeGreen, out lblGreen, delegate(double v) { return string.Format("{0}%", (int)v); }));
            colorSp.Children.Add(CreateSliderRow(I18n.T("Blue"), "100%", "#3B82F6", 0, 100, 100, out sliderBlue, out badgeBlue, out lblBlue, delegate(double v) { return string.Format("{0}%", (int)v); }));
            colorCard.Child = colorSp;
            sp.Children.Add(colorCard);

            // Presets Section
            Border presetCard = CreateCard();
            StackPanel presetSp = new StackPanel();
            secPresetsTitle = CreateSectionHeader("⚡ " + I18n.T("SecPresets"));
            presetSp.Children.Add(secPresetsTitle);

            WrapPanel wp = new WrapPanel();
            wp.Margin = new Thickness(0, 8, 0, 0);

            btnPresetDefault = CreatePresetButton(I18n.T("PresetDefault"), delegate { ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0); });
            btnPresetNight = CreatePresetButton(I18n.T("PresetNight"), delegate { ApplyPreset(0.65, 0.95, 1.05, 3400, 1.0, 0.90, 0.70); });
            btnPresetReading = CreatePresetButton(I18n.T("PresetReading"), delegate { ApplyPreset(0.80, 1.05, 1.10, 4500, 1.0, 0.96, 0.88); });
            btnPresetCinema = CreatePresetButton(I18n.T("PresetCinema"), delegate { ApplyPreset(1.05, 1.20, 0.95, 6500, 1.0, 1.0, 1.0); });
            btnPresetReset = CreatePresetButton(I18n.T("PresetReset"), delegate { ResetCurrentOrAll(); });

            wp.Children.Add(btnPresetDefault);
            wp.Children.Add(btnPresetNight);
            wp.Children.Add(btnPresetReading);
            wp.Children.Add(btnPresetCinema);
            wp.Children.Add(btnPresetReset);

            presetSp.Children.Add(wp);
            presetCard.Child = presetSp;
            sp.Children.Add(presetCard);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private Border CreateVisualMonitorCard(int targetIndex, string title, string subtitle, string badgeText) {
            Border b = new Border();
            b.Width = 160;
            b.Height = 84;
            b.CornerRadius = new CornerRadius(8);
            b.Margin = new Thickness(0, 0, 10, 0);
            b.Cursor = Cursors.Hand;
            b.Padding = new Thickness(10, 8, 10, 8);

            Grid cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            cardGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Header of card: Icon + Badge
            Grid header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock t1 = new TextBlock();
            t1.Text = title;
            t1.FontSize = 13;
            t1.FontWeight = FontWeights.Bold;
            t1.Foreground = Brushes.White;
            Grid.SetColumn(t1, 0);
            header.Children.Add(t1);

            Border pill = new Border();
            pill.CornerRadius = new CornerRadius(4);
            pill.Background = new SolidColorBrush(Color.FromRgb(40, 48, 68));
            pill.Padding = new Thickness(6, 2, 6, 2);

            TextBlock pillText = new TextBlock();
            pillText.Text = badgeText;
            pillText.FontSize = 10;
            pillText.FontWeight = FontWeights.SemiBold;
            pillText.Foreground = new SolidColorBrush(Color.FromRgb(96, 205, 255));
            pill.Child = pillText;
            Grid.SetColumn(pill, 1);
            header.Children.Add(pill);

            Grid.SetRow(header, 0);
            cardGrid.Children.Add(header);

            // Subtitle
            TextBlock subTb = new TextBlock();
            subTb.Text = subtitle;
            subTb.FontSize = 11;
            subTb.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            subTb.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(subTb, 1);
            cardGrid.Children.Add(subTb);

            b.Child = cardGrid;

            int capturedIdx = targetIndex;
            b.MouseLeftButtonUp += delegate {
                selectedMonitorIndex = capturedIdx;
                syncAllMonitors = (capturedIdx == -1);
                UpdateVisualMonitorSelection();
                UpdateUiFromSettings();
            };

            return b;
        }

        private void UpdateVisualMonitorSelection() {
            if (visualMonitorCards == null) return;
            for (int i = 0; i < visualMonitorCards.Count; i++) {
                Border b = visualMonitorCards[i];
                int cardIndex = i - 1; // 0 is All (-1), 1 is Mon 0, etc.
                bool isSelected = (cardIndex == selectedMonitorIndex);

                if (isSelected) {
                    b.Background = new SolidColorBrush(Color.FromRgb(38, 48, 70));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                    b.BorderThickness = new Thickness(2);
                } else {
                    b.Background = new SolidColorBrush(Color.FromRgb(26, 30, 40));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(44, 50, 68));
                    b.BorderThickness = new Thickness(1);
                }
            }
        }
        #endregion

        #region Settings View (Windows 11 Style)
        private Grid CreateSettingsView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 8, 24, 24);

            // Settings Header
            StackPanel headerSp = new StackPanel();
            headerSp.Margin = new Thickness(0, 0, 0, 16);

            txtSettingsTitle = new TextBlock();
            txtSettingsTitle.Text = I18n.T("SettingsTitle");
            txtSettingsTitle.FontSize = 22;
            txtSettingsTitle.FontWeight = FontWeights.SemiBold;
            txtSettingsTitle.Foreground = Brushes.White;
            headerSp.Children.Add(txtSettingsTitle);

            txtSettingsSubtitle = new TextBlock();
            txtSettingsSubtitle.Text = I18n.T("SettingsSubtitle");
            txtSettingsSubtitle.FontSize = 13;
            txtSettingsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            txtSettingsSubtitle.Margin = new Thickness(0, 2, 0, 0);
            headerSp.Children.Add(txtSettingsSubtitle);

            sp.Children.Add(headerSp);

            // 1. Language Card
            Border langCard = CreateCard();
            Grid langGrid = new Grid();
            langGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            langGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });

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
            txtLangDesc.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            txtLangDesc.TextWrapping = TextWrapping.Wrap;
            txtLangDesc.Margin = new Thickness(0, 4, 16, 0);
            langInfo.Children.Add(txtLangDesc);

            Grid.SetColumn(langInfo, 0);
            langGrid.Children.Add(langInfo);

            comboLanguage = new ComboBox();
            comboLanguage.Background = new SolidColorBrush(Color.FromRgb(36, 42, 58));
            comboLanguage.Foreground = Brushes.White;
            comboLanguage.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 70, 96));
            comboLanguage.Height = 36;
            comboLanguage.VerticalAlignment = VerticalAlignment.Center;
            comboLanguage.VerticalContentAlignment = VerticalAlignment.Center;
            comboLanguage.Padding = new Thickness(8, 4, 8, 4);

            foreach (string lName in I18n.LanguageNames) {
                comboLanguage.Items.Add(lName);
            }
            comboLanguage.SelectedIndex = (int)I18n.CurrentLanguage;

            // Change language IMMEDIATELY for live preview
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

            // 2. Startup Card (Windows 11 Card with subitem)
            Border startupCard = CreateCard();
            StackPanel startupSp = new StackPanel();

            // Main Start with Windows toggle
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
            txtStartWinDesc.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
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

            // Sub-item: Start Minimized to Tray (Greyed out when parent is unchecked!)
            subCardStartMinimized = new Border();
            subCardStartMinimized.Margin = new Thickness(0, 12, 0, 0);
            subCardStartMinimized.Padding = new Thickness(14, 10, 14, 10);
            subCardStartMinimized.Background = new SolidColorBrush(Color.FromRgb(32, 36, 48));
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
            txtStartMinDesc.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
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

            // Wire up greyed out logic
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

            // 3. Minimize to Tray on Window Close Card
            Border closeCard = CreateCard();
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
            txtMinCloseDesc.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            closeInfo.Children.Add(txtMinCloseDesc);

            Grid.SetColumn(closeInfo, 0);
            closeGrid.Children.Add(closeInfo);

            chkMinimizeOnClose = new CheckBox();
            chkMinimizeOnClose.IsChecked = config.MinimizeOnClose; // True by default
            chkMinimizeOnClose.VerticalAlignment = VerticalAlignment.Center;
            chkMinimizeOnClose.Cursor = Cursors.Hand;
            Grid.SetColumn(chkMinimizeOnClose, 1);
            closeGrid.Children.Add(chkMinimizeOnClose);

            closeCard.Child = closeGrid;
            sp.Children.Add(closeCard);

            // 4. About & Version Card (Version 0.1.0)
            Border aboutCard = CreateCard();
            Grid aboutGrid = new Grid();
            aboutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            aboutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border aboutIcon = new Border();
            aboutIcon.Width = 40;
            aboutIcon.Height = 40;
            aboutIcon.CornerRadius = new CornerRadius(8);
            aboutIcon.Background = new SolidColorBrush(Color.FromRgb(40, 48, 68));
            aboutIcon.Margin = new Thickness(0, 0, 14, 0);

            TextBlock iTb = new TextBlock();
            iTb.Text = "ℹ";
            iTb.FontSize = 20;
            iTb.Foreground = new SolidColorBrush(Color.FromRgb(96, 205, 255));
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
            txtAboutVersion.Foreground = new SolidColorBrush(Color.FromRgb(96, 205, 255));
            txtAboutVersion.Margin = new Thickness(0, 2, 0, 2);
            aboutInfo.Children.Add(txtAboutVersion);

            txtAboutDesc = new TextBlock();
            txtAboutDesc.Text = I18n.T("AboutDesc");
            txtAboutDesc.FontSize = 11;
            txtAboutDesc.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            aboutInfo.Children.Add(txtAboutDesc);

            Grid.SetColumn(aboutInfo, 1);
            aboutGrid.Children.Add(aboutInfo);
            aboutCard.Child = aboutGrid;
            sp.Children.Add(aboutCard);

            // 5. Action Buttons Bar (Apply, OK, Cancel)
            Grid actionBar = new Grid();
            actionBar.Margin = new Thickness(0, 10, 0, 0);
            actionBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            actionBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            lblSaveNotice = new TextBlock();
            lblSaveNotice.Text = "✔ " + I18n.T("SavedNotice");
            lblSaveNotice.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
            lblSaveNotice.FontSize = 13;
            lblSaveNotice.FontWeight = FontWeights.SemiBold;
            lblSaveNotice.VerticalAlignment = VerticalAlignment.Center;
            lblSaveNotice.Visibility = Visibility.Collapsed;
            Grid.SetColumn(lblSaveNotice, 0);
            actionBar.Children.Add(lblSaveNotice);

            StackPanel btnRow = new StackPanel();
            btnRow.Orientation = Orientation.Horizontal;

            btnCancel = CreateModernButton(I18n.T("BtnCancel"), false, delegate {
                // Cancel: Revert to committed language and settings
                I18n.CurrentLanguage = I18n.CommittedLanguage;
                comboLanguage.SelectedIndex = (int)I18n.CommittedLanguage;
                chkStartWithWindows.IsChecked = config.StartWithWindows;
                chkStartMinimized.IsChecked = config.StartMinimized;
                chkMinimizeOnClose.IsChecked = config.MinimizeOnClose;
                UpdateAllTexts();
                SwitchTab(true);
            });

            btnApply = CreateModernButton(I18n.T("BtnApply"), true, delegate {
                SaveSettingsState();
                lblSaveNotice.Text = "✔ " + I18n.T("SavedNotice");
                lblSaveNotice.Visibility = Visibility.Visible;
            });

            btnOK = CreateModernButton(I18n.T("BtnOK"), true, delegate {
                SaveSettingsState();
                SwitchTab(true);
            });

            btnRow.Children.Add(btnCancel);
            btnRow.Children.Add(btnApply);
            btnRow.Children.Add(btnOK);

            Grid.SetColumn(btnRow, 1);
            actionBar.Children.Add(btnRow);
            sp.Children.Add(actionBar);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
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

        private Button CreateModernButton(string text, bool isAccent, RoutedEventHandler click) {
            Button b = new Button();
            b.Content = text;
            b.Height = 34;
            b.MinWidth = 85;
            b.Padding = new Thickness(14, 4, 14, 4);
            b.Margin = new Thickness(6, 0, 0, 0);
            b.FontSize = 13;
            b.FontWeight = FontWeights.SemiBold;
            b.Cursor = Cursors.Hand;
            b.BorderThickness = new Thickness(1);

            if (isAccent) {
                b.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                b.Foreground = Brushes.White;
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(20, 140, 240));
            } else {
                b.Background = new SolidColorBrush(Color.FromRgb(36, 42, 56));
                b.Foreground = Brushes.White;
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 64, 88));
            }
            b.Click += click;
            return b;
        }
        #endregion

        #region Helpers & UI Updating
        public void UpdateAllTexts() {
            if (headerTitle == null) return;

            btnNavDisplays.Content = "🖥 " + I18n.T("NavDisplays");
            btnNavSettings.Content = "⚙ " + I18n.T("NavSettings");
            headerSubtitle.Text = I18n.T("HeaderSubtitle", screenDevices.Count);

            lblVisualMonitorsTitle.Text = "🖥 " + I18n.T("AllDisplays").ToUpper();
            secLuminanceTitle.Text = "💡 " + I18n.T("SecLuminance");
            secColorTitle.Text = "🎨 " + I18n.T("SecColor");
            secPresetsTitle.Text = "⚡ " + I18n.T("SecPresets");

            lblBrightness.Text = I18n.T("Brightness");
            lblContrast.Text = I18n.T("Contrast");
            lblGamma.Text = I18n.T("Gamma");
            lblColorTemp.Text = I18n.T("ColorTemp");
            lblRed.Text = I18n.T("Red");
            lblGreen.Text = I18n.T("Green");
            lblBlue.Text = I18n.T("Blue");

            btnPresetDefault.Content = I18n.T("PresetDefault");
            btnPresetNight.Content = I18n.T("PresetNight");
            btnPresetReading.Content = I18n.T("PresetReading");
            btnPresetCinema.Content = I18n.T("PresetCinema");
            btnPresetReset.Content = I18n.T("PresetReset");

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

            btnApply.Content = I18n.T("BtnApply");
            btnOK.Content = I18n.T("BtnOK");
            btnCancel.Content = I18n.T("BtnCancel");

            UpdateColorTempBadge();
            UpdateTrayMenu();
        }

        private void UpdateColorTempBadge() {
            if (badgeColorTemp == null || sliderColorTemp == null) return;
            int k = (int)sliderColorTemp.Value;
            if (k <= 4000) badgeColorTemp.Text = string.Format("{0} K ({1})", k, I18n.T("Warm"));
            else if (k >= 8000) badgeColorTemp.Text = string.Format("{0} K ({1})", k, I18n.T("Cold"));
            else if (Math.Abs(k - 6500) < 100) badgeColorTemp.Text = string.Format("6500 K ({0})", I18n.T("Neutral"));
            else badgeColorTemp.Text = string.Format("{0} K", k);
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
            badgeGamma.Text = string.Format("{0:0.00}", sliderGamma.Value / 100.0);
            UpdateColorTempBadge();
            badgeRed.Text = string.Format("{0}%", (int)sliderRed.Value);
            badgeGreen.Text = string.Format("{0}%", (int)sliderGreen.Value);
            badgeBlue.Text = string.Format("{0}%", (int)sliderBlue.Value);

            UpdateColorPreview();
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

            UpdateColorPreview();
            ApplyAllActiveSettings();

            // Save values to config
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
            if (syncAllMonitors || selectedMonitorIndex == -1) {
                // Apply overlay dimming to all
                overlayManager.UpdateAll(globalSettings);

                // Apply gamma ramp to all
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

        private void UpdateColorPreview() {
            DisplaySettings s = GetCurrentTargetSettings();
            double tr, tg, tb;
            GammaMath.CalculateColorTemp(s.Temperature, out tr, out tg, out tb);

            byte r = (byte)Math.Min(255, Math.Max(0, (int)Math.Round(s.Red * tr * 255.0)));
            byte g = (byte)Math.Min(255, Math.Max(0, (int)Math.Round(s.Green * tg * 255.0)));
            byte b = (byte)Math.Min(255, Math.Max(0, (int)Math.Round(s.Blue * tb * 255.0)));

            colorPreviewBox.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        private void ApplyPreset(double b, double c, double g, double temp, double red, double green, double blue) {
            DisplaySettings s = GetCurrentTargetSettings();
            s.Brightness = b;
            s.Contrast = c;
            s.Gamma = g;
            s.Temperature = temp;
            s.Red = red;
            s.Green = green;
            s.Blue = blue;

            UpdateUiFromSettings();
            OnControlChanged();
        }

        private void ResetCurrentOrAll() {
            if (syncAllMonitors || selectedMonitorIndex == -1) {
                globalSettings.ResetToDefaults();
                foreach (var kvp in settingsPerMonitor) {
                    kvp.Value.ResetToDefaults();
                }
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

        private Border CreateCard() {
            Border b = new Border();
            b.Background = new SolidColorBrush(Color.FromRgb(26, 30, 40));
            b.CornerRadius = new CornerRadius(8);
            b.Padding = new Thickness(16, 14, 16, 14);
            b.Margin = new Thickness(0, 0, 0, 12);
            b.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 46, 62));
            b.BorderThickness = new Thickness(1);
            return b;
        }

        private TextBlock CreateSectionHeader(string text) {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.FontSize = 12;
            tb.FontWeight = FontWeights.Bold;
            tb.Foreground = new SolidColorBrush(Color.FromRgb(96, 205, 255));
            tb.Margin = new Thickness(0, 0, 0, 8);
            return tb;
        }

        private Grid CreateSliderRow(string label, string initialValue, string accentHex, double min, double max, double initial,
                                     out Slider slider, out TextBlock badge, out TextBlock lbl, Func<double, string> formatVal) {
            Grid g = new Grid();
            g.Margin = new Thickness(0, 4, 0, 8);
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            lbl = new TextBlock();
            lbl.Text = label;
            lbl.FontSize = 13;
            lbl.Foreground = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            Grid.SetColumn(lbl, 0);
            header.Children.Add(lbl);

            badge = new TextBlock();
            badge.Text = initialValue;
            badge.FontSize = 12;
            badge.FontWeight = FontWeights.SemiBold;
            badge.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(accentHex);
            Grid.SetColumn(badge, 1);
            header.Children.Add(badge);

            Grid.SetRow(header, 0);
            g.Children.Add(header);

            slider = new Slider();
            slider.Minimum = min;
            slider.Maximum = max;
            slider.Value = initial;
            slider.Margin = new Thickness(0, 4, 0, 0);
            slider.Height = 22;

            TextBlock capturedBadge = badge;
            slider.ValueChanged += delegate(object s, RoutedPropertyChangedEventArgs<double> e) {
                capturedBadge.Text = formatVal(e.NewValue);
                if (!isUpdatingUi) {
                    OnControlChanged();
                }
            };

            Grid.SetRow(slider, 1);
            g.Children.Add(slider);

            return g;
        }

        private Button CreatePresetButton(string text, RoutedEventHandler onClick) {
            Button btn = new Button();
            btn.Content = text;
            btn.Background = new SolidColorBrush(Color.FromRgb(36, 42, 58));
            btn.Foreground = Brushes.White;
            btn.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 68, 92));
            btn.BorderThickness = new Thickness(1);
            btn.Padding = new Thickness(12, 6, 12, 6);
            btn.Margin = new Thickness(0, 0, 8, 8);
            btn.FontSize = 12;
            btn.Cursor = Cursors.Hand;
            btn.Click += onClick;
            return btn;
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
            menu.MenuItems.Add(I18n.T("TraySettings"), delegate { RestoreWindow(); SwitchTab(false); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(I18n.T("PresetDefault"), delegate { ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0); });
            menu.MenuItems.Add(I18n.T("PresetNight"), delegate { ApplyPreset(0.65, 0.95, 1.05, 3400, 1.0, 0.90, 0.70); });
            menu.MenuItems.Add(I18n.T("PresetReading"), delegate { ApplyPreset(0.80, 1.05, 1.10, 4500, 1.0, 0.96, 0.88); });
            menu.MenuItems.Add(I18n.T("PresetCinema"), delegate { ApplyPreset(1.05, 1.20, 0.95, 6500, 1.0, 1.0, 1.0); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(I18n.T("PresetReset"), delegate { ResetCurrentOrAll(); });
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
                            SD.Color.FromArgb(0, 120, 212), SD.Color.FromArgb(99, 102, 241))) {
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
            if (overlayManager != null) {
                overlayManager.CloseAll();
            }
            Application.Current.Shutdown();
        }
        #endregion
    }
}
