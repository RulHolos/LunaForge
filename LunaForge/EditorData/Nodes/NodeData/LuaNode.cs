using LunaForge.EditorData.Project;
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

    [JsonConstructor]
    private LuaNode() : base() { }
    public LuaNode(string pathToLua)
        : base()
    {
        PathToLua = pathToLua;
        CreateScript();
    }

    [JsonProperty("PathToLua")]
    public string PathToLua { get; set; } = string.Empty;

    public override string ToString() => "Root";

    public Script CreateScript()
    {
        if (Script != null)
            return Script;
        Script script = new();
        script.Globals["this"] = this;
        script.Globals["GetChildrenLua"] = (Func<int, IEnumerable<string>>)GetChildrenLua;
        try
        {
            script.DoFile(PathToLua);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Problem with loading lua script: {ex}");
        }

        Script = script;
        return script;
    }

    public IEnumerable<string> GetChildrenLua(int spacing)
    {
        Console.WriteLine("Getting children");
        foreach (var children in base.ToLua(spacing + 1))
            yield return children;
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
                    yield return Indent(spacing) + result.String;
                }

                // Resume the coroutine to get the next result
                GetCoroutineResult(ref coroutine, ref result);
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
            result = null;
        }
    }

    public override object Clone()
    {
        LuaNode node = new(PathToLua);
        node.CopyData(this);
        return node;
    }
}
