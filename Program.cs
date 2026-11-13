using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LucentControl {
    public class App : Application {
        [STAThread]
        public static void Main(string[] args) {
            string errPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.WriteAllText(errPath, "Started with args: " + string.Join(" ", args) + "\n");
            try {
                AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e) {
                    File.WriteAllText(errPath, "AppDomain Unhandled: " + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "null"));
                };

                bool startMinimized = false;
                bool isRender = false;
                bool noElevate = false;
                foreach (string arg in args) {
                    if (arg.Equals("/minimized", StringComparison.OrdinalIgnoreCase) || arg.Equals("-minimized", StringComparison.OrdinalIgnoreCase)) {
                        startMinimized = true;
                    } else if (arg.Equals("/render", StringComparison.OrdinalIgnoreCase) || arg.Equals("-render", StringComparison.OrdinalIgnoreCase)) {
                        isRender = true;
                    } else if (arg.Equals("--no-elevate", StringComparison.OrdinalIgnoreCase)) {
                        noElevate = true;
                    }
                }

                if (!isRender && !noElevate) {
                    AppConfig bootCfg = AppConfig.Load();
                    if (bootCfg.RunAsAdmin && !AdminHelper.IsElevated()) {
                        string relaunchArgs = startMinimized ? "/minimized --no-elevate" : "--no-elevate";
                        if (AdminHelper.RelaunchElevated(relaunchArgs)) {
                            return;
                        }
                    }
                }

                App app = new App();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                app.DispatcherUnhandledException += delegate(object s, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
                    File.WriteAllText(errPath, "Dispatcher Unhandled: " + (e.Exception != null ? e.Exception.ToString() : "null"));
                };

                if (isRender) {
                    try {
                        MainWindow win = new MainWindow(false);
                        win.Show();
                        win.UpdateLayout();

                        Action<string> capture = delegate(string p) {
                            try {
                                win.UpdateLayout();
                                int w = (int)Math.Max(win.ActualWidth, 1140);
                                int h = (int)Math.Max(win.ActualHeight, 740);

                                RenderTargetBitmap rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
                                rtb.Render(win);

                                PngBitmapEncoder enc = new PngBitmapEncoder();
                                enc.Frames.Add(BitmapFrame.Create(rtb));
                                using (FileStream fs = File.Create(p)) {
                                    enc.Save(fs);
                                }
                            } catch (Exception ex) {
                                File.AppendAllText(errPath, "Capture err: " + ex.ToString() + "\n");
                            }
                        };

                        string outDir = @"C:\Users\pedro\.gemini\antigravity\brain\5410f106-a632-40f4-a222-56db17083a4a";
                        capture(Path.Combine(outDir, "render_dashboard_v2.png"));
                        win.SwitchView(5);
                        capture(Path.Combine(outDir, "render_settings_v2.png"));
                        if (win.scrollSettings != null) {
                            win.scrollSettings.ScrollToBottom();
                        }
                        win.UpdateLayout();
                        capture(Path.Combine(outDir, "render_settings_bottom.png"));
                        win.SwitchView(2);
                        capture(Path.Combine(outDir, "render_presets_v2.png"));
                        win.SwitchView(3);
                        capture(Path.Combine(outDir, "render_profiles_v2.png"));
                        win.SwitchView(6);
                        capture(Path.Combine(outDir, "render_help_v2.png"));

                        win.Close();
                    } catch (Exception ex) {
                        File.WriteAllText(errPath, "Render block err: " + ex.ToString());
                    }
                    return;
                }

                MainWindow mainWin = new MainWindow(startMinimized);
                app.MainWindow = mainWin;
                if (!startMinimized) {
                    mainWin.Show();
                }
                app.Run();
            } catch (Exception ex) {
                File.WriteAllText(errPath, "Main Catch: " + ex.ToString());
            }
        }
    }
}
