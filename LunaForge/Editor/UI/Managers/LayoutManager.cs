using Hexa.NET.ImGui;
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
    private static readonly string defaultPath = Path.Combine(basePath, "default.ini");
    private static readonly List<LayoutConfig> layouts = [];
    private static bool changed = false;

    static LayoutManager()
    {
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);
        layouts.Add(new LayoutConfig(defaultPath, "Default"));
        foreach (var file in Directory.GetFiles(basePath, "*.ini"))
        {
            if (file.EndsWith("default.ini"))
                continue;
            layouts.Add(new LayoutConfig(file));
        }
    }

    public static IReadOnlyList<LayoutConfig> Layouts => layouts;

    public static string SelectedLayout
    {
        get => EditorConfig.Default.Get<string>("SelectedLayout").Value ??= defaultPath;
        set
        {
            if (layouts.Contains(new LayoutConfig() { Path = value }))
            {
                SetLayout(value);
            }
        }
    }

    public static string BasePath => basePath;

    internal static unsafe bool Init()
    {
        var layout = SelectedLayout!;
        SetIniString(layout);

        if (File.Exists(layout))
        {
            ImGui.LoadIniSettingsFromDisk(layout);
            return true;
        }
        return false;
    }

    internal static void NewFrame()
    {
        if (changed)
        {
            var layout = SelectedLayout!;
            ImGui.LoadIniSettingsFromDisk(layout);
            SetIniString(layout);
            changed = false;
        }
    }

    private static unsafe void SetLayout(string value)
    {
        EditorConfig.Default.SetOrCreate("SelectedLayout", value);
        EditorConfig.Default.CommitAllAndSave();
        changed = true;
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

    public static void CreateNewLayout(string name)
    {
        string path = Path.Combine(basePath, $"{name}.ini");
        layouts.Add(new(path, name));
        SetIniString(path);
    }
}