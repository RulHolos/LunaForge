using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Attributes;

/// <summary>
/// ONLY USE FOR ENUMS!
/// </summary>
/// <param name="category"></param>
/// <param name="defaultValue"></param>
/// <param name="defaultValueType"></param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class BaseConfigAttribute(ConfigSystemCategory category, object defaultValue, Type defaultValueType) : Attribute
{
    public ConfigSystemCategory Category { get; } = category;
    public object DefaultValue { get; } = defaultValue;
    public Type DefaultValueType { get; } = defaultValueType;

    public BaseConfigAttribute(ConfigSystemCategory category, object defaultValue)
        : this(category, defaultValue, defaultValue.GetType())
    { }
}
