using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.Commands;
using LunaForge.Editor.Projects;
using LunaForge.Editor.UI.Dialogs;
using LunaForge.Editor.UI.Managers;
using LunaForge.Editor.UI.Popups;
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

        if (ImGui.BeginMenu("File"))
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
                /*
                SaveFileDialog dialog = new();
                dialog.CurrentFolder = EditorConfig.Default.ProjectsFolder;
                dialog.AllowedExtensions.Add(".lfp");
                dialog.OnlyAllowFilteredExtensions = true;
                dialog.Show(NewProjectCallback);*/
                PopupManager.Show<NewProjWindow>();
            }
            if (ImGui.MenuItem("Open Project"))
            {
                OpenFileDialog dialog = new();
                dialog.CurrentFolder = EditorConfig.Default.ProjectsFolder;
                dialog.AllowedExtensions.Add(".lfp");
                dialog.OnlyAllowFilteredExtensions = true;
                dialog.Show(OpenProjectCallback);
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

            if (ImGui.MenuItem($"Build Project", "F4"))
            {

            }

            if (ImGui.MenuItem("Run LuaSTG", "F5"))
            {

            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Layout"))
        {
            if (ImGui.MenuItem("Save Current Layout"))
            {
                CreateFileDialog dialog = new(LayoutManager.BasePath) { Extension = ".ini" };
                dialog.Show(CreateNewLayoutCallback);
            }
            if (ImGui.BeginMenu("Apply Layout"))
            {
                foreach (var layout in LayoutManager.Layouts)
                {
                    if (ImGui.MenuItem(layout.Name, LayoutManager.SelectedLayout == layout.Path))
                        LayoutManager.SelectedLayout = layout.Path;
                }

                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Manage Layouts"))
            {
                //WindowManager.ShowWindow<>
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.MenuItem("Settings"))
            {

            }

            if (ImGui.MenuItem("About"))
            {

            }

            if (ImGui.MenuItem("Documentation"))
            {

            }

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

    private static void CreateNewLayoutCallback(object? sender, DialogResult result)
    {
        if (result != DialogResult.Ok || sender is not CreateFileDialog dialog)
            return;
        LayoutManager.CreateNewLayout(dialog.FileName);
    }

    private static void NewProjectCallback(object? sender, DialogResult result)
    {
        if (result != DialogResult.Ok || sender is not SaveFileDialog dialog)
            return;

        Directory.CreateDirectory(dialog.SelectedFile!);
        ProjectManager.Create(dialog.SelectedFile);
    }

    private static void OpenProjectCallback(object? sender, DialogResult result)
    {
        if (result != DialogResult.Ok || sender is not OpenFileDialog dialog)
            return;

       ProjectManager.Load(dialog.SelectedFile);
    }

    private static unsafe void EditSubMenu()
    {
        if (ImGui.MenuItem("Undo", "Ctrl+Z", false, History.CanUndo))
        {
            History.Undo();
        }
        if (ImGui.MenuItem("Redo", "Ctrl+Y", false, History.CanRedo))
        {
            History.Redo();
        }

        if (History.UndoCount != 0)
        {
            ImGui.Text("Undo Stack");
            foreach (Command command in History.CommandStack)
            {
                ImGui.MenuItem(command.ToString());
            }
        }
        if (History.UndoCount != 0 && History.RedoCount != 0)
            ImGui.Separator();
        if (History.RedoCount != 0)
        {
            ImGui.Text("Redo Stack");
            foreach (Command command in History.UndoCommandStack)
            {
                ImGui.MenuItem(command.ToString());
            }
        }
    }
}
