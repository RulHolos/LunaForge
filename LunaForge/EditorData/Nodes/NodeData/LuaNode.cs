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
        CreateScript();
    }

    [JsonProperty("PathToLua")]
    public string PathToLua { get; set; } = string.Empty;

    public override string ToString()
    {
        CreateScript();
        if (InvalidNode)
        {
            CheckTrace();
            return $"--- Invalid Node: {PathToLua} ---";
        }        
        return Script?.Call(Script.Globals.Get("ToString")).String;
    }

    public Script CreateScript()
    {
        if (Script != null)
            return Script;
        if (!File.Exists(PathToLua))
        {
            InvalidNode = true;
            Console.WriteLine($"Problem with loading lua script: {PathToLua}");
            return null;
        }
        Script script = new();
        script.Globals["this"] = this;
        script.Globals["GetChildrenLua"] = (Func<int, IEnumerable<string>>)GetChildrenLua;
        script.Globals["GetChildrenLuaFromPriority"] = (Func<int, IEnumerable<string>>)GetChildrenLuaFromPriority;
        script.Globals["GetAttribute"] = (Func<string, string>)GetAttribute;
        script.Globals["SetAttribute"] = (Action<string, string>)SetAttribute;
        script.Globals["Macrolize"] = (Func<int, string>)Macrolize;
        script.Globals["NonMacrolize"] = (Func<int, string>)NonMacrolize;
        script.Globals["SetupAttribute"] = (Func<string, string, string, string>)SetupAttribute;
        script.Globals["SetupMeta"] = (Action<Table>)SetupMeta;
        script.Globals["Indent"] = (Func<int, string>)Indent;
        try
        {
            script.DoFile(PathToLua);
            RemoveUnusedAttributes();
            InvalidNode = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Problem with loading lua script: {ex}");
            InvalidNode = true;
        }

        Script = script;
        return script;
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

    // TODO : Return a compilation error if this function fails at any point.
    public override IEnumerable<string> ToLua(int spacing)
    {
        CreateScript();
        if (Script.Globals["ToLua"] != null)
        {
            DynValue function = Script.Globals.Get("ToLua");
            Coroutine coroutine = Script.CreateCoroutine(function).Coroutine;

            DynValue result = null;
            GetCoroutineResult(ref coroutine, ref result, spacing);

            while (result != null && result.Type != DataType.Void && result.Type != DataType.Nil)
            {
                if (result.Type == DataType.String)
                {
                    yield return $"{result.String}";
                }

                GetCoroutineResult(ref coroutine, ref result, spacing);
            }
        }
    }

    public void GetCoroutineResult(ref Coroutine co, ref DynValue result, params object[] args)
    {
        try
        {
            result = co.Resume(args);
        }
        catch (Exception ex)
        {
            NotificationManager.AddToast($"Couldn't generated code from node {GetType().Name}, see console for more infos.", ToastType.Error);
            Console.WriteLine(ex.ToString());
            result = null;
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
