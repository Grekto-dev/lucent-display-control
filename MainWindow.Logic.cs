using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using SD = System.Drawing;
using SWF = System.Windows.Forms;

namespace LucentControl {
    public partial class MainWindow : Window {
        #region Monitor Enumeration & Cards
        public void DetectMonitors() {
            screenDevices.Clear();
            uint devNum = 0;
            NativeMethods.DISPLAY_DEVICE d = new NativeMethods.DISPLAY_DEVICE();
            d.cb = System.Runtime.InteropServices.Marshal.SizeOf(d);

            while (NativeMethods.EnumDisplayDevices(null, devNum, ref d, 0)) {
                if ((d.StateFlags & 1) != 0) { // DISPLAY_DEVICE_ATTACHED_TO_DESKTOP
                    ScreenDevice sd = new ScreenDevice();
                    sd.Number = screenDevices.Count + 1;
                    sd.DeviceName = d.DeviceName;
                    sd.FriendlyName = d.DeviceString;
                    sd.IsPrimary = (d.StateFlags & 4) != 0; // DISPLAY_DEVICE_PRIMARY_DEVICE

                    NativeMethods.DEVMODE dm = new NativeMethods.DEVMODE();
                    dm.dmSize = (short)System.Runtime.InteropServices.Marshal.SizeOf(dm);
                    if (NativeMethods.EnumDisplaySettings(d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm)) {
                        sd.Bounds = new SD.Rectangle(dm.dmPositionX, dm.dmPositionY, dm.dmPelsWidth, dm.dmPelsHeight);
                        sd.RefreshRate = dm.dmDisplayFrequency;
                        sd.BitsPerPixel = dm.dmBitsPerPel;
                    }

                    // Look for monitor model name under adapter
                    NativeMethods.DISPLAY_DEVICE mon = new NativeMethods.DISPLAY_DEVICE();
                    mon.cb = System.Runtime.InteropServices.Marshal.SizeOf(mon);
                    if (NativeMethods.EnumDisplayDevices(d.DeviceName, 0, ref mon, 0)) {
                        if (!string.IsNullOrEmpty(mon.DeviceString) && mon.DeviceString != "Generic PnP Monitor") {
                            sd.FriendlyName = mon.DeviceString;
                        }
                    }

                    screenDevices.Add(sd);
                    if (!settingsPerMonitor.ContainsKey(sd.DeviceName)) {
                        settingsPerMonitor[sd.DeviceName] = globalSettings.Clone();
                    }
                }
                devNum++;
            }

            if (screenDevices.Count == 0) {
                ScreenDevice fallback = new ScreenDevice();
                fallback.Number = 1;
                fallback.DeviceName = "\\\\.\\DISPLAY1";
                fallback.FriendlyName = "Default Display";
                fallback.IsPrimary = true;
                fallback.Bounds = new SD.Rectangle(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
                fallback.RefreshRate = 60;
                fallback.BitsPerPixel = 32;
                screenDevices.Add(fallback);
                settingsPerMonitor[fallback.DeviceName] = globalSettings.Clone();
            }

            if (sidebarMonitorsCount != null) {
                sidebarMonitorsCount.Text = I18n.T("MonitorsConnected", screenDevices.Count);
            }
        }

        private void UpdateVisualMonitorCards() {
            if (monitorCardsRow == null) return;
            monitorCardsRow.Children.Clear();
            if (monitorCardBorders == null) monitorCardBorders = new List<Border>();
            monitorCardBorders.Clear();

            for (int i = 0; i < screenDevices.Count; i++) {
                Border card = CreateMonitorCard(i, screenDevices[i]);
                monitorCardBorders.Add(card);
                monitorCardsRow.Children.Add(card);
            }

            // Sync All Card
            Border syncCard = CreateSyncMonitorsCard();
            monitorCardBorders.Add(syncCard);
            monitorCardsRow.Children.Add(syncCard);

            HighlightActiveMonitorCard();
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

            StackPanel infoSp = new StackPanel();
            infoSp.VerticalAlignment = VerticalAlignment.Bottom;

            StackPanel nameSp = new StackPanel();
            nameSp.Orientation = Orientation.Horizontal;
            nameSp.HorizontalAlignment = HorizontalAlignment.Center;

            Ellipse dot = new Ellipse();
            dot.Width = 6;
            dot.Height = 6;
            dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
            dot.Margin = new Thickness(0, 0, 6, 0);
            dot.VerticalAlignment = VerticalAlignment.Center;
            nameSp.Children.Add(dot);

            TextBlock nameTb = new TextBlock();
            nameTb.Text = sd.FriendlyName;
            nameTb.FontSize = 12;
            nameTb.FontWeight = FontWeights.SemiBold;
            nameTb.Foreground = Brushes.White;
            nameTb.TextTrimming = TextTrimming.CharacterEllipsis;
            nameSp.Children.Add(nameTb);
            infoSp.Children.Add(nameSp);

            TextBlock resTb = new TextBlock();
            resTb.Text = sd.ResolutionText;
            resTb.FontSize = 10;
            resTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            resTb.HorizontalAlignment = HorizontalAlignment.Center;
            resTb.Margin = new Thickness(0, 2, 0, 0);
            infoSp.Children.Add(resTb);

            Grid.SetRow(infoSp, 1);
            g.Children.Add(infoSp);

            b.Child = g;
            int capturedIdx = index;
            b.MouseLeftButtonDown += delegate {
                syncAllMonitors = false;
                selectedMonitorIndex = capturedIdx;
                HighlightActiveMonitorCard();
                UpdateUiFromSettings();
            };
            return b;
        }

        private Border CreateSyncMonitorsCard() {
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
            badgeTb.Text = "⇆";
            badgeTb.FontSize = 11;
            badgeTb.FontWeight = FontWeights.Bold;
            badgeTb.Foreground = Brushes.White;
            badgeTb.HorizontalAlignment = HorizontalAlignment.Center;
            badgeTb.VerticalAlignment = VerticalAlignment.Center;
            badge.Child = badgeTb;
            topRow.Children.Add(badge);

            Canvas monIcon = new Canvas();
            monIcon.Width = 36;
            monIcon.Height = 22;
            monIcon.HorizontalAlignment = HorizontalAlignment.Center;

            Path m1 = new Path();
            m1.Data = Geometry.Parse("M 0 0 L 16 0 L 16 11 L 0 11 Z M 1 1 L 1 10 L 15 10 L 15 1 Z M 6 11 L 10 11 L 12 14 L 4 14 Z");
            m1.Fill = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            monIcon.Children.Add(m1);

            Path m2 = new Path();
            m2.Data = Geometry.Parse("M 20 0 L 36 0 L 36 11 L 20 11 Z M 21 1 L 21 10 L 35 10 L 35 1 Z M 26 11 L 30 11 L 32 14 L 24 14 Z");
            m2.Fill = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            monIcon.Children.Add(m2);

            Grid.SetColumn(monIcon, 1);
            topRow.Children.Add(monIcon);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            StackPanel infoSp = new StackPanel();
            infoSp.VerticalAlignment = VerticalAlignment.Bottom;

            TextBlock nameTb = new TextBlock();
            nameTb.Text = I18n.T("AllDisplays");
            nameTb.FontSize = 12;
            nameTb.FontWeight = FontWeights.SemiBold;
            nameTb.Foreground = Brushes.White;
            nameTb.HorizontalAlignment = HorizontalAlignment.Center;
            infoSp.Children.Add(nameTb);

            TextBlock resTb = new TextBlock();
            resTb.Text = I18n.T("MonitorsConnected", screenDevices.Count);
            resTb.FontSize = 10;
            resTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            resTb.HorizontalAlignment = HorizontalAlignment.Center;
            resTb.Margin = new Thickness(0, 2, 0, 0);
            infoSp.Children.Add(resTb);

            Grid.SetRow(infoSp, 1);
            g.Children.Add(infoSp);

            b.Child = g;
            b.MouseLeftButtonDown += delegate {
                syncAllMonitors = true;
                HighlightActiveMonitorCard();
                UpdateUiFromSettings();
            };
            return b;
        }

        private void HighlightActiveMonitorCard() {
            if (monitorCardBorders == null) return;
            for (int i = 0; i < monitorCardBorders.Count; i++) {
                Border b = monitorCardBorders[i];
                bool isSelected = (!syncAllMonitors && i == selectedMonitorIndex) || (syncAllMonitors && i == monitorCardBorders.Count - 1);
                if (isSelected) {
                    b.Background = new SolidColorBrush(Color.FromRgb(16, 26, 42));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    b.BorderThickness = new Thickness(2);
                } else {
                    b.Background = new SolidColorBrush(Color.FromRgb(20, 25, 36));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(34, 43, 62));
                    b.BorderThickness = new Thickness(1);
                }
            }

            if (btnApplyToMonitor != null) {
                string monLbl = syncAllMonitors ? I18n.T("AllDisplays") : (selectedMonitorIndex < screenDevices.Count ? screenDevices[selectedMonitorIndex].DisplayLabel : "Monitor");
                SetHeaderActionBtnText(btnApplyToMonitor, "\u2713", I18n.T("ApplyToMonitor", monLbl));
            }
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

                Grid topRow = new Grid();
                topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                StackPanel nameSp = new StackPanel();
                nameSp.Orientation = Orientation.Horizontal;

                Ellipse dot = new Ellipse();
                dot.Width = 8;
                dot.Height = 8;
                dot.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
                dot.Margin = new Thickness(0, 0, 8, 0);
                dot.VerticalAlignment = VerticalAlignment.Center;
                nameSp.Children.Add(dot);

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

                UniformGrid specsGrid = new UniformGrid();
                specsGrid.Margin = new Thickness(0, 14, 0, 0);
                specsGrid.Rows = 2;
                specsGrid.Columns = 3;

                specsGrid.Children.Add(CreateSpecItem(I18n.T("Resolution"), sd.ResolutionText));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("AdapterName"), sd.DeviceName));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("DesktopBounds"), string.Format("X={0}, Y={1}", sd.Bounds.X, sd.Bounds.Y)));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("RefreshRate"), string.Format("{0} Hz", sd.RefreshRate)));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("ColorDepth"), "32-bit True Color (ARGB)"));
                specsGrid.Children.Add(CreateSpecItem(I18n.T("VideoEngine"), "Hardware DWM Composition (MagEffect)"));

                Grid.SetRow(specsGrid, 1);
                g.Children.Add(specsGrid);

                card.Child = g;
                monitorsHardwareContainer.Children.Add(card);
            }
        }
        #endregion

        #region Settings Management & Real-time Output
        private DisplaySettings GetCurrentWorkingSettings() {
            if (syncAllMonitors) return globalSettings;
            if (selectedMonitorIndex < screenDevices.Count) {
                string dev = screenDevices[selectedMonitorIndex].DeviceName;
                if (settingsPerMonitor.ContainsKey(dev)) {
                    return settingsPerMonitor[dev];
                }
            }
            return globalSettings;
        }

        private void UpdateUiFromSettings() {
            isUpdatingUi = true;
            DisplaySettings s = GetCurrentWorkingSettings();

            if (sliderBrightness != null) {
                sliderBrightness.Value = s.Brightness;
                badgeBrightness.Text = string.Format("{0}%", (int)Math.Round(s.Brightness * 100));
                HighlightBrightnessTick();
            }
            if (sliderContrast != null) {
                sliderContrast.Value = s.Contrast;
                badgeContrast.Text = string.Format("{0}%", (int)Math.Round(s.Contrast * 100));
            }
            if (sliderGamma != null) {
                sliderGamma.Value = s.Gamma;
                badgeGamma.Text = string.Format(CultureInfo.InvariantCulture, "{0:F2}", s.Gamma);
                HighlightGammaTick();
            }
            if (sliderColorTemp != null) {
                sliderColorTemp.Value = s.Temperature;
                badgeColorTemp.Text = string.Format("{0} K", (int)Math.Round(s.Temperature));
            }
            if (sliderRed != null) {
                sliderRed.Value = s.Red;
                badgeRed.Text = string.Format("{0}", (int)Math.Round(s.Red * 100));
            }
            if (sliderGreen != null) {
                sliderGreen.Value = s.Green;
                badgeGreen.Text = string.Format("{0}", (int)Math.Round(s.Green * 100));
            }
            if (sliderBlue != null) {
                sliderBlue.Value = s.Blue;
                badgeBlue.Text = string.Format("{0}", (int)Math.Round(s.Blue * 100));
            }

            isUpdatingUi = false;
        }

        private void HighlightBrightnessTick() {
            if (sliderBrightness == null || brightTick0 == null || brightTick100Badge == null || brightTick200 == null) return;
            double v = sliderBrightness.Value;
            bool isMid = Math.Abs(v - 1.0) < 0.05;
            bool isMin = v <= 0.05;
            bool isMax = v >= 1.95;

            brightTick0.Foreground = isMin ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            brightTick200.Foreground = isMax ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));

            if (isMid) {
                brightTick100Badge.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
                brightTick100Text.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            } else {
                brightTick100Badge.Background = Brushes.Transparent;
                brightTick100Text.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            }
        }

        private void HighlightGammaTick() {
            if (sliderGamma == null || gammaTick18 == null) return;
            double v = sliderGamma.Value;

            gammaTick18.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            gammaTick20.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            gammaTick24.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            gammaTick26.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));

            if (Math.Abs(v - 0.6) < 0.1) gammaTick18.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            else if (Math.Abs(v - 0.8) < 0.1) gammaTick20.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            else if (Math.Abs(v - 1.4) < 0.1) gammaTick24.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            else if (Math.Abs(v - 2.0) < 0.1) gammaTick26.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
        }

        private void OnControlChanged() {
            if (isUpdatingUi) return;
            DisplaySettings s = GetCurrentWorkingSettings();
            s.Brightness = sliderBrightness.Value;
            s.Contrast = sliderContrast.Value;
            s.Gamma = sliderGamma.Value;
            s.Temperature = sliderColorTemp.Value;
            s.Red = sliderRed.Value;
            s.Green = sliderGreen.Value;
            s.Blue = sliderBlue.Value;

            if (syncAllMonitors) {
                foreach (string k in settingsPerMonitor.Keys) {
                    settingsPerMonitor[k].CopyFrom(s);
                }
            }

            ApplyAllActiveSettings();

            if (config.AutoSave) {
                // First-time save prompt if user has not named a custom profile yet
                if (!hasCustomProfileAssigned) {
                    string promptTitle = I18n.T("NewProfileTitle");
                    string promptDesc = I18n.T("CustomPresetDesc");
                    string defaultName = profileManager.UniqueName("Custom");
                    string newName = DarkDialog.Prompt(this, promptTitle, promptDesc, defaultName, I18n.T("BtnSave"));
                    if (!string.IsNullOrEmpty(newName)) {
                        SaveCurrentAsProfile(newName);
                    } else {
                        // User skipped naming; keep live on screen
                        hasCustomProfileAssigned = true; // avoid popping prompt on every subsequent pixel drag
                    }
                } else {
                    SaveActiveProfile();
                }
            } else {
                hasUnsavedChanges = true;
                UpdateUnsavedBadge();
            }
        }

        public void ApplyAllActiveSettings() {
            DisplaySettings s = GetCurrentWorkingSettings();
            DwmEngine.Apply(s, true);
            overlayManager.Apply(screenDevices, settingsPerMonitor);
        }

        private void ApplyPreset(double b, double c, double g, double t, double red, double green, double blue, string profileName) {
            DisplaySettings s = GetCurrentWorkingSettings();
            s.Brightness = b;
            s.Contrast = c;
            s.Gamma = g;
            s.Temperature = t;
            s.Red = red;
            s.Green = green;
            s.Blue = blue;

            if (syncAllMonitors) {
                foreach (string k in settingsPerMonitor.Keys) {
                    settingsPerMonitor[k].CopyFrom(s);
                }
            }

            activeProfileName = profileName;
            UserProfile up = profileManager.Find(profileName);
            hasCustomProfileAssigned = (up != null && !up.IsBuiltIn);

            UpdateUiFromSettings();
            ApplyAllActiveSettings();

            if (comboActiveProfile != null) {
                int pIdx = comboActiveProfile.Items.IndexOf(profileName);
                if (pIdx >= 0) comboActiveProfile.SelectedIndex = pIdx;
            }

            if (config.AutoSave && hasCustomProfileAssigned) {
                SaveActiveProfile();
            } else {
                hasUnsavedChanges = false;
                UpdateUnsavedBadge();
            }

            if (lblActiveProfileBanner != null) {
                lblActiveProfileBanner.Text = string.Format(I18n.T("ActiveBanner"), activeProfileName);
            }
        }

        private void ResetCurrentOrAll() {
            DisplaySettings s = GetCurrentWorkingSettings();
            s.ResetToDefaults();
            if (syncAllMonitors) {
                foreach (string k in settingsPerMonitor.Keys) {
                    settingsPerMonitor[k].ResetToDefaults();
                }
            }
            activeProfileName = "Standard";
            hasCustomProfileAssigned = false;
            hasUnsavedChanges = false;

            UpdateUiFromSettings();
            ApplyAllActiveSettings();
            HighlightPresetPill(btnPresetNeutral);

            if (comboActiveProfile != null) {
                int pIdx = comboActiveProfile.Items.IndexOf("Standard");
                if (pIdx >= 0) comboActiveProfile.SelectedIndex = pIdx;
            }
            UpdateUnsavedBadge();
        }
        #endregion

        #region Profile Storage & Dropdown
        public void PopulateProfilesDropdown(ComboBox cb) {
            if (cb == null) return;
            string prev = cb.SelectedItem != null ? cb.SelectedItem.ToString() : activeProfileName;
            cb.Items.Clear();

            foreach (UserProfile up in profileManager.Profiles) {
                cb.Items.Add(up.Name);
            }

            int idx = cb.Items.IndexOf(prev);
            cb.SelectedIndex = (idx >= 0) ? idx : 0;
        }

        public void SaveCurrentAsProfile(string name) {
            name = ProfileManager.SanitizeName(name);
            if (string.IsNullOrEmpty(name)) return;

            DisplaySettings s = GetCurrentWorkingSettings();
            profileManager.SaveProfile(name, s);
            activeProfileName = name;
            hasCustomProfileAssigned = true;
            hasUnsavedChanges = false;

            PopulateProfilesDropdown(comboActiveProfile);
            PopulateStartupProfilesCombo();
            RefreshProfilesList();
            UpdateUnsavedBadge();
        }

        public void SaveActiveProfile() {
            if (string.IsNullOrEmpty(activeProfileName) || !hasCustomProfileAssigned) {
                string promptTitle = I18n.T("NewProfileTitle");
                string promptDesc = I18n.T("CustomPresetDesc");
                string defaultName = profileManager.UniqueName("Custom");
                string newName = DarkDialog.Prompt(this, promptTitle, promptDesc, defaultName, I18n.T("BtnSave"));
                if (!string.IsNullOrEmpty(newName)) {
                    SaveCurrentAsProfile(newName);
                }
                return;
            }

            DisplaySettings s = GetCurrentWorkingSettings();
            profileManager.SaveProfile(activeProfileName, s);
            hasUnsavedChanges = false;
            UpdateUnsavedBadge();
            RefreshProfilesList();
        }

        public void DeleteProfile(string name) {
            if (profileManager.DeleteProfile(name)) {
                if (string.Equals(activeProfileName, name, StringComparison.OrdinalIgnoreCase)) {
                    activeProfileName = "Standard";
                    hasCustomProfileAssigned = false;
                }
                PopulateProfilesDropdown(comboActiveProfile);
                PopulateStartupProfilesCombo();
                RefreshProfilesList();
                UpdateUnsavedBadge();
            }
        }
        #endregion

        #region Automated Scheduler
        private void InitSchedulerTimer() {
            schedulerTimer = new DispatcherTimer();
            schedulerTimer.Interval = TimeSpan.FromSeconds(30);
            schedulerTimer.Tick += delegate {
                if (config.SchedulerEnabled) {
                    CheckSchedulerTick();
                }
            };
            schedulerTimer.Start();
        }

        private void CheckSchedulerTick() {
            DateTime now = DateTime.Now;
            TimeSpan current = now.TimeOfDay;

            TimeSpan[] starts = new TimeSpan[] {
                new TimeSpan(config.SlotDayHour, config.SlotDayMinute, 0),
                new TimeSpan(config.SlotSunsetHour, config.SlotSunsetMinute, 0),
                new TimeSpan(config.SlotNightHour, config.SlotNightMinute, 0)
            };

            int slotIdx = SchedulerLogic.CurrentSlot(current, starts);
            string targetProf = config.SlotDayProfile;
            string slotKey = "Day";
            if (slotIdx == 1) { targetProf = config.SlotSunsetProfile; slotKey = "Sunset"; }
            else if (slotIdx == 2) { targetProf = config.SlotNightProfile; slotKey = "Night"; }

            if (lastAppliedScheduleSlot != slotKey) {
                lastAppliedScheduleSlot = slotKey;
                UserProfile up = profileManager.Find(targetProf);
                if (up != null) {
                    ApplyPreset(up.Brightness, up.Contrast, up.Gamma, up.Temperature, up.Red, up.Green, up.Blue, up.Name);

                    if (config.SchedulerNotify && trayIcon != null) {
                        string slotName = I18n.T("Slot" + slotKey);
                        trayIcon.ShowBalloonTip(3000, "Lucent Display Control", string.Format("Shifted to {0} ({1})", slotName, targetProf), SWF.ToolTipIcon.Info);
                    }
                }
            }
        }

        public void UpdateSchedulerUi() {
            if (lblSchedulerStatus == null) return;
            bool en = (chkEnableScheduler.IsChecked == true);
            lblSchedulerStatus.Text = en ? "● Active: Monitoring circadian cycle" : "○ Inactive: Manual profile management";
            lblSchedulerStatus.Foreground = en ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
        }

        private void SaveSchedulerSettings() {
            config.SchedulerEnabled = (chkEnableScheduler.IsChecked == true);
            config.SchedulerNotify = (chkSchedulerNotify.IsChecked == true);

            if (comboDayHour != null) config.SlotDayHour = comboDayHour.SelectedIndex;
            if (comboDayMinute != null) config.SlotDayMinute = comboDayMinute.SelectedIndex * 5;
            if (comboDayProfile != null && comboDayProfile.SelectedItem != null) config.SlotDayProfile = comboDayProfile.SelectedItem.ToString();

            if (comboSunsetHour != null) config.SlotSunsetHour = comboSunsetHour.SelectedIndex;
            if (comboSunsetMinute != null) config.SlotSunsetMinute = comboSunsetMinute.SelectedIndex * 5;
            if (comboSunsetProfile != null && comboSunsetProfile.SelectedItem != null) config.SlotSunsetProfile = comboSunsetProfile.SelectedItem.ToString();

            if (comboNightHour != null) config.SlotNightHour = comboNightHour.SelectedIndex;
            if (comboNightMinute != null) config.SlotNightMinute = comboNightMinute.SelectedIndex * 5;
            if (comboNightProfile != null && comboNightProfile.SelectedItem != null) config.SlotNightProfile = comboNightProfile.SelectedItem.ToString();

            config.Save();
            if (config.SchedulerEnabled) {
                CheckSchedulerTick();
            }

            if (lblSchedulerSaveNotice != null) {
                lblSchedulerSaveNotice.Text = I18n.T("SavedNotice");
                lblSchedulerSaveNotice.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region System Tray & Lifecycle
        private void SetupTrayIcon() {
            trayIcon = new SWF.NotifyIcon();
            trayIcon.Text = "Lucent Display Control";
            trayIcon.Icon = AppIcon.Tray();
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

        public void RestoreWindow() {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (config.MinimizeOnClose) {
                e.Cancel = true;
                Hide();
                NativeMethods.TrimWorkingSet();
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

        #region Text Localization Updates
        public void UpdateAllTexts() {
            if (mainTitle == null) return;

            headerStatus.Text = I18n.T("RunningInTray");
            sidebarMonitorsCount.Text = I18n.T("MonitorsConnected", screenDevices.Count);

            SetSidebarBtnContent(btnNavDashboard, "\uE80F", I18n.T("NavDashboard"), allViews == null || allViews[0].Visibility == Visibility.Visible);
            SetSidebarBtnContent(btnNavMonitors, "\uE7F4", I18n.T("NavMonitors"), allViews != null && allViews[1].Visibility == Visibility.Visible);
            SetSidebarBtnContent(btnNavPresets, "\uE74C", I18n.T("NavPresets"), allViews != null && allViews[2].Visibility == Visibility.Visible);
            SetSidebarBtnContent(btnNavProfiles, "\uE77B", I18n.T("NavProfiles"), allViews != null && allViews[3].Visibility == Visibility.Visible);
            SetSidebarBtnContent(btnNavScheduler, "\uE823", I18n.T("NavScheduler"), allViews != null && allViews[4].Visibility == Visibility.Visible);
            SetSidebarBtnContent(btnNavSettings, "\uE713", I18n.T("NavSettings"), allViews != null && allViews[5].Visibility == Visibility.Visible);

            mainTitle.Text = I18n.T("MonitorsTitle");
            mainSubtitle.Text = I18n.T("MonitorsSubtitle");

            SetHeaderActionBtnText(btnIdentify, "\uE946", I18n.T("Identify"));
            SetHeaderActionBtnText(btnMinimizeToTray, "\uE718", I18n.T("MinimizeToTray"));

            if (lblBrightnessTitle != null) lblBrightnessTitle.Text = I18n.T("Brightness");
            if (lblContrastTitle != null) lblContrastTitle.Text = I18n.T("Contrast");
            if (lblGammaTitle != null) lblGammaTitle.Text = I18n.T("Gamma");
            if (lblColorTempTitle != null) lblColorTempTitle.Text = I18n.T("ColorTemp");
            if (lblRedTitle != null) lblRedTitle.Text = I18n.T("Red");
            if (lblGreenTitle != null) lblGreenTitle.Text = I18n.T("Green");
            if (lblBlueTitle != null) lblBlueTitle.Text = I18n.T("Blue");
            if (lblQuickPresetsTitle != null) lblQuickPresetsTitle.Text = I18n.T("QuickPresets");
            if (lblQuickPresetsSubtitle != null) lblQuickPresetsSubtitle.Text = I18n.T("ApplyProfileInstantly");
            if (lblActiveProfileTitle != null) lblActiveProfileTitle.Text = I18n.T("ActiveProfile");

            if (btnSaveProfile != null) btnSaveProfile.Content = I18n.T("BtnSave");
            if (btnSaveAsNewProfile != null) btnSaveAsNewProfile.Content = I18n.T("BtnSaveAsNew");
            if (lblUnsavedBadge != null) lblUnsavedBadge.Text = "● " + I18n.T("TagModified");

            SetHeaderActionBtnText(btnResetDefault, "\uE72C", I18n.T("ResetToDefault"));
            string monLbl = syncAllMonitors ? I18n.T("AllDisplays") : (selectedMonitorIndex < screenDevices.Count ? screenDevices[selectedMonitorIndex].DisplayLabel : "Monitor");
            SetHeaderActionBtnText(btnApplyToMonitor, "\u2713", I18n.T("ApplyToMonitor", monLbl));

            if (txtMonHardwareTitle != null) txtMonHardwareTitle.Text = I18n.T("MonHardwareTitle");
            if (txtMonHardwareSubtitle != null) txtMonHardwareSubtitle.Text = I18n.T("MonHardwareSubtitle");
            if (txtCalibrationTitle != null) txtCalibrationTitle.Text = I18n.T("CalibrationTitle");
            if (txtCalibrationDesc != null) txtCalibrationDesc.Text = I18n.T("CalibrationDesc");
            SetHeaderActionBtnText(btnRefreshDisplays, "\uE72C", I18n.T("RefreshMonitors"));
            SetHeaderActionBtnText(btnIdentifyFromMon, "\uE946", I18n.T("Identify"));

            if (txtPresetsTitle != null) txtPresetsTitle.Text = I18n.T("PresetsTitle");
            if (txtPresetsSubtitle != null) txtPresetsSubtitle.Text = I18n.T("PresetsSubtitle");
            if (btnSaveCustomPreset != null) btnSaveCustomPreset.Content = I18n.T("BtnSaveProfile");

            if (txtProfilesTitle != null) txtProfilesTitle.Text = I18n.T("ProfilesTitle");
            if (txtProfilesSubtitle != null) txtProfilesSubtitle.Text = I18n.T("ProfilesSubtitle");
            if (btnCreateProfile != null) btnCreateProfile.Content = I18n.T("BtnSaveProfile");
            if (lblActiveProfileBanner != null) lblActiveProfileBanner.Text = string.Format(I18n.T("ActiveBanner"), activeProfileName);

            if (txtSchedulerTitle != null) txtSchedulerTitle.Text = I18n.T("SchedulerTitle");
            if (txtSchedulerSubtitle != null) txtSchedulerSubtitle.Text = I18n.T("SchedulerSubtitle");
            if (chkEnableScheduler != null) chkEnableScheduler.Content = I18n.T("EnableScheduler");
            if (chkSchedulerNotify != null) chkSchedulerNotify.Content = I18n.T("NotifyOnShift");
            if (btnSaveScheduler != null) btnSaveScheduler.Content = I18n.T("BtnSaveScheduler");

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

            if (txtAutoSaveTitle != null) txtAutoSaveTitle.Text = I18n.T("AutoSaveTitle");
            if (txtAutoSaveDesc != null) txtAutoSaveDesc.Text = I18n.T("AutoSaveDesc");
            if (txtAutoApplyTitle != null) txtAutoApplyTitle.Text = I18n.T("AutoApplyTitle");
            if (txtAutoApplyDesc != null) txtAutoApplyDesc.Text = I18n.T("AutoApplyDesc");
            if (txtStartupProfileLabel != null) txtStartupProfileLabel.Text = I18n.T("StartupProfileLabel");
            if (txtDataSectionTitle != null) txtDataSectionTitle.Text = I18n.T("DataTitle");
            if (txtRestoreTitle != null) txtRestoreTitle.Text = I18n.T("RestoreTitle");
            if (txtRestoreDesc != null) txtRestoreDesc.Text = I18n.T("RestoreDesc");
            if (btnRestoreDefaults != null) btnRestoreDefaults.Content = I18n.T("BtnRestore");
            if (txtEraseTitle != null) txtEraseTitle.Text = I18n.T("EraseTitle");
            if (txtEraseDesc != null) txtEraseDesc.Text = I18n.T("EraseDesc");
            if (btnEraseAll != null) btnEraseAll.Content = I18n.T("BtnErase");

            if (txtAboutTitle != null) txtAboutTitle.Text = I18n.T("AboutTitle");
            if (txtAboutVersion != null) txtAboutVersion.Text = I18n.T("AboutVersion");
            if (txtAboutDesc != null) txtAboutDesc.Text = I18n.T("AboutDesc");
            if (btnSettingsApply != null) btnSettingsApply.Content = I18n.T("BtnApply");
            if (btnSettingsOK != null) btnSettingsOK.Content = I18n.T("BtnOK");
            if (btnSettingsCancel != null) btnSettingsCancel.Content = I18n.T("BtnCancel");
            if (lblSettingsSaveNotice != null) lblSettingsSaveNotice.Text = I18n.T("SavedNotice");

            UpdateTrayMenu();
        }
        #endregion
    }
}
