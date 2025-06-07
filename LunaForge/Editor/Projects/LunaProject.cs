using LunaForge.Editor.Backend.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LunaForge.Editor.Projects;

public class LunaProject
{

    private static ILogger Logger = CoreLogger.Create("LunaProject");

    [YamlIgnore] public string ProjectRoot { get; private set; }
    [YamlIgnore] public string ProjectFile => Path.Combine(ProjectRoot, "Project.lfp");
    [YamlIgnore] public string DotFolder => Path.Combine(ProjectRoot, ".lunaforge");

    [YamlIgnore] public ProjectFileCollection ProjectFileCollection { get; private set; } = [];

    #region Project Config

    public ConfigSystem ProjectConfig { get; private set; } = new();

    #endregion

    public LunaProject()
    {
        
    }

    public static LunaProject CreateEmpty() => new();

    public static LunaProject CreateNew(string folder)
    {
        LunaProject project = new()
        {
            ProjectRoot = folder
        };

        project.Save();

        return project;
    }

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

            LunaProject proj = new()
            {
                ProjectRoot = Path.GetDirectoryName(path),
                ProjectConfig = ConfigSystem.Load<ConfigSystem>(path)
            };

            proj.ProjectConfig.Register(ConfigSystemCategory.CurrentProject, "OpenedFiles", new List<string>());
            proj.ProjectConfig.CommitAll();
            proj.Save();

            return (proj, "");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load project. Reason:\e{ex}");
            return (null, ex.ToString());
        }
    }

    #endregion
}
