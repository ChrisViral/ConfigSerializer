using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigLoader.Attributes;
using ConfigLoader.Extensions;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader.Parsers;

/// <summary>
/// Parser objects database
/// </summary>
public class ParserDatabase
{
    /// <summary>
    /// ParserDatabase instance
    /// </summary>
    public static ParserDatabase Instance { get; } = new();

    /// <summary>
    /// ConfigNode value parsers
    /// </summary>
    public ParserFilter<IConfigValueParser> ValueParsers { get; }
    /// <summary>
    /// ConfigNode node parsers
    /// </summary>
    public ParserFilter<IConfigNodeParser> NodeParsers { get; }

    private ParserDatabase()
    {
        // Get all currently loaded types
        List<Type> allTypes = [];
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                allTypes.AddRange(assembly.GetTypes());

            }
            catch (Exception e)
            {
                this.LogException($"Could not load types of assembly {assembly.GetName().Name}", e);
            }
        }

        // Filter out valid parser types
        Type[] parserTypes = allTypes.Where(t => t.IsInstantiable()
                                              && typeof(IConfigParserBase).IsAssignableFrom(t)
                                              && t.IsDefined(typeof(ConfigParserAttribute)))
                                     .ToArray();

        // Create relevant filters
        this.ValueParsers = new ParserFilter<IConfigValueParser>(parserTypes);
        this.NodeParsers  = new ParserFilter<IConfigNodeParser>(parserTypes);
    }
}
