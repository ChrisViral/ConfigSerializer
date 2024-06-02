using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using ConfigLoader.Attributes;
using ConfigLoader.Extensions;
using ConfigLoader.Parsers;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader;

/// <summary>
/// Config object serializer
/// </summary>
public static class ConfigSerializer
{
    /// <summary>
    /// Serializable member cache
    /// </summary>
    /// <typeparam name="T">Type of element to serialize</typeparam>
    private static class SerializableMembers<T> where T : ISerializableConfig
    {
        private const MemberTypes MEMBERS = MemberTypes.Field | MemberTypes.Property;
        private const BindingFlags FLAGS  = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Serializable members for this type
        /// </summary>
        /// ReSharper disable once StaticMemberInGenericType
        public static ReadOnlyCollection<MemberInfo> Members { get; }

        static SerializableMembers()
        {
            // Get all relevant members in the type
            Members = typeof(T).StripNullable()
                               .FindMembers(MEMBERS, FLAGS, FilterConfigMembers, null)
                               .ToList()
                               .AsReadOnly();
        }

        /// <summary>
        /// FindMember filter that ensures the members are valid and instantiatable
        /// </summary>
        /// <param name="member">Member to test</param>
        /// <param name="criteria">Test criteria (unused)</param>
        /// <returns><see langword="true"/> if the member is valid, <see langword="false"/> otherwise</returns>
        private static bool FilterConfigMembers(MemberInfo member, object criteria)
        {
            // Ensure the member has the proper attribute
            if (!member.IsDefined(typeof(ConfigFieldAttribute), false)) return false;

            // Get targeted type
            Type? targetType = member switch
            {
                FieldInfo field       => field.FieldType,
                PropertyInfo property => property.PropertyType,
                _                     => null
            };

            // Ensure the type of the member is instantiable
            return targetType?.IsInstantiable() ?? false;
        }
    }

    #region Fields
    /// <summary>
    /// ICollection Add reflection parameter buffer
    /// </summary>
    private static readonly object[] CollectionAddParameters = new object[1];
    #endregion

    #region Serialize
    /// <summary>
    /// Serializes the object to a <see cref="ConfigNode"/> and returns it
    /// </summary>
    /// <typeparam name="T">Type of object to Serialize</typeparam>
    /// <param name="nodeName">Name of the ConfigNode to serialize it to</param>
    /// <param name="instance">Object to serialize</param>
    /// <param name="serializerSettings">Serializer settings, leave blank to use default settings</param>
    /// <returns>The serialized ConfigNode version of <paramref name="instance"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodeName"/> or <paramref name="instance"/> is <see langword="null"/></exception>
    public static ConfigNode Serialize<T>(string nodeName, T instance, in ConfigSerializerSettings? serializerSettings = null) where T : ISerializableConfig
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance), "Instance to serialize cannot be null");
        if (string.IsNullOrEmpty(nodeName)) throw new ArgumentNullException(nameof(nodeName), "ConfigNode name cannot be null or empty");

        // Create node then serialize on it
        ConfigNode node = new(nodeName);
        Serialize(node, instance, serializerSettings);
        return node;
    }

    /// <summary>
    /// Serializes the object to an existing <see cref="ConfigNode"/>
    /// </summary>
    /// <typeparam name="T">Type of object to Serialize</typeparam>
    /// <param name="node">ConfigNode to save the object to</param>
    /// <param name="instance">Object to serialize</param>
    /// <param name="serializerSettings">Serializer settings, leave blank to use default settings</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> or <paramref name="instance"/> is <see langword="null"/></exception>
    public static void Serialize<T>(ConfigNode node, T instance, in ConfigSerializerSettings? serializerSettings = null) where T : ISerializableConfig
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance), "Instance to serialize cannot be null");
        if (node is null) throw new ArgumentNullException(nameof(node), "ConfigNode cannot be null");

        // Boxing now prevents value type data loss, we'll have to box it eventually anyway
        object boxedInstance = instance;
        ConfigSerializerSettings settings = serializerSettings ?? new ConfigSerializerSettings();

        // Load all members individually
        foreach (MemberInfo member in SerializableMembers<T>.Members)
        {
            try
            {
                SaveMember(member, node, boxedInstance, settings);
            }
            catch (Exception e)
            {
                Utils.LogException(nameof(ConfigSerializer), $"Could not load {member.Name} while deserializing {typeof(T).FullName}", e);
            }
        }
    }

    private static void SaveMember(MemberInfo member, ConfigNode node, object instance, in ConfigSerializerSettings settings)
    {
        // Get member data
        ConfigFieldAttribute attribute = member.GetCustomAttribute<ConfigFieldAttribute>();
        string name = string.IsNullOrEmpty(attribute.Name) ? member.Name : attribute.Name;
        ConfigSerializerSettings memberSettings = settings.ApplyAttributeOverrides(attribute);

        // Get member values then serialize them to the node
        switch (member)
        {
            case FieldInfo field:
                object? value = field.GetValue(instance);
                if (!EnsureMemberValid(value, name, attribute.Required)) return;

                WriteMember(node, name, value, field.FieldType, memberSettings);
                break;

            case PropertyInfo property:
                value = property.GetValue(instance);
                if (!EnsureMemberValid(value, name, attribute.Required)) return;

                WriteMember(node, name, value, property.PropertyType, memberSettings);
                break;

            default:
                throw new InvalidOperationException($"Invalid member type detected ({member.GetType()})");
        }
    }

    private static void WriteMember(ConfigNode node, string name, object value, Type targetType, in ConfigSerializerSettings settings)
    {
        // Make sure to strip out the nullable type before saving
        targetType = targetType.StripNullable();
        if (targetType.IsCollectionType(out Type? elementType))
        {
            // Save node collections, handles both ICollection and arrays
            WriteCollection(node, name, value, elementType!.StripNullable(), settings);
        }
        else if (targetType.IsConfigType())
        {
            // Save node objects
            ConfigNode savedNode = WriteNode(name, value, targetType, settings);
            node.AddNode(savedNode);
        }
        else
        {
            // Save simple values
            string savedValue = WriteValue(value, targetType, settings);
            node.AddNode(name, savedValue);
        }
    }

    private static void WriteCollection(ConfigNode node, string name, object collection, Type elementType, in ConfigSerializerSettings settings)
    {
        // Get enumerable implementation, this shouldn't ever fail, but if it does, we want it to be loud
        IEnumerable collectionEnumerable = (IEnumerable)collection;
        if (elementType.IsConfigType())
        {
            foreach (object element in collectionEnumerable)
            {
                // Save each node individually
                ConfigNode savedNode = WriteNode(name, element, elementType, settings);
                node.AddNode(savedNode);
            }
        }
        else
        {
            switch (settings.ArrayHandling)
            {
                case ArrayHandling.SINGLE_VALUE:
                    // Single value array
                    StringBuilder valueBuilder = StringBuilderCache.Acquire();
                    foreach (object element in collectionEnumerable)
                    {
                        string savedValue = WriteValue(element, elementType, settings);
                        valueBuilder.Append(savedValue).Append(settings.ArraySeparator);
                    }

                    // Clear out final separator and add to node
                    valueBuilder.Length -= settings.ArraySeparator.Length;
                    node.AddValue(name, valueBuilder.ToStringAndRelease());
                    break;

                case ArrayHandling.SEPARATE_VALUES:
                    // Separate values array
                    foreach (object element in collectionEnumerable)
                    {
                        string savedValue = WriteValue(element, elementType, settings);
                        node.AddValue(name, savedValue);
                    }
                    break;

                default:
                    // Default to single if invalid
                    goto case ArrayHandling.SINGLE_VALUE;
            }
        }
    }

    private static ConfigNode WriteNode(string name, object value, Type targetType, in ConfigSerializerSettings settings)
    {
        // Find correct parser and deserialize
        ConfigNode savedNode = new(name);
        if (ParserDatabase.Instance.NodeParsers.TryFindBestParserForType(targetType, out IConfigNodeParser? parser))
        {
            if (value is IConfigSerializationCallbacks serializable)
            {
                // Call PreSerialize if relevant
                serializable.OnPreSerialize();
            }

            parser!.Save(savedNode, value, settings);
        }

        return savedNode;
    }

    private static string WriteValue(object value, Type targetType, in ConfigSerializerSettings settings)
    {
        if (ParserDatabase.Instance.ValueParsers.TryFindBestParserForType(targetType, out IConfigValueParser? parser))
        {
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            return parser!.Save(value, settings) ?? string.Empty;
        }

        return string.Empty;
    }
    #endregion

    #region Deserialize
    /// <summary>
    /// Creates a new instance and deserializes to config data into it
    /// </summary>
    /// <typeparam name="T">Config type to deserialize</typeparam>
    /// <param name="node">Source node to deserialize from</param>
    /// <param name="serializerSettings">Serializer settings, leave blank to use default settings</param>
    /// <returns>The deserialized value</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="node"/> is <see langword="null"/></exception>
    public static T Deserialize<T>(ConfigNode node, in ConfigSerializerSettings? serializerSettings = null) where T : ISerializableConfig, new()
    {
        if (node is null) throw new ArgumentNullException(nameof(node), "ConfigNode cannot be null");

        // Create instance and deserialize into it
        T instance = new();
        Deserialize(node, ref instance, serializerSettings);
        return instance;
    }

    /// <summary>
    /// Deserializes the given config using the passed defaults
    /// </summary>
    /// <typeparam name="T">Config type to deserialize</typeparam>
    /// <param name="node">Source node to deserialize from</param>
    /// <param name="defaults">Value to get defaults for each fields from (will be modified if a reference type)</param>
    /// <param name="serializerSettings">Serializer settings, leave blank to use default settings</param>
    /// <returns>The deserialized value into <paramref name="defaults"/>, if it is a reference type, the original reference is returned</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="node"/> or <paramref name="defaults"/> is <see langword="null"/></exception>
    public static T Deserialize<T>(ConfigNode node, T defaults, in ConfigSerializerSettings? serializerSettings = null) where T : ISerializableConfig
    {
        Deserialize(node, ref defaults, serializerSettings);
        return defaults;
    }

    /// <summary>
    /// Deserializes the given config into the passed instance
    /// </summary>
    /// <typeparam name="T">Config type to deserialize</typeparam>
    /// <param name="node">Source node to deserialize from</param>
    /// <param name="instance">Value to deserialize into</param>
    /// <param name="serializerSettings">Serializer settings, leave blank to use default settings</param>
    /// <exception cref="ArgumentNullException">If <paramref name="node"/> or <paramref name="instance"/> is <see langword="null"/></exception>
    public static void Deserialize<T>(ConfigNode node, ref T instance, in ConfigSerializerSettings? serializerSettings = null) where T : ISerializableConfig
    {
        if (node is null) throw new ArgumentNullException(nameof(node), "ConfigNode cannot be null");
        if (instance == null) throw new ArgumentNullException(nameof(instance), "Instance to populate cannot be null");

        // Boxing now prevents value type data loss, we'll have to box it eventually anyway
        object boxedInstance = instance;
        ConfigSerializerSettings settings = serializerSettings ?? new ConfigSerializerSettings();

        // Load all members individually
        foreach (MemberInfo member in SerializableMembers<T>.Members)
        {
            try
            {
                LoadMember(member, node, boxedInstance, settings);
            }
            catch (Exception e)
            {
                Utils.LogException(nameof(ConfigSerializer), $"Could not load {member.Name} while deserializing {typeof(T).FullName}", e);
            }
        }

        // Unbox the value
        instance = (T)boxedInstance;
    }

    private static void LoadMember(MemberInfo member, ConfigNode node, object instance, in ConfigSerializerSettings settings)
    {
        // Get member data
        ConfigFieldAttribute attribute = member.GetCustomAttribute<ConfigFieldAttribute>();
        string name = string.IsNullOrEmpty(attribute.Name) ? member.Name : attribute.Name;
        ConfigSerializerSettings memberSettings = settings.ApplyAttributeOverrides(attribute);

        // Parse and assign members, ignore if did not load
        switch (member)
        {
            case FieldInfo field:
                object? value = ParseMember(node, name, field.FieldType, memberSettings);
                if (!EnsureMemberValid(value, name, attribute.Required)) return;

                field.SetValue(instance, value);
                break;

            case PropertyInfo property:
                value = ParseMember(node, name, property.PropertyType, memberSettings);
                if (!EnsureMemberValid(value, name, attribute.Required)) return;

                property.SetValue(instance, value);
                break;

            default:
                throw new InvalidOperationException($"Invalid member type detected ({member.GetType()})");
        }
    }

    private static object? ParseMember(ConfigNode node, string name, Type targetType, in ConfigSerializerSettings settings)
    {
        // Make sure to strip out the nullable type before parsing
        targetType = targetType.StripNullable();
        if (targetType.IsArray)
        {
            // Parse array of values or nodes
            return ParseArray(node, name, targetType.GetElementType()!.StripNullable(), settings);
        }

        if (targetType.IsCollectionType(out Type? elementType))
        {
            // Parse collection of value or nodes
            return ParseCollection(node, name, targetType, elementType!.StripNullable(), settings);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (targetType.IsConfigType())
        {
            // Parse node-based object
            return node.TryGetNode(name, out ConfigNode childNode) ? ParseNode(childNode, targetType, settings) : null;
        }

        // Parse value-based object
        return node.TryGetValue(name, out string value) ? ParseValue(value, targetType, settings) : null;
    }

    private static object ParseArray(ConfigNode node, string name, Type elementType, in ConfigSerializerSettings settings)
    {
        // Load values and create matching array
        object?[] parsed = ParseMultiple(node, name, elementType, settings);
        Array array = Array.CreateInstance(elementType, parsed.Length);
        for (int i = 0; i < parsed.Length; i++)
        {
            array.SetValue(parsed[i] ?? elementType.GetDefault(), i);
        }

        return array;
    }

    private static object ParseCollection(ConfigNode node, string name, Type collectionType, Type elementType, in ConfigSerializerSettings settings)
    {
        // Create collection object and get handle to add method
        object collection = Activator.CreateInstance(collectionType);
        // This cannot be cached as late-binding on generics is not allowed
        MethodInfo addMethod = collectionType.GetMethod(nameof(ICollection<int>.Add))!;

        // Parse values then add them to the collection
        object?[] parsed = ParseMultiple(node, name, elementType, settings);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < parsed.Length; i++)
        {
            CollectionAddParameters[0] = parsed[i] ?? elementType.GetDefault();
            addMethod.Invoke(collection, CollectionAddParameters);
        }

        return collection;
    }

    private static object?[] ParseMultiple(ConfigNode node, string name, Type targetType, in ConfigSerializerSettings settings)
    {
        object?[] parsed;
        // For node-based objects
        if (targetType.IsConfigType())
        {
            // If no nodes of given name, return an empty array
            if (!node.HasNode(name)) return [];

            // Parse individually and slot in output array
            ConfigNode[] nodes = node.GetNodes(name);
            parsed = new object[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                parsed[i] = ParseNode(nodes[i], targetType, settings);
            }

            return parsed;
        }

        string[] values;
        switch (settings.ArrayHandling)
        {
            case ArrayHandling.SINGLE_VALUE:
                // Load multiple values on one line
                if (!node.TryGetValue(name, out string allValues)) return [];

                values = Utils.ParseArray(allValues, settings.ArraySeparator);
                if (values.Length is 0) return [];
                break;

            case ArrayHandling.SEPARATE_VALUES:
                if (!node.HasValue(name)) return [];

                // Load multiple values on separate lines
                values = node.GetValues(name);
                break;

            default:
                // Default to single if invalid
                goto case ArrayHandling.SINGLE_VALUE;
        }

        // Parse individually and slot in output array
        parsed = new object?[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            parsed[i] = ParseValue(values[i], targetType, settings);
        }

        return parsed;
    }

    private static object? ParseNode(ConfigNode node, Type targetType, in ConfigSerializerSettings settings)
    {
        // Find correct parser and deserialize
        if (ParserDatabase.Instance.NodeParsers.TryFindBestParserForType(targetType, out IConfigNodeParser? parser))
        {
            object parsed = parser!.Parse(node, settings);
            if (parsed is IConfigSerializationCallbacks serializable)
            {
                // Call PostDeserialize if relevant
                serializable.OnPostDeserialize();
            }

            return parsed;
        }

        return null;

    }

    private static object? ParseValue(string value, Type targetType, in ConfigSerializerSettings settings)
    {
        // Find correct parser and deserialize
        if (ParserDatabase.Instance.ValueParsers.TryFindBestParserForType(targetType, out IConfigValueParser? parser))
        {
            return parser!.Parse(value, settings);
        }

        return null;
    }
    #endregion

    #region Helper methods
    private static bool EnsureMemberValid(object? value, string name, bool required)
    {
        if (value is not null) return true;

        return required ? throw new MissingConfigFieldException("Field could not be properly loaded", name) : false;
    }
    #endregion
}