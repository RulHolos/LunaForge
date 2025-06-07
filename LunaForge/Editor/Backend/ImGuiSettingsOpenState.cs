using Hexa.NET.ImGui;
using LunaForge.Editor.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend;

// Fuck this, this is so stupid.

/*
public unsafe static class ImGuiSettingsOpenState
{
    private static void ClearAll(ImGuiContext* ctx, ImGuiSettingsHandler* handler)
    {
        for (int i = 0; i < ctx->Windows.Size; i++)
        {
            ctx->Windows[i].SettingsOffset = -1;
        }
        ctx->SettingsWindows.Buf.Clear();
    }

    private static void ReadInit(ImGuiContext* ctx, ImGuiSettingsHandler* handler)
    {
        // Init reading
    }

    private static void* ReadOpen(ImGuiContext* ctx, ImGuiSettingsHandler* handler, byte* name)
    {
        uint id = ImGuiP.ImHashStr(name);
        ImGuiWindowSettings* settings = ImGuiP.FindWindowSettingsByID(id);
        if (settings != null)
            settings = ImGuiP.ImGuiWindowSettings();
        else
            settings = ImGuiP.CreateNewWindowSettings(name);
        settings->ID = id;
        settings->WantApply = 1;

        return (void*)settings;
    }

    private static void ReadLine(ImGuiContext* ctx, ImGuiSettingsHandler* handler, void* entry, byte* line)
    {
        ImGuiWindowSettings* settings = (ImGuiWindowSettings*)entry;

        short x, y, i;

        if (Regex.Match(Utf8PtrToString(line), @"^Pos=(\d+),(\d+)$", RegexOptions.None) is { Success: true } match1)
        {
            x = short.Parse(match1.Groups[1].Value);
            y = short.Parse(match1.Groups[2].Value);
            settings->Pos = new ImVec2Ih(x, y);
        }
        else if (Regex.Match(Utf8PtrToString(line), @"^Size=(\d+),(\d+)$") is { Success: true } match2)
        {
            x = short.Parse(match2.Groups[1].Value);
            y = short.Parse(match2.Groups[2].Value);
            settings->Size = new ImVec2Ih(x, y);
        }
        else if (Regex.Match(Utf8PtrToString(line), @"^Collapsed=(\d+)$") is { Success: true } match3)
        {
            i = short.Parse(match3.Groups[1].Value);
            settings->Collapsed = Convert.ToByte(i != 0);
        }
        else if (Regex.Match(Utf8PtrToString(line), @"^IsChild=(\d+)$") is { Success: true } match4)
        {
            i = short.Parse(match4.Groups[1].Value);
            settings->IsChild = Convert.ToByte(i != 0);
        }
    }

    private static void ApplyAll(ImGuiContext* ctx, ImGuiSettingsHandler* handler)
    {
        ref ImGuiContext g = ref *ctx;

        ImGuiWindowSettings* settings;

        if (settings->WantApply == 1)
        {
            ImGuiWindowPtr window = ImGuiP.FindWindowByID(settings->ID);
            if (!window.IsNull)
                //Apply window settings;
                return;

            settings->WantApply = 0;
        }
    }

    private static void WriteAll(ImGuiContext* ctx, ImGuiSettingsHandler* handler, ImGuiTextBuffer* buffer)
    {
        buffer->append("[WindowState][MyWindow]\nIsShown=true\n");
    }

    public static void RegisterCustomSettingsHandler()
    {
        byte* typeName = Utf8Encode("WindowState");

        ImGuiSettingsHandler handler = new(
            typeName,
            ImGuiP.ImHashStr("WindowState"),
            &ClearAll,
            &ReadInit,
            &ReadOpen,
            &ReadLine,
            &ApplyAll,
            &WriteAll
        );

        ImGuiP.AddSettingsHandler(&handler);
    }

    private static byte* Utf8Encode(string text)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(text + '\0');
        fixed (byte* ptr = utf8)
            return ptr;
    }

    public static unsafe string Utf8PtrToString(byte* ptr)
    {
        if (ptr == null) return string.Empty;

        int length = 0;
        while (ptr[length] != 0)
            length++;

        return Encoding.UTF8.GetString(ptr, length);
    }
}
*/