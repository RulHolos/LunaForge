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
        PathToLuaRelative = GetRelativePathToLua(pathToLua);
    }

    [JsonIgnore]
    public string PathToLuaFull => Path.Combine(ParentDef.ParentProject.PathToNodeData, PathToLuaRelative);
    [JsonProperty("PathToLua")]
    public string PathToLuaRelative { get; set; } = string.Empty;

    public string GetRelativePathToLua(string fullPath) => Path.GetRelativePath(ParentDef.ParentProject.PathToNodeData, fullPath);

    /*
     * TODO:
     * Proper error handling (ToString, ToLua traced error, ReflectAttr) - Check
     * RequireParent & RequireAncestor - Check
     * Throw traces.
     * Allow node plugins to be turned on and off. - Check
     * 
     * Put PathToLua to be relative to the project root.
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
                return $"--- Invalid Node: {Path.GetFileName(PathToLuaFull)} ---";
            }
            NodeScript.SetScriptToString(Script, this);
            DynValue? ToStringFunc = Script?.Globals.Get("ToString");
            if (ToStringFunc.Function != null)
                return Script?.Call(Script.Globals.Get("ToString")).String;
            return $"--- {NodeName} (No ToString method) ---";
        }
        catch (ScriptRuntimeException ex)
        {
            Console.WriteLine(ex.DecoratedMessage);
            return $"--- Invalid Node: {Path.GetFileName(PathToLuaFull)} (Error: Report to console.) ---";
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
        if (Script?.Globals["ToLua"] != null)
        {
            NodeScript.SetScriptToLua(Script, this);
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
            if (args != null && args.Length > 0)
                result = co.Resume(args);
            else
                result = co.Resume();
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
            DynValue? func = Script?.Globals.Get("ReflectAttr");
            if (func?.Function != null)
            {
                NodeScript.SetScriptReflectAttr(Script, this);
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
        LuaNode node = new(ParentDef, PathToLuaFull);
        node.CopyData(this);
        return node;
    }

    #region Traces

    public static Dictionary<string, Type> TraceTypes { get; } = new()
    {
        ["ArgNotNull"] = typeof(ArgNotNullTrace),
        ["FileMustExist"] = typeof(FileMustExistTrace),
        ["ProjectPathNotNull"] = typeof(ProjectPathNotNullTrace)
    };

    public List<EditorTrace> TempTraces;

    public override List<EditorTrace> GetTraces()
    {
        List<EditorTrace> traces = [];
        TempTraces = [];
        try
        {
            DynValue? func = Script?.Globals.Get("GetTraces");
            if (func?.Function != null)
            {
                NodeScript.SetScriptCheckTrace(Script, this);
                Script.Call(func);
            }
        }
        catch (ScriptRuntimeException ex)
        {
            Console.WriteLine(ex.DecoratedMessage);
        }

        if (InvalidNode)
            traces.Add(new InvalidNodeTrace(this));

        foreach (EditorTrace trace in TempTraces)
            traces.Add(trace);

        return traces;
    }

    public void AddTrace(bool condition, string type, params string[] args)
    {
        if (condition == false)
            return; // Doesn't do anything if condition is false.
        TraceTypes.TryGetValue(type, out Type traceType);
        if (traceType == null)
        {
            Console.WriteLine($"Type {type} is not a valid trace type. See documentation for a list of valid traces.");
        }
        EditorTrace trace = (EditorTrace)Activator.CreateInstance(traceType, args);
        if (trace != null)
            TempTraces.Add(trace);
    }

    #endregion
}
