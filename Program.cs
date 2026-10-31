using System;
using System.Windows;

namespace LucentControl {
    public class App : Application {
        [STAThread]
        public static void Main(string[] args) {
            string errPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            try {
                AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e) {
                    System.IO.File.WriteAllText(errPath, "AppDomain Unhandled: " + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "null"));
                };

                bool startMinimized = false;
                foreach (string arg in args) {
                    if (arg.Equals("/minimized", StringComparison.OrdinalIgnoreCase) || arg.Equals("-minimized", StringComparison.OrdinalIgnoreCase)) {
                        startMinimized = true;
                    }
                }

                App app = new App();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                app.DispatcherUnhandledException += delegate(object s, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
                    System.IO.File.WriteAllText(errPath, "Dispatcher Unhandled: " + (e.Exception != null ? e.Exception.ToString() : "null"));
                };

                MainWindow win = new MainWindow(startMinimized);
                app.MainWindow = win;
                if (!startMinimized) {
                    win.Show();
                }
                app.Run();
            } catch (Exception ex) {
                System.IO.File.WriteAllText(errPath, "Main Catch: " + ex.ToString());
            }
        }
    }
}
