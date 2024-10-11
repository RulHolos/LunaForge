using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Nodes.Attributes;
using LunaForge.EditorData.Toolbox;
using LunaForge.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Plugins.Services;

public interface IToolboxService
{
    public bool RegisterNodePickerRegister<T>(string name);

    public bool Insert(TreeNode node, bool doInvoke = true);
}

internal class ToolboxService : IToolboxService
{
    public bool RegisterNodePickerRegister<T>(string name)
    {
        Type pluginNodeType = typeof(T);
        /*if (typeof(NodePickerRegister).IsAssignableFrom(pluginNodeType))
        {
            try
            {
                MainWindow.ToolboxWin.NodePickerBox.AddRegister((NodePickerRegister)Activator.CreateInstance(pluginNodeType, args: [new NodePickerTab(name)]));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                NotificationManager.AddToast($"Couldn't register node register \"{pluginNodeType.Name}\".", ToastType.Error);
                return false;
            }
        }*/
        return false;
    }

    public bool Insert(TreeNode node, bool doInvoke = true)
    {
        return MainWindow.Insert(node, doInvoke);
    }
}
