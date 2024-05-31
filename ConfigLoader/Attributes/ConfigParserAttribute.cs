using System;

namespace ConfigLoader.Attributes;

/// <summary>
/// Config parser attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class ConfigParserAttribute : Attribute
{
    #region Properties
    /// <summary>
    /// Type targeted by this parser
    /// </summary>
    public Type TargetType { get; init; }
    /// <summary>
    /// Parser priority
    /// </summary>
    public int Priority { get; init; } = 0;
    #endregion
}
