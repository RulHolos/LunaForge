using LunaForge.GUI;
using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Plugins.Services;

public interface IWindowService
{
    public T GetWindow<T>(string windowId) where T : new();
}

internal class WindowService : IWindowService
{
    public T GetWindow<T>(string windowId) where T : new() => MainWindow.GetWindow<T>(windowId);
}
