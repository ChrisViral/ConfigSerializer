using System;
using System.Reflection;
using ConfigLoader.Attributes;
using ConfigLoader.Extensions;
using UnityEngine;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader;

/// <summary>
/// Base class for objects that can be auto-loaded from ConfigNodes using the ConfigFieldAttribute
/// </summary>
public abstract class ConfigObject
{
    #region Constants
    //Bunch of useful constants, avoids having to create a delegate every time and excessive repeated usage of typeof()
    private const MemberTypes MEMBERS = MemberTypes.Field | MemberTypes.Property;
    private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly MemberFilter FILTER = (mi, _) => mi.IsDefined(ATTRIBUTE_TYPE, false);
    private static readonly Type ATTRIBUTE_TYPE = typeof(ConfigFieldAttribute);
    private static readonly Type STRING_TYPE = typeof(string);
    private static readonly Type STRING_ARRAY_TYPE = typeof(string[]);
    private static readonly Type INT_TYPE = typeof(int);
    private static readonly Type FLOAT_TYPE = typeof(float);
    private static readonly Type DOUBLE_TYPE = typeof(double);
    private static readonly Type BOOL_TYPE = typeof(bool);
    private static readonly Type VECTOR3_TYPE = typeof(Vector3);
    private static readonly Type CONFIG_TYPE = typeof(ConfigObject);
    private static readonly Type NODE_TYPE = typeof(ConfigNode);
    #endregion

    #region Fields
    //Cache for the relevant fields and properties, allows having to use reflection multiple times
    private readonly MemberInfo[] members;
    #endregion

    #region Properties
    /// <summary>
    /// Name used for the ConfigNode when saving, must be overriden to change from the default "NODE"
    /// </summary>
    protected virtual string NodeName { get; } = "NODE";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a ConfigObject from the given ConfigNode
    /// </summary>
    /// <param name="node">ConfigNode to load the object from</param>
    /// <param name="load">If the object should be loaded right away, defaults to true</param>
    protected ConfigObject(ConfigNode node, bool load = true)
    {
        //Get relevant members
        this.members = CONFIG_TYPE.FindMembers(MEMBERS, FLAGS, FILTER, null);

        //Only proceed with a load if requested to
        if (load) { Load(node); }
    }
    #endregion

    #region Static methods
    /// <summary>
    /// Loads the given value from a ConfigNode
    /// </summary>
    /// <param name="node">Node to get the value from</param>
    /// <param name="name">Name of the value to get</param>
    /// <param name="type">Type of value to get</param>
    /// <param name="mandatory">If this field is mandatory or not</param>
    /// <returns>The loaded value from the node</returns>
    /// <exception cref="MissingConfigFieldException">Thrown when a mandatory field is not found or could not be loaded</exception>
    private static object GetValue(ConfigNode node, string name, Type type, bool mandatory)
    {
        //Check to make sure the name of the value we're looking for is valid
        if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name), "Value name cannot be null or empty"); }

        //Find the correct type
        if (type.IsEnum)
        {
            return node.TryGetValue(name, out string s) || !mandatory ? Enum.Parse(type, s, true) : throw new MissingConfigFieldException($"Mandatory {type.Name} field {name} missing");
        }
        if (type == STRING_TYPE)
        {
            return node.TryGetValue(name, out string s) || !mandatory ? s : throw new MissingConfigFieldException($"Mandatory String field {name} missing");
        }
        if (type == STRING_ARRAY_TYPE)
        {
            return node.TryGetValue(name, out string s) || !mandatory ? Utils.ParseArray(s) : throw new MissingConfigFieldException($"Mandatory String field {name} missing");
        }
        if (type == INT_TYPE)
        {
            return node.TryGetValue(name, out int i) || !mandatory ? i : throw new MissingConfigFieldException($"Mandatory Int field {name} missing");
        }
        if (type == FLOAT_TYPE)
        {
            return node.TryGetValue(name, out float f) || !mandatory ? f : throw new MissingConfigFieldException($"Mandatory Float field {name} missing");
        }
        if (type == DOUBLE_TYPE)
        {
            return node.TryGetValue(name, out double d) || !mandatory ? d : throw new MissingConfigFieldException($"Mandatory Double field {name} missing");
        }
        if (type == BOOL_TYPE)
        {
            return node.TryGetValue(name, out bool b) || !mandatory ? b : throw new MissingConfigFieldException($"Mandatory Bool field {name} missing");
        }
        if (type == VECTOR3_TYPE)
        {
            return node.TryGetValue(name, out Vector3 v) || !mandatory ? v : throw new MissingConfigFieldException($"Mandatory Vector3 field {name} missing");
        }
        if (type.IsSubclassOf(CONFIG_TYPE))
        {
            return node.TryGetNode(name, out ConfigNode n) || !mandatory ? Activator.CreateInstance(type, n) : throw new MissingConfigFieldException($"Mandatory {type.Name} field {name} missing");
        }
        if (type == NODE_TYPE)
        {
            return node.TryGetNode(name, out ConfigNode n) || !mandatory ? n : throw new MissingConfigFieldException($"Mandatory ConfigNode field {name} missing");
        }

        //If type not found, the type isn't supported
        throw new NotSupportedException(type.FullName + " is not a supported ConfigField type");
    }

    /// <summary>
    /// Add a value or node to the given ConfigNode
    /// </summary>
    /// <param name="node">Node to add the value or node to</param>
    /// <param name="value">Value or node to add</param>
    /// <param name="name">Name of the value (not used for nodes)</param>
    private static void AddValue(ConfigNode node, object value, string name)
    {
        //Check to make sure the name and value aren't null
        if (value == null) { throw new ArgumentNullException(nameof(value), "Cannot save a null object to the config"); }
        if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name), "Value cannot be saved to a null or empty name"); }

        //Treat differently if a ConfigNode, ConfigObject, or string[]
        switch (value)
        {
            case ConfigNode n:
                node.AddNode(n);
                return;

            case ConfigObject o:
                node.AddNode(o.Save());
                return;

            case string[] a:
                node.AddValue(name, string.Join(", ", a));
                return;
        }

        //If not, just add as is
        node.AddValue(name, value);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Loads data from the given ConfigNode to the object
    /// </summary>
    /// <param name="node">ConfigNode to load data from</param>
    public void Load(ConfigNode node)
    {
        //Make sure the passed node isn't null
        if (node == null) { throw new ArgumentNullException(nameof(node), "Provided ConfigNode is null"); }

        //Treat each member individually
        foreach (MemberInfo mi in this.members)
        {
            //Get ConfigFieldAttribute
            ConfigFieldAttribute attribute = (ConfigFieldAttribute)mi.GetCustomAttributes(ATTRIBUTE_TYPE, false)[0];
            //Get custom name if any, or default name
            string valueName = string.IsNullOrEmpty(attribute.Name) ? mi.Name : attribute.Name;

            //Handle fields and properties
            switch (mi)
            {
                case FieldInfo fi:
                    fi.SetValue(this, GetValue(node, valueName, fi.FieldType, attribute.Required));
                    break;

                case PropertyInfo pi:
                    pi.SetValue(this, GetValue(node, valueName, pi.PropertyType, attribute.Required), null);
                    break;
            }
        }
    }

    /// <summary>
    /// Saves the ConfigObject to a ConfigNode, then returns it
    /// The name of the created ConfigNode is taken from the NodeName property
    /// </summary>
    /// <returns>The created ConfigNode containing the saved data</returns>
    public ConfigNode Save()
    {
        //Create the ConfigNode
        ConfigNode node = new ConfigNode(this.NodeName);

        //Treat each member individually
        foreach (MemberInfo mi in this.members)
        {
            //Get ConfigFieldAttribute
            ConfigFieldAttribute attribute = (ConfigFieldAttribute)mi.GetCustomAttributes(ATTRIBUTE_TYPE, false)[0];
            //Get custom name if any, or default name
            string valueName = string.IsNullOrEmpty(attribute.Name) ? mi.Name : attribute.Name;

            //Handle fields and properties
            switch (mi)
            {
                case FieldInfo fi:
                    AddValue(node, fi.GetValue(this), valueName);
                    break;

                case PropertyInfo pi:
                    AddValue(node, pi.GetValue(this, null), valueName);
                    break;
            }
        }

        //Return final node
        return node;
    }
    #endregion
}