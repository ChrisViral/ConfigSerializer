using System;

namespace ConfigLoader.Attributes;

/// <summary>
/// Config parser attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class ConfigParserAttribute : Attribute
{
    /// <summary>
    /// Type targeted by this parser
    /// </summary>
    public Type TargetType { get; set; }
}
