using Hexa.NET.ImGui;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.ImGuiExtension;

[Serializable]
public class ImGuiUnifiedConfig
{
    public ImGuiUnifiedConfig() { }

    public string ImGuiIniContent { get; set; } = string.Empty;

    public Dictionary<string, object> Settings { get; set; } = [];

    public void Save(string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public static ImGuiUnifiedConfig Load(string path)
    {
        if (!File.Exists(path))
            return new ImGuiUnifiedConfig();

        var content = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<ImGuiUnifiedConfig>(content) ?? new ImGuiUnifiedConfig();
    }
}