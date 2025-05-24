using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor;

public class EditorConfig
{
    private const string configFile = "config.json";
    private static readonly string configPath = Path.Combine(GetConfigPath(), configFile);
    public static readonly string BasePath = GetConfigPath();

    #region Config

    public bool SetupDone { get; set; } = false;

    public string ProjectsFolder { get; set; } = null!;

    public string? SelectedLayout { get; set; }

    public string ProjectAuthor { get; set; } = "John Dough";

    #endregion

    public static EditorConfig Default { get; } = Load();

    public void Save()
    {
        File.WriteAllText(configPath, JsonConvert.SerializeObject(this));
    }

    public static EditorConfig Load()
    {
        EditorConfig config;
        if (File.Exists(configPath))
            config = JsonConvert.DeserializeObject<EditorConfig>(File.ReadAllText(configPath)) ?? new();
        else
            config = new();

        config.Save();
        return config;
    }

    private static string GetConfigPath()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LunaForge");

        Directory.CreateDirectory(path);
        return path;
    }
}
