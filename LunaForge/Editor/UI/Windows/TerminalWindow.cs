using Hexa.NET.ImGui;
using LunaForge.Editor.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Windows;

public class TerminalWindow : EditorWindow
{
    public override bool IsShown { get; protected set; } = true;

    private readonly List<Tuple<string, ITerminal>> terminals = [];

    public TerminalWindow()
    {
        Flags = ImGuiWindowFlags.MenuBar;

        terminals.Add(new Tuple<string, ITerminal>("Output", new OutputTerminal()));
    }

    protected override string Name => "Terminal";

    public override void DrawContent()
    {
        ImGui.BeginTabBar("Terminals", ImGuiTabBarFlags.Reorderable);

        for (int i = 0; i < terminals.Count; i++)
        {
            var terminal = terminals[i].Item2;
            var name = terminals[i].Item1;
            var open = true;

            if (ImGui.BeginTabItem(name, ref open))
            {
                terminal.Draw();
                ImGui.EndTabItem();
            }
        }
        ImGui.EndTabBar();
    }
}
