﻿using System;
using System.Collections.Generic;
using System.Reflection;
using ConfigLoader.Attributes;
using ConfigLoader.Extensions;
using UnityEngine;

namespace ConfigLoader;

public static class ConfigSerializer
{
    #region Deserialize
    public static T Deserialize<T>(ConfigNode node) where T : IConfigObject, new()
    {
        if (node is null) throw new ArgumentNullException(nameof(node), "ConfigNode cannot be null");

        T instance = new();
        Deserialize(node, ref instance);
        return instance;
    }

    public static void Deserialize<T>(ConfigNode node, ref T instance) where T : IConfigObject
    {
        if (node is null) throw new ArgumentNullException(nameof(node), "ConfigNode cannot be null");
        if (instance is null) throw new ArgumentNullException(nameof(instance), "Instance to populate cannot be null");

        foreach (MemberInfo member in GetMembers<T>())
        {
            try
            {
                LoadMember(member, node, instance);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConfigSerializer]: Could not load {member.Name} while deserializing {typeof(T).FullName}\n{e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
        }
    }
    #endregion

    #region Serialize
    public static bool Serialize<T>(T instance, string fileName, out ConfigNode serialized) where T : IConfigObject
    {
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName), "File name cannot be null or empty");

        serialized = Serialize(instance);
        return serialized?.Save(fileName) ?? false;
    }

    public static ConfigNode Serialize<T>(T instance) where T : IConfigObject
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance), "Instance to serialize cannot be null");

        ConfigNode node = new(instance.Name);

        return node;
    }
    #endregion

    #region Methods
    private static IEnumerable<MemberInfo> GetMembers<T>() where T : IConfigObject
    {
        const MemberTypes MEMBERS = MemberTypes.Field | MemberTypes.Property;
        const BindingFlags FLAGS  = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        return typeof(T).FindMembers(MEMBERS, FLAGS, FilterConfigMembers, null);
    }

    private static bool FilterConfigMembers(MemberInfo member, object criteria) => member.IsDefined(typeof(ConfigFieldAttribute), false);

    private static void LoadMember<T>(MemberInfo member, ConfigNode node, T instance)
    {
        ConfigFieldAttribute attribute = member.GetCustomAttribute<ConfigFieldAttribute>();
        string name = string.IsNullOrEmpty(attribute.Name) ? member.Name : attribute.Name;

        switch (member)
        {
            case FieldInfo field:
                object value = ParseMember(node, name, field.FieldType, attribute);
                if (value is null) return;

                field.SetValue(instance, value);
                break;

            case PropertyInfo property:
                value = ParseMember(node, name, property.PropertyType, attribute);
                if (value is null) return;

                property.SetValue(instance, value);
                break;

            default:
                throw new InvalidOperationException($"Invalid member type detected ({member.GetType()})");
        }
    }

    private static object ParseMember(ConfigNode node, string name, Type type, ConfigFieldAttribute attribute)
    {
        return null;
    }
    #endregion
}
