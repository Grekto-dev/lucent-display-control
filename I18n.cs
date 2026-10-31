using System;
using System.Collections.Generic;

namespace LucentControl {
    public enum AppLanguage {
        English = 0,
        Portuguese = 1,
        Spanish = 2,
        French = 3,
        Italian = 4,
        Chinese = 5,
        Japanese = 6
    }

    public static class I18n {
        public static AppLanguage CurrentLanguage = AppLanguage.English;
        public static AppLanguage CommittedLanguage = AppLanguage.English;

        public static readonly string[] LanguageNames = new string[] {
            "English",
            "Português (Brasil)",
            "Español",
            "Français",
            "Italiano",
            "中文 (Chinese)",
            "日本語 (Japanese)"
        };

        private static readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>() {
            { "NavDisplays", new string[] { "Displays & Controls", "Telas & Controles", "Pantallas y Controles", "Écrans et Contrôles", "Schermi e Controlli", "显示与控制", "ディスプレイとコントロール" } },
            { "NavSettings", new string[] { "Settings", "Configurações", "Configuración", "Paramètres", "Impostazioni", "设置", "設定" } },
            { "HeaderSubtitle", new string[] { "{0} active displays controlled via software", "{0} telas ativas controladas via software", "{0} pantallas activas controladas por software", "{0} écrans actifs contrôlés par logiciel", "{0} schermi attivi controllati via software", "{0} 个活动屏幕由软件控制", "{0} 台のアクティブ画面をソフトウェア制御中" } },
            { "AllDisplays", new string[] { "All Displays (Synced)", "Todas as Telas (Sincronizado)", "Todas las Pantallas (Sincronizado)", "Tous les Écrans (Synchronisé)", "Tutti gli Schermi (Sincronizzati)", "所有屏幕（同步）", "すべての画面（同期）" } },
            { "Monitor", new string[] { "Monitor", "Monitor", "Monitor", "Moniteur", "Monitor", "显示器", "モニター" } },
            { "Primary", new string[] { "Primary", "Principal", "Principal", "Principal", "Principale", "主显示器", "メイン" } },
            { "Secondary", new string[] { "Secondary", "Secundário", "Secundario", "Secondaire", "Secondario", "次要显示器", "サブ" } },
            { "SecLuminance", new string[] { "BRIGHTNESS & CONTRAST", "ILUMINAÇÃO & CONTRASTE", "BRILLO Y CONTRASTE", "LUMINOSITÉ ET CONTRASTE", "LUMINOSITÀ E CONTRASTO", "亮度与对比度", "明るさとコントラスト" } },
            { "Brightness", new string[] { "Brightness", "Brilho", "Brillo", "Luminosité", "Luminosità", "亮度", "明るさ" } },
            { "Contrast", new string[] { "Contrast", "Contraste", "Contraste", "Contraste", "Contrasto", "对比度", "コントラスト" } },
            { "Gamma", new string[] { "Gamma", "Gama", "Gamma", "Gamma", "Gamma", "伽玛", "ガンマ" } },
            { "SecColor", new string[] { "COLORS & TEMPERATURE", "CORES & TEMPERATURA", "COLORES Y TEMPERATURA", "COULEURS ET TEMPÉRATURE", "COLORI E TEMPERATURA", "色彩与色温", "色と色温度" } },
            { "ColorTemp", new string[] { "Color Temperature", "Temperatura de Cor", "Temperatura de Color", "Température de Couleur", "Temperatura Colore", "色温", "色温度" } },
            { "Red", new string[] { "Red (R)", "Vermelho (R)", "Rojo (R)", "Rouge (R)", "Rosso (R)", "红 (R)", "赤 (R)" } },
            { "Green", new string[] { "Green (G)", "Verde (G)", "Verde (G)", "Vert (G)", "Verde (G)", "绿 (G)", "緑 (G)" } },
            { "Blue", new string[] { "Blue (B)", "Azul (B)", "Azul (B)", "Bleu (B)", "Blu (B)", "蓝 (B)", "青 (B)" } },
            { "SecPresets", new string[] { "QUICK PRESETS", "MODOS RÁPIDOS", "MODOS RÁPIDOS", "PRÉRÉGLAGES RAPIDES", "PREIMPOSTAZIONI RAPIDE", "快速预设", "クイックプリセット" } },
            { "PresetDefault", new string[] { "☀ Default", "☀ Padrão", "☀ Predeterminado", "☀ Par défaut", "☀ Predefinito", "☀ 默认", "☀ デフォルト" } },
            { "PresetNight", new string[] { "🌙 Night", "🌙 Noite", "🌙 Noche", "🌙 Nuit", "🌙 Notte", "🌙 夜间", "🌙 夜間" } },
            { "PresetReading", new string[] { "📖 Reading", "📖 Leitura", "📖 Lectura", "📖 Lecture", "📖 Lettura", "📖 阅读", "📖 読書" } },
            { "PresetCinema", new string[] { "🎬 Cinema", "🎬 Cinema", "🎬 Cine", "🎬 Cinéma", "🎬 Cinema", "🎬 影院", "🎬 シネマ" } },
            { "PresetReset", new string[] { "🔄 Reset", "🔄 Redefinir", "🔄 Restablecer", "🔄 Réinitialiser", "🔄 Ripristina", "🔄 重置", "🔄 リセット" } },
            { "SettingsTitle", new string[] { "Settings", "Configurações", "Configuración", "Paramètres", "Impostazioni", "设置", "設定" } },
            { "SettingsSubtitle", new string[] { "Application preferences and system startup", "Preferências do aplicativo e inicialização do sistema", "Preferencias de la aplicación e inicio del sistema", "Préférences de l'application et démarrage", "Preferenze dell'applicazione e avvio", "应用程序偏好与系统自启", "アプリの設定とシステム起動" } },
            { "LangTitle", new string[] { "Display Language", "Idioma de Exibição", "Idioma de Visualización", "Langue d'Affichage", "Lingua dell'Interfaccia", "界面语言", "表示言語" } },
            { "LangDesc", new string[] { "Select language. Changes apply immediately for preview, but only persist if you click Apply or OK.", "Selecione o idioma. A troca é imediata para teste, mas só persiste ao clicar em Aplicar ou OK.", "Seleccione el idioma. Cambia de inmediato, pero solo se guarda al pulsar Aplicar u OK.", "Sélectionnez la langue. Changement immédiat, permanent uniquement en cliquant sur Appliquer ou OK.", "Seleziona la lingua. Cambia subito, ma viene salvata solo cliccando su Applica o OK.", "选择语言。选择后即时预览，点击“应用”或“确定”后永久保存。", "言語を選択。プレビューで即時反映されますが、「適用」または「OK」を押すまで保存されません。" } },
            { "StartWinTitle", new string[] { "Start with Windows", "Iniciar junto com o PC", "Iniciar con Windows", "Démarrer avec Windows", "Avvia con Windows", "开机自启", "Windows起動時に実行" } },
            { "StartWinDesc", new string[] { "Automatically launch Lucent when your computer turns on.", "Executar o Lucent automaticamente quando você ligar o computador.", "Ejecutar Lucent automáticamente al encender el equipo.", "Lancer automatiquement Lucent au démarrage de l'ordinateur.", "Avvia automaticamente Lucent all'accensione del computer.", "开机时自动启动 Lucent 应用程序。", "PC起動時に Lucent を自動的に起動します。" } },
            { "StartMinTitle", new string[] { "Start minimized to system tray", "Iniciar minimizado para a bandeja", "Iniciar minimizado en la bandeja", "Démarrer réduit dans la barre d'état", "Avvia ridotto a icona nella barra delle applicazioni", "启动时最小化到系统托盘", "システムトレイに最小化して起動" } },
            { "StartMinDesc", new string[] { "Launch silently in background without opening the main window.", "Inicia em segundo plano silenciosamente sem exibir a janela.", "Inicia en segundo plano sin mostrar la ventana principal.", "Se lance discrètement en arrière-plan sans ouvrir la fenêtre.", "Avvia silenziosamente in background senza aprire la finestra.", "静默自启，不弹出主窗口界面。", "メインウィンドウを開かずにバックグラウンドで起動します。" } },
            { "MinCloseTitle", new string[] { "Minimize to tray on window close", "Minimizar para a bandeja ao fechar a janela", "Minimizar a la bandeja al cerrar la ventana", "Réduire dans la barre d'état à la fermeture", "Riduci a icona alla chiusura della finestra", "关闭窗口时最小化到系统托盘", "ウィンドウを閉じた時にトレイに最小化" } },
            { "MinCloseDesc", new string[] { "Keeps running in the background. If unchecked, closing the window exits the program.", "Mantém ativo em segundo plano. Se desmarcado, fechar a janela encerra o programa.", "Mantiene la app en segundo plano. Si se desmarca, cerrar la ventana cierra el programa.", "Garde l'application active. Si décoché, fermer la fenêtre quitte le programme.", "Mantiene l'app in background. Se deselezionato, chiudendo la finestra si esce dal programma.", "在后台继续运行。如取消勾选，关闭窗口将彻底退出程序。", "バックグラウンドで常駐します。オフにすると、ウィンドウを閉じるとアプリが終了します。" } },
            { "AboutTitle", new string[] { "About Lucent Display Control", "Sobre o Lucent Display Control", "Acerca de Lucent Display Control", "À propos de Lucent Display Control", "Informazioni su Lucent Display Control", "关于 Lucent Display Control", "Lucent Display Control について" } },
            { "AboutVersion", new string[] { "Version 0.1.0", "Versão 0.1.0", "Versión 0.1.0", "Version 0.1.0", "Versione 0.1.0", "版本 0.1.0", "バージョン 0.1.0" } },
            { "AboutDesc", new string[] { "Modern video output, brightness, contrast, gamma and color controller for DisplayLink and integrated displays.", "Controle moderno de saída de vídeo, brilho, contraste, gama e cores para telas DisplayLink e integradas.", "Controlador moderno de vídeo, brillo, contraste, gama y color para DisplayLink y pantallas integradas.", "Contrôleur moderne de luminosité, contraste, gamma et couleur pour DisplayLink et écrans intégrés.", "Controller moderno di luminosità, contrasto, gamma e colore per DisplayLink e schermi integrati.", "针对 DisplayLink 与集成显示器的现代亮度、对比度、伽玛与色彩控制工具。", "DisplayLink および統合ディスプレイ向けの最新の明るさ・コントラスト・ガンマ・色温度調整ツール。" } },
            { "BtnApply", new string[] { "Apply", "Aplicar", "Aplicar", "Appliquer", "Applica", "应用", "適用" } },
            { "BtnOK", new string[] { "OK", "OK", "Aceptar", "OK", "OK", "确定", "OK" } },
            { "BtnCancel", new string[] { "Cancel", "Cancelar", "Cancelar", "Annuler", "Annulla", "取消", "キャンセル" } },
            { "TrayOpen", new string[] { "🖥 Open Lucent", "🖥 Abrir Lucent", "🖥 Abrir Lucent", "🖥 Ouvrir Lucent", "🖥 Apri Lucent", "🖥 打开 Lucent", "🖥 Lucentを開く" } },
            { "TraySettings", new string[] { "⚙ Settings", "⚙ Configurações", "⚙ Configuración", "⚙ Paramètres", "⚙ Impostazioni", "⚙ 设置", "⚙ 設定" } },
            { "TrayExit", new string[] { "❌ Exit", "❌ Sair", "❌ Salir", "❌ Quitter", "❌ Esci", "❌ 退出", "❌ 終了" } },
            { "Warm", new string[] { "Warm", "Quente", "Cálido", "Chaud", "Caldo", "暖色", "暖色" } },
            { "Cold", new string[] { "Cool", "Frio", "Frío", "Froid", "Freddo", "冷色", "寒色" } },
            { "Neutral", new string[] { "Neutral", "Neutro", "Neutro", "Neutre", "Neutro", "中性", "ニュートラル" } },
            { "SavedNotice", new string[] { "Settings saved successfully!", "Configurações salvas com sucesso!", "¡Configuración guardada correctamente!", "Paramètres enregistrés avec succès !", "Impostazioni salvate con successo!", "设置已成功保存！", "設定を正常に保存しました！" } }
        };

        public static string T(string key) {
            string[] values;
            if (Strings.TryGetValue(key, out values)) {
                int idx = (int)CurrentLanguage;
                if (idx >= 0 && idx < values.Length) {
                    return values[idx];
                }
                return values[0];
            }
            return key;
        }

        public static string T(string key, params object[] args) {
            string fmt = T(key);
            try {
                return string.Format(fmt, args);
            } catch {
                return fmt;
            }
        }
    }
}
