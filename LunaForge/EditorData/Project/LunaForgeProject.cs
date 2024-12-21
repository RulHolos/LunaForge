using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaForge.EditorData.Commands;
using LunaForge.GUI;
using LunaForge.GUI.Windows;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using LunaForge.EditorData.Traces;
using LunaForge.EditorData.Traces.EditorTraces;
using LunaForge.GUI.Helpers;
using LunaForge.EditorData.Toolbox;
using DiscordRPC;

namespace LunaForge.EditorData.Project;

public delegate void UpdateSelectedProjectEventHandler();
public delegate void UnloadProjectEventHandler();

public partial class LunaForgeProject : ITraceThrowable
{
    #region Configuration

    [DefaultValue("")]
    public string AuthorName { get; set; }

    [DefaultValue("")]
    public string ProjectName { get; set; }

    [DefaultValue(false)]
    public bool AllowPr { get; set; }

    [DefaultValue(false)]
    public bool AllowScPr { get; set; }

    [DefaultValue("")]
    public string PathToLuaSTGExecutable = string.Empty;

    [DefaultValue(true)]
    public bool CheckUpdatesOnStartup = true;

    [DefaultValue(false)]
    public bool UseFolderPacking = false;

    [DefaultValue(true)]
    public bool Windowed = true;

    [DefaultValue(false)]
    public bool Cheat = false;

    [DefaultValue(false)]
    public bool LogWindowSub = false;

    // Project-wide defined difficulties. By default: ["Easy", "Normal", "Hard", "Lunatic", "Extra", "Phantasm"];
    public List<string> Difficulties = ["Easy", "Normal", "Hard", "Lunatic", "Extra", "Phantasm"];

    [DefaultValue("start_game=true is_debug=true setting.nosplash=true setting.windowed={0} setting.resx={1} setting.resy={2} cheat={3} updatelib=false setting.mod={4}")]
    public string LaunchArguments = "start_game = true is_debug=true setting.nosplash=true setting.windowed={0} setting.resx={1} setting.resy={2} cheat={3} updatelib=false setting.mod={4}";

    public Vector2 DebugRes = new(800, 600);

    /// <summary>
    /// Path to the entry point lfd or lua file. Relative to <see cref="PathToProjectRoot"/>
    /// </summary>
    [DefaultValue("")]
    public string EntryPoint = string.Empty;

    [YamlIgnore]
    public string EntryPointRelative => Path.GetRelativePath(PathToProjectRoot, EntryPoint).Replace("\\", "/");

    /// <summary>
    /// Replace files in the zip only if the MD5 hash signature doesn't match.<br/>
    /// TODO: Will require the lfp.meta file.
    /// </summary>
    [DefaultValue(true)]
    public bool UseMD5Files = true;

    /// <summary>
    /// List of Node Plugins not to load at project loading. (loads the path relative to the project root.)
    /// </summary>
    public List<string> DisabledNodePlugins = [];
    public void SetDisabledPlugin(string fullPath)
    {
        string path = Path.GetRelativePath(PathToProjectRoot, fullPath);
        if (!DisabledNodePlugins.Contains(path))
            DisabledNodePlugins.Add(path);
    }

    #endregion

    [YamlIgnore]
    public List<EditorTrace> Traces { get; private set; } = [];

    [YamlIgnore]
    public string PathToProjectRoot { get; set; }
    [YamlIgnore]
    public string PathToLFP => Path.Combine(PathToProjectRoot, "Project.lfp");
    [YamlIgnore]
    public string PathToData => Path.Combine(PathToProjectRoot, ".lunaforge");
    [YamlIgnore]
    public string PathToNodeData => Path.Combine(PathToData, "nodes");

    [YamlIgnore]
    public int Hash { get; set; }
    [YamlIgnore]
    public int ProjectFileMaxHash = 0;

    [YamlIgnore]
    public bool IsSelected;
    [YamlIgnore]
    public bool IsUnsaved { get; set; } = false;

    [YamlIgnore]
    public ProjectViewerWindow Window { get; set; }
    [YamlIgnore]
    public ProjectCollection Parent { get; set; }
    [YamlIgnore]
    public List<LunaProjectFile> ProjectFiles { get; set; } = [];
    [YamlIgnore]
    public LunaProjectFile? CurrentProjectFile = null;

    [YamlIgnore]
    public CompileProcess CompileProcess { get; set; }

    [YamlIgnore]
    public TargetVersion TargetLuaSTG { get; private set; }

    [YamlIgnore]
    public DefinitionsCache DefCache { get; private set; }

    [YamlIgnore]
    public NodePicker Toolbox { get; private set; }

    [YamlIgnore]
    public DateTime StartedTimestamp { get; private set; }

    [YamlIgnore]
    public AutoBackup Backups { get; private set; }

    /* This fucking line took 1 hour of my life for nothing.
     * YamlDotNet, please make your fucking Exceptions more precise. How the fuck was I supposed to know that
     * "(Line: 1, Col: 1, Idx: 0) - (Line: 1, Col: 1, Idx: 0): Exception during deserialization"
     * means that I'm missing a fucking constructor????
     * Please.
     */
    public LunaForgeProject() : this(null, string.Empty) { }

    public LunaForgeProject(NewProjWindow? newProjWin, string rootFolder)
    {
        AuthorName = newProjWin?.Author;
        ProjectName = newProjWin?.ProjectName;
        AllowPr = newProjWin?.AllowPr ?? false;
        AllowScPr = newProjWin?.AllowScPr ?? false;
        PathToProjectRoot = rootFolder;

        if (Configuration.Default.AutoBackup)
        {
            Backups = new();
            Backups.Setup(this, Configuration.Default.AutoBackupFreq);
        }

        StartedTimestamp = DateTime.UtcNow;

        OnUpdateSelectedProject += UpdateRPC;
        OnUnloadProject += MainWindow.ResetRPC;
        OnUnloadProject += Backups.Stop;
    }

    #region IO

    public void ResetVariables(NewProjWindow newProjWin)
    {
        AuthorName = newProjWin.Author;
        ProjectName = newProjWin.ProjectName;
        AllowPr = newProjWin.AllowPr;
        AllowScPr = newProjWin.AllowScPr;
    }

    /// <summary>
    /// Tries and generate the project folder structure and the configuration/project file.ini.<br/>
    /// Only use this for debugging since the editor only supports template copying at Release.
    /// </summary>
    /// <returns>False if the project already exists or something went wrong. True if the Project was created.</returns>
    public bool TryGenerateProject()
    {
        if (Directory.Exists(PathToProjectRoot))
            return false;

        try
        {
            // Create folders
            Directory.CreateDirectory(PathToProjectRoot);
            Directory.CreateDirectory(Path.Combine(PathToProjectRoot, "Definitions"));
            Directory.CreateDirectory(Path.Combine(PathToProjectRoot, "Scripts"));
            Directory.CreateDirectory(Path.Combine(PathToProjectRoot, "Assets"));

            // Create .lfp file
            Save();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }

    }

    public void CloseProjectAtClosing()
    {
        if (ProjectFiles.Count == 0)
        {
            Parent.Remove(this);
            RemoveTraces();
            MainWindow.ScriptCache.Clear(); // TODO: Only clear the cache of this project.
            RaiseUnloaded();
            Parent.Current = null;
        }
        else
        {
            ProjectFiles[0].ForceClose = true;
            ProjectFiles[0].IsOpened = false;
        }
    }

    public bool IsFileOpened(string path) => ProjectFiles.Any(x => x.FullFilePath == path);

    #endregion
    #region Serialization

    public bool Save()
    {
        try
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(this);
            using StreamWriter sw = new(PathToLFP);
            sw.Write(yaml);
            DefCache.Save();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public static LunaForgeProject CreateFromFile(string pathToFile)
    {
        try
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            using StreamReader sr = new(pathToFile);
            LunaForgeProject proj = deserializer.Deserialize<LunaForgeProject>(sr);
            proj.PathToProjectRoot = Path.GetDirectoryName(pathToFile);

            ProjectFileSystem.CreateLunaForgeData(proj.PathToData);
            proj.DefCache = DefinitionsCache.LoadFromProject(proj);
            proj.Toolbox = NodePicker.FromXml(Path.Combine(proj.PathToData, "nodes"), proj);

            return proj;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    #endregion
    #region Definitions

    public async Task<bool> OpenDefinitionFile(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        MainWindow.IsOpeningFile = true;
        LunaDefinition newDef = await LunaDefinition.CreateFromFile(this, filePath);
        newDef.AllocHash(ref ProjectFileMaxHash);
        ProjectFiles.Add(newDef);
        MainWindow.IsOpeningFile = false;

        return true;
    }

    #endregion
    #region Scripts

    public async Task<bool> OpenScriptFile(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        MainWindow.IsOpeningFile = true;
        LunaScript newScript = await LunaScript.CreateFromFile(this, filePath);
        newScript.AllocHash(ref ProjectFileMaxHash);
        ProjectFiles.Add(newScript);
        MainWindow.IsOpeningFile = false;

        return true;
    }

    #endregion
    #region Shaders

    public async Task<bool> OpenShaderFile(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        MainWindow.IsOpeningFile = true;
        LunaShader newShader = await LunaShader.CreateFromFile(this, filePath);
        newShader.AllocHash(ref ProjectFileMaxHash);
        ProjectFiles.Add(newShader);
        MainWindow.IsOpeningFile = false;

        return true;
    }

    #endregion
    #region Traces

    public void RemoveTraces()
    {
        EditorTraceContainer.RemoveChecksFromSource(this);
    }

    public virtual List<EditorTrace> GetTraces()
    {
        List<EditorTrace> traces = [];
        if (!(File.Exists(PathToLuaSTGExecutable) && PathToLuaSTGExecutable.EndsWith(".exe")))
            traces.Add(new ProjectPathNotNullTrace(this, "LuaSTG Executable"));
        if (!(File.Exists(EntryPoint) && (EntryPoint.EndsWith(".lua") || EntryPoint.EndsWith(".lfd"))))
            traces.Add(new ProjectPathNotNullTrace(this, "Project Entry Point"));
        return traces;
    }

    public void CheckTrace()
    {
        List<EditorTrace> traces = GetTraces();
        Traces.Clear();

        foreach (EditorTrace trace in traces)
            Traces.Add(trace);
        EditorTraceContainer.UpdateTraces(this);
    }

    #endregion
    #region Discord

    public void UpdateRPC()
    {
        MainWindow.Discord?.SetPresence(new RichPresence()
        {
            Details = ProjectName,
            State = CurrentProjectFile?.FileName ?? "No File Open",
            Timestamps = new Timestamps(StartedTimestamp),
            Assets = new Assets()
            {
                LargeImageKey = "lunaforgeicon",
                LargeImageText = ProjectName,
                SmallImageKey = "fileicon",
                SmallImageText = ProjectName,
            }
        });
    }

    #endregion
    #region Events

    [YamlIgnore]
    public UpdateSelectedProjectEventHandler OnUpdateSelectedProject;
    [YamlIgnore]
    public UnloadProjectEventHandler OnUnloadProject;

    public void RaiseUpdateSelected()
    {
        OnUpdateSelectedProject?.Invoke();
    }

    public void RaiseUnloaded()
    {
        OnUnloadProject?.Invoke();
    }

    #endregion
}
