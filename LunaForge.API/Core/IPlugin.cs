﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.API.Core;

/// <summary>
/// Base interface for LunaForge plugins. Every plugin needs to implement this class.<br/>
/// There can be only one class implementing this interface by assembly.
/// </summary>
public interface ILunaPlugin : IDisposable
{
    /// <summary>
    /// The plugin's name displayed in the plugin manager.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The plugin's author(s). Every string in the array is a different person.
    /// </summary>
    public string[] Authors { get; }

    /// <summary>
    /// The entry point of the plugin. This method will be used everytime your plugin is loaded/enabled.
    /// </summary>
    public void Initialize();
}