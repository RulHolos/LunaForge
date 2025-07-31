using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.LunaTreeNodes;

public class ConfigSystemEntryRecord
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class TreeNodeRecord
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }

    public string NodeName { get; set; } = string.Empty;

    public string SerializedNode { get; set; } = string.Empty;

    public ulong? ParentId { get; set; }
}

public static class FlattenTreeHelper
{
    public static List<TreeNodeRecord> FlattenTree(TreeNode root, ref ulong idCounter, ulong? parentId = null)
    {
        List<TreeNodeRecord> result = [];
        ulong id = ++idCounter;

        var sql = new TreeNodeRecord
        {
            Id = id,
            NodeName = root.GetType().AssemblyQualifiedName,
            SerializedNode = JsonConvert.SerializeObject(root),
            ParentId = parentId
        };

        result.Add(sql);

        foreach (var child in root.Children)
            result.AddRange(FlattenTree(child, ref idCounter, id));

        return result;
    }

    public static List<TreeNodeRecord> FlattenTree(TreeNode root)
    {
        ulong counter = 0;
        return FlattenTree(root, ref counter, null);
    }
}