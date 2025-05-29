using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaForge.Editor.UI.Windows;

namespace LunaForge.Editor.Commands;

/// <summary>
/// Each CommandHistory is stored in an <see cref="IEditorWindow"/> instance.
/// </summary>
public sealed class CommandHistory
{
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
}
