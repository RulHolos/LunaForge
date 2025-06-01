using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using Hexa.NET.Raylib;
using LunaForge.Editor.Backend;
using LunaForge.Editor.Backend.Services;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.Projects;
using LunaForge.Editor.UI.Managers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI;

/*
 * No editor logic outside of UI interaction in the Windows code.
 * 
 * The undo/redo history is not global but local to each window.
 * 
 * Compilation is threaded. It looks at every files (.lfd, .lfs, .lua) and compiles them in parallel.
 * 
 */

public static class MainWindow
{
    public static readonly string LunaForgeName = $"LunaForge Editor";
    public static readonly Version? VersionNumber = Assembly.GetEntryAssembly()?.GetName().Version;

    private static ILogger Logger = CoreLogger.Create("Main Window");

    private static ImGuiManager manager;

    public static Image EditorIcon = Raylib.LoadImage(Path.Combine(Directory.GetCurrentDirectory(), "Assets/Icon.png"));

    static MainWindow()
    {
        CoreLogger.Initialize();

        Raylib.SetConfigFlags((uint)(ConfigFlags.FlagMsaa4XHint
            | ConfigFlags.FlagWindowHighdpi
            | ConfigFlags.FlagVsyncHint
            | ConfigFlags.FlagWindowResizable));
        Raylib.InitWindow(1920, 1080, $"{LunaForgeName} v{VersionNumber}");
        Raylib.SetWindowIcon(EditorIcon);
        Raylib.SetExitKey((int)KeyboardKey.Null);
        Raylib.MaximizeWindow();
        Raylib.SetTargetFPS(60);

        manager = new();

        Designer.Init();

        ServiceManager.RegisterService<DiscordRPCService>();

        ServiceManager.InitServices();
    }

    public static void Dispose()
    {
        Raylib.UnloadImage(EditorIcon);

        ServiceManager.Dispose();
    }

    public static void Run()
    {
        bool exitWindow = false;

        while (!exitWindow && !ForceCloseWindow)
        {
            try
            {
                manager.NewFrame();

                Designer.Draw();

                //ImGui.ShowDemoWindow();

                if (Raylib.WindowShouldClose())
                {
                    Logger.Debug("Exit window requested.");
                    if (ProjectManager.CurrentProject != null && ProjectManager.CurrentProject.ProjectFileCollection.Any(x => x.IsUnsaved))
                    {
                        MessageBoxes.Show(
                            new MessageBox("Closing", "Some files are not saved.\nDo you want to save all files before closing?", MessageBoxType.YesNoCancel, null,
                            (s, e) =>
                            {
                                if (s.Result == MessageBoxResult.No)
                                    exitWindow = true;
                                else if (s.Result == MessageBoxResult.Yes)
                                {
                                    // TODO
                                    
                                    exitWindow = true;
                                }
                            })
                        );
                    }
                    else
                    {
                        exitWindow = true;
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.Blank);
                manager.EndFrame();
                Raylib.EndDrawing();
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Girl failure:\n{ex}");
            }
        }

        manager.Dispose();
        Raylib.CloseWindow();

        EditorConfig.Default.Save();
    }

    private static bool ForceCloseWindow = false;

    public static void ForceClose()
    {
        ForceCloseWindow = true;
    }
}
