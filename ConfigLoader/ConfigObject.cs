/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader;

/// <summary>
/// Base class for auto-serializing <see cref="IConfigNode"/> implementations
/// </summary>
public abstract class ConfigObject : ISerializableConfig, IConfigNode
{
    #region IConfigNode implementation
    /// <summary>
    /// Loads the object from the given node
    /// </summary>
    /// <param name="node">Node to load from</param>
    public virtual void Load(ConfigNode node) => ConfigSerializer.Deserialize(node, this);

    /// <summary>
    /// Saves the object to the given node
    /// </summary>
    /// <param name="node">Node to save to</param>
    public virtual void Save(ConfigNode node) => ConfigSerializer.Serialize(node, this);
    #endregion
}
