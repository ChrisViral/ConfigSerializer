using System;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader.Attributes;

/// <summary>
/// Attribute used to mark fields and properties that can be loaded via ConfigObject
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ConfigFieldAttribute : Attribute
{
    #region Properties
    /// <summary>
    /// The name of the field in the config, if left blank, the name of the code field is used
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// If this config field is required to exist during deserialization
    /// </summary>
    public bool Required { get; init; } = false;
    /// <summary>
    /// How loading of array values is handled, defaults to <see cref="ArrayHandling.SINGLE_VALUE"/>;
    /// </summary>
    public ArrayHandling? ArrayHandling { get; init; }
    /// <summary>
    /// Separator string for array types
    /// </summary>
    public char? ArraySeparator { get; init; }
    #endregion
}