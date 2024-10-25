using LunaForge.GUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaForge.EditorData.Project;
using System.Numerics;
using Raylib_cs;
using System.IO;
using System.Diagnostics;
using LunaForge.GUI.ImGuiFileDialog;
using LunaForge.EditorData.Toolbox;

namespace LunaForge.GUI.Windows;

public class ProjectViewerWindow : ImGuiWindow
{
    public LunaForgeProject ParentProject { get; set; }
    public bool JustCreated = true;

    public LunaProjectFile? fileToClose = null;
    public LunaProjectFile? filePendingModal = null;

    private bool SettingsModalClosed = true;

    private bool ShouldOpenSettings = false;

    private bool ShouldForceClose = false;

    public ProjectViewerWindow()
        : base(true) { }

    public override void Render()
    {
        if (!ShowWindow)
            return;

        if (JustCreated)
        {
            ImGui.SetNextWindowFocus();
            JustCreated = false;
        }

        ImGui.PushID(ParentProject.Hash);
        if (Begin($"{ParentProject.ProjectName}"))
        {
            UpdateCurrentProject();

            if (ImGui.BeginTabBar($"{ParentProject.ProjectName}OpenFilesTab", ImGuiTabBarFlags.AutoSelectNewTabs))
            {
                if (ParentProject.ProjectFiles.Count == 0)
                {
                    if (ImGui.BeginTabItem("Empty"))
                    {
                        ImGui.Text("Select a file to open in the \"Project Files\" window to begin editing.");
                        ImGui.EndTabItem();
                    }
                }
                foreach (LunaProjectFile file in ParentProject.ProjectFiles.ToList())
                {
                    ImGui.PushID(file.Hash);

                    bool isOpened = file.IsOpened;
                    ImGuiTabItemFlags flags = ImGuiTabItemFlags.NoPushId
                        | ImGuiTabItemFlags.NoReorder
                        | ImGuiTabItemFlags.NoAssumedClosure;
                    if (file.IsUnsaved)
                        flags |= ImGuiTabItemFlags.UnsavedDocument;

                    if (ImGui.BeginTabItem(file.FileName, ref isOpened, flags))
                    {
                        if (ParentProject.CurrentProjectFile == null || ParentProject.CurrentProjectFile != file)
                        {
                            ParentProject.CurrentProjectFile = file;
#if DEBUG
                            Console.WriteLine($"Current ProjectFile: {file.FileName}");
#endif
                        }
                        file.Render();

                        ImGui.EndTabItem();
                    }
                    file.IsOpened = isOpened;

                    ImGui.PopID();

                    if (!file.IsOpened && ParentProject.ProjectFiles.Contains(file))
                    {
                        if (file.IsUnsaved)
                        {
                            filePendingModal = file;
                            ImGui.OpenPopup("Confirm close of unsaved file");
                            file.IsOpened = true;
                        }
                        else
                        {
                            fileToClose = file;
                        }
                    }
                }

                if (ImGui.TabItemButton($"{IconFonts.FontAwesome6.Gear}##{ParentProject.ProjectName}", ImGuiTabItemFlags.Trailing) || ShouldOpenSettings)
                {
                    OpenSettings();
                    OpenSettingsPopup();
                }

                ConfirmCloseModal();
                RenderProjectSettings();

                // Close the file if confirmed
                if (fileToClose != null)
                {
                    fileToClose?.Close();
                    fileToClose = null;
                }
                

                ImGui.EndTabBar();
            }

            End();
        }
        ImGui.PopID();

        if (!ShowWindow || ShouldForceClose)
            CheckProjectSaveState();
    }

    public async void ConfirmCloseModal()
    {
        if (ImGui.BeginPopupModal("Confirm close of unsaved file", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDocking))
        {
            if (!filePendingModal.ForceClose)
                ImGui.Text($"The file \"{filePendingModal?.FileName}\" has unsaved changes. Do you really want to close it?");
            else
                ImGui.Text($"The file \"{filePendingModal?.FileName}\" has unsaved changes. Do you want to save before closing?");

            if (ImGui.Button("Yes"))
            {
                if (filePendingModal.ForceClose)
                    await filePendingModal.Save();
                fileToClose = filePendingModal;
                filePendingModal = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("No"))
            {
                if (filePendingModal.ForceClose)
                    fileToClose = filePendingModal;
                filePendingModal = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    public void UpdateCurrentProject()
    {
        if (ImGui.IsWindowFocused() && ParentProject.Parent.Current != ParentProject)
        {
            ParentProject.Parent.Current = ParentProject;
            MainWindow.FSWin.InitializeProject(ParentProject);
#if DEBUG
            Console.WriteLine($"Current Project: {ParentProject.ProjectName}");
#endif
        }
    }

    public void CheckProjectSaveState()
    {
        ShowWindow = true;
        ShouldForceClose = true;
        ParentProject.CloseProjectAtClosing();
        //ParentWindow.CloseProject(ParentProject);
    }

    #region Project Settings

    public readonly List<Vector2> ListOfRes = [
        new(640, 480),
        new(800, 600),
        new(960, 720),
        new(1024, 768),
        new(1280, 960)
    ];

    public string TempPathToLuaSTGExecutable;
    public string TempEntryPoint;
    public bool TempUseMD5Files;
    public bool TempCheckUpdatesOnStartup;
    public bool TempUseFolderPacking;
    public bool TempWindowed;
    public bool TempCheat;
    public bool TempLogWindowSub;
    public Vector2 TempDebugRes;
    public int TempSelectedRes;
    public int TempSelectedNodePluginId = 0;
    public NodePlugin TempSelectedNodePlugin = null;
    public string TempDifficulties = string.Empty; // 1 difficulty per line. Detected with line breaks.

    public void OpenSettings()
    {
        ShouldOpenSettings = true;
    }

    private void OpenSettingsPopup()
    {
        if (!ShouldOpenSettings)
            return;
        ImGui.OpenPopup($"Project Settings##{ParentProject.ProjectName}");
        ShouldOpenSettings = false;
    }

    public void RenderProjectSettings()
    {
        Vector2 modalSize = new(700, 500);
        Vector2 renderSize = new(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
        ImGui.SetNextWindowSize(modalSize);
        ImGui.SetNextWindowPos(renderSize/2 - (modalSize/2));
        if (ImGui.BeginPopupModal($"Project Settings##{ParentProject.ProjectName}", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDocking))
        {
            if (SettingsModalClosed)
                GetSettings();

            if (ImGui.BeginTabBar($"{ParentProject.ProjectName}_ProjectSettings"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    RenderLuaSTGPath();
                    RenderEntryPointPath();
                    RenderUseMD5();
                    RenderUseFolderPacking();
                    RenderCheckUpdatesOnStartup();

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Debug"))
                {
                    RenderDebugRes();
                    ImGui.Checkbox("Windowed", ref TempWindowed);
                    ImGui.Spacing();
                    ImGui.Checkbox("Cheat mode", ref TempCheat);
                    ImGui.Spacing();
                    ImGui.Checkbox("Enable log window (LuaSTG Sub only)", ref TempLogWindowSub);
                    ImGui.Separator();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Nodes & Plugins"))
                {
                    RenderNodePluginList();

                    ImGui.Spacing();
                    ImGui.Separator();

                    RenderDifficultiesList();
                }

                ImGui.EndTabBar();
            }

            // Set buttons at the bottom.
            float availableHeight = ImGui.GetWindowHeight() - ImGui.GetCursorPosY();
            float buttonHeight = ImGui.CalcTextSize("Ok").Y + ImGui.GetStyle().FramePadding.Y * 2;
            float spacing = ImGui.GetStyle().ItemSpacing.Y + 4;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + availableHeight - buttonHeight - spacing);

            if (ImGui.Button("Ok"))
                ApplySettings();
            ImGui.SameLine();
            if (ImGui.Button("Apply"))
                ApplySettings(true);
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                SettingsModalClosed = true;
            }

            ImGui.EndPopup();
        }
    }

    #region Draw Options

    public void RenderLuaSTGPath()
    {
        bool validPath = File.Exists(TempPathToLuaSTGExecutable) && TempPathToLuaSTGExecutable.EndsWith(".exe");
        ImGui.PushStyleColor(ImGuiCol.Text, validPath ? ImGui.GetColorU32(ImGuiCol.Text) : 0xFF0000FFu);
        ImGui.Text("Path to LuaSTG Executable");
        if (!validPath && ImGui.IsItemHovered())
            ImGui.SetTooltip("This path is invalid.\nCheck that it redirects to an executable file.");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(450);
        ImGui.InputText("##PathToLuaSTGExecutable", ref TempPathToLuaSTGExecutable, 1024);
        ImGui.SameLine();
        if (ImGui.Button("...##PathToLuaSTGExecutableBtn"))
            PromptLuaSTGPath();

        ImGui.Text($"Target version: {GetTargetVersion()}");

        ImGui.Spacing();
    }

    private string GetTargetVersion()
    {
        if (!File.Exists(TempPathToLuaSTGExecutable))
        {
            return "No LuaSTG executable selected.";
        }
        FileVersionInfo LuaSTGExecutableInfos = FileVersionInfo.GetVersionInfo(TempPathToLuaSTGExecutable);
        ParentProject.SetTargetVersion(TempPathToLuaSTGExecutable);
        return $"{LuaSTGExecutableInfos.ProductName} v{LuaSTGExecutableInfos.ProductVersion}";
    }

    public void RenderEntryPointPath()
    {
        bool validPath = File.Exists(TempEntryPoint) && (TempEntryPoint.EndsWith(".lua") || TempEntryPoint.EndsWith(".lfd"));
        ImGui.PushStyleColor(ImGuiCol.Text, validPath ? ImGui.GetColorU32(ImGuiCol.Text) : 0xFF0000FFu);
        ImGui.Text("Path to project entry point");
        if (!validPath && ImGui.IsItemHovered())
            ImGui.SetTooltip("This path is invalid.\nCheck that it redirects to a .lfd or .lua file.");
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(450);
        ImGui.InputText("##PathEntryPoint", ref TempEntryPoint, 1024);
        ImGui.SameLine();
        if (ImGui.Button("...##PathEntryPointBtn"))
            PromptEntryPointPath();

        ImGui.Spacing();
        ImGui.Separator();
    }

    private void RenderUseMD5()
    {
        ImGui.Checkbox("Use MD5 hash file check during packing project.", ref TempUseMD5Files);

        ImGui.Spacing();
    }

    private void RenderUseFolderPacking()
    {
        ImGui.Checkbox("Use Folder Packing.", ref TempUseFolderPacking);

        ImGui.Spacing();
        ImGui.Separator();
    }

    private void RenderCheckUpdatesOnStartup()
    {
        ImGui.Checkbox("Check for updates on startup.", ref TempCheckUpdatesOnStartup);

        ImGui.Spacing();
    }

    private void RenderDebugRes()
    {
        List<string> itemStrings = [];
        foreach (var item in ListOfRes)
        {
            itemStrings.Add($"{item.X} x {item.Y}");
        }

        string[] itemArray = itemStrings.ToArray();

        if (ImGui.Combo("Debug Resolution", ref TempSelectedRes, itemArray, itemArray.Length))
        {
            TempDebugRes = ListOfRes[TempSelectedRes];
        }
    }

    private void RenderNodePluginList()
    {
        ImGui.BeginGroup();
        {
            Vector2 listSize = new(ImGui.GetContentRegionAvail().X / 2, ImGui.GetContentRegionAvail().Y - 300);
            if (ImGui.BeginListBox($"##{ParentProject.ProjectName}_ProjectNodes", listSize))
            {
                int i = 0;
                foreach (NodePlugin nodePlugin in ParentProject.Toolbox.Plugins)
                {
                    if (nodePlugin.DisplayName == "System")
                        continue;
                    ImGui.PushStyleColor(ImGuiCol.Text, nodePlugin.Enabled ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(ImGuiCol.TextDisabled));
                    if (ImGui.Selectable($"{nodePlugin.DisplayName}##nodePlugin_{i}", TempSelectedNodePluginId == i))
                    {
                        TempSelectedNodePluginId = i;
                        TempSelectedNodePlugin = nodePlugin;
                    }
                    ImGui.PopStyleColor();
                    i++;
                }
                ImGui.EndListBox();
            }

            ImGui.SameLine();

            if (TempSelectedNodePlugin != null)
            {
                ImGui.BeginGroup();
                ImGui.Text(TempSelectedNodePlugin.DisplayName);
                if (!string.IsNullOrEmpty(TempSelectedNodePlugin.Authors))
                    ImGui.TextWrapped($"By: {TempSelectedNodePlugin.Authors}");
                string btnText = TempSelectedNodePlugin.Enabled ? "Disable" : "Enable";
                if (ImGui.Button($"{btnText}"))
                {
                    TempSelectedNodePlugin.ToggleState(ParentProject);
                }
                ImGui.TextWrapped("(You will need to reload your project for these changes to take effect.)");
                ImGui.EndGroup();
            }
        }
        ImGui.EndGroup();
    }

    private void RenderDifficultiesList()
    {
        Vector2 listSize = new(ImGui.GetContentRegionAvail().X / 3, 100);
        ImGui.Text("Difficulty list.\nPlease enter one difficulty per line.");
        ImGui.InputTextMultiline("##DifficultyListInput", ref TempDifficulties, (uint)ImGui.GetContentRegionAvail().X, listSize);
    }

    #endregion

    public void GetSettings()
    {
        TempPathToLuaSTGExecutable = ParentProject.PathToLuaSTGExecutable;
        TempEntryPoint = ParentProject.EntryPoint;
        TempUseMD5Files = ParentProject.UseMD5Files;
        TempCheckUpdatesOnStartup = ParentProject.CheckUpdatesOnStartup;
        TempUseFolderPacking = ParentProject.UseFolderPacking;
        TempWindowed = ParentProject.Windowed;
        TempCheat = ParentProject.Cheat;
        TempLogWindowSub = ParentProject.LogWindowSub;
        TempDebugRes = ParentProject.DebugRes;
        TempSelectedRes = ListOfRes.IndexOf(ParentProject.DebugRes);
        TempDifficulties = string.Join('\n', ParentProject.Difficulties);

        SettingsModalClosed = false;
    }

    public void ApplySettings(bool quitPopup = false)
    {
        ParentProject.PathToLuaSTGExecutable = TempPathToLuaSTGExecutable;
        ParentProject.EntryPoint = TempEntryPoint;
        ParentProject.UseMD5Files = TempUseMD5Files;
        ParentProject.CheckUpdatesOnStartup = TempCheckUpdatesOnStartup;
        ParentProject.UseFolderPacking = TempUseFolderPacking;
        ParentProject.Windowed = TempWindowed;
        ParentProject.Cheat = TempCheat;
        ParentProject.LogWindowSub = TempLogWindowSub;
        ParentProject.DebugRes = TempDebugRes;
        ParentProject.Difficulties = [.. TempDifficulties.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

        ParentProject.Save();

        ParentProject.CheckTrace();

        if (quitPopup)
        {
            ImGui.CloseCurrentPopup();
            SettingsModalClosed = true;
        }
    }

    /// <summary>
    /// Let the user choose the LuaSTG Executable Path.
    /// </summary>
    public void PromptLuaSTGPath()
    {
        void SelectPath(bool success, List<string> paths)
        {
            if (!success)
                TempPathToLuaSTGExecutable = paths[0];
            ShouldOpenSettings = true;
        }

        string lastUsedPath = Configuration.Default.LastUsedPath;
        MainWindow.FileDialogManager.OpenFileDialog("Choose Executable", "LuaSTG Executable{.exe}", SelectPath, 1, string.IsNullOrEmpty(ParentProject.PathToLuaSTGExecutable)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : Path.GetDirectoryName(ParentProject.PathToLuaSTGExecutable), true);
    }

    public void PromptEntryPointPath()
    {
        void SelectPath(bool success, List<string> paths)
        {
            if (success)
                TempEntryPoint = paths[0];
            ShouldOpenSettings = true;
        }

        string lastUsedPath = Configuration.Default.LastUsedPath;
        MainWindow.FileDialogManager.OpenFileDialog("Choose Definition", "LunaForge Definition{.lfd}", SelectPath, 1, string.IsNullOrEmpty(ParentProject.PathToProjectRoot)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : Path.GetDirectoryName(ParentProject.PathToProjectRoot), true);
    }

    #endregion
}
