using Hexa.NET.ImGui;
using LunaForge.Editor.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows;

public class ProjectWindow : EditorWindow
{
    protected override string Name => "Project Files";
    public override bool IsShown { get; protected set; } = true;
    public override ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.NoCollapse;
    public override bool CanBeClosed { get; set; } = false;

    private LunaProject? currentProject;

    protected override void InitWindow()
    {
        base.InitWindow();

        ProjectManager.ProjectLoaded += ProjectLoaded;
    }

    private void ProjectLoaded(LunaProject project)
    {
        currentProject = project;
    }

    public override void DrawContent()
    {
        if (currentProject == null)
            return;

        if (ImGui.BeginTabBar("##ProjectTabBar", ImGuiTabBarFlags.Reorderable))
        {
            for (int i = 0; i < currentProject.ProjectFileCollection.Count; i++)
            {
                ImGuiTabItemFlags flags = ImGuiTabItemFlags.None;
                if (currentProject.ProjectFileCollection[i].IsUnsaved)
                    flags |= ImGuiTabItemFlags.UnsavedDocument;
                if (ImGui.BeginTabItem(currentProject.ProjectFileCollection[i].GetUniqueName(), flags))
                {
                    currentProject.ProjectFileCollection[i].Draw();
                    ImGui.EndTabItem();
                }
            }
            if (currentProject.ProjectFileCollection.Count == 0)
            {
                DrawNoFiles();
            }

            ImGui.EndTabBar();
        }
    }

    public void DrawNoFiles()
    {
        if (ImGui.BeginTabItem("No files..."))
        {
            ImGui.Text("You do not have any files open.\nPlease select a file type to begin!");

            if (ImGui.Button("Tree View"))
            {
                currentProject.ProjectFileCollection.Add(new LunaTreeView());
            }
            if (ImGui.Button("Shader Editor"))
            {
                currentProject.ProjectFileCollection.Add(new LunaNodeGraph());
            }

            ImGui.EndTabItem();
        }
    }
}
