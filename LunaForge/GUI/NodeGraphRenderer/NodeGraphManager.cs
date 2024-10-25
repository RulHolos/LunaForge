using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.NodeGraphRenderer;

internal enum NodeGraphState
{
    None,
    Drag,
    Select
}

[Flags]
internal enum NodeGraphFlags
{
    None,
    HideGrid,
}

internal partial class NodeGraph
{
    private NodeGraphFlags Flags = NodeGraphFlags.None;
    private CanvasState? Canvas;

    public bool BeginCanvas(string label, NodeGraphFlags flags = NodeGraphFlags.None)
    {
        Canvas ??= new();
        Flags = flags;
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
            uint gridColor = ImGui.GetColorU32(Canvas.Colors[(int)StyleColor.ColCanvasLines]);
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

        Canvas.Impl.PrevSelectCount = Canvas.Impl.CurrSelectCount;
        Canvas.Impl.CurrSelectCount = 0;
        return true;
    }

    public void EndCanvas()
    {
        if (Canvas == null)
            return;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        

        if (Canvas.Impl.DoSelectionFrame <= ImGui.GetFrameCount())
            Canvas.Impl.SingleSelectedNode = null;

        switch (Canvas.Impl.State)
        {
            case NodeGraphState.None:
                Canvas.Impl.HoveredNodeId = Canvas.Impl.PendingHoveredNodeId;

                if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
                {
                    if (!ImGui.IsWindowFocused())
                        ImGui.SetWindowFocus();

                    if (!ImGui.IsAnyItemActive())
                    {
                        ImGuiIOPtr io = ImGui.GetIO();
                        if (!io.KeyCtrl && !io.KeyShift)
                        {
                            Canvas.Impl.SingleSelectedNode = null;
                            Canvas.Impl.DoSelectionFrame = ImGui.GetFrameCount() + 1;
                        }
                    }
                }
                break;
            case NodeGraphState.Drag:
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Canvas.Impl.State = NodeGraphState.None;
                    Canvas.Impl.DragNode = null;
                }
                break;
            case NodeGraphState.Select:
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    drawList.AddRectFilled(Canvas.Impl.SelectionStart, ImGui.GetMousePos(), ImGui.GetColorU32(Canvas.Colors[(int)StyleColor.ColSelectBg]));
                    drawList.AddRect(Canvas.Impl.SelectionStart, ImGui.GetMousePos(), ImGui.GetColorU32(Canvas.Colors[(int)StyleColor.ColSelectBorder]));
                }
                else
                {
                    Canvas.Impl.State = NodeGraphState.None;
                }
                break;
        }

        Canvas.Impl.PendingHoveredNodeId = 0;

        ImGui.SetWindowFontScale(1f);
        ImGui.PopID();
    }
}