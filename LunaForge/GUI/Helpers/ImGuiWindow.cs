﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Helpers;

public abstract class ImGuiWindow(MainWindow parentWin, bool showByDefault)
{
    public MainWindow ParentWindow = parentWin;

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

    public bool BeginNoClose(string windowName, Vector2? windowSize = null)
    {
        if (!ShowWindow)
            return false;
        if (windowSize == null)
            windowSize = new Vector2(200, 100);

        ImGui.SetNextWindowSize((Vector2)windowSize, ImGuiCond.FirstUseEver);
        if (ImGui.Begin(windowName))
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
}