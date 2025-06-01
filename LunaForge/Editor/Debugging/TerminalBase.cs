using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Debugging;

public abstract class TerminalBase : ITerminal
{
    private readonly TerminalColorPalette colorPalette = new();
    private readonly List<TerminalMessage> messages = [];
    private bool shown;
    private string inBuffer = string.Empty;
    private bool autoScroll = true;
    private bool scrollToBottom;
    private bool coloredOutput = true;

    public bool Shown => shown;

    public IReadOnlyList<TerminalMessage> Messages => messages;

    public static void HelpMarker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public virtual void Draw()
    {
        DrawMenuBar();
        ImGui.Separator();
        DrawMessages();
        DrawInput();
    }

    public virtual void Focus() { }

    public virtual void Show()
    {
        shown = true;
    }

    public virtual void Close()
    {
        shown = false;
    }

    public virtual void ScrollToBottom()
    {
        scrollToBottom = true;
    }

    protected virtual void AddMessage(string text)
    {
        lock (messages)
        {
            messages.Add(new TerminalMessage() { Message = text, Color = TerminalColor.White });
        }
        scrollToBottom = true;
    }

    protected virtual void AddMessage(string text, TerminalColor color)
    {
        lock (messages)
        {
            messages.Add(new TerminalMessage() { Message = text, Color = color });
        }
        scrollToBottom = true;
    }

    protected virtual void Input(string text)
    {

    }

    public void Clear()
    {
        lock (messages)
            messages.Clear();
    }

    public virtual void SetMessage(int index, TerminalMessage message)
    {
        messages[index] = message;
    }

    protected virtual unsafe void DrawInput()
    {
        bool reclaimFocus = false;
        ImGui.PushItemWidth(-ImGui.GetStyle().ItemSpacing.X * 7);
        if (ImGui.InputText("Input", ref inBuffer, 1024, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory))
        {
            Input(inBuffer);
            inBuffer = string.Empty;
            reclaimFocus = true;
        }

        ImGui.SetItemDefaultFocus();
        if (reclaimFocus)
        {
            ImGui.SetKeyboardFocusHere(-1);
        }
    }

    protected virtual void DrawMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Settings"))
            {
                ImGui.Checkbox("Colored Output", ref coloredOutput);
                ImGui.SameLine();
                HelpMarker("Enable colored command output");

                ImGui.Checkbox("Auto Scroll", ref autoScroll);
                ImGui.SameLine();
                HelpMarker("Automatically scroll to the bottom of the console log");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Appearance"))
            {
                ImGui.TextUnformatted("Color Palette");
                ImGui.Indent();

                ImGui.Unindent();

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    protected virtual unsafe void DrawMessages()
    {
        float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        lock (messages)
        {
            ImGui.BeginChild(1, new Vector2(0, -footerHeightToReserve), ImGuiWindowFlags.HorizontalScrollbar);

            float scrollPos = ImGui.GetScrollY();
            float lineHeight = ImGui.GetTextLineHeightWithSpacing();
            int startLine = (int)(scrollPos / lineHeight);

            float windowHeight = ImGui.GetWindowHeight();
            int visibleLines = (int)(windowHeight / lineHeight);
            int endLine = startLine + visibleLines;

            endLine = Math.Min(endLine, messages.Count);

            float dummyHeight = messages.Count * lineHeight;

            Vector2 cursor = ImGui.GetCursorPos();
            ImGui.Dummy(new(0, dummyHeight));
            ImGui.SetCursorPos(cursor + new Vector2(0, startLine * lineHeight));

            for (int i = 0; i < messages.Count; i++)
            {
                var msg = messages[i];
                if (coloredOutput)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, colorPalette[msg.Color]);
                    ImGui.TextUnformatted(msg.Message);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.TextUnformatted(msg.Message);
                }
            }
            if (scrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || autoScroll))
            {
                ImGui.SetScrollHereY(1.0f);
            }

            scrollToBottom = false;

            ImGui.EndChild();
        }
    }

    protected virtual unsafe int InputCallback(ImGuiInputTextCallbackData* data)
    {
        return 0;
    }
}
