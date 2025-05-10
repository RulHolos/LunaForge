using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Popups;

public class NewProjWindow : Modal
{
    private bool first = true;

    public override string Name { get; } = "New Project";

    protected override ImGuiWindowFlags Flags { get; } =
        ImGuiWindowFlags.NoSavedSettings
        //ImGuiWindowFlags.MenuBar
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoDocking
        //| ImGuiWindowFlags.NoTitleBar
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
        Vector2 avail = ImGui.GetContentRegionAvail();
        const float footerHeight = 50;
        avail.Y -= footerHeight;
        ImGui.BeginChild("Content", avail);

        ImGui.EndChild();

        ImGui.BeginTable("#Table", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("");

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(1);

        if (ImGui.Button("Cancel"))
        {
            Close();
        }
        ImGui.SameLine();
        if (ImGui.Button("Create"))
        {

        }

        ImGui.EndTable();
    }

    public override void Reset()
    {
        
    }
}
