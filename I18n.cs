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
            // Navigation
            { "NavDashboard", new string[] { "Dashboard", "Início", "Panel", "Tableau de bord", "Dashboard", "仪表盘", "ダッシュボード" } },
            { "NavMonitors", new string[] { "Monitors", "Monitores", "Monitores", "Moniteurs", "Monitor", "显示器", "モニター" } },
            { "NavPresets", new string[] { "Presets", "Predefinições", "Preajustes", "Préréglages", "Preimpostazioni", "预设", "プリセット" } },
            { "NavProfiles", new string[] { "Profiles", "Perfis", "Perfiles", "Profils", "Profili", "配置文件", "プロファイル" } },
            { "NavScheduler", new string[] { "Scheduler", "Agendador", "Programador", "Planificateur", "Pianificatore", "计划任务", "スケジューラ" } },
            { "NavSettings", new string[] { "Settings", "Configurações", "Configuración", "Paramètres", "Impostazioni", "设置", "設定" } },
            { "NavHelp", new string[] { "Help", "Ajuda", "Ayuda", "Aide", "Guida", "帮助", "ヘルプ" } },

            // Header & General
            { "MonitorsTitle", new string[] { "Monitors", "Monitores", "Monitores", "Moniteurs", "Monitor", "显示器", "モニター" } },
            { "MonitorsSubtitle", new string[] { "Select a display to adjust its settings", "Selecione uma tela para ajustar suas configurações", "Seleccione una pantalla para ajustar su configuración", "Sélectionnez un écran pour ajuster ses paramètres", "Seleziona uno schermo per regolarne le impostazioni", "选择要调整设置的显示器", "設定を調整するディスプレイを選択" } },
            { "MonitorsConnected", new string[] { "{0} monitors connected", "{0} monitores conectados", "{0} monitores conectados", "{0} moniteurs connectés", "{0} monitor connessi", "{0} 台显示器已连接", "{0} 台のモニターが接続されています" } },
            { "RunningInTray", new string[] { "Running in system tray", "Executando na bandeja", "Ejecutando en la bandeja", "Actif dans la barre d'état", "In esecuzione nella barra", "在系统托盘运行中", "システムトレイで実行中" } },

            { "Identify", new string[] { "Identify", "Identificar", "Identificar", "Identifier", "Identifica", "识别屏幕", "識別" } },
            { "MinimizeToTray", new string[] { "Minimize to tray", "Minimizar para bandeja", "Minimizar a bandeja", "Réduire dans la barre", "Riduci a icona", "最小化到托盘", "トレイに最小化" } },

            { "AllDisplays", new string[] { "All Displays (Synced)", "Todas as Telas (Sincronizado)", "Todas las Pantallas (Sincronizado)", "Tous les Écrans (Synchronisé)", "Tutti gli Schermi (Sincronizzati)", "所有屏幕（同步）", "すべての画面（同期）" } },
            { "Monitor", new string[] { "Monitor", "Monitor", "Monitor", "Moniteur", "Monitor", "显示器", "モニター" } },
            { "Primary", new string[] { "Primary", "Principal", "Principal", "Principal", "Principale", "主显示器", "メイン" } },
            { "Secondary", new string[] { "Secondary", "Secundário", "Secundario", "Secondaire", "Secondario", "次要显示器", "サブ" } },

            // Sliders & Controls
            { "Brightness", new string[] { "Brightness", "Brilho", "Brillo", "Luminosité", "Luminosità", "亮度", "明るさ" } },
            { "Contrast", new string[] { "Contrast", "Contraste", "Contraste", "Contraste", "Contrasto", "对比度", "コントラスト" } },
            { "Gamma", new string[] { "Gamma", "Gama", "Gamma", "Gamma", "Gamma", "伽玛", "ガンマ" } },
            { "Vibrance", new string[] { "Color Vibrance", "Vibração de Cor", "Intensidad de Color", "Éclat des Couleurs", "Vividezza Colore", "色彩鲜艳度", "色の鮮やかさ" } },

            { "ColorTemp", new string[] { "Color Temperature", "Temperatura de Cor", "Temperatura de Color", "Température de Couleur", "Temperatura Colore", "色温", "色温度" } },
            { "Red", new string[] { "Red", "Vermelho", "Rojo", "Rouge", "Rosso", "红", "赤" } },
            { "Green", new string[] { "Green", "Verde", "Verde", "Vert", "Verde", "绿", "緑" } },
            { "Blue", new string[] { "Blue", "Azul", "Azul", "Bleu", "Blu", "蓝", "青" } },

            // Presets
            { "QuickPresets", new string[] { "Quick Presets", "Modos Rápidos", "Modos Rápidos", "Préréglages Rapides", "Preimpostazioni Rapide", "快速预设", "クイックプリセット" } },
            { "QuickPresetsDesc", new string[] { "Apply a color profile instantly", "Aplique um perfil de cores instantaneamente", "Aplique un perfil de color al instante", "Appliquez un profil de couleur instantanément", "Applica un profilo colore all'istante", "即刻应用色彩配置", "カラープロファイルを即座に適用" } },
            { "ApplyProfileInstantly", new string[] { "Apply a color profile instantly", "Aplique um perfil de cor instantaneamente", "Aplique un perfil de color al instante", "Appliquez un profil de couleur instantanément", "Applica un profilo colore all'istante", "即刻应用色彩配置", "カラープロファイルを即座に適用" } },

            { "PresetStandard", new string[] { "Standard", "Padrão", "Estándar", "Standard", "Standard", "标准", "標準" } },
            { "PresetWarm", new string[] { "☀ Warm", "☀ Quente", "☀ Cálido", "☀ Chaud", "☀ Caldo", "☀ 暖色", "☀ 暖色" } },
            { "PresetNeutral", new string[] { "🎯 Neutral", "🎯 Neutro", "🎯 Neutro", "🎯 Neutre", "🎯 Neutro", "🎯 中性", "🎯 ニュートラル" } },
            { "PresetCool", new string[] { "❄ Cool", "❄ Frio", "❄ Frío", "❄ Froid", "❄ Freddo", "❄ 冷色", "❄ 寒色" } },
            { "PresetMovie", new string[] { "🎬 Movie", "🎬 Cinema", "🎬 Cine", "🎬 Cinéma", "🎬 Cinema", "🎬 影院", "🎬 シネマ" } },
            { "PresetWork", new string[] { "🖥 Work", "🖥 Trabalho", "🖥 Trabajo", "🖥 Bureau", "🖥 Lavoro", "🖥 办公", "🖥 仕事" } },
            { "PresetNight", new string[] { "🌙 Night", "🌙 Noite", "🌙 Noche", "🌙 Nuit", "🌙 Notte", "🌙 夜间", "🌙 夜間" } },
            { "PresetGaming", new string[] { "🎮 Gaming", "🎮 Jogos", "🎮 Juegos", "🎮 Jeux", "🎮 Giochi", "🎮 游戏", "🎮 ゲーム" } },
            { "PresetFocus", new string[] { "🖋 Focus", "🖋 Foco P&B", "🖋 Enfoque", "🖋 Concentration", "🖋 Concentrazione", "🖋 专注黑白", "🖋 フォーカス" } },

            // Footer
            { "ActiveProfile", new string[] { "Active Profile", "Perfil Ativo", "Perfil Activo", "Profil Actif", "Profilo Attivo", "当前配置", "アクティブプロファイル" } },
            { "ResetToDefault", new string[] { "Reset to Default", "Redefinir Padrões", "Restablecer Valores", "Réinitialiser", "Ripristina Predefiniti", "重置为默认值", "デフォルトに戻す" } },
            { "ApplyToMonitor", new string[] { "Apply to {0}", "Aplicar a {0}", "Aplicar a {0}", "Appliquer à {0}", "Applica a {0}", "应用到 {0}", "{0} に適用" } },

            // Monitors Tab Specifics
            { "MonHardwareTitle", new string[] { "Hardware & Display Topology", "Hardware & Topologia de Vídeo", "Hardware y Topología de Pantallas", "Matériel et Topologie d'Affichage", "Hardware e Topologia Schermi", "硬件与显示拓扑", "ハードウェアとディスプレイトポロジ" } },
            { "MonHardwareSubtitle", new string[] { "Real-time technical specifications and calibration assistant", "Especificações técnicas em tempo real e assistente de calibração", "Especificaciones técnicas en tiempo real y asistente de calibración", "Spécifications techniques en direct et assistant de calibrage", "Specifiche tecniche in tempo reale e calibrazione", "实时技术规格与校准向导", "リアルタイム技術仕様とキャリブレーションウィザード" } },
            { "AdapterName", new string[] { "Graphics Adapter", "Adaptador Gráfico", "Adaptador Gráfico", "Carte Graphique", "Scheda Grafica", "显卡适配器", "グラフィックアダプタ" } },
            { "Resolution", new string[] { "Native Resolution", "Resolução Ativa", "Resolución Activa", "Résolution Native", "Risoluzione Attiva", "当前分辨率", "ネイティブ解像度" } },
            { "RefreshRate", new string[] { "Refresh Rate", "Taxa de Atualização", "Tasa de Refresco", "Taux de Rafraîchissement", "Frequenza di Aggiornamento", "刷新率", "リフレッシュレート" } },
            { "DesktopBounds", new string[] { "Desktop Position", "Posição na Área de Trabalho", "Posición de Escritorio", "Position sur le Bureau", "Posizione Desktop", "桌面坐标位置", "デスクトップ座標" } },
            { "ColorDepth", new string[] { "Color Depth", "Profundidade de Cores", "Profundidad de Color", "Profondeur de Couleur", "Profondità Colore", "颜色深度", "色深度" } },
            { "CalibrationTitle", new string[] { "Visual Display Calibration", "Calibração Visual de Imagem", "Calibración Visual de Imagen", "Calibrage Visuel de l'Écran", "Calibrazione Visiva Schermo", "视觉显示校准", "視覚ディスプレイ調整" } },
            { "CalibrationDesc", new string[] { "Display fullscreen test patterns to verify black level, gamma linearity, and color balance without distortion. Press ESC to exit test.", "Exiba padrões de teste em tela cheia para verificar nível de preto, linearidade de gama e balanço de cores. Pressione ESC para sair.", "Muestre patrones de prueba en pantalla completa para verificar nivel de negro y gama. Pulse ESC para salir.", "Affichez des motifs plein écran pour vérifier les noirs et le gamma. Appuyez sur Échap pour quitter.", "Visualizza pattern di test a schermo intero per verificare nero e gamma. Premi ESC per uscire.", "全屏显示测试图案以检查黑电平与伽玛线性度。按 ESC 退出测试。", "フルスクリーンのテストパターンを表示してガンマと黒レベルを確認します。ESCキーで終了します。" } },
            { "PatWhite", new string[] { "⬜ White Screen", "⬜ Tela Branca", "⬜ Pantalla Blanca", "⬜ Écran Blanc", "⬜ Schermo Bianco", "⬜ 全白画面", "⬜ 白画面" } },
            { "PatBlack", new string[] { "⬛ Black Screen", "⬛ Tela Preta", "⬛ Pantalla Negra", "⬛ Écran Noir", "⬛ Schermo Nero", "⬛ 全黑画面", "⬛ 黒画面" } },
            { "PatGray", new string[] { "🏁 Grayscale Steps", "🏁 Escala de Cinza", "🏁 Escala de Grises", "🏁 Échelle de Gris", "🏁 Scala di Grigi", "🏁 灰阶梯度", "🏁 グレースケール" } },
            { "PatColor", new string[] { "🎨 Color Bars", "🎨 Barras de Cores", "🎨 Barras de Color", "🎨 Barres de Couleur", "🎨 Barre di Colore", "🎨 彩条测试", "🎨 カラーバー" } },

            // Profiles Tab
            { "ProfilesTitle", new string[] { "Color & Display Profiles", "Perfis de Cor e Imagem", "Perfiles de Color e Imagen", "Profils de Couleur", "Profili Colore", "色彩与显示配置文件", "カラープロファイル管理" } },
            { "ProfilesSubtitle", new string[] { "Save customized monitor configurations and switch between them instantly", "Salve configurações personalizadas e alterne entre elas instantaneamente", "Guarde configuraciones personalizadas y cambie entre ellas", "Enregistrez des configurations et basculez instantanément", "Salva configurazioni personalizzate e cambia subito", "保存自定义配置并即刻切换", "カスタム設定を保存して瞬時に切り替え" } },
            { "BtnActivateProfile", new string[] { "▶ Activate Profile", "▶ Ativar Perfil", "▶ Activar Perfil", "▶ Activer le Profil", "▶ Attiva Profilo", "▶ 激活此配置", "▶ プロファイルを適用" } },
            { "BtnSaveToProfile", new string[] { "💾 Overwrite with Current", "💾 Gravar Configuração Atual", "💾 Guardar Actual en Perfil", "💾 Écraser avec l'Actuel", "💾 Salva Corrente", "💾 用当前设置覆盖", "💾 現在の設定で上書き" } },
            { "NewProfileTitle", new string[] { "Create New Custom Profile", "Criar Novo Perfil Personalizado", "Crear Nuevo Perfil", "Créer un Nouveau Profil", "Crea Nuovo Profilo", "新建自定义配置文件", "新規プロファイルを作成" } },
            { "NewProfileBtn", new string[] { "➕ Create Profile", "➕ Criar Perfil", "➕ Crear Perfil", "➕ Créer le Profil", "➕ Crea Profilo", "➕ 新建配置", "➕ プロファイル作成" } },

            // Scheduler Tab
            { "SchedulerTitle", new string[] { "Automated Day & Night Scheduler", "Agendador Automático Dia & Noite", "Programador Automático Día y Noche", "Planificateur Automatique Jour & Nuit", "Pianificatore Automatico Giorno e Notte", "昼夜自动计划任务", "昼夜自動スケジューラ" } },
            { "SchedulerSubtitle", new string[] { "Automatically shift screen brightness and color temperature at scheduled times", "Alterne automaticamente o brilho e a temperatura de cor nos horários programados", "Cambie automáticamente el brillo y la temperatura según la hora", "Ajustez automatiquement la luminosité et la température selon l'heure", "Cambia automaticamente luminosità e temperatura negli orari stabiliti", "在预设时间自动调整屏幕亮度与色温", "スケジュールされた時刻に画面の明るさと色温度を自動調整" } },
            { "EnableScheduler", new string[] { "Enable Automatic Scheduler in background", "Ativar Agendamento Automático em segundo plano", "Activar Programador Automático en segundo plano", "Activer le planificateur automatique en arrière-plan", "Abilita Pianificatore Automatico in background", "启用后台自动定时调整", "バックグラウンドで自動スケジュールを有効にする" } },
            { "SchedulerStatusActive", new string[] { "● Scheduler Active: Next automatic shift scheduled", "● Agendador Ativo: Próxima transição agendada", "● Programador Activo: Próximo cambio programado", "● Planificateur Actif : Prochaine transition programmée", "● Pianificatore Attivo: Prossimo cambio pianificato", "● 计划任务运行中：下次自动切换已就绪", "● スケジューラ有効：次回の切り替えが待機中" } },
            { "SlotDay", new string[] { "☀️ Day Mode", "☀️ Modo Diurno", "☀️ Modo Diurno", "☀️ Mode Jour", "☀️ Modalità Giorno", "☀️ 日间模式", "☀️ 昼モード" } },
            { "SlotSunset", new string[] { "🌅 Sunset Mode", "🌅 Modo Entardecer", "🌅 Modo Atardecer", "🌅 Mode Coucher de Soleil", "🌅 Modalità Tramonto", "🌅 日落模式", "🌅 夕方モード" } },
            { "SlotNight", new string[] { "🌙 Night Mode", "🌙 Modo Noturno", "🌙 Modo Nocturno", "🌙 Mode Nuit", "🌙 Modalità Notte", "🌙 夜间模式", "🌙 夜モード" } },
            { "NotifyOnShift", new string[] { "Show system tray notification when profile shifts", "Exibir notificação na bandeja ao trocar o perfil", "Mostrar notificación en la bandeja al cambiar perfil", "Afficher une notification lors du changement de profil", "Mostra notifica nella barra al cambio profilo", "自动切换配置时在系统托盘弹出通知", "プロファイル切り替え時にトレイ通知を表示" } },
            { "BtnSaveScheduler", new string[] { "💾 Save Schedule", "💾 Salvar Agendamento", "💾 Guardar Horarios", "💾 Enregistrer le Planning", "💾 Salva Pianificazione", "💾 保存计划设置", "💾 スケジュールを保存" } },

            // Settings Tab
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
            { "AboutVersion", new string[] { "Version 0.2.0", "Versão 0.2.0", "Versión 0.2.0", "Version 0.2.0", "Versione 0.2.0", "版本 0.2.0", "バージョン 0.2.0" } },
            { "AboutDesc", new string[] { "Modern video output, brightness, contrast, gamma and color controller for DisplayLink and integrated displays.", "Controle moderno de saída de vídeo, brilho, contraste, gama e cores para telas DisplayLink e integradas.", "Controlador moderno de vídeo, brillo, contraste, gama y color para DisplayLink y pantallas integradas.", "Contrôleur moderne de luminosité, contraste, gamma et couleur pour DisplayLink et écrans intégrés.", "Controller moderno di luminosità, contrasto, gamma e colore per DisplayLink e schermi integrati.", "针对 DisplayLink 与集成显示器的现代亮度、对比度、伽玛与色彩控制工具。", "DisplayLink および統合ディスプレイ向けの最新の明るさ・コントラスト・ガンマ・色温度調整ツール。" } },

            { "BtnApply", new string[] { "Apply", "Aplicar", "Aplicar", "Appliquer", "Applica", "应用", "適用" } },
            { "BtnOK", new string[] { "OK", "OK", "Aceptar", "OK", "OK", "确定", "OK" } },
            { "BtnCancel", new string[] { "Cancel", "Cancelar", "Cancelar", "Annuler", "Annulla", "取消", "キャンセル" } },

            // Tray Menu
            { "TrayOpen", new string[] { "🖥 Open Lucent", "🖥 Abrir Lucent", "🖥 Abrir Lucent", "🖥 Ouvrir Lucent", "🖥 Apri Lucent", "🖥 打开 Lucent", "🖥 Lucentを開く" } },
            { "TraySettings", new string[] { "⚙ Settings", "⚙ Configurações", "⚙ Configuración", "⚙ Paramètres", "⚙ Impostazioni", "⚙ 设置", "⚙ 設定" } },
            { "TrayExit", new string[] { "❌ Exit", "❌ Sair", "❌ Salir", "❌ Quitter", "❌ Esci", "❌ 退出", "❌ 終了" } },

            { "Warm", new string[] { "Warm", "Quente", "Cálido", "Chaud", "Caldo", "暖色", "暖色" } },
            { "Cold", new string[] { "Cool", "Frio", "Frío", "Froid", "Freddo", "冷色", "寒色" } },
            { "Neutral", new string[] { "Neutral", "Neutro", "Neutro", "Neutre", "Neutro", "中性", "ニュートラル" } },
            { "SavedNotice", new string[] { "Settings saved successfully!", "Configurações salvas com sucesso!", "¡Configuración guardada correctamente!", "Paramètres enregistrés avec succès !", "Impostazioni salvate con successo!", "设置已成功保存！", "設定を正常に保存しました！" } },

            // Extra keys for Presets, Profiles & Scheduler
            { "PresetsTitle", new string[] { "Color Presets & Modes", "Modos & Predefinições de Cor", "Preajustes y Modos de Color", "Préréglages et Modes de Couleur", "Preimpostazioni e Modalità Colore", "色彩预设与模式", "カラープリセットとモード" } },
            { "PresetsSubtitle", new string[] { "Select an optimized profile or create a custom one", "Selecione um perfil otimizado ou crie um personalizado", "Seleccione un perfil optimizado o cree uno personalizado", "Sélectionnez un profil optimisé ou créez-en un", "Seleziona un profilo ottimizzato o creane uno", "选择针对性优化的预设配置或创建自定义模式", "最適化されたプロファイルを選択またはカスタム作成" } },
            { "BtnDeleteProfile", new string[] { "Delete", "Excluir", "Eliminar", "Supprimer", "Elimina", "删除", "削除" } },
            { "BtnApplyAll", new string[] { "Apply to All", "Aplicar a Todos", "Aplicar a Todos", "Appliquer à Tous", "Applica a Tutti", "应用到全部显示器", "すべてのディスプレイに適用" } },
            { "BtnApplySelected", new string[] { "Apply to Selected", "Aplicar ao Selecionado", "Aplicar al Seleccionado", "Appliquer au Sélectionné", "Applica al Selezionato", "应用到当前选中", "選択中のディスプレイに適用" } },
            { "TestPatternNotice", new string[] { "Click anywhere or press ESC to exit test pattern", "Clique em qualquer lugar ou pressione ESC para sair do padrão de teste", "Haga clic en cualquier lugar o presione ESC para salir", "Cliquez n'importe où ou appuyez sur Échap pour quitter", "Fai clic ovunque o premi ESC per uscire dal pattern", "点击任意位置或按 ESC 退出测试图案", "任意の場所をクリックするか ESC キーを押して終了" } },
            { "PresetCreated", new string[] { "Custom preset created!", "Predefinição criada com sucesso!", "¡Preajuste creado correctamente!", "Préréglage créé avec succès !", "Preimpostazione creata con successo!", "自定义预设已创建！", "カスタムプリセットを作成しました！" } },
            { "ProfileSaved", new string[] { "Profile saved successfully!", "Perfil salvo com sucesso!", "¡Perfil guardado correctamente!", "Profil enregistré avec succès !", "Profilo salvato con successo!", "配置文件已成功保存！", "プロファイルを保存しました！" } },
            { "ScheduleSaved", new string[] { "Schedule saved successfully!", "Agendamento salvo com sucesso!", "¡Horarios guardados correctamente!", "Planning enregistré avec succès !", "Pianificazione salvata con successo!", "计划任务已成功保存！", "スケジュールを保存しました！" } },
            { "RefreshMonitors", new string[] { "Refresh Monitors", "Atualizar Telas", "Actualizar Pantallas", "Actualiser les Écrans", "Aggiorna Monitor", "刷新显示器", "モニターを再検出" } },

            // Profile selection & saving
            { "ProfileUnsaved", new string[] { "Custom (unsaved)", "Personalizado (não salvo)", "Personalizado (sin guardar)", "Personnalisé (non enregistré)", "Personalizzato (non salvato)", "自定义（未保存）", "カスタム（未保存）" } },
            { "ProfileNew", new string[] { "+ New profile…", "+ Novo perfil…", "+ Nuevo perfil…", "+ Nouveau profil…", "+ Nuovo profilo…", "+ 新建配置…", "+ 新規プロファイル…" } },
            { "BtnSave", new string[] { "Save", "Salvar", "Guardar", "Enregistrer", "Salva", "保存", "保存" } },
            { "BtnSaveAsNew", new string[] { "Save as New", "Salvar como Novo", "Guardar como Nuevo", "Enregistrer sous", "Salva come nuovo", "另存为", "新規保存" } },
            { "BtnApplyPreset", new string[] { "Apply", "Aplicar", "Aplicar", "Appliquer", "Applica", "应用", "適用" } },
            { "BtnSaveProfile", new string[] { "Save Profile", "Salvar Perfil", "Guardar Perfil", "Enregistrer le Profil", "Salva Profilo", "保存配置", "プロファイルを保存" } },
            { "SaveTip", new string[] { "Save changes to the active profile", "Salvar alterações no perfil ativo", "Guardar cambios en el perfil activo", "Enregistrer les modifications dans le profil actif", "Salva le modifiche nel profilo attivo", "将更改保存到当前配置", "変更をアクティブなプロファイルに保存" } },
            { "SaveAsTip", new string[] { "Save as a new profile…", "Salvar como novo perfil…", "Guardar como nuevo perfil…", "Enregistrer comme nouveau profil…", "Salva come nuovo profilo…", "另存为新配置…", "新しいプロファイルとして保存…" } },
            { "UnsavedTip", new string[] { "Unsaved changes", "Alterações não salvas", "Cambios sin guardar", "Modifications non enregistrées", "Modifiche non salvate", "有未保存的更改", "未保存の変更があります" } },
            { "PromptNameTitle", new string[] { "Save profile", "Salvar perfil", "Guardar perfil", "Enregistrer le profil", "Salva profilo", "保存配置", "プロファイルを保存" } },
            { "PromptNameMsg", new string[] { "Enter a name for the new profile:", "Digite um nome para o novo perfil:", "Introduzca un nombre para el nuevo perfil:", "Saisissez un nom pour le nouveau profil :", "Inserisci un nome per il nuovo profilo:", "请输入新配置的名称：", "新しいプロファイルの名前を入力してください:" } },
            { "DefaultNewName", new string[] { "My profile", "Meu perfil", "Mi perfil", "Mon profil", "Il mio profilo", "我的配置", "マイプロファイル" } },
            { "OverwriteMsg", new string[] { "A profile named \"{0}\" already exists. Overwrite it?", "Já existe um perfil chamado \"{0}\". Deseja substituí-lo?", "Ya existe un perfil llamado \"{0}\". ¿Desea sobrescribirlo?", "Un profil nommé « {0} » existe déjà. Voulez-vous le remplacer ?", "Esiste già un profilo chiamato \"{0}\". Sovrascriverlo?", "已存在名为“{0}”的配置。要覆盖吗？", "「{0}」というプロファイルは既に存在します。上書きしますか？" } },
            { "BtnOverwrite", new string[] { "Overwrite", "Substituir", "Sobrescribir", "Remplacer", "Sovrascrivi", "覆盖", "上書き" } },
            { "BtnConfirm", new string[] { "Confirm", "Confirmar", "Confirmar", "Confirmer", "Conferma", "确认", "確認" } },
            { "SavedTo", new string[] { "Saved to \"{0}\"", "Salvo em \"{0}\"", "Guardado en \"{0}\"", "Enregistré dans « {0} »", "Salvato in \"{0}\"", "已保存到“{0}”", "「{0}」に保存しました" } },

            // Settings: saving, startup profile, data
            { "AutoSaveTitle", new string[] { "Save changes automatically", "Salvar alterações automaticamente", "Guardar cambios automáticamente", "Enregistrer automatiquement les modifications", "Salva automaticamente le modifiche", "自动保存更改", "変更を自動的に保存" } },
            { "AutoSaveDesc", new string[] { "Every change is saved to the active profile. If off, use the Save button. The first save asks for a profile name.", "Cada alteração é salva no perfil ativo. Se desligado, use o botão Salvar. O primeiro salvamento pede o nome do perfil.", "Cada cambio se guarda en el perfil activo. Si está desactivado, use el botón Guardar. El primer guardado solicita un nombre de perfil.", "Chaque modification est enregistrée dans le profil actif. Sinon, utilisez le bouton Enregistrer. Le premier enregistrement demande un nom de profil.", "Ogni modifica viene salvata nel profilo attivo. Se disattivato, usa il pulsante Salva. Il primo salvataggio richiede il nome del profilo.", "每次更改都会保存到当前配置。关闭后请使用“保存”按钮。首次保存时会询问配置名称。", "変更はすべてアクティブなプロファイルに保存されます。オフの場合は［保存］ボタンを使用します。初回保存時にプロファイル名を入力します。" } },
            { "AutoApplyTitle", new string[] { "Apply a profile at startup", "Aplicar um perfil ao iniciar", "Aplicar un perfil al iniciar", "Appliquer un profil au démarrage", "Applica un profilo all'avvio", "启动时自动应用配置", "起動時にプロファイルを適用" } },
            { "AutoApplyDesc", new string[] { "Automatically load the selected profile every time Lucent starts.", "Carrega automaticamente o perfil selecionado sempre que o Lucent iniciar.", "Carga automáticamente el perfil seleccionado cada vez que Lucent se inicia.", "Charge automatiquement le profil sélectionné à chaque démarrage de Lucent.", "Carica automaticamente il profilo selezionato a ogni avvio di Lucent.", "每次启动 Lucent 时自动加载所选配置。", "Lucent の起動時に選択したプロファイルを自動的に読み込みます。" } },
            { "StartupProfileLabel", new string[] { "Startup profile", "Perfil de inicialização", "Perfil de inicio", "Profil de démarrage", "Profilo di avvio", "启动配置", "起動プロファイル" } },
            { "DataTitle", new string[] { "Presets & data", "Predefinições e dados", "Preajustes y datos", "Préréglages et données", "Preimpostazioni e dati", "预设与数据", "プリセットとデータ" } },
            { "RestoreTitle", new string[] { "Restore default presets", "Restaurar predefinições padrão", "Restaurar preajustes predeterminados", "Restaurer les préréglages par défaut", "Ripristina preimpostazioni predefinite", "恢复默认预设", "既定のプリセットを復元" } },
            { "RestoreDesc", new string[] { "Resets the built-in presets to their original values. Your custom profiles are kept.", "Redefine as predefinições internas para os valores originais. Seus perfis personalizados são mantidos.", "Restablece los preajustes integrados a sus valores originales. Se conservan sus perfiles personalizados.", "Réinitialise les préréglages intégrés à leurs valeurs d'origine. Vos profils personnalisés sont conservés.", "Ripristina le preimpostazioni integrate ai valori originali. I profili personalizzati vengono mantenuti.", "将内置预设重置为原始值。自定义配置会保留。", "内蔵プリセットを元の値に戻します。カスタムプロファイルは保持されます。" } },
            { "BtnRestore", new string[] { "Restore presets", "Restaurar predefinições", "Restaurar preajustes", "Restaurer", "Ripristina", "恢复预设", "プリセットを復元" } },
            { "RestoreConfirm", new string[] { "Restore all built-in presets to their original values? Presets you changed will be overwritten. Custom profiles are not affected.", "Restaurar todas as predefinições internas para os valores originais? As que você alterou serão sobrescritas. Perfis personalizados não são afetados.", "¿Restaurar todos los preajustes integrados a sus valores originales? Los que haya modificado se sobrescribirán. Los perfiles personalizados no se ven afectados.", "Restaurer tous les préréglages intégrés à leurs valeurs d'origine ? Ceux que vous avez modifiés seront écrasés. Les profils personnalisés ne sont pas affectés.", "Ripristinare tutte le preimpostazioni integrate ai valori originali? Quelle modificate verranno sovrascritte. I profili personalizzati non sono interessati.", "要将所有内置预设恢复为原始值吗？您修改过的预设将被覆盖，自定义配置不受影响。", "すべての内蔵プリセットを元の値に戻しますか？変更したプリセットは上書きされます。カスタムプロファイルには影響しません。" } },
            { "RestoreDone", new string[] { "Default presets restored.", "Predefinições padrão restauradas.", "Preajustes predeterminados restaurados.", "Préréglages par défaut restaurés.", "Preimpostazioni predefinite ripristinate.", "已恢复默认预设。", "既定のプリセットを復元しました。" } },
            { "EraseTitle", new string[] { "Erase all data", "Apagar todos os dados", "Borrar todos los datos", "Effacer toutes les données", "Cancella tutti i dati", "清除所有数据", "すべてのデータを消去" } },
            { "EraseDesc", new string[] { "Deletes settings, profiles and schedules and returns the app to its out-of-the-box state.", "Exclui configurações, perfis e agendamentos e devolve o aplicativo ao estado de fábrica.", "Elimina configuración, perfiles y horarios y devuelve la aplicación a su estado original.", "Supprime paramètres, profils et plannings et remet l'application à son état d'origine.", "Elimina impostazioni, profili e pianificazioni e riporta l'app allo stato iniziale.", "删除设置、配置和计划，并将应用恢复为初始状态。", "設定、プロファイル、スケジュールを削除し、アプリを初期状態に戻します。" } },
            { "BtnErase", new string[] { "Erase data", "Apagar dados", "Borrar datos", "Effacer", "Cancella", "清除数据", "データを消去" } },
            { "EraseConfirm", new string[] { "This permanently deletes all settings, custom profiles and preset changes, and returns Lucent to its original state. This cannot be undone. Continue?", "Isto exclui permanentemente todas as configurações, perfis personalizados e alterações de predefinições, e devolve o Lucent ao estado original. Não é possível desfazer. Continuar?", "Esto elimina permanentemente toda la configuración, los perfiles personalizados y los cambios de preajustes, y devuelve Lucent a su estado original. No se puede deshacer. ¿Continuar?", "Cela supprime définitivement tous les paramètres, profils personnalisés et modifications de préréglages, et remet Lucent à son état d'origine. Action irréversible. Continuer ?", "Questo elimina definitivamente tutte le impostazioni, i profili personalizzati e le modifiche alle preimpostazioni e riporta Lucent allo stato originale. Operazione irreversibile. Continuare?", "这将永久删除所有设置、自定义配置和预设修改，并将 Lucent 恢复为初始状态。此操作无法撤销。是否继续？", "すべての設定、カスタムプロファイル、プリセットの変更が完全に削除され、Lucent は初期状態に戻ります。この操作は元に戻せません。続行しますか？" } },
            { "EraseDone", new string[] { "All data erased. Lucent is back to its original state.", "Todos os dados foram apagados. O Lucent voltou ao estado original.", "Datos borrados. Lucent volvió a su estado original.", "Données effacées. Lucent est revenu à son état d'origine.", "Dati cancellati. Lucent è tornato allo stato originale.", "所有数据已清除，Lucent 已恢复初始状态。", "すべてのデータを消去しました。Lucent は初期状態に戻りました。" } },

            // Presets / Profiles views
            { "TagBuiltIn", new string[] { "Built-in", "Padrão", "Integrado", "Intégré", "Integrato", "内置", "内蔵" } },
            { "TagCustom", new string[] { "Custom", "Personalizado", "Personalizado", "Personnalisé", "Personalizzato", "自定义", "カスタム" } },
            { "TagModified", new string[] { "Modified", "Modificado", "Modificado", "Modifié", "Modificato", "已修改", "変更済み" } },
            { "TagActive", new string[] { "Active", "Ativo", "Activo", "Actif", "Attivo", "当前", "使用中" } },
            { "ActiveBanner", new string[] { "Active profile: {0}", "Perfil ativo: {0}", "Perfil activo: {0}", "Profil actif : {0}", "Profilo attivo: {0}", "当前配置：{0}", "アクティブなプロファイル: {0}" } },
            { "NoActiveBanner", new string[] { "No saved profile is active — changes are not saved yet.", "Nenhum perfil salvo está ativo — as alterações ainda não foram salvas.", "Ningún perfil guardado está activo: los cambios aún no se han guardado.", "Aucun profil enregistré n'est actif — les modifications ne sont pas encore enregistrées.", "Nessun profilo salvato è attivo — le modifiche non sono ancora salvate.", "当前没有已保存的配置 — 更改尚未保存。", "保存済みのプロファイルが有効ではありません — 変更はまだ保存されていません。" } },
            { "CustomPresetDesc", new string[] { "Save your current brightness, contrast, gamma and color levels as a new reusable profile.", "Salve o brilho, contraste, gama e cores atuais como um novo perfil reutilizável.", "Guarde el brillo, contraste, gamma y color actuales como un nuevo perfil reutilizable.", "Enregistrez la luminosité, le contraste, le gamma et les couleurs actuels comme nouveau profil réutilisable.", "Salva luminosità, contrasto, gamma e colori attuali come nuovo profilo riutilizzabile.", "将当前的亮度、对比度、伽玛和色彩保存为可复用的新配置。", "現在の明るさ・コントラスト・ガンマ・色を新しいプロファイルとして保存します。" } },
            { "DescStandard", new string[] { "True color reproduction, neutral balance", "Reprodução fiel das cores, balanço neutro", "Reproducción fiel del color, equilibrio neutro", "Reproduction fidèle des couleurs, équilibre neutre", "Riproduzione fedele dei colori, bilanciamento neutro", "真实色彩还原，中性平衡", "忠実な色再現、ニュートラルなバランス" } },
            { "DescWarm", new string[] { "Gentle on the eyes, ideal for reading", "Suave para os olhos, ideal para leitura", "Suave para la vista, ideal para leer", "Doux pour les yeux, idéal pour lire", "Delicato per gli occhi, ideale per leggere", "柔和护眼，适合阅读", "目に優しく、読書に最適" } },
            { "DescCool", new string[] { "Crisp daylight colors with punchy whites", "Cores nítidas de luz do dia com brancos intensos", "Colores nítidos de luz diurna con blancos intensos", "Couleurs lumière du jour nettes, blancs éclatants", "Colori nitidi da luce diurna con bianchi intensi", "清晰的日光色调，白色更通透", "くっきりした昼光色と鮮やかな白" } },
            { "DescMovie", new string[] { "Deeper blacks and rich cinematic contrast", "Pretos mais profundos e contraste cinematográfico", "Negros más profundos y contraste cinematográfico", "Noirs plus profonds et contraste cinématographique", "Neri più profondi e contrasto cinematografico", "更深的黑色与电影级对比度", "深い黒と映画のようなコントラスト" } },
            { "DescWork", new string[] { "Comfortable daytime brightness for documents", "Brilho diurno confortável para documentos", "Brillo diurno cómodo para documentos", "Luminosité de jour confortable pour les documents", "Luminosità diurna confortevole per i documenti", "适合文档处理的舒适日间亮度", "ドキュメント作業に快適な昼間の明るさ" } },
            { "DescNight", new string[] { "Maximum blue-light reduction for late hours", "Máxima redução de luz azul para a noite", "Máxima reducción de luz azul para la noche", "Réduction maximale de la lumière bleue la nuit", "Massima riduzione della luce blu per la notte", "夜间最大程度减少蓝光", "夜間のブルーライトを最大限カット" } },
            { "DescGaming", new string[] { "Boosted shadows and saturation for games", "Sombras e saturação reforçadas para jogos", "Sombras y saturación reforzadas para juegos", "Ombres et saturation renforcées pour les jeux", "Ombre e saturazione potenziate per i giochi", "增强暗部与饱和度，适合游戏", "ゲーム向けに暗部と彩度を強調" } },
            { "DescFocus", new string[] { "Maximum contrast for coding and documents", "Contraste máximo para código e documentos", "Contraste máximo para código y documentos", "Contraste maximal pour le code et les documents", "Contrasto massimo per codice e documenti", "最高对比度，适合编程与文档", "コーディングや文書向けの最大コントラスト" } },

            // Monitors view
            { "PatWhiteSub", new string[] { "Backlight & uniformity", "Luz de fundo e uniformidade", "Retroiluminación y uniformidad", "Rétroéclairage et uniformité", "Retroilluminazione e uniformità", "背光与均匀度", "バックライトと均一性" } },
            { "PatBlackSub", new string[] { "Black level & glow", "Nível de preto e brilho", "Nivel de negro y brillo", "Niveau de noir et halo", "Livello del nero e bagliore", "黑位与漏光", "黒レベルと光漏れ" } },
            { "PatGraySub", new string[] { "16-step gamma ramp", "Rampa de gama em 16 níveis", "Rampa de gamma de 16 niveles", "Rampe gamma à 16 niveaux", "Rampa gamma a 16 livelli", "16 级伽玛渐变", "16段階ガンマランプ" } },
            { "PatColorSub", new string[] { "SMPTE color bars", "Barras de cor SMPTE", "Barras de color SMPTE", "Barres de couleur SMPTE", "Barre di colore SMPTE", "SMPTE 彩条", "SMPTE カラーバー" } },
            { "VideoEngine", new string[] { "Video output engine", "Motor de saída de vídeo", "Motor de salida de vídeo", "Moteur de sortie vidéo", "Motore di uscita video", "视频输出引擎", "映像出力エンジン" } },
            { "VideoEngineValue", new string[] { "DWM color matrix (hardware)", "Matriz de cores DWM (hardware)", "Matriz de color DWM (hardware)", "Matrice de couleurs DWM (matériel)", "Matrice colori DWM (hardware)", "DWM 颜色矩阵（硬件）", "DWM カラーマトリックス（ハードウェア）" } },
            { "BitsValue", new string[] { "{0}-bit color", "Cor de {0} bits", "Color de {0} bits", "Couleur {0} bits", "Colore a {0} bit", "{0} 位色", "{0} ビットカラー" } },

            // Scheduler
            { "TimeLabel", new string[] { "Time:", "Hora:", "Hora:", "Heure :", "Ora:", "时间：", "時刻:" } },
            { "ProfileLabel", new string[] { "Profile:", "Perfil:", "Perfil:", "Profil :", "Profilo:", "配置：", "プロファイル:" } },
            { "SchedulerPaused", new string[] { "Automatic scheduler is paused.", "O agendador automático está pausado.", "El programador automático está en pausa.", "Le planificateur automatique est en pause.", "Il pianificatore automatico è in pausa.", "自动计划已暂停。", "自動スケジューラは一時停止中です。" } },
            { "SchedulerActiveNow", new string[] { "Scheduler active — now: {0}", "Agendador ativo — agora: {0}", "Programador activo — ahora: {0}", "Planificateur actif — maintenant : {0}", "Pianificatore attivo — ora: {0}", "计划运行中 — 当前：{0}", "スケジューラ有効 — 現在: {0}" } },
            { "SchedNotify", new string[] { "Scheduled profile applied: {0}", "Perfil agendado aplicado: {0}", "Perfil programado aplicado: {0}", "Profil planifié appliqué : {0}", "Profilo pianificato applicato: {0}", "已应用计划配置：{0}", "スケジュールされたプロファイルを適用: {0}" } },
            { "ClickToEditValue", new string[] { "Click to enter value manually", "Clique para digitar o valor manualmente", "Haga clic para ingresar el valor manualmente", "Cliquez pour saisir la valeur manuellement", "Fai clic per inserire il valore manualmente", "点击手动输入数值", "クリックして数値を直接入力" } },

            // Hardware Gamma LUT Settings
            { "LutTitle", new string[] { "Hardware Gamma LUT (Start Menu, Taskbar & UAC)", "LUT de Gama por Hardware (Menu Iniciar, Barra e UAC)", "LUT de Gamma por Hardware (Inicio, Barra y UAC)", "LUT Gamma Matérielle (Menu Démarrer, Barre et UAC)", "LUT Gamma Hardware (Start, Barra e UAC)", "硬件伽玛 LUT（覆盖开始菜单、任务栏和 UAC）", "ハードウェア ガンマ LUT（スタート・タスクバー・UAC 対応）" } },
            { "LutDesc", new string[] { "Uses the GPU display LUT (SetDeviceGammaRamp) for brightness, contrast, gamma, temperature and RGB so adjustments stay active over Start Menu, Taskbar and UAC. Color Vibrance continues via DWM.", "Usa a tabela LUT do adaptador de vídeo (SetDeviceGammaRamp) para brilho, contraste, gama, temperatura e RGB, mantendo os ajustes sobre o Menu Iniciar, Barra de Tarefas e UAC. A Vibração de Cor continua via DWM.", "Usa la tabla LUT de GPU (SetDeviceGammaRamp) para brillo, contraste, gamma, temperatura y RGB para mantener los ajustes sobre Inicio, Barra de tareas y UAC. La intensidad sigue vía DWM.", "Utilise la LUT matérielle du GPU (SetDeviceGammaRamp) pour la luminosité, le contraste, le gamma, la température et le RVB afin de rester actif sur le Menu Démarrer, la Barre des tâches et l'UAC.", "Usa la LUT hardware GPU (SetDeviceGammaRamp) per luminosità, contrasto, gamma, temperatura e RGB così le modifiche restano attive su Start, Barra delle applicazioni e UAC.", "使用 GPU 硬件 LUT（SetDeviceGammaRamp）处理亮度、对比度、伽玛、色温和 RGB，使调整在开始菜单、任务栏和 UAC 界面依然生效。鲜艳度仍通过 DWM 处理。", "GPU ハードウェア LUT（SetDeviceGammaRamp）を使用し、スタートメニュー・タスクバー・UAC 画面でも明るさ・コントラスト・ガンマ・色温度・RGB を維持します。鮮やかさは DWM で処理されます。" } },
            { "LutReadyNote", new string[] { "Hardware LUT driver support looks ready.", "Suporte a LUT por hardware pronto neste adaptador.", "El soporte de LUT por hardware está listo.", "La prise en charge LUT matérielle est prête.", "Supporto LUT hardware pronto.", "硬件 LUT 驱动支持已就绪。", "ハードウェア LUT ドライバーサポートの準備ができています。" } },
            { "LutDisplayLinkWarn", new string[] { "DisplayLink USB detected without EnableGammaRamp=true in Registry. DisplayLink ignores hardware LUT until enabled and the monitor/PC is reconnected.", "DisplayLink USB detectado sem EnableGammaRamp=true no Registro. O driver DisplayLink ignora LUT por hardware até ativar e reconectar o monitor/reiniciar.", "Se detectó DisplayLink USB sin EnableGammaRamp=true en el Registro. DisplayLink ignora la LUT hasta activarlo y reconectar el monitor.", "DisplayLink USB détecté sans EnableGammaRamp=true dans le Registre. Reconnectez l'écran ou redémarrez après activation.", "DisplayLink USB rilevato senza EnableGammaRamp=true nel Registro. Riconnetti lo schermo o riavvia dopo l'attivazione.", "检测到 DisplayLink USB 尚未开启注册表项 EnableGammaRamp=true。启用并重新插拔显示器或重启前，DisplayLink 将忽略硬件 LUT。", "DisplayLink USB でレジストリ EnableGammaRamp=true が未設定です。有効化後にモニターを再接続または再起動するまで無視されます。" } },
            { "BtnEnableDisplayLinkGamma", new string[] { "Copy Registry command", "Copiar comando do Registro", "Copiar comando del Registro", "Copier la commande Registre", "Copia comando Registro", "复制注册表命令", "レジストリコマンドをコピー" } },
            { "BtnEnableDisplayLinkDirect", new string[] { "Enable DisplayLink LUT", "Ativar LUT DisplayLink", "Activar LUT DisplayLink", "Activer LUT DisplayLink", "Attiva LUT DisplayLink", "启用 DisplayLink LUT", "DisplayLink LUT を有効化" } },
            { "LutDisplayLinkDone", new string[] { "Command copied. Run it in an Administrator terminal, then reconnect your DisplayLink monitor (or restart Windows).", "Comando copiado. Execute em um terminal como Administrador e reconecte o monitor DisplayLink (ou reinicie o Windows).", "Comando copiado. Ejecútelo en una terminal de Administrador y vuelva a conectar el monitor DisplayLink.", "Commande copiée. Exécutez-la dans un terminal Administrateur puis reconnectez l'écran DisplayLink.", "Comando copiato. Eseguilo in un terminale Amministratore e riconnetti il monitor DisplayLink.", "命令已复制。请在管理员终端中运行，然后重新插拔 DisplayLink 显示器（或重启 Windows）。", "コマンドをコピーしました。管理者ターミナルで実行し、DisplayLink モニターを再接続してください。" } },
            { "LutDirectDone", new string[] { "DisplayLink LUT enabled in Registry. Unplug and reconnect your DisplayLink monitor (or restart Windows).", "LUT do DisplayLink ativado no Registro. Desconecte e reconecte o monitor DisplayLink (ou reinicie o Windows).", "LUT de DisplayLink activada en el Registro. Reconecte su monitor DisplayLink o reinicie Windows.", "LUT DisplayLink activée dans le Registre. Rebranchez votre écran DisplayLink ou redémarrez.", "LUT DisplayLink attivata nel Registro. Ricollega il monitor DisplayLink o riavvia Windows.", "DisplayLink LUT 已写入注册表。请拔插 DisplayLink 显示器（或重启 Windows）。", "レジストリで DisplayLink LUT を有効化しました。モニターを再接続または再起動してください。" } },
            { "ResetControlTip", new string[] { "Reset to default", "Voltar ao padrão", "Restablecer predeterminado", "Rétablir par défaut", "Ripristina predefinito", "恢复默认值", "デフォルトに戻す" } },

            // Administrator Elevation Settings
            { "RunAsAdminTitle", new string[] { "Run as Administrator", "Executar como Administrador", "Ejecutar como Administrador", "Exécuter en tant qu'Administrateur", "Esegui come Amministratore", "以管理员身份运行", "管理者として実行" } },
            { "RunAsAdminDesc", new string[] { "Request administrator privileges automatically when launching Lucent to avoid Windows permission restrictions on system-wide display and driver settings.", "Solicita privilégios de administrador automaticamente ao abrir o Lucent para evitar bloqueios de permissão do Windows em ajustes de vídeo e driver.", "Solicita privilegios de administrador al iniciar Lucent para evitar bloqueos de permisos en ajustes de pantalla y controladores.", "Demande les privilèges administrateur au lancement de Lucent pour éviter les restrictions d'autorisation Windows.", "Richiede i privilegi di amministratore all'avvio di Lucent per evitare limiti di autorizzazione Windows.", "启动 Lucent 时自动请求管理员权限，以避免系统级显示与驱动设置的权限限制。", "起動時に管理者権限を自動で要求し、ディスプレイやドライバー設定での権限エラーを防ぎます。" } },
            { "StatusAdminActive", new string[] { "● Running with Administrator privileges", "● Executando com privilégios de Administrador", "● Ejecutando con privilegios de Administrador", "● Exécuté avec privilèges Administrateur", "● In esecuzione come Amministratore", "● 当前正以管理员权限运行", "● 管理者権限で実行中" } },
            { "StatusAdminStandard", new string[] { "○ Running with standard user permissions", "○ Executando com permissões padrão", "○ Ejecutando con permisos estándar", "○ Exécuté avec permissions standard", "○ In esecuzione con permessi standard", "○ 当前以标准用户权限运行", "○ 標準ユーザー権限で実行中" } },
            { "BtnRestartAsAdmin", new string[] { "Restart as Admin", "Reiniciar como Admin", "Reiniciar como Admin", "Redémarrer en Admin", "Riavvia come Admin", "以管理员重启", "管理者として再起動" } },
            { "VideoEngineValueLut", new string[] { "Hardware Gamma LUT + DWM Vibrance", "LUT Gama por Hardware + Vibração DWM", "LUT Gamma Hardware + Intensidad DWM", "LUT Gamma Matérielle + Éclat DWM", "LUT Gamma Hardware + Vividezza DWM", "硬件伽玛 LUT + DWM 鲜艳度", "ハードウェア LUT + DWM 鮮やかさ" } },
            { "SchedStatusActive", new string[] { "● Active: Monitoring circadian cycle", "● Ativo: Monitorando ciclo circadiano", "● Activo: Monitoreando ciclo circadiano", "● Actif : Surveillance du cycle circadien", "● Attivo: Monitoraggio ciclo circadiano", "● 运行中：正在监测昼夜节律计划", "● 有効: サーカディアンサイクルを監視中" } },
            { "SchedStatusInactive", new string[] { "○ Inactive: Manual profile management", "○ Inativo: Gerenciamento manual de perfis", "○ Inactivo: Gestión manual de perfiles", "○ Inactif : Gestion manuelle des profils", "○ Inattivo: Gestione manuale dei profili", "○ 未启用：手动管理配置", "○ 停止中: 手動プロファイル管理" } },

            // Help Tab Documentation
            { "HelpTitle", new string[] { "Help & Documentation", "Ajuda e Documentação", "Ayuda y Documentación", "Aide et Documentation", "Guida e Documentazione", "帮助与文档", "ヘルプとドキュメント" } },
            { "HelpSubtitle", new string[] { "Complete reference for all sliders, shortcuts, profiles, display engines and settings in Lucent v0.2.0", "Referência completa de todos os ajustes, atalhos, perfis, motores de vídeo e configurações do Lucent v0.2.0", "Referencia completa de controles, atajos, perfiles, motores de vídeo y ajustes en Lucent v0.2.0", "Référence complète des curseurs, raccourcis, profils, moteurs d'affichage et paramètres de Lucent v0.2.0", "Guida completa a cursori, scorciatoie, profili, motori video e impostazioni di Lucent v0.2.0", "Lucent v0.2.0 所有滑块、快捷键、配置文件、显示引擎与设置的完整指南", "Lucent v0.2.0 のすべてのスライダー、ショートカット、プロファイル、表示エンジン、設定の完全ガイド" } },

            { "HelpMonitorsTitle", new string[] { "1. Multi-Monitor Control & Calibration", "1. Controle Multimonitor e Calibração", "1. Control Multimonitor y Calibración", "1. Contrôle Multi-Écrans et Calibrage", "1. Controllo Multimonitor e Calibrazione", "1. 多显示器控制与校准", "1. マルチモニター制御とキャリブレーション" } },
            { "HelpMonitorsBody", new string[] {
                "• Monitor Cards (1, 2, ...): Click any monitor card on the Dashboard to adjust settings independently for that screen.\n• All Displays (Synced): Keeps every connected monitor synchronized using the same global profile.\n• Identify: Flashes large numbered badges on each physical monitor so you know which screen is 1, 2, etc.\n• Monitors Tab: Inspect resolution, refresh rate, desktop coordinates, color depth, active output engine, and launch fullscreen test patterns (White, Black, Grayscale, SMPTE Color Bars; press ESC or click to exit).",
                "• Cartões de Monitor (1, 2, ...): Clique em qualquer cartão de monitor no Início para ajustar aquela tela individualmente.\n• Todas as Telas (Sincronizado): Mantém todos os monitores conectados sincronizados com os mesmos ajustes.\n• Identificar: Exibe emblemas numerados grandes em cada tela física para identificar Monitor 1, 2, etc.\n• Aba Monitores: Veja resolução, taxa de atualização, coordenadas, profundidade de cor, motor ativo e abra padrões de calibração em tela cheia (Branco, Preto, Cinza, Barras SMPTE; pressione ESC ou clique para sair).",
                "• Tarjetas de Monitor (1, 2, ...): Haga clic en cualquier tarjeta para ajustar esa pantalla por separado.\n• Todas las Pantallas (Sincronizado): Sincroniza todos los monitores con el mismo ajuste.\n• Identificar: Muestra números grandes en cada monitor físico.\n• Pestaña Monitores: Muestra resolución, frecuencia, coordenadas, motor activo y patrones de prueba en pantalla completa (pulse ESC o clic para salir).",
                "• Cartes Moniteur (1, 2, ...) : Cliquez sur un moniteur pour l'ajuster séparément.\n• Tous les Écrans (Synchronisé) : Applique les mêmes réglages sur tous les écrans.\n• Identifier : Affiche un grand numéro sur chaque écran physique.\n• Onglet Moniteurs : Affiche résolution, taux de rafraîchissement, coordonnées, moteur actif et mires plein écran (Échap ou clic pour quitter).",
                "• Schede Monitor (1, 2, ...): Fai clic su un monitor per regolarlo separatamente.\n• Tutti gli Schermi (Sincronizzati): Mantiene tutti gli schermi sincronizzati.\n• Identifica: Mostra badge numerati su ogni schermo fisico.\n• Scheda Monitor: Mostra risoluzione, frequenza, coordinate, motore attivo e pattern di calibrazione a schermo intero (ESC o clic per uscire).",
                "• 显示器卡片（1、2 等）：点击任意卡片可单独调整对应屏幕。\n• 所有屏幕（同步）：让全部已连接的显示器保持同步参数。\n• 识别屏幕：在每台物理显示器上显示大号编号徽章。\n• 显示器标签页：查看分辨率、刷新率、坐标、输出引擎，并启动全屏校准测试图（按 ESC 或点击退出）。",
                "• モニターカード（1、2 など）: 個別の画面ごとに調整できます。\n• すべての画面（同期）: 接続された全モニターを同じ設定で同期します。\n• 識別: 各物理ディスプレイに大きな番号バッジを表示します。\n• モニタータブ: 解像度、リフレッシュレート、座標、出力エンジンを確認し、全画面テストパターンを表示します（ESC またはクリックで終了）。"
            } },

            { "HelpSlidersTitle", new string[] { "2. Image, Tone & Color Adjustments", "2. Ajustes de Imagem, Tom e Cor", "2. Ajustes de Imagen, Tono y Color", "2. Réglages d'Image, Tonalité et Couleur", "2. Regolazioni Immagine, Tono e Colore", "2. 画面、色调与色彩调节", "2. 映像・トーン・カラー調整" } },
            { "HelpSlidersBody", new string[] {
                "• Brightness (0% – 200%, center 100%): Dims dark rooms below 100% or boosts panel gain up to 200%.\n• Contrast (50% – 150%, default 100%): Expands or softens separation between shadows and highlights.\n• Gamma (0.60 – 2.00, default 1.00): Adjusts midtone luminance curve without clipping black/white points.\n• Color Vibrance (0% – 200%, center 100%): Rec.709 color saturation engine — 0% turns the display monochrome, 100% keeps natural colors, and 101%–200% boosts vividness like NVIDIA Digital Vibrance while preserving neutral grays.\n• Color Temperature (3000 K – 9000 K, default 6500 K) & RGB (0 – 100): Shifts warm amber to cool daylight or balances Red, Green, and Blue channels independently.",
                "• Brilho (0% – 200%, centro em 100%): Reduz a luminosidade abaixo de 100% ou aumenta o ganho de imagem até 200%.\n• Contraste (50% – 150%, padrão 100%): Aumenta ou suaviza a separação entre sombras e realces.\n• Gama (0.60 – 2.00, padrão 1.00): Corrige a curva dos meios-tons sem estourar pretos ou brancos.\n• Vibração de Cor (0% – 200%, centro em 100%): Motor de saturação Rec.709 — 0% deixa em preto e branco, 100% mantém cores naturais e até 200% intensifica a saturação (estilo NVIDIA Digital Vibrance) preservando cinzas neutros.\n• Temperatura de Cor (3000 K – 9000 K, padrão 6500 K) e RGB (0 – 100): Ajusta o ponto branco quente/frio e os canais Vermelho, Verde e Azul separadamente.",
                "• Brillo (0% – 200%, centro 100%): Atenúa o refuerza la ganancia hasta 200%.\n• Contraste (50% – 150%, defecto 100%): Ajusta la separación entre sombras y luces.\n• Gamma (0.60 – 2.00, defecto 1.00): Ajusta los tonos medios.\n• Intensidad de Color (0% – 200%, centro 100%): Saturación Rec.709 estilo NVIDIA Digital Vibrance preservando grises neutros.\n• Temperatura (3000 K – 9000 K, 6500 K por defecto) y RGB (0 – 100): Balance cálido/frío y canales independientes.",
                "• Luminosité (0% – 200%, centre 100%) : Réduit ou augmente le gain jusqu'à 200%.\n• Contraste (50% – 150%) & Gamma (0.60 – 2.00) : Ajuste les demi-teintes.\n• Éclat des Couleurs (0% – 200%, centre 100%) : Saturation Rec.709 (0% N&B, 100% neutre, 200% vif façon NVIDIA Digital Vibrance).\n• Température (3000 K – 9000 K) et RVB (0 – 100) : Équilibre chaud/froid et canaux séparés.",
                "• Luminosità (0% – 200%, centro 100%): Attenua o aumenta il guadagno fino al 200%.\n• Contrasto (50% – 150%) e Gamma (0.60 – 2.00): Regola la curva dei mezzitoni.\n• Vividezza Colore (0% – 200%, centro 100%): Saturazione Rec.709 stile NVIDIA Digital Vibrance con grigi neutri invariati.\n• Temperatura Colore (3000 K – 9000 K) e RGB (0 – 100): Toni caldi/freddi e canali indipendenti.",
                "• 亮度（0%–200%，中心 100%）：低于 100% 降暗，最高可提升至 200% 增益。\n• 对比度（50%–150%）与伽玛（0.60–2.00）：调整明暗反差与中间调曲线。\n• 色彩鲜艳度（0%–200%，中心 100%）：Rec.709 饱和度矩阵（0% 黑白、100% 自然、200% 鲜艳，类似 NVIDIA 数字振动且不偏灰阶）。\n• 色温（3000 K–9000 K）与 RGB（0–100）：暖冷白平衡与红绿蓝三通道微调。",
                "• 明るさ（0%–200%、中央 100%）: 100% 以下で減光、200% までゲインを増加します。\n• コントラスト（50%–150%）とガンマ（0.60–2.00）: 中間調カーブを調整します。\n• 色の鮮やかさ（0%–200%、中央 100%）: Rec.709 彩度エンジン（0% モノクロ、100% 標準、200% 鮮やか・NVIDIA Digital Vibrance 相当）。\n• 色温度（3000 K–9000 K）と RGB（0–100）: 暖色/寒色と各色チャンネルを個別調整。"
            } },

            { "HelpPrecisionTitle", new string[] { "3. Precision Editing, Keyboard Step & Quick Reset", "3. Edição Manual, Teclado 1% e Reset Individual", "3. Edición Manual, Teclado 1% y Restablecimiento", "3. Saisie Manuelle, Clavier 1% et Réinitialisation", "3. Inserimento Manuale, Tastiera 1% e Reset", "3. 手动输入、键盘 1% 微调与单项重置", "3. 数値直接入力・1%キーボード操作・個別リセット" } },
            { "HelpPrecisionBody", new string[] {
                "• Click any numeric value (e.g. 100%, 1.00, 6500 K, 100) to type an exact value directly. Supports %, K, comma or dot decimals (e.g. 1,25). Press Enter or click outside to apply, or ESC to cancel.\n• Keyboard Fine Control: Click/focus any slider and press Left or Right arrow keys to step precisely by 1% (0.01 for Brightness, Contrast, Gamma, Vibrance, RGB; 50 K for Color Temperature).\n• Circular Reset Arrow (↻): Click the ↻ icon right next to any control value to return only that slider to its factory default.\n• Reset to Default: Restores all sliders on the active monitor (or all synced displays) back to Standard.",
                "• Clique sobre qualquer número (100%, 1.00, 6500 K, 100) para digitar o valor manualmente. Aceita %, K e vírgula ou ponto decimal (ex.: 1,25). Pressione Enter ou clique fora para confirmar, ou Esc para cancelar.\n• Setas do Teclado (1%): Clique em qualquer barra e use as setas Esquerda/Direita para mover exatamente 1% por vez (0,01 em Brilho, Contraste, Gama, Vibração e RGB; 50 K em Temperatura).\n• Setinha Circular (↻): Clique no ícone ↻ ao lado de cada número para voltar apenas aquele ajuste ao valor padrão.\n• Redefinir Padrões: Retorna todos os controles da tela ativa ao perfil Standard.",
                "• Haga clic en cualquier valor numérico para escribirlo manualmente (acepta %, K, coma o punto decimal). Enter confirma y Esc cancela.\n• Flechas del teclado (1%): Enfoque un deslizador y use Izquierda/Derecha para ajustar 1% (o 50 K en Temperatura).\n• Flecha circular (↻): Al lado de cada número restaura solo ese control; Restablecer Valores restaura todos.",
                "• Cliquez sur une valeur numérique pour la saisir au clavier (%, K, virgule ou point). Entrée valide, Échap annule.\n• Flèches du clavier (1%) : Gauche/Droite ajuste par pas de 1% (ou 50 K).\n• Flèche circulaire (↻) : Réinitialise uniquement ce réglage.",
                "• Fai clic su qualsiasi valore numerico per digitarlo manualmente (%, K, virgola o punto). Invio conferma, Esc annulla.\n• Frecce tastiera (1%): Sinistra/Destra sposta esattamente dell'1% (o 50 K).\n• Freccia circolare (↻): Ripristina solo quel controllo al valore predefinito.",
                "• 点击任意数值可直接输入目标值（支持 %、K、小数点或逗号）。回车确认，Esc 取消。\n• 键盘 1% 微调：选中任意滑块后按左/右方向键按 1% 步进（色温为 50 K）。\n• 环形重置箭头（↻）：点击数值旁的 ↻ 仅将该项恢复默认值。",
                "• 数値バッジをクリックすると直接数値を入力できます（%、K、小数点/カンマ対応、Enter 確定、Esc キャンセル）。\n• キーボード 1% 調整: スライダー選択中に左右矢印キーで 1% 刻み（色温度は 50 K 刻み）で調整します。\n• 円形リセット矢印（↻）: 各数値の横の ↻ でその項目のみを既定値に戻します。"
            } },

            { "HelpProfilesTitle", new string[] { "4. Presets & Custom Profiles", "4. Modos Rápidos e Perfis Personalizados", "4. Preajustes y Perfiles Personalizados", "4. Préréglages et Profils Personnalisés", "4. Preimpostazioni e Profili Personalizzati", "4. 预设与自定义配置文件", "4. プリセットとカスタムプロファイル" } },
            { "HelpProfilesBody", new string[] {
                "• Quick Presets & Presets Tab: 8 calibrated profiles (Standard, Warm, Cool, Movie, Work, Night, Gaming, Focus) with tuned brightness, gamma, color temperature and vibrance.\n• Manual Save vs Auto-Save: With Auto-Save off, edited profiles show a '● Modified' indicator until you click Save or Save as New. With Auto-Save on (Settings), edits save automatically to your active custom profile (the first edit prompts for a profile name).\n• Profiles Tab: Switch active profile, inspect summary values, create new profiles or delete custom profiles.\n• Startup Profile: Enable 'Apply a profile at startup' in Settings to automatically load any saved profile when Lucent opens.",
                "• Modos Rápidos e Aba Predefinições: 8 perfis integrados (Standard, Warm, Cool, Movie, Work, Night, Gaming, Focus) já com vibração calibrada.\n• Salvar Manual ou Automático: No modo manual, alterações exibem '● Modificado' até clicar em Salvar ou Salvar como Novo. No modo automático (Configurações), cada alteração salva no perfil ativo (na primeira vez solicita um nome).\n• Aba Perfis: Alterne perfis ativos, crie novos ou exclua perfis personalizados.\n• Perfil na Inicialização: Marque 'Autoaplicar um perfil ao iniciar' nas Configurações para aplicar seu perfil favorito ao abrir o programa.",
                "• Preajustes rápidos y pestaña Preajustes: 8 modos integrados.\n• Guardado manual o automático: En modo manual muestra '● Modificado' hasta guardar; en automático guarda cada cambio.\n• Pestaña Perfiles: Cree, aplique o elimine perfiles y configure el perfil de inicio en Configuración.",
                "• Préréglages rapides : 8 modes intégrés avec éclat calibré.\n• Sauvegarde manuelle ou automatique : Mode manuel avec badge '● Modifié', ou sauvegarde auto dans le profil actif.\n• Profil de démarrage : Charge automatiquement le profil choisi au lancement.",
                "• Preimpostazioni rapide: 8 modalità integrate.\n• Salvataggio manuale o automatico: Indicatore '● Modificato' o salvataggio automatico continuo.\n• Profilo all'avvio: Carica automaticamente il profilo scelto nelle Impostazioni.",
                "• 快速预设与预设页：8 种内置校准预设（含鲜艳度参数）。\n• 手动与自动保存：手动模式下修改会显示“● 已修改”；开启自动保存后改动立即写入当前配置（首次会提示命名）。\n• 启动自动应用：可在设置中选择启动时自动加载的配置文件。",
                "• クイックプリセット＆プリセットタブ: 8種類の内蔵プロファイル。\n• 手動/自動保存: 手動モードでは「● 変更済み」が表示され、自動保存では変更が即座に保存されます。\n• 起動時自動適用: 設定から起動時に読み込むプロファイルを選択できます。"
            } },

            { "HelpSchedulerTitle", new string[] { "5. Circadian Scheduler", "5. Agendador Circadiano", "5. Programador Circadiano", "5. Planificateur Circadien", "5. Pianificatore Circadiano", "5. 昼夜节律自动计划", "5. サーカディアン スケジューラ" } },
            { "HelpSchedulerBody", new string[] {
                "• Day / Sunset / Night Slots: Assign an exact HH:MM trigger time and target profile for each phase of the day.\n• Smart Midnight Wrap: Works cleanly even if your Night slot is set after midnight (e.g. 01:00).\n• Background Timer & Notifications: Checks the current time slot every 30 seconds and optionally shows a system tray notification when shifting profiles.",
                "• Períodos Dia / Pôr do Sol / Noite: Defina horário exato (HH:MM) e perfil para cada faixa do dia.\n• Virada de Meia-Noite: Funciona corretamente mesmo com horário noturno após 00:00 (ex.: 01:00).\n• Monitoramento Leve: Verifica a faixa horária a cada 30 segundos e pode notificar na bandeja ao trocar o perfil.",
                "• Franjas Día / Atardecer / Noche: Asigne hora exacta y perfil para cada momento.\n• Soporte de medianoche y notificaciones opcionales en la bandeja cada 30 segundos.",
                "• Plages Jour / Coucher / Nuit : Associez une heure exacte et un profil à chaque période (avec gestion après minuit et notifications).",
                "• Fasce Giorno / Tramonto / Notte: Imposta orario esatto e profilo per ciascuna fase, con supporto oltre la mezzanotte.",
                "• 白天 / 日落 / 夜间时段：为各时段配置触发时间与配置文件，支持跨午夜时间及托盘通知。",
                "• 昼 / 夕方 / 夜のスロット: 各時間帯の時刻とプロファイルを設定でき、深夜0時をまたぐ設定やトレイ通知に対応します。"
            } },

            { "HelpEnginesTitle", new string[] { "6. Display Engines: DWM vs Hardware Gamma LUT", "6. Motores de Vídeo: Matriz DWM vs Hardware LUT", "6. Motores de Vídeo: Matriz DWM vs LUT Hardware", "6. Moteurs d'Affichage : DWM vs LUT Matérielle", "6. Motori Video: Matrice DWM vs LUT Hardware", "6. 显示引擎：DWM 矩阵 vs 硬件伽玛 LUT", "6. 表示エンジン: DWM マトリックス vs ハードウェア LUT" } },
            { "HelpEnginesBody", new string[] {
                "• DWM Color Matrix (Default): Uses Windows Desktop Window Manager (MagSetFullscreenColorEffect). Works out-of-the-box on every adapter including DisplayLink USB, and always powers Color Vibrance.\n• Why Windows excludes Start Menu / UAC in default mode: Windows 11 DWM isolates elevated shell bands and the UAC Winlogon secure desktop from unsigned non-uiAccess apps.\n• Hardware Gamma LUT (Settings): Uses SetDeviceGammaRamp at GPU display scanout so Brightness, Contrast, Gamma, Color Temp and RGB stay active over Start Menu, Taskbar and UAC prompts (auto-falls back to DWM below ~35% brightness).\n• DisplayLink USB Setup: DisplayLink ignores hardware LUT until HKLM\\SOFTWARE\\DisplayLink\\Core\\EnableGammaRamp = \"true\" is set (either directly by Lucent when running as Admin, or via the copied reg command + monitor reconnect).",
                "• Matriz de Cor DWM (Padrão): Usa o compositor do Windows (MagSetFullscreenColorEffect). Funciona nativamente em qualquer placa de vídeo e adaptadores DisplayLink USB, além de aplicar a Vibração de Cor.\n• Por que Menu Iniciar e UAC ficam de fora no modo padrão: Por isolamento de segurança do Windows 11, apps portáteis sem uiAccess não cobrem bandas elevadas do Shell nem o desktop seguro do UAC (Winlogon).\n• LUT de Gama por Hardware (Configurações): Programa a tabela SetDeviceGammaRamp na saída de vídeo para manter Brilho, Contraste, Gama, Temperatura e RGB ativos sobre Menu Iniciar, Barra de Tarefas e UAC (com fallback abaixo de ~35% de brilho).\n• Monitores DisplayLink USB: O driver DisplayLink exige HKLM\\SOFTWARE\\DisplayLink\\Core\\EnableGammaRamp = \"true\" (que o próprio Lucent pode gravar se estiver como Administrador) e reconexão do monitor/reinício.",
                "• Matriz DWM (Por defecto): Funciona en todas las GPUs y DisplayLink USB y ejecuta la Intensidad de Color.\n• LUT de Gamma por Hardware: Usa SetDeviceGammaRamp para mantener Brillo, Contraste, Gamma, Temperatura y RGB activos sobre Inicio, Barra y UAC (DisplayLink requiere EnableGammaRamp=\"true\" en el Registro).",
                "• Matrice DWM (Par défaut) : Compatible avec tous les adaptateurs dont DisplayLink USB et gère l'Éclat des Couleurs.\n• LUT Gamma Matérielle : Maintient Luminosité, Contraste, Gamma, Température et RVB sur Menu Démarrer, Barre des tâches et UAC (DisplayLink requiert EnableGammaRamp=\"true\").",
                "• Matrice DWM (Predefinita): Funziona su tutte le GPU e DisplayLink USB e gestisce Vividezza Colore.\n• LUT Gamma Hardware: Mantiene Luminosità, Contrasto, Gamma, Temperatura e RGB attivi su Start, Barra e UAC (DisplayLink richiede EnableGammaRamp=\"true\").",
                "• DWM 颜色矩阵（默认）：开箱即支持包括 DisplayLink USB 在内的所有显卡，并负责色彩鲜艳度。\n• 硬件伽玛 LUT（设置）：通过 SetDeviceGammaRamp 让亮度、对比度、伽玛、色温和 RGB 在开始菜单、任务栏与 UAC 界面下仍保持生效（DisplayLink 需开启 EnableGammaRamp=\"true\"）。",
                "• DWM カラーマトリックス（既定）: DisplayLink USB を含むすべてのアダプターで動作し、色の鮮やかさを処理します。\n• ハードウェア ガンマ LUT（設定）: スタートメニュー、タスクバー、UAC 画面でも調整を維持します（DisplayLink は EnableGammaRamp=\"true\" が必要）。"
            } },

            { "HelpSystemTitle", new string[] { "7. Settings, Permissions, System Tray & CLI", "7. Configurações, Permissões, Bandeja e Linha de Comando", "7. Configuración, Permisos, Bandeja y CLI", "7. Paramètres, Droits, Barre d'État et CLI", "7. Impostazioni, Permessi, Barra e CLI", "7. 设置、权限、托盘与命令行参数", "7. 設定・権限・トレイ・コマンドライン" } },
            { "HelpSystemBody", new string[] {
                "• 7 Languages: Instant UI preview; persisted only when clicking Apply or OK.\n• Run as Administrator & Restart as Admin: Requests UAC elevation automatically at launch and unlocks direct Registry writes for DisplayLink hardware LUT.\n• Startup & Tray: Configure 'Start with Windows', 'Start minimized', and 'Minimize to tray on close' (trims memory working set while idling in tray). Right-click the tray icon to switch presets or restore/exit quickly.\n• Presets & Data Maintenance: 'Restore default presets' resets built-in presets while preserving custom profiles; 'Erase all data' performs a complete factory reset.\n• CLI Arguments: /minimized (launch silently in tray), -render (capture UI screenshots), --no-elevate (launch without repeating UAC prompt).",
                "• 7 Idiomas: Pré-visualização imediata; salvo apenas ao clicar em Aplicar ou OK.\n• Executar como Administrador e Reiniciar como Admin: Solicita elevação UAC ao abrir e habilita gravação direta no Registro para LUT DisplayLink.\n• Inicialização e Bandeja: Configure 'Iniciar junto com o PC', 'Iniciar minimizado' e 'Minimizar ao fechar' (reduz o uso de memória RAM na bandeja). Clique direito no ícone da bandeja para trocar presets rapidamente.\n• Manutenção de Dados: 'Restaurar predefinições padrão' mantém seus perfis personalizados; 'Apagar todos os dados' zera tudo ao estado de fábrica.\n• Argumentos CLI: /minimized (inicia na bandeja), -render (gera capturas de tela) e --no-elevate (evita novo prompt UAC).",
                "• 7 Idiomas con vista previa inmediata.\n• Ejecutar como Administrador y Reiniciar como Admin.\n• Inicio automático, bandeja del sistema con menú rápido, Restaurar preajustes, Borrar datos y parámetros CLI (/minimized, -render, --no-elevate).",
                "• 7 Langues avec aperçu instantané.\n• Exécuter en tant qu'Administrateur, démarrage Windows, réduction dans la barre d'état, réinitialisation et arguments CLI (/minimized, -render, --no-elevate).",
                "• 7 Lingue con anteprima immediata.\n• Esegui come Amministratore, avvio con Windows, barra delle applicazioni, ripristino dati e argomenti CLI (/minimized, -render, --no-elevate).",
                "• 7 种语言即时预览；以管理员身份运行、开机自启、托盘右键快捷菜单、恢复预设、清除所有数据及命令行参数（/minimized、-render、--no-elevate）。",
                "• 7言語の即時プレビュー、管理者として実行、Windows 起動時実行、システムトレイのクイックメニュー、初期化機能、CLI 引数（/minimized、-render、--no-elevate）に対応。"
            } }
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
