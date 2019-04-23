using UnityEngine;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader.Extensions
{
    /// <summary>
    /// ConfigNode extension methods
    /// Having out parameters was more convenient to my usage
    /// </summary>
    internal static class ConfigNodeExtensions
    {
        #region Extension methods
        /// <summary>
        /// Tries to get a named ConfigNode from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the child node from</param>
        /// <param name="name">Name of the node find</param>
        /// <param name="result">Resulting node</param>
        /// <returns>True if the node has been found, false otherwise</returns>
        public static bool TryGetNode(this ConfigNode node, string name, out ConfigNode result)
        {
            if (node.HasNode(name))
            {
                result = node.GetNode(name);
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Tries to get multiple nodes with the same name from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the child node from</param>
        /// <param name="name">Name of the node find</param>
        /// <param name="results">An array of the loaded nodes</param>
        /// <returns></returns>
        public static bool TryGetNodes(this ConfigNode node, string name, out ConfigNode[] results)
        {
            results = node.GetNodes(name);
            if (results.Length == 0)
            {
                results = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to get a string value from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the value from</param>
        /// <param name="name">Name of the value find</param>
        /// <param name="result">Resulting string value</param>
        /// <returns>True if the value has been found, false otherwise</returns>
        public static bool TryGetValue(this ConfigNode node, string name, out string result)
        {
            if (node.HasValue(name))
            {
                result = node.GetValue(name);
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Tries to get an int value from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the value from</param>
        /// <param name="name">Name of the value find</param>
        /// <param name="result">Resulting int value</param>
        /// <returns>True if the value has been found, false otherwise</returns>
        public static bool TryGetValue(this ConfigNode node, string name, out int result)
        {
            if (node.HasValue(name))
            {
                return int.TryParse(node.GetValue(name), out result);
            }

            result = 0;
            return false;
        }
        
        /// <summary>
        /// Tries to get a float value from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the value from</param>
        /// <param name="name">Name of the value find</param>
        /// <param name="result">Resulting float value</param>
        /// <returns>True if the value has been found, false otherwise</returns>
        public static bool TryGetValue(this ConfigNode node, string name, out float result)
        {
            if (node.HasValue(name))
            {
                return float.TryParse(node.GetValue(name), out result);
            }

            result = 0f;
            return false;
        }
        
        /// <summary>
        /// Tries to get a double value from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the value from</param>
        /// <param name="name">Name of the value find</param>
        /// <param name="result">Resulting double value</param>
        /// <returns>True if the value has been found, false otherwise</returns>
        public static bool TryGetValue(this ConfigNode node, string name, out double result)
        {
            if (node.HasValue(name))
            {
                return double.TryParse(node.GetValue(name), out result);
            }

            result = 0d;
            return false;
        }
        
        /// <summary>
        /// Tries to get a bool value from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the value from</param>
        /// <param name="name">Name of the value find</param>
        /// <param name="result">Resulting bool value</param>
        /// <returns>True if the value has been found, false otherwise</returns>
        public static bool TryGetValue(this ConfigNode node, string name, out bool result)
        {
            if (node.HasValue(name))
            {
                return bool.TryParse(node.GetValue(name), out result);
            }

            result = false;
            return false;
        }
        
        /// <summary>
        /// Tries to get a Vector3 value from the given ConfigNode
        /// </summary>
        /// <param name="node">Node to get the value from</param>
        /// <param name="name">Name of the value find</param>
        /// <param name="result">Resulting Vector3 value</param>
        /// <returns>True if the value has been found, false otherwise</returns>
        public static bool TryGetValue(this ConfigNode node, string name, out Vector3 result)
        {
            if (node.HasValue(name))
            {
                string[] splits = Utils.ParseArray(node.GetValue(name));
                if (splits.Length == 3 && !float.TryParse(splits[0], out float x) && !float.TryParse(splits[0], out float y) && !float.TryParse(splits[0], out float z))
                {
                    result = new Vector3(x, y, z);
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }
        #endregion
    }
}
