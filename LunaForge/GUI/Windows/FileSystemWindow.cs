using IconFonts;
using ImGuiNET;
using LunaForge.EditorData.Project;
using LunaForge.GUI.Helpers;
using LunaForge.GUI.ImGuiFileDialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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

    // TODO: Correct fucking context menus. Please.

    public FileSystemWindow() : base(true) { }

    public void InitializeProject(LunaForgeProject project)
    {
        BasePath = project.PathToProjectRoot;
        SetPath(BasePath);
        ProjectOpened = true;
    }

    private bool IsItemPopupOpen = false;

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

            if (MainWindow.IsOpeningFile)
            {
                ImGui.ProgressBar(-1.0f * (float)ImGui.GetTime(), new Vector2(-1.0f, 0.0f), "Opening file...");
            }

            RenderHeader();
            RenderContent();
            if (!IsItemPopupOpen)
            {
                if (ImGui.BeginPopupContextItem() || ImGui.BeginPopupContextWindow())
                {
                    bool shouldClose = false;
                    WindowContextMenu(ref shouldClose);
                    if (shouldClose)
                        ImGui.CloseCurrentPopup();
                    ImGui.EndPopup();
                }
                else
                {
                    newFilePopupOpen = -1;
                    newFolderPopupOpen = false;
                }
            }

            End();
            IsItemPopupOpen = false;
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

                var click = ImGuiEx.DoubleClickButton($"/{PathDecomposition[idx]}");
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
    #region Context Menus

    public static string AddEllipses(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }

        string ellipses = "...";
        string trimmedText = text[^(maxLength - ellipses.Length)..];

        return ellipses + trimmedText;
    }

    private bool newFolderPopupOpen = false;
    private int newFilePopupOpen = -1;

    public void WindowContextMenu(ref bool shouldClose)
    {
        ImGui.MenuItem($"{AddEllipses(CurrentRelativePath, 30)}", null, false, false);
        ImGui.Separator();
        if (ImGui.Selectable("New Folder", false, ImGuiSelectableFlags.NoAutoClosePopups) || newFolderPopupOpen)
        {
            ImGui.OpenPopup("Enter Folder Name");
            newFolderPopupOpen = true;
        }
        if (ImGui.BeginPopup("Enter Folder Name"))
        {
            ImGui.Text("Enter Folder name:");
            ImGui.SetKeyboardFocusHere();
            if (ImGui.InputText("##newFolderName", ref newFolderName, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                string p = Path.Combine(CurrentPath, newFolderName);
                Directory.CreateDirectory(p);
                newFolderName = string.Empty;
                SetPath(p);
                ImGui.CloseCurrentPopup();
                newFolderPopupOpen = false;
                shouldClose = true;
            }
            ImGui.EndPopup();
        }
        NewFileContext("Definition", ".lfd", 0, ref shouldClose);
        NewFileContext("Script", ".lua", 1, ref shouldClose);
        NewFileContext("Shader", ".lfs", 2, ref shouldClose);
    }

    public void NewFileContext(string type, string ext, int housamas_return, ref bool shouldClose)
    {
        if (ImGui.Selectable($"New {type}", false, ImGuiSelectableFlags.NoAutoClosePopups) || newFilePopupOpen == housamas_return)
        {
            ImGui.OpenPopup($"Enter {type} Name");
            newFilePopupOpen = housamas_return;
        }
        if (ImGui.BeginPopup($"Enter {type} Name"))
        {
            ImGui.Text($"Enter {type} name:");
            ImGui.SetKeyboardFocusHere();
            if (ImGui.InputText("##newFileName", ref newFileName, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                newFileName = Path.ChangeExtension(newFileName, ext);
                using FileStream fs = File.Create(Path.Combine(CurrentPath, newFileName));
                newFileName = string.Empty;
                ImGui.CloseCurrentPopup();
                newFilePopupOpen = -1;
                shouldClose = true;
            }
            ImGui.EndPopup();
        }
    }

    public void FileContextMenu(string file, string fileName)
    {
        if (ImGui.BeginPopupContextItem())
        {
            IsItemPopupOpen = true;
            ImGui.MenuItem(fileName, null, false, false);
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

            ImGui.Separator();

            if (ImGui.Selectable("Open in file explorer"))
            {
                string path = Path.GetDirectoryName(file);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Process.Start("explorer.exe", path);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    Process.Start("open", path);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    Process.Start("xdg-open", path);
            }

            ImGui.EndPopup();
        }
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
                if (ImGui.Selectable($" {FontAwesome6.Folder} {folderName}##FSWin_Folder_{i}", false, flags: ImGuiSelectableFlags.AllowDoubleClick))
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
                ImGui.Selectable($" {FontAwesome6.File} {fileName}##{fileName}_{i}");
                FileContextMenu(file, fileName);
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    OpenFile(file);
                }
                j++;
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            NotificationManager.AddToast("Couldn't find directory path.\nStepping back.", ToastType.Warning);
            SetPath(Directory.GetParent(directoryPath).FullName);
            Console.WriteLine($"Couldn't find current path, reverting to the previous one.\n{ex}");
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public async Task OpenFile(string filePath)
    {
        if (MainWindow.Workspaces.Current!.IsFileOpened(filePath) || MainWindow.IsOpeningFile)
            return; // File already opened or is in the proccess of opening this or another file: Don't do anything.

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
