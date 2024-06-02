using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ConfigLoader.Extensions;

internal static class TypeExtensions
{
    private static readonly Dictionary<Type, object> DefaultsByType = new();
    private static readonly Type CollectionType = typeof(ICollection<>);
    private static readonly Type ConfigNodeInterfaceType = typeof(IConfigNode);
    private static readonly Type SerializableConfigInterfaceType = typeof(ISerializableConfig);
    private static readonly Type ConfigNodeType = typeof(ConfigNode);

    public static object GetDefault(this Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type), "Type to get default for cannot be null");

        if (!DefaultsByType.TryGetValue(type, out object def))
        {
            Expression<Func<object>> getDefaultExp = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Default(type), typeof(object)));
            Func<object> getDefault = getDefaultExp.Compile();
            def = getDefault();
            DefaultsByType.Add(type, def);
        }
        return def;
    }

    public static bool IsCollectionType(this Type type, out Type? elementType)
    {
        if (type is null) throw new ArgumentNullException(nameof(type), "Type to check cannot be null");

        Type? collectionType = type;
        if (!collectionType.IsGenericICollectionType())
        {
            collectionType = type.GetInterfaces().FirstOrDefault(IsGenericICollectionType);
        }

        if (collectionType is not null)
        {
            elementType = collectionType.GetGenericArguments()[0];
            return true;
        }

        elementType = null;
        return false;
    }

    public static bool IsGenericICollectionType(this Type type)
    {
        return type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == CollectionType);
    }

    public static bool IsConfigType(this Type type)
    {
        return ConfigNodeInterfaceType.IsAssignableFrom(type)
            || SerializableConfigInterfaceType.IsAssignableFrom(type)
            || ConfigNodeType.IsAssignableFrom(type);
    }

    public static bool IsInstantiable(this Type type)
    {
        return type is { IsAbstract: false, IsInterface: false, IsGenericType: false, IsGenericParameter: false };
    }

    public static Type StripNullable(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
}
