using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConfigLoader.Extensions;

internal static class TypeExtensions
{
    private static readonly Dictionary<Type, object> DefaultsByType = new();

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

    public static bool IsCollectionType(this Type type)
    {
        return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(ICollection<>);
    }

    public static bool IsInstantiable(this Type type)
    {
        return type is { IsAbstract: false, IsInterface: false, IsGenericType: false, IsGenericParameter: false };
    }

    public static Type StripNullable(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
}
