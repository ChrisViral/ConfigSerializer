using System;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader.Attributes;

/// <summary>
/// Config parser attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ConfigParserAttribute(Type targetType) : Attribute
{
    #region Properties
    /// <summary>
    /// Type targeted by this parser
    /// </summary>
    public Type TargetType { get; } = targetType;

    /// <summary>
    /// Parser priority
    /// </summary>
    public int Priority { get; init; } = 0;
    #endregion
}
