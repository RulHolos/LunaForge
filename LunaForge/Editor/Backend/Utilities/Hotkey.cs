using Hexa.NET.Raylib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LunaForge.Editor.Backend.Utilities;

public sealed class Hotkey
{
    private string? cache;
    private bool isHeld = false;
    private readonly List<KeyboardKey> keys = [];
    private readonly List<KeyboardKey> defaults = [];
    private readonly HashSet<Hotkey> conflicts = [];

    public readonly string Name;

    [JsonIgnore] public bool Enabled { get; set; } = true;
    [JsonIgnore] public Action Callback;
    [JsonIgnore] public HashSet<Hotkey> Conflicts => conflicts;

    public List<KeyboardKey> Keys { get; private set; }
    public List<KeyboardKey> Defaults => defaults;

    public Hotkey(string name, Action callback)
    {
        Name = name;
        Callback = callback;
        keys = [];
        Keys = [.. keys];
    }

    public Hotkey(string name, Action callback, List<KeyboardKey> defaults)
    {
        Name = name;
        Callback = callback;
        this.defaults.AddRange(defaults);
        keys = defaults;
        Keys = [.. keys];
    }

    public Hotkey(string name, Action callback, IEnumerable<KeyboardKey> defaults)
    {
        Name = name;
        Callback = callback;
        this.defaults.AddRange(defaults);
        keys = [.. defaults];
        Keys = [.. keys];
    }

    public void AddConflictingHotkey(Hotkey hotkey)
    {
        conflicts.Add(hotkey);
        hotkey.conflicts.Add(this);
    }

    public void RemoveConflictingHotkey(Hotkey hotkey)
    {
        conflicts.Remove(hotkey);
        hotkey.conflicts.Remove(this);
    }

    public bool IsConflicting(Hotkey other, bool useHashSet = true)
    {
        if (useHashSet)
            return conflicts.Contains(other);

        if (Keys.Count != other.Keys.Count)
            return false;

        for (int i = 0; i < Keys.Count; i++)
        {
            if (Keys[i] != other.Keys[i])
                return false;
        }

        return true;
    }

    public bool IsSet => Keys.Count != 0;

    public bool CanExecute()
    {
        return Enabled && Keys.Count > 0;
    }

    public bool TryExecute()
    {
        if (isHeld)
        {
            if (Keys.All(k => !Raylib.IsKeyDown((int)k)))
                isHeld = false;
            return false;
        }

        if (Keys.All(k => Raylib.IsKeyDown((int)k)))
        {
            isHeld = true;
            Callback.Invoke();
            return true;
        }

        return false;
    }

    public override string ToString()
    {
        if (cache == null)
        {
            StringBuilder sb = new();
            
            for (int i = 0; i < Keys.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append("+" + Keys[i]);
                else
                    sb.Append(Keys[i]);
            }
            cache = sb.ToString();
        }
        return cache;
    }

    public void Clear()
    {
        Keys.Clear();
        cache = null;
    }

    public void Add(KeyboardKey key)
    {
        if (Keys.Contains(key))
            return;

        Keys.Add(key);
        cache = null;
    }

    public void AddRange(IEnumerable<KeyboardKey> keys)
    {
        foreach (var key in keys)
            Keys.Add(key);
        cache = null;
    }

    public void SetToDefault()
    {
        Clear();
        Keys.AddRange(defaults);
    }
}
