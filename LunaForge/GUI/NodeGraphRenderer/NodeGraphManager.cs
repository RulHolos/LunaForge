using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.NodeGraphRenderer;

public enum NodeGraphState
{
    None,
    Drag,
    Select
}

[Flags]
public enum NodeGraphFlags
{
    None,
    HideGrid,
    HideControls,
}

public partial class NodeGraph()
{
    private NodeGraphFlags Flags = NodeGraphFlags.None;
    public CanvasState Canvas = new();
    public NodeGraphData Data = new();

    #region Rendering pipeline

    public void BeginCanvas(string label, NodeGraphFlags flags = NodeGraphFlags.None)
    {
        Flags = flags;
        ImGui.BeginChild($"{label}_ChildWindow");
        ImGui.PushID(label);

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        ImGuiIOPtr io = ImGui.GetIO();

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
        {
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
                Canvas.Offset += io.MouseDelta;

            if (io.KeyShift && !io.KeyCtrl)
                Canvas.Offset.X += io.MouseWheel * 16f;

            if (!io.KeyShift && !io.KeyCtrl)
            {
                Canvas.Offset.Y += io.MouseWheel * 16f;
                Canvas.Offset.X += io.MouseWheelH * 16f;
            }

            if (!io.KeyShift && io.KeyCtrl)
            {
                if (io.MouseWheel != 0)
                {
                    Vector2 mouseRel = new(ImGui.GetMousePos().X - ImGui.GetWindowPos().X, ImGui.GetMousePos().Y - ImGui.GetWindowPos().Y);
                    float prevZoom = Canvas.Zoom;
                    Canvas.Zoom = Math.Clamp(Canvas.Zoom + io.MouseWheel * Canvas.Zoom / 16f, 0.3f, 3f);
                    float zoomFactor = (prevZoom - Canvas.Zoom) / prevZoom;
                    Canvas.Offset += (mouseRel - Canvas.Offset) * zoomFactor;
                }
            }
        }

        float grid = Canvas.Style.GridSpacing * Canvas.Zoom;

        Vector2 pos = ImGui.GetWindowPos();
        Vector2 size = ImGui.GetWindowSize();

        if (!Flags.HasFlag(NodeGraphFlags.HideGrid))
        {
            uint gridColor = ImGui.GetColorU32(Canvas.Style.Colors[(int)StyleColor.ColCanvasLines]);
            for (float x = MathF.IEEERemainder(Canvas.Offset.X, grid); x < size.X; x += grid)
            {
                drawList.AddLine(new Vector2(x, 0) + pos, new Vector2(x, size.Y) + pos, gridColor);
            }
            for (float y = MathF.IEEERemainder(Canvas.Offset.Y, grid); y < size.Y; y += grid)
            {
                drawList.AddLine(new Vector2(0, y) + pos, new Vector2(size.X, y) + pos, gridColor);
            }
        }

        ImGui.SetWindowFontScale(Canvas.Zoom);

        Data.PrevSelectCount = Data.CurrSelectCount;
        Data.CurrSelectCount = 0;
    }

    public void EndCanvas()
    {
        if (Canvas == null)
            return;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        if (Data.DoSelectionFrame <= ImGui.GetFrameCount())
            Data.SingleSelectedNode = null;

        switch (Data.State)
        {
            case NodeGraphState.None:
                Data.HoveredNodeId = Data.PendingHoveredNodeId;

                if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
                {
                    if (!ImGui.IsWindowFocused())
                        ImGui.SetWindowFocus();

                    if (!ImGui.IsAnyItemActive())
                    {
                        ImGuiIOPtr io = ImGui.GetIO();
                        if (!io.KeyCtrl && !io.KeyShift)
                        {
                            Data.SingleSelectedNode = null;
                            Data.DoSelectionFrame = ImGui.GetFrameCount() + 1;
                        }
                    }
                }
                break;
            case NodeGraphState.Drag:
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Data.State = NodeGraphState.None;
                    Data.DragNode = null;
                }
                break;
            case NodeGraphState.Select:
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    drawList.AddRectFilled(Data.SelectionStart, ImGui.GetMousePos(), ImGui.GetColorU32(Canvas.Style.GetColor(StyleColor.ColSelectBg)));
                    drawList.AddRect(Data.SelectionStart, ImGui.GetMousePos(), ImGui.GetColorU32(Canvas.Style.GetColor(StyleColor.ColSelectBorder)));
                }
                else
                {
                    Data.State = NodeGraphState.None;
                }
                break;
        }

        Data.PendingHoveredNodeId = 0;

        ImGui.SetWindowFontScale(1f);
        ImGui.EndChild();
        ImGui.PopID();
    }

    public readonly struct CanvasScope : IDisposable
    {
        private readonly NodeGraph _manager;

        public CanvasScope(NodeGraph graph, string label, NodeGraphFlags flags = NodeGraphFlags.None)
        {
            _manager = graph;
            _manager.BeginCanvas(label, flags);
        }

        public void Dispose()
        {
            _manager.EndCanvas();
        }
    }

    public CanvasScope DoCanvas(string label, NodeGraphFlags flags = NodeGraphFlags.None) => new(this, label, flags);

    public Vector2 RelativeToGrid(Vector2 position)
    {
        return Canvas.Offset + position;
    }

    public Vector2 ScreenToGrid(Vector2 screenPosition)
    {
        return screenPosition - Canvas.Offset;
    }

    #endregion
    #region Oui



    #endregion
}