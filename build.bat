@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set WPF=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF

echo ========================================================
echo   Compilando Lucent Display Control v0.1.0...
echo ========================================================

"%CSC%" /target:winexe /optimize+ /platform:anycpu ^
    /resource:"%~dp0assets\icon.ico",LucentControl.icon.ico ^
    /r:"%WPF%\PresentationCore.dll" ^
    /r:"%WPF%\PresentationFramework.dll" ^
    /r:"%WPF%\WindowsBase.dll" ^
    /r:System.Xaml.dll ^
    /r:System.Drawing.dll ^
    /r:System.Windows.Forms.dll ^
    /out:"%~dp0LucentControl.exe" ^
    "%~dp0AssemblyInfo.cs" ^
    "%~dp0AppIcon.cs" ^
    "%~dp0Dialogs.cs" ^
    "%~dp0I18n.cs" ^
    "%~dp0Models.cs" ^
    "%~dp0Overlay.cs" ^
    "%~dp0MainWindow.cs" ^
    "%~dp0MainWindow.Views.cs" ^
    "%~dp0MainWindow.Settings.cs" ^
    "%~dp0MainWindow.Logic.cs" ^
    "%~dp0Program.cs"

if %ERRORLEVEL% EQU 0 (
    echo [SUCESSO] LucentControl.exe gerado com sucesso com icone unificado!
) else (
    echo [ERRO] Falha na compilacao. Verifique as mensagens de erro acima.
)
