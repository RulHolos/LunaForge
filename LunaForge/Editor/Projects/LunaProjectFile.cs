using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.Commands;
using LunaForge.Editor.UI;
using LunaForge.Editor.UI.Dialogs;
using LunaForge.Editor.UI.Managers;
using Newtonsoft.Json;
using Serilog;

namespace LunaForge.Editor.Projects;

[Serializable]
public abstract class LunaProjectFile : IDisposable
{
    [JsonIgnore] private static ILogger Logger;

    [JsonIgnore] public string? FilePath;
    [JsonIgnore] public int Hash;

    [JsonIgnore] public bool IsUnsaved;

    [JsonIgnore] public bool IsOpened;

    public LunaProjectFile()
    {
        Logger = CoreLogger.Create(GetType().Name);
    }

    public override string ToString()
    {
        return Path.GetFileName(FilePath) ?? "Unnamed";
    }

    public string GetUniqueName() => ToString() + $"##{Hash}";

    public static T CreateNew<T>(string name = "Unnamed") where T : LunaProjectFile, new()
    {
        T projectFile = new()
        {
            FilePath = name,
            IsUnsaved = true,
            IsOpened = true,
        };
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
        projFile.IsOpened = true;
        return projFile;
    }

    public void Save(bool saveAs = false)
    {
        string parsedJson = JsonConvert.SerializeObject(this);

        void writeToFile(bool success, string path)
        {
            if (!success)
                return;
            using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            sw.Write(parsedJson);
        }
        
        if (!File.Exists(FilePath) || saveAs)
        {
            MainWindow.FileDialogManager.SaveFileDialog("Save Project File", "LunaForge Definition{.lfd,.lfg,.lua}",
                FilePath, ".lfd", writeToFile, EditorConfig.Default.Get<string>("ProjectsFolder").Value);
        }
        else
        {
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            sw.Write(parsedJson);
        }
    }

    public virtual void Dispose()
    {
        IsOpened = false;
    }

    public abstract void Draw();

    #region CommandHistory

    public Stack<Command> CommandStack { get; private set; } = [];
    public Stack<Command> UndoCommandStack { get; private set; } = [];

    public bool CanUndo => CommandStack.Count > 0;
    public bool CanRedo => UndoCommandStack.Count > 0;

    public int UndoCount => CommandStack.Count;
    public int RedoCount => UndoCommandStack.Count;

    public void Undo()
    {
        if (!CanUndo) return;
        CommandStack.Peek().Undo();
        UndoCommandStack.Push(CommandStack.Pop());
    }
    public void Redo()
    {
        if (!CanRedo) return;
        UndoCommandStack.Peek().Execute();
        CommandStack.Push(UndoCommandStack.Pop());
    }

    public bool AddAndExecuteCommand(Command command)
    {
        if (command == null)
            return false;

        CommandStack.Push(command);
        CommandStack.Peek().Execute();
        UndoCommandStack.Clear();

        return true;
    }

    public static bool AddAndExecuteCommandStatic(Command command)
    {
        if (command == null || ProjectFileCollection.CurrentF != null)
            return false;

        return ProjectFileCollection.CurrentF.AddAndExecuteCommand(command);
    }

    #endregion
}
