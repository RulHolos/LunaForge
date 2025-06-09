using LunaForge.Editor.Commands;
using LunaForge.Editor.LunaTreeNodes;
using LunaForge.Editor.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Tests.Projects;

public class LunaTreeViewTests
{
    [Fact]
    public void AddNode_IsUnsaved()
    {
        var treeView = new LunaNodeTree();

        treeView.AddNode(new TestTreeNode());

        Assert.True(treeView.IsUnsaved);
    }

    [Fact]
    public void WorkTree_OnNodeAdded_IsRaised()
    {
        var workTree = new WorkTree();
        var node = new TestTreeNode();
        bool eventRaised = false;

        workTree.OnNodeAdded += n =>
        {
            eventRaised = true;
            Assert.Equal(node, n);
        };

        workTree.Add(node);

        Assert.True(eventRaised);
    }

    [Fact]
    public void WorkTree_OnNodeRemoved_IsRaised()
    {
        var workTree = new WorkTree();
        var node = new TestTreeNode();
        workTree.Add(node);
        bool eventRaised = false;

        workTree.OnNodeRemoved += n =>
        {
            eventRaised = true;
            Assert.Equal(node, n);
        };

        workTree.Remove(node);

        Assert.True(eventRaised);
    }
}

public class TestCommand(string name) : Command
{
    public string Name { get; } = name;

    public override void Execute() { }

    public override string ToString() => Name;

    public override void Undo() { }
}

public class TestAttribute : NodeAttribute
{
    public TestAttribute(string name)
        : base(name, Editor.Backend.Enums.NodeEditorWindowType.Boolean, "OldValue")
    {

    }
}