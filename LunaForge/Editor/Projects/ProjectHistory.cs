using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Projects;

public struct HistoryEntry
{
    public HistoryEntry(string name, string path)
    {
        Name = name;
        Path = path;
        Pinned = false;
        LastAccess = DateTime.UtcNow;
    }

    public HistoryEntry(string name, string path, bool pinned)
    {
        Name = name;
        Path = path;
        Pinned = pinned;
        LastAccess = DateTime.UtcNow;
    }

    [JsonConstructor]
    public HistoryEntry(string name, string path, bool pinned, DateTime lastAccess)
    {
        Name = name;
        Path = path;
        Pinned = pinned;
        LastAccess = lastAccess;
    }

    public string Name { get; set; }

    public string Path { get; set; }

    public bool Pinned { get; set; }

    public DateTime LastAccess { get; set; }
}

public readonly struct LastAccessComparer : IComparer<HistoryEntry>
{
    public static readonly LastAccessComparer Instance = new();

    public readonly int Compare(HistoryEntry x, HistoryEntry y)
    {
        if (x.Pinned && !y.Pinned)
        {
            return -1;
        }
        else if (!x.Pinned && y.Pinned)
        {
            return 1;
        }
        return y.LastAccess.CompareTo(x.LastAccess);
    }
}

public static class ProjectHistory
{
    private const string historyFile = "projectHistory.json";
    private static readonly string historyPath = Path.Combine(EditorConfig.BasePath, historyFile);
    private static readonly List<HistoryEntry> entries;
    private static readonly List<HistoryEntry> pinned;

    static ProjectHistory()
    {
        if (File.Exists(historyPath))
        {
            entries = JsonConvert.DeserializeObject<List<HistoryEntry>>(File.ReadAllText(historyPath)) ?? [];
            entries.Sort(LastAccessComparer.Instance);
        }
        else
        {
            entries = [];
        }

        pinned = [.. entries.Where(x => x.Pinned)];
    }

    public static IReadOnlyList<HistoryEntry> Entries => entries;
    public static IReadOnlyList<HistoryEntry> Pinned => pinned;

    public static void AddEntry(string name, string path)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.Path == path)
            {
                entry.LastAccess = DateTime.UtcNow;
                entries[i] = entry;
                Save();
                return;
            }
        }

        entries.Add(new(name, path));
        Save();
    }

    public static void Pin(string path)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.Path == path)
            {
                entry.Pinned = true;
                pinned.Add(entry);
                entries[i] = entry;
                Save();
                return;
            }
        }
    }

    public static void Unpin(string path)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.Path == path)
            {
                entry.Pinned = false;
                pinned.Remove(entry);
                entries[i] = entry;
                Save();
                return;
            }
        }
    }

    public static void RemoveEntryByName(string name)
    {
        entries.RemoveAll(x => x.Name == name);
        Save();
    }

    public static void RemoveEntryByPath(string path)
    {
        entries.RemoveAll(x => x.Path == path);
        Save();
    }

    public static void Clear()
    {
        entries.Clear();
        Save();
    }

    private static void Save()
    {
        File.WriteAllText(historyPath, JsonConvert.SerializeObject(entries));
    }
}