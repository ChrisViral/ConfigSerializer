using System.Collections.Generic;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader.Extensions;

internal static class DictionaryExtensions
{
    /// <summary>
    /// Tuple deconstruction of a KeyValuePair
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="pair">Pair to deconstruct</param>
    /// <param name="key">Key output value</param>
    /// <param name="value">Value output value</param>
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key   = pair.Key;
        value = pair.Value;
    }
}