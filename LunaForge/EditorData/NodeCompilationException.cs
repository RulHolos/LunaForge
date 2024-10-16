using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData;

public class NodeCompilationException : Exception
{
    public NodeCompilationException() { }

    public NodeCompilationException(string message) { }

    public NodeCompilationException(string message, Exception inner)
        : base(message, inner) { }
}
