using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.UI.Dialogs;
using LunaForge.Editor.UI.Managers;
using Newtonsoft.Json;
using Serilog;

namespace LunaForge.Editor.Projects;

[Serializable]
public abstract class LunaProjectFile : IDisposable
{
    [JsonIgnore] private static ILogger Logger;

    [JsonIgnore] protected string? FilePath;
    [JsonIgnore] public int Hash;

    [JsonIgnore] public bool IsUnsaved;

    public LunaProjectFile()
    {
        Logger = CoreLogger.Create(GetType().Name);
    }

    public override string ToString()
    {
        return Path.GetFileName(FilePath) ?? "Unnamed";
    }

    public string GetUniqueName() => ToString() + $"##{Hash}";

    public static T CreateNew<T>() where T : LunaProjectFile, new()
    {
        T projectFile = new();

        return projectFile;
    }

    /// <summary>
    /// Loads an already existing file into the project.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath">Path to the definition.</param>
    /// <returns>A new <typeparamref name="T"/> instance based of a saved file.</returns>
    public static T Load<T>(string filePath) where T : LunaProjectFile, new()
    {
        if (!File.Exists(filePath))
        {
            CoreLogger.Logger.Error($"Project File '{filePath}' doesn't exist. Can't load.");
            return default;
        }

        T projFile = JsonConvert.DeserializeObject<T>(filePath) ?? new T();
        projFile.FilePath = filePath;
        return projFile;
    }

    public void Save(bool saveAs = false)
    {
        string parsedJson = JsonConvert.SerializeObject(this);

        void writeToFile(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not SaveFileDialog dialog)
                return;
            using FileStream fs = new(dialog.SelectedFile, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            sw.Write(parsedJson);
        }
        
        if (!File.Exists(FilePath) || saveAs)
        {
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            sw.Write(parsedJson);
        }
        else
        {
            SaveFileDialog sfd = new(FilePath) { OnlyAllowFilteredExtensions = true };
            sfd.AllowedExtensions.Add(".lfd");
            sfd.Show(writeToFile);
        }
    }

    public abstract void Dispose();

    public abstract void Draw();
}
