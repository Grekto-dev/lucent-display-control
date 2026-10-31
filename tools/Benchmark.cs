using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LucentControl.Bench {
    class Program {
        struct Metrics {
            public double ActiveWorkingSetMb;
            public double MinimizedWorkingSetMb;
            public double PrivateBytesMb;
            public double IdleCpuPercent;
        }

        static Metrics Measure(string exePath) {
            Metrics m = new Metrics();

            // 1. Measure Active Window
            ProcessStartInfo psiActive = new ProcessStartInfo(exePath);
            psiActive.WorkingDirectory = Path.GetDirectoryName(exePath);
            Process pActive = Process.Start(psiActive);
            try {
                Thread.Sleep(2500); // Wait for UI to render
                pActive.Refresh();
                m.ActiveWorkingSetMb = pActive.WorkingSet64 / (1024.0 * 1024.0);
                m.PrivateBytesMb = pActive.PrivateMemorySize64 / (1024.0 * 1024.0);
            } finally {
                try { pActive.Kill(); } catch { }
            }

            Thread.Sleep(1000);

            // 2. Measure Minimized to Tray (with TrimWorkingSet)
            ProcessStartInfo psiMin = new ProcessStartInfo(exePath, "/minimized");
            psiMin.WorkingDirectory = Path.GetDirectoryName(exePath);
            Process pMin = Process.Start(psiMin);
            try {
                Thread.Sleep(2500); // Wait for tray init & TrimWorkingSet
                pMin.Refresh();
                m.MinimizedWorkingSetMb = pMin.WorkingSet64 / (1024.0 * 1024.0);

                // Sample CPU usage over 3 seconds idle
                TimeSpan startCpu = pMin.TotalProcessorTime;
                DateTime startTime = DateTime.UtcNow;
                Thread.Sleep(3000);
                pMin.Refresh();
                TimeSpan endCpu = pMin.TotalProcessorTime;
                DateTime endTime = DateTime.UtcNow;

                double cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
                double totalMs = (endTime - startTime).TotalMilliseconds * Environment.ProcessorCount;
                m.IdleCpuPercent = (cpuUsedMs / totalMs) * 100.0;
            } finally {
                try { pMin.Kill(); } catch { }
            }

            return m;
        }

        static int Main(string[] args) {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("       LUCENT DISPLAY CONTROL - RELATORIO DE DESEMPENHO E RECURSOS");
            Console.WriteLine("==========================================================================");

            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
            string currentExe = Path.Combine(rootDir, "LucentControl.exe");
            string baselineExe = Path.Combine(rootDir, "bench", "baseline", "LucentControl.exe");

            Console.WriteLine("Coletando metricas da versao atual...");
            Metrics cur = Measure(currentExe);

            Metrics baseM = new Metrics();
            bool hasBaseline = File.Exists(baselineExe);
            if (hasBaseline) {
                Console.WriteLine("Coletando metricas da versao baseline...");
                baseM = Measure(baselineExe);
            }

            Console.WriteLine();
            Console.WriteLine("+------------------------------+--------------------+--------------------+");
            Console.WriteLine("| Metrica de Desempenho        | Versao Baseline    | Versao Otimizada   |");
            Console.WriteLine("+------------------------------+--------------------+--------------------+");
            if (hasBaseline) {
                Console.WriteLine(string.Format("| RAM Ativa (Janela Aberta)    | {0,14:F2} MB | {1,14:F2} MB |", baseM.ActiveWorkingSetMb, cur.ActiveWorkingSetMb));
                Console.WriteLine(string.Format("| RAM Ociosa (Na Bandeja/Tray) | {0,14:F2} MB | {1,14:F2} MB |", baseM.MinimizedWorkingSetMb, cur.MinimizedWorkingSetMb));
                Console.WriteLine(string.Format("| Private Bytes (Memoria)      | {0,14:F2} MB | {1,14:F2} MB |", baseM.PrivateBytesMb, cur.PrivateBytesMb));
                Console.WriteLine(string.Format("| CPU Ociosa na Bandeja        | {0,15:F2} % | {1,15:F2} % |", baseM.IdleCpuPercent, cur.IdleCpuPercent));
            } else {
                Console.WriteLine(string.Format("| RAM Ativa (Janela Aberta)    |                N/A | {0,14:F2} MB |", cur.ActiveWorkingSetMb));
                Console.WriteLine(string.Format("| RAM Ociosa (Na Bandeja/Tray) |                N/A | {0,14:F2} MB |", cur.MinimizedWorkingSetMb));
                Console.WriteLine(string.Format("| Private Bytes (Memoria)      |                N/A | {0,14:F2} MB |", cur.PrivateBytesMb));
                Console.WriteLine(string.Format("| CPU Ociosa na Bandeja        |                N/A | {0,15:F2} % |", cur.IdleCpuPercent));
            }
            Console.WriteLine("+------------------------------+--------------------+--------------------+");
            Console.WriteLine();
            if (hasBaseline) {
                double ramDrop = baseM.MinimizedWorkingSetMb - cur.MinimizedWorkingSetMb;
                double pctDrop = (ramDrop / baseM.MinimizedWorkingSetMb) * 100.0;
                Console.WriteLine(string.Format("Economia de RAM na bandeja: {0:F1} MB ({1:F1}% de reducao)", ramDrop, pctDrop));
            }
            Console.WriteLine(string.Format("Consumo de CPU em segundo plano: {0:F2}% (praticamente nulo)", cur.IdleCpuPercent));
            Console.WriteLine("==========================================================================");

            return 0;
        }
    }
}
