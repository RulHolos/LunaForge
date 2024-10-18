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
    public Script? Script => GetScript();

    [JsonIgnore]
    public bool InvalidNode { get; set; } = false;

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
     * Allow node plugins to be turned on and off. - Check
     * 
     * Put PathToLua to be relative to the project root.
     * 
     * For the ToLua: Use threads and split all children into separate buffers given to the thread. They're put back together after
     */

    public Script GetScript()
    {
        return NodeScript.CreateScript(this);
    }

    public override string ToString()
    {
        try
        {
            if (InvalidNode)
            {
                return $"--- Invalid Node: {Path.GetFileName(PathToLua)} ---";
            }
            // Set script.Globals dynamically this context being this
            NodeScript.SetScriptToString(Script, this);
            return Script?.Call(Script.Globals.Get("ToString")).String;
        }
        catch (ScriptRuntimeException ex)
        {
            Console.WriteLine(ex.DecoratedMessage);
            return $"--- Invalid Node: {Path.GetFileName(PathToLua)} (Error: Report to console.) ---";
        }
    }

    public void CreateScript()
    {
        NodeScript.CreateScript(this);
        if (Script?.Globals["Initialize"] != null)
            Script.Call(Script.Globals.Get("Initialize"));
    }

    public IEnumerable<string> GetChildrenLua(int spacing)
    {
        foreach (var a in base.ToLua(spacing))
            yield return a;
    }

    public IEnumerable<string> GetChildrenLuaFromPriority(int spacing)
    {
        foreach (var a in base.ToLua(spacing, FromPriority()))
            yield return a;
    }

    public override IEnumerable<string> ToLua(int spacing)
    {
        NodeScript.SetScriptToLua(Script, this);
        if (Script?.Globals["ToLua"] != null)
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

    public override void ReflectAttr(NodeAttribute o, AttributeChangedEventArgs e)
    {
        try
        {
            NodeScript.SetScriptReflectAttr(Script, this);
            DynValue? func = Script?.Globals.Get("ReflectAttr");
            if (func?.Function != null)
            {
                Script.Call(func, o.AttrName, o.AttrValue, e.OriginalValue);
            }
        }
        catch (NullReferenceException) { }
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
