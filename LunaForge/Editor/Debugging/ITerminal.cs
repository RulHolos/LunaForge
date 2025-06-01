using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Debugging;

public interface ITerminal
{
    bool Shown { get; }

    void Draw();

    void Focus();

    void Close();

    void Show();
}
