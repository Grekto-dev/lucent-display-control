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
            { "AboutVersion", new string[] { "Version 0.1.0", "Versão 0.1.0", "Versión 0.1.0", "Version 0.1.0", "Versione 0.1.0", "版本 0.1.0", "バージョン 0.1.0" } },
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
            { "ClickToEditValue", new string[] { "Click to enter value manually", "Clique para digitar o valor manualmente", "Haga clic para ingresar el valor manualmente", "Cliquez pour saisir la valeur manuellement", "Fai clic per inserire il valore manualmente", "点击手动输入数值", "クリックして数値を直接入力" } }
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
