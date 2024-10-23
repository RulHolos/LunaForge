using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using Newtonsoft.Json;
using LunaForge.Plugins;
using LunaForge.Execution;
using LunaForge.GUI.Helpers;
using LunaForge.GUI.Windows;
using LunaForge.GUI.ImGuiFileDialog;
using LunaForge.EditorData.Project;
using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Traces;
using LunaForge.EditorData.InputWindows;
using NetSparkleUpdater;
using MoonSharp.Interpreter;
using LunaForge.EditorData.Nodes.NodeData;

namespace LunaForge.GUI;

/*
 * LunaForge - *.lfp (LunaForge Project) for the ini file. - *.lfd (LunaForge Definition) for object definitions.
 * 
 * Design:
 * A project has a .ini file for the configuration and everything else.
 * 
 * A project is NOT a single file but an entire folder.
 * Each object definition is a lua file.
 * 
 * There's a "Project Tree" window will all files.
 * 
 * Each project is a window instance.
 * 
 * Templates are in a zip format and unzipped at creation.
 * Have a single file for the entry point (main menu? (with an option to hijack the launcher.lua?))
 * 
 * Definitions:
 * Each .lfd is a different definition.
 * There is a node to instanciate the ones who can be accessed and another one to load the definition file.
 * The packed "mod" has the same format and file tree as the actual project. Every .lfd are replaced by its lua counterpart.
 * (so it can be loaded by LoadDefinition by just replacing the extension)
 * Definitions CAN contain multiple other definition nodes (object define, item define, ...)
 * 
 * 
 * Plugin system:
 * Plugin manager, loaded at startup. Can add nodes or other features.
 * An interface entry plugin scanned with Reflection and instanciated.
 * Plugin window in config to enable/disable them with warning like "can allow malicious code to run"
 * -----: Discard the API. Turn every "important" classes to internal and keep the Service API.
 * 
 * 
 * Definitions:
 * The typical TreeNode architecture. Most of the software uses these. Every file definition is a single object.
 * One definition can be the entry point of the game.
 * There a node "Import Definition" to allow the editor to allow the user to instanciate this definition. "Create Object"
 * 
 * For the Object Definition node: the parameters of the init() are stored in the defcache.yaml. Each parameter is a new Attribute in the Object Create node.
 * 
 * 
 * Scripts:
 * Pure lua scripts. Can be opened by the editor as code.
 * 
 * 
 * Structure:
 * .LunaForge invisible folder in the root of the project. Contains the defcache.yaml, meta.dat, and other files. Doesn't get included in the templates
 * and is instead generated at runtime on a project on which it's missing.
 * 
 * defcache.yaml : data of all definitions on the project: The file, the object type and optional params and its scope (cf: loaded by another definition)
 * This Definition cache is updated every time a .lfd file is saved.
 * 
 * meta.dat : md5 hash of files to pack.
 * 
 * Export profile: Since THlib is not the only lib existing (KaleidoLib, HolosLib, CuOLib ...), specify a pre-existing profile for exporting (like, arguments and values, etc)
 */

/// <summary>
/// How should a new <see cref="TreeNode"/> be inserted into the tree.
/// </summary>
internal enum InsertMode
{
    /// <summary>
    /// Insert node before the selected one.
    /// </summary>
    Before,
    /// <summary>
    /// Insert node as a child of the currently selected node.
    /// </summary>
    Child,
    /// <summary>
    /// Insert node after the selected one.
    /// </summary>
    After,
}

/// <summary>
/// Entry point of the editor. Must not be instancied more than once.
/// </summary>
internal static class MainWindow
{
    /// <summary>
    /// Static string to be inserted into the window titles.
    /// </summary>
    public static readonly string LunaForgeName = $"LunaForge Editor";
    private static Version? VersionNumber = Assembly.GetEntryAssembly()?.GetName().Version;

    /// <summary>
    /// Should the interface be used and generated on startup? (Should ALWAYS be yes on release versions)
    /// </summary>
    public static bool UseInterface { get; } = true;

    /// <summary>
    /// The plugin manager. Currently not active in alpha. See the API documentation to see how to use plugins.
    /// </summary>
    public static PluginManager PluginManager { get; private set; } = new();

    public static FileDialogManager FileDialogManager { get; set; } = new();

    public static SparkleUpdater Sparkle;

    #region Windows

    public static List<ImGuiWindow> Windows { get; set; } = [];

    public static ToolboxWindow ToolboxWin;
    public static NodeAttributeWindow NodeAttributeWin;
    public static DefinitionsWindow DefinitionsWin;
    public static TracesWindow TracesWin;
    public static DebugLogWindow DebugLogWin;
    public static NewProjWindow NewProjWin;
    public static FileSystemWindow FSWin;
    public static ViewCodeWindow ViewCodeWin;
    public static SparkleWindow SparkleWin;
    public static PluginManagerWindow PluginManagerWin;

    #endregion
    #region Properties

    public static InsertMode InsertMode { get; set; } = InsertMode.Child;

    /// <summary>
    /// The images loaded by <see cref="LoadEditorImages"/>. Must be disposed.
    /// </summary>
    public static Dictionary<string, Texture2D> EditorImages = [];
    /// <summary>
    /// The LunaForge icon <see cref="Image"/>.
    /// </summary>
    public static Image EditorIcon = Raylib.LoadImage(Path.Combine(Directory.GetCurrentDirectory(), "Images/Icon.png"));

    /// <summary>
    /// The list of currently opened <see cref="LunaForgeProject"/>s.
    /// </summary>
    public static ProjectCollection Workspaces;

    public static List<PresetListInfo> PresetsList { get; set; } = [];

    /// <summary>
    /// Script cache for <see cref="LuaNode"/>. Item1 is <see cref="TreeNode.NodeName"/>, Item2 is <see cref="Script"/>.
    /// </summary>
    public static Dictionary<string, Script> ScriptCache = [];

    #endregion

    /// <summary>
    /// Get rid of the loaded <see cref="Texture2D"/> since Raylib is not resource managed.
    /// </summary>
    public static void Dispose()
    {
        foreach (Texture2D texture in EditorImages.Values)
            Raylib.UnloadTexture(texture);
        Raylib.UnloadImage(EditorIcon);
    }

    /// <summary>
    /// Raylib/ImGui window initialization and main rendering loop of the editor.
    /// </summary>
    public static void Initialize()
    {
        Configuration.Load();

        ToolboxWin = new();
        NodeAttributeWin = new();
        DefinitionsWin = new();
        TracesWin = new();
        DebugLogWin = new();
        NewProjWin = new();
        FSWin = new();
        ViewCodeWin = new();
        SparkleWin = new();
        PluginManagerWin = new();

        Workspaces = [];

        try
        {
            PluginManager.GetAllPluginInfo();
            PluginManager.LoadAllEnabledPlugins();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong at plugin loading: {ex}");
        }

        InputWindowSelector.Register(new InputWindowSelectorRegister());

        UserData.RegisterType<LuaNode>();
        UserData.RegisterType<TreeNode>();

        GetPresets();

        // Define the interface only if UseInterface is true. Should always be the case in release.
        if (UseInterface)
        {
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.HighDpiWindow | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(1280, 800, $"{LunaForgeName} v{VersionNumber}");
            Raylib.SetWindowIcon(EditorIcon);
            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.MaximizeWindow();
            Raylib.SetTargetFPS(60);

            LoadEditorImages();

            rlImGui.Setup(true, true);
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

            ShortcutList.RegisterShortcuts();

            bool exitWindow = false;
            bool exitWindowRequested = false;

            while (!exitWindow && !ForceCloseWindow)
            {
                try
                {
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.Black);

                    rlImGui.Begin();

                    ImGui.DockSpaceOverViewport();
                    ShortcutList.CheckKeybinds();

                    RenderMenu();
                    //ImGui.ShowDemoWindow();
                    Render();

                    if (Raylib.WindowShouldClose() || exitWindowRequested) // Will ask save files if they're unsaved then close all opened files then projects.
                    {
                        exitWindowRequested = true;
                        exitWindow = RenderCloseOpenedProjects();
                    }

                    rlImGui.End();

                    Raylib.EndDrawing();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            Dispose();

            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }

        Configuration.Save();
    }

    private static void Render()
    {
        ToolboxWin.Render();
        NodeAttributeWin.Render();
        foreach (LunaForgeProject? proj in Workspaces.ToList())
            proj?.Window?.Render();
        DefinitionsWin.Render();
        TracesWin.Render();
        DebugLogWin.Render();
        NewProjWin.Render();
        FSWin.Render();
        ViewCodeWin.Render();
        SparkleWin.Render();
        PluginManagerWin.Render();

        foreach (ImGuiWindow window in Windows)
            window?.Render();

        InputWindowSelector.CurrentInputWindow?.Render();
        FileDialogManager.Draw();
        NotificationManager.Render();
    }

    private static bool ForceCloseWindow = false;

    public static void ForceClose()
    {
        ForceCloseWindow = true;
    }

    public static bool RenderCloseOpenedProjects()
    {
        if (Workspaces.Count > 0)
        {
            Workspaces[0].CloseProjectAtClosing();
            return false;
        }
        else
            return true;
    }

    #region Init

    public static void SetupDiscordRpc()
    {

    }

    #endregion
    #region RenderMenu

    private static void RenderMenu()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New", "Ctrl+N"))
                    NewProj();
                if (ImGui.MenuItem("Open", "Ctrl+O"))
                    OpenProj();
                ImGui.Separator();
                if (ImGui.MenuItem("Save", "Ctrl+S", false, SaveActiveProjectFile_CanExecute()))
                    SaveActiveProjectFile();
                if (ImGui.MenuItem("Save As", string.Empty, false, SaveActiveProjectAsFile_CanExecute()))
                    SaveActiveProjectFileAs();
                ImGui.Separator();
                ImGui.MenuItem("Close", "Ctrl+Q");
                ImGui.EndMenu();
            }
            /*
            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Edit");
                ImGui.Separator();
                ImGui.MenuItem("Undo");
                ImGui.MenuItem("Redo");
                ImGui.Separator();
                ImGui.MenuItem("Cut");
                ImGui.MenuItem("Copy");
                ImGui.MenuItem("Paste");
                ImGui.Separator();
                ImGui.MenuItem("Ban");
                ImGui.Separator();
                ImGui.MenuItem("Delete");
                ImGui.EndMenu();
            }
            */
            if (ImGui.BeginMenu("Insert"))
            {
                if (ImGui.MenuItem("Before", "Alt+Up", InsertMode == InsertMode.Before))
                    InsertMode = InsertMode.Before;
                if (ImGui.MenuItem("Child", "Alt+Right", InsertMode == InsertMode.Child))
                    InsertMode = InsertMode.Child;
                if (ImGui.MenuItem("After", "Alt+Down", InsertMode == InsertMode.After))
                    InsertMode = InsertMode.After;
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Presets"))
            {
                if (ImGui.MenuItem("Create template from project"))
                    ProjectToTemplate();
                ImGui.Separator();
                if (ImGui.BeginMenu("Presets..."))
                {
                    RenderPresetList();
                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Save Preset", string.Empty, false, NodeToPreset_CanExecute()))
                    NodeToPreset();
                if (ImGui.MenuItem("Refresh preset list"))
                    GetPresets();
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Compile"))
            {
                if (ImGui.MenuItem("Run", "F5", false, RunProject_CanExecute()))
                    RunProject();
                ImGui.MenuItem("Test spell card", "F6");
                ImGui.MenuItem("Test from scene node", "F7");
                if (ImGui.MenuItem("Pack project", string.Empty, false, PackProject_CanExecute()))
                    PackProject();
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("Object Definitions", string.Empty, ref DefinitionsWin.ShowWindow);
                ImGui.EndMenu();
            }
            ImGui.MenuItem("Plugins", string.Empty, ref PluginManagerWin.ShowWindow);
            if (ImGui.BeginMenu("Settings"))
            {
                ImGui.MenuItem("General settings");
                ImGui.MenuItem("Compiler settings");
                ImGui.MenuItem("Editor settings");
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Documentation"))
                    Raylib.OpenURL("https://rulholos.github.io/LunaForge/index.html");
                ImGui.Separator();
                if (ImGui.MenuItem("Check for Updates"))
                    SparkleWin.Sparkle.CheckForUpdatesAtUserRequest();
                ImGui.MenuItem("About");
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    #endregion
    #region Presets

    public static void GetPresets()
    {
        PresetsList.Clear();
        string path = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LunaForge Definition Presets"));
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        foreach (string dirPath in Directory.GetDirectories(path))
        {
            PresetsList.Add(new PresetListInfo(dirPath));
        }
    }

    public static void RenderPresetList()
    {
        int i = 0;
        foreach (PresetListInfo presetDir in  PresetsList)
        {
            ImGui.PushID($"PresetDir{i}");
            if (ImGui.BeginMenu(presetDir.DirName))
            {
                foreach (KeyValuePair<string, string> pair in presetDir.PresetList)
                {
                    if (ImGui.MenuItem(pair.Value, string.Empty, false, InsertPreset_CanExecute()))
                        InsertPreset(pair.Key); // Send the file path.
                    if (ImGui.IsItemHovered() && !InsertPreset_CanExecute())
                        ImGui.SetTooltip("Cannot insert preset without an opened definition file.");
                }
                ImGui.EndMenu();
            }
            ImGui.PopID();
            i++;
        }
    }

    public static void NodeToPreset(TreeNode nodeToSave = null)
    {
        TreeNode node = nodeToSave ?? (Workspaces.Current?.CurrentProjectFile as LunaDefinition).SelectedNode.Clone() as TreeNode;
        void SelectPath(bool success, string path)
        {
            if (!success)
                return;
            try
            {
                using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new(fs);
                node.SerializeToFile(sw, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        FileDialogManager.SaveFileDialog("Create Preset", "LunaForge Preset{.lfdpreset}", "", "LunaForge Definition{.lfdpreset}", SelectPath,
            Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LunaForge Definition Presets")), true);
    }
    public static bool NodeToPreset_CanExecute() => (Workspaces.Current?.CurrentProjectFile as LunaDefinition) != null;

    public static async Task InsertPreset(string path)
    {
        try
        {
            LunaDefinition def = await LunaDefinition.CreateFromFile(Workspaces.Current, path);
            TreeNode node = def.TreeNodes[0] ?? throw new Exception();
            Insert(node, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public static bool InsertPreset_CanExecute() => (Workspaces.Current?.CurrentProjectFile as LunaDefinition) != null;

    #endregion
    #region Editor Images

    public static string DefaultImage => "Unknown";

    /// <summary>
    /// Loads all images inside the "Image" directory to a <see cref="Texture2D"/> and adds them to <see cref="EditorImages"/>.
    /// </summary>
    /// <seealso cref="LoadEditorImageFromFile(string)"/>
    public static void LoadEditorImages()
    {
        EditorImages = [];
        string rootDir = Path.Combine(Directory.GetCurrentDirectory(), "Images");

        if (!Directory.Exists(rootDir))
            return;

        string[] images = Directory.GetFiles(rootDir, "*.png", SearchOption.AllDirectories);
        foreach (string image in images)
        {
            LoadEditorImageFromFile(image);
        }
    }

    /// <summary>
    /// Loads an image from a file path as a <see cref="Texture2D"/> and adds them to <see cref="EditorImages"/>.
    /// </summary>
    public static string LoadEditorImageFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return DefaultImage;

        string key = Path.GetFileNameWithoutExtension(filePath);
        EditorImages.TryAdd(key, Raylib.LoadTexture(filePath));
        return key;
    }

    /// <summary>
    /// Tries to find a <see cref="Texture2D"/> with the corresponding name.
    /// </summary>
    /// <param name="name">The name of the texture to find in <see cref="EditorImages"/>.</param>
    /// <returns>A <see cref="Texture2D"/> object if it's found; otherwise, the default value, which is an "Unknown" image.</returns>
    public static Texture2D FindTexture(string name)
    {
        if (EditorImages.TryGetValue(name, out Texture2D value))
            return value;
        else
            return EditorImages[DefaultImage];
    }

    #endregion
    #region Editor Exec

    /// <summary>
    /// Prompts the user to choose a directory and creates a new project at the given location.
    /// </summary>
    public static void CreateNewProject()
    {
        string pathToTemplate = Path.GetFullPath(NewProjWin.SelectedPath);

        void SelectPath(bool success, string path)
        {
            if (!success)
                return;
            Configuration.Default.LastUsedPath = path;
            CloneTemplate(pathToTemplate, path);
        }

        string lastUsedPath = Configuration.Default.LastUsedPath;
        FileDialogManager.OpenFolderDialog("New Project", SelectPath,
            string.IsNullOrEmpty(lastUsedPath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : lastUsedPath, true);
    }

    /// <summary>
    /// Tried to find a template "*.zip" and extract its content to the specified path.
    /// </summary>
    /// <param name="pathToTemplate">The path to the *.zip template file.</param>
    /// <param name="pathToFolder">The path in which to extract the template.</param>
    public static async void CloneTemplate(string pathToTemplate, string pathToFolder)
    {
        try
        {
#if DEBUG
            // No template, debug only
            if (pathToTemplate.Contains("specialCommand|=GenerateEmpty"))
            {
                LunaForgeProject newProjDebug = new(NewProjWin, Path.Combine(pathToFolder, NewProjWin.ProjectName));
                Workspaces.Add(newProjDebug);
                newProjDebug.TryGenerateProject();
                return; // Return only for debug to avoid cloning the template.
            }
#endif
            await Task.Factory.StartNew(() =>
            {
                ZipFile.ExtractToDirectory(pathToTemplate, Path.Combine(pathToFolder, NewProjWin.ProjectName));
            });
            string pathToLFP = Path.Combine(pathToFolder, NewProjWin.ProjectName, "Project.lfp");
            Console.WriteLine(pathToLFP);
            LunaForgeProject newProj = LunaForgeProject.CreateFromFile(pathToLFP);
            newProj.ResetVariables(NewProjWin);
            newProj.Save();
            newProj.CheckTrace();
            Workspaces.Add(newProj);
        }
        catch (Exception ex)
        {
            NotificationManager.AddToast($"Couldn't clone template.\n{ex.Message}.", ToastType.Error);
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Prompts the used to select a ".lfp" project file and creates a new ImGui window context to open it.
    /// </summary>
    public static void OpenProject()
    {
        void SelectPath(bool success, List<string> paths)
        {
            if (success)
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    if (!IsProjectOpened(paths[i]))
                        OpenProjectFromPath(paths[i]);
                    Configuration.Default.LastUsedPath = Path.GetDirectoryName(paths[i]);
                }
            }
        }

        string lastUsedPath = Configuration.Default.LastUsedPath;
        FileDialogManager.OpenFileDialog("Open Project File", "LunaForge Project{.lfp}", SelectPath, 1, string.IsNullOrEmpty(lastUsedPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : lastUsedPath, true);
    }

    /// <summary>
    /// Tries to detect if a project is already opened within the editor.
    /// </summary>
    /// <param name="path">Path to the project .lfp file.</param>
    /// <returns>True if the project is opened in the editor; otherwise, false.</returns>
    public static bool IsProjectOpened(string path) => Workspaces.Any(x => Path.GetDirectoryName(x.PathToLFP) == Path.GetDirectoryName(path));

    /// <summary>
    /// Tries to load a project .lfp file.
    /// </summary>
    /// <param name="path">The path to the .lfp file.</param>
    /// <returns>A <see cref="LunaForgeProject"/> instance.</returns>
    public static LunaForgeProject? OpenProjectFromPath(string path)
    {
        try
        {
            LunaForgeProject proj = LunaForgeProject.CreateFromFile(path);
            Workspaces.Add(proj);
            proj.CheckTrace();
            return proj;
        }
        catch (JsonException ex)
        {
            NotificationManager.AddToast($"Couldn't open project.\n{ex.Message}", ToastType.Error);
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Handle the disposing of a given project.
    /// </summary>
    /// <param name="proj">The project to close.</param>
    /// <returns>True if all opened files were closed; false if some weren't closed.</returns>
    /// <seealso cref="CloseProjectFile(LunaProjectFile)">This method is called to close individual opened files in the project window.</seealso>
    public static bool CloseProject(LunaForgeProject proj)
    {
        bool allClosed = true;
        foreach (LunaProjectFile file in proj.ProjectFiles.ToList())
        {
            if (!CloseProjectFile(file))
                allClosed = false;
        }
        if (allClosed)
        {
            Workspaces.Remove(proj);
            Workspaces.Current = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Close a single opened <see cref="LunaProjectFile"/> instance.
    /// </summary>
    /// <param name="file">The project file instance.</param>
    /// <returns>True if the file was saved/closed; otherwise, false.</returns>
    [Obsolete]
    public static bool CloseProjectFile(LunaProjectFile file)
    {
        void CloseCallback()
        {
            Workspaces.Current!.ProjectFiles.Remove(file);
            Workspaces.Current!.CurrentProjectFile = null;
        }

        if (file.IsUnsaved)
        {
            /*
            switch (System.Windows.MessageBox.Show($"Do you want to save \"{file.FileName}\"?",
                    LunaForgeName, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    if (SaveProject(file))
                    {
                        Workspaces.Current!.ProjectFiles.Remove(file);
                        Workspaces.Current!.CurrentProjectFile = null;
                    }
                    return true;
                case MessageBoxResult.No:
                    break;
                default:
                    return false;
            }*/
        }
        else
        {
            CloseCallback();
        }
        return true;
    }

    #endregion
    #region Shortcuts

    public static void NewProj()
    {
        NewProjWin.ResetAndShow();
    }
    public static bool NewProj_CanExecute() => true;

    public static void OpenProj()
    {
        OpenProject();
    }
    public static bool OpenProj_CanExecute() => true;

    public static bool SaveActiveProjectFile_CanExecute()
    {
        LunaProjectFile file = Workspaces.Current?.CurrentProjectFile;
        if (file == null)
            return false;
        return file.IsUnsaved;
    }

    public static bool SaveActiveProjectAsFile_CanExecute()
    {
        return Workspaces.Current?.CurrentProjectFile != null;
    }

    public static void Undo()
    {
        Workspaces.Current?.CurrentProjectFile?.Undo();
    }
    public static bool Undo_CanExecute() => Workspaces.Current?.CurrentProjectFile?.CommandStack.Count > 0;

    public static void Redo()
    {
        Workspaces.Current?.CurrentProjectFile?.Redo();
    }
    public static bool Redo_CanExecute() => Workspaces.Current?.CurrentProjectFile?.UndoCommandStack.Count > 0;

    public static void Delete()
    {
        if (Workspaces.Current?.CurrentProjectFile == null)
            return;
        Workspaces.Current.CurrentProjectFile!.Delete();
    }
    public static bool Delete_CanExecute()
    {
        if (Workspaces.Current?.CurrentProjectFile == null)
            return false;
        return Workspaces.Current.CurrentProjectFile!.Delete_CanExecute();
    }

    public static void CutNode()
    {
        (Workspaces.Current?.CurrentProjectFile as LunaDefinition)?.CutNode();
    }
    public static bool CutNode_CanExecute()
    {
        LunaDefinition def = (Workspaces.Current?.CurrentProjectFile as LunaDefinition);
        if (def == null)
            return false;
        return def.CutNode_CanExecute();
    }

    public static void CopyNode()
    {
        (Workspaces.Current?.CurrentProjectFile as LunaDefinition)?.CopyNode();
    }
    public static bool CopyNode_CanExecute()
    {
        LunaDefinition def = (Workspaces.Current?.CurrentProjectFile as LunaDefinition);
        if (def == null)
            return false;
        return def.CopyNode_CanExecute();
    }

    public static void PasteNode()
    {
        (Workspaces.Current?.CurrentProjectFile as LunaDefinition)?.PasteNode();
    }
    public static bool PasteNode_CanExecute()
    {
        LunaDefinition def = (Workspaces.Current?.CurrentProjectFile as LunaDefinition);
        if (def == null)
            return false;
        return def.PasteNode_CanExecute();
    }

    #endregion
    #region Compile

    public static bool OpenedProject() => Workspaces.Current != null;
    public static bool OpenedDefinition() => Workspaces.Current?.CurrentProjectFile as LunaDefinition != null;
    public static bool OpenedScript() => Workspaces.Current?.CurrentProjectFile as LunaScript != null;

    public static void RunProject() => BeginPackingCurrentProject();
    public static bool RunProject_CanExecute() => OpenedProject();
    public static void PackProject() => BeginPackingCurrentProject(run: false);
    public static bool PackProject_CanExecute() => OpenedProject();
    public static void SCDebugProject()
    {
        TreeNode node = (Workspaces.Current?.CurrentProjectFile as LunaDefinition)?.SelectedNode;
        if (node == null)
            return;

        while (node != null)
        {
            // TODO: Match SC Node types.
        }
        BeginPackingCurrentProject(node);
    }
    public static void StageDebugProject()
    {
        TreeNode node = (Workspaces.Current?.CurrentProjectFile as LunaDefinition)?.SelectedNode;
        BeginPackingCurrentProject(null, node);
    }

    /// <summary>
    /// Calls <see cref="BeginPacking(LunaForgeProject, TreeNode, TreeNode, bool)"/> with the currently active project.
    /// </summary>
    /// <param name="SCDebugger"></param>
    /// <param name="StageDebugger"></param>
    /// <param name="run"></param>
    public static void BeginPackingCurrentProject(
        TreeNode SCDebugger = null,
        TreeNode StageDebugger = null,
        bool run = true)
    => BeginPacking(Workspaces.Current, SCDebugger, StageDebugger, run);

    /// <summary>
    /// Starts the compilation process of the project to pack it inside a .zip in the mod folder of the LuaSTG installation folder.
    /// </summary>
    /// <param name="projectToCompile"></param>
    /// <param name="SCDebugger"></param>
    /// <param name="StageDebugger"></param>
    /// <param name="run"></param>
    public static void BeginPacking(
        LunaForgeProject projectToCompile,
        TreeNode SCDebugger = null,
        TreeNode StageDebugger = null,
        bool run = true)
    {
        if (projectToCompile == null)
            return; // This shouldn't happen, but just in case.

        if (EditorTraceContainer.ContainSeverity(TraceSeverity.Error))
        {
            NotificationManager.AddToast($"There are errors inside your project.\nFix them and try again.", ToastType.Error);
            return;
        }

        // Threading for packing to not stop the UI/Main thread.
        Thread packingThread = new(async () =>
        {
            NotificationManager.AddToast($"Beginning packaging...", ToastType.Info);
            try
            {
                bool saveMeta = false;
                if (!run)
                {
                    saveMeta = projectToCompile.UseMD5Files;
                }

                DebugLogWin.DebugLogContent = string.Empty;
                projectToCompile.GatherCompileInfo();
                projectToCompile.CompileProcess.ProgressChanged +=
                    (o, e) => PackageProgressReport(o, e);
                bool success = await projectToCompile.CompileProcess.ExecuteProcess(SCDebugger != null, StageDebugger != null);
                if (success)
                {
                    NotificationManager.AddToast($"Packaging finished!", ToastType.Success);
                    if (run)
                        RunLuaSTG();
                }
                else
                {
                    NotificationManager.AddToast($"Packaging failed...", ToastType.Error);
                }
            }
            catch (Exception ex)
            {
                NotificationManager.AddToast($"Packaging failed...", ToastType.Error);
                Console.WriteLine(ex.ToString());
                return;
            }
        });
        packingThread.Start();
        
    }
    public static bool BeginPacking_CanExecute() => Workspaces.Current != null;

    private static void PackageProgressReport(object sender, ProgressChangedEventArgs args)
    {
        DebugLogWin.DebugLogContent += args.UserState?.ToString() + "\n";
    }

    public static void RunLuaSTG()
    {
        LSTGExecution exec = null;
        LunaForgeProject proj = Workspaces.Current;
        proj.SetTargetVersion();
        switch (proj.TargetLuaSTG)
        {
            case TargetVersion.Plus:
                exec = new PlusExecution(proj);
                break;
            case TargetVersion.Sub:
                exec = new SubExecution(proj);
                break;
            case TargetVersion.x:
                exec = new XExecution(proj);
                break;
            case TargetVersion.Evo:
                exec = null;
                break;
        }
        exec.BeforeRun();
        exec.Run((s) =>
        {
            DebugLogWin.DebugLogContent = s;
        }, () => { });
    }

    #endregion
    #region Project Operation

    /// <summary>
    /// Saves the currently selected <see cref="LunaProjectFile"/>.
    /// </summary>
    /// <returns>True if the save went without problem; otherwise, false.</returns>
    /// <seealso cref="SaveActiveProjectFileAs">Saves the project file but as a new file.</seealso>
    public static void SaveActiveProjectFile() => SaveProject(Workspaces.Current?.CurrentProjectFile);
    /// <summary>
    /// Saves the currently selected <see cref="LunaProjectFile"/> as new file.
    /// </summary>
    /// <returns>True if the save went without problem; otherwise, false.</returns>
    /// <seealso cref="SaveActiveProjectFile">Saves the project file normally.</seealso>
    public static void SaveActiveProjectFileAs() => SaveProject(Workspaces.Current?.CurrentProjectFile, true);

    /// <summary>
    /// Saves the given <see cref="LunaProjectFile"/> normally or as a new file.
    /// </summary>
    /// <param name="projFile">The project file to save.</param>
    /// <param name="saveAs">Tells the editor to save the file as a new file.</param>
    /// <returns>True if the save went without problem; otherwise, false.</returns>
    /// <seealso cref="SaveActiveProjectFile">Saves the project file normally.</seealso>
    /// <seealso cref="SaveActiveProjectFileAs">Saves the project file but as a new file.</seealso>
    public static void SaveProject(LunaProjectFile projFile, bool saveAs = false)
    {
        projFile.Save(saveAs);
    }

    /// <summary>
    /// Tries to pack the currently selected project into a .zip template being able to be used by <see cref="CreateNewProject"/>.
    /// </summary>
    /// <returns>True if the .zip was created successfuly; otherwise, false.</returns>
    public static bool ProjectToTemplate()
    {
        LunaForgeProject? currentProj = Workspaces.Current;
        if (currentProj == null)
            return false;

        try
        {
            string pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"{currentProj.ProjectName}.zip");
            ZipFile.CreateFromDirectory(currentProj.PathToProjectRoot, pathToFile);
            NotificationManager.AddToast($"Templated created!\n({pathToFile})", ToastType.Success);
            return true;
        }
        catch (Exception ex)
        {
            NotificationManager.AddToast($"Couldn't create template.\n{ex.Message}", ToastType.Error);
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    #endregion
    #region Node Operation

    /// <summary>
    /// Inserts a new node into the selected <see cref="LunaDefinition"/>'s tree.
    /// </summary>
    /// <param name="node">The new node to insert into the tree.</param>
    /// <param name="doInvoke">Tells the editor to execute the <see cref="TreeNode.GetCreateInvoke"/> attribute.</param>
    /// <returns></returns>
    public static bool Insert(TreeNode node, bool doInvoke = true)
    {
        try
        {
            return (Workspaces.Current?.CurrentProjectFile as LunaDefinition).Insert(node, doInvoke);
        }
        catch (Exception ex)
        {
            NotificationManager.AddToast($"Cannot insert node.\n{ex.Message}", ToastType.Error);
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public static void ClearScriptCache() => ScriptCache.Clear();

    #endregion
}
