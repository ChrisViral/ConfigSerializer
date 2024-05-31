using System;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader;

/// <summary>
/// Exception thrown when a config field is expected but not found
/// </summary>
public class MissingConfigFieldException : ArgumentException
{
    #region Constructors
    /// <summary>
    /// Creates a new <see cref="MissingConfigFieldException"/>
    /// </summary>
    public MissingConfigFieldException() { }

    /// <summary>
    /// Creates a new <see cref="MissingConfigFieldException"/> with the given message
    /// </summary>
    /// <param name="message">Exception message</param>
    public MissingConfigFieldException(string message) : base(message) { }

    /// <summary>
    /// Creates a new <see cref="MissingConfigFieldException"/> with the given message and missing field name
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="fieldName">Name of the missing field</param>
    public MissingConfigFieldException(string message, string fieldName) : base(message, fieldName) { }
    #endregion
}