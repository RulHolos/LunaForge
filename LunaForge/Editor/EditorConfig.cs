using LunaForge.Editor.Backend.Utilities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace LunaForge.Editor;

public class EditorConfig : ConfigSystem
{
    private const string configFile = "config.toml";
    private static readonly string configPath = Path.Combine(GetConfigPath(), configFile);
    public static readonly string BasePath = GetConfigPath();

    public static EditorConfig Default { get; } = Load<EditorConfig>(configPath);

    private static string GetConfigPath()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LunaForge");

        Directory.CreateDirectory(path);
        return path;
    }
}