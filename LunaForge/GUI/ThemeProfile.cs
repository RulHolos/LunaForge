using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.IO.Compression;

namespace LunaForge.GUI;

[Serializable]
public class ThemeProfile
{
    public ThemeProfile()
    {
        TempName = Name;
        TempFontPath = FontPath;
        TempFontSize = FontSize;
    }

    public ThemeProfile(Vector4[] imGuiCols)
        : this()
    {
        Colors = imGuiCols;
        TempColors = imGuiCols;
    }

    [DefaultValue("New Profile")]
    public string Name = "New Profile";
    [YamlIgnore]
    public string TempName = "New Profile";
    [DefaultValue("")]
    public string? FontPath = string.Empty;
    [YamlIgnore]
    public string? TempFontPath = string.Empty;

    [DefaultValue(12f)]
    public float FontSize { get; set; } = 12f;
    [YamlIgnore]
    public float TempFontSize = 12f;
    public Vector4[] Colors { get; set; }
    public Vector4[] TempColors { get; set; }

    public Vector4[] GetTempColors() => (Vector4[])Colors.Clone();

    public string ExportToYAML()
    {
        ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

        string yaml = serializer.Serialize(this);
        return yaml;
    }

    public string ExportToBase64()
    {
        string seri = ExportToYAML();
        var bytes = Encoding.UTF8.GetBytes(seri);
        using var ms = new MemoryStream();
        using (var gs = new GZipStream(ms, CompressionMode.Compress))
            gs.Write(bytes, 0, bytes.Length);
        return Convert.ToBase64String(ms.ToArray());
    }

    public static ThemeProfile FromBase64(string s)
    {
        var data = Convert.FromBase64String(s);
        using var ms = new MemoryStream(data);
        using var gs = new GZipStream(ms, CompressionMode.Decompress);
        using var r = new StreamReader(gs);
        string confYaml = r.ReadToEnd();

        IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

        ThemeProfile config = null;
        try { config = deserializer.Deserialize<ThemeProfile>(confYaml); }
        catch (Exception ex)
        {
            NotificationManager.AddToast("There has been an error trying to get the theme data.\nSee console for more info.", ToastType.Error);
            Console.WriteLine($"There was an error trying to get theme from base64:\n{ex}");
        }
        return config;
    }
}

public static class RangeAccessorExtensions
{
    public static T[] ConvertToArray<T>(this RangeAccessor<T> accessor) where T : struct
    {
        T[] array = new T[accessor.Count];
        for (int i = 0; i < accessor.Count; i++)
        {
            array[i] = accessor[i];
        }
        return array;
    }
}