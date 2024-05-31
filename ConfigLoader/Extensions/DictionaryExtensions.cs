using System.Collections.Generic;

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