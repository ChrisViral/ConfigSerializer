using ConfigLoader.Attributes;

namespace ConfigLoader;

public enum ArrayHandling
{
    SINGLE_VALUE,
    SEPARATE_VALUES
}

public readonly record struct ConfigSerializerSettings(ArrayHandling ArrayHandling = ArrayHandling.SINGLE_VALUE,
                                                       char ArraySeparator = ',')
{
    public ConfigSerializerSettings ApplyAttributeOverrides(ConfigFieldAttribute fieldAttribute)
    {
        ConfigSerializerSettings updatedSettings = this;
        if (fieldAttribute.ArrayHandling is not null && fieldAttribute.ArrayHandling != this.ArrayHandling)
        {
            updatedSettings = updatedSettings with { ArrayHandling = fieldAttribute.ArrayHandling.Value };
        }

        if (fieldAttribute.ArraySeparator is not null && fieldAttribute.ArraySeparator != this.ArraySeparator)
        {
            updatedSettings = updatedSettings with { ArraySeparator = fieldAttribute.ArraySeparator.Value };
        }

        return updatedSettings;
    }
}
