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

    public Type[] RequireParent { get; } = null;
    public Type[][] RequireAncestor { get; } = null;

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

        RequireParent = GetTypes(type.GetCustomAttribute<RequireParentAttribute>()?.ParentType);
        var attrs = type.GetCustomAttributes<RequireAncestorAttribute>();
        if (attrs.Count() != 0)
        {
            RequireAncestor = (from RequireAncestorAttribute at in attrs
                               select GetTypes(at.RequiredTypes)).ToArray();
        }

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

        Priority = FindAttribute(meta, "Priority", 0);

        // TODO: RequireParent, RequireAncestor from the Name of the node, not the type (cuz it's all the same type).
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

    private static void FromDataType(DataType valueDataType, TablePair item, string attrib, out DynValue? result)
    {
        result = null;
        if (item.Key.Type == DataType.String && item.Value.Type == valueDataType)
            if (item.Key.String.Equals(attrib, StringComparison.OrdinalIgnoreCase))
                result = item.Value;
    }

    #endregion

    public static Type[] GetTypes(Type[] src)
    {
        if (src != null)
        {
            LinkedList<Type> types = new LinkedList<Type>();
            Type it = typeof(IEnumerable<Type>);
            foreach (Type t in src)
            {
                if (it.IsAssignableFrom(t))
                {
                    IEnumerable<Type> o = t.GetConstructor(Type.EmptyTypes).Invoke([]) as IEnumerable<Type>;
                    foreach (Type ty in o)
                    {
                        types.AddLast(ty);
                    }
                }
                else
                {
                    types.AddLast(t);
                }
            }
            return types.ToArray();
        }
        return null;
    }
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
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }
        catch { return default(TValue); }
    }
}