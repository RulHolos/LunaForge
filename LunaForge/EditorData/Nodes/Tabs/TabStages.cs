using LunaForge.EditorData.Toolbox;
using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LunaForge.EditorData.Toolbox.NodePicker;

namespace LunaForge.EditorData.Nodes.Tabs;

public class TabStages
{
    public NodePickerTab RegisterTab()
    {
        //Tab.AddNode(new NodePickerItem("definestagegroup", "StageGroup.png", "Define Stage Group", new AddNode(AddNode_DefineStageGroup)));

        return null;
    }

    #region Delegates

    private void AddNode_DefineStageGroup()
    {
        //MainApp.Insert(new DefineDifficulty(MainApp.CurrentWorkspace));
    }

    #endregion
}