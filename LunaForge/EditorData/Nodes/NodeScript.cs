using LunaForge.EditorData.Nodes.NodeData;
using LunaForge.EditorData.Traces;
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
    // TODO: Unload script cache for the current project at project closure.
    public static Script CreateScript(LuaNode context)
    {
        if (!File.Exists(context.PathToLuaFull))
        {
            context.CheckTrace();
            context.InvalidNode = true;
            NotificationManager.AddToast($"Cannot load a node.\nCheck console for more infos.", ToastType.Error);
            Console.WriteLine($"Path to node {context.PathToLuaFull} doesn't exist.");
            return null;
        }
        var s = context.PathToLuaRelative;
        context.NodeName = context.ParentDef.ParentProject.Toolbox.LookupNameFromPath(context.PathToLuaRelative);

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
                script.DoFile(context.PathToLuaFull);
                MainWindow.ScriptCache.TryAdd(context.NodeName, script);
                context.InvalidNode = false;
                context.CheckTrace();
            }
            catch (InterpreterException ex)
            {
                context.CheckTrace();
                NotificationManager.AddToast($"Cannot load node: {Path.GetFileName(context.PathToLuaRelative)}.\nCheck console for more infos.", ToastType.Error);
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
        script.Globals["IsNullOrEmpty"] = (Func<string, bool>)((str) => { return string.IsNullOrEmpty(str); });
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
        script.Globals["IsNullOrEmpty"] = (Func<string, bool>)((str) => { return string.IsNullOrEmpty(str); });
        script.Globals["GetDefinitionName"] = (Func<string>)context.GetParentDefinitionClassName;
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
        script.Globals["IsNullOrEmpty"] = (Func<string, bool>)((str) => { return string.IsNullOrEmpty(str); });
    }

    public static void SetScriptCheckTrace(Script script, LuaNode context)
    {
        script.Globals["this"] = context;
        script.Globals["INFO"] = TraceSeverity.Info;
        script.Globals["WARNING"] = TraceSeverity.Warning;
        script.Globals["ERROR"] = TraceSeverity.Error;
        script.Globals["AddTrace"] = (Action<bool, string, string>)context.AddTrace;
        script.Globals["IsNullOrEmpty"] = (Func<string, bool>)((str) => { return string.IsNullOrEmpty(str); });
    }
}
