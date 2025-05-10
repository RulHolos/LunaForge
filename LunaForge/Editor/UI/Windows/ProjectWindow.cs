using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows;

public class ProjectWindow : EditorWindow
{
    protected override string Name => "Project";
    public override bool IsShown { get; protected set; } = true;
    public override ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.NoCollapse;
    public override bool CanBeClosed { get; set; } = false;

    protected override void InitWindow()
    {
        base.InitWindow();
    }

    public override void DrawContent()
    {
        if (ImGui.BeginTabBar("##ProjectTabBar", ImGuiTabBarFlags.Reorderable))
        {
            if (ImGui.BeginTabItem("Tab 1"))
                ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Tab 2"))
                ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Tab 3"))
                ImGui.EndTabItem();

            ImGui.EndTabBar();
        }
    }
}
