using LunaForge.EditorData.Commands;
using LunaForge.EditorData.Traces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Project;

public abstract class LunaProjectFile : ITraceThrowable
{
    public LunaForgeProject ParentProject { get; set; }

    public int Hash { get; set; } = -1;
    public string FullFilePath { get; set; }
    public string FileName { get; set; }

    public bool IsOpened = true;
    public bool ForceClose = false; // Only set to true when the file is open while the editor is on the closing process or if bulk closing.

    public Stack<Command> CommandStack { get; set; } = [];
    public Stack<Command> UndoCommandStack { get; set; } = [];
    public Command? SavedCommand { get; set; } = null;

    public virtual bool IsUnsaved
    {
        get
        {
            if (CommandStack.Count > 0)
                return CommandStack.Peek() != SavedCommand;
            else
                return SavedCommand != null;
        }
    }

    public List<EditorTrace> Traces { get; private set; } = [];

    public LunaProjectFile(LunaForgeProject parentProj, string path)
    {
        ParentProject = parentProj;
        FullFilePath = path;
        FileName = Path.GetFileName(path);
    }

    public override string ToString() => FileName;

    public void AllocHash(ref int maxHash)
    {
        if (Hash != -1)
            return; // Hash has already been allocated. Return immediately. Not actually needed, it's more of a satefy measure.
        Hash = maxHash;
        maxHash++;
    }

    #region Commands

    public void Undo()
    {
        CommandStack.Peek().Undo();
        UndoCommandStack.Push(CommandStack.Pop());
    }

    public void Redo()
    {
        UndoCommandStack.Peek().Execute();
        CommandStack.Push(UndoCommandStack.Pop());
    }

    public void PushSavedCommand()
    {
        try { SavedCommand = CommandStack.Peek(); }
        catch (InvalidOperationException) { SavedCommand = null; }
    }

    public bool AddAndExecuteCommand(Command command)
    {
        if (command == null)
            return false;
        CommandStack.Push(command);
        CommandStack.Peek().Execute();
        UndoCommandStack = [];
        return true;
    }

    public void RevertUntilSaved()
    {
        if (SavedCommand == null || CommandStack.Contains(SavedCommand))
            while (CommandStack.Count != 0 && CommandStack.Peek() != SavedCommand)
                Undo();
        else
            while (UndoCommandStack.Count != 0 && UndoCommandStack.Peek() != SavedCommand)
                Redo();
    }

    #endregion
    #region IO

    public abstract Task Save(bool saveAs = false);

    #endregion
    #region Traces

    public void CheckTrace()
    {
        throw new NotImplementedException();
    }

    public List<EditorTrace> GetTraces()
    {
        throw new NotImplementedException();
    }

    #endregion
    #region Abstract Impl

    public abstract void Delete();
    public abstract bool Delete_CanExecute();

    public abstract void Render();

    public abstract void Close();

    #endregion
}
