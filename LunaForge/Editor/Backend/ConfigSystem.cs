using LunaForge.Editor.Backend.Utilities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;
using YamlDotNet.Core.Tokens;

namespace LunaForge.Editor;

public enum ConfigSystemCategory
{
    General,
    DefaultProject,
    CurrentProject
}

public interface IConfigSystemEntry
{
    public ConfigSystemCategory Category { get; }
    string Key { get; }
    object TempValueObj { get; set; }

    public void Commit();
    public void Revert();
}

public class ConfigSystemEntry<T> : IConfigSystemEntry
{
    [IgnoreDataMember]
    public ConfigSystemCategory Category { get; }
    public string Key { get; }
    public T Value { get; set; }
    [IgnoreDataMember]
    public T? TempValue { get; set; }

    public object TempValueObj
    {
        get => TempValue;
        set
        {
            if (value is T tempValue)
                TempValue = tempValue;
            else
                throw new InvalidCastException($"Cannot cast {value.GetType()} to {typeof(T)} for key '{Key}'.");
        }
    }

    public ConfigSystemEntry() { }

    public ConfigSystemEntry(ConfigSystemCategory category, string key, T defaultValue)
    {
        Category = category;
        Key = key;
        Value = defaultValue;
        TempValue = defaultValue;
    }

    public void Commit() => Value = TempValue!;

    public void Revert() => TempValue = Value;
    
    public E GetEnum<E>() where E: struct, Enum
    {
        if (Value == null)
            return default;

        return Enum.Parse<E>(TempValue.ToString());
    }

    public object GetEnum([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    {
        if (TempValue == null)
            return Activator.CreateInstance(type);

        return Enum.Parse(type, TempValue.ToString());
    }
}

public class ConfigSystem
{
    public string configPath { get; set; }

    private Dictionary<string, IConfigSystemEntry> entries { get; set; } = [];
    public IEnumerable<KeyValuePair<string, IConfigSystemEntry>> AllEntries => entries.OrderBy(e => e.Key);

    private static readonly ILogger Logger = CoreLogger.Create("ConfigSystem");

    /// <summary>
    /// Registers a new config option with the specified category, key, and default value. Skips registration if the key already exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="category"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    public void Register<T>(ConfigSystemCategory category, string key, T defaultValue)
    {
        if (!entries.ContainsKey(key))
            entries[key] = new ConfigSystemEntry<T>(category, key, defaultValue);
    }

    public ConfigSystemEntry<T> Get<T>(string key, ConfigSystemCategory category = ConfigSystemCategory.General)
    {
        if (entries.TryGetValue(key, out var obj) && obj is ConfigSystemEntry<T> entry)
            return entry;

        Logger.Warning($"Config entry '{key}' not found or type mismatch.");
        return new ConfigSystemEntry<T>(category, key, default!);
    }

    public void SetOrCreate<T>(string key, T value, ConfigSystemCategory category = ConfigSystemCategory.General)
    {
        if (!entries.ContainsKey(key))
            Register(category, key, value);

        Get<T>(key).TempValue = value;
    }

    public void Set<T>(string key, T value)
    {
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

    public void CommitAllAndSave()
    {
        CommitAll();
        Save();
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
        Save(configPath);
    }

    public void Save(string filePath)
    {
        var model = new TomlTable();

        foreach (var entry in entries.Values)
        {
            dynamic dyn = entry;
            string section = dyn.Category.ToString();

            if (!model.ContainsKey(section))
                model[section] = new TomlTable();

            ((TomlTable)model[section])[dyn.Key] = dyn.Value;
        }

        File.WriteAllText(filePath, Toml.FromModel(model));
    }

    public static T Load<T>(string configPath) where T : ConfigSystem, new()
    {
        T config = new()
        {
            configPath = configPath
        };

        if (File.Exists(configPath))
        {
            try
            {
                var table = Toml.Parse(File.ReadAllText(configPath)).ToModel();

                foreach (var (sectionKey, sectionValue) in table)
                {
                    if (sectionValue is not TomlTable section)
                        continue;

                    if (!Enum.TryParse(sectionKey, out ConfigSystemCategory category))
                    {
                        Logger.Warning($"Unknown config section '{sectionKey}' in {configPath}. Skipping.");
                        continue;
                    }

                    foreach (var (key, val) in section)
                    {
                        var type = typeof(ConfigSystemEntry<>).MakeGenericType(val.GetType());
                        config.entries[key] = (IConfigSystemEntry)Activator.CreateInstance(type, category, key, val);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load config. Reason:\n{ex}");
            }
        }

        if (config is EditorConfig)
        {
            config.Register(ConfigSystemCategory.General, "SetupDone", false);
            config.Register(ConfigSystemCategory.General, "ProjectsFolder", string.Empty);
            config.Register(ConfigSystemCategory.General, "SelectedLayout", string.Empty);
            config.Register(ConfigSystemCategory.DefaultProject, "ProjectAuthor", "John Dough");

            config.CommitAll();
            config.Save();
        }

        return config;
    }
}
