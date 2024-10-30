using ImGuiNET;
using LunaForge.EditorData.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.GraphNodes;

[Flags]
public enum GraphNodeSlotKind
{
    None, // This slot will be ignored.
    Input, // Displayed on the left.
    Output, // Displayed on the right.
    UniqueConnection, // Can have only one connection.
    MultipleConnections, // Can have multiple connections.
}

[Flags]
public enum GraphNodeFlags
{
    None,
}

public struct GraphNodeSlot(string id, GraphNodeSlotKind kind = GraphNodeSlotKind.Input | GraphNodeSlotKind.UniqueConnection)
{
    public string ImGUIID = id;
    public GraphNodeSlotKind Kind = kind;
    public int Order = 0;
    public string Data = null;
}


/*
 * Design:
 * Variable node with no input, only outputs
 * 
 * GraphNodeSlot:
 * Have "Data" which can be a variable name or a string with the data value
 */

public abstract class GraphNode
{
    public GraphNodeFlags Flags { get; set; } = GraphNodeFlags.None;
    public List<GraphNodeSlot> Slots { get; set; }
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = new Vector2(50, 60);

    public LunaShader ParentShader { get; set; }

    public abstract string NodeTitle { get; set; }

    public GraphNode()
    {

    }

    public GraphNode(LunaShader parentShader)
        : this()
    {
        ParentShader = parentShader;
    }

    #region Rendering

    public void BeginNode(string label, GraphNodeFlags flags)
    {
        Flags = flags;
        
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        ImGuiIOPtr io = ImGui.GetIO();

        // 0 -> Node Rect, Curves
        // 1 -> Node Content
        drawList.ChannelsSplit(2);

        ImGui.PushID(label);
        ImGui.BeginGroup();
        drawList.ChannelsSetCurrent(1);
    }

    public void EndNode()
    {
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        ImGuiIOPtr io = ImGui.GetIO();

        bool activate = false;

        ImGui.EndGroup();

        drawList.ChannelsSetCurrent(0);


        drawList.ChannelsMerge();
        ImGui.PopID();
    }

    private void DoContextMenu()
    {

    }

    public readonly struct NodeScope : IDisposable
    {
        private readonly GraphNode _manager;

        public NodeScope(GraphNode graph, string label, GraphNodeFlags flags = GraphNodeFlags.None)
        {
            _manager = graph;
            _manager.BeginNode(label, flags);
        }

        public void Dispose()
        {
            _manager.EndNode();
        }
    }

    public NodeScope DoNode(string label, GraphNodeFlags flags = GraphNodeFlags.None) => new(this, label, flags);

    #endregion
    #region ToHLSL

    public abstract IEnumerable<string> ToHLSL();

    #endregion
    #region ToGLSL

    public abstract IEnumerable<string> ToGLSL();

    #endregion
    #region Events



    #endregion
    #region Slots

    public void SetupSlot()
    {

    }

    #endregion
}