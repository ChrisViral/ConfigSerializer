/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader;

/// <summary>
/// Autoloading Config Object interface
/// </summary>
public interface ISerializableConfig : IConfigNode
{
    void OnPostDeserialize();

    void OnPreSerialize();
}
