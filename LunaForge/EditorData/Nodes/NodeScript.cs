using LunaForge.EditorData.Nodes.NodeData;
using LunaForge.GUI;
using Microsoft.VisualBasic;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes;

internal static class NodeScript
{
    public static Script CreateScript(LuaNode context)
    {
        if (!File.Exists(context.PathToLua))
        {
            context.CheckTrace();
            context.InvalidNode = true;
            NotificationManager.AddToast($"Cannot load a node.\nCheck console for more infos.", ToastType.Error);
            Console.WriteLine($"Path to node {context.PathToLua} doesn't exist.");
            return null;
        }
        context.NodeName = context.ParentDef.ParentProject.Toolbox.LookupNameFromPath(context.PathToLua);

        // Set script to a reference of the script cache.
        if (MainWindow.ScriptCache.TryGetValue(context.NodeName, out Script scriptRef))
        {
            // TODO: Wrapped class for managing instances of "this".
            SetScriptInitial(scriptRef, context);
            return scriptRef;
        }
        else
        {
            // Removes LoadMethods, OS_System and IO.
            Script script = new(CoreModules.Preset_SoftSandbox);
            SetScriptInitial(script, context);
            try
            {
                script.DoFile(context.PathToLua);
                MainWindow.ScriptCache.TryAdd(context.NodeName, script);
                context.InvalidNode = false;
                context.CheckTrace();
            }
            catch (InterpreterException ex)
            {
                context.CheckTrace();
                NotificationManager.AddToast($"Cannot load node: {Path.GetFileName(context.PathToLua)}.\nCheck console for more infos.", ToastType.Error);
                Console.WriteLine($"Problem with loading lua script: {ex.DecoratedMessage}");
                context.InvalidNode = true;
            }

            return script;
        }
    }

    // TODO: Argument exception, key was already set.

    public static void SetScriptToString(Script script, LuaNode context)
    {
        script.Globals["GetChildrenLua"] = (Func<int, IEnumerable<string>>)context.GetChildrenLua;
        script.Globals["GetChildrenLuaFromPriority"] = (Func<int, IEnumerable<string>>)context.GetChildrenLuaFromPriority;
        script.Globals["GetAttribute"] = (Func<string, string>)context.GetAttribute;
        script.Globals["GetAttributeInt"] = (Func<int, string>)context.GetAttribute;
    }

    public static void SetScriptInitial(Script script, LuaNode context)
    {
        script.Globals["SetAttribute"] = (Action<string, string>)context.SetAttribute;
        script.Globals["SetupAttribute"] = (Func<string, string, string, string>)context.SetupAttribute;
        script.Globals["SetupDependencyAttribute"] = (Func<string, string, string, string>)context.SetupDependencyAttribute;
        script.Globals["SetupMeta"] = (Action<Table>)context.SetupMeta;
        script.Globals["GetAttribute"] = (Func<string, string>)context.GetAttribute;
        script.Globals["GetAttributeInt"] = (Func<int, string>)context.GetAttribute;
        script.Globals["Indent"] = (Func<int, string>)context.Indent;
        //script.Globals["AddTrace"] = (Action<bool>)AddTraceWithCondition;
    }

    public static void SetScriptToLua(Script script, LuaNode context)
    {
        SetScriptToString(script, context);
        script.Globals["SetAttribute"] = (Action<string, string>)context.SetAttribute;
        script.Globals["AddAttribute"] = (Func<string, string, string, string>)context.AddAttribute;
        script.Globals["RemoveAttribute"] = (Action<string>)context.RemoveAttribute;
        script.Globals["RemoveAttributeInt"] = (Action<int>)context.RemoveAttribute;
        script.Globals["HideAttribute"] = (Action<int>)context.HideAttribute;
        script.Globals["GetUsedAttrCount"] = (Func<int>)context.GetUsedAttrCount;
    }

    public static void SetScriptReflectAttr(Script script, LuaNode context)
    {
        SetScriptToString(script, context);
        script.Globals["SetAttribute"] = (Action<string, string>)context.SetAttribute;
        script.Globals["AddAttribute"] = (Func<string, string, string, string>)context.AddAttribute;
        script.Globals["RemoveAttribute"] = (Action<string>)context.RemoveAttribute;
        script.Globals["RemoveAttributeInt"] = (Action<int>)context.RemoveAttribute;
        script.Globals["HideAttribute"] = (Action<int>)context.HideAttribute;
        script.Globals["GetUsedAttrCount"] = (Func<int>)context.GetUsedAttrCount;
    }
}
