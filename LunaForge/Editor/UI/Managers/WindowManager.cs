using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using LunaForge.Editor.UI.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Managers;

public enum WindowCategory
{
    General,
    Project,
    Editor,
    Debug,
    Tools,
    Settings
}

public static unsafe class WindowManager
{
    private static readonly List<IEditorWindow> windows = [];

    public static IEditorWindow? CurrentFocusedWindow { get; set; } = null;

    static unsafe WindowManager()
    {
    
    }

    public static IReadOnlyList<IEditorWindow> Windows => windows;
    public static bool BlockInput { get; internal set; }

    public static T? GetWindow<T>() where T : class, IEditorWindow
    {
        for (int i = 0; i < windows.Count; i++)
            if (windows[i] is T t)
                return t;
        return null;
    }

    public static IEditorWindow? GetWindow(string name)
    {
        for (int i = 0; i < windows.Count; i++)
            if (windows[i].Name == name)
                return windows[i];
        return null;
    }

    public static bool TryGetWindow<T>([NotNullWhen(true)] out T? editorWindow) where T : class, IEditorWindow
    {
        editorWindow = GetWindow<T>();
        return editorWindow != null;
    }

    public static bool TryGetWindow(string name, [NotNullWhen(true)] out IEditorWindow? editorWindow)
    {
        editorWindow = GetWindow(name);
        return editorWindow != null;
    }

    public static void RegisterAndShowWindow<T>(WindowCategory category = WindowCategory.General) where T : IEditorWindow, new()
    {
        IEditorWindow window = new T
        {
            Category = category
        };
        window.Shown += Shown;
        window.Closed += Closed;
        if (!window.Initialized)
            window.Init();
        windows.Add(window);
    }

    public static void RegisterWindow<T>(WindowCategory category = WindowCategory.General) where T : IEditorWindow, new()
    {
        IEditorWindow window = new T
        {
            Category = category
        };
        window.Shown += Shown;
        window.Closed += Closed;
        windows.Add(window);
    }

    public static void ShowWindow<T>() where T : class, IEditorWindow
    {
        if (TryGetWindow(out T window))
            window.Show();
    }

    private static void Closed(IEditorWindow window)
    {
        windows.Remove(window);
        window.Dispose();

        if (LayoutManager.SelectedLayout.Settings.ContainsKey($"{window.Name}.IsShown"))
            LayoutManager.SelectedLayout.Settings.Remove($"{window.Name}.IsShown", out _);

        ImGuiIOPtr io = ImGui.GetIO();
        io.WantSaveIniSettings = true;
    }

    private static void Shown(IEditorWindow window)
    {
        if (!window.Initialized)
            window.Init();

        LayoutManager.SelectedLayout.Settings.TryAdd($"{window.Name}.IsShown", true);
        ImGuiIOPtr io = ImGui.GetIO();
        io.WantSaveIniSettings = true;
    }

    public static void Init()
    {
        for (int i = 0; i < windows.Count; i++)
            if (windows[i].IsShown)
                windows[i].Init();
    }

    public static void Draw()
    {
        ImGui.BeginDisabled(BlockInput);

        RefreshStateFromConfig();

        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (!window.IsShown)
                continue;

            window.DrawWindow();
        }

        ImGui.EndDisabled();
    }

    public static void DrawMenu()
    {
        /*for (int i = 0; i < windows.Count; i++)
        {
            windows[i].DrawMenu();
        }*/

        foreach (WindowCategory category in Enum.GetValues<WindowCategory>())
        {
            if (ImGui.BeginMenu(Enum.GetName(category)))
            {
                foreach (IEditorWindow window in windows.Where(w => w.Category == category))
                {
                    if (ImGui.MenuItem($"{window.Name}", string.Empty, window.IsShown))
                    {
                        if (window.IsShown)
                            window.Focus();
                        else
                            window.Show();
                    }
                }

                ImGui.EndMenu();
            }
        }
    }

    public static void RefreshStateFromConfig()
    {
        if (LayoutManager.SelectedLayout == null)
            return;
        foreach (IEditorWindow window in windows)
        {
            if (LayoutManager.SelectedLayout.Settings.TryGetValue($"{window.Name}.IsShown", out object isShown))
            {
                if (Convert.ToBoolean(isShown))
                    window.Show();
            }
        } 
    }

    public static void Dispose()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            window.Shown -= Shown;
            window.Closed -= Closed;
            if (window.Initialized)
                window.Dispose();
        }
        windows.Clear();
    }
}
