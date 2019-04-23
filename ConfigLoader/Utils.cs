using System;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader
{
    /// <summary>
    /// Collection of utility methods
    /// </summary>
    internal static class Utils
    {
        #region Static methods
        /// <summary>
        /// Parses a comma separated string line to a string array. Entries are space trimmed and never empty
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>The resulting string non-empty string array</returns>
        public static string[] ParseArray(string text) => ParseArray(text, ',');

        /// <summary>
        /// Parses a string line with given delimiter to a string array. Entries are space trimmed and are never empty
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="delimiters">Char delimiters to separate the array</param>
        /// <returns>The resulting string non-empty string array</returns>
        public static string[] ParseArray(string text, params char[] delimiters)
        {
            string[] array = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }
            return array;
        }
        #endregion
    }
}
