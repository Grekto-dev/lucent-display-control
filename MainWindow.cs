using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;
using SD = System.Drawing;
using SWF = System.Windows.Forms;

namespace LucentControl {
    public partial class MainWindow : Window {
        private AppConfig config;
        private ProfileManager profileManager;
        private List<ScreenDevice> screenDevices;
        private Dictionary<string, DisplaySettings> settingsPerMonitor;
        private DisplaySettings globalSettings;
        private bool syncAllMonitors = false;
        private int selectedMonitorIndex = 0;
        private string activeProfileName = "Standard";
        private bool hasCustomProfileAssigned = false;
        private bool hasUnsavedChanges = false;

        private OverlayManager overlayManager;
        private SWF.NotifyIcon trayIcon;
        private DispatcherTimer schedulerTimer;
        private string lastAppliedScheduleSlot = "";

        // Custom Styles
        private Style darkButtonStyle;
        private Style darkPrimaryButtonStyle;
        private Style darkDangerButtonStyle;
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
        public ScrollViewer scrollSettings;
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
        private List<Border> editableBadgeBorders = new List<Border>();
        private List<Action> cancelActiveBadgeEdits = new List<Action>();
        private NativeMethods.WinEventDelegate winEventDelegate;
        private IntPtr winEventHook = IntPtr.Zero;
        private DispatcherTimer shellReapplyTimer;
        private TextBlock brightTick0;
        private Border brightTick100Badge;
        private TextBlock brightTick100Text;
        private TextBlock brightTick200;
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
        private Button btnSaveProfile;
        private Button btnSaveAsNewProfile;
        private TextBlock lblUnsavedBadge;
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

        // Settings: Auto-Save, Startup Profile, Presets & Data
        private TextBlock txtAutoSaveTitle;
        private TextBlock txtAutoSaveDesc;
        private CheckBox chkAutoSave;
        private TextBlock txtAutoApplyTitle;
        private TextBlock txtAutoApplyDesc;
        private CheckBox chkAutoApply;
        private Border subCardStartupProfile;
        private TextBlock txtStartupProfileLabel;
        private ComboBox comboStartupProfile;
        private TextBlock txtDataSectionTitle;
        private TextBlock txtRestoreTitle;
        private TextBlock txtRestoreDesc;
        private Button btnRestoreDefaults;
        private TextBlock txtEraseTitle;
        private TextBlock txtEraseDesc;
        private Button btnEraseAll;

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

            // Load startup profile if auto-apply is enabled, otherwise Standard
            string targetProfile = "Standard";
            if (config.AutoApplyProfile && !string.IsNullOrEmpty(config.StartupProfile)) {
                targetProfile = config.StartupProfile;
            }
            UserProfile up = profileManager.Find(targetProfile) ?? profileManager.Find("Standard");
            if (up != null) {
                globalSettings.CopyFrom(up.ToSettings());
                activeProfileName = up.Name;
                hasCustomProfileAssigned = !up.IsBuiltIn;
            } else {
                activeProfileName = "Standard";
                hasCustomProfileAssigned = false;
            }

            DetectMonitors();
            overlayManager = new OverlayManager();

            LoadCustomStyles();
            InitializeComponent();
            SetupTrayIcon();
            InitSchedulerTimer();

            Loaded += delegate {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.EnableDarkMode(hwnd);
                InitShellWatcher();
                ApplyAllActiveSettings();

                if (config.SchedulerEnabled) {
                    CheckSchedulerTick();
                }

                if (startMinimized) {
                    Hide();
                    NativeMethods.TrimWorkingSet();
                }
            };
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

                    <!-- Dark Danger Accent Button -->
                    <Style x:Key='DarkDangerButton' TargetType='Button'>
                        <Setter Property='Background' Value='#FFDC2626'/>
                        <Setter Property='Foreground' Value='#FFFFFFFF'/>
                        <Setter Property='BorderBrush' Value='#FFEF4444'/>
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
                                            <Setter TargetName='border' Property='Background' Value='#FFEF4444'/>
                                            <Setter TargetName='border' Property='BorderBrush' Value='#FFF87171'/>
                                        </Trigger>
                                        <Trigger Property='IsPressed' Value='True'>
                                            <Setter TargetName='border' Property='Background' Value='#FFB91C1C'/>
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

                    <!-- Transparent Slider Track RepeatButton -->
                    <Style x:Key='SliderRepeatButtonStyle' TargetType='RepeatButton'>
                        <Setter Property='OverridesDefaultStyle' Value='True'/>
                        <Setter Property='IsTabStop' Value='False'/>
                        <Setter Property='Focusable' Value='False'/>
                        <Setter Property='Template'>
                            <Setter.Value>
                                <ControlTemplate TargetType='RepeatButton'>
                                    <Border Background='Transparent' Height='28' Cursor='Hand'/>
                                </ControlTemplate>
                            </Setter.Value>
                        </Setter>
                    </Style>

                    <!-- Modern Blue Slider -->
                    <ControlTemplate x:Key='BlueSliderTemplate' TargetType='Slider'>
                        <Grid VerticalAlignment='Center' Height='28'>
                            <Border Height='6' CornerRadius='3' Background='#FF222634'/>
                            <Track x:Name='PART_Track'>
                                <Track.DecreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.DecreaseLarge'/>
                                </Track.DecreaseRepeatButton>
                                <Track.IncreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.IncreaseLarge'/>
                                </Track.IncreaseRepeatButton>
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
                                <Track.DecreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.DecreaseLarge'/>
                                </Track.DecreaseRepeatButton>
                                <Track.IncreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.IncreaseLarge'/>
                                </Track.IncreaseRepeatButton>
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
                                <Track.DecreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.DecreaseLarge'/>
                                </Track.DecreaseRepeatButton>
                                <Track.IncreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.IncreaseLarge'/>
                                </Track.IncreaseRepeatButton>
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
                                <Track.DecreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.DecreaseLarge'/>
                                </Track.DecreaseRepeatButton>
                                <Track.IncreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.IncreaseLarge'/>
                                </Track.IncreaseRepeatButton>
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
                                <Track.DecreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.DecreaseLarge'/>
                                </Track.DecreaseRepeatButton>
                                <Track.IncreaseRepeatButton>
                                    <RepeatButton Style='{StaticResource SliderRepeatButtonStyle}' Command='Slider.IncreaseLarge'/>
                                </Track.IncreaseRepeatButton>
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
                darkDangerButtonStyle = (Style)dict["DarkDangerButton"];
                darkComboBoxStyle = (Style)dict["DarkComboBox"];
                darkTextBoxStyle = (Style)dict["DarkTextBox"];

                Theme.Button = darkButtonStyle;
                Theme.PrimaryButton = darkPrimaryButtonStyle;
                Theme.DangerButton = darkDangerButtonStyle;
                Theme.ComboBox = darkComboBoxStyle;
                Theme.TextBox = darkTextBoxStyle;

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
            Icon = AppIcon.Wpf();

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

            tb.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            tb.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            tb.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Left: Logo + App Name
            StackPanel leftSp = new StackPanel();
            leftSp.Orientation = Orientation.Horizontal;
            leftSp.Margin = new Thickness(16, 0, 0, 0);
            leftSp.VerticalAlignment = VerticalAlignment.Center;

            Image logoImg = new Image();
            logoImg.Source = AppIcon.Wpf(24);
            logoImg.Width = 22;
            logoImg.Height = 22;
            logoImg.Margin = new Thickness(0, 0, 10, 0);
            leftSp.Children.Add(logoImg);

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

            // Right: Standard Window Chrome Buttons (Minimize, Maximize, Close)
            StackPanel rightSp = new StackPanel();
            rightSp.Orientation = Orientation.Horizontal;
            WindowChrome.SetIsHitTestVisibleInChrome(rightSp, true);

            Button btnMin = CreateTitleBarBtn("🗕", delegate { WindowState = WindowState.Minimized; }, false);
            Button btnMax = CreateTitleBarBtn("🗖", delegate {
                WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            }, false);
            Button btnClose = CreateTitleBarBtn("✕", delegate { Close(); }, true);

            rightSp.Children.Add(btnMin);
            rightSp.Children.Add(btnMax);
            rightSp.Children.Add(btnClose);

            Grid.SetColumn(rightSp, 2);
            tb.Children.Add(rightSp);
            return tb;
        }

        private Button CreateTitleBarBtn(string glyph, RoutedEventHandler handler, bool isClose) {
            Button b = new Button();
            b.Width = 46;
            b.Height = 48;
            b.Cursor = Cursors.Hand;
            b.BorderThickness = new Thickness(0);
            WindowChrome.SetIsHitTestVisibleInChrome(b, true);

            ControlTemplate ct = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "btnBorder");
            border.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            border.SetValue(Border.SnapsToDevicePixelsProperty, true);

            FrameworkElementFactory txt = new FrameworkElementFactory(typeof(TextBlock));
            txt.SetValue(TextBlock.TextProperty, glyph);
            txt.SetValue(TextBlock.FontSizeProperty, isClose ? 12.0 : 10.0);
            txt.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(203, 213, 225)));
            txt.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            txt.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(txt);

            ct.VisualTree = border;

            Trigger hoverTrig = new Trigger();
            hoverTrig.Property = Button.IsMouseOverProperty;
            hoverTrig.Value = true;
            hoverTrig.Setters.Add(new Setter(Border.BackgroundProperty, isClose ? new SolidColorBrush(Color.FromRgb(225, 29, 72)) : new SolidColorBrush(Color.FromRgb(30, 41, 59)), "btnBorder"));
            ct.Triggers.Add(hoverTrig);

            b.Template = ct;
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
            b.Cursor = Cursors.Hand;
            b.HorizontalContentAlignment = HorizontalAlignment.Left;
            b.Padding = new Thickness(12, 0, 12, 0);

            ControlTemplate ct = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "navBorder");
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            FrameworkElementFactory cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            cp.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Button.PaddingProperty));
            border.AppendChild(cp);
            ct.VisualTree = border;
            b.Template = ct;

            SetSidebarBtnContent(b, icon, label, isSelected);
            b.Click += click;
            return b;
        }

        private void SetSidebarBtnContent(Button b, string icon, string label, bool isSelected) {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock iTb = new TextBlock();
            iTb.Text = icon;
            iTb.FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
            iTb.FontSize = 15;
            iTb.Width = 24;
            iTb.VerticalAlignment = VerticalAlignment.Center;
            iTb.Foreground = isSelected ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
            sp.Children.Add(iTb);

            TextBlock lTb = new TextBlock();
            lTb.Text = label;
            lTb.FontSize = 13;
            lTb.FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Medium;
            lTb.Margin = new Thickness(8, 0, 0, 0);
            lTb.VerticalAlignment = VerticalAlignment.Center;
            lTb.Foreground = isSelected ? Brushes.White : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            sp.Children.Add(lTb);

            b.Content = sp;
            b.Background = isSelected ? new SolidColorBrush(Color.FromRgb(26, 36, 54)) : Brushes.Transparent;
            b.BorderBrush = isSelected ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : Brushes.Transparent;
            b.BorderThickness = isSelected ? new Thickness(3, 0, 0, 0) : new Thickness(0);
        }

        private void ApplySidebarBtnStyle(Button b, bool isSelected) {
            StackPanel sp = b.Content as StackPanel;
            if (isSelected) {
                b.Background = new SolidColorBrush(Color.FromRgb(26, 36, 54));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                b.BorderThickness = new Thickness(3, 0, 0, 0);
                if (sp != null && sp.Children.Count >= 2) {
                    ((TextBlock)sp.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    ((TextBlock)sp.Children[1]).Foreground = Brushes.White;
                    ((TextBlock)sp.Children[1]).FontWeight = FontWeights.SemiBold;
                }
            } else {
                b.Background = Brushes.Transparent;
                b.BorderThickness = new Thickness(0);
                if (sp != null && sp.Children.Count >= 2) {
                    ((TextBlock)sp.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                    ((TextBlock)sp.Children[1]).Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240));
                    ((TextBlock)sp.Children[1]).FontWeight = FontWeights.Medium;
                }
            }
        }

        public void SwitchView(int index) {
            for (int i = 0; i < allViews.Count; i++) {
                allViews[i].Visibility = (i == index) ? Visibility.Visible : Visibility.Collapsed;
            }
            for (int i = 0; i < navButtons.Count; i++) {
                ApplySidebarBtnStyle(navButtons[i], i == index);
            }

            if (index == 1) UpdateMonitorsHardwareCards();
            if (index == 2) RefreshPresetsCatalog();
            if (index == 3) RefreshProfilesList();
            if (index == 4) UpdateSchedulerUi();
            if (index == 5 && lblSettingsSaveNotice != null) lblSettingsSaveNotice.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
