using LunaForge.EditorData.Project;
using LunaForge.EditorData.Traces.EditorTraces;
using LunaForge.EditorData.Traces;
using LunaForge.GUI;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.NodeData;

internal class LuaNode : TreeNode
{
    [JsonIgnore]
    public Script Script { get; private set; }

    [JsonIgnore]
    public bool InvalidNode { get; private set; } = false;

    [JsonConstructor]
    private LuaNode() : base() { }
    public LuaNode(LunaDefinition def, string pathToLua)
        : base(def)
    {
        PathToLua = pathToLua;
    }

    [JsonProperty("PathToLua")]
    public string PathToLua { get; set; } = string.Empty;

    /*
     * TODO:
     * Proper error handling (ToString, ToLua traced error, ReflectAttr) - Check
     * RequireParent & RequireAncestor - Check
     * Throw traces.
     * Allow node plugins to be turned on and off.
     */

    public override string ToString()
    {
        try
        {
            if (InvalidNode)
            {
                return $"--- Invalid Node: {Path.GetFileName(PathToLua)} ---";
            }
            return Script?.Call(Script.Globals.Get("ToString")).String;
        }
        catch (ScriptRuntimeException ex)
        {
            Console.WriteLine(ex.DecoratedMessage);
            return $"--- Invalid Node: {Path.GetFileName(PathToLua)} (Error: Report to console.) ---";
        }
    }

    public Script CreateScript()
    {
        if (Script != null)
            return Script;
        if (!File.Exists(PathToLua))
        {
            CheckTrace();
            InvalidNode = true;
            NotificationManager.AddToast($"Cannot load a node.\nCheck console for more infos.", ToastType.Error);
            Console.WriteLine($"Path to node {PathToLua} doesn't exist.");
            return null;
        }
        Script script = new();
        SetScriptGlobals(ref script);
        try
        {
            script.DoFile(PathToLua);
            NodeName = ParentDef.ParentProject.Toolbox.LookupNameFromPath(PathToLua);
            InvalidNode = false;
            CheckTrace();
        }
        catch (InterpreterException ex)
        {
            CheckTrace();
            NotificationManager.AddToast($"Cannot load node: {Path.GetFileName(PathToLua)}.\nCheck console for more infos.", ToastType.Error);
            Console.WriteLine($"Problem with loading lua script: {ex.DecoratedMessage}");
            InvalidNode = true;
        }

        Script = script;
        return script;
    }

    private void SetScriptGlobals(ref Script script)
    {
        script.Globals["this"] = this;
        script.Globals["GetChildrenLua"] = (Func<int, IEnumerable<string>>)GetChildrenLua;
        script.Globals["GetChildrenLuaFromPriority"] = (Func<int, IEnumerable<string>>)GetChildrenLuaFromPriority;
        script.Globals["GetAttribute"] = (Func<string, string>)GetAttribute;
        script.Globals["GetAttributeInt"] = (Func<int, string>)GetAttribute;
        script.Globals["SetAttribute"] = (Action<string, string>)SetAttribute;
        script.Globals["Macrolize"] = (Func<int, string>)Macrolize;
        script.Globals["NonMacrolize"] = (Func<int, string>)NonMacrolize;
        script.Globals["SetupAttribute"] = (Func<string, string, string, string>)SetupAttribute;
        script.Globals["SetupDependencyAttribute"] = (Func<string, string, string, string>)SetupDependencyAttribute;
        script.Globals["SetupMeta"] = (Action<Table>)SetupMeta;
        script.Globals["Indent"] = (Func<int, string>)Indent;
        script.Globals["AddAttribute"] = (Func<string, string, string, string>)AddAttribute;
        script.Globals["RemoveAttribute"] = (Action<string>)RemoveAttribute;
        script.Globals["RemoveAttributeInt"] = (Action<int>)RemoveAttribute;
        script.Globals["HideAttribute"] = (Action<int>)HideAttribute;
        script.Globals["GetUsedAttrCount"] = (Func<int>)GetUsedAttrCount;
        //script.Globals["AddTrace"] = (Action<bool>)AddTraceWithCondition;
    }

    private IEnumerable<string> GetChildrenLua(int spacing)
    {
        foreach (var a in base.ToLua(spacing))
            yield return a;
    }

    private IEnumerable<string> GetChildrenLuaFromPriority(int spacing)
    {
        foreach (var a in base.ToLua(spacing, FromPriority()))
            yield return a;
    }

    // TODO : Return a compilation error if this function fails at any point.
    public override IEnumerable<string> ToLua(int spacing)
    {
        if (Script.Globals["ToLua"] != null)
        {
            Coroutine coroutine = null;
            try
            {
                DynValue function = Script.Globals.Get("ToLua");
                coroutine = Script.CreateCoroutine(function).Coroutine;
            }
            catch (InterpreterException ex)
            {
                InvalidNode = true;
                CheckTrace();
                NotificationManager.AddToast($"Couldn't generated code from node {GetType().Name}, see console for more infos.", ToastType.Error);
                Console.WriteLine(ex.DecoratedMessage);
            }

            DynValue result = null;
            GetCoroutineResult(ref coroutine, ref result, spacing);

            while (result != null && result.Type != DataType.Void && result.Type != DataType.Nil && coroutine != null)
            {
                if (result.Type == DataType.String)
                {
                    yield return $"{result.String}";
                }

                GetCoroutineResult(ref coroutine, ref result, spacing);
            }
        }
    }

    private void GetCoroutineResult(ref Coroutine co, ref DynValue result, params object[] args)
    {
        try
        {
            result = co.Resume(args);
        }
        catch (ScriptRuntimeException ex)
        {
            InvalidNode = true;
            CheckTrace();
            NotificationManager.AddToast($"Couldn't generated code from node {GetType().Name}, see console for more infos.", ToastType.Error);
            Console.WriteLine(ex.DecoratedMessage);
            result = null;
        }
    }

    public override void ReflectAttr(NodeAttribute o, DependencyAttributeChangedEventArgs e)
    {
        try
        {
            DynValue func = Script.Globals.Get("ReflectAttr");
            if (func.Function != null)
            {
                Script.Call(func, o.AttrName, o.AttrValue, e.OriginalValue);
            }
        }
        catch (ScriptRuntimeException ex)
        {
            Console.WriteLine(ex.DecoratedMessage);
            NotificationManager.AddToast($"Couldn't reflect {NodeName}.\nCheck console for more infos.", ToastType.Error);
        }
    }

    public override object Clone()
    {
        LuaNode node = new(ParentDef, PathToLua);
        node.CopyData(this);
        return node;
    }

    public override List<EditorTrace> GetTraces()
    {
        List<EditorTrace> traces = [];
        if (InvalidNode)
            traces.Add(new InvalidNodeTrace(this));
        return traces;
    }
}
