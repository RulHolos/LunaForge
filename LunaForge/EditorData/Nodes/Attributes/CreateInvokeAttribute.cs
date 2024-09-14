﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Nodes.Attributes;

/// <summary>
/// Identify a <see cref="TreeNode"/> invoke to edit an <see cref="NodeAttribute"/> when create.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CreateInvokeAttribute(int id) : Attribute
{
    public int ID = id;
}