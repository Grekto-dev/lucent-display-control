using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LucentControl {
    public partial class MainWindow : Window {
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
            btnMinimizeToTray = CreateHeaderActionBtn("\uE718", I18n.T("MinimizeToTray"), delegate {
                Hide();
                NativeMethods.TrimWorkingSet();
            });

            actSp.Children.Add(btnIdentify);
            actSp.Children.Add(btnMinimizeToTray);
            Grid.SetColumn(actSp, 1);
            headGrid.Children.Add(actSp);
            sp.Children.Add(headGrid);

            // 2. Monitors Row (Cards per display + Synced card)
            monitorCardsRow = new StackPanel();
            monitorCardsRow.Orientation = Orientation.Horizontal;
            monitorCardsRow.Margin = new Thickness(0, 16, 0, 16);
            sp.Children.Add(monitorCardsRow);

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

            // 6. Bottom Controls: Active Profile + Save + Reset + Apply
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
            if (btn == null) return;
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

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "☼";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(250, 204, 21));
            icon.Margin = new Thickness(0, 0, 8, 0);
            icon.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(icon);

            lblBrightnessTitle = new TextBlock();
            lblBrightnessTitle.Text = I18n.T("Brightness");
            lblBrightnessTitle.FontSize = 13;
            lblBrightnessTitle.FontWeight = FontWeights.SemiBold;
            lblBrightnessTitle.Foreground = Brushes.White;
            lblBrightnessTitle.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(lblBrightnessTitle);
            topRow.Children.Add(titleSp);

            badgeBrightness = new TextBlock();
            badgeBrightness.Text = "100%";
            badgeBrightness.FontSize = 12;
            badgeBrightness.FontWeight = FontWeights.SemiBold;
            badgeBrightness.Foreground = Brushes.White;
            badgeBrightness.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(badgeBrightness, 1);
            topRow.Children.Add(badgeBrightness);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            Grid sliderRow = new Grid();
            sliderRow.Margin = new Thickness(0, 14, 0, 0);
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock iconDim = new TextBlock();
            iconDim.Text = "☼";
            iconDim.FontSize = 11;
            iconDim.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            iconDim.VerticalAlignment = VerticalAlignment.Center;
            iconDim.Margin = new Thickness(0, 0, 8, 0);
            sliderRow.Children.Add(iconDim);

            sliderBrightness = new Slider();
            sliderBrightness.Minimum = 0.20;
            sliderBrightness.Maximum = 1.00;
            sliderBrightness.Value = 1.00;
            sliderBrightness.Style = sliderStyleBlue;
            sliderBrightness.ValueChanged += delegate {
                if (!isUpdatingUi) {
                    badgeBrightness.Text = string.Format("{0}%", (int)Math.Round(sliderBrightness.Value * 100));
                    OnControlChanged();
                }
            };
            Grid.SetColumn(sliderBrightness, 1);
            sliderRow.Children.Add(sliderBrightness);

            TextBlock iconBright = new TextBlock();
            iconBright.Text = "☼";
            iconBright.FontSize = 14;
            iconBright.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            iconBright.VerticalAlignment = VerticalAlignment.Center;
            iconBright.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(iconBright, 2);
            sliderRow.Children.Add(iconBright);

            Grid.SetRow(sliderRow, 1);
            g.Children.Add(sliderRow);
            return g;
        }

        private Grid CreateContrastControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "◑";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            icon.Margin = new Thickness(0, 0, 8, 0);
            icon.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(icon);

            lblContrastTitle = new TextBlock();
            lblContrastTitle.Text = I18n.T("Contrast");
            lblContrastTitle.FontSize = 13;
            lblContrastTitle.FontWeight = FontWeights.SemiBold;
            lblContrastTitle.Foreground = Brushes.White;
            lblContrastTitle.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(lblContrastTitle);
            topRow.Children.Add(titleSp);

            badgeContrast = new TextBlock();
            badgeContrast.Text = "100%";
            badgeContrast.FontSize = 12;
            badgeContrast.FontWeight = FontWeights.SemiBold;
            badgeContrast.Foreground = Brushes.White;
            badgeContrast.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(badgeContrast, 1);
            topRow.Children.Add(badgeContrast);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            Grid sliderRow = new Grid();
            sliderRow.Margin = new Thickness(0, 14, 0, 0);
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            sliderRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock iconLow = new TextBlock();
            iconLow.Text = "◔";
            iconLow.FontSize = 11;
            iconLow.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            iconLow.VerticalAlignment = VerticalAlignment.Center;
            iconLow.Margin = new Thickness(0, 0, 8, 0);
            sliderRow.Children.Add(iconLow);

            sliderContrast = new Slider();
            sliderContrast.Minimum = 0.50;
            sliderContrast.Maximum = 1.50;
            sliderContrast.Value = 1.00;
            sliderContrast.Style = sliderStyleBlue;
            sliderContrast.ValueChanged += delegate {
                if (!isUpdatingUi) {
                    badgeContrast.Text = string.Format("{0}%", (int)Math.Round(sliderContrast.Value * 100));
                    OnControlChanged();
                }
            };
            Grid.SetColumn(sliderContrast, 1);
            sliderRow.Children.Add(sliderContrast);

            TextBlock iconHigh = new TextBlock();
            iconHigh.Text = "◕";
            iconHigh.FontSize = 13;
            iconHigh.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            iconHigh.VerticalAlignment = VerticalAlignment.Center;
            iconHigh.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(iconHigh, 2);
            sliderRow.Children.Add(iconHigh);

            Grid.SetRow(sliderRow, 1);
            g.Children.Add(sliderRow);
            return g;
        }

        private Grid CreateGammaControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "📈";
            icon.FontSize = 12;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(192, 132, 252));
            icon.Margin = new Thickness(0, 0, 8, 0);
            icon.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(icon);

            lblGammaTitle = new TextBlock();
            lblGammaTitle.Text = I18n.T("Gamma");
            lblGammaTitle.FontSize = 13;
            lblGammaTitle.FontWeight = FontWeights.SemiBold;
            lblGammaTitle.Foreground = Brushes.White;
            lblGammaTitle.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(lblGammaTitle);
            topRow.Children.Add(titleSp);

            badgeGamma = new TextBlock();
            badgeGamma.Text = "1.00";
            badgeGamma.FontSize = 12;
            badgeGamma.FontWeight = FontWeights.SemiBold;
            badgeGamma.Foreground = Brushes.White;
            badgeGamma.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(badgeGamma, 1);
            topRow.Children.Add(badgeGamma);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            sliderGamma = new Slider();
            sliderGamma.Margin = new Thickness(0, 14, 0, 0);
            sliderGamma.Minimum = 0.60;
            sliderGamma.Maximum = 2.00;
            sliderGamma.Value = 1.00;
            sliderGamma.Style = sliderStyleBlue;
            sliderGamma.ValueChanged += delegate {
                if (!isUpdatingUi) {
                    badgeGamma.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F2}", sliderGamma.Value);
                    HighlightGammaTick();
                    OnControlChanged();
                }
            };
            Grid.SetRow(sliderGamma, 1);
            g.Children.Add(sliderGamma);

            // Gamma Ticks Row
            UniformGrid ticksGrid = new UniformGrid();
            ticksGrid.Columns = 5;
            ticksGrid.Margin = new Thickness(0, 6, 0, 0);

            gammaTick18 = CreateGammaTickText("0.6");
            ticksGrid.Children.Add(gammaTick18);
            gammaTick20 = CreateGammaTickText("0.8");
            ticksGrid.Children.Add(gammaTick20);

            gammaTick22Badge = new Border();
            gammaTick22Badge.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
            gammaTick22Badge.CornerRadius = new CornerRadius(10);
            gammaTick22Badge.Padding = new Thickness(8, 2, 8, 2);
            gammaTick22Badge.HorizontalAlignment = HorizontalAlignment.Center;
            gammaTick22Text = new TextBlock();
            gammaTick22Text.Text = "1.0";
            gammaTick22Text.FontSize = 10;
            gammaTick22Text.FontWeight = FontWeights.Bold;
            gammaTick22Text.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            gammaTick22Badge.Child = gammaTick22Text;
            ticksGrid.Children.Add(gammaTick22Badge);

            gammaTick24 = CreateGammaTickText("1.4");
            ticksGrid.Children.Add(gammaTick24);
            gammaTick26 = CreateGammaTickText("2.0");
            ticksGrid.Children.Add(gammaTick26);

            Grid.SetRow(ticksGrid, 2);
            g.Children.Add(ticksGrid);
            return g;
        }

        private TextBlock CreateGammaTickText(string t) {
            TextBlock tb = new TextBlock();
            tb.Text = t;
            tb.FontSize = 10;
            tb.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            return tb;
        }

        private Grid CreateColorTempControl() {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "🌡";
            icon.FontSize = 12;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            icon.Margin = new Thickness(0, 0, 8, 0);
            icon.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(icon);

            lblColorTempTitle = new TextBlock();
            lblColorTempTitle.Text = I18n.T("ColorTemp");
            lblColorTempTitle.FontSize = 13;
            lblColorTempTitle.FontWeight = FontWeights.SemiBold;
            lblColorTempTitle.Foreground = Brushes.White;
            lblColorTempTitle.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(lblColorTempTitle);
            topRow.Children.Add(titleSp);

            badgeColorTemp = new TextBlock();
            badgeColorTemp.Text = "6500 K";
            badgeColorTemp.FontSize = 12;
            badgeColorTemp.FontWeight = FontWeights.SemiBold;
            badgeColorTemp.Foreground = Brushes.White;
            badgeColorTemp.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(badgeColorTemp, 1);
            topRow.Children.Add(badgeColorTemp);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            sliderColorTemp = new Slider();
            sliderColorTemp.Margin = new Thickness(0, 14, 0, 0);
            sliderColorTemp.Minimum = 3000;
            sliderColorTemp.Maximum = 9000;
            sliderColorTemp.Value = 6500;
            sliderColorTemp.Style = sliderStyleTemp;
            sliderColorTemp.ValueChanged += delegate {
                if (!isUpdatingUi) {
                    badgeColorTemp.Text = string.Format("{0} K", (int)Math.Round(sliderColorTemp.Value));
                    OnControlChanged();
                }
            };
            Grid.SetRow(sliderColorTemp, 1);
            g.Children.Add(sliderColorTemp);

            // Bottom Labels (Warm, Neutral, Cool)
            Grid labelsRow = new Grid();
            labelsRow.Margin = new Thickness(0, 6, 0, 0);
            labelsRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            labelsRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            labelsRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock wLbl = new TextBlock();
            wLbl.Text = I18n.T("Warm");
            wLbl.FontSize = 10;
            wLbl.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            labelsRow.Children.Add(wLbl);

            Border nBadge = new Border();
            nBadge.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
            nBadge.CornerRadius = new CornerRadius(10);
            nBadge.Padding = new Thickness(8, 2, 8, 2);
            nBadge.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock nTb = new TextBlock();
            nTb.Text = I18n.T("Neutral");
            nTb.FontSize = 10;
            nTb.FontWeight = FontWeights.Bold;
            nTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            nBadge.Child = nTb;
            Grid.SetColumn(nBadge, 1);
            labelsRow.Children.Add(nBadge);

            TextBlock cLbl = new TextBlock();
            cLbl.Text = I18n.T("Cool");
            cLbl.FontSize = 10;
            cLbl.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            Grid.SetColumn(cLbl, 2);
            labelsRow.Children.Add(cLbl);

            Grid.SetRow(labelsRow, 2);
            g.Children.Add(labelsRow);
            return g;
        }

        private Grid CreateChannelControl(string channelName, Color dotColor, out Slider outSlider, out TextBlock outBadge, Style sliderStyle) {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            Ellipse dot = new Ellipse();
            dot.Width = 8;
            dot.Height = 8;
            dot.Fill = new SolidColorBrush(dotColor);
            dot.Margin = new Thickness(0, 0, 8, 0);
            dot.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(dot);

            TextBlock lbl = new TextBlock();
            lbl.Text = I18n.T(channelName);
            lbl.FontSize = 13;
            lbl.FontWeight = FontWeights.SemiBold;
            lbl.Foreground = Brushes.White;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            if (channelName == "Red") lblRedTitle = lbl;
            else if (channelName == "Green") lblGreenTitle = lbl;
            else if (channelName == "Blue") lblBlueTitle = lbl;
            titleSp.Children.Add(lbl);
            topRow.Children.Add(titleSp);

            TextBlock badge = new TextBlock();
            badge.Text = "100";
            badge.FontSize = 12;
            badge.FontWeight = FontWeights.SemiBold;
            badge.Foreground = Brushes.White;
            badge.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(badge, 1);
            topRow.Children.Add(badge);
            outBadge = badge;
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            Slider s = new Slider();
            s.Margin = new Thickness(0, 14, 0, 0);
            s.Minimum = 0.0;
            s.Maximum = 1.0;
            s.Value = 1.0;
            s.Style = sliderStyle;
            TextBlock bRef = badge;
            s.ValueChanged += delegate {
                if (!isUpdatingUi) {
                    bRef.Text = string.Format("{0}", (int)Math.Round(s.Value * 100));
                    OnControlChanged();
                }
            };
            outSlider = s;
            Grid.SetRow(s, 1);
            g.Children.Add(s);

            Grid lblRow = new Grid();
            lblRow.Margin = new Thickness(0, 6, 0, 0);
            lblRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            lblRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock minL = new TextBlock();
            minL.Text = "0";
            minL.FontSize = 10;
            minL.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            lblRow.Children.Add(minL);

            TextBlock maxL = new TextBlock();
            maxL.Text = "100";
            maxL.FontSize = 10;
            maxL.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            Grid.SetColumn(maxL, 1);
            lblRow.Children.Add(maxL);

            Grid.SetRow(lblRow, 2);
            g.Children.Add(lblRow);
            return g;
        }

        private Grid CreateQuickPresetsControl() {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Left: Title + Subtitle
            StackPanel titleSp = new StackPanel();
            titleSp.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Margin = new Thickness(0, 0, 24, 0);

            StackPanel tRow = new StackPanel();
            tRow.Orientation = Orientation.Horizontal;

            TextBlock icon = new TextBlock();
            icon.Text = "✦";
            icon.FontSize = 14;
            icon.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            icon.Margin = new Thickness(0, 0, 8, 0);
            icon.VerticalAlignment = VerticalAlignment.Center;
            tRow.Children.Add(icon);

            lblQuickPresetsTitle = new TextBlock();
            lblQuickPresetsTitle.Text = I18n.T("QuickPresets");
            lblQuickPresetsTitle.FontSize = 13;
            lblQuickPresetsTitle.FontWeight = FontWeights.Bold;
            lblQuickPresetsTitle.Foreground = Brushes.White;
            lblQuickPresetsTitle.VerticalAlignment = VerticalAlignment.Center;
            tRow.Children.Add(lblQuickPresetsTitle);
            titleSp.Children.Add(tRow);

            lblQuickPresetsSubtitle = new TextBlock();
            lblQuickPresetsSubtitle.Text = I18n.T("ApplyProfileInstantly");
            lblQuickPresetsSubtitle.FontSize = 11;
            lblQuickPresetsSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            lblQuickPresetsSubtitle.Margin = new Thickness(0, 2, 0, 0);
            titleSp.Children.Add(lblQuickPresetsSubtitle);

            Grid.SetColumn(titleSp, 0);
            g.Children.Add(titleSp);

            // Right: 6 Preset Pill Buttons
            StackPanel pillsSp = new StackPanel();
            pillsSp.Orientation = Orientation.Horizontal;
            pillsSp.HorizontalAlignment = HorizontalAlignment.Right;
            pillsSp.VerticalAlignment = VerticalAlignment.Center;

            btnPresetWarm = CreatePresetPill("☼", I18n.T("PresetWarm"), false, delegate {
                ApplyPreset(0.85, 0.95, 1.05, 4500, 1.0, 0.95, 0.85, "Warm");
                HighlightPresetPill(btnPresetWarm);
            });
            btnPresetNeutral = CreatePresetPill("◎", I18n.T("PresetNeutral"), true, delegate {
                ApplyPreset(1.0, 1.0, 1.0, 6500, 1.0, 1.0, 1.0, "Standard");
                HighlightPresetPill(btnPresetNeutral);
            });
            btnPresetCool = CreatePresetPill("❄", I18n.T("PresetCool"), false, delegate {
                ApplyPreset(1.0, 1.10, 1.0, 8000, 0.92, 0.96, 1.0, "Cool");
                HighlightPresetPill(btnPresetCool);
            });
            btnPresetMovie = CreatePresetPill("🎞", I18n.T("PresetMovie"), false, delegate {
                ApplyPreset(0.90, 1.15, 1.15, 5800, 1.0, 0.95, 0.90, "Movie");
                HighlightPresetPill(btnPresetMovie);
            });
            btnPresetWork = CreatePresetPill("💻", I18n.T("PresetWork"), false, delegate {
                ApplyPreset(0.80, 1.0, 1.0, 6000, 1.0, 1.0, 0.96, "Work");
                HighlightPresetPill(btnPresetWork);
            });
            btnPresetNight = CreatePresetPill("🌙", I18n.T("PresetNight"), false, delegate {
                ApplyPreset(0.65, 0.90, 1.0, 3400, 1.0, 0.84, 0.68, "Night");
                HighlightPresetPill(btnPresetNight);
            });

            presetButtons = new List<Button>() {
                btnPresetWarm,
                btnPresetNeutral,
                btnPresetCool,
                btnPresetMovie,
                btnPresetWork,
                btnPresetNight
            };

            foreach (Button b in presetButtons) {
                pillsSp.Children.Add(b);
            }

            Grid.SetColumn(pillsSp, 1);
            g.Children.Add(pillsSp);
            return g;
        }

        private Button CreatePresetPill(string icon, string label, bool isSelected, RoutedEventHandler click) {
            Button b = new Button();
            b.Height = 32;
            b.Margin = new Thickness(4, 0, 0, 0);
            b.Cursor = Cursors.Hand;
            b.Padding = new Thickness(12, 0, 12, 0);

            ControlTemplate ct = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "pillBorder");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

            FrameworkElementFactory cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Button.PaddingProperty));
            border.AppendChild(cp);
            ct.VisualTree = border;
            b.Template = ct;

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock it = new TextBlock();
            it.Text = icon;
            it.FontSize = 11;
            it.Margin = new Thickness(0, 0, 6, 0);
            it.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(it);

            TextBlock lt = new TextBlock();
            lt.Text = label;
            lt.FontSize = 11;
            lt.FontWeight = FontWeights.Medium;
            lt.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(lt);

            b.Content = sp;
            ApplyPresetPillStyle(b, isSelected);
            b.Click += click;
            return b;
        }

        private void ApplyPresetPillStyle(Button b, bool isSelected) {
            StackPanel sp = b.Content as StackPanel;
            if (isSelected) {
                b.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                if (sp != null && sp.Children.Count >= 2) {
                    ((TextBlock)sp.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    ((TextBlock)sp.Children[1]).Foreground = Brushes.White;
                }
            } else {
                b.Background = new SolidColorBrush(Color.FromRgb(26, 32, 46));
                b.BorderBrush = new SolidColorBrush(Color.FromRgb(47, 58, 82));
                if (sp != null && sp.Children.Count >= 2) {
                    ((TextBlock)sp.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                    ((TextBlock)sp.Children[1]).Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
                }
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

            // Left: Active Profile Dropdown Card + Save + Save as New + Unsaved Badge
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
                        activeProfileName = p.Name;
                        hasCustomProfileAssigned = !p.IsBuiltIn;
                        hasUnsavedChanges = false;
                        UpdateUnsavedBadge();
                    }
                }
            };
            profSp.Children.Add(comboActiveProfile);

            // Save button
            btnSaveProfile = new Button();
            btnSaveProfile.Style = darkButtonStyle;
            btnSaveProfile.Content = I18n.T("BtnSave");
            btnSaveProfile.Height = 32;
            btnSaveProfile.Margin = new Thickness(8, 0, 0, 0);
            btnSaveProfile.Padding = new Thickness(12, 0, 12, 0);
            btnSaveProfile.Click += delegate { SaveActiveProfile(); };
            profSp.Children.Add(btnSaveProfile);

            // Save as New... button
            btnSaveAsNewProfile = new Button();
            btnSaveAsNewProfile.Style = darkButtonStyle;
            btnSaveAsNewProfile.Content = I18n.T("BtnSaveAsNew");
            btnSaveAsNewProfile.Height = 32;
            btnSaveAsNewProfile.Margin = new Thickness(6, 0, 0, 0);
            btnSaveAsNewProfile.Padding = new Thickness(12, 0, 12, 0);
            btnSaveAsNewProfile.Click += delegate {
                string promptTitle = I18n.T("NewProfileTitle");
                string promptDesc = I18n.T("CustomPresetDesc");
                string defaultName = profileManager.UniqueName("Custom");
                string newName = DarkDialog.Prompt(this, promptTitle, promptDesc, defaultName, I18n.T("BtnSave"));
                if (!string.IsNullOrEmpty(newName)) {
                    SaveCurrentAsProfile(newName);
                }
            };
            profSp.Children.Add(btnSaveAsNewProfile);

            // Unsaved badge
            lblUnsavedBadge = new TextBlock();
            lblUnsavedBadge.Text = "● " + I18n.T("TagModified");
            lblUnsavedBadge.FontSize = 11;
            lblUnsavedBadge.FontWeight = FontWeights.SemiBold;
            lblUnsavedBadge.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
            lblUnsavedBadge.Margin = new Thickness(10, 0, 0, 0);
            lblUnsavedBadge.VerticalAlignment = VerticalAlignment.Center;
            lblUnsavedBadge.Visibility = Visibility.Collapsed;
            profSp.Children.Add(lblUnsavedBadge);

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
                if (config.AutoSave) {
                    SaveActiveProfile();
                }
            };
            actRow.Children.Add(btnApplyToMonitor);

            Grid.SetColumn(actRow, 2);
            g.Children.Add(actRow);

            b.Child = g;
            return b;
        }

        private void UpdateUnsavedBadge() {
            if (lblUnsavedBadge == null) return;
            lblUnsavedBadge.Visibility = (hasUnsavedChanges && !config.AutoSave) ? Visibility.Visible : Visibility.Collapsed;
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
            txtCalibrationDesc.Margin = new Thickness(0, 4, 0, 14);
            calibSp.Children.Add(txtCalibrationDesc);

            // Test Pattern Action Buttons (White, Black, 16-step Gray Ramp, SMPTE Color Bars)
            UniformGrid patternsGrid = new UniformGrid();
            patternsGrid.Columns = 4;

            btnCalibWhite = CreatePatternButton(I18n.T("PatternWhite"), I18n.T("PatWhiteSub"), Color.FromRgb(248, 250, 252), delegate {
                overlayManager.ShowPattern(CalibrationPatternType.White);
            });
            btnCalibBlack = CreatePatternButton(I18n.T("PatternBlack"), I18n.T("PatBlackSub"), Color.FromRgb(15, 23, 42), delegate {
                overlayManager.ShowPattern(CalibrationPatternType.Black);
            });
            btnCalibGray = CreatePatternButton(I18n.T("PatternGray"), I18n.T("PatGraySub"), Color.FromRgb(100, 116, 139), delegate {
                overlayManager.ShowPattern(CalibrationPatternType.Grayscale);
            });
            btnCalibColor = CreatePatternButton(I18n.T("PatternColor"), I18n.T("PatColorSub"), Color.FromRgb(56, 189, 248), delegate {
                overlayManager.ShowPattern(CalibrationPatternType.ColorBars);
            });

            patternsGrid.Children.Add(btnCalibWhite);
            patternsGrid.Children.Add(btnCalibBlack);
            patternsGrid.Children.Add(btnCalibGray);
            patternsGrid.Children.Add(btnCalibColor);

            calibSp.Children.Add(patternsGrid);
            calibCard.Child = calibSp;
            sp.Children.Add(calibCard);

            scroll.Content = sp;
            g.Children.Add(scroll);
            return g;
        }

        private Button CreatePatternButton(string title, string sub, Color swatchColor, RoutedEventHandler click) {
            Button b = new Button();
            b.Style = darkButtonStyle;
            b.Height = 64;
            b.Margin = new Thickness(4, 0, 4, 0);
            b.Padding = new Thickness(10, 8, 10, 8);
            b.Click += click;

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border swatch = new Border();
            swatch.Width = 32;
            swatch.Height = 32;
            swatch.CornerRadius = new CornerRadius(6);
            swatch.Background = new SolidColorBrush(swatchColor);
            swatch.BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));
            swatch.BorderThickness = new Thickness(1);
            swatch.Margin = new Thickness(0, 0, 10, 0);
            swatch.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(swatch, 0);
            g.Children.Add(swatch);

            StackPanel sp = new StackPanel();
            sp.VerticalAlignment = VerticalAlignment.Center;

            TextBlock tt = new TextBlock();
            tt.Text = title;
            tt.FontSize = 12;
            tt.FontWeight = FontWeights.SemiBold;
            tt.Foreground = Brushes.White;
            sp.Children.Add(tt);

            TextBlock st = new TextBlock();
            st.Text = sub;
            st.FontSize = 10;
            st.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            st.Margin = new Thickness(0, 2, 0, 0);
            sp.Children.Add(st);

            Grid.SetColumn(sp, 1);
            g.Children.Add(sp);

            b.Content = g;
            return b;
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

            // 2. Presets Grid
            presetsGrid = new UniformGrid();
            presetsGrid.Columns = 2;
            presetsGrid.Margin = new Thickness(-6, 0, -6, 16);
            sp.Children.Add(presetsGrid);
            RefreshPresetsCatalog();

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
            custDesc.Text = I18n.T("CustomPresetDesc");
            custDesc.FontSize = 11;
            custDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            custDesc.Margin = new Thickness(0, 2, 0, 12);
            custSp.Children.Add(custDesc);

            Grid rowInput = new Grid();
            rowInput.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            rowInput.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            txtCustomPresetName = new TextBox();
            txtCustomPresetName.Style = darkTextBoxStyle;
            txtCustomPresetName.Height = 36;
            txtCustomPresetName.VerticalContentAlignment = VerticalAlignment.Center;
            txtCustomPresetName.Text = profileManager.UniqueName("Custom");
            Grid.SetColumn(txtCustomPresetName, 0);
            rowInput.Children.Add(txtCustomPresetName);

            btnSaveCustomPreset = new Button();
            btnSaveCustomPreset.Style = darkPrimaryButtonStyle;
            btnSaveCustomPreset.Content = I18n.T("BtnSaveProfile");
            btnSaveCustomPreset.Height = 36;
            btnSaveCustomPreset.Padding = new Thickness(20, 0, 20, 0);
            btnSaveCustomPreset.Margin = new Thickness(10, 0, 0, 0);
            btnSaveCustomPreset.Click += delegate {
                string name = ProfileManager.SanitizeName(txtCustomPresetName.Text);
                if (!string.IsNullOrEmpty(name)) {
                    SaveCurrentAsProfile(name);
                    lblCustomPresetNotice.Text = string.Format(I18n.T("SavedTo"), name);
                    lblCustomPresetNotice.Visibility = Visibility.Visible;
                    txtCustomPresetName.Text = profileManager.UniqueName("Custom");
                }
            };
            Grid.SetColumn(btnSaveCustomPreset, 1);
            rowInput.Children.Add(btnSaveCustomPreset);
            custSp.Children.Add(rowInput);

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

        private void RefreshPresetsCatalog() {
            if (presetsGrid == null) return;
            presetsGrid.Children.Clear();

            AddPresetCatalogCard("Standard", Color.FromRgb(248, 250, 252));
            AddPresetCatalogCard("Warm", Color.FromRgb(251, 146, 60));
            AddPresetCatalogCard("Cool", Color.FromRgb(56, 189, 248));
            AddPresetCatalogCard("Movie", Color.FromRgb(168, 85, 247));
            AddPresetCatalogCard("Work", Color.FromRgb(34, 197, 94));
            AddPresetCatalogCard("Night", Color.FromRgb(249, 115, 22));
            AddPresetCatalogCard("Gaming", Color.FromRgb(239, 68, 68));
            AddPresetCatalogCard("Focus", Color.FromRgb(234, 179, 8));
        }

        private void AddPresetCatalogCard(string presetKey, Color dotColor) {
            UserProfile up = profileManager.Find(presetKey);
            if (up == null) return;

            Border b = CreateDashboardCard();
            b.Margin = new Thickness(6, 6, 6, 6);

            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Title Row
            Grid topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel titleSp = new StackPanel();
            titleSp.Orientation = Orientation.Horizontal;

            Ellipse dot = new Ellipse();
            dot.Width = 10;
            dot.Height = 10;
            dot.Fill = new SolidColorBrush(dotColor);
            dot.Margin = new Thickness(0, 0, 8, 0);
            dot.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(dot);

            TextBlock tt = new TextBlock();
            string pName = I18n.T("Preset" + presetKey);
            if (pName.StartsWith("☀ ") || pName.StartsWith("◎ ") || pName.StartsWith("❄ ") || pName.StartsWith("🎬 ") || pName.StartsWith("🖥 ") || pName.StartsWith("🌙 ") || pName.StartsWith("🎮 ") || pName.StartsWith("🖋 ") || pName.StartsWith("🎯 ")) {
                pName = pName.Substring(2).Trim();
            }
            tt.Text = pName;
            tt.FontSize = 14;
            tt.FontWeight = FontWeights.Bold;
            tt.Foreground = Brushes.White;
            tt.VerticalAlignment = VerticalAlignment.Center;
            titleSp.Children.Add(tt);
            topRow.Children.Add(titleSp);

            // Tags: BuiltIn / Modified / Active
            StackPanel tagsSp = new StackPanel();
            tagsSp.Orientation = Orientation.Horizontal;

            if (profileManager.IsModified(up)) {
                Border modBadge = new Border();
                modBadge.Background = new SolidColorBrush(Color.FromArgb(90, 180, 83, 9));
                modBadge.CornerRadius = new CornerRadius(4);
                modBadge.Padding = new Thickness(6, 2, 6, 2);
                modBadge.Margin = new Thickness(0, 0, 4, 0);
                TextBlock modTb = new TextBlock();
                modTb.Text = I18n.T("TagModified");
                modTb.FontSize = 10;
                modTb.FontWeight = FontWeights.Bold;
                modTb.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60));
                modBadge.Child = modTb;
                tagsSp.Children.Add(modBadge);
            }

            if (string.Equals(activeProfileName, up.Name, StringComparison.OrdinalIgnoreCase)) {
                Border actBadge = new Border();
                actBadge.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
                actBadge.CornerRadius = new CornerRadius(4);
                actBadge.Padding = new Thickness(6, 2, 6, 2);
                TextBlock actTb = new TextBlock();
                actTb.Text = I18n.T("TagActive");
                actTb.FontSize = 10;
                actTb.FontWeight = FontWeights.Bold;
                actTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                actBadge.Child = actTb;
                tagsSp.Children.Add(actBadge);
            }

            Grid.SetColumn(tagsSp, 1);
            topRow.Children.Add(tagsSp);
            Grid.SetRow(topRow, 0);
            g.Children.Add(topRow);

            // Description
            TextBlock dt = new TextBlock();
            dt.Text = I18n.T(up.DescriptionKey);
            dt.FontSize = 11;
            dt.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            dt.TextWrapping = TextWrapping.Wrap;
            dt.Margin = new Thickness(0, 6, 0, 10);
            Grid.SetRow(dt, 1);
            g.Children.Add(dt);

            // Bottom Specs & Apply Button
            Grid btmRow = new Grid();
            btmRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            btmRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock st = new TextBlock();
            st.Text = string.Format("Bri {0}%  ·  Con {1}%  ·  {2}K", (int)Math.Round(up.Brightness * 100), (int)Math.Round(up.Contrast * 100), (int)Math.Round(up.Temperature));
            st.FontSize = 11;
            st.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            st.VerticalAlignment = VerticalAlignment.Center;
            btmRow.Children.Add(st);

            Button applyBtn = new Button();
            applyBtn.Style = darkButtonStyle;
            applyBtn.Content = I18n.T("BtnApplyPreset");
            applyBtn.Height = 28;
            applyBtn.Padding = new Thickness(12, 0, 12, 0);
            UserProfile captured = up;
            applyBtn.Click += delegate {
                ApplyPreset(captured.Brightness, captured.Contrast, captured.Gamma, captured.Temperature, captured.Red, captured.Green, captured.Blue, captured.Name);
                activeProfileName = captured.Name;
                hasCustomProfileAssigned = !captured.IsBuiltIn;
                hasUnsavedChanges = false;
                UpdateUnsavedBadge();
                RefreshPresetsCatalog();
            };
            Grid.SetColumn(applyBtn, 1);
            btmRow.Children.Add(applyBtn);

            Grid.SetRow(btmRow, 2);
            g.Children.Add(btmRow);

            b.Child = g;
            presetsGrid.Children.Add(b);
        }
        #endregion

        #region View 4: Profiles Management
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
            lblActiveProfileBanner.Text = string.Format(I18n.T("ActiveBanner"), activeProfileName);
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

            TextBlock npDesc = new TextBlock();
            npDesc.Text = I18n.T("CustomPresetDesc");
            npDesc.FontSize = 11;
            npDesc.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            npDesc.Margin = new Thickness(0, 2, 0, 12);
            npSp.Children.Add(npDesc);

            Grid npInput = new Grid();
            npInput.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            npInput.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            txtNewProfileName = new TextBox();
            txtNewProfileName.Style = darkTextBoxStyle;
            txtNewProfileName.Height = 36;
            txtNewProfileName.VerticalContentAlignment = VerticalAlignment.Center;
            txtNewProfileName.Text = profileManager.UniqueName("Custom");
            Grid.SetColumn(txtNewProfileName, 0);
            npInput.Children.Add(txtNewProfileName);

            btnCreateProfile = new Button();
            btnCreateProfile.Style = darkPrimaryButtonStyle;
            btnCreateProfile.Content = I18n.T("BtnSaveProfile");
            btnCreateProfile.Height = 36;
            btnCreateProfile.Padding = new Thickness(20, 0, 20, 0);
            btnCreateProfile.Margin = new Thickness(10, 0, 0, 0);
            btnCreateProfile.Click += delegate {
                string name = ProfileManager.SanitizeName(txtNewProfileName.Text);
                if (!string.IsNullOrEmpty(name)) {
                    SaveCurrentAsProfile(name);
                    lblProfileNotice.Text = string.Format(I18n.T("SavedTo"), name);
                    lblProfileNotice.Visibility = Visibility.Visible;
                    txtNewProfileName.Text = profileManager.UniqueName("Custom");
                }
            };
            Grid.SetColumn(btnCreateProfile, 1);
            npInput.Children.Add(btnCreateProfile);
            npSp.Children.Add(npInput);

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
                lblActiveProfileBanner.Text = string.Format(I18n.T("ActiveBanner"), activeProfileName);
            }

            foreach (UserProfile up in profileManager.Profiles) {
                profilesListContainer.Children.Add(CreateProfileRow(up));
            }
        }

        private Border CreateProfileRow(UserProfile up) {
            Border b = CreateDashboardCard();
            b.Margin = new Thickness(0, 0, 0, 8);
            b.Padding = new Thickness(14, 10, 14, 10);

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Left: Name + Tags
            StackPanel left = new StackPanel();
            left.Orientation = Orientation.Horizontal;
            left.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Text = up.Name;
            name.FontSize = 13;
            name.FontWeight = FontWeights.Bold;
            name.Foreground = Brushes.White;
            name.Margin = new Thickness(0, 0, 10, 0);
            name.VerticalAlignment = VerticalAlignment.Center;
            left.Children.Add(name);

            Border tag = new Border();
            tag.CornerRadius = new CornerRadius(4);
            tag.Padding = new Thickness(6, 2, 6, 2);
            tag.Background = up.IsBuiltIn ? new SolidColorBrush(Color.FromArgb(90, 30, 41, 59)) : new SolidColorBrush(Color.FromArgb(90, 15, 118, 110));
            tag.BorderBrush = up.IsBuiltIn ? new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(20, 184, 166));
            tag.BorderThickness = new Thickness(1);
            TextBlock tagTb = new TextBlock();
            tagTb.Text = up.IsBuiltIn ? I18n.T("TagBuiltIn") : I18n.T("TagCustom");
            tagTb.FontSize = 10;
            tagTb.FontWeight = FontWeights.SemiBold;
            tagTb.Foreground = up.IsBuiltIn ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(45, 212, 191));
            tag.Child = tagTb;
            left.Children.Add(tag);

            if (string.Equals(activeProfileName, up.Name, StringComparison.OrdinalIgnoreCase)) {
                Border actTag = new Border();
                actTag.CornerRadius = new CornerRadius(4);
                actTag.Padding = new Thickness(6, 2, 6, 2);
                actTag.Margin = new Thickness(6, 0, 0, 0);
                actTag.Background = new SolidColorBrush(Color.FromArgb(90, 30, 58, 138));
                actTag.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                actTag.BorderThickness = new Thickness(1);
                TextBlock actTb = new TextBlock();
                actTb.Text = I18n.T("TagActive");
                actTb.FontSize = 10;
                actTb.FontWeight = FontWeights.Bold;
                actTb.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                actTag.Child = actTb;
                left.Children.Add(actTag);
            }

            Grid.SetColumn(left, 0);
            g.Children.Add(left);

            // Center: Summary Specs
            TextBlock specTb = new TextBlock();
            specTb.Text = string.Format("{0}% Bri · {1}% Con · {2:F2} Gamma · {3}K",
                (int)Math.Round(up.Brightness * 100), (int)Math.Round(up.Contrast * 100), up.Gamma, (int)Math.Round(up.Temperature));
            specTb.FontSize = 11;
            specTb.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            specTb.VerticalAlignment = VerticalAlignment.Center;
            specTb.Margin = new Thickness(16, 0, 16, 0);
            Grid.SetColumn(specTb, 1);
            g.Children.Add(specTb);

            // Right: Action Buttons (Apply, Delete)
            StackPanel actSp = new StackPanel();
            actSp.Orientation = Orientation.Horizontal;

            Button applyBtn = new Button();
            applyBtn.Style = darkPrimaryButtonStyle;
            applyBtn.Content = I18n.T("BtnApplyPreset");
            applyBtn.Height = 28;
            applyBtn.Padding = new Thickness(12, 0, 12, 0);
            UserProfile cap = up;
            applyBtn.Click += delegate {
                ApplyPreset(cap.Brightness, cap.Contrast, cap.Gamma, cap.Temperature, cap.Red, cap.Green, cap.Blue, cap.Name);
                activeProfileName = cap.Name;
                hasCustomProfileAssigned = !cap.IsBuiltIn;
                hasUnsavedChanges = false;
                UpdateUnsavedBadge();
                RefreshProfilesList();
            };
            actSp.Children.Add(applyBtn);

            if (!up.IsBuiltIn) {
                Button delBtn = new Button();
                delBtn.Style = darkDangerButtonStyle;
                delBtn.Content = I18n.T("BtnDelete");
                delBtn.Height = 28;
                delBtn.Padding = new Thickness(10, 0, 10, 0);
                delBtn.Margin = new Thickness(6, 0, 0, 0);
                string delName = up.Name;
                delBtn.Click += delegate {
                    string msg = string.Format(I18n.T("DeleteConfirmMsg"), delName);
                    if (DarkDialog.Confirm(this, I18n.T("DeleteConfirmTitle"), msg, I18n.T("BtnDelete"), true)) {
                        DeleteProfile(delName);
                    }
                };
                actSp.Children.Add(delBtn);
            }

            Grid.SetColumn(actSp, 2);
            g.Children.Add(actSp);

            b.Child = g;
            return b;
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
        #endregion
    }
}
