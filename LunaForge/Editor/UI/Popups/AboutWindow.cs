using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Popups;

public class AboutWindow : Modal
{
    private bool first = true;

    public override string Name { get; } = "About LunaForge";

    protected override ImGuiWindowFlags Flags { get; } =
        ImGuiWindowFlags.MenuBar
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoDocking
        | ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoMove;

    public override unsafe void Draw()
    {
        Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
        Vector2 main_viewport_size = ImGui.GetMainViewport().Size;

        ImGui.SetNextWindowPos(main_viewport_pos);
        ImGui.SetNextWindowSize(main_viewport_size);
        ImGui.SetNextWindowBgAlpha(0.9f);
        ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
        ImGui.End();

        if (first)
        {
            ImGui.SetNextWindowSize(new(800, 500));
            Vector2 size = new(800, 500);
            Vector2 mainViewportPos = ImGui.GetMainViewport().Pos;

            ImGui.SetNextWindowPos(mainViewportPos + (main_viewport_size / 2 - size / 2));
            first = false;
        }
        base.Draw();
    }

    protected override void DrawContent()
    {
        ImGui.SeparatorText(Name);
        Vector2 avail = ImGui.GetContentRegionAvail();
        const float footerHeight = 50;
        avail.Y -= footerHeight;
        ImGui.BeginChild("Content", avail);

        ImGui.TextWrapped("LunaForge is a successor to LuaSTG Editor Sharp-X. This editor aims to have more modern features and customization options as well as permitting user-defined node behaviours.");
        ImGui.TextWrapped("LunaForge is developed by Rül Hölos.");

        ImGui.Spacing();

        ImGui.TextWrapped($"LunaForge v{MainWindow.VersionNumber} - {DateTime.UtcNow.Year}");

        ImGui.EndChild();

        if (ImGui.Button("Close"))
            Close();
    }

    public override void Reset()
    {
        
    }
}
