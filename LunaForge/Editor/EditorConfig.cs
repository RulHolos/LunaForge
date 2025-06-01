using LunaForge.Editor.Backend.Utilities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LunaForge.Editor;

public class EditorConfigEntry<T>
{
    public string Key { get; }
    public T Value { get; set; }
    public T? TempValue { get; set; } // Used in settings UI only

    public EditorConfigEntry(string key, T defaultValue)
    {
        Key = key;
        Value = defaultValue;
        TempValue = defaultValue;
    }

    public void Commit() => Value = TempValue!;

    public void Revert() => TempValue = Value;
}

public class EditorConfig
{
    private const string configFile = "config.json";
    private static readonly string configPath = Path.Combine(GetConfigPath(), configFile);
    public static readonly string BasePath = GetConfigPath();

    private Dictionary<string, object> entries = [];
    public IEnumerable<KeyValuePair<string, object>> AllEntries => entries.OrderBy(e => e.Key);

    private static readonly ILogger Logger = CoreLogger.Create("EditorConfig");

    public static EditorConfig Default { get; } = Load();

    public void Register<T>(string key, T defaultValue)
    {
        if (!entries.ContainsKey(key))
            entries[key] = new EditorConfigEntry<T>(key, defaultValue);
    }

    public EditorConfigEntry<T> Get<T>(string key)
    {
        if (entries.TryGetValue(key, out var obj) && obj is EditorConfigEntry<T> entry)
            return entry;

        Logger.Warning($"Config entry '{key}' not found or type mismatch.");
        return new EditorConfigEntry<T>(key, default!);
    }

    public void SetOrCreate<T>(string key, T value)
    {
        if (!entries.ContainsKey(key))
            Register(key, value);

        Get<T>(key).TempValue = value;
    }

    public void CommitAll()
    {
        foreach (var entry in entries.Values)
        {
            var commitMethod = entry.GetType().GetMethod("Commit");
            commitMethod?.Invoke(entry, null);
        }
    }

    public void RevertAll()
    {
        foreach (var entry in entries.Values)
        {
            var revertMethod = entry.GetType().GetMethod("Revert");
            revertMethod?.Invoke(entry, null);
        }
    }

    public void Save()
    {
        var data = entries.ToDictionary(
            e => e.Key,
            e => e.Value.GetType().GetProperty("Value")?.GetValue(e.Value)
        );

        File.WriteAllText(configPath, JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public static EditorConfig Load()
    {
        EditorConfig config = new();

        if (File.Exists(configPath))
        {
            try
            {
                Dictionary<string, object> rawData = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(configPath));

                foreach (var (key, value) in rawData)
                {
                    var type = typeof(EditorConfigEntry<>).MakeGenericType(value.GetType());
                    config.entries[key] = Activator.CreateInstance(type, key, value);
                    /*if (value is bool b)
                        config.entries[key] = new EditorConfigEntry<bool>(key, b);
                    else if (value is string s)
                        config.entries[key] = new EditorConfigEntry<string>(key, s);
                    else if (value != null)
                        config.entries[key] = new EditorConfigEntry<string>(key, value.ToString() ?? "");*/
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load config: {ex}");
            }
        }

        config.Register("SetupDone", false);
        config.Register("ProjectsFolder", string.Empty);
        config.Register("SelectedLayout", string.Empty);
        config.Register("ProjectAuthor", "John Dough");

        foreach (var entry in config.AllEntries)
        {
            Logger.Debug($"Key: {entry.Key}, Value: {entry.Value}");
        }

        config.CommitAll();
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
