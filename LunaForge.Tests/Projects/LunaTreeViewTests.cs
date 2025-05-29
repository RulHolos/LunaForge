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

    [Fact]
    public void CommandHistory_CanUndoAndRedo()
    {
        var history = new CommandHistory();
        var command1 = new TestCommand("Command 1");
        var command2 = new TestCommand("Command 2");

        history.AddAndExecuteCommand(command1);

        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);

        history.AddAndExecuteCommand(command2);

        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);

        history.Undo();

        Assert.True(history.CanRedo);
        Assert.Equal(1, history.UndoCount);
        Assert.Equal(1, history.RedoCount);

        history.Redo();

        Assert.False(history.CanRedo);
    }

    [Fact]
    public void NodeAttributes_NodeAttribute_IsRaised()
    {
        var node = new TestTreeNode();
        bool eventRaised = false;
        node.Attributes.Add(new TestAttribute("TestAttribute"));

        NodeAttribute.OnNodeAttributeChanged += (attr, args) =>
        {
            eventRaised = true;
            Assert.Equal("TestAttribute", attr.Name);
            Assert.Equal("OldValue", args.OldValue);
            Assert.Equal("NewValue", args.NewValue);
        };

        node.Attributes[0].EditAttr("NewValue");

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