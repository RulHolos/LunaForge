using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI;

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}

public struct Toast
{
    public string Message;
    public ToastType Type;
    public DateTime TimeAdded;
    public float Duration;
    public bool IsHovered;
    public Action ClickCallback;
}

internal static class NotificationManager
{
    private static List<Toast> toasts { get; set; } = [];

    /// <summary>
    /// Max number of displayed toasts at the same time.
    /// </summary>
    public static int MaxToasts { get; set; } = 5;
    /// <summary>
    /// Maximum display time for a toast (in seconds).
    /// </summary>
    public static float MaximumDuration { get; set; } = 5.0f;

    public static int ToastSize { get; set; } = 300;

    /// <summary>
    /// Adds a new notification toast to be displayed.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="duration">Duration in seconds. Negative numbers means that the toast won't expire with time.</param>
    /// <param name="clickCallback">If null, clicking the toast will close it.</param>
    public static void AddToast(
        string message,
        ToastType type = ToastType.Info,
        float duration = 5f,
        Action clickCallback = null)
    {
        if (toasts.Count >= MaxToasts)
            toasts.RemoveAt(0); // Remove oldest if limit is reached.

        toasts.Add(new Toast
        {
            Message = message,
            Type = type,
            TimeAdded = DateTime.Now,
            Duration = duration,
            IsHovered = false,
            ClickCallback = clickCallback,
        });
    }

    public static void AddToast(Toast toast)
    {
        if (toasts.Count >= MaxToasts)
            toasts.RemoveAt(0);

        toasts.Add(toast);
    }

    private static void DeleteToast(ref int index, bool decrement = true)
    {
        toasts.RemoveAt(index);
        if (decrement)
            index--;
    }

    public static void Render()
    {
        if (toasts.Count == 0)
            return; // Nothing to do.

        Vector2 pos = new(ImGui.GetIO().DisplaySize.X - ToastSize, 40);

        for (int i = 0; i < toasts.Count; i++)
        {
            var toast = toasts[i];

            if (toast.Duration > 0
                && (DateTime.Now - toast.TimeAdded).TotalSeconds > MathF.Min(MaximumDuration, toast.Duration)
                && !toast.IsHovered
            )
            {
                DeleteToast(ref i);
                continue;
            }

            ImGui.SetNextWindowPos(pos);
            ImGui.SetNextWindowSize(new Vector2(ToastSize - 10, 50));
            var bgColor = toast.Type switch
            {
                ToastType.Info => new Vector4(0.4f, 0.4f, 0.8f, 1.0f), // Default gray for info
                ToastType.Success => new Vector4(0.0f, 0.8f, 0.0f, 1.0f), // Green for success
                ToastType.Warning => new Vector4(0.7f, 0.7f, 0.0f, 1.0f), // Yellow for warning
                ToastType.Error => new Vector4(0.8f, 0.0f, 0.0f, 1.0f), // Red for error
                _ => new Vector4(0f, 0f, 0f, 1f),
            };
            ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor);
            if (ImGui.Begin($"##ToastNotification_{i}",
                ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.AlwaysAutoResize
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.TextWrapped(toast.Message);

                if (ImGui.IsWindowHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
                if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    if (toast.ClickCallback != null)
                    {
                        toast.ClickCallback();
                    }
                    else
                    {
                        DeleteToast(ref i);
                    }
                }

                ImGui.End();
            }
            
            ImGui.PopStyleColor();

            pos.Y += 60;
        }
    }
}
