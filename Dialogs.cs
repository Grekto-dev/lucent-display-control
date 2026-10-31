using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LucentControl {
    /// <summary>Dark, localized confirm / prompt dialogs matching the app theme.</summary>
    public class DarkDialog : Window {
        private TextBox input;
        private string resultText;
        private bool accepted;

        private DarkDialog(Window owner, string title, string message, string okText, bool askInput, string defaultText, bool danger) {
            Title = title;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.Height;
            Width = 440;
            Background = new SolidColorBrush(Color.FromRgb(16, 20, 30));
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI, Segoe UI Variable Display");
            Icon = AppIcon.Wpf();

            if (owner != null && owner.IsVisible) {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Topmost = true;
            }

            Border frame = new Border();
            frame.BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));
            frame.BorderThickness = new Thickness(1);

            StackPanel sp = new StackPanel();
            sp.Margin = new Thickness(24, 20, 24, 20);

            TextBlock tt = new TextBlock();
            tt.Text = title;
            tt.FontSize = 16;
            tt.FontWeight = FontWeights.Bold;
            tt.Foreground = Brushes.White;
            sp.Children.Add(tt);

            TextBlock msg = new TextBlock();
            msg.Text = message;
            msg.FontSize = 13;
            msg.TextWrapping = TextWrapping.Wrap;
            msg.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
            msg.Margin = new Thickness(0, 10, 0, 0);
            sp.Children.Add(msg);

            if (askInput) {
                input = new TextBox();
                input.Style = Theme.TextBox;
                input.Text = defaultText ?? "";
                input.Height = 36;
                input.MaxLength = 40;
                input.Margin = new Thickness(0, 14, 0, 0);
                sp.Children.Add(input);
            }

            StackPanel buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.HorizontalAlignment = HorizontalAlignment.Right;
            buttons.Margin = new Thickness(0, 20, 0, 0);

            Button ok = new Button();
            ok.Style = danger ? Theme.DangerButton : Theme.PrimaryButton;
            ok.Content = okText;
            ok.MinWidth = 96;
            ok.Height = 34;
            ok.Margin = new Thickness(0, 0, 8, 0);
            ok.Click += delegate { Accept(); };
            buttons.Children.Add(ok);

            Button cancel = new Button();
            cancel.Style = Theme.Button;
            cancel.Content = I18n.T("BtnCancel");
            cancel.MinWidth = 96;
            cancel.Height = 34;
            cancel.Click += delegate { DialogResult = false; };
            buttons.Children.Add(cancel);
            sp.Children.Add(buttons);

            frame.Child = sp;
            Content = frame;

            MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e) {
                if (e.OriginalSource is Border || e.OriginalSource is TextBlock) {
                    try { DragMove(); } catch { }
                }
            };
            PreviewKeyDown += delegate(object s, KeyEventArgs e) {
                if (e.Key == Key.Escape) { DialogResult = false; e.Handled = true; }
                else if (e.Key == Key.Enter && askInput) { Accept(); e.Handled = true; }
            };
            SourceInitialized += delegate {
                NativeMethods.EnableDarkMode(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            };
            Loaded += delegate {
                if (input != null) { input.Focus(); input.SelectAll(); } else { ok.Focus(); }
            };
        }

        private void Accept() {
            accepted = true;
            resultText = input != null ? input.Text : null;
            DialogResult = true;
        }

        /// <summary>Shows a confirmation dialog. Returns true when the user confirms.</summary>
        public static bool Confirm(Window owner, string title, string message, string okText, bool danger) {
            DarkDialog d = new DarkDialog(owner, title, message, okText, false, null, danger);
            d.ShowDialog();
            return d.accepted;
        }

        /// <summary>Asks for a text value. Returns null when cancelled.</summary>
        public static string Prompt(Window owner, string title, string message, string defaultText, string okText) {
            DarkDialog d = new DarkDialog(owner, title, message, okText, true, defaultText, false);
            d.ShowDialog();
            return d.accepted ? d.resultText : null;
        }
    }
}
