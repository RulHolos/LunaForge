using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.ImNodesEditor;

namespace LunaForge.Editor.Projects;

public class LunaNodeGraph : LunaProjectFile
{
    private NodeEditor editor = new();

    public LunaNodeGraph()
        : base()
    {
        editor.Initialize();

        var node1 = editor.CreateNode("Node");
        node1.CreatePin(editor, "Nique", PinKind.Input, PinType.Integer, ImNodesPinShape.Circle);
        var out1 = node1.CreatePin(editor, "tamer", PinKind.Output, PinType.Integer, ImNodesPinShape.Circle);
        var node2 = editor.CreateNode("Node");
        var in2 = node2.CreatePin(editor, "In", PinKind.Input, PinType.Integer, ImNodesPinShape.Quad);
        var out2 = node2.CreatePin(editor, "Out", PinKind.Output, PinType.Integer, ImNodesPinShape.Circle);
        var node3 = editor.CreateNode("Node");
        var in3 = node3.CreatePin(editor, "In", PinKind.Input, PinType.Integer, ImNodesPinShape.Circle);
        node3.CreatePin(editor, "Out", PinKind.Output, PinType.Integer, ImNodesPinShape.Circle);
    }

    public override void Draw()
    {
        editor.Draw();

    }

    public override void Dispose()
    {
        editor.Destroy();
        editor = null;
    }
}
