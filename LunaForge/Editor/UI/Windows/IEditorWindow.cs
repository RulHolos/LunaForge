using Hexa.NET.ImGui;
using LunaForge.Editor.Commands;
using LunaForge.Editor.UI.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows;

public interface IEditorWindow
{
    bool Initialized { get; }
    bool IsShown { get; }
    string Name { get; }

    event Action<IEditorWindow>? Shown;
    event Action<IEditorWindow>? Closed;

    public CommandHistory History { get; set; }

    void Init();
    void DrawWindow();
    void DrawMenu();
    void DrawContent();
    void Focus();
    void Dispose();
}

public abstract class EditorWindow : IEditorWindow
{
    protected bool IsDocked;
    public virtual ImGuiWindowFlags Flags { get; set; }
    protected bool initialized;
    protected bool isShown;
    public virtual bool CanBeClosed { get; set; } = true;

    protected abstract string Name { get; }

    string IEditorWindow.Name => Name;

    public virtual bool IsShown { get => isShown; protected set => isShown = value; }

    public bool Initialized => initialized;

    public event Action<IEditorWindow>? Shown;
    public event Action<IEditorWindow>? Closed;

    public CommandHistory History { get; set; } = new();

    public void Init()
    {
        InitWindow();
        initialized = true;
    }

    protected virtual void InitWindow()
    {
    }

    public virtual void DrawWindow()
    {
        bool hasBegun;
        if (!CanBeClosed)
            hasBegun = ImGui.Begin(Name, Flags);
        else
            hasBegun = ImGui.Begin(Name, ref isShown, Flags);
        if (!hasBegun)
        {
            ImGui.End();
            return;
        }

        DoUndoRedo();
        DrawContent();

        ImGui.End();
    }

    /// <summary>
    /// Rework this, not only the window should be able to handle undo/redo, but also <see cref="LunaProjectFile"/>s.
    /// </summary>
    private void DoUndoRedo()
    {
        // Only undo/redo if the current window is focused. Avoids conflicts with other windows.
        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
        {
            WindowManager.CurrentFocusedWindow = this;
        }
    }

    public abstract void DrawContent();

    public virtual void DrawMenu()
    {
        if (ImGui.MenuItem(Name))
        {
            IsShown = true;
        }
    }

    public void Dispose()
    {
        DisposeCore();
        initialized = false;
    }

    protected virtual void DisposeCore()
    {
    }

    public void Focus()
    {
        ImGuiWindowPtr window = ImGuiP.FindWindowByName(Name);
        ImGuiP.FocusWindow(window, ImGuiFocusRequestFlags.UnlessBelowModal);
    }
}