using Hexa.NET.ImGui;
using LunaForge.Editor.Backend;
using LunaForge.Editor.UI.ImGuiExtension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Managers;

public struct LayoutConfig : IEquatable<LayoutConfig>
{
    public string Path;
    public string Name;

    public LayoutConfig(string path, string name)
    {
        Path = path;
        Name = name;
    }

    public LayoutConfig(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(path);
    }

    public override readonly bool Equals(object? obj) => obj is LayoutConfig config && Equals(config);

    public readonly bool Equals(LayoutConfig other) => Path == other.Path;

    public override int GetHashCode() => HashCode.Combine(Path);

    public static bool operator ==(LayoutConfig left, LayoutConfig right) => left.Equals(right);

    public static bool operator !=(LayoutConfig left, LayoutConfig right) => !(left == right);
}

public static class LayoutManager
{
    private static readonly string basePath = Path.Combine(EditorConfig.BasePath, "Layouts");
    private static readonly string defaultPath = Path.Combine(basePath, "default.json");
    private static readonly List<LayoutConfig> layouts = [];
    private static bool changed = false;

    static LayoutManager()
    {
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);
        layouts.Add(new LayoutConfig(defaultPath, "Default"));
        foreach (string file in Directory.GetFiles(basePath, "*.json"))
        {
            if (file.EndsWith("default.json"))
                continue;
            layouts.Add(new LayoutConfig(file));
        }
    }

    public static IReadOnlyList<LayoutConfig> Layouts => layouts;

    public static string SelectedLayoutPath
    {
        get
        {
            var entry = EditorConfig.Default.Get<string>("SelectedLayout");
            if (string.IsNullOrEmpty(entry.Value))
                entry.Value = defaultPath;
            return entry.Value;
        }
        set
        {
            if (layouts.Contains(new LayoutConfig() { Path = value }))
                SetLayout(value);
        }
    }

    public static ImGuiUnifiedConfig SelectedLayout;

    public static string BasePath => basePath;

    internal static unsafe bool Init()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.IniFilename = null;

        var layout = SelectedLayoutPath!;
        SelectedLayout = ImGuiUnifiedConfig.Load(layout);
        //SetIniString(layout);

        if (File.Exists(layout))
        {
            ImGui.LoadIniSettingsFromMemory(SelectedLayout.ImGuiIniContent ?? string.Empty);
            return true;
        }
        return false;
    }

    internal static void NewFrame()
    {
        if (changed)
        {
            var layout = SelectedLayoutPath!;
            SelectedLayout = ImGuiUnifiedConfig.Load(layout);

            ImGui.LoadIniSettingsFromMemory(SelectedLayout.ImGuiIniContent ?? string.Empty);

            SelectedLayout.Save(layout);
            //SetIniString(layout);
            changed = false;
        }
    }

    private static unsafe void SetLayout(string value)
    {
        EditorConfig.Default.SetOrCreate("SelectedLayout", value);
        EditorConfig.Default.CommitAllAndSave();
        changed = true;
    }

    public static unsafe void Save()
    {
        byte* ptr = ImGui.SaveIniSettingsToMemory();
        if (ptr == null)
            return;

        int length = 0;
        while (ptr[length] != 0)
            length++;

        string initStr = Encoding.UTF8.GetString(ptr, length);

        SelectedLayout.ImGuiIniContent = initStr;
        SelectedLayout.Save(SelectedLayoutPath);
    }

    private static unsafe void SetIniString(string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        var str = (byte*)ImGui.MemAlloc((ulong)(byteCount + 1));
        fixed (char* pValue = value)
        {
            Encoding.UTF8.GetBytes(pValue, value.Length, str, byteCount);
        }
        str[byteCount] = 0;

        var io = ImGui.GetIO();
        io.IniFilename = str;
    }

    public static void ResetLayout()
    {
        ImGuiManager.ResetLayout();
    }

    public static void CreateNewLayout(string name)
    {
        string path = Path.Combine(basePath, $"{name}.json");
        layouts.Add(new(path, name));
        SelectedLayoutPath = path;
        SelectedLayout = ImGuiUnifiedConfig.Load(SelectedLayoutPath);
        SelectedLayout.Save(SelectedLayoutPath);
        //SetIniString(path);
    }
}