using ImGuiNET;
using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Windows;

public class AboutWindow : ImGuiWindow
{
    public AboutWindow()
        : base(false)
    {

    }

    public override void Render()
    {
        if (BeginFlags("About LunaForge", ImGuiWindowFlags.NoDocking, new Vector2(800, 450)))
        {
            ImGui.TextLinkOpenURL($"{MainWindow.LunaForgeName} v{MainWindow.VersionNumber}", "https://github.com/RulHolos/LunaForge");
            ImGui.Text($"By Rül Hölos.");

            End();
        }
    }
}

