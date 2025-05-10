using Hexa.NET.ImGui;
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

    void Dispose();
    void DrawContent();
    void DrawMenu();
    void DrawWindow();
    void Init();
    void Focus();
}

public abstract class EditorWindow : IEditorWindow
{
    protected bool IsDocked;
    public virtual ImGuiWindowFlags Flags { get; set; }
    protected bool windowEnded;
    protected bool wasShown;
    protected bool initialized;
    protected bool isShown;
    public virtual bool CanBeClosed { get; set; } = true;

    protected abstract string Name { get; }

    string IEditorWindow.Name => Name;

    public virtual bool IsShown { get => isShown; protected set => isShown = value; }

    public bool Initialized => initialized;

    public event Action<IEditorWindow>? Shown;

    public event Action<IEditorWindow>? Closed;

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
        if (!IsShown)
        {
            return;
        }

        bool hasBegun;
        if (!CanBeClosed)
            hasBegun = ImGui.Begin(Name, Flags);
        else
            hasBegun = ImGui.Begin(Name, ref isShown, Flags);
        if (!hasBegun)
        {
            if (wasShown)
            {
                OnClosed();
            }
            wasShown = false;
            ImGui.End();
            return;
        }

        if (!wasShown)
        {
            OnShown();
        }
        wasShown = true;

        windowEnded = false;

        DrawContent();

        if (!windowEnded)
        {
            ImGui.End();
        }
    }

    protected virtual void OnShown()
    {
        Shown?.Invoke(this);
    }

    protected virtual void OnClosed()
    {
        Closed?.Invoke(this);
    }

    public abstract void DrawContent();

    protected void EndWindow()
    {
        if (!IsShown)
        {
            return;
        }

        ImGui.End();
        windowEnded = true;
    }

    public virtual void DrawMenu()
    {
        if (ImGui.MenuItem(Name))
        {
            IsShown = true;
        }
    }

    public virtual void Show()
    {
        IsShown = true;
    }

    public virtual void Close()
    {
        IsShown = false;
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