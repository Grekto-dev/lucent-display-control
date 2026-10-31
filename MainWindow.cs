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
using System.Windows.Threading;
using SD = System.Drawing;
using G2D = System.Drawing.Drawing2D;
using SWF = System.Windows.Forms;

namespace LucentControl {
    public class MainWindow : Window {
        private AppConfig config;
        private ProfileManager profileManager;
        private List<ScreenDevice> screenDevices;
        private Dictionary<string, DisplaySettings> settingsPerMonitor;
        private DisplaySettings globalSettings;
        private bool syncAllMonitors = false;
        private int selectedMonitorIndex = 0;

        private OverlayManager overlayManager;
        private SWF.NotifyIcon trayIcon;
        private DispatcherTimer schedulerTimer;
        private string lastAppliedScheduleSlot = "";

        // Custom Styles
        private Style darkButtonStyle;
        private Style darkPrimaryButtonStyle;
        private Style darkComboBoxStyle;
        private Style darkTextBoxStyle;
        private Style sliderStyleBlue;
        private Style sliderStyleTemp;
        private Style sliderStyleRed;
        private Style sliderStyleGreen;
        private Style sliderStyleBlueChannel;

        // Navigation Elements
        private Button btnNavDashboard;
        private Button btnNavMonitors;
        private Button btnNavPresets;
        private Button btnNavProfiles;
        private Button btnNavScheduler;
        private Button btnNavSettings;
        private List<Button> navButtons;

        // Views
        private Grid viewDashboard;
        private Grid viewMonitors;
        private Grid viewPresets;
        private Grid viewProfiles;
        private Grid viewScheduler;
        private Grid viewSettings;
        private List<Grid> allViews;

        // Dashboard Elements
        private StackPanel monitorCardsRow;
        private List<Border> monitorCardBorders;
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
        private Button btnIdentify;
        private Button btnMinimizeToTray;

        // Monitors View Elements
        private StackPanel monitorsHardwareContainer;
        private TextBlock txtMonHardwareTitle;
        private TextBlock txtMonHardwareSubtitle;
        private TextBlock txtCalibrationTitle;
        private TextBlock txtCalibrationDesc;
        private Button btnRefreshDisplays;
        private Button btnIdentifyFromMon;
        private Button btnCalibWhite;
        private Button btnCalibBlack;
        private Button btnCalibGray;
        private Button btnCalibColor;

        // Presets View Elements
        private TextBlock txtPresetsTitle;
        private TextBlock txtPresetsSubtitle;
        private UniformGrid presetsGrid;
        private TextBox txtCustomPresetName;
        private Button btnSaveCustomPreset;
        private TextBlock lblCustomPresetNotice;

        // Profiles View Elements
        private TextBlock txtProfilesTitle;
        private TextBlock txtProfilesSubtitle;
        private TextBlock lblActiveProfileBanner;
        private StackPanel profilesListContainer;
        private TextBox txtNewProfileName;
        private Button btnCreateProfile;
        private TextBlock lblProfileNotice;

        // Scheduler View Elements
        private TextBlock txtSchedulerTitle;
        private TextBlock txtSchedulerSubtitle;
        private CheckBox chkEnableScheduler;
        private TextBlock lblSchedulerStatus;
        private ComboBox comboDayHour;
        private ComboBox comboDayMinute;
        private ComboBox comboDayProfile;
        private ComboBox comboSunsetHour;
        private ComboBox comboSunsetMinute;
        private ComboBox comboSunsetProfile;
        private ComboBox comboNightHour;
        private ComboBox comboNightMinute;
        private ComboBox comboNightProfile;
        private CheckBox chkSchedulerNotify;
        private Button btnSaveScheduler;
        private TextBlock lblSchedulerSaveNotice;

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

        // Dynamic Header Elements
        private TextBlock headerTitle;
        private TextBlock headerStatus;
        private TextBlock sidebarMonitorsCount;
        private TextBlock mainTitle;
        private TextBlock mainSubtitle;
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
            profileManager = new ProfileManager();
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
            InitSchedulerTimer();

            Loaded += delegate {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.EnableDarkMode(hwnd);
                ApplyAllActiveSettings();

                if (config.SchedulerEnabled) {
                    CheckSchedulerTick();
                }

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
                    
                    <!-- Dark Button Style -->
                    <Style x:Key='DarkButton' TargetType='Button'>
                        <Setter Property='Background' Value='#FF1E2536'/>
                        <Setter Property='Foreground' Value='#FFF8FAFC'/>
                        <Setter Property='BorderBrush' Value='#FF334155'/>
                        <Setter Property='BorderThickness' Value='1'/>
                        <Setter Property='Padding' Value='12,6'/>
                        <Setter Property='Cursor' Value='Hand'/>
                        <Setter Property='FontSize' Value='12'/>
                        <Setter Property='FontWeight' Value='Medium'/>
                        <Setter Property='Template'>
                            <Setter.Value>
                                <ControlTemplate TargetType='Button'>
                                    <Border x:Name='border' CornerRadius='6' 
                                            Background='{TemplateBinding Background}' 
                                            BorderBrush='{TemplateBinding BorderBrush}' 
                                            BorderThickness='{TemplateBinding BorderThickness}' 
                                            SnapsToDevicePixels='True'>
                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center' 
                                                          Margin='{TemplateBinding Padding}' 
                                                          RecognizesAccessKey='True'
                                                          TextBlock.Foreground='{TemplateBinding Foreground}'/>
                                    </Border>
                                    <ControlTemplate.Triggers>
                                        <Trigger Property='IsMouseOver' Value='True'>
                                            <Setter TargetName='border' Property='Background' Value='#FF2A364F'/>
                                            <Setter TargetName='border' Property='BorderBrush' Value='#FF475569'/>
                                        </Trigger>
                                        <Trigger Property='IsPressed' Value='True'>
                                            <Setter TargetName='border' Property='Background' Value='#FF161D2B'/>
                                        </Trigger>
                                        <Trigger Property='IsEnabled' Value='False'>
                                            <Setter TargetName='border' Property='Opacity' Value='0.5'/>
                                        </Trigger>
                                    </ControlTemplate.Triggers>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>

                    <!-- Dark Primary Accent Button -->
                    <Style x:Key='DarkPrimaryButton' TargetType='Button'>
                        <Setter Property='Background' Value='#FF0078D4'/>
                        <Setter Property='Foreground' Value='#FFFFFFFF'/>
                        <Setter Property='BorderBrush' Value='#FF0078D4'/>
                        <Setter Property='BorderThickness' Value='1'/>
                        <Setter Property='Padding' Value='16,8'/>
                        <Setter Property='Cursor' Value='Hand'/>
                        <Setter Property='FontSize' Value='13'/>
                        <Setter Property='FontWeight' Value='SemiBold'/>
                        <Setter Property='Template'>
                            <Setter.Value>
                                <ControlTemplate TargetType='Button'>
                                    <Border x:Name='border' CornerRadius='6' 
                                            Background='{TemplateBinding Background}' 
                                            BorderBrush='{TemplateBinding BorderBrush}' 
                                            BorderThickness='{TemplateBinding BorderThickness}' 
                                            SnapsToDevicePixels='True'>
                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center' 
                                                          Margin='{TemplateBinding Padding}' 
                                                          RecognizesAccessKey='True'
                                                          TextBlock.Foreground='{TemplateBinding Foreground}'/>
                                    </Border>
                                    <ControlTemplate.Triggers>
                                        <Trigger Property='IsMouseOver' Value='True'>
                                            <Setter TargetName='border' Property='Background' Value='#FF1084D9'/>
                                            <Setter TargetName='border' Property='BorderBrush' Value='#FF38BDF8'/>
                                        </Trigger>
                                        <Trigger Property='IsPressed' Value='True'>
                                            <Setter TargetName='border' Property='Background' Value='#FF006CBE'/>
                                        </Trigger>
                                        <Trigger Property='IsEnabled' Value='False'>
                                            <Setter TargetName='border' Property='Opacity' Value='0.5'/>
                                        </Trigger>
                                    </ControlTemplate.Triggers>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>

                    <!-- Dark ComboBox Style -->
                    <ControlTemplate x:Key='DarkComboBoxToggleButton' TargetType='ToggleButton'>
                        <Border x:Name='border' Background='#FF1E2536' BorderBrush='#FF334155' BorderThickness='1' CornerRadius='6' SnapsToDevicePixels='True'>
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width='*'/>
                                    <ColumnDefinition Width='24'/>
                                </Grid.ColumnDefinitions>
                                <Path Grid.Column='1' HorizontalAlignment='Center' VerticalAlignment='Center' Data='M 0 0 L 4 4 L 8 0 Z' Fill='#FF94A3B8'/>
                            </Grid>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsMouseOver' Value='True'>
                                <Setter TargetName='border' Property='BorderBrush' Value='#FF38BDF8'/>
                                <Setter TargetName='border' Property='Background' Value='#FF263044'/>
                            </Trigger>
                            <Trigger Property='IsChecked' Value='True'>
                                <Setter TargetName='border' Property='BorderBrush' Value='#FF38BDF8'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>

                    <Style x:Key='DarkComboBox' TargetType='ComboBox'>
                        <Setter Property='Foreground' Value='#FFF8FAFC'/>
                        <Setter Property='Background' Value='#FF1E2536'/>
                        <Setter Property='BorderBrush' Value='#FF334155'/>
                        <Setter Property='BorderThickness' Value='1'/>
                        <Setter Property='FontSize' Value='12'/>
                        <Setter Property='Padding' Value='10,4,10,4'/>
                        <Setter Property='SnapsToDevicePixels' Value='True'/>
                        <Setter Property='ItemContainerStyle'>
                            <Setter.Value>
                                <Style TargetType='ComboBoxItem'>
                                    <Setter Property='Foreground' Value='#FFF8FAFC'/>
                                    <Setter Property='Background' Value='#FF1E2536'/>
                                    <Setter Property='Padding' Value='8,6'/>
                                    <Setter Property='BorderThickness' Value='0'/>
                                    <Setter Property='Template'>
                                        <Setter.Value>
                                            <ControlTemplate TargetType='ComboBoxItem'>
                                                <Border x:Name='itemBorder' Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}' CornerRadius='4' Margin='2'>
                                                    <ContentPresenter Content='{TemplateBinding Content}' ContentTemplate='{TemplateBinding ContentTemplate}' VerticalAlignment='Center'/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter TargetName='itemBorder' Property='Background' Value='#FF2E3E5B'/>
                                                    </Trigger>
                                                    <Trigger Property='IsSelected' Value='True'>
                                                        <Setter TargetName='itemBorder' Property='Background' Value='#FF1E40AF'/>
                                                        <Setter Property='Foreground' Value='#FFFFFFFF'/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Setter.Value>
                        </Setter>
                        <Setter Property='Template'>
                            <Setter.Value>
                                <ControlTemplate TargetType='ComboBox'>
                                    <Grid>
                                        <ToggleButton Name='ToggleButton' Template='{StaticResource DarkComboBoxToggleButton}' Focusable='false'
                                                      IsChecked='{Binding Path=IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}'
                                                      ClickMode='Press'/>
                                        <ContentPresenter Name='ContentSite' IsHitTestVisible='False' Content='{TemplateBinding SelectionBoxItem}'
                                                          ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}'
                                                          ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'
                                                          Margin='10,4,26,4' VerticalAlignment='Center' HorizontalAlignment='Left'/>
                                        <Popup Name='PART_Popup' Placement='Bottom' IsOpen='{TemplateBinding IsDropDownOpen}'
                                               AllowsTransparency='True' Focusable='False' PopupAnimation='Slide'>
                                            <Border x:Name='DropDownBorder' Background='#FF161B26' BorderBrush='#FF334155' BorderThickness='1'
                                                    CornerRadius='6' Margin='0,4,0,0' MinWidth='{TemplateBinding ActualWidth}' MaxHeight='220'>
                                                <ScrollViewer Margin='2' SnapsToDevicePixels='True'>
                                                    <StackPanel IsItemsHost='True' KeyboardNavigation.DirectionalNavigation='Contained'/>
                                                </ScrollViewer>
                                            </Border>
                                        </Popup>
                                    </Grid>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>

                    <!-- Dark TextBox Style -->
                    <Style x:Key='DarkTextBox' TargetType='TextBox'>
                        <Setter Property='Background' Value='#FF1E2536'/>
                        <Setter Property='Foreground' Value='#FFF8FAFC'/>
                        <Setter Property='CaretBrush' Value='#FFF8FAFC'/>
                        <Setter Property='BorderBrush' Value='#FF334155'/>
                        <Setter Property='BorderThickness' Value='1'/>
                        <Setter Property='Padding' Value='10,6'/>
                        <Setter Property='FontSize' Value='13'/>
                        <Setter Property='Template'>
                            <Setter.Value>
                                <ControlTemplate TargetType='TextBox'>
                                    <Border x:Name='border' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='6' SnapsToDevicePixels='True'>
                                        <ScrollViewer x:Name='PART_ContentHost' Margin='0' SnapsToDevicePixels='True'/>
                                    </Border>
                                    <ControlTemplate.Triggers>
                                        <Trigger Property='IsFocused' Value='True'>
                                            <Setter TargetName='border' Property='BorderBrush' Value='#FF38BDF8'/>
                                        </Trigger>
                                    </ControlTemplate.Triggers>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>

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
                Resources.MergedDictionaries.Add(dict);

                darkButtonStyle = (Style)dict["DarkButton"];
                darkPrimaryButtonStyle = (Style)dict["DarkPrimaryButton"];
                darkComboBoxStyle = (Style)dict["DarkComboBox"];
                darkTextBoxStyle = (Style)dict["DarkTextBox"];

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
            Width = 1140;
            Height = 740;
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

            // 1. Custom Title Bar
            Grid titleBar = CreateTitleBar();
            Grid.SetRow(titleBar, 0);
            root.Children.Add(titleBar);

            // 2. Main Body: Left Sidebar + Right Content Area
            Grid mainBody = new Grid();
            mainBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) }); // Left Sidebar
            mainBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Content Area

            Border sidebar = CreateSidebar();
            Grid.SetColumn(sidebar, 0);
            mainBody.Children.Add(sidebar);

            Grid contentArea = new Grid();
            viewDashboard = CreateDashboardView();
            viewMonitors = CreateMonitorsView();
            viewPresets = CreatePresetsView();
            viewProfiles = CreateProfilesView();
            viewScheduler = CreateSchedulerView();
            viewSettings = CreateSettingsView();

            allViews = new List<Grid>() {
                viewDashboard,
                viewMonitors,
                viewPresets,
                viewProfiles,
                viewScheduler,
                viewSettings
            };

            for (int i = 0; i < allViews.Count; i++) {
                allViews[i].Visibility = (i == 0) ? Visibility.Visible : Visibility.Collapsed;
                contentArea.Children.Add(allViews[i]);
            }

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

            Border logoBorder = new Border();
            logoBorder.Width = 24;
            logoBorder.Height = 24;
            logoBorder.CornerRadius = new CornerRadius(6);
            LinearGradientBrush lgb = new LinearGradientBrush();
            lgb.StartPoint = new Point(0, 0);
            lgb.EndPoint = new Point(1, 1);
            lgb.GradientStops.Add(new GradientStop(Color.FromRgb(56, 189, 248), 0.0));
            lgb.GradientStops.Add(new GradientStop(Color.FromRgb(99, 102, 241), 1.0));
            logoBorder.Background = lgb;
            logoBorder.Margin = new Thickness(0, 0, 10, 0);

            Canvas logoCanvas = new Canvas();
            logoCanvas.Width = 14;
            logoCanvas.Height = 14;
            logoCanvas.Margin = new Thickness(5, 5, 0, 0);
            Path p1 = new Path();
            p1.Data = Geometry.Parse("M 0 7 L 7 0 L 14 7 L 7 14 Z");
            p1.Fill = Brushes.White;
            logoCanvas.Children.Add(p1);
            logoBorder.Child = logoCanvas;
            leftSp.Children.Add(logoBorder);

            headerTitle = new TextBlock();
            headerTitle.Text = "Lucent Display Control";
            headerTitle.FontSize = 13;
            headerTitle.FontWeight = FontWeights.SemiBold;
            headerTitle.Foreground = Brushes.White;
            headerTitle.VerticalAlignment = VerticalAlignment.Center;
            leftSp.Children.Add(headerTitle);

            Grid.SetColumn(leftSp, 0);
            tb.Children.Add(leftSp);

            // Center Status: Green dot + Running in system tray
            StackPanel centerSp = new StackPanel();
            centerSp.Orientation = Orientation.Horizontal;
            centerSp.HorizontalAlignment = HorizontalAlignment.Right;
            centerSp.VerticalAlignment = VerticalAlignment.Center;
            centerSp.Margin = new Thickness(0, 0, 24, 0);

            Ellipse dot = new Ellipse();
            dot.Width = 7;
            dot.Height = 7;
            dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            dot.Margin = new Thickness(0, 0, 8, 0);
            centerSp.Children.Add(dot);

            headerStatus = new TextBlock();
            headerStatus.Text = I18n.T("RunningInTray");
            headerStatus.FontSize = 12;
            headerStatus.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            centerSp.Children.Add(headerStatus);

            Grid.SetColumn(centerSp, 1);
            tb.Children.Add(centerSp);

            // Right: Standard Window Chrome Buttons
            StackPanel rightSp = new StackPanel();
            rightSp.Orientation = Orientation.Horizontal;

            Button btnMin = CreateTitleBarBtn("🗕", delegate { WindowState = WindowState.Minimized; });
            Button btnMax = CreateTitleBarBtn("🗖", delegate {
                WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            });
            Button btnClose = CreateTitleBarBtn("✕", delegate { Close(); });
            btnClose.MouseEnter += delegate { btnClose.Background = new SolidColorBrush(Color.FromRgb(225, 29, 72)); };
            btnClose.MouseLeave += delegate { btnClose.Background = Brushes.Transparent; };

            rightSp.Children.Add(btnMin);
            rightSp.Children.Add(btnMax);
            rightSp.Children.Add(btnClose);

            Grid.SetColumn(rightSp, 2);
            tb.Children.Add(rightSp);
            return tb;
        }

        private Button CreateTitleBarBtn(string glyph, RoutedEventHandler handler) {
            Button b = new Button();
            b.Width = 46;
            b.Height = 48;
            b.Background = Brushes.Transparent;
            b.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
            b.BorderThickness = new Thickness(0);
            b.Content = glyph;
            b.FontSize = 11;
            b.Cursor = Cursors.Hand;
            b.Click += handler;
            return b;
        }

        private Border CreateSidebar() {
            Border b = new Border();
            b.Background = new SolidColorBrush(Color.FromRgb(10, 12, 18));
            b.BorderBrush = new SolidColorBrush(Color.FromRgb(26, 32, 46));
            b.BorderThickness = new Thickness(0, 0, 1, 0);
            b.Padding = new Thickness(10, 14, 10, 16);

            Grid sbGrid = new Grid();
            sbGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            sbGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Navigation List
            StackPanel navList = new StackPanel();

            btnNavDashboard = CreateSidebarBtn("\uE80F", I18n.T("NavDashboard"), true, delegate { SwitchView(0); });
            btnNavMonitors = CreateSidebarBtn("\uE7F4", I18n.T("NavMonitors"), false, delegate { SwitchView(1); });
            btnNavPresets = CreateSidebarBtn("\uE74C", I18n.T("NavPresets"), false, delegate { SwitchView(2); });
            btnNavProfiles = CreateSidebarBtn("\uE77B", I18n.T("NavProfiles"), false, delegate { SwitchView(3); });
            btnNavScheduler = CreateSidebarBtn("\uE823", I18n.T("NavScheduler"), false, delegate { SwitchView(4); });
            btnNavSettings = CreateSidebarBtn("\uE713", I18n.T("NavSettings"), false, delegate { SwitchView(5); });

            navButtons = new List<Button>() {
                btnNavDashboard,
                btnNavMonitors,
                btnNavPresets,
                btnNavProfiles,
                btnNavScheduler,
                btnNavSettings
            };

            foreach (Button nb in navButtons) {
                navList.Children.Add(nb);
            }

            Grid.SetRow(navList, 0);
            sbGrid.Children.Add(navList);

            // Bottom Info
            StackPanel foot = new StackPanel();
            StackPanel statusRow = new StackPanel();
            statusRow.Orientation = Orientation.Horizontal;
            statusRow.Margin = new Thickness(6, 0, 0, 4);

            Ellipse dot = new Ellipse();
            dot.Width = 6;
            dot.Height = 6;
            dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            dot.Margin = new Thickness(0, 0, 8, 0);
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
            verTb.Margin = new Thickness(6, 0, 0, 0);
            foot.Children.Add(verTb);

            Grid.SetRow(foot, 1);
            sbGrid.Children.Add(foot);

            b.Child = sbGrid;
            return b;
        }

        private Button CreateSidebarBtn(string icon, string label, bool isSelected, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 40;
            b.Margin = new Thickness(0, 0, 0, 4);
            b.BorderThickness = new Thickness(0);
            b.Cursor = Cursors.Hand;
            b.HorizontalContentAlignment = HorizontalAlignment.Left;
            b.Padding = new Thickness(12, 0, 12, 0);

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock iTb = new TextBlock();
            iTb.Text = icon;
            iTb.FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
            iTb.FontSize = 15;
            iTb.Width = 24;
            iTb.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(iTb);

            TextBlock lTb = new TextBlock();
            lTb.Text = label;
            lTb.FontSize = 13;
            lTb.FontWeight = FontWeights.Medium;
            lTb.Margin = new Thickness(8, 0, 0, 0);
            lTb.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(lTb);

            b.Content = sp;
            b.Click += click;
            ApplySidebarBtnStyle(b, isSelected);
            return b;
        }

        private void ApplySidebarBtnStyle(Button b, bool isSelected) {
            StackPanel sp = b.Content as StackPanel;
            if (isSelected) {
                b.Background = new SolidColorBrush(Color.FromRgb(26, 36, 54));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                b.BorderThickness = new Thickness(3, 0, 0, 0);
                if (sp != null && sp.Children.Count >= 2) {
                    ((TextBlock)sp.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    ((TextBlock)sp.Children[1]).Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                    ((TextBlock)sp.Children[1]).FontWeight = FontWeights.SemiBold;
                }
            } else {
                b.Background = Brushes.Transparent;
                b.BorderThickness = new Thickness(0);
                if (sp != null && sp.Children.Count >= 2) {
                    ((TextBlock)sp.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                    ((TextBlock)sp.Children[1]).Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                    ((TextBlock)sp.Children[1]).FontWeight = FontWeights.Medium;
                }
            }
        }

        private void SwitchView(int index) {
            for (int i = 0; i < allViews.Count; i++) {
                allViews[i].Visibility = (i == index) ? Visibility.Visible : Visibility.Collapsed;
            }
            for (int i = 0; i < navButtons.Count; i++) {
                ApplySidebarBtnStyle(navButtons[i], i == index);
            }

            if (index == 1) UpdateMonitorsHardwareCards();
            if (index == 3) RefreshProfilesList();
            if (index == 4) UpdateSchedulerUi();
            if (index == 5 && lblSettingsSaveNotice != null) lblSettingsSaveNotice.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region View 1: Dashboard
        private Grid CreateDashboardView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 18, 24, 24);

            // 1. Header: Monitors Title + Subtitle + Action Buttons
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
            mainSubtitle.Margin = new Thickness(0, 3, 0, 0);
            titleSp.Children.Add(mainSubtitle);
            headGrid.Children.Add(titleSp);

            StackPanel actSp = new StackPanel();
            actSp.Orientation = Orientation.Horizontal;

            btnIdentify = CreateHeaderActionBtn("\uE946", I18n.T("Identify"), delegate {
                overlayManager.ShowIdentifyBadges(screenDevices);
            });
            btnMinimizeToTray = CreateHeaderActionBtn("\uE921", I18n.T("MinimizeToTray"), delegate {
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
                int idx = i;
                Border card = CreateMonitorCard(idx, screenDevices[idx]);
                monitorCardBorders.Add(card);
                monitorCardsRow.Children.Add(card);
            }

            // Synced All Displays Card
            Border allCard = CreateAllDisplaysCard();
            monitorCardBorders.Add(allCard);
            monitorCardsRow.Children.Add(allCard);

            monScroll.Content = monitorCardsRow;
            sp.Children.Add(monScroll);

            // 3. Top Row Controls: Brightness, Contrast, Gamma (3 Columns)
            Grid topControls = new Grid();
            topControls.Margin = new Thickness(0, 0, 0, 14);
            topControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(14) });
            topControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(14) });
            topControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border cardBright = CreateDashboardCard();
            cardBright.Child = CreateBrightnessControl();
            Grid.SetColumn(cardBright, 0);
            topControls.Children.Add(cardBright);

            Border cardContrast = CreateDashboardCard();
            cardContrast.Child = CreateContrastControl();
            Grid.SetColumn(cardContrast, 2);
            topControls.Children.Add(cardContrast);

            Border cardGamma = CreateDashboardCard();
            cardGamma.Child = CreateGammaControl();
            Grid.SetColumn(cardGamma, 4);
            topControls.Children.Add(cardGamma);
            sp.Children.Add(topControls);

            // 4. Middle Row Controls: Color Temp, Red, Green, Blue (4 Columns)
            Grid midControls = new Grid();
            midControls.Margin = new Thickness(0, 0, 0, 14);
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.3, GridUnitType.Star) });
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(14) });
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(14) });
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(14) });
            midControls.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border cardTemp = CreateDashboardCard();
            cardTemp.Child = CreateColorTempControl();
            Grid.SetColumn(cardTemp, 0);
            midControls.Children.Add(cardTemp);

            Border cardRed = CreateDashboardCard();
            cardRed.Child = CreateChannelControl("Red", Color.FromRgb(239, 68, 68), out sliderRed, out badgeRed, sliderStyleRed);
            Grid.SetColumn(cardRed, 2);
            midControls.Children.Add(cardRed);

            Border cardGreen = CreateDashboardCard();
            cardGreen.Child = CreateChannelControl("Green", Color.FromRgb(34, 197, 94), out sliderGreen, out badgeGreen, sliderStyleGreen);
            Grid.SetColumn(cardGreen, 4);
            midControls.Children.Add(cardGreen);

            Border cardBlue = CreateDashboardCard();
            cardBlue.Child = CreateChannelControl("Blue", Color.FromRgb(59, 130, 246), out sliderBlue, out badgeBlue, sliderStyleBlueChannel);
            Grid.SetColumn(cardBlue, 6);
            midControls.Children.Add(cardBlue);
            sp.Children.Add(midControls);

            // 5. Quick Presets Card
            Border cardPresets = CreateDashboardCard();
            cardPresets.Margin = new Thickness(0, 0, 0, 14);
            cardPresets.Child = CreateQuickPresetsControl();
            sp.Children.Add(cardPresets);

            // 6. Bottom Controls: Active Profile + Reset + Apply
            Border cardFooter = CreateFooterControl();
            sp.Children.Add(cardFooter);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private Button CreateHeaderActionBtn(string icon, string label, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 34;
            b.Style = darkButtonStyle;
            b.Margin = new Thickness(8, 0, 0, 0);
            b.Padding = new Thickness(14, 0, 14, 0);

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock it = new TextBlock();
            it.Text = icon;
            it.FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
            it.FontSize = 13;
            it.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            it.Margin = new Thickness(0, 0, 8, 0);
            it.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(it);

            TextBlock lt = new TextBlock();
            lt.Text = label;
            lt.FontSize = 12;
            lt.FontWeight = FontWeights.Medium;
            lt.Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            lt.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(lt);

            b.Content = sp;
            b.Click += click;
            return b;
        }

        private void SetHeaderActionBtnText(Button btn, string icon, string text) {
            StackPanel sp = btn.Content as StackPanel;
            if (sp != null && sp.Children.Count >= 2) {
                TextBlock it = sp.Children[0] as TextBlock;
                TextBlock lt = sp.Children[1] as TextBlock;
                if (it != null) it.Text = icon;
                if (lt != null) {
                    lt.Text = text;
                    lt.Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                }
            }
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

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border badge = new Border();
            badge.Width = 22;
            badge.Height = 22;
            badge.CornerRadius = new CornerRadius(11);
            badge.Background = new SolidColorBrush(Color.FromArgb(80, 51, 65, 85));
            badge.BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105));
            badge.BorderThickness = new Thickness(1);

            TextBlock badgeTb = new TextBlock();
            badgeTb.Text = sd.Number.ToString();
            badgeTb.FontSize = 11;
            badgeTb.FontWeight = FontWeights.Bold;
            badgeTb.Foreground = Brushes.White;
            badgeTb.HorizontalAlignment = HorizontalAlignment.Center;
            badgeTb.VerticalAlignment = VerticalAlignment.Center;
            badge.Child = badgeTb;
            topRow.Children.Add(badge);

            // Monitor Icon Vector
            Canvas monIcon = new Canvas();
            monIcon.Width = 28;
            monIcon.Height = 22;
            monIcon.HorizontalAlignment = HorizontalAlignment.Center;
            Path scrPath = new Path();
            scrPath.Data = Geometry.Parse("M 0 0 L 28 0 L 28 17 L 0 17 Z M 2 2 L 2 15 L 26 15 L 26 2 Z");
            scrPath.Fill = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            Path baseLine = new Path();
            baseLine.Data = Geometry.Parse("M 10 17 L 18 17 L 18 20 L 22 22 L 6 22 L 10 20 Z");
            baseLine.Fill = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            monIcon.Children.Add(scrPath);
            monIcon.Children.Add(baseLine);

            Grid.SetColumn(monIcon, 1);
            topRow.Children.Add(monIcon);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            // Bottom Info: Green dot + Monitor Name + Resolution
            StackPanel botSp = new StackPanel();
            botSp.VerticalAlignment = VerticalAlignment.Bottom;

            StackPanel nameRow = new StackPanel();
            nameRow.Orientation = Orientation.Horizontal;
            nameRow.HorizontalAlignment = HorizontalAlignment.Center;

            Ellipse dot = new Ellipse();
            dot.Width = 6;
            dot.Height = 6;
            dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            dot.Margin = new Thickness(0, 0, 6, 0);
            nameRow.Children.Add(dot);

            TextBlock nameTb = new TextBlock();
            nameTb.Text = sd.FriendlyName.Length > 22 ? sd.FriendlyName.Substring(0, 20) + "..." : sd.FriendlyName;
            nameTb.FontSize = 13;
            nameTb.FontWeight = FontWeights.SemiBold;
            nameTb.Foreground = Brushes.White;
            nameRow.Children.Add(nameTb);
            botSp.Children.Add(nameRow);

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
            b.MouseDown += delegate {
                syncAllMonitors = false;
                selectedMonitorIndex = index;
                config.SyncMonitors = false;
                UpdateVisualMonitorCards();
                UpdateUiFromSettings();
            };
            return b;
        }

        private Border CreateAllDisplaysCard() {
            Border b = new Border();
            b.Width = 240;
            b.Height = 110;
            b.CornerRadius = new CornerRadius(8);
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
            badge.Background = new SolidColorBrush(Color.FromArgb(80, 51, 65, 85));
            badge.BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105));
            badge.BorderThickness = new Thickness(1);

            TextBlock badgeTb = new TextBlock();
            badgeTb.Text = "⇄";
            badgeTb.FontSize = 11;
            badgeTb.FontWeight = FontWeights.Bold;
            badgeTb.Foreground = Brushes.White;
            badgeTb.HorizontalAlignment = HorizontalAlignment.Center;
            badgeTb.VerticalAlignment = VerticalAlignment.Center;
            badge.Child = badgeTb;
            topRow.Children.Add(badge);

            // Double Screen Vector Icon
            Canvas dualIcon = new Canvas();
            dualIcon.Width = 34;
            dualIcon.Height = 22;
            dualIcon.HorizontalAlignment = HorizontalAlignment.Center;
            Path p1 = new Path();
            p1.Data = Geometry.Parse("M 0 2 L 15 2 L 15 15 L 0 15 Z M 1 3 L 1 14 L 14 14 L 14 3 Z");
            p1.Fill = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            Path p2 = new Path();
            p2.Data = Geometry.Parse("M 17 0 L 33 0 L 33 14 L 17 14 Z M 18 1 L 18 13 L 32 13 L 32 1 Z");
            p2.Fill = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            dualIcon.Children.Add(p1);
            dualIcon.Children.Add(p2);

            Grid.SetColumn(dualIcon, 1);
            topRow.Children.Add(dualIcon);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            StackPanel botSp = new StackPanel();
            botSp.VerticalAlignment = VerticalAlignment.Bottom;

            TextBlock titleTb = new TextBlock();
            titleTb.Text = I18n.T("AllDisplays");
            titleTb.FontSize = 13;
            titleTb.FontWeight = FontWeights.SemiBold;
            titleTb.Foreground = Brushes.White;
            titleTb.HorizontalAlignment = HorizontalAlignment.Center;
            botSp.Children.Add(titleTb);

            TextBlock descTb = new TextBlock();
            descTb.Text = I18n.T("MonitorsConnected", screenDevices.Count);
            descTb.FontSize = 11;
            descTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            descTb.HorizontalAlignment = HorizontalAlignment.Center;
            descTb.Margin = new Thickness(0, 2, 0, 0);
            botSp.Children.Add(descTb);

            Grid.SetRow(botSp, 1);
            g.Children.Add(botSp);

            b.Child = g;
            b.MouseDown += delegate {
                syncAllMonitors = true;
                config.SyncMonitors = true;
                UpdateVisualMonitorCards();
                UpdateUiFromSettings();
            };
            return b;
        }

        private void UpdateVisualMonitorCards() {
            if (monitorCardBorders == null || monitorCardBorders.Count == 0) return;

            SolidColorBrush unselBg = new SolidColorBrush(Color.FromRgb(20, 25, 36));
            SolidColorBrush unselBorder = new SolidColorBrush(Color.FromRgb(34, 43, 62));

            SolidColorBrush selBg = new SolidColorBrush(Color.FromRgb(22, 33, 54));
            SolidColorBrush selBorder = new SolidColorBrush(Color.FromRgb(56, 189, 248));

            for (int i = 0; i < monitorCardBorders.Count; i++) {
                Border b = monitorCardBorders[i];
                bool isSel = false;
                if (syncAllMonitors) {
                    isSel = (i == monitorCardBorders.Count - 1);
                } else {
                    isSel = (i == selectedMonitorIndex);
                }

                if (isSel) {
                    b.Background = selBg;
                    b.BorderBrush = selBorder;
                    b.BorderThickness = new Thickness(2);
                } else {
                    b.Background = unselBg;
                    b.BorderBrush = unselBorder;
                    b.BorderThickness = new Thickness(1);
                }
            }

            if (btnApplyToMonitor != null) {
                string targetName = syncAllMonitors ? I18n.T("AllDisplays") : (selectedMonitorIndex < screenDevices.Count ? screenDevices[selectedMonitorIndex].DisplayLabel : "Monitor");
                SetHeaderActionBtnText(btnApplyToMonitor, "\u2713", I18n.T("ApplyToMonitor", targetName));
            }
        }

        private Border CreateDashboardCard() {
            Border b = new Border();
            b.Background = new SolidColorBrush(Color.FromRgb(20, 25, 36));
            b.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 43, 62));
            b.BorderThickness = new Thickness(1);
            b.CornerRadius = new CornerRadius(8);
            b.Padding = new Thickness(16, 14, 16, 14);
            return b;
        }

        private Grid CreateBrightnessControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "☼";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(250, 204, 21));
            icon.Margin = new Thickness(0, 0, 8, 0);
            titleSp.Children.Add(icon);

            lblBrightnessTitle = new TextBlock();
            lblBrightnessTitle.Text = I18n.T("Brightness");
            lblBrightnessTitle.FontSize = 13;
            lblBrightnessTitle.FontWeight = FontWeights.SemiBold;
            lblBrightnessTitle.Foreground = Brushes.White;
            titleSp.Children.Add(lblBrightnessTitle);
            head.Children.Add(titleSp);

            badgeBrightness = new TextBlock();
            badgeBrightness.Text = "100%";
            badgeBrightness.FontSize = 13;
            badgeBrightness.FontWeight = FontWeights.Bold;
            badgeBrightness.Foreground = Brushes.White;
            Grid.SetColumn(badgeBrightness, 1);
            head.Children.Add(badgeBrightness);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            Grid sliderRow = new Grid();
            sliderRow.Margin = new Thickness(0, 14, 0, 0);
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock minIc = new TextBlock();
            minIc.Text = "☼";
            minIc.FontSize = 10;
            minIc.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            minIc.Margin = new Thickness(0, 0, 8, 0);
            sliderRow.Children.Add(minIc);

            sliderBrightness = new Slider();
            sliderBrightness.Minimum = 0.10;
            sliderBrightness.Maximum = 1.50;
            sliderBrightness.Value = 1.0;
            sliderBrightness.Style = sliderStyleBlue;
            sliderBrightness.ValueChanged += delegate {
                badgeBrightness.Text = string.Format("{0}%", (int)Math.Round(sliderBrightness.Value * 100));
                OnControlChanged();
            };
            Grid.SetColumn(sliderBrightness, 1);
            sliderRow.Children.Add(sliderBrightness);

            TextBlock maxIc = new TextBlock();
            maxIc.Text = "☼";
            maxIc.FontSize = 14;
            maxIc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            maxIc.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(maxIc, 2);
            sliderRow.Children.Add(maxIc);

            Grid.SetRow(sliderRow, 1);
            g.Children.Add(sliderRow);
            return g;
        }

        private Grid CreateContrastControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "◐";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            icon.Margin = new Thickness(0, 0, 8, 0);
            titleSp.Children.Add(icon);

            lblContrastTitle = new TextBlock();
            lblContrastTitle.Text = I18n.T("Contrast");
            lblContrastTitle.FontSize = 13;
            lblContrastTitle.FontWeight = FontWeights.SemiBold;
            lblContrastTitle.Foreground = Brushes.White;
            titleSp.Children.Add(lblContrastTitle);
            head.Children.Add(titleSp);

            badgeContrast = new TextBlock();
            badgeContrast.Text = "100%";
            badgeContrast.FontSize = 13;
            badgeContrast.FontWeight = FontWeights.Bold;
            badgeContrast.Foreground = Brushes.White;
            Grid.SetColumn(badgeContrast, 1);
            head.Children.Add(badgeContrast);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            Grid sliderRow = new Grid();
            sliderRow.Margin = new Thickness(0, 14, 0, 0);
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock minIc = new TextBlock();
            minIc.Text = "◐";
            minIc.FontSize = 10;
            minIc.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            minIc.Margin = new Thickness(0, 0, 8, 0);
            sliderRow.Children.Add(minIc);

            sliderContrast = new Slider();
            sliderContrast.Minimum = 0.20;
            sliderContrast.Maximum = 2.00;
            sliderContrast.Value = 1.0;
            sliderContrast.Style = sliderStyleBlue;
            sliderContrast.ValueChanged += delegate {
                badgeContrast.Text = string.Format("{0}%", (int)Math.Round(sliderContrast.Value * 100));
                OnControlChanged();
            };
            Grid.SetColumn(sliderContrast, 1);
            sliderRow.Children.Add(sliderContrast);

            TextBlock maxIc = new TextBlock();
            maxIc.Text = "◑";
            maxIc.FontSize = 14;
            maxIc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            maxIc.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(maxIc, 2);
            sliderRow.Children.Add(maxIc);

            Grid.SetRow(sliderRow, 1);
            g.Children.Add(sliderRow);
            return g;
        }

        private Grid CreateGammaControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "☡";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(192, 132, 252));
            icon.Margin = new Thickness(0, 0, 8, 0);
            titleSp.Children.Add(icon);

            lblGammaTitle = new TextBlock();
            lblGammaTitle.Text = I18n.T("Gamma");
            lblGammaTitle.FontSize = 13;
            lblGammaTitle.FontWeight = FontWeights.SemiBold;
            lblGammaTitle.Foreground = Brushes.White;
            titleSp.Children.Add(lblGammaTitle);
            head.Children.Add(titleSp);

            badgeGamma = new TextBlock();
            badgeGamma.Text = "1.0";
            badgeGamma.FontSize = 13;
            badgeGamma.FontWeight = FontWeights.Bold;
            badgeGamma.Foreground = Brushes.White;
            Grid.SetColumn(badgeGamma, 1);
            head.Children.Add(badgeGamma);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            sliderGamma = new Slider();
            sliderGamma.Minimum = 0.5;
            sliderGamma.Maximum = 2.5;
            sliderGamma.Value = 1.0;
            sliderGamma.Margin = new Thickness(0, 14, 0, 0);
            sliderGamma.Style = sliderStyleBlue;
            sliderGamma.ValueChanged += delegate {
                badgeGamma.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F2}", sliderGamma.Value);
                HighlightGammaTick();
                OnControlChanged();
            };
            Grid.SetRow(sliderGamma, 1);
            g.Children.Add(sliderGamma);

            // Ticks Row: 0.6, 0.8, 1.0 (Standard), 1.4, 2.0
            Grid ticksGrid = new Grid();
            ticksGrid.Margin = new Thickness(0, 4, 0, 0);
            ticksGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticksGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticksGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticksGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ticksGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            gammaTick18 = CreateTickLabel("0.6", 0, ticksGrid);
            gammaTick20 = CreateTickLabel("0.8", 1, ticksGrid);

            gammaTick22Badge = new Border();
            gammaTick22Badge.Background = new SolidColorBrush(Color.FromArgb(80, 56, 189, 248));
            gammaTick22Badge.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            gammaTick22Badge.BorderThickness = new Thickness(1);
            gammaTick22Badge.CornerRadius = new CornerRadius(10);
            gammaTick22Badge.Padding = new Thickness(6, 1, 6, 1);
            gammaTick22Badge.HorizontalAlignment = HorizontalAlignment.Center;
            gammaTick22Badge.Cursor = Cursors.Hand;
            gammaTick22Badge.MouseDown += delegate { sliderGamma.Value = 1.0; };

            gammaTick22Text = new TextBlock();
            gammaTick22Text.Text = "1.0";
            gammaTick22Text.FontSize = 10;
            gammaTick22Text.FontWeight = FontWeights.Bold;
            gammaTick22Text.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            gammaTick22Badge.Child = gammaTick22Text;
            Grid.SetColumn(gammaTick22Badge, 2);
            ticksGrid.Children.Add(gammaTick22Badge);

            gammaTick24 = CreateTickLabel("1.4", 3, ticksGrid);
            gammaTick26 = CreateTickLabel("2.0", 4, ticksGrid);

            Grid.SetRow(ticksGrid, 2);
            g.Children.Add(ticksGrid);
            return g;
        }

        private TextBlock CreateTickLabel(string text, int col, Grid parent) {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.FontSize = 10;
            tb.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Cursor = Cursors.Hand;
            tb.MouseDown += delegate {
                double v;
                if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v)) {
                    sliderGamma.Value = v;
                }
            };
            Grid.SetColumn(tb, col);
            parent.Children.Add(tb);
            return tb;
        }

        private void HighlightGammaTick() {
            if (gammaTick22Badge == null) return;
            if (Math.Abs(sliderGamma.Value - 1.0) < 0.05) {
                gammaTick22Badge.Background = new SolidColorBrush(Color.FromArgb(90, 56, 189, 248));
                gammaTick22Badge.BorderThickness = new Thickness(1);
            } else {
                gammaTick22Badge.Background = Brushes.Transparent;
                gammaTick22Badge.BorderThickness = new Thickness(0);
            }
        }

        private Grid CreateColorTempControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "🌡";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            icon.Margin = new Thickness(0, 0, 8, 0);
            titleSp.Children.Add(icon);

            lblColorTempTitle = new TextBlock();
            lblColorTempTitle.Text = I18n.T("ColorTemp");
            lblColorTempTitle.FontSize = 13;
            lblColorTempTitle.FontWeight = FontWeights.SemiBold;
            lblColorTempTitle.Foreground = Brushes.White;
            titleSp.Children.Add(lblColorTempTitle);
            head.Children.Add(titleSp);

            badgeColorTemp = new TextBlock();
            badgeColorTemp.Text = "6500 K";
            badgeColorTemp.FontSize = 13;
            badgeColorTemp.FontWeight = FontWeights.Bold;
            badgeColorTemp.Foreground = Brushes.White;
            Grid.SetColumn(badgeColorTemp, 1);
            head.Children.Add(badgeColorTemp);
            Grid.SetRow(head, 0);
            g.Children.Add(head);

            sliderColorTemp = new Slider();
            sliderColorTemp.Minimum = 2500;
            sliderColorTemp.Maximum = 9500;
            sliderColorTemp.Value = 6500;
            sliderColorTemp.Margin = new Thickness(0, 14, 0, 0);
            sliderColorTemp.Style = sliderStyleTemp;
            sliderColorTemp.ValueChanged += delegate {
                badgeColorTemp.Text = string.Format("{0} K", (int)Math.Round(sliderColorTemp.Value));
                OnControlChanged();
            };
            Grid.SetRow(sliderColorTemp, 1);
            g.Children.Add(sliderColorTemp);

            // Labels: Warm, Neutral (clickable badge), Cool
            Grid labelsGrid = new Grid();
            labelsGrid.Margin = new Thickness(0, 4, 0, 0);
            labelsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            labelsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            labelsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock wTb = new TextBlock();
            wTb.Text = I18n.T("Warm");
            wTb.FontSize = 10;
            wTb.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            wTb.HorizontalAlignment = HorizontalAlignment.Left;
            wTb.Cursor = Cursors.Hand;
            wTb.MouseDown += delegate { sliderColorTemp.Value = 4000; };
            Grid.SetColumn(wTb, 0);
            labelsGrid.Children.Add(wTb);

            Border nBadge = new Border();
            nBadge.Background = new SolidColorBrush(Color.FromArgb(80, 56, 189, 248));
            nBadge.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            nBadge.BorderThickness = new Thickness(1);
            nBadge.CornerRadius = new CornerRadius(10);
            nBadge.Padding = new Thickness(8, 1, 8, 1);
            nBadge.HorizontalAlignment = HorizontalAlignment.Center;
            nBadge.Cursor = Cursors.Hand;
            nBadge.MouseDown += delegate { sliderColorTemp.Value = 6500; };

            TextBlock nTb = new TextBlock();
            nTb.Text = I18n.T("Neutral");
            nTb.FontSize = 10;
            nTb.FontWeight = FontWeights.Bold;
            nTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            nBadge.Child = nTb;
            Grid.SetColumn(nBadge, 1);
            labelsGrid.Children.Add(nBadge);

            TextBlock cTb = new TextBlock();
            cTb.Text = I18n.T("Cold");
            cTb.FontSize = 10;
            cTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            cTb.HorizontalAlignment = HorizontalAlignment.Right;
            cTb.Cursor = Cursors.Hand;
            cTb.MouseDown += delegate { sliderColorTemp.Value = 8500; };
            Grid.SetColumn(cTb, 2);
            labelsGrid.Children.Add(cTb);

            Grid.SetRow(labelsGrid, 2);
            g.Children.Add(labelsGrid);
            return g;
        }

        private Grid CreateChannelControl(string channelName, Color dotColor, out Slider targetSlider, out TextBlock targetBadge, Style sliderStyle) {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            Ellipse dot = new Ellipse();
            dot.Width = 8;
            dot.Height = 8;
            dot.Fill = new SolidColorBrush(dotColor);
            dot.Margin = new Thickness(0, 0, 8, 0);
            titleSp.Children.Add(dot);

            TextBlock lbl = new TextBlock();
            lbl.Text = I18n.T(channelName);
            lbl.FontSize = 13;
            lbl.FontWeight = FontWeights.SemiBold;
            lbl.Foreground = Brushes.White;
            if (channelName == "Red") lblRedTitle = lbl;
            else if (channelName == "Green") lblGreenTitle = lbl;
            else if (channelName == "Blue") lblBlueTitle = lbl;
            titleSp.Children.Add(lbl);
            head.Children.Add(titleSp);

            TextBlock badge = new TextBlock();
            badge.Text = "100";
            badge.FontSize = 13;
            badge.FontWeight = FontWeights.Bold;
            badge.Foreground = Brushes.White;
            Grid.SetColumn(badge, 1);
            head.Children.Add(badge);
            targetBadge = badge;

            Grid.SetRow(head, 0);
            g.Children.Add(head);

            Slider s = new Slider();
            s.Minimum = 0.0;
            s.Maximum = 1.0;
            s.Value = 1.0;
            s.Margin = new Thickness(0, 14, 0, 0);
            s.Style = sliderStyle;
            TextBlock bRef = badge;
            s.ValueChanged += delegate {
                bRef.Text = string.Format("{0}", (int)Math.Round(s.Value * 100));
                OnControlChanged();
            };
            targetSlider = s;
            Grid.SetRow(s, 1);
            g.Children.Add(s);

            Grid minMaxGrid = new Grid();
            minMaxGrid.Margin = new Thickness(0, 4, 0, 0);
            minMaxGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            minMaxGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock zeroTb = new TextBlock();
            zeroTb.Text = "0";
            zeroTb.FontSize = 10;
            zeroTb.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            minMaxGrid.Children.Add(zeroTb);

            TextBlock hundTb = new TextBlock();
            hundTb.Text = "100";
            hundTb.FontSize = 10;
            hundTb.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            hundTb.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(hundTb, 1);
            minMaxGrid.Children.Add(hundTb);

            Grid.SetRow(minMaxGrid, 2);
            g.Children.Add(minMaxGrid);
            return g;
        }

        private Grid CreateQuickPresetsControl() {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(180) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel infoSp = new StackPanel();
            infoSp.VerticalAlignment = VerticalAlignment.Center;

            StackPanel titleRow = new StackPanel();
            titleRow.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "✦";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            icon.Margin = new Thickness(0, 0, 6, 0);
            titleRow.Children.Add(icon);

            lblQuickPresetsTitle = new TextBlock();
            lblQuickPresetsTitle.Text = I18n.T("QuickPresets");
            lblQuickPresetsTitle.FontSize = 13;
            lblQuickPresetsTitle.FontWeight = FontWeights.SemiBold;
            lblQuickPresetsTitle.Foreground = Brushes.White;
            titleRow.Children.Add(lblQuickPresetsTitle);
            infoSp.Children.Add(titleRow);

            lblQuickPresetsSubtitle = new TextBlock();
            lblQuickPresetsSubtitle.Text = I18n.T("QuickPresetsDesc");
            lblQuickPresetsSubtitle.FontSize = 11;
            lblQuickPresetsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            lblQuickPresetsSubtitle.Margin = new Thickness(0, 2, 0, 0);
            infoSp.Children.Add(lblQuickPresetsSubtitle);

            Grid.SetColumn(infoSp, 0);
            g.Children.Add(infoSp);

            // Preset Pills Row
            UniformGrid pillsGrid = new UniformGrid();
            pillsGrid.Rows = 1;
            pillsGrid.Columns = 6;
            pillsGrid.VerticalAlignment = VerticalAlignment.Center;

            presetButtons = new List<Button>();
            btnPresetWarm = CreatePresetPillBtn(I18n.T("PresetWarm"), false, delegate {
                ApplyPreset(0.85, 0.95, 1.05, 4500, 1.0, 0.95, 0.85, "Warm");
                HighlightPresetPill(btnPresetWarm);
            });
            btnPresetNeutral = CreatePresetPillBtn(I18n.T("PresetNeutral"), true, delegate {
                ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0, "Standard");
                HighlightPresetPill(btnPresetNeutral);
            });
            btnPresetCool = CreatePresetPillBtn(I18n.T("PresetCool"), false, delegate {
                ApplyPreset(1.0, 1.10, 1.0, 8000, 0.92, 0.96, 1.0, "Cool");
                HighlightPresetPill(btnPresetCool);
            });
            btnPresetMovie = CreatePresetPillBtn(I18n.T("PresetMovie"), false, delegate {
                ApplyPreset(0.90, 1.15, 1.15, 5800, 1.0, 0.95, 0.90, "Movie");
                HighlightPresetPill(btnPresetMovie);
            });
            btnPresetWork = CreatePresetPillBtn(I18n.T("PresetWork"), false, delegate {
                ApplyPreset(0.80, 1.0, 1.0, 6000, 1.0, 1.0, 0.96, "Work");
                HighlightPresetPill(btnPresetWork);
            });
            btnPresetNight = CreatePresetPillBtn(I18n.T("PresetNight"), false, delegate {
                ApplyPreset(0.65, 0.90, 1.0, 3400, 1.0, 0.84, 0.68, "Night");
                HighlightPresetPill(btnPresetNight);
            });

            presetButtons.Add(btnPresetWarm);
            presetButtons.Add(btnPresetNeutral);
            presetButtons.Add(btnPresetCool);
            presetButtons.Add(btnPresetMovie);
            presetButtons.Add(btnPresetWork);
            presetButtons.Add(btnPresetNight);

            foreach (Button pb in presetButtons) {
                pillsGrid.Children.Add(pb);
            }

            Grid.SetColumn(pillsGrid, 1);
            g.Children.Add(pillsGrid);
            return g;
        }

        private Button CreatePresetPillBtn(string label, bool isSelected, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 32;
            b.Margin = new Thickness(4, 0, 4, 0);
            b.Cursor = Cursors.Hand;
            b.Style = darkButtonStyle;
            b.Content = label;
            b.FontSize = 11;
            b.FontWeight = FontWeights.Medium;
            b.Click += click;
            ApplyPresetPillStyle(b, isSelected);
            return b;
        }

        private void ApplyPresetPillStyle(Button b, bool isSelected) {
            if (isSelected) {
                b.Background = new SolidColorBrush(Color.FromRgb(26, 42, 68));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                b.BorderThickness = new Thickness(1.5);
                b.Foreground = Brushes.White;
            } else {
                b.Background = new SolidColorBrush(Color.FromRgb(20, 25, 36));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 43, 62));
                b.BorderThickness = new Thickness(1);
                b.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
            }
        }

        private void HighlightPresetPill(Button activeBtn) {
            if (presetButtons == null) return;
            foreach (Button b in presetButtons) {
                ApplyPresetPillStyle(b, b == activeBtn);
            }
        }

        private Border CreateFooterControl() {
            Border b = new Border();
            b.Margin = new Thickness(0, 4, 0, 0);

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Left: Active Profile Dropdown Card
            Border profBorder = new Border();
            profBorder.Background = new SolidColorBrush(Color.FromRgb(20, 25, 36));
            profBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 43, 62));
            profBorder.BorderThickness = new Thickness(1);
            profBorder.CornerRadius = new CornerRadius(8);
            profBorder.Padding = new Thickness(14, 6, 14, 6);

            StackPanel profSp = new StackPanel();
            profSp.Orientation = Orientation.Horizontal;
            profSp.VerticalAlignment = VerticalAlignment.Center;

            TextBlock uIc = new TextBlock();
            uIc.Text = "\uE77B";
            uIc.FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
            uIc.FontSize = 14;
            uIc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            uIc.Margin = new Thickness(0, 0, 8, 0);
            uIc.VerticalAlignment = VerticalAlignment.Center;
            profSp.Children.Add(uIc);

            lblActiveProfileTitle = new TextBlock();
            lblActiveProfileTitle.Text = I18n.T("ActiveProfile");
            lblActiveProfileTitle.FontSize = 12;
            lblActiveProfileTitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            lblActiveProfileTitle.Margin = new Thickness(0, 0, 8, 0);
            lblActiveProfileTitle.VerticalAlignment = VerticalAlignment.Center;
            profSp.Children.Add(lblActiveProfileTitle);

            comboActiveProfile = new ComboBox();
            comboActiveProfile.Style = darkComboBoxStyle;
            comboActiveProfile.Width = 140;
            comboActiveProfile.Height = 32;
            PopulateProfilesDropdown(comboActiveProfile);

            comboActiveProfile.SelectionChanged += delegate {
                if (!isUpdatingUi && comboActiveProfile.SelectedItem != null) {
                    string profName = comboActiveProfile.SelectedItem.ToString();
                    UserProfile p = profileManager.Find(profName);
                    if (p != null) {
                        ApplyPreset(p.Brightness, p.Contrast, p.Gamma, p.Temperature, p.Red, p.Green, p.Blue, p.Name);
                    }
                    config.ActiveProfile = profName;
                    config.Save();
                }
            };
            profSp.Children.Add(comboActiveProfile);

            profBorder.Child = profSp;
            Grid.SetColumn(profBorder, 0);
            g.Children.Add(profBorder);

            // Right: Reset to Default + Apply to Monitor
            StackPanel actRow = new StackPanel();
            actRow.Orientation = Orientation.Horizontal;

            btnResetDefault = CreateHeaderActionBtn("\uE72C", I18n.T("ResetToDefault"), delegate {
                ResetCurrentOrAll();
            });
            btnResetDefault.Height = 38;
            btnResetDefault.Padding = new Thickness(16, 0, 16, 0);
            btnResetDefault.Margin = new Thickness(0, 0, 12, 0);
            actRow.Children.Add(btnResetDefault);

            btnApplyToMonitor = new Button();
            btnApplyToMonitor.Height = 38;
            btnApplyToMonitor.Style = darkPrimaryButtonStyle;
            btnApplyToMonitor.Padding = new Thickness(20, 0, 20, 0);
            btnApplyToMonitor.Cursor = Cursors.Hand;

            StackPanel applySp = new StackPanel();
            applySp.Orientation = Orientation.Horizontal;
            TextBlock applyIc = new TextBlock();
            applyIc.Text = "\u2713";
            applyIc.FontSize = 13;
            applyIc.FontWeight = FontWeights.Bold;
            applyIc.Foreground = Brushes.White;
            applyIc.Margin = new Thickness(0, 0, 8, 0);
            applySp.Children.Add(applyIc);

            TextBlock applyLt = new TextBlock();
            string monLbl = syncAllMonitors ? I18n.T("AllDisplays") : (selectedMonitorIndex < screenDevices.Count ? screenDevices[selectedMonitorIndex].DisplayLabel : "Monitor");
            applyLt.Text = I18n.T("ApplyToMonitor", monLbl);
            applyLt.FontSize = 13;
            applyLt.FontWeight = FontWeights.SemiBold;
            applyLt.Foreground = Brushes.White;
            applySp.Children.Add(applyLt);

            btnApplyToMonitor.Content = applySp;
            btnApplyToMonitor.Click += delegate {
                ApplyAllActiveSettings();
                config.Save();
            };
            actRow.Children.Add(btnApplyToMonitor);

            Grid.SetColumn(actRow, 2);
            g.Children.Add(actRow);
            b.Child = g;
            return b;
        }

        private void PopulateProfilesDropdown(ComboBox cb) {
            cb.Items.Clear();
            foreach (UserProfile up in profileManager.Profiles) {
                cb.Items.Add(up.Name);
            }
            int idx = cb.Items.IndexOf(config.ActiveProfile);
            cb.SelectedIndex = (idx >= 0) ? idx : 0;
        }
        #endregion

        #region View 2: Monitors (Hardware Details & Calibration)
        private Grid CreateMonitorsView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 18, 24, 24);

            // 1. Header
            Grid headGrid = new Grid();
            headGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            headGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            txtMonHardwareTitle = new TextBlock();
            txtMonHardwareTitle.Text = I18n.T("MonHardwareTitle");
            txtMonHardwareTitle.FontSize = 20;
            txtMonHardwareTitle.FontWeight = FontWeights.Bold;
            txtMonHardwareTitle.Foreground = Brushes.White;
            titleSp.Children.Add(txtMonHardwareTitle);

            txtMonHardwareSubtitle = new TextBlock();
            txtMonHardwareSubtitle.Text = I18n.T("MonHardwareSubtitle");
            txtMonHardwareSubtitle.FontSize = 12;
            txtMonHardwareSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtMonHardwareSubtitle.Margin = new Thickness(0, 3, 0, 0);
            titleSp.Children.Add(txtMonHardwareSubtitle);
            headGrid.Children.Add(titleSp);

            StackPanel actSp = new StackPanel();
            actSp.Orientation = Orientation.Horizontal;

            btnRefreshDisplays = CreateHeaderActionBtn("\uE72C", I18n.T("RefreshMonitors"), delegate {
                DetectMonitors();
                UpdateMonitorsHardwareCards();
                UpdateVisualMonitorCards();
            });
            btnIdentifyFromMon = CreateHeaderActionBtn("\uE946", I18n.T("Identify"), delegate {
                overlayManager.ShowIdentifyBadges(screenDevices);
            });

            actSp.Children.Add(btnRefreshDisplays);
            actSp.Children.Add(btnIdentifyFromMon);
            Grid.SetColumn(actSp, 1);
            headGrid.Children.Add(actSp);
            sp.Children.Add(headGrid);

            // 2. Display Hardware Topology Cards Container
            monitorsHardwareContainer = new StackPanel();
            monitorsHardwareContainer.Margin = new Thickness(0, 18, 0, 18);
            sp.Children.Add(monitorsHardwareContainer);
            UpdateMonitorsHardwareCards();

            // 3. Visual Calibration Assistant Section
            Border calibCard = CreateDashboardCard();
            StackPanel calibSp = new StackPanel();

            txtCalibrationTitle = new TextBlock();
            txtCalibrationTitle.Text = I18n.T("CalibrationTitle");
            txtCalibrationTitle.FontSize = 15;
            txtCalibrationTitle.FontWeight = FontWeights.Bold;
            txtCalibrationTitle.Foreground = Brushes.White;
            calibSp.Children.Add(txtCalibrationTitle);

            txtCalibrationDesc = new TextBlock();
            txtCalibrationDesc.Text = I18n.T("CalibrationDesc");
            txtCalibrationDesc.FontSize = 12;
            txtCalibrationDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtCalibrationDesc.TextWrapping = TextWrapping.Wrap;
            txtCalibrationDesc.Margin = new Thickness(0, 4, 0, 16);
            calibSp.Children.Add(txtCalibrationDesc);

            UniformGrid patGrid = new UniformGrid();
            patGrid.Rows = 1;
            patGrid.Columns = 4;

            btnCalibWhite = CreatePatternButton(I18n.T("PatWhite"), "Backlight & Uniformity", delegate {
                LaunchCalibrationPattern(CalibrationPatternType.White);
            });
            btnCalibBlack = CreatePatternButton(I18n.T("PatBlack"), "IPS Glow & True Black", delegate {
                LaunchCalibrationPattern(CalibrationPatternType.Black);
            });
            btnCalibGray = CreatePatternButton(I18n.T("PatGray"), "16-Step Gamma Ramp", delegate {
                LaunchCalibrationPattern(CalibrationPatternType.Grayscale);
            });
            btnCalibColor = CreatePatternButton(I18n.T("PatColor"), "SMPTE 8 Color Bars", delegate {
                LaunchCalibrationPattern(CalibrationPatternType.ColorBars);
            });

            patGrid.Children.Add(btnCalibWhite);
            patGrid.Children.Add(btnCalibBlack);
            patGrid.Children.Add(btnCalibGray);
            patGrid.Children.Add(btnCalibColor);

            calibSp.Children.Add(patGrid);
            calibCard.Child = calibSp;
            sp.Children.Add(calibCard);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private Button CreatePatternButton(string title, string subtitle, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 72;
            b.Margin = new Thickness(5);
            b.Style = darkButtonStyle;
            b.Cursor = Cursors.Hand;

            StackPanel sp = new StackPanel();
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock tt = new TextBlock();
            tt.Text = title;
            tt.FontSize = 13;
            tt.FontWeight = FontWeights.SemiBold;
            tt.Foreground = Brushes.White;
            tt.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Children.Add(tt);

            TextBlock st = new TextBlock();
            st.Text = subtitle;
            st.FontSize = 11;
            st.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            st.HorizontalAlignment = HorizontalAlignment.Center;
            st.Margin = new Thickness(0, 4, 0, 0);
            sp.Children.Add(st);

            b.Content = sp;
            b.Click += click;
            return b;
        }

        private void LaunchCalibrationPattern(CalibrationPatternType pat) {
            SWF.Screen target = SWF.Screen.PrimaryScreen;
            if (selectedMonitorIndex < screenDevices.Count) {
                string devName = screenDevices[selectedMonitorIndex].DeviceName;
                foreach (SWF.Screen s in SWF.Screen.AllScreens) {
                    if (s.DeviceName == devName) {
                        target = s;
                        break;
                    }
                }
            }
            CalibrationWindow win = new CalibrationWindow(pat, target);
            win.Show();
        }

        private void UpdateMonitorsHardwareCards() {
            if (monitorsHardwareContainer == null) return;
            monitorsHardwareContainer.Children.Clear();

            for (int i = 0; i < screenDevices.Count; i++) {
                ScreenDevice sd = screenDevices[i];
                Border card = CreateDashboardCard();
                card.Margin = new Thickness(0, 0, 0, 12);

                Grid g = new Grid();
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                // Top Line: Icon + Display Name + Primary Badge
                Grid topRow = new Grid();
                topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                StackPanel nameSp = new StackPanel();
                nameSp.Orientation = Orientation.Horizontal;

                TextBlock icon = new TextBlock();
                icon.Text = "\uE7F4";
                icon.FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
                icon.FontSize = 16;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                icon.Margin = new Thickness(0, 0, 8, 0);
                icon.VerticalAlignment = VerticalAlignment.Center;
                nameSp.Children.Add(icon);

                TextBlock title = new TextBlock();
                title.Text = string.Format("{0} - {1}", sd.DisplayLabel, sd.FriendlyName);
                title.FontSize = 14;
                title.FontWeight = FontWeights.Bold;
                title.Foreground = Brushes.White;
                title.VerticalAlignment = VerticalAlignment.Center;
                nameSp.Children.Add(title);
                topRow.Children.Add(nameSp);

                Border tag = new Border();
                tag.CornerRadius = new CornerRadius(4);
                tag.Padding = new Thickness(8, 2, 8, 2);
                tag.Background = sd.IsPrimary ? new SolidColorBrush(Color.FromRgb(30, 58, 138)) : new SolidColorBrush(Color.FromRgb(30, 41, 59));
                tag.BorderBrush = sd.IsPrimary ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(71, 85, 105));
                tag.BorderThickness = new Thickness(1);

                TextBlock tagTb = new TextBlock();
                tagTb.Text = sd.IsPrimary ? I18n.T("Primary") : I18n.T("Secondary");
                tagTb.FontSize = 11;
                tagTb.FontWeight = FontWeights.SemiBold;
                tagTb.Foreground = sd.IsPrimary ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
                tag.Child = tagTb;
                Grid.SetColumn(tag, 1);
                topRow.Children.Add(tag);

                Grid.SetRow(topRow, 0);
                g.Children.Add(topRow);

                // Hardware Parameters Grid
                UniformGrid specsGrid = new UniformGrid();
                specsGrid.Margin = new Thickness(0, 14, 0, 0);
                specsGrid.Rows = 2;
                specsGrid.Columns = 3;

                specsGrid.Children.Add(CreateSpecItem(I18n.T("Resolution"), sd.ResolutionText));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("AdapterName"), sd.DeviceName));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("DesktopBounds"), string.Format("X={0}, Y={1}", sd.Bounds.X, sd.Bounds.Y)));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("RefreshRate"), string.Format("{0} Hz", sd.RefreshRate)));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("ColorDepth"), "32-bit True Color (ARGB)"));
                specsGrid.Children.Add(CreateSpecItem("Video Output Engine", "Hardware DWM Composition (MagEffect)"));

                Grid.SetRow(specsGrid, 1);
                g.Children.Add(specsGrid);

                card.Child = g;
                monitorsHardwareContainer.Children.Add(card);
            }
        }

        private StackPanel CreateSpecItem(string label, string val) {
            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(4, 4, 12, 4);

            TextBlock lt = new TextBlock();
            lt.Text = label;
            lt.FontSize = 11;
            lt.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            sp.Children.Add(lt);

            TextBlock vt = new TextBlock();
            vt.Text = val;
            vt.FontSize = 12;
            vt.FontWeight = FontWeights.Medium;
            vt.Foreground = new SolidColorBrush(Color.FromRgb(241, 245, 249));
            vt.Margin = new Thickness(0, 2, 0, 0);
            sp.Children.Add(vt);
            return sp;
        }
        #endregion

        #region View 3: Presets Catalog
        private Grid CreatePresetsView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 18, 24, 24);

            // 1. Header
            StackPanel titleSp = new StackPanel();
            titleSp.Margin = new Thickness(0, 0, 0, 18);

            txtPresetsTitle = new TextBlock();
            txtPresetsTitle.Text = I18n.T("PresetsTitle");
            txtPresetsTitle.FontSize = 20;
            txtPresetsTitle.FontWeight = FontWeights.Bold;
            txtPresetsTitle.Foreground = Brushes.White;
            titleSp.Children.Add(txtPresetsTitle);

            txtPresetsSubtitle = new TextBlock();
            txtPresetsSubtitle.Text = I18n.T("PresetsSubtitle");
            txtPresetsSubtitle.FontSize = 12;
            txtPresetsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtPresetsSubtitle.Margin = new Thickness(0, 3, 0, 0);
            titleSp.Children.Add(txtPresetsSubtitle);
            sp.Children.Add(titleSp);

            // 2. Presets Grid (8 Catalog Profiles)
            presetsGrid = new UniformGrid();
            presetsGrid.Columns = 2;
            presetsGrid.Margin = new Thickness(-6, 0, -6, 16);

            AddPresetCatalogCard("Standard / Neutral", "True color reproduction, factory calibrated balance", 1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0, Color.FromRgb(248, 250, 252));
            AddPresetCatalogCard("Warm / Reading", "Gentle on the eyes, reduces fatigue during reading", 0.85, 0.95, 1.05, 4500, 1.0, 0.95, 0.85, Color.FromRgb(251, 146, 60));
            AddPresetCatalogCard("Cool / Vivid", "Crisp daylight color, punchy whites and sharp edges", 1.0, 1.10, 1.0, 8000, 0.92, 0.96, 1.0, Color.FromRgb(56, 189, 248));
            AddPresetCatalogCard("Cinema / Movie", "Deeper blacks, rich contrast and cinematic highlights", 0.90, 1.15, 1.15, 5800, 1.0, 0.95, 0.90, Color.FromRgb(168, 85, 247));
            AddPresetCatalogCard("Office / Work", "Comfortable daytime brightness for spreadsheets and docs", 0.80, 1.0, 1.0, 6000, 1.0, 1.0, 0.96, Color.FromRgb(34, 197, 94));
            AddPresetCatalogCard("Night Shift", "Maximum blue light reduction for late night comfort", 0.65, 0.90, 1.0, 3400, 1.0, 0.84, 0.68, Color.FromRgb(249, 115, 22));
            AddPresetCatalogCard("Gaming / Shadows", "Boosted shadow visibility and high saturation for games", 1.0, 1.20, 1.25, 7000, 1.0, 1.0, 1.0, Color.FromRgb(239, 68, 68));
            AddPresetCatalogCard("High Contrast / Focus", "Maximum contrast edge separation for coding & documents", 0.95, 1.30, 1.0, 6500, 1.0, 1.0, 1.0, Color.FromRgb(234, 179, 8));

            sp.Children.Add(presetsGrid);

            // 3. Create Custom Preset Card
            Border customCard = CreateDashboardCard();
            StackPanel custSp = new StackPanel();

            TextBlock custTitle = new TextBlock();
            custTitle.Text = I18n.T("NewProfileTitle");
            custTitle.FontSize = 14;
            custTitle.FontWeight = FontWeights.Bold;
            custTitle.Foreground = Brushes.White;
            custSp.Children.Add(custTitle);

            TextBlock custDesc = new TextBlock();
            custDesc.Text = "Save your current screen brightness, contrast, gamma and color levels as a new reusable preset.";
            custDesc.FontSize = 11;
            custDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            custDesc.Margin = new Thickness(0, 2, 0, 12);
            custSp.Children.Add(custDesc);

            Grid inputRow = new Grid();
            inputRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            inputRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            txtCustomPresetName = new TextBox();
            txtCustomPresetName.Style = darkTextBoxStyle;
            txtCustomPresetName.Text = "My Custom Preset";
            txtCustomPresetName.Height = 36;
            txtCustomPresetName.Margin = new Thickness(0, 0, 12, 0);
            Grid.SetColumn(txtCustomPresetName, 0);
            inputRow.Children.Add(txtCustomPresetName);

            btnSaveCustomPreset = new Button();
            btnSaveCustomPreset.Style = darkPrimaryButtonStyle;
            btnSaveCustomPreset.Content = "💾 " + I18n.T("BtnSaveToProfile");
            btnSaveCustomPreset.Height = 36;
            btnSaveCustomPreset.Padding = new Thickness(16, 0, 16, 0);
            btnSaveCustomPreset.Click += delegate {
                string pName = txtCustomPresetName.Text.Trim();
                if (string.IsNullOrEmpty(pName)) pName = "Custom Preset";
                profileManager.AddOrUpdate(pName, globalSettings);
                PopulateProfilesDropdown(comboActiveProfile);
                lblCustomPresetNotice.Text = I18n.T("PresetCreated");
                lblCustomPresetNotice.Visibility = Visibility.Visible;
            };
            Grid.SetColumn(btnSaveCustomPreset, 1);
            inputRow.Children.Add(btnSaveCustomPreset);
            custSp.Children.Add(inputRow);

            lblCustomPresetNotice = new TextBlock();
            lblCustomPresetNotice.FontSize = 12;
            lblCustomPresetNotice.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            lblCustomPresetNotice.Margin = new Thickness(0, 8, 0, 0);
            lblCustomPresetNotice.Visibility = Visibility.Collapsed;
            custSp.Children.Add(lblCustomPresetNotice);

            customCard.Child = custSp;
            sp.Children.Add(customCard);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private void AddPresetCatalogCard(string name, string desc, double b, double c, double g, double t, double red, double green, double blue, Color tint) {
            Border card = CreateDashboardCard();
            card.Margin = new Thickness(6);

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Title & Tint preview bar
            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });

            TextBlock titleTb = new TextBlock();
            titleTb.Text = name;
            titleTb.FontSize = 14;
            titleTb.FontWeight = FontWeights.Bold;
            titleTb.Foreground = Brushes.White;
            topRow.Children.Add(titleTb);

            Border tintBox = new Border();
            tintBox.Height = 12;
            tintBox.CornerRadius = new CornerRadius(3);
            tintBox.Background = new SolidColorBrush(tint);
            Grid.SetColumn(tintBox, 1);
            topRow.Children.Add(tintBox);

            Grid.SetRow(topRow, 0);
            root.Children.Add(topRow);

            TextBlock descTb = new TextBlock();
            descTb.Text = desc;
            descTb.FontSize = 11;
            descTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            descTb.Margin = new Thickness(0, 4, 0, 10);
            Grid.SetRow(descTb, 1);
            root.Children.Add(descTb);

            // Metrics Row
            TextBlock metricsTb = new TextBlock();
            metricsTb.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Brightness: {0}%  ·  Contrast: {1}%  ·  Gamma: {2:F2}  ·  Temp: {3}K",
                (int)Math.Round(b * 100), (int)Math.Round(c * 100), g, (int)Math.Round(t));
            metricsTb.FontSize = 10;
            metricsTb.FontWeight = FontWeights.Medium;
            metricsTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            metricsTb.Margin = new Thickness(0, 0, 0, 12);
            Grid.SetRow(metricsTb, 2);
            root.Children.Add(metricsTb);

            // Buttons: Apply to All & Apply to Current
            Grid btnRow = new Grid();
            btnRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            btnRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });
            btnRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Button btnAll = new Button();
            btnAll.Style = darkPrimaryButtonStyle;
            btnAll.Content = I18n.T("BtnApplyAll");
            btnAll.Height = 32;
            btnAll.FontSize = 11;
            btnAll.Padding = new Thickness(8, 0, 8, 0);
            btnAll.Click += delegate {
                syncAllMonitors = true;
                config.SyncMonitors = true;
                ApplyPreset(b, c, g, t, red, green, blue, name);
                SwitchView(0);
            };
            Grid.SetColumn(btnAll, 0);
            btnRow.Children.Add(btnAll);

            Button btnSel = new Button();
            btnSel.Style = darkButtonStyle;
            btnSel.Content = I18n.T("BtnApplySelected");
            btnSel.Height = 32;
            btnSel.FontSize = 11;
            btnSel.Padding = new Thickness(8, 0, 8, 0);
            btnSel.Click += delegate {
                ApplyPreset(b, c, g, t, red, green, blue, name);
                SwitchView(0);
            };
            Grid.SetColumn(btnSel, 2);
            btnRow.Children.Add(btnSel);

            Grid.SetRow(btnRow, 3);
            root.Children.Add(btnRow);

            card.Child = root;
            presetsGrid.Children.Add(card);
        }
        #endregion

        #region View 4: Profiles Manager
        private Grid CreateProfilesView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 18, 24, 24);

            // 1. Header
            StackPanel titleSp = new StackPanel();
            titleSp.Margin = new Thickness(0, 0, 0, 18);

            txtProfilesTitle = new TextBlock();
            txtProfilesTitle.Text = I18n.T("ProfilesTitle");
            txtProfilesTitle.FontSize = 20;
            txtProfilesTitle.FontWeight = FontWeights.Bold;
            txtProfilesTitle.Foreground = Brushes.White;
            titleSp.Children.Add(txtProfilesTitle);

            txtProfilesSubtitle = new TextBlock();
            txtProfilesSubtitle.Text = I18n.T("ProfilesSubtitle");
            txtProfilesSubtitle.FontSize = 12;
            txtProfilesSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtProfilesSubtitle.Margin = new Thickness(0, 3, 0, 0);
            titleSp.Children.Add(txtProfilesSubtitle);
            sp.Children.Add(titleSp);

            // Active Profile Banner
            Border activeBanner = new Border();
            activeBanner.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
            activeBanner.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            activeBanner.BorderThickness = new Thickness(1);
            activeBanner.CornerRadius = new CornerRadius(8);
            activeBanner.Padding = new Thickness(16, 12, 16, 12);
            activeBanner.Margin = new Thickness(0, 0, 0, 16);

            lblActiveProfileBanner = new TextBlock();
            lblActiveProfileBanner.Text = string.Format("Active Profile: {0}", config.ActiveProfile);
            lblActiveProfileBanner.FontSize = 13;
            lblActiveProfileBanner.FontWeight = FontWeights.SemiBold;
            lblActiveProfileBanner.Foreground = Brushes.White;
            activeBanner.Child = lblActiveProfileBanner;
            sp.Children.Add(activeBanner);

            // 2. Profiles List Container
            profilesListContainer = new StackPanel();
            sp.Children.Add(profilesListContainer);
            RefreshProfilesList();

            // 3. Create New Profile Card
            Border newProfCard = CreateDashboardCard();
            newProfCard.Margin = new Thickness(0, 12, 0, 0);
            StackPanel npSp = new StackPanel();

            TextBlock npTitle = new TextBlock();
            npTitle.Text = I18n.T("NewProfileTitle");
            npTitle.FontSize = 14;
            npTitle.FontWeight = FontWeights.Bold;
            npTitle.Foreground = Brushes.White;
            npSp.Children.Add(npTitle);

            Grid npInputRow = new Grid();
            npInputRow.Margin = new Thickness(0, 10, 0, 0);
            npInputRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            npInputRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            txtNewProfileName = new TextBox();
            txtNewProfileName.Style = darkTextBoxStyle;
            txtNewProfileName.Text = "Custom Studio Mode";
            txtNewProfileName.Height = 36;
            txtNewProfileName.Margin = new Thickness(0, 0, 12, 0);
            Grid.SetColumn(txtNewProfileName, 0);
            npInputRow.Children.Add(txtNewProfileName);

            btnCreateProfile = new Button();
            btnCreateProfile.Style = darkPrimaryButtonStyle;
            btnCreateProfile.Content = I18n.T("NewProfileBtn");
            btnCreateProfile.Height = 36;
            btnCreateProfile.Padding = new Thickness(16, 0, 16, 0);
            btnCreateProfile.Click += delegate {
                string name = txtNewProfileName.Text.Trim();
                if (!string.IsNullOrEmpty(name)) {
                    profileManager.AddOrUpdate(name, globalSettings);
                    config.ActiveProfile = name;
                    config.Save();
                    PopulateProfilesDropdown(comboActiveProfile);
                    RefreshProfilesList();
                    lblProfileNotice.Text = I18n.T("ProfileSaved");
                    lblProfileNotice.Visibility = Visibility.Visible;
                }
            };
            Grid.SetColumn(btnCreateProfile, 1);
            npInputRow.Children.Add(btnCreateProfile);
            npSp.Children.Add(npInputRow);

            lblProfileNotice = new TextBlock();
            lblProfileNotice.FontSize = 12;
            lblProfileNotice.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            lblProfileNotice.Margin = new Thickness(0, 8, 0, 0);
            lblProfileNotice.Visibility = Visibility.Collapsed;
            npSp.Children.Add(lblProfileNotice);

            newProfCard.Child = npSp;
            sp.Children.Add(newProfCard);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private void RefreshProfilesList() {
            if (profilesListContainer == null) return;
            profilesListContainer.Children.Clear();
            if (lblActiveProfileBanner != null) {
                lblActiveProfileBanner.Text = string.Format("Active Profile: {0}", config.ActiveProfile);
            }

            foreach (UserProfile p in profileManager.Profiles) {
                UserProfile curP = p;
                Border card = CreateDashboardCard();
                card.Margin = new Thickness(0, 0, 0, 8);

                Grid g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                // Left: Name + Tag + Settings Summary
                StackPanel left = new StackPanel();

                StackPanel titleRow = new StackPanel();
                titleRow.Orientation = Orientation.Horizontal;

                TextBlock nameTb = new TextBlock();
                nameTb.Text = curP.Name;
                nameTb.FontSize = 14;
                nameTb.FontWeight = FontWeights.Bold;
                nameTb.Foreground = Brushes.White;
                nameTb.Margin = new Thickness(0, 0, 10, 0);
                titleRow.Children.Add(nameTb);

                Border tag = new Border();
                tag.CornerRadius = new CornerRadius(4);
                tag.Padding = new Thickness(6, 1, 6, 1);
                tag.Background = curP.IsBuiltIn ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(20, 83, 45));
                tag.BorderBrush = curP.IsBuiltIn ? new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(34, 197, 94));
                tag.BorderThickness = new Thickness(1);

                TextBlock tagTb = new TextBlock();
                tagTb.Text = curP.IsBuiltIn ? "Built-in" : "Custom";
                tagTb.FontSize = 10;
                tagTb.Foreground = curP.IsBuiltIn ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(34, 197, 94));
                tag.Child = tagTb;
                titleRow.Children.Add(tag);
                left.Children.Add(titleRow);

                TextBlock sumTb = new TextBlock();
                sumTb.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Brightness: {0}%  ·  Contrast: {1}%  ·  Gamma: {2:F2}  ·  Temp: {3}K",
                    (int)Math.Round(curP.Brightness * 100), (int)Math.Round(curP.Contrast * 100), curP.Gamma, (int)Math.Round(curP.Temperature));
                sumTb.FontSize = 11;
                sumTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                sumTb.Margin = new Thickness(0, 4, 0, 0);
                left.Children.Add(sumTb);

                Grid.SetColumn(left, 0);
                g.Children.Add(left);

                // Right: Actions (Activate, Overwrite, Delete)
                StackPanel act = new StackPanel();
                act.Orientation = Orientation.Horizontal;

                Button btnAct = new Button();
                btnAct.Style = darkPrimaryButtonStyle;
                btnAct.Content = I18n.T("BtnActivateProfile");
                btnAct.Height = 32;
                btnAct.FontSize = 11;
                btnAct.Padding = new Thickness(12, 0, 12, 0);
                btnAct.Margin = new Thickness(0, 0, 8, 0);
                btnAct.Click += delegate {
                    ApplyPreset(curP.Brightness, curP.Contrast, curP.Gamma, curP.Temperature, curP.Red, curP.Green, curP.Blue, curP.Name);
                    config.ActiveProfile = curP.Name;
                    config.Save();
                    PopulateProfilesDropdown(comboActiveProfile);
                    RefreshProfilesList();
                };
                act.Children.Add(btnAct);

                Button btnOver = new Button();
                btnOver.Style = darkButtonStyle;
                btnOver.Content = I18n.T("BtnSaveToProfile");
                btnOver.Height = 32;
                btnOver.FontSize = 11;
                btnOver.Padding = new Thickness(12, 0, 12, 0);
                btnOver.Margin = new Thickness(0, 0, 8, 0);
                btnOver.Click += delegate {
                    profileManager.AddOrUpdate(curP.Name, globalSettings);
                    RefreshProfilesList();
                };
                act.Children.Add(btnOver);

                if (!curP.IsBuiltIn) {
                    Button btnDel = new Button();
                    btnDel.Style = darkButtonStyle;
                    btnDel.Content = "🗑 " + I18n.T("BtnDeleteProfile");
                    btnDel.Height = 32;
                    btnDel.FontSize = 11;
                    btnDel.Padding = new Thickness(10, 0, 10, 0);
                    btnDel.Click += delegate {
                        profileManager.DeleteProfile(curP.Name);
                        PopulateProfilesDropdown(comboActiveProfile);
                        RefreshProfilesList();
                    };
                    act.Children.Add(btnDel);
                }

                Grid.SetColumn(act, 1);
                g.Children.Add(act);

                card.Child = g;
                profilesListContainer.Children.Add(card);
            }
        }
        #endregion

        #region View 5: Automated Scheduler
        private Grid CreateSchedulerView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 18, 24, 24);

            // 1. Header
            StackPanel titleSp = new StackPanel();
            titleSp.Margin = new Thickness(0, 0, 0, 18);

            txtSchedulerTitle = new TextBlock();
            txtSchedulerTitle.Text = I18n.T("SchedulerTitle");
            txtSchedulerTitle.FontSize = 20;
            txtSchedulerTitle.FontWeight = FontWeights.Bold;
            txtSchedulerTitle.Foreground = Brushes.White;
            titleSp.Children.Add(txtSchedulerTitle);

            txtSchedulerSubtitle = new TextBlock();
            txtSchedulerSubtitle.Text = I18n.T("SchedulerSubtitle");
            txtSchedulerSubtitle.FontSize = 12;
            txtSchedulerSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtSchedulerSubtitle.Margin = new Thickness(0, 3, 0, 0);
            titleSp.Children.Add(txtSchedulerSubtitle);
            sp.Children.Add(titleSp);

            // 2. Main Toggle Card
            Border toggleCard = CreateDashboardCard();
            toggleCard.Margin = new Thickness(0, 0, 0, 14);

            StackPanel togSp = new StackPanel();
            chkEnableScheduler = new CheckBox();
            chkEnableScheduler.IsChecked = config.SchedulerEnabled;
            chkEnableScheduler.Content = I18n.T("EnableScheduler");
            chkEnableScheduler.Foreground = Brushes.White;
            chkEnableScheduler.FontSize = 13;
            chkEnableScheduler.FontWeight = FontWeights.SemiBold;
            chkEnableScheduler.Cursor = Cursors.Hand;
            chkEnableScheduler.Checked += delegate { UpdateSchedulerUi(); };
            chkEnableScheduler.Unchecked += delegate { UpdateSchedulerUi(); };
            togSp.Children.Add(chkEnableScheduler);

            lblSchedulerStatus = new TextBlock();
            lblSchedulerStatus.FontSize = 12;
            lblSchedulerStatus.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            lblSchedulerStatus.Margin = new Thickness(0, 8, 0, 0);
            togSp.Children.Add(lblSchedulerStatus);

            toggleCard.Child = togSp;
            sp.Children.Add(toggleCard);

            // 3. Time Slots: Day, Sunset, Night
            sp.Children.Add(CreateScheduleSlotCard(I18n.T("SlotDay"), "☀", config.SlotDayHour, config.SlotDayMinute, config.SlotDayProfile, out comboDayHour, out comboDayMinute, out comboDayProfile));
            sp.Children.Add(CreateScheduleSlotCard(I18n.T("SlotSunset"), "🌅", config.SlotSunsetHour, config.SlotSunsetMinute, config.SlotSunsetProfile, out comboSunsetHour, out comboSunsetMinute, out comboSunsetProfile));
            sp.Children.Add(CreateScheduleSlotCard(I18n.T("SlotNight"), "🌙", config.SlotNightHour, config.SlotNightMinute, config.SlotNightProfile, out comboNightHour, out comboNightMinute, out comboNightProfile));

            // 4. Notification & Save Row
            Border footCard = CreateDashboardCard();
            footCard.Margin = new Thickness(0, 14, 0, 0);

            Grid footGrid = new Grid();
            footGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            footGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            chkSchedulerNotify = new CheckBox();
            chkSchedulerNotify.IsChecked = config.SchedulerNotify;
            chkSchedulerNotify.Content = I18n.T("NotifyOnShift");
            chkSchedulerNotify.Foreground = Brushes.White;
            chkSchedulerNotify.FontSize = 12;
            chkSchedulerNotify.VerticalAlignment = VerticalAlignment.Center;
            chkSchedulerNotify.Cursor = Cursors.Hand;
            Grid.SetColumn(chkSchedulerNotify, 0);
            footGrid.Children.Add(chkSchedulerNotify);

            btnSaveScheduler = new Button();
            btnSaveScheduler.Style = darkPrimaryButtonStyle;
            btnSaveScheduler.Content = I18n.T("BtnSaveScheduler");
            btnSaveScheduler.Height = 36;
            btnSaveScheduler.Padding = new Thickness(20, 0, 20, 0);
            btnSaveScheduler.Click += delegate {
                SaveSchedulerSettings();
            };
            Grid.SetColumn(btnSaveScheduler, 1);
            footGrid.Children.Add(btnSaveScheduler);

            footCard.Child = footGrid;
            sp.Children.Add(footCard);

            lblSchedulerSaveNotice = new TextBlock();
            lblSchedulerSaveNotice.FontSize = 12;
            lblSchedulerSaveNotice.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            lblSchedulerSaveNotice.Margin = new Thickness(4, 8, 0, 0);
            lblSchedulerSaveNotice.Visibility = Visibility.Collapsed;
            sp.Children.Add(lblSchedulerSaveNotice);

            scroll.Content = sp;
            g.Children.Add(scroll);
            UpdateSchedulerUi();
            return g;
        }

        private Border CreateScheduleSlotCard(string title, string icon, int defHour, int defMin, string defProfile, out ComboBox outHour, out ComboBox outMin, out ComboBox outProfile) {
            Border b = CreateDashboardCard();
            b.Margin = new Thickness(0, 0, 0, 10);

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Left: Icon + Title
            StackPanel left = new StackPanel();
            left.Orientation = Orientation.Horizontal;
            left.VerticalAlignment = VerticalAlignment.Center;

            TextBlock ic = new TextBlock();
            ic.Text = icon;
            ic.FontSize = 16;
            ic.Margin = new Thickness(0, 0, 10, 0);
            ic.VerticalAlignment = VerticalAlignment.Center;
            left.Children.Add(ic);

            TextBlock tt = new TextBlock();
            tt.Text = title;
            tt.FontSize = 13;
            tt.FontWeight = FontWeights.SemiBold;
            tt.Foreground = Brushes.White;
            tt.VerticalAlignment = VerticalAlignment.Center;
            left.Children.Add(tt);

            Grid.SetColumn(left, 0);
            g.Children.Add(left);

            // Right: Time Picker (HH:MM) + Profile Selector
            StackPanel right = new StackPanel();
            right.Orientation = Orientation.Horizontal;
            right.VerticalAlignment = VerticalAlignment.Center;

            TextBlock timeLbl = new TextBlock();
            timeLbl.Text = "Time:";
            timeLbl.FontSize = 12;
            timeLbl.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            timeLbl.VerticalAlignment = VerticalAlignment.Center;
            timeLbl.Margin = new Thickness(0, 0, 6, 0);
            right.Children.Add(timeLbl);

            ComboBox cbH = new ComboBox();
            cbH.Style = darkComboBoxStyle;
            cbH.Width = 60;
            cbH.Height = 30;
            for (int h = 0; h < 24; h++) cbH.Items.Add(string.Format("{0:D2}", h));
            cbH.SelectedIndex = Math.Max(0, Math.Min(23, defHour));
            right.Children.Add(cbH);
            outHour = cbH;

            TextBlock sep = new TextBlock();
            sep.Text = ":";
            sep.FontSize = 14;
            sep.FontWeight = FontWeights.Bold;
            sep.Foreground = Brushes.White;
            sep.VerticalAlignment = VerticalAlignment.Center;
            sep.Margin = new Thickness(4, 0, 4, 0);
            right.Children.Add(sep);

            ComboBox cbM = new ComboBox();
            cbM.Style = darkComboBoxStyle;
            cbM.Width = 60;
            cbM.Height = 30;
            for (int m = 0; m < 60; m += 5) cbM.Items.Add(string.Format("{0:D2}", m));
            int mIdx = defMin / 5;
            cbM.SelectedIndex = Math.Max(0, Math.Min(cbM.Items.Count - 1, mIdx));
            right.Children.Add(cbM);
            outMin = cbM;

            TextBlock profLbl = new TextBlock();
            profLbl.Text = "Profile:";
            profLbl.FontSize = 12;
            profLbl.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            profLbl.VerticalAlignment = VerticalAlignment.Center;
            profLbl.Margin = new Thickness(14, 0, 6, 0);
            right.Children.Add(profLbl);

            ComboBox cbP = new ComboBox();
            cbP.Style = darkComboBoxStyle;
            cbP.Width = 140;
            cbP.Height = 30;
            foreach (UserProfile up in profileManager.Profiles) {
                cbP.Items.Add(up.Name);
            }
            int pIdx = cbP.Items.IndexOf(defProfile);
            cbP.SelectedIndex = (pIdx >= 0) ? pIdx : 0;
            right.Children.Add(cbP);
            outProfile = cbP;

            Grid.SetColumn(right, 1);
            g.Children.Add(right);

            b.Child = g;
            return b;
        }

        private void UpdateSchedulerUi() {
            if (lblSchedulerStatus == null) return;
            if (chkEnableScheduler.IsChecked == true) {
                string currentSlot = GetCurrentScheduledSlotName();
                lblSchedulerStatus.Text = string.Format("● {0} (Active now: {1})", I18n.T("SchedulerStatusActive"), currentSlot);
                lblSchedulerStatus.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            } else {
                lblSchedulerStatus.Text = "○ Automatic scheduler is paused.";
                lblSchedulerStatus.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            }
        }

        private void SaveSchedulerSettings() {
            config.SchedulerEnabled = (chkEnableScheduler.IsChecked == true);
            config.SchedulerNotify = (chkSchedulerNotify.IsChecked == true);

            int h, m;
            if (int.TryParse(comboDayHour.SelectedItem.ToString(), out h)) config.SlotDayHour = h;
            if (int.TryParse(comboDayMinute.SelectedItem.ToString(), out m)) config.SlotDayMinute = m;
            if (comboDayProfile.SelectedItem != null) config.SlotDayProfile = comboDayProfile.SelectedItem.ToString();

            if (int.TryParse(comboSunsetHour.SelectedItem.ToString(), out h)) config.SlotSunsetHour = h;
            if (int.TryParse(comboSunsetMinute.SelectedItem.ToString(), out m)) config.SlotSunsetMinute = m;
            if (comboSunsetProfile.SelectedItem != null) config.SlotSunsetProfile = comboSunsetProfile.SelectedItem.ToString();

            if (int.TryParse(comboNightHour.SelectedItem.ToString(), out h)) config.SlotNightHour = h;
            if (int.TryParse(comboNightMinute.SelectedItem.ToString(), out m)) config.SlotNightMinute = m;
            if (comboNightProfile.SelectedItem != null) config.SlotNightProfile = comboNightProfile.SelectedItem.ToString();

            config.Save();
            UpdateSchedulerUi();

            lblSchedulerSaveNotice.Text = I18n.T("ScheduleSaved");
            lblSchedulerSaveNotice.Visibility = Visibility.Visible;

            if (config.SchedulerEnabled) {
                CheckSchedulerTick();
            }
        }

        private void InitSchedulerTimer() {
            schedulerTimer = new DispatcherTimer();
            schedulerTimer.Interval = TimeSpan.FromSeconds(20);
            schedulerTimer.Tick += delegate {
                if (config.SchedulerEnabled) {
                    CheckSchedulerTick();
                }
            };
            schedulerTimer.Start();
        }

        private string GetCurrentScheduledSlotName() {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan day = new TimeSpan(config.SlotDayHour, config.SlotDayMinute, 0);
            TimeSpan sunset = new TimeSpan(config.SlotSunsetHour, config.SlotSunsetMinute, 0);
            TimeSpan night = new TimeSpan(config.SlotNightHour, config.SlotNightMinute, 0);

            if (now >= day && now < sunset) return "Day Mode (" + config.SlotDayProfile + ")";
            if (now >= sunset && now < night) return "Sunset Mode (" + config.SlotSunsetProfile + ")";
            return "Night Mode (" + config.SlotNightProfile + ")";
        }

        private void CheckSchedulerTick() {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan day = new TimeSpan(config.SlotDayHour, config.SlotDayMinute, 0);
            TimeSpan sunset = new TimeSpan(config.SlotSunsetHour, config.SlotSunsetMinute, 0);
            TimeSpan night = new TimeSpan(config.SlotNightHour, config.SlotNightMinute, 0);

            string targetSlot = "Day";
            string targetProfile = config.SlotDayProfile;

            if (now >= day && now < sunset) {
                targetSlot = "Day";
                targetProfile = config.SlotDayProfile;
            } else if (now >= sunset && now < night) {
                targetSlot = "Sunset";
                targetProfile = config.SlotSunsetProfile;
            } else {
                targetSlot = "Night";
                targetProfile = config.SlotNightProfile;
            }

            if (targetSlot != lastAppliedScheduleSlot) {
                lastAppliedScheduleSlot = targetSlot;
                UserProfile up = profileManager.Find(targetProfile);
                if (up != null) {
                    ApplyPreset(up.Brightness, up.Contrast, up.Gamma, up.Temperature, up.Red, up.Green, up.Blue, up.Name);
                    config.ActiveProfile = up.Name;
                    config.Save();
                    PopulateProfilesDropdown(comboActiveProfile);
                    UpdateSchedulerUi();

                    if (config.SchedulerNotify && trayIcon != null) {
                        trayIcon.ShowBalloonTip(3000, "Lucent Display Control", "Scheduled transition applied: " + targetProfile, SWF.ToolTipIcon.Info);
                    }
                }
            }
        }
        #endregion

        #region View 6: Settings (Windows 11 Style)
        private Grid CreateSettingsView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 18, 24, 24);

            // Settings Header
            StackPanel headerSp = new StackPanel();
            headerSp.Margin = new Thickness(0, 0, 0, 20);

            txtSettingsTitle = new TextBlock();
            txtSettingsTitle.Text = I18n.T("SettingsTitle");
            txtSettingsTitle.FontSize = 20;
            txtSettingsTitle.FontWeight = FontWeights.Bold;
            txtSettingsTitle.Foreground = Brushes.White;
            headerSp.Children.Add(txtSettingsTitle);

            txtSettingsSubtitle = new TextBlock();
            txtSettingsSubtitle.Text = I18n.T("SettingsSubtitle");
            txtSettingsSubtitle.FontSize = 12;
            txtSettingsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtSettingsSubtitle.Margin = new Thickness(0, 3, 0, 0);
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
            comboLanguage.Style = darkComboBoxStyle;
            comboLanguage.Height = 36;
            comboLanguage.VerticalAlignment = VerticalAlignment.Center;

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
            txtStartWinDesc.Margin = new Thickness(0, 4, 16, 0);
            startInfo.Children.Add(txtStartWinDesc);

            Grid.SetColumn(startInfo, 0);
            startRow.Children.Add(startInfo);

            chkStartWithWindows = new CheckBox();
            chkStartWithWindows.IsChecked = config.StartWithWindows;
            chkStartWithWindows.VerticalAlignment = VerticalAlignment.Center;
            chkStartWithWindows.Cursor = Cursors.Hand;
            chkStartWithWindows.Checked += delegate { UpdateStartupState(); };
            chkStartWithWindows.Unchecked += delegate { UpdateStartupState(); };
            Grid.SetColumn(chkStartWithWindows, 1);
            startRow.Children.Add(chkStartWithWindows);
            startupSp.Children.Add(startRow);

            // Indented Subcard: Start Minimized
            subCardStartMinimized = new Border();
            subCardStartMinimized.Margin = new Thickness(24, 14, 0, 0);
            subCardStartMinimized.Padding = new Thickness(12, 10, 12, 10);
            subCardStartMinimized.Background = new SolidColorBrush(Color.FromRgb(16, 20, 30));
            subCardStartMinimized.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 38, 54));
            subCardStartMinimized.BorderThickness = new Thickness(1);
            subCardStartMinimized.CornerRadius = new CornerRadius(6);

            Grid subRow = new Grid();
            subRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            subRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel subInfo = new StackPanel();
            txtStartMinTitle = new TextBlock();
            txtStartMinTitle.Text = I18n.T("StartMinTitle");
            txtStartMinTitle.FontSize = 13;
            txtStartMinTitle.FontWeight = FontWeights.Medium;
            txtStartMinTitle.Foreground = Brushes.White;
            subInfo.Children.Add(txtStartMinTitle);

            txtStartMinDesc = new TextBlock();
            txtStartMinDesc.Text = I18n.T("StartMinDesc");
            txtStartMinDesc.FontSize = 11;
            txtStartMinDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtStartMinDesc.Margin = new Thickness(0, 2, 16, 0);
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
            startupCard.Child = startupSp;
            sp.Children.Add(startupCard);

            // 3. Minimize to Tray on Close Card
            Border closeCard = CreateDashboardCard();
            closeCard.Margin = new Thickness(0, 0, 0, 12);

            Grid closeRow = new Grid();
            closeRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            closeRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

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
            txtMinCloseDesc.Margin = new Thickness(0, 4, 16, 0);
            closeInfo.Children.Add(txtMinCloseDesc);

            Grid.SetColumn(closeInfo, 0);
            closeRow.Children.Add(closeInfo);

            chkMinimizeOnClose = new CheckBox();
            chkMinimizeOnClose.IsChecked = config.MinimizeOnClose;
            chkMinimizeOnClose.VerticalAlignment = VerticalAlignment.Center;
            chkMinimizeOnClose.Cursor = Cursors.Hand;
            Grid.SetColumn(chkMinimizeOnClose, 1);
            closeRow.Children.Add(chkMinimizeOnClose);
            closeCard.Child = closeRow;
            sp.Children.Add(closeCard);

            // 4. About Card with Version 0.1.0
            Border aboutCard = CreateDashboardCard();
            aboutCard.Margin = new Thickness(0, 0, 0, 18);
            StackPanel aboutSp = new StackPanel();

            txtAboutTitle = new TextBlock();
            txtAboutTitle.Text = I18n.T("AboutTitle");
            txtAboutTitle.FontSize = 14;
            txtAboutTitle.FontWeight = FontWeights.SemiBold;
            txtAboutTitle.Foreground = Brushes.White;
            aboutSp.Children.Add(txtAboutTitle);

            txtAboutVersion = new TextBlock();
            txtAboutVersion.Text = I18n.T("AboutVersion");
            txtAboutVersion.FontSize = 13;
            txtAboutVersion.FontWeight = FontWeights.Bold;
            txtAboutVersion.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            txtAboutVersion.Margin = new Thickness(0, 4, 0, 4);
            aboutSp.Children.Add(txtAboutVersion);

            txtAboutDesc = new TextBlock();
            txtAboutDesc.Text = I18n.T("AboutDesc");
            txtAboutDesc.FontSize = 12;
            txtAboutDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtAboutDesc.TextWrapping = TextWrapping.Wrap;
            aboutSp.Children.Add(txtAboutDesc);

            aboutCard.Child = aboutSp;
            sp.Children.Add(aboutCard);

            // 5. Action Buttons (Apply, OK, Cancel)
            Grid btnGrid = new Grid();
            btnGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            btnGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            lblSettingsSaveNotice = new TextBlock();
            lblSettingsSaveNotice.Text = I18n.T("SavedNotice");
            lblSettingsSaveNotice.FontSize = 12;
            lblSettingsSaveNotice.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            lblSettingsSaveNotice.VerticalAlignment = VerticalAlignment.Center;
            lblSettingsSaveNotice.Visibility = Visibility.Collapsed;
            Grid.SetColumn(lblSettingsSaveNotice, 0);
            btnGrid.Children.Add(lblSettingsSaveNotice);

            StackPanel btnRow = new StackPanel();
            btnRow.Orientation = Orientation.Horizontal;

            btnSettingsApply = new Button();
            btnSettingsApply.Style = darkPrimaryButtonStyle;
            btnSettingsApply.Content = I18n.T("BtnApply");
            btnSettingsApply.Height = 36;
            btnSettingsApply.Padding = new Thickness(18, 0, 18, 0);
            btnSettingsApply.Margin = new Thickness(0, 0, 8, 0);
            btnSettingsApply.Click += delegate {
                SaveSettingsState();
                lblSettingsSaveNotice.Visibility = Visibility.Visible;
            };
            btnRow.Children.Add(btnSettingsApply);

            btnSettingsOK = new Button();
            btnSettingsOK.Style = darkButtonStyle;
            btnSettingsOK.Content = I18n.T("BtnOK");
            btnSettingsOK.Height = 36;
            btnSettingsOK.Padding = new Thickness(18, 0, 18, 0);
            btnSettingsOK.Margin = new Thickness(0, 0, 8, 0);
            btnSettingsOK.Click += delegate {
                SaveSettingsState();
                SwitchView(0);
            };
            btnRow.Children.Add(btnSettingsOK);

            btnSettingsCancel = new Button();
            btnSettingsCancel.Style = darkButtonStyle;
            btnSettingsCancel.Content = I18n.T("BtnCancel");
            btnSettingsCancel.Height = 36;
            btnSettingsCancel.Padding = new Thickness(18, 0, 18, 0);
            btnSettingsCancel.Click += delegate {
                I18n.CurrentLanguage = I18n.CommittedLanguage;
                comboLanguage.SelectedIndex = (int)I18n.CurrentLanguage;
                UpdateAllTexts();
                SwitchView(0);
            };
            btnRow.Children.Add(btnSettingsCancel);

            Grid.SetColumn(btnRow, 1);
            btnGrid.Children.Add(btnRow);
            sp.Children.Add(btnGrid);

            scroll.Content = sp;
            g.Children.Add(scroll);
            UpdateStartupState();
            return g;
        }

        private void UpdateStartupState() {
            if (subCardStartMinimized == null) return;
            bool enabled = (chkStartWithWindows.IsChecked == true);
            subCardStartMinimized.IsEnabled = enabled;
            subCardStartMinimized.Opacity = enabled ? 1.0 : 0.45;
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

            // Nav buttons
            btnNavDashboard.Content = CreateSidebarBtnContent("\uE80F", I18n.T("NavDashboard"));
            btnNavMonitors.Content = CreateSidebarBtnContent("\uE7F4", I18n.T("NavMonitors"));
            btnNavPresets.Content = CreateSidebarBtnContent("\uE74C", I18n.T("NavPresets"));
            btnNavProfiles.Content = CreateSidebarBtnContent("\uE77B", I18n.T("NavProfiles"));
            btnNavScheduler.Content = CreateSidebarBtnContent("\uE823", I18n.T("NavScheduler"));
            btnNavSettings.Content = CreateSidebarBtnContent("\uE713", I18n.T("NavSettings"));

            // Dashboard
            mainTitle.Text = I18n.T("MonitorsTitle");
            mainSubtitle.Text = I18n.T("MonitorsSubtitle");

            SetHeaderActionBtnText(btnIdentify, "\uE946", I18n.T("Identify"));
            SetHeaderActionBtnText(btnMinimizeToTray, "\uE921", I18n.T("MinimizeToTray"));

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

            SetHeaderActionBtnText(btnResetDefault, "\uE72C", I18n.T("ResetToDefault"));

            btnPresetWarm.Content = I18n.T("PresetWarm");
            btnPresetNeutral.Content = I18n.T("PresetNeutral");
            btnPresetCool.Content = I18n.T("PresetCool");
            btnPresetMovie.Content = I18n.T("PresetMovie");
            btnPresetWork.Content = I18n.T("PresetWork");
            btnPresetNight.Content = I18n.T("PresetNight");

            string targetName = syncAllMonitors ? I18n.T("AllDisplays") : (selectedMonitorIndex < screenDevices.Count ? screenDevices[selectedMonitorIndex].DisplayLabel : "Monitor");
            SetHeaderActionBtnText(btnApplyToMonitor, "\u2713", I18n.T("ApplyToMonitor", targetName));

            // Monitors Tab
            if (txtMonHardwareTitle != null) txtMonHardwareTitle.Text = I18n.T("MonHardwareTitle");
            if (txtMonHardwareSubtitle != null) txtMonHardwareSubtitle.Text = I18n.T("MonHardwareSubtitle");
            if (txtCalibrationTitle != null) txtCalibrationTitle.Text = I18n.T("CalibrationTitle");
            if (txtCalibrationDesc != null) txtCalibrationDesc.Text = I18n.T("CalibrationDesc");
            if (btnRefreshDisplays != null) SetHeaderActionBtnText(btnRefreshDisplays, "\uE72C", I18n.T("RefreshMonitors"));
            if (btnIdentifyFromMon != null) SetHeaderActionBtnText(btnIdentifyFromMon, "\uE946", I18n.T("Identify"));

            // Presets Tab
            if (txtPresetsTitle != null) txtPresetsTitle.Text = I18n.T("PresetsTitle");
            if (txtPresetsSubtitle != null) txtPresetsSubtitle.Text = I18n.T("PresetsSubtitle");

            // Profiles Tab
            if (txtProfilesTitle != null) txtProfilesTitle.Text = I18n.T("ProfilesTitle");
            if (txtProfilesSubtitle != null) txtProfilesSubtitle.Text = I18n.T("ProfilesSubtitle");
            if (btnCreateProfile != null) btnCreateProfile.Content = I18n.T("NewProfileBtn");

            // Scheduler Tab
            if (txtSchedulerTitle != null) txtSchedulerTitle.Text = I18n.T("SchedulerTitle");
            if (txtSchedulerSubtitle != null) txtSchedulerSubtitle.Text = I18n.T("SchedulerSubtitle");
            if (chkEnableScheduler != null) chkEnableScheduler.Content = I18n.T("EnableScheduler");
            if (chkSchedulerNotify != null) chkSchedulerNotify.Content = I18n.T("NotifyOnShift");
            if (btnSaveScheduler != null) btnSaveScheduler.Content = I18n.T("BtnSaveScheduler");

            // Settings Tab
            if (txtSettingsTitle != null) txtSettingsTitle.Text = I18n.T("SettingsTitle");
            if (txtSettingsSubtitle != null) txtSettingsSubtitle.Text = I18n.T("SettingsSubtitle");
            if (txtLangTitle != null) txtLangTitle.Text = I18n.T("LangTitle");
            if (txtLangDesc != null) txtLangDesc.Text = I18n.T("LangDesc");
            if (txtStartWinTitle != null) txtStartWinTitle.Text = I18n.T("StartWinTitle");
            if (txtStartWinDesc != null) txtStartWinDesc.Text = I18n.T("StartWinDesc");
            if (txtStartMinTitle != null) txtStartMinTitle.Text = I18n.T("StartMinTitle");
            if (txtStartMinDesc != null) txtStartMinDesc.Text = I18n.T("StartMinDesc");
            if (txtMinCloseTitle != null) txtMinCloseTitle.Text = I18n.T("MinCloseTitle");
            if (txtMinCloseDesc != null) txtMinCloseDesc.Text = I18n.T("MinCloseDesc");
            if (txtAboutTitle != null) txtAboutTitle.Text = I18n.T("AboutTitle");
            if (txtAboutVersion != null) txtAboutVersion.Text = I18n.T("AboutVersion");
            if (txtAboutDesc != null) txtAboutDesc.Text = I18n.T("AboutDesc");
            if (btnSettingsApply != null) btnSettingsApply.Content = I18n.T("BtnApply");
            if (btnSettingsOK != null) btnSettingsOK.Content = I18n.T("BtnOK");
            if (btnSettingsCancel != null) btnSettingsCancel.Content = I18n.T("BtnCancel");
            if (lblSettingsSaveNotice != null) lblSettingsSaveNotice.Text = I18n.T("SavedNotice");

            UpdateTrayMenu();
        }

        private StackPanel CreateSidebarBtnContent(string icon, string label) {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock iTb = new TextBlock();
            iTb.Text = icon;
            iTb.FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
            iTb.FontSize = 15;
            iTb.Width = 24;
            iTb.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(iTb);

            TextBlock lTb = new TextBlock();
            lTb.Text = label;
            lTb.FontSize = 13;
            lTb.FontWeight = FontWeights.Medium;
            lTb.Margin = new Thickness(8, 0, 0, 0);
            lTb.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(lTb);
            return sp;
        }

        private void OnControlChanged() {
            if (isUpdatingUi) return;

            DisplaySettings curS = GetCurrentWorkingSettings();
            curS.Brightness = sliderBrightness.Value;
            curS.Contrast = sliderContrast.Value;
            curS.Gamma = sliderGamma.Value;
            curS.Temperature = sliderColorTemp.Value;
            curS.Red = sliderRed.Value;
            curS.Green = sliderGreen.Value;
            curS.Blue = sliderBlue.Value;

            if (syncAllMonitors) {
                globalSettings = curS.Clone();
                foreach (var kvp in settingsPerMonitor) {
                    kvp.Value.Brightness = curS.Brightness;
                    kvp.Value.Contrast = curS.Contrast;
                    kvp.Value.Gamma = curS.Gamma;
                    kvp.Value.Temperature = curS.Temperature;
                    kvp.Value.Red = curS.Red;
                    kvp.Value.Green = curS.Green;
                    kvp.Value.Blue = curS.Blue;
                }
            }

            ApplyAllActiveSettings();
        }

        private DisplaySettings GetCurrentWorkingSettings() {
            if (syncAllMonitors) return globalSettings;
            if (selectedMonitorIndex < screenDevices.Count) {
                return settingsPerMonitor[screenDevices[selectedMonitorIndex].DeviceName];
            }
            return globalSettings;
        }

        private void UpdateUiFromSettings() {
            isUpdatingUi = true;
            DisplaySettings s = GetCurrentWorkingSettings();

            sliderBrightness.Value = s.Brightness;
            badgeBrightness.Text = string.Format("{0}%", (int)Math.Round(s.Brightness * 100));

            sliderContrast.Value = s.Contrast;
            badgeContrast.Text = string.Format("{0}%", (int)Math.Round(s.Contrast * 100));

            sliderGamma.Value = s.Gamma;
            badgeGamma.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F2}", s.Gamma);
            HighlightGammaTick();

            sliderColorTemp.Value = s.Temperature;
            badgeColorTemp.Text = string.Format("{0} K", (int)Math.Round(s.Temperature));

            sliderRed.Value = s.Red;
            badgeRed.Text = string.Format("{0}", (int)Math.Round(s.Red * 100));

            sliderGreen.Value = s.Green;
            badgeGreen.Text = string.Format("{0}", (int)Math.Round(s.Green * 100));

            sliderBlue.Value = s.Blue;
            badgeBlue.Text = string.Format("{0}", (int)Math.Round(s.Blue * 100));

            isUpdatingUi = false;
        }

        private void ApplyPreset(double b, double c, double g, double t, double red, double green, double blue, string profName) {
            isUpdatingUi = true;
            DisplaySettings curS = GetCurrentWorkingSettings();
            curS.Brightness = b;
            curS.Contrast = c;
            curS.Gamma = g;
            curS.Temperature = t;
            curS.Red = red;
            curS.Green = green;
            curS.Blue = blue;
            curS.ProfileName = profName;

            if (syncAllMonitors) {
                globalSettings = curS.Clone();
                foreach (var kvp in settingsPerMonitor) {
                    kvp.Value.Brightness = b;
                    kvp.Value.Contrast = c;
                    kvp.Value.Gamma = g;
                    kvp.Value.Temperature = t;
                    kvp.Value.Red = red;
                    kvp.Value.Green = green;
                    kvp.Value.Blue = blue;
                    kvp.Value.ProfileName = profName;
                }
            }

            UpdateUiFromSettings();
            isUpdatingUi = false;
            ApplyAllActiveSettings();

            config.ActiveProfile = profName;
            config.Brightness = b;
            config.Contrast = c;
            config.Gamma = g;
            config.Temperature = t;
            config.Red = red;
            config.Green = green;
            config.Blue = blue;
            config.Save();

            int idx = comboActiveProfile.Items.IndexOf(profName);
            if (idx >= 0) comboActiveProfile.SelectedIndex = idx;
        }

        private void ApplyAllActiveSettings() {
            DisplaySettings s = GetCurrentWorkingSettings();

            // 1. DWM Fullscreen Hardware Effect for Contrast, Gamma, Kelvin and Brightness
            DwmEngine.Apply(s);

            // 2. Overlay Layer for per-monitor dimming & tinting
            if (syncAllMonitors) {
                overlayManager.UpdateAll(s);
            } else if (selectedMonitorIndex < screenDevices.Count) {
                overlayManager.UpdateDisplay(screenDevices[selectedMonitorIndex].DeviceName, s);
            }

            // 3. Hardware GDI Gamma Ramp where supported
            if (syncAllMonitors) {
                byte[] ramp = GammaMath.BuildGammaRamp(globalSettings);
                foreach (ScreenDevice sd in screenDevices) {
                    ApplyRampToDevice(sd.DeviceName, ramp);
                }
            } else if (selectedMonitorIndex < screenDevices.Count) {
                string dev = screenDevices[selectedMonitorIndex].DeviceName;
                byte[] ramp = GammaMath.BuildGammaRamp(settingsPerMonitor[dev]);
                ApplyRampToDevice(dev, ramp);
            }
        }

        private void ApplyRampToDevice(string deviceName, byte[] ramp) {
            try {
                IntPtr hdc = NativeMethods.CreateDC(null, deviceName, null, IntPtr.Zero);
                if (hdc != IntPtr.Zero) {
                    NativeMethods.SetDeviceGammaRamp(hdc, ramp);
                    NativeMethods.DeleteDC(hdc);
                }
            } catch { }
        }

        private void ResetCurrentOrAll() {
            DwmEngine.Reset();
            if (syncAllMonitors) {
                globalSettings.ResetToDefaults();
                foreach (ScreenDevice sd in screenDevices) {
                    DisplaySettings s = settingsPerMonitor[sd.DeviceName];
                    s.ResetToDefaults();
                    overlayManager.UpdateDisplay(sd.DeviceName, s);
                    if (sd.OriginalRamp != null) ApplyRampToDevice(sd.DeviceName, sd.OriginalRamp);
                }
            } else if (selectedMonitorIndex < screenDevices.Count) {
                ScreenDevice sd = screenDevices[selectedMonitorIndex];
                DisplaySettings s = settingsPerMonitor[sd.DeviceName];
                s.ResetToDefaults();
                overlayManager.UpdateDisplay(sd.DeviceName, s);
                if (sd.OriginalRamp != null) ApplyRampToDevice(sd.DeviceName, sd.OriginalRamp);
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
            menu.MenuItems.Add(I18n.T("TraySettings"), delegate { RestoreWindow(); SwitchView(5); });
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(I18n.T("PresetWarm"), delegate { ApplyPreset(0.85, 0.95, 1.05, 4500, 1.0, 0.95, 0.85, "Warm"); });
            menu.MenuItems.Add(I18n.T("PresetNeutral"), delegate { ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0, "Standard"); });
            menu.MenuItems.Add(I18n.T("PresetCool"), delegate { ApplyPreset(1.0, 1.10, 1.0, 8000, 0.92, 0.96, 1.0, "Cool"); });
            menu.MenuItems.Add(I18n.T("PresetMovie"), delegate { ApplyPreset(0.90, 1.15, 1.15, 5800, 1.0, 0.95, 0.90, "Movie"); });
            menu.MenuItems.Add(I18n.T("PresetWork"), delegate { ApplyPreset(0.80, 1.0, 1.0, 6000, 1.0, 1.0, 0.96, "Work"); });
            menu.MenuItems.Add(I18n.T("PresetNight"), delegate { ApplyPreset(0.65, 0.90, 1.0, 3400, 1.0, 0.84, 0.68, "Night"); });
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
            if (schedulerTimer != null) {
                schedulerTimer.Stop();
            }
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
