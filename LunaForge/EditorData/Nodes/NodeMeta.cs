using LunaForge.EditorData.Nodes.Attributes;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes;

public sealed class NodeMeta
{
    public bool IsFolder { get; }

    public bool IsDefinition { get; }

    public bool IsLeafNode { get; }

    public bool CannotBeDeleted { get; }

    public bool CannotBeBanned { get; }

    public bool IgnoreValidation { get; }

    public bool Unique { get; }

    public string Icon { get; }

    public string[] RequireParent { get; } = null;
    public string[] RequireAncestor { get; } = null;

    public int? CreateInvokeId { get; } = null;
    public int? RCInvokeId { get; } = null;
    public int? Priority { get; } = null;

    public NodeMeta(TreeNode node)
    {
        Type type = node.GetType();

        IsFolder = type.IsDefined(typeof(IsFolderAttribute), false);
        IsDefinition = type.IsDefined(typeof(DefinitionNodeAttribute), true);
        IsLeafNode = type.IsDefined(typeof(LeafNodeAttribute), true);
        CannotBeDeleted = type.IsDefined(typeof(CannotBeDeletedAttribute), false);
        CannotBeBanned = type.IsDefined(typeof(CannotBeBannedAttribute), false);
        IgnoreValidation = type.IsDefined(typeof(IgnoreValidationAttribute), false);
        Unique = type.IsDefined(typeof(UniqueAttribute), false);

        string pathToImage = type.GetAttributeValue((NodeIconAttribute img) => img.Path);
        Icon = $"{(string.IsNullOrEmpty(pathToImage) ? "Unknown" : pathToImage)}";

        RequireParent = type.GetCustomAttribute<RequireParentAttribute>()?.ParentType;
        RequireAncestor = type.GetCustomAttribute<RequireAncestorAttribute>()?.RequiredTypes;

        CreateInvokeId = type.GetCustomAttribute<CreateInvokeAttribute>()?.ID;
        RCInvokeId = type.GetCustomAttribute<RCInvokeAttribute>()?.ID;
    }

    public NodeMeta(TreeNode source, Table meta)
        : this(source)
    {
        IsFolder = FindAttribute(meta, "IsFolder", false);
        IsDefinition = FindAttribute(meta, "IsDefinition", false);
        IsLeafNode = FindAttribute(meta, "LeafNode", false);
        CannotBeDeleted = FindAttribute(meta, "CannotBeDeleted", false);
        CannotBeBanned = FindAttribute(meta, "CannotBeBanned", false);
        IgnoreValidation = FindAttribute(meta, "IgnoreValidation", false);
        Unique = FindAttribute(meta, "Unique", false);

        Icon = FindAttribute(meta, "Icon", "Unknown");

        RequireParent = FindAttribute(meta, "RequireParent", Array.Empty<string>());
        RequireAncestor = FindAttribute(meta, "RequireAncestor", Array.Empty<string>());

        Priority = FindAttribute(meta, "Priority", 0);
        CreateInvokeId = FindAttribute(meta, "InvokeID", 0);
        RCInvokeId = FindAttribute(meta, "RCInvoke", 0);
    }

    #region From Lua

    public static bool FindAttribute(Table meta, string attrib, bool defaultValue)
    {
        if (meta == null)
            return defaultValue;

        foreach (DynValue item in meta.Values)
            if (item.Type == DataType.String)
                if (item.String.Equals(attrib, StringComparison.OrdinalIgnoreCase))
                    return true;

        return defaultValue;
    }

    public static string FindAttribute(Table meta, string attrib, string defaultValue)
    {
        if (meta == null)
            return defaultValue;

        foreach (TablePair item in meta.Pairs)
        {
            FromDataType(DataType.String, item, attrib, out DynValue? result);
            if (result != null)
                return item.Value.String;
        }

        return defaultValue;
    }

    public static int FindAttribute(Table meta, string attrib, int defaultValue)
    {
        if (meta == null)
            return defaultValue;

        foreach (TablePair item in meta.Pairs)
        {
            FromDataType(DataType.Number, item, attrib, out DynValue? result);
            if (result != null)
                return (int)item.Value.Number;
        }

        return defaultValue;
    }

    public static string[] FindAttribute(Table meta, string attrib, string[] defaultValue)
    {
        if (meta == null)
            return defaultValue;

        foreach (TablePair item in meta.Pairs)
        {
            FromDataType(DataType.Table, item, attrib, out DynValue? result);
            if (result != null)
            {
                List<string> list = [];
                foreach (DynValue val in result.Table.Values)
                    if (val.Type == DataType.String)
                        list.Add(val.String);
                return [.. list];
            }
        }

        return defaultValue;
    }

    private static void FromDataType(DataType valueDataType, TablePair item, string attrib, out DynValue? result)
    {
        result = null;
        if (item.Key.Type == DataType.String && item.Value.Type == valueDataType)
            if (item.Key.String.Equals(attrib, StringComparison.OrdinalIgnoreCase))
                result = item.Value;
    }

    #endregion
}

public static class AttributeExtensions
{
    public static TValue GetAttributeValue<TAttribute, TValue>(
        this Type type,
        Func<TAttribute, TValue> valueSelector)
        where TAttribute : Attribute
    {
        try
        {
            if (type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() is TAttribute att)
            {
                return valueSelector(att);
            }
            return default;
        }
        catch { return default; }
    }
}