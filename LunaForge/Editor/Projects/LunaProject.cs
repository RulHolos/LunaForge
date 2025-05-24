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

    public List<string> openedFiles = [];

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
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(this);
            using StreamWriter sw = new(ProjectFile);
            sw.WriteLine(yaml);

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

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            using StreamReader sr = new(path);
            LunaProject proj = deserializer.Deserialize<LunaProject>(sr);
            proj.ProjectRoot = Path.GetDirectoryName(path);

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
