using IconFonts;
using ImGuiNET;
using LunaForge.EditorData.Project;
using LunaForge.GUI.Helpers;
using LunaForge.GUI.ImGuiFileDialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LunaForge.GUI.Windows;

public class FileSystemWindow : ImGuiWindow
{
    private bool ProjectOpened { get; set; } = false;

    private static Vector4 pathDecompColor = new(0.188f, 0.188f, 0.2f, 1f);
    private static Vector4 selectedTextColor = new(1.00000000000f, 0.33333333333f, 0.33333333333f, 1f);
    private static Vector4 dirTextColor = new(0.54509803922f, 0.91372549020f, 0.99215686275f, 1f);
    private static Vector4 codeTextColor = new(0.94509803922f, 0.98039215686f, 0.54901960784f, 1f);
    private static Vector4 miscTextColor = new(1.00000000000f, 0.47450980392f, 0.77647058824f, 1f);
    private static Vector4 imageTextColor = new(0.31372549020f, 0.98039215686f, 0.48235294118f, 1f);
    private static Vector4 standardTextColor = new(1f);

    private string BasePath { get; set; }
    private List<string> PathDecomposition { get; set; } = [];
    private string CurrentPath;
    private string CurrentRelativePath => GetRelativePath(CurrentPath);
    private string GetRelativePath(string fullPath)
    {
        string rootName = Directory.GetParent(BasePath).FullName;
        string relativeFrom = Path.GetRelativePath(rootName, fullPath);
        return relativeFrom;
    }

    private bool PathClicked = false;

    string newFolderName = string.Empty;
    string newFileName = string.Empty;

    public FileSystemWindow() : base(true) { }

    public void InitializeProject(LunaForgeProject project)
    {
        BasePath = project.PathToProjectRoot;
        SetPath(BasePath);
        ProjectOpened = true;
    }

    public override void Render()
    {
        if (BeginNoClose("Project Files", flags: ImGuiWindowFlags.AlwaysAutoResize))
        {
            // No project
            if (MainWindow.Workspaces.Current == null)
                ProjectOpened = false;
            if (!ProjectOpened)
            {
                End();
                return;
            }

            RenderHeader();
            RenderContent();
            if (ImGui.BeginPopupContextItem() || ImGui.BeginPopupContextWindow())
            {
                WindowContextMenu();
                ImGui.EndPopup();
            }

            End();
        }
    }

    private void AddPath(string path)
    {
        SetPath(Path.Combine(CurrentPath, path));
    }

    private void SetPath(string path)
    {
        CurrentPath = path;
        PathDecomposition = new List<string>(CurrentRelativePath.Split(Path.DirectorySeparatorChar));
    }

    #region Header

    private void RenderHeader()
    {
        PathComposer();

        ImGui.SetCursorPosY(ImGui.GetCursorPosY());
        ImGui.Separator();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY());
    }

    private void PathComposer()
    {
        if (PathDecomposition.Count > 0)
        {
            float availableWidth = ImGui.GetContentRegionAvail().X; // Get the available width
            float currentLineWidth = 0f;

            for (var idx = 0; idx < PathDecomposition.Count; idx++)
            {
                ImGui.PushID(idx);
                ImGui.PushStyleColor(ImGuiCol.Button, pathDecompColor);

                Vector2 buttonSize = ImGui.CalcTextSize($"/{PathDecomposition[idx]}") + new Vector2(20, 0);

                if (currentLineWidth + buttonSize.X > availableWidth && currentLineWidth > 0)
                {
                    currentLineWidth = 0; // Reset line width counter
                }
                else if (idx > 0)
                {
                    ImGui.SameLine();
                    //ImGui.SetCursorPosX(ImGui.GetCursorPosX());
                }

                var click = ImGui.Button($"/{PathDecomposition[idx]}");
                ImGui.PopStyleColor();
                ImGui.PopID();

                currentLineWidth += buttonSize.X;

                if (click)
                {
                    List<string> paths = [Directory.GetParent(BasePath).FullName];
                    paths.AddRange([.. PathDecomposition.GetRange(0, idx + 1)]);
                    SetPath(Path.Combine([.. paths]));
                    PathClicked = true;
                    break;
                }
            }
        }
    }

    #endregion
    #region Content

    private void RenderContent()
    {
        ImGui.BeginChild("##FileSystemWindow_Child");
        RenderFileTree(CurrentPath);
        ImGui.EndChild();
    }

    #endregion

    public void RenderFileTree(string directoryPath)
    {
        try
        {
            string[] directories = Directory.GetDirectories(directoryPath, "*", new EnumerationOptions() { AttributesToSkip = FileAttributes.Hidden });
            string[] files = Directory.GetFiles(directoryPath);

            int i = 0;
            foreach (var dir in directories)
            {
                string folderName = Path.GetFileName(dir);
                ImGui.PushStyleColor(ImGuiCol.Text, dirTextColor);
                if (ImGui.Selectable($"{FontAwesome6.Folder} {folderName}##FSWin_Folder_{i}", false, flags: ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        AddPath(folderName);
                }
                ImGui.PopStyleColor();
                i++;
            }

            int j = 0;
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName.EndsWith(".lfp"))
                    continue;
                ImGui.Selectable($"{FontAwesome6.File} {fileName}##{fileName}_{i}");
                if (ImGui.BeginPopupContextItem())
                {
                    ImGui.Text(fileName);
                    ImGui.Separator();

                    if (ImGui.Selectable("Open file"))
                    {
                        OpenFile(file);
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGui.GetIO().KeyShift ? ImGuiCol.Text : ImGuiCol.TextDisabled));
                    if (ImGui.Selectable("Delete file") && ImGui.GetIO().KeyShift)
                    {
                        FileInfo fi = new(file);
                        fi.Delete();
                    }
                    ImGui.PopStyleColor();
                    if (!ImGui.GetIO().KeyShift && ImGui.IsItemHovered())
                        ImGui.SetTooltip("Warning: This will delete the file.\nTHIS IS NOT REVERSIBLE.\nHold SHIFT to delete.");
                    ImGui.EndPopup();
                }
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                {
                    OpenFile(file);
                }
                j++;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    #region Context Menus

    public void WindowContextMenu()
    {
        if (ImGui.Button("New Folder"))
        {
            ImGui.OpenPopup("Enter Folder Name");
        }
        if (ImGui.BeginPopup("Enter Folder Name"))
        {
            ImGui.Text("Enter folder name:");
            if (ImGui.InputText("##newFolderName", ref newFolderName, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                string p = Path.Combine(CurrentPath, newFolderName);
                Directory.CreateDirectory(p);
                newFolderName = string.Empty;
                SetPath(p);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
        NewFileContext("Definition", ".lfd");
        NewFileContext("Script", ".lua");
        NewFileContext("Shader", ".lfs");
    }

    public void NewFileContext(string type, string ext)
    {
        if (ImGui.Button($"New {type}"))
        {
            ImGui.OpenPopup("Enter File Name");
        }
        if (ImGui.BeginPopup("Enter File Name"))
        {
            ImGui.Text("Enter file name:");
            if (ImGui.InputText("##newFileName", ref newFileName, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                newFileName = Path.ChangeExtension(newFileName, ext);
                using FileStream fs = File.Create(Path.Combine(CurrentPath, newFileName));
                newFileName = string.Empty;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    #endregion

    public async Task OpenFile(string filePath)
    {
        if (MainWindow.Workspaces.Current!.IsFileOpened(filePath))
            return; // File already opened, don't do anything.

        switch (Path.GetExtension(filePath))
        {
            case ".png":
                break;
            case ".lfd":
                MainWindow.Workspaces.Current!.OpenDefinitionFile(filePath);
                break;
            case ".lua":
                MainWindow.Workspaces.Current!.OpenScriptFile(filePath);
                break;
            case ".lfs":
                MainWindow.Workspaces.Current!.OpenShaderFile(filePath);
                break;
            default:
                return;
        }
    }
}
