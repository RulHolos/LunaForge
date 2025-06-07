using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
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

    private LunaProject? currentProject { get; set; }
    private LunaProjectFile? currentFile { get; set; }

    private LunaProjectFile? fileToClose = null;
    private LunaProjectFile? filePendingModal = null;

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
                LunaProjectFile file = currentProject.ProjectFileCollection[i];
                if (currentFile != file)
                    currentFile = file;

                ImGuiTabItemFlags flags = ImGuiTabItemFlags.NoAssumedClosure | ImGuiTabItemFlags.NoPushId;
                if (file.IsUnsaved)
                    flags |= ImGuiTabItemFlags.UnsavedDocument;
                if (ImGui.BeginTabItem(file.GetUniqueName(), ref file.IsOpened, flags))
                {
                    file.Draw();
                    ImGui.EndTabItem();
                }

                if (!file.IsOpened && currentProject.ProjectFileCollection.Contains(file))
                {
                    if (file.IsUnsaved)
                    {
                        filePendingModal = file;
                        ConfirmCloseModal();
                        file.IsOpened = true;
                    }
                    else
                    {
                        fileToClose = file;
                    }
                }
            }
            if (currentProject.ProjectFileCollection.Count == 0)
            {
                DrawNoFiles();
            }

            ImGui.EndTabBar();
        }

        if (fileToClose != null)
        {
            currentProject.ProjectFileCollection.Remove(fileToClose);
            fileToClose.Dispose();
            fileToClose = null;
        }
    }

    private void ConfirmCloseModal()
    {
        MessageBoxes.Show(
            new MessageBox("Closing", $"{Path.GetFileName(filePendingModal?.FilePath)} has unsaved changes.\nDo you want to save before closing?", MessageBoxType.YesNoCancel, null,
            (s, e) =>
            {
                if (s.Result == MessageBoxResult.Yes)
                {
                    if (File.Exists(filePendingModal.FilePath))
                        filePendingModal.Save();
                    else
                        filePendingModal.Save(true); // TODO: Box for saving path?.
                    fileToClose = filePendingModal;
                    filePendingModal = null;
                }
                else if (s.Result == MessageBoxResult.No)
                {
                    fileToClose = filePendingModal;
                    filePendingModal = null;
                }
                else
                {
                    filePendingModal = null;
                    fileToClose = null;
                }
            })
        );
    }

    public void DrawNoFiles()
    {
        if (ImGui.BeginTabItem("No files..."))
        {
            ImGui.Indent(50);
            ImGui.Text("You do not have any files open.\nPlease select a file type to begin!");
            ImGui.Unindent(50);

            ImGui.Spacing(); ImGui.Spacing();
            ImGui.SeparatorText("Visual Files");

            if (ImGui.Button($"{FA.ListUl} Tree View"))
            {
                currentProject.ProjectFileCollection.Add(LunaProjectFile.CreateNew<LunaNodeTree>($"Unnamed {currentProject.ProjectFileCollection.MaxHash + 1}"));
            }
            ImGui.SameLine();
            if (ImGui.Button($"{FA.ShareNodes} Shader Editor"))
            {
                currentProject.ProjectFileCollection.Add(LunaProjectFile.CreateNew<LunaNodeGraph>($"Unnamed {currentProject.ProjectFileCollection.MaxHash + 1}"));
            }

            ImGui.Spacing(); ImGui.Spacing();
            ImGui.SeparatorText("Text-based Files");

            if (ImGui.Button($"{FA.Pen} Lua Script"))
            {
                // Implement scripting.
            }

            ImGui.EndTabItem();
        }
    }
}
