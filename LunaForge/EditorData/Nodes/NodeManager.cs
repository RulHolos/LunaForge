using LunaForge.EditorData.Nodes.NodeData.Stages;
using LunaForge.EditorData.Project;
using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes;

internal static class NodeManager
{
    public static Dictionary<string, AddDefNode> DefinitionNodes { get; set; } = [];

    public delegate void AddDefNode(LunaDefinition def);

    public static void RegisterDefinitionNodes()
    {
        DefinitionNodes.Add("Stage Group", new AddDefNode(AddDefNode_StageGroup));
        DefinitionNodes.Add("Main Menu", new AddDefNode(AddDefNode_MainMenu));
        DefinitionNodes.Add("Empty", new AddDefNode(AddDefNode_Folder));
    }

    #region Add Nodes

    private static void AddDefNode_StageGroup(LunaDefinition def)
    {
        TreeNode node = new StageGroupDefinition(def);
        node.IsExpanded = true;
        def.TreeNodes[0] = node;
    }

    private static void AddDefNode_MainMenu(LunaDefinition def)
    {
        TreeNode node = new MainMenuDefinition(def);
        node.AddChild(new MainMenuInit(def));
        node.AddChild(new MainMenuFrame(def));
        node.IsExpanded = true;
        def.TreeNodes[0] = node;
    }

    private static void AddDefNode_Folder(LunaDefinition def)
    {
        //TreeNode node = new Folder(def);
    }

    #endregion
}
