using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SD = System.Drawing;
using SWF = System.Windows.Forms;

namespace LucentControl {
    /// <summary>Single source of truth for the application icon (embedded assets\icon.ico).</summary>
    public static class AppIcon {
        private static byte[] data;
        private static ImageSource windowIcon;

        private static byte[] Data {
            get {
                if (data == null) {
                    using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("LucentControl.icon.ico")) {
                        if (s == null) return null;
                        MemoryStream ms = new MemoryStream();
                        byte[] buf = new byte[8192];
                        int n;
                        while ((n = s.Read(buf, 0, buf.Length)) > 0) ms.Write(buf, 0, n);
                        data = ms.ToArray();
                    }
                }
                return data;
            }
        }

        /// <summary>Icon used for windows (taskbar, Alt+Tab, dialogs).</summary>
        public static ImageSource Wpf() {
            if (windowIcon == null) {
                byte[] d = Data;
                if (d == null) return null;
                BitmapDecoder dec = BitmapDecoder.Create(new MemoryStream(d), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                windowIcon = dec.Frames[0];
                windowIcon.Freeze();
            }
            return windowIcon;
        }

        /// <summary>Smallest embedded frame that is at least <paramref name="px"/> pixels wide.</summary>
        public static ImageSource Wpf(int px) {
            byte[] d = Data;
            if (d == null) return null;
            BitmapDecoder dec = BitmapDecoder.Create(new MemoryStream(d), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            BitmapFrame best = dec.Frames[dec.Frames.Count - 1];
            foreach (BitmapFrame f in dec.Frames) {
                if (f.PixelWidth >= px && f.PixelWidth <= best.PixelWidth) best = f;
            }
            best.Freeze();
            return best;
        }

        /// <summary>Notification-area icon at the system's small icon size.</summary>
        public static SD.Icon Tray() {
            byte[] d = Data;
            if (d == null) return SD.SystemIcons.Application;
            return new SD.Icon(new MemoryStream(d), SWF.SystemInformation.SmallIconSize);
        }
    }

    /// <summary>Shared control styles (parsed once by the main window, reused by dialogs).</summary>
    public static class Theme {
        public static System.Windows.Style Button;
        public static System.Windows.Style PrimaryButton;
        public static System.Windows.Style DangerButton;
        public static System.Windows.Style ComboBox;
        public static System.Windows.Style TextBox;
        public static System.Windows.Style Toggle;
    }
}
