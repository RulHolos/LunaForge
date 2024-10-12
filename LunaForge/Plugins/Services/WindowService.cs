using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Plugins.Services;

public interface IWindowService
{
    public void RegisterWindow(ImGuiWindow window);
}

internal class WindowService : IWindowService
{
    public void RegisterWindow(ImGuiWindow window)
    {

    }
}
