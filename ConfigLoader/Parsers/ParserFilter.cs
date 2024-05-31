using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ConfigLoader.Attributes;
using ConfigLoader.Extensions;

namespace ConfigLoader.Parsers;

/// <summary>
/// Parser type filter
/// </summary>
/// <typeparam name="TParser">Parser type</typeparam>
public class ParserFilter<TParser> where TParser : IConfigParserBase
{
    /// <summary>
    /// Parser data
    /// </summary>
    /// <param name="Parser">Parser instance</param>
    /// <param name="Attribute">Parser attribute</param>
    public record struct ParserData(TParser Parser, ConfigParserAttribute Attribute);

    #region Properties
    /// <summary>
    /// Parsers keyed by parser type
    /// </summary>
    public ReadOnlyDictionary<Type, ParserData> ParsersByType { get; }
    /// <summary>
    /// Parsers keyed by target type
    /// </summary>
    public ReadOnlyDictionary<Type, ParserData> ParsersByTargetType { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new ParserFilter from a set of valid types based on the parser subtype
    /// </summary>
    /// <param name="parserTypes">Enumerable of valid parser types</param>
    internal ParserFilter(IEnumerable<Type> parserTypes)
    {
        Dictionary<Type, ParserData> parsersByType       = [];
        Dictionary<Type, ParserData> parsersByTargetType = [];

        foreach (Type parserType in parserTypes.Where(t => typeof(TParser).IsAssignableFrom(t)))
        {
            ConfigParserAttribute attribute = parserType.GetCustomAttribute<ConfigParserAttribute>();
            TParser parser = Activator.CreateInstance<TParser>();
            ParserData data = new(parser, attribute);

            parsersByType.Add(parserType, data);
            parsersByTargetType.Add(attribute.TargetType, data);
        }

        this.ParsersByType       = new ReadOnlyDictionary<Type, ParserData>(parsersByType);
        this.ParsersByTargetType = new ReadOnlyDictionary<Type, ParserData>(parsersByTargetType);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Tries to get a parser for the given parser type
    /// </summary>
    /// <param name="parserType">Parser type to find</param>
    /// <param name="parser">Found parser instance, uninitialized if no match found</param>
    /// <returns><see langword="true"/> if a valid parser has been found, otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parserType"/> is <see langword="null"/></exception>
    public bool TryGetParserByType(Type parserType, out TParser? parser)
    {
        if (parserType is null) throw new ArgumentNullException(nameof(parserType), "Parser type to get cannot be null");

        if (this.ParsersByType.TryGetValue(parserType, out ParserData data))
        {
            parser = data.Parser;
            return true;
        }

        parser = default;
        return false;
    }

    /// <summary>
    /// Tries to find the best parser for a given target type
    /// </summary>
    /// <param name="targetType">Target type to parse to</param>
    /// <param name="parser">Found parser instance, uninitialized if no match found</param>
    /// <returns><see langword="true"/> if a valid parser has been found, otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType"/> is <see langword="null"/></exception>
    public bool TryFindBestParserForType(Type targetType, out TParser? parser)
    {
        if (targetType is null) throw new ArgumentNullException(nameof(targetType), "Target type to match cannot be null");

        // If an exact match exists, use it automatically
        if (this.ParsersByType.TryGetValue(targetType, out ParserData data))
        {
            parser = data.Parser;
            return true;
        }

        parser = default;
        int? highestPriority = null;
        foreach ((Type parserTargetType, ParserData parserData) in this.ParsersByTargetType)
        {
            // Check if the target type inherits from the parser target type, and validate for highest priority
            if (!parserTargetType.IsAssignableFrom(targetType) || parserData.Attribute.Priority <= highestPriority) continue;

            parser = parserData.Parser;
            highestPriority = parserData.Attribute.Priority;
        }

        return highestPriority is not null;
    }
    #endregion
}
