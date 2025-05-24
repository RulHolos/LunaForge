using Hexa.NET.Raylib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Utilities;

public sealed class Hotkey
{
    private string? cache;
    private readonly List<KeyboardKey> keys = [];
    private readonly List<KeyboardKey> defaults = [];
    private readonly HashSet<Hotkey> conflicts = [];

    public readonly string Name;

    [JsonIgnore] public bool Enabled { get; set; } = true;
    [JsonIgnore] public Action Callback;
    [JsonIgnore] public HashSet<Hotkey> Conflicts => conflicts;

    public List<KeyboardKey> Keys { get; private set; }
    public List<KeyboardKey> Defaults => defaults;
}
