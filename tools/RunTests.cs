using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using LucentControl;

namespace LucentControl.Tests {
    class Program {
        private static int passed = 0;
        private static int failed = 0;

        private static void Run(string name, Action act) {
            try {
                act();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[PASS] ");
                Console.ResetColor();
                Console.WriteLine(name);
                passed++;
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[FAIL] ");
                Console.ResetColor();
                Console.WriteLine(name + ": " + ex.Message);
                failed++;
            }
        }

        [STAThread]
        static int Main(string[] args) {
            Console.WriteLine("=================================================");
            Console.WriteLine("  LUCENT DISPLAY CONTROL - SUITE DE TESTES");
            Console.WriteLine("=================================================");

            // 1. Icon Tests
            Run("AppIcon: Carregamento do icone WPF", delegate {
                var icon = AppIcon.Wpf();
                if (icon == null) throw new Exception("Wpf() retornou null");
                var icon32 = AppIcon.Wpf(32);
                if (icon32 == null) throw new Exception("Wpf(32) retornou null");
            });

            Run("AppIcon: Carregamento do icone da bandeja (Tray)", delegate {
                using (var trayIcon = AppIcon.Tray()) {
                    if (trayIcon == null) throw new Exception("Tray() retornou null");
                    if (trayIcon.Width <= 0 || trayIcon.Height <= 0) throw new Exception("Dimensoes invalidas no TrayIcon");
                }
            });

            // 2. Config & Atomic Persistence Tests
            string tempDir = Path.Combine(Path.GetTempPath(), "LucentTest_" + Guid.NewGuid().ToString("N"));
            AppPaths.DataDirOverride = tempDir;

            Run("Config: Persistencia atomica e novas propriedades (AutoSave, AutoApply, StartupProfile)", delegate {
                AppConfig cfg = new AppConfig();
                cfg.Language = AppLanguage.Portuguese;
                cfg.AutoSave = true;
                cfg.AutoApplyProfile = true;
                cfg.StartupProfile = "Gaming";
                cfg.MinimizeOnClose = false;
                cfg.Save();

                AppConfig reloaded = AppConfig.Load();
                if (reloaded.Language != AppLanguage.Portuguese) throw new Exception("Language mismatch");
                if (!reloaded.AutoSave) throw new Exception("AutoSave mismatch");
                if (!reloaded.AutoApplyProfile) throw new Exception("AutoApplyProfile mismatch");
                if (reloaded.StartupProfile != "Gaming") throw new Exception("StartupProfile mismatch");
                if (reloaded.MinimizeOnClose) throw new Exception("MinimizeOnClose mismatch");
            });

            // 3. ProfileManager CRUD & RestoreDefaults
            Run("Profiles: Criacao, Sobrescrita e RestoreDefaults", delegate {
                ProfileManager pm = new ProfileManager();
                if (pm.Profiles.Count != 8) throw new Exception("Contagem de presets padrao incorreta: " + pm.Profiles.Count);

                // Modificar Standard
                DisplaySettings mod = new DisplaySettings();
                mod.Brightness = 0.77;
                mod.Contrast = 1.33;
                pm.SaveProfile("Standard", mod);
                UserProfile std = pm.Find("Standard");
                if (!pm.IsModified(std)) throw new Exception("Standard deveria estar marcado como IsModified");

                // Adicionar Custom
                pm.SaveProfile("MeuPerfilCustom", mod);
                if (!pm.Exists("MeuPerfilCustom")) throw new Exception("MeuPerfilCustom nao foi salvo");

                // RestoreDefaults
                pm.RestoreDefaults();
                std = pm.Find("Standard");
                if (pm.IsModified(std)) throw new Exception("Standard nao deveria mais estar modificado apos RestoreDefaults");
                if (!pm.Exists("MeuPerfilCustom")) throw new Exception("Perfil customizado deveria ser mantido apos RestoreDefaults");
            });

            // 4. Erase All Data
            Run("Data: Erase All Data (Factory Reset)", delegate {
                if (!File.Exists(AppPaths.ConfigFile)) throw new Exception("ConfigFile deveria existir antes do erase");
                AppPaths.DeleteAll();
                if (File.Exists(AppPaths.ConfigFile)) throw new Exception("ConfigFile ainda existe apos DeleteAll");
            });

            // 5. DwmEngine 5x5 Matrix & Math
            Run("DwmEngine: Calculo da Matriz 5x5 e Identidade", delegate {
                float[] m = new float[25];
                DisplaySettings def = new DisplaySettings();
                DwmEngine.BuildMatrix(def, true, m);

                // Verifica diagonal principal (1, 1, 1, 1, 1)
                if (Math.Abs(m[0] - 1.0f) > 0.01f || Math.Abs(m[6] - 1.0f) > 0.01f || Math.Abs(m[12] - 1.0f) > 0.01f) {
                    throw new Exception("Matriz neutra nao possui diagonal correta");
                }

                // Matriz com contraste alterado
                DisplaySettings cSettings = new DisplaySettings();
                cSettings.Contrast = 1.5;
                DwmEngine.BuildMatrix(cSettings, true, m);
                if (Math.Abs(m[0] - 1.5f) > 0.01f) throw new Exception("Diagonal do contraste incorreta: " + m[0]);
            });

            // 6. Scheduler Logic Tests
            Run("Scheduler: Logica circadiana de time slots", delegate {
                TimeSpan[] slots = new TimeSpan[] {
                    new TimeSpan(8, 0, 0),   // Day
                    new TimeSpan(18, 30, 0), // Sunset
                    new TimeSpan(22, 0, 0)   // Night
                };

                // 10:00 -> Day (slot 0)
                if (SchedulerLogic.CurrentSlot(new TimeSpan(10, 0, 0), slots) != 0) throw new Exception("10:00 deveria ser Day");
                // 19:00 -> Sunset (slot 1)
                if (SchedulerLogic.CurrentSlot(new TimeSpan(19, 0, 0), slots) != 1) throw new Exception("19:00 deveria ser Sunset");
                // 23:30 -> Night (slot 2)
                if (SchedulerLogic.CurrentSlot(new TimeSpan(23, 30, 0), slots) != 2) throw new Exception("23:30 deveria ser Night");
                // 02:00 -> Night (slot 2, wrapping past midnight)
                if (SchedulerLogic.CurrentSlot(new TimeSpan(2, 0, 0), slots) != 2) throw new Exception("02:00 deveria ser Night");
            });

            // 7. Overlay Requirement
            Run("Overlay: NeedsOverlay condicao sob demanda", delegate {
                DisplaySettings neutral = new DisplaySettings();
                if (ScreenOverlay.NeedsOverlay(neutral)) throw new Exception("Display neutro nao precisa de overlay");

                DisplaySettings dimmed = new DisplaySettings();
                dimmed.Brightness = 0.8;
                if (!ScreenOverlay.NeedsOverlay(dimmed)) throw new Exception("Display com brilho 0.8 precisa de overlay");
            });

            // 8. Brightness 0-200% Range and Boost
            Run("Brightness: Escala 0 a 200% com centro em 100% (1.0) e ganho de matriz", delegate {
                DisplaySettings boost = new DisplaySettings();
                boost.Brightness = 2.0; // 200%
                float[] m = new float[25];
                DwmEngine.BuildMatrix(boost, true, m);
                if (Math.Abs(m[0] - 2.0f) > 0.01f) throw new Exception("Diagonal do ganho de brilho 200% incorreta: " + m[0]);

                if (!ScreenOverlay.NeedsOverlay(boost)) throw new Exception("Brilho 200% deve ativar Overlay para per-monitor");

                // Teste clamp no ProfileManager (suporta ate 2.00 sem truncar)
                DisplaySettings pSet = new DisplaySettings();
                pSet.Brightness = 1.85;
                ProfileManager pm = new ProfileManager();
                pm.SaveProfile("BoostTest", pSet);
                UserProfile p = pm.Find("BoostTest");
                if (p == null || Math.Abs(p.Brightness - 1.85) > 0.01) throw new Exception("ProfileManager nao preservou brilho 185%");
            });

            // 9. Manual Numeric Entry Parser & DwmEngine.ForceReapply
            Run("Input: SliderValueParser (% / K / virgula / clamp) e ForceReapply", delegate {
                double v;
                if (!SliderValueParser.TryParseSliderValue("145%", 0.0, 2.0, 100.0, false, out v) || Math.Abs(v - 1.45) > 0.001)
                    throw new Exception("Falha ao interpretar '145%'");

                if (!SliderValueParser.TryParseSliderValue("1,25", 0.6, 2.0, 1.0, true, out v) || Math.Abs(v - 1.25) > 0.001)
                    throw new Exception("Falha ao interpretar gamma '1,25'");

                if (!SliderValueParser.TryParseSliderValue("4800 K", 3000, 9000, 1.0, false, out v) || Math.Abs(v - 4800) > 0.01)
                    throw new Exception("Falha ao interpretar '4800 K'");

                if (!SliderValueParser.TryParseSliderValue("999", 0.0, 2.0, 100.0, false, out v) || Math.Abs(v - 2.0) > 0.001)
                    throw new Exception("Falha no clamp maximo (999 -> 2.0)");

                if (!SliderValueParser.TryParseSliderValue("-10", 0.5, 1.5, 100.0, false, out v) || Math.Abs(v - 0.5) > 0.001)
                    throw new Exception("Falha no clamp minimo (-10 -> 0.5)");

                if (SliderValueParser.TryParseSliderValue("abc", 0.0, 2.0, 100.0, false, out v))
                    throw new Exception("Texto invalido nao deveria ser aceito");

                DwmEngine.ForceReapply();
            });

            // Limpa diretorio temporario de testes
            AppPaths.DataDirOverride = null;
            try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }

            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine(string.Format("Resultado: {0} passaram, {1} falharam.", passed, failed));
            Console.WriteLine("=================================================");

            return failed == 0 ? 0 : 1;
        }
    }
}
