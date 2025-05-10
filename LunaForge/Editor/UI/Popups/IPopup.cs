using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Popups;

public interface IPopup
{
    string Name { get; }
    bool Shown { get; }

    void Close();
    void Draw();
    void Reset();
    void Show();
}

