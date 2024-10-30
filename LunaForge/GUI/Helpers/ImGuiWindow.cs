using ImGuiNET;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Helpers;

public abstract class ImGuiWindow(bool showByDefault)
{
    public bool ShowWindow = showByDefault;

    public bool Begin(string windowName, Vector2? windowSize = null)
    {
        if (!ShowWindow)
            return false;
        if (windowSize == null)
            windowSize = new Vector2(200, 100);

        ImGui.SetNextWindowSize((Vector2)windowSize, ImGuiCond.FirstUseEver);
        if (ImGui.Begin(windowName, ref ShowWindow))
        {
            return true;
        }
        return false;
    }

    public bool BeginNoClose(string windowName, Vector2? windowSize = null, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
    {
        if (!ShowWindow)
            return false;
        if (windowSize == null)
            windowSize = new Vector2(200, 100);

        ImGui.SetNextWindowSize((Vector2)windowSize, ImGuiCond.FirstUseEver);
        if (ImGui.Begin(windowName, flags))
        {
            return true;
        }
        return false;
    }

    public bool BeginFlags(string windowName, ImGuiWindowFlags flags, Vector2? windowSize = null)
    {
        if (!ShowWindow)
            return false;
        if (windowSize == null)
            windowSize = new Vector2(200, 100);

        ImGui.SetNextWindowSize((Vector2)windowSize, ImGuiCond.FirstUseEver);
        if (ImGui.Begin(windowName, ref ShowWindow, flags))
        {
            return true;
        }
        return false;
    }

    public abstract void Render();

    public void End()
    {
        ImGui.End();
    }

    public static bool BeginDisabledButton(string label, bool isEnabled)
    {
        if (!isEnabled)
            ImGui.BeginDisabled();

        return ImGui.Button(label);
    }

    public static void EndDisabledButton(bool isEnabled)
    {
        if (!isEnabled)
            ImGui.EndDisabled();
    }

    public static void VerticalSeparator(float size = 20.0f)
    {
        ImGui.SameLine(0, size);
    }

    protected void SetModalToCenter(Vector2 modalSize)
    {
        Vector2 renderSize = new(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
        ImGui.SetNextWindowSize(modalSize);
        ImGui.SetNextWindowPos(renderSize / 2 - (modalSize / 2));
    }
}
