using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LucentControl {
    public partial class MainWindow : Window {
        #region View 6: Settings (Windows 11 Style)
        private Grid CreateSettingsView() {
            Grid g = new Grid();
            ScrollViewer scroll = new ScrollViewer();
            scrollSettings = scroll;
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

            // 2. Startup Card with Conditional Subitem (Start with Windows)
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

            // 4. Auto-Save vs Manual Save Card
            Border autoSaveCard = CreateDashboardCard();
            autoSaveCard.Margin = new Thickness(0, 0, 0, 12);

            Grid autoSaveRow = new Grid();
            autoSaveRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            autoSaveRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel autoSaveInfo = new StackPanel();
            txtAutoSaveTitle = new TextBlock();
            txtAutoSaveTitle.Text = I18n.T("AutoSaveTitle");
            txtAutoSaveTitle.FontSize = 14;
            txtAutoSaveTitle.FontWeight = FontWeights.SemiBold;
            txtAutoSaveTitle.Foreground = Brushes.White;
            autoSaveInfo.Children.Add(txtAutoSaveTitle);

            txtAutoSaveDesc = new TextBlock();
            txtAutoSaveDesc.Text = I18n.T("AutoSaveDesc");
            txtAutoSaveDesc.FontSize = 12;
            txtAutoSaveDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtAutoSaveDesc.TextWrapping = TextWrapping.Wrap;
            txtAutoSaveDesc.Margin = new Thickness(0, 4, 16, 0);
            autoSaveInfo.Children.Add(txtAutoSaveDesc);

            Grid.SetColumn(autoSaveInfo, 0);
            autoSaveRow.Children.Add(autoSaveInfo);

            chkAutoSave = new CheckBox();
            chkAutoSave.IsChecked = config.AutoSave;
            chkAutoSave.VerticalAlignment = VerticalAlignment.Center;
            chkAutoSave.Cursor = Cursors.Hand;
            chkAutoSave.Checked += delegate {
                config.AutoSave = true;
                UpdateUnsavedBadge();
            };
            chkAutoSave.Unchecked += delegate {
                config.AutoSave = false;
                UpdateUnsavedBadge();
            };
            Grid.SetColumn(chkAutoSave, 1);
            autoSaveRow.Children.Add(chkAutoSave);
            autoSaveCard.Child = autoSaveRow;
            sp.Children.Add(autoSaveCard);

            // 5. Auto-Apply Profile on Startup Card (with greyed-out dropdown)
            Border autoApplyCard = CreateDashboardCard();
            autoApplyCard.Margin = new Thickness(0, 0, 0, 12);
            StackPanel autoApplySp = new StackPanel();

            Grid autoApplyRow = new Grid();
            autoApplyRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            autoApplyRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel autoApplyInfo = new StackPanel();
            txtAutoApplyTitle = new TextBlock();
            txtAutoApplyTitle.Text = I18n.T("AutoApplyTitle");
            txtAutoApplyTitle.FontSize = 14;
            txtAutoApplyTitle.FontWeight = FontWeights.SemiBold;
            txtAutoApplyTitle.Foreground = Brushes.White;
            autoApplyInfo.Children.Add(txtAutoApplyTitle);

            txtAutoApplyDesc = new TextBlock();
            txtAutoApplyDesc.Text = I18n.T("AutoApplyDesc");
            txtAutoApplyDesc.FontSize = 12;
            txtAutoApplyDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtAutoApplyDesc.TextWrapping = TextWrapping.Wrap;
            txtAutoApplyDesc.Margin = new Thickness(0, 4, 16, 0);
            autoApplyInfo.Children.Add(txtAutoApplyDesc);

            Grid.SetColumn(autoApplyInfo, 0);
            autoApplyRow.Children.Add(autoApplyInfo);

            chkAutoApply = new CheckBox();
            chkAutoApply.IsChecked = config.AutoApplyProfile;
            chkAutoApply.VerticalAlignment = VerticalAlignment.Center;
            chkAutoApply.Cursor = Cursors.Hand;
            chkAutoApply.Checked += delegate { UpdateAutoApplyState(); };
            chkAutoApply.Unchecked += delegate { UpdateAutoApplyState(); };
            Grid.SetColumn(chkAutoApply, 1);
            autoApplyRow.Children.Add(chkAutoApply);
            autoApplySp.Children.Add(autoApplyRow);

            // Indented Subcard: Startup Profile Dropdown (greyed out unless checked)
            subCardStartupProfile = new Border();
            subCardStartupProfile.Margin = new Thickness(24, 14, 0, 0);
            subCardStartupProfile.Padding = new Thickness(14, 10, 14, 10);
            subCardStartupProfile.Background = new SolidColorBrush(Color.FromRgb(16, 20, 30));
            subCardStartupProfile.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 38, 54));
            subCardStartupProfile.BorderThickness = new Thickness(1);
            subCardStartupProfile.CornerRadius = new CornerRadius(6);

            Grid subProfRow = new Grid();
            subProfRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            subProfRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });

            txtStartupProfileLabel = new TextBlock();
            txtStartupProfileLabel.Text = I18n.T("StartupProfileLabel");
            txtStartupProfileLabel.FontSize = 13;
            txtStartupProfileLabel.FontWeight = FontWeights.Medium;
            txtStartupProfileLabel.Foreground = Brushes.White;
            txtStartupProfileLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(txtStartupProfileLabel, 0);
            subProfRow.Children.Add(txtStartupProfileLabel);

            comboStartupProfile = new ComboBox();
            comboStartupProfile.Style = darkComboBoxStyle;
            comboStartupProfile.Height = 34;
            comboStartupProfile.VerticalAlignment = VerticalAlignment.Center;
            PopulateStartupProfilesCombo();
            Grid.SetColumn(comboStartupProfile, 1);
            subProfRow.Children.Add(comboStartupProfile);

            subCardStartupProfile.Child = subProfRow;
            autoApplySp.Children.Add(subCardStartupProfile);
            autoApplyCard.Child = autoApplySp;
            sp.Children.Add(autoApplyCard);

            // 5b. Hardware Gamma LUT Card (Start Menu, Taskbar & UAC)
            Border lutCard = CreateDashboardCard();
            lutCard.Margin = new Thickness(0, 0, 0, 12);
            StackPanel lutSp = new StackPanel();

            Grid lutRow = new Grid();
            lutRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            lutRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel lutInfo = new StackPanel();
            txtLutTitle = new TextBlock();
            txtLutTitle.Text = I18n.T("LutTitle");
            txtLutTitle.FontSize = 14;
            txtLutTitle.FontWeight = FontWeights.SemiBold;
            txtLutTitle.Foreground = Brushes.White;
            lutInfo.Children.Add(txtLutTitle);

            txtLutDesc = new TextBlock();
            txtLutDesc.Text = I18n.T("LutDesc");
            txtLutDesc.FontSize = 12;
            txtLutDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtLutDesc.TextWrapping = TextWrapping.Wrap;
            txtLutDesc.Margin = new Thickness(0, 4, 16, 0);
            lutInfo.Children.Add(txtLutDesc);

            Grid.SetColumn(lutInfo, 0);
            lutRow.Children.Add(lutInfo);

            chkUseHardwareLut = new CheckBox();
            chkUseHardwareLut.IsChecked = config.UseHardwareLut;
            chkUseHardwareLut.VerticalAlignment = VerticalAlignment.Center;
            chkUseHardwareLut.Cursor = Cursors.Hand;
            chkUseHardwareLut.Checked += delegate {
                config.UseHardwareLut = true;
                UpdateHardwareLutState();
                ApplyAllActiveSettings();
            };
            chkUseHardwareLut.Unchecked += delegate {
                config.UseHardwareLut = false;
                UpdateHardwareLutState();
                ApplyAllActiveSettings();
            };
            Grid.SetColumn(chkUseHardwareLut, 1);
            lutRow.Children.Add(chkUseHardwareLut);
            lutSp.Children.Add(lutRow);

            subCardLutNotice = new Border();
            subCardLutNotice.Margin = new Thickness(24, 12, 0, 0);
            subCardLutNotice.Padding = new Thickness(12, 10, 12, 10);
            subCardLutNotice.Background = new SolidColorBrush(Color.FromRgb(16, 20, 30));
            subCardLutNotice.BorderBrush = new SolidColorBrush(Color.FromRgb(30, 38, 54));
            subCardLutNotice.BorderThickness = new Thickness(1);
            subCardLutNotice.CornerRadius = new CornerRadius(6);

            Grid lutNoteRow = new Grid();
            lutNoteRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            lutNoteRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            txtLutStatusNote = new TextBlock();
            txtLutStatusNote.FontSize = 11;
            txtLutStatusNote.TextWrapping = TextWrapping.Wrap;
            txtLutStatusNote.VerticalAlignment = VerticalAlignment.Center;
            txtLutStatusNote.Margin = new Thickness(0, 0, 12, 0);
            Grid.SetColumn(txtLutStatusNote, 0);
            lutNoteRow.Children.Add(txtLutStatusNote);

            btnEnableDisplayLinkLut = new Button();
            btnEnableDisplayLinkLut.Style = darkButtonStyle;
            btnEnableDisplayLinkLut.Content = I18n.T("BtnEnableDisplayLinkGamma");
            btnEnableDisplayLinkLut.Height = 32;
            btnEnableDisplayLinkLut.Padding = new Thickness(12, 0, 12, 0);
            btnEnableDisplayLinkLut.Click += delegate { EnableDisplayLinkGammaAction(); };
            Grid.SetColumn(btnEnableDisplayLinkLut, 1);
            lutNoteRow.Children.Add(btnEnableDisplayLinkLut);

            subCardLutNotice.Child = lutNoteRow;
            lutSp.Children.Add(subCardLutNotice);

            lutCard.Child = lutSp;
            sp.Children.Add(lutCard);

            // 6. Presets & Data Management Card (Restore Defaults & Erase All)
            Border dataCard = CreateDashboardCard();
            dataCard.Margin = new Thickness(0, 0, 0, 12);
            StackPanel dataSp = new StackPanel();

            txtDataSectionTitle = new TextBlock();
            txtDataSectionTitle.Text = I18n.T("DataTitle");
            txtDataSectionTitle.FontSize = 14;
            txtDataSectionTitle.FontWeight = FontWeights.Bold;
            txtDataSectionTitle.Foreground = Brushes.White;
            txtDataSectionTitle.Margin = new Thickness(0, 0, 0, 14);
            dataSp.Children.Add(txtDataSectionTitle);

            // Restore Presets Row
            Grid restoreRow = new Grid();
            restoreRow.Margin = new Thickness(0, 0, 0, 12);
            restoreRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            restoreRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel restInfo = new StackPanel();
            txtRestoreTitle = new TextBlock();
            txtRestoreTitle.Text = I18n.T("RestoreTitle");
            txtRestoreTitle.FontSize = 13;
            txtRestoreTitle.FontWeight = FontWeights.SemiBold;
            txtRestoreTitle.Foreground = Brushes.White;
            restInfo.Children.Add(txtRestoreTitle);

            txtRestoreDesc = new TextBlock();
            txtRestoreDesc.Text = I18n.T("RestoreDesc");
            txtRestoreDesc.FontSize = 11;
            txtRestoreDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtRestoreDesc.TextWrapping = TextWrapping.Wrap;
            txtRestoreDesc.Margin = new Thickness(0, 2, 16, 0);
            restInfo.Children.Add(txtRestoreDesc);

            Grid.SetColumn(restInfo, 0);
            restoreRow.Children.Add(restInfo);

            btnRestoreDefaults = new Button();
            btnRestoreDefaults.Style = darkButtonStyle;
            btnRestoreDefaults.Content = I18n.T("BtnRestore");
            btnRestoreDefaults.Height = 34;
            btnRestoreDefaults.Padding = new Thickness(16, 0, 16, 0);
            btnRestoreDefaults.Click += delegate { RestoreDefaultsAction(); };
            Grid.SetColumn(btnRestoreDefaults, 1);
            restoreRow.Children.Add(btnRestoreDefaults);
            dataSp.Children.Add(restoreRow);

            // Erase Data Row (Danger)
            Grid eraseRow = new Grid();
            eraseRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            eraseRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel eraseInfo = new StackPanel();
            txtEraseTitle = new TextBlock();
            txtEraseTitle.Text = I18n.T("EraseTitle");
            txtEraseTitle.FontSize = 13;
            txtEraseTitle.FontWeight = FontWeights.SemiBold;
            txtEraseTitle.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
            eraseInfo.Children.Add(txtEraseTitle);

            txtEraseDesc = new TextBlock();
            txtEraseDesc.Text = I18n.T("EraseDesc");
            txtEraseDesc.FontSize = 11;
            txtEraseDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            txtEraseDesc.TextWrapping = TextWrapping.Wrap;
            txtEraseDesc.Margin = new Thickness(0, 2, 16, 0);
            eraseInfo.Children.Add(txtEraseDesc);

            Grid.SetColumn(eraseInfo, 0);
            eraseRow.Children.Add(eraseInfo);

            btnEraseAll = new Button();
            btnEraseAll.Style = darkDangerButtonStyle;
            btnEraseAll.Content = I18n.T("BtnErase");
            btnEraseAll.Height = 34;
            btnEraseAll.Padding = new Thickness(16, 0, 16, 0);
            btnEraseAll.Click += delegate { EraseAllDataAction(); };
            Grid.SetColumn(btnEraseAll, 1);
            eraseRow.Children.Add(btnEraseAll);
            dataSp.Children.Add(eraseRow);

            dataCard.Child = dataSp;
            sp.Children.Add(dataCard);

            // 7. About Card with Version 0.1.0
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

            // 8. Action Buttons (Apply, OK, Cancel)
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
            UpdateAutoApplyState();
            UpdateHardwareLutState();
            return g;
        }

        private void UpdateHardwareLutState() {
            if (subCardLutNotice == null || txtLutStatusNote == null || btnEnableDisplayLinkLut == null) return;
            bool enabled = (chkUseHardwareLut != null && chkUseHardwareLut.IsChecked == true);
            bool hasDisplayLink = HardwareLutEngine.IsDisplayLinkConnected(screenDevices);
            bool dlGammaReady = HardwareLutEngine.IsDisplayLinkGammaEnabled();

            if (!enabled) {
                subCardLutNotice.Visibility = Visibility.Collapsed;
                return;
            }

            subCardLutNotice.Visibility = Visibility.Visible;
            if (hasDisplayLink && !dlGammaReady) {
                txtLutStatusNote.Text = I18n.T("LutDisplayLinkWarn");
                txtLutStatusNote.Foreground = new SolidColorBrush(Color.FromRgb(251, 191, 36));
                btnEnableDisplayLinkLut.Visibility = Visibility.Visible;
            } else {
                txtLutStatusNote.Text = I18n.T("LutReadyNote");
                txtLutStatusNote.Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128));
                btnEnableDisplayLinkLut.Visibility = Visibility.Collapsed;
            }
        }

        private void EnableDisplayLinkGammaAction() {
            try {
                Clipboard.SetText("reg add \"HKLM\\SOFTWARE\\DisplayLink\\Core\" /v EnableGammaRamp /t REG_SZ /d true /f");
                if (lblSettingsSaveNotice != null) {
                    lblSettingsSaveNotice.Text = I18n.T("LutDisplayLinkDone");
                    lblSettingsSaveNotice.Visibility = Visibility.Visible;
                }
            } catch { }
        }

        private void UpdateStartupState() {
            if (subCardStartMinimized == null) return;
            bool enabled = (chkStartWithWindows.IsChecked == true);
            subCardStartMinimized.IsEnabled = enabled;
            subCardStartMinimized.Opacity = enabled ? 1.0 : 0.45;
        }

        private void UpdateAutoApplyState() {
            if (subCardStartupProfile == null) return;
            bool enabled = (chkAutoApply.IsChecked == true);
            subCardStartupProfile.IsEnabled = enabled;
            subCardStartupProfile.Opacity = enabled ? 1.0 : 0.45;
        }

        public void PopulateStartupProfilesCombo() {
            if (comboStartupProfile == null) return;
            string prev = comboStartupProfile.SelectedItem != null ? comboStartupProfile.SelectedItem.ToString() : config.StartupProfile;
            comboStartupProfile.Items.Clear();

            foreach (UserProfile up in profileManager.Profiles) {
                comboStartupProfile.Items.Add(up.Name);
            }

            int idx = comboStartupProfile.Items.IndexOf(prev);
            comboStartupProfile.SelectedIndex = (idx >= 0) ? idx : 0;
        }

        private void SaveSettingsState() {
            I18n.CommittedLanguage = I18n.CurrentLanguage;
            config.Language = I18n.CurrentLanguage;
            config.StartWithWindows = (chkStartWithWindows.IsChecked == true);
            config.StartMinimized = (chkStartMinimized.IsChecked == true);
            config.MinimizeOnClose = (chkMinimizeOnClose.IsChecked == true);
            config.AutoSave = (chkAutoSave.IsChecked == true);
            config.AutoApplyProfile = (chkAutoApply.IsChecked == true);
            config.UseHardwareLut = (chkUseHardwareLut != null && chkUseHardwareLut.IsChecked == true);
            if (comboStartupProfile.SelectedItem != null) {
                config.StartupProfile = comboStartupProfile.SelectedItem.ToString();
            }

            AppConfig.SetAutoStart(config.StartWithWindows, config.StartMinimized);
            config.Save();
            ApplyAllActiveSettings();
            UpdateTrayMenu();
        }

        private void RestoreDefaultsAction() {
            if (DarkDialog.Confirm(this, I18n.T("RestoreTitle"), I18n.T("RestoreConfirm"), I18n.T("BtnRestore"), false)) {
                profileManager.RestoreDefaults();
                PopulateProfilesDropdown(comboActiveProfile);
                PopulateStartupProfilesCombo();
                RefreshPresetsCatalog();
                RefreshProfilesList();

                // If active profile was restored, reapply
                UserProfile cur = profileManager.Find(activeProfileName);
                if (cur != null) {
                    ApplyPreset(cur.Brightness, cur.Contrast, cur.Gamma, cur.Temperature, cur.Red, cur.Green, cur.Blue, cur.Vibrance, cur.Name);
                }

                if (lblSettingsSaveNotice != null) {
                    lblSettingsSaveNotice.Text = I18n.T("RestoreDone");
                    lblSettingsSaveNotice.Visibility = Visibility.Visible;
                }
            }
        }

        private void EraseAllDataAction() {
            if (DarkDialog.Confirm(this, I18n.T("EraseTitle"), I18n.T("EraseConfirm"), I18n.T("BtnErase"), true)) {
                AppPaths.DeleteAll();

                // Reset config & profiles to factory state
                config = new AppConfig();
                config.Save();
                AppConfig.SetAutoStart(false, false);

                profileManager = new ProfileManager();
                I18n.CurrentLanguage = AppLanguage.English;
                I18n.CommittedLanguage = AppLanguage.English;

                globalSettings.ResetToDefaults();
                foreach (string k in new List<string>(settingsPerMonitor.Keys)) {
                    settingsPerMonitor[k].ResetToDefaults();
                }

                activeProfileName = "Standard";
                hasCustomProfileAssigned = false;
                hasUnsavedChanges = false;

                DwmEngine.Reset();
                HardwareLutEngine.ResetAll(screenDevices);
                overlayManager.CloseAll();

                if (comboLanguage != null) comboLanguage.SelectedIndex = (int)AppLanguage.English;
                if (chkStartWithWindows != null) chkStartWithWindows.IsChecked = false;
                if (chkStartMinimized != null) chkStartMinimized.IsChecked = false;
                if (chkMinimizeOnClose != null) chkMinimizeOnClose.IsChecked = true;
                if (chkAutoSave != null) chkAutoSave.IsChecked = false;
                if (chkAutoApply != null) chkAutoApply.IsChecked = false;
                if (chkUseHardwareLut != null) chkUseHardwareLut.IsChecked = false;

                UpdateStartupState();
                UpdateAutoApplyState();
                UpdateHardwareLutState();
                PopulateProfilesDropdown(comboActiveProfile);
                PopulateStartupProfilesCombo();
                RefreshPresetsCatalog();
                RefreshProfilesList();
                UpdateAllTexts();
                UpdateUiFromSettings();

                if (lblSettingsSaveNotice != null) {
                    lblSettingsSaveNotice.Text = I18n.T("EraseDone");
                    lblSettingsSaveNotice.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion
    }
}
