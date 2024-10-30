using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace LunaForge.GUI.NodeGraphRenderer;

public partial class NodeGraph
{
    #region Rendering things

    public bool RenderConnection(Vector2 inputSlot, Vector2 outputSlot, float thickness)
    {
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        ImGuiStylePtr style = ImGui.GetStyle();

        thickness *= Canvas.Zoom;
        Vector2 p2 = inputSlot - new Vector2(Canvas.Style.CurveStrength * Canvas.Zoom, 0);
        Vector2 p3 = outputSlot + new Vector2(Canvas.Style.CurveStrength * Canvas.Zoom, 0);

        Vector2 closest_pt = ClosestPointOnCubicBezier(inputSlot, p2, p3, outputSlot, ImGui.GetMousePos(), style.CurveTessellationTol);

        float min_square_distance = MathF.Abs(SquaredDistance(ImGui.GetMousePos(), closest_pt));
        bool is_close = min_square_distance <= thickness * thickness;
        uint col = is_close
            ? ImGui.GetColorU32(Canvas.Style.Colors[(int)StyleColor.ColConnectionActive])
            : ImGui.GetColorU32(Canvas.Style.Colors[(int)StyleColor.ColConnection]);
        drawList.AddBezierCubic(inputSlot, p2, p3, outputSlot, col, thickness, 0);

        return is_close;
    }

    #endregion
    #region Maths.

    public static Vector2 ClosestPointOnCubicBezier(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float subdivisions = 20f)
    {
        float closestDistSq = float.MaxValue;
        Vector2 closestPoint = Vector2.Zero;

        for (int i = 0; i <= subdivisions; i++)
        {
            float t = i / subdivisions;
            Vector2 pointOnCurve = DeCasteljau(p0, p1, p2, p3, t);
            float distanceSq = Vector2.DistanceSquared(p, pointOnCurve);

            if (distanceSq < closestDistSq)
            {
                closestDistSq = distanceSq;
                closestPoint = pointOnCurve;
            }
        }

        return closestPoint;
    }

    public static Vector2 DeCasteljau(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 a = Vector2.Lerp(p0, p1, t);
        Vector2 b = Vector2.Lerp(p1, p2, t);
        Vector2 c = Vector2.Lerp(p2, p3, t);

        Vector2 d = Vector2.Lerp(a, b, t);
        Vector2 e = Vector2.Lerp(b, c, t);

        return Vector2.Lerp(d, e, t);
    }

    public static float SquaredDistance(Vector2 pointA, Vector2 pointB)
    {
        Vector2 difference = pointA - pointB;
        return difference.LengthSquared();
    }

    #endregion
}
