using System;
using System.Windows;

namespace LucentControl {
    public class App : Application {
        [STAThread]
        public static void Main(string[] args) {
            bool startMinimized = false;
            foreach (string arg in args) {
                if (arg.Equals("/minimized", StringComparison.OrdinalIgnoreCase) || arg.Equals("-minimized", StringComparison.OrdinalIgnoreCase)) {
                    startMinimized = true;
                }
            }

            App app = new App();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            MainWindow win = new MainWindow(startMinimized);
            app.MainWindow = win;
            if (!startMinimized) {
                win.Show();
            }
            app.Run();
        }
    }
}
