using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.UI.Popups;
using LunaForge.Editor.UI.Windows;
using LunaForge.Editor.UI.Windows.ProjectBrowser;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Managers;

public static class Designer
{

    private static ILogger Logger = CoreLogger.Create("Designer");

    public static void Init()
    {
        WindowManager.Init();

        if (!EditorConfig.Default.Get<bool>("SetupDone").Value)
            PopupManager.Show<SetupWindow>();

        PopupManager.Show<LauncherWindow>();

        WindowManager.ShowWindow<ProjectWindow>();
        WindowManager.ShowWindow<ProjectBrowserWindow>();
        //WindowManager.ShowWindow<TerminalWindow>(); // Heavily rework

        Logger.Information("Designer initialized");
    }

    public static void Draw()
    {
        MainMenuBar.Draw();
        WindowManager.Draw();
        PopupManager.Draw();
        DialogManager.Draw();
        MessageBoxes.Draw();
    }

    public static void Dispose()
    {
        //MainMenuBar.Dispose();
        WindowManager.Dispose();
        PopupManager.Dispose();
    }
}
