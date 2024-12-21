using LunaForge.EditorData.Nodes.NodeData;
using LunaForge.EditorData.Project;
using LunaForge.GUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes;

public static class TreeSerializer
{
    public static readonly JsonSerializerSettings TreeNodeSettings =
        new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            SerializationBinder = new NodeTypeBinder()
        };

    public static string SerializeTreeNode(TreeNode node)
    {
        return JsonConvert.SerializeObject(node, typeof(TreeNode), TreeNodeSettings);
    }

    public static TreeNode? DeserializeTreeNode(LunaDefinition def, string node)
    {
        try
        {
            return JsonConvert.DeserializeObject(node, typeof(TreeNode), TreeNodeSettings) as TreeNode;
        }
        catch (JsonSerializationException ex)
        {
            NotificationManager.AddToast("Couldn't parse project file.\nIs your file corrupted?", ToastType.Error);
            return new LuaNode(def, Path.Combine(def.ParentProject.PathToNodeData, "You Shouldn't Even Be Able To See That.lua"));
        }
    }
}