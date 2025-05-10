using Hexa.NET.ImGui.Widgets;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.UI.Popups;
using LunaForge.Editor.UI.Windows;
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

        if (!EditorConfig.Default.SetupDone)
            PopupManager.Show<SetupWindow>();

        WindowManager.ShowWindow<ProjectWindow>();

        Logger.Information("Designer initialized");
    }

    public static void Draw()
    {
        MainMenuBar.Draw();
        WindowManager.Draw();
        MessageBoxes.Draw();
        PopupManager.Draw();
    }

    public static void Dispose()
    {
        WindowManager.Dispose();
        PopupManager.Dispose();
    }
}
