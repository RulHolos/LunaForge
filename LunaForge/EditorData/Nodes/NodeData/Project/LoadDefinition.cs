﻿using LunaForge.EditorData.Nodes.Attributes;
using LunaForge.EditorData.Project;
using LunaForge.EditorData.Traces;
using LunaForge.EditorData.Traces.EditorTraces;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.NodeData.Project;

[NodeIcon("LoadDef")]
[LeafNode]
public class LoadDefinition : TreeNode
{
    [JsonConstructor]
    private LoadDefinition() : base() { }
    public LoadDefinition(LunaDefinition def) : this(def, "", "false") { }
    public LoadDefinition(LunaDefinition def, string filePath, string local) : base(def)
    {
        PathToDefinition = filePath;
        Local = local;
    }

    [JsonIgnore]
    public override string NodeName { get => "Load Definition"; }

    [JsonIgnore, NodeAttribute, DefaultValue("")]
    public string PathToDefinition
    {
        get => CheckAttr(0, "Path to Definition", "definitionFile").AttrValue;
        set => CheckAttr(0, "Path to Definition", "definitionFile").AttrValue = value;
    }

    [JsonIgnore, NodeAttribute, DefaultValue("false")]
    public string Local
    {
        get => CheckAttr(1, "Local variable", "bool").AttrValue;
        set => CheckAttr(1, "Local variable", "bool").AttrValue = value;
    }

    public override string ToString()
    {
        string local = GetAttribute(1) == "true" ? "local " : "";
        return $"Load {local}Definition from \"{Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, GetAttribute(0))}\"";
    }

    public override IEnumerable<string> ToLua(int spacing)
    {
        string sp = Indent(spacing);
        string local = GetAttribute(1) == "true" ? "local " : "";
        yield return sp + $"{local}last_definition = Include('{Path.ChangeExtension(
            Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, GetAttribute(0) ?? string.Empty), ".lua")
            .Replace("\\", "/")}')\n";
    }

    public override object Clone()
    {
        LoadDefinition cloned = new(ParentDef);
        cloned.CopyData(this);
        return cloned;
    }

    public override List<EditorTrace> GetTraces()
    {
        List<EditorTrace> traces = [];
        if (string.IsNullOrEmpty(GetAttribute(0)))
            traces.Add(new ArgNotNullTrace(this, GetAttr(0).AttrName));
        if (!string.IsNullOrEmpty(GetAttribute(0)) && !File.Exists(Path.Combine(ParentDef.ParentProject.PathToProjectRoot, GetAttribute(0))))
            traces.Add(new FileMustExistTrace(this, GetAttr(0).AttrName));
        return traces;
    }

    /*
    public override void OnCreateNodeImpl(OnCreateEventArgs e)
    {
        AddCache(GetAttribute(0), ParentDef.FullFilePath);
    }

    public override void OnAttributeChangedImpl(NodeAttribute attr, AttributeChangedEventArgs e)
    {
        RemoveCache(e.OriginalValue, ParentDef.FullFilePath);
        AddCache(e.NewValue, ParentDef.FullFilePath);
    }

    public override void OnRemoveNodeImpl(OnRemoveEventArgs e)
    {
        RemoveCache(GetAttribute(0), ParentDef.FullFilePath);
    }

    private void AddCache(string pathToDef, string pathToSource)
    {
        if (string.IsNullOrEmpty(pathToDef) || !File.Exists(pathToDef))
            return;
        DefinitionsCache defCache = ParentDef.ParentProject.DefCache;
        string relativeDefPath = Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, pathToDef);
        string relativeSrcPath = Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, pathToSource);
        if (!defCache.DefinitionExistsInCache(relativeDefPath))
            defCache.AddToCache(relativeDefPath, relativeSrcPath);
        else
            defCache.AddAccessibleFrom(relativeDefPath, relativeSrcPath);
    }

    private void RemoveCache(string pathToDef, string pathToSource)
    {
        if (string.IsNullOrEmpty(pathToDef) || !File.Exists(pathToDef))
            return;
        DefinitionsCache defCache = ParentDef.ParentProject.DefCache;
        string relativeDefPath = Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, pathToDef);
        string relativeSrcPath = Path.GetRelativePath(ParentDef.ParentProject.PathToProjectRoot, pathToSource);
        if (defCache.DefinitionExistsInCache(relativeDefPath))
            defCache.RemoveAccessibleFrom(relativeDefPath, relativeSrcPath);
    }
    */
}
