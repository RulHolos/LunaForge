using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Commands;

public abstract class Command
{
    public abstract void Execute();
    public abstract void Undo();
    public abstract new string ToString();
}

public static class History
{
    public static Stack<Command> CommandStack { get; set; } = [];
    public static Stack<Command> UndoCommandStack { get; set; } = [];

    public static bool CanUndo => CommandStack.Count > 0;
    public static bool CanRedo => UndoCommandStack.Count > 0;

    public static int UndoCount = CommandStack.Count;
    public static int RedoCount = UndoCommandStack.Count;

    public static void Undo()
    {
        CommandStack.Peek().Undo();
        UndoCommandStack.Push(CommandStack.Pop());
    }
    public static void Redo()
    {
        UndoCommandStack.Peek().Execute();
        CommandStack.Push(UndoCommandStack.Pop());
    }

    public static bool AddAndExecuteCommand(Command command)
    {
        if (command == null)
            return false;

        CommandStack.Push(command);
        CommandStack.Peek().Execute();
        UndoCommandStack.Clear();

        return true;
    }
}
