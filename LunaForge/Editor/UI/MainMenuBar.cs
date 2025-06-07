using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.Backend;
using LunaForge.Editor.Commands;
using LunaForge.Editor.Projects;
using LunaForge.Editor.UI.Dialogs;
using LunaForge.Editor.UI.Managers;
using LunaForge.Editor.UI.Popups;
using LunaForge.Editor.UI.Windows;
using PopupManager = LunaForge.Editor.UI.Managers.PopupManager;

namespace LunaForge.Editor.UI;

public static class MainMenuBar
{
    private static float progress = -1;
    private static string? progressOverlay;
    private static long progressOverlayTime;

    public static bool IsShown = true;

    static MainMenuBar()
    {

    }

    internal static unsafe void Draw()
    {
        if (!IsShown)
            return;

        if (!ImGui.BeginMainMenuBar())
            return;

        if (ImGui.BeginMenu($"File"))
        {
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Edit"))
        {
            EditSubMenu();
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            WindowManager.DrawMenu();

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"Project"))
        {
            if (ImGui.MenuItem("New Project"))
            {
                PopupManager.Show<NewProjWindow>();
            }
            if (ImGui.MenuItem("Open Project"))
            {
                MainWindow.FileDialogManager.OpenFileDialog("Open Project", "LunaForge Project{.lfp}",
                    OpenProjectCallback, 1, EditorConfig.Default.Get<string>("ProjectsFolder").Value);
            }
            ImGui.Separator();
            if (ImGui.BeginMenu("Open Recent"))
            {
                var entries = ProjectHistory.Entries;
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (ImGui.MenuItem(entry.Name))
                    {
                        ProjectManager.Load(entry.Path);
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled(entry.Path);
                }
                if (entries.Count == 0)
                    ImGui.MenuItem("No recent projects...", string.Empty, false, false);

                ImGui.EndMenu();
            }

            ImGui.SeparatorText("Packaging");

            if (ImGui.MenuItem($"Build Project", "F4", false, ProjectManager.CurrentProject != null))
            {

            }

            if (ImGui.MenuItem("Run LuaSTG", "F5", false, ProjectManager.CurrentProject != null))
            {

            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Git"))
        {
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Layout"))
        {
            if (ImGui.MenuItem("Save Current Layout"))
            {
                MainWindow.FileDialogManager.SaveFileDialog("Create New Layout", "Layout Definition{.json}", "default.json", ".json", CreateNewLayoutCallback);
            }
            if (ImGui.BeginMenu("Apply Layout"))
            {
                foreach (var layout in LayoutManager.Layouts)
                {
                    if (ImGui.MenuItem(layout.Name, LayoutManager.SelectedLayoutPath == layout.Path))
                        LayoutManager.SelectedLayoutPath = layout.Path;
                }

                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Manage Layouts"))
            {
                //WindowManager.ShowWindow<>
            }
            if (ImGui.MenuItem("Reset Layout"))
            {
                LayoutManager.ResetLayout();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Settings"))
        {
            if (ImGui.MenuItem("Editor Settings"))
            {
                WindowManager.ShowWindow<SettingsWindow>();
            }

            if (ImGui.MenuItem("Project Settings", string.Empty, false, ProjectManager.CurrentProject != null))
            {

            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.MenuItem("About"))
            {
                PopupManager.Show<AboutWindow>();
            }

            if (ImGui.MenuItem("Documentation"))
            {

            }

            if (ImGui.MenuItem("Redo Initial Setup"))
                PopupManager.Show<SetupWindow>();

            ImGui.EndMenu();
        }

        if (progress != -1)
        {
            ImGui.ProgressBar(progress, new(200, 0), progressOverlay);
            if (progress == 1 && progressOverlayTime == 0)
            {
                progressOverlayTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency;
            }
            else if (progressOverlayTime != 0 && progressOverlayTime < Stopwatch.GetTimestamp())
            {
                progress = -1;
                progressOverlay = null;
                progressOverlayTime = 0;
            }
        }

        ImGui.EndMainMenuBar();
    }

    private static void CreateNewLayoutCallback(bool success, string path)
    {
        if (!success)
            return;

        LayoutManager.CreateNewLayout(path);
    }

    private static void OpenProjectCallback(bool success, List<string> paths)
    {
        if (!success)
            return;

        ProjectManager.Load(paths[0]);
    }

    private static unsafe void EditSubMenu()
    {
        CommandHistory? currentHistoryCtx = WindowManager.CurrentFocusedWindow?.History;

        ImGui.MenuItem($"History Context: ???", string.Empty, false, false);

        if (ImGui.MenuItem("Undo", "Ctrl+Z", false, currentHistoryCtx.CanUndo && currentHistoryCtx != null))
        {
            currentHistoryCtx.Undo();
        }
        if (ImGui.MenuItem("Redo", "Ctrl+Y", false, currentHistoryCtx.CanRedo && currentHistoryCtx != null))
        {
            currentHistoryCtx.Redo();
        }

        if (currentHistoryCtx.UndoCount != 0 && currentHistoryCtx != null)
        {
            ImGui.Text("Undo Stack");
            foreach (Command command in currentHistoryCtx.CommandStack)
            {
                ImGui.MenuItem(command.ToString());
            }
        }
        if (currentHistoryCtx.UndoCount != 0 && currentHistoryCtx.RedoCount != 0 && currentHistoryCtx != null)
            ImGui.Separator();
        if (currentHistoryCtx.RedoCount != 0 && currentHistoryCtx != null)
        {
            ImGui.Text("Redo Stack");
            foreach (Command command in currentHistoryCtx.UndoCommandStack)
            {
                ImGui.MenuItem(command.ToString());
            }
        }
    }
}
