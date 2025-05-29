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
