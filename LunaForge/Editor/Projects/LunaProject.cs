using Lua;
using Lua.Standard;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.LunaTreeNodes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LunaForge.Editor.Projects;

public class LunaProject : IDisposable
{

    private static ILogger Logger = CoreLogger.Create("LunaProject");

    [YamlIgnore] public string ProjectRoot { get; private set; }
    [YamlIgnore] public string ProjectFile => Path.Combine(ProjectRoot, "Project.lfp");
    [YamlIgnore] public string DotFolder => Path.Combine(ProjectRoot, ".lunaforge");

    [YamlIgnore] public ProjectFileCollection ProjectFileCollection { get; private set; } = [];

    [YamlIgnore] public List<LuaState> LuaStates { get; private set; } = [];

    [YamlIgnore] public List<NodeBox> NodeBoxes { get; private set; } = [];

    #region Project Config

    public ConfigSystem ProjectConfig { get; private set; } = new();

    #endregion

    public LunaProject()
    {
        
    }

    public LunaProject(string folder)
    {
        ProjectRoot = folder;

        FindAllNodeBoxes();
    }

    public static LunaProject CreateEmpty() => new();

    public static LunaProject CreateNew(string folder)
    {
        LunaProject project = new(folder);

        project.Save();

        return project;
    }

    #region Lua

    private async Task FindAllNodeBoxes()
    {
        Logger.Verbose("Reading all NodeBoxes...");

        string[] allfiles = Directory.GetFiles(Path.Combine(DotFolder, "nodes"), "init.lua", SearchOption.AllDirectories);
        Logger.Verbose($"NodeBoxes found: {allfiles.Length}");
        Logger.Verbose($"Setting up Lua Environment...");

        foreach (string file in allfiles)
        {
            try
            {
                string dir = Path.GetDirectoryName(file);
                LuaState state = LuaState.Create();
                state.ModuleLoader = new LunaModuleLoader(dir);
                SetSharedTypes(ref state);

                LuaValue[] results = await state.DoFileAsync(Path.Combine(DotFolder, dir, "init.lua"));
                var fdfsdf = results[0].Read<NodeBox>();

                LuaStates.Add(state);
            }
            catch (LuaRuntimeException ex)
            {
                Logger.Error($"""
                    A lua runtime exception as occured!
                    {ex.Message}
                    <=== Traceback ===>
                    {ex.LuaTraceback}
                    """);
            }
        }
    }

    private void SetSharedTypes(ref LuaState state)
    {
        state.OpenBasicLibrary();
        state.OpenBitwiseLibrary();
        state.OpenStringLibrary();
        state.OpenTableLibrary();
        state.OpenMathLibrary();
        state.OpenModuleLibrary();

        state.Environment["NodeBox"] = new NodeBox();
        state.Environment["TreeNode"] = new TreeNode();
    }

    #endregion

    #region Serialization

    public bool Save()
    {
        try
        {
            ProjectConfig.Save(ProjectFile);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save project. Reason:\n{ex}");
            return false;
        }
    }

    public static (LunaProject, string) Load(string path)
    {
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Project file doesn't exist: {path}");

            LunaProject proj = new(Path.GetDirectoryName(path))
            {
                ProjectConfig = ConfigSystem.Load<ConfigSystem>(path)
            };

            proj.ProjectConfig.Register<TomlArray>(ConfigSystemCategory.CurrentProject, "OpenedFiles", []);
            proj.ProjectConfig.CommitAll();
            proj.ProjectConfig.Save();

            // Fix this
            foreach (string filePath in proj.ProjectConfig.Get<TomlArray>("OpenedFiles", ConfigSystemCategory.CurrentProject).Value)
            {
                LunaProjectFile? file = null;
                switch (Path.GetExtension(path))
                {
                    case ".lfd":
                        file = LunaNodeTree.Load(path);
                        break;
                    case ".lfg":
                        file = LunaProjectFile.Load<LunaNodeGraph>(path);
                        break;
                    case ".lua":
                        file = LunaProjectFile.Load<LunaScriptEditor>(path);
                        break;
                }
                if (file != null)
                    proj.ProjectFileCollection.Add(file);
            }

            return (proj, "");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load project. Reason:\e{ex}");
            return (null, ex.ToString());
        }
    }

    #endregion

    public void Dispose()
    {
        TomlArray openedFiles = [.. ProjectFileCollection.Select(x => x.FilePath)];
        ProjectConfig.SetOrCreate("OpenedFiles", openedFiles, ConfigSystemCategory.CurrentProject);
        ProjectConfig.CommitAll();
        ProjectConfig.Save();

        foreach (LunaProjectFile file in ProjectFileCollection)
            file.Dispose();
    }
}

public class LunaModuleLoader(string folder) : ILuaModuleLoader
{
    /*
     * En fait il faudrait que je compte le nombre de nodeboxes à load, et ensuite que je construise un lua state par nodebox.
     * Là, actuellement c'est un par project, ce qui va pas, parce que le lua state sera global et ça me fait chier.
     */

    private readonly string folder = folder;

    public bool Exists(string moduleName)
    {
        return File.Exists(Path.Combine(folder, moduleName));
    }

    public async ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        string text = moduleName;
        if (!Path.HasExtension(text))
            text += ".lua";

        return new LuaModule(moduleName, await File.ReadAllTextAsync(Path.Combine(folder, text), cancellationToken));
    }
}