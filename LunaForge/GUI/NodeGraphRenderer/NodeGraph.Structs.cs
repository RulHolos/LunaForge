using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.NodeGraphRenderer;

internal enum StyleColor
{
    ColCanvasLines,
    ColNodeBg,
    ColNodeActiveBg,
    ColNodeBorder,
    ColConnection,
    ColConnectionActive,
    ColSelectBg,
    ColSelectBorder,
    ColMax,
}

internal struct CanvasStyle()
{
    public float CurveThickness = 5f;
    public float ConnectionIndent = 1f;

    public float GridSpacing = 64f;
    public float CurveStrength = 100f;
    public float NodeRounding = 5f;
    public Vector2 NodeSpacing = new(4f, 4f);
}

internal class CanvasState
{
    public CanvasState()
    {
        var imGuiStyle = ImGui.GetStyle();
        Colors[(int)StyleColor.ColCanvasLines] = imGuiStyle.Colors[(int)ImGuiCol.Separator];
        Colors[(int)StyleColor.ColNodeBg] = imGuiStyle.Colors[(int)ImGuiCol.WindowBg];
        Colors[(int)StyleColor.ColNodeActiveBg] = imGuiStyle.Colors[(int)ImGuiCol.FrameBgActive];
        Colors[(int)StyleColor.ColNodeBorder] = imGuiStyle.Colors[(int)ImGuiCol.Border];
        Colors[(int)StyleColor.ColConnection] = imGuiStyle.Colors[(int)ImGuiCol.PlotLines];
        Colors[(int)StyleColor.ColConnectionActive] = imGuiStyle.Colors[(int)ImGuiCol.PlotLinesHovered];
        Colors[(int)StyleColor.ColSelectBg] = imGuiStyle.Colors[(int)ImGuiCol.FrameBgActive];
        Colors[(int)StyleColor.ColSelectBg].W = .25f;
        Colors[(int)StyleColor.ColSelectBorder] = imGuiStyle.Colors[(int)ImGuiCol.Border];
    }

    public CanvasStyle Style = new();
    public float Zoom = 1.0f;
    public Vector2 Offset = Vector2.Zero;
    public Vector4[] Colors = new Vector4[(int)StyleColor.ColMax];
    public CanvasStateImpl Impl = new();
}

internal struct NodeData()
{
    string Id = string.Empty;
    Vector2 Pos = Vector2.Zero;
    bool Selected = false;
    string ItemId = string.Empty;
}

internal struct SlotData()
{
    public int Kind = 0;
    public string Title = "";
}

internal struct Connection()
{
    public uint? InputNode = null;
    public string? InputSlot = null;
    public uint? OutputNode = null;
    public string? OutputSlot = null;
}

internal struct CanvasStateImpl()
{
    public ImGuiStorage CachedData = new();

    public List<NodeData> Node = [];
    public List<SlotData> Slots = [];
    public List<Connection> Connections = [];

    public uint? AutoPositionNodeId = null;

    public Vector2 SelectionStart = Vector2.Zero;
    public uint? DragNode = null;
    public bool DragNodeSelected = false;
    public uint? SingleSelectedNode = null;
    public int DoSelectionFrame = 0;
    public NodeGraphState State = NodeGraphState.None;
    public bool JustConnected = false;
    public List<IgnoreSlot> IgnoreConnections = [];
    public int PrevSelectCount = 0;
    public int CurrSelectCount = 0;
    public uint PendingActiveItemId = 0;
    public uint PendingActiveSlotId = 0;
    public uint HoveredNodeId = 0;
    public uint PendingHoveredNodeId = 0;
}

internal struct IgnoreSlot()
{
    public uint? NodeId = null;
    public string? SlotName = null;
    public int SlotKind = 0;

    public readonly bool Equals(IgnoreSlot other)
    {
        if (NodeId != other.NodeId || SlotKind != other.SlotKind)
            return false;

        if (SlotName != null && other.SlotName != null)
            return SlotName.CompareTo(SlotName) == 0;

        return other.SlotName == SlotName;
    }
}

internal struct DragConnectionPayload()
{
    string? NodeId = null;
    string? SlotTitle = null;
    int SlotKind = 0;
}