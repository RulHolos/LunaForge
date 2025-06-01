using Hexa.NET.Raylib;
using LunaForge.Editor.Backend.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.UI.Managers;

public static class HotkeyManager
{
    //private static readonly List<KeyboardKey> keys = [];
    private static readonly List<Hotkey> hotkeys = [];
    private const string FileName = "hotkeys.json";

    static HotkeyManager()
    {
        if (File.Exists(FileName))
            JsonConvert.DeserializeObject(File.ReadAllText(FileName));
    }

    public static object SyncObject => hotkeys;
    public static IReadOnlyList<Hotkey> Hotkeys => hotkeys;
    public static int Count => hotkeys.Count;

    public static void Save()
    {
        lock (hotkeys)
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(hotkeys));
        }
    }

    public static int IndexOf(string name)
    {
        lock (hotkeys)
        {
            for (int i = 0; i < hotkeys.Count; i++)
            {
                if (hotkeys[i].Name == name)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public static Hotkey? Find(string name)
    {
        lock (hotkeys)
        {
            for (int i = 0; i < hotkeys.Count; i++)
            {
                if (hotkeys[i].Name == name)
                {
                    return hotkeys[i];
                }
            }
        }

        return null;
    }

    public static bool TryFind(string name, [NotNullWhen(true)] out Hotkey? hotkey)
    {
        hotkey = Find(name);
        return hotkey != null;
    }

    private static IEnumerable<Hotkey> FindConflicts(Hotkey hotkey)
    {
        foreach (Hotkey other in hotkeys)
        {
            if (hotkey.IsConflicting(other, false))
                yield return other;
        }
    }

    public static Hotkey Register(string name, Action callback)
    {
        lock (hotkeys)
        {
            if (TryFind(name, out var hotkey))
                hotkey.Callback = callback;
            else
            {
                hotkey = new(name, callback);
                hotkeys.Add(hotkey);

                foreach (var conflicts in FindConflicts(hotkey))
                {
                    hotkey.AddConflictingHotkey(conflicts);
                }
            }

            Save();

            return hotkey;
        }
    }

    public static Hotkey Register(string name, Action callback, params KeyboardKey[] defaults)
    {
        lock (hotkeys)
        {
            if (TryFind(name, out var hotkey))
                hotkey.Callback = callback;
            else
            {
                hotkey = new(name, callback, defaults);
                hotkeys.Add(hotkey);

                foreach (var conflicts in FindConflicts(hotkey))
                {
                    hotkey.AddConflictingHotkey(conflicts);
                }
            }

            Save();

            return hotkey;
        }
    }

    public static void Unregister(string name)
    {
        lock (hotkeys)
        {
            var index = IndexOf(name);
            if (index == -1)
                return;

            var hotkey = hotkeys[index];
            
            foreach (var conflict in hotkey.Conflicts)
                hotkey.RemoveConflictingHotkey(conflict);

            hotkeys.RemoveAt(index);
        }

        Save();
    }

    public static void DoHotkeys()
    {
        lock (hotkeys)
        {
            for (int i = 0; i < hotkeys.Count; i++)
            {
                var hotkey = hotkeys[i];

                if (!hotkey.CanExecute())
                    continue;

                hotkey.TryExecute();
            }
        }
    }
}
