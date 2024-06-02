using ConfigLoader.Attributes;

namespace ConfigLoader;

public enum ArrayHandling
{
    NONE,
    SINGLE_VALUE,
    SEPARATE_VALUES
}

public readonly record struct ConfigSerializerSettings(ArrayHandling ArrayHandling = ArrayHandling.SINGLE_VALUE,
                                                       string ArraySeparator = ",")
{
    public ConfigSerializerSettings ApplyAttributeOverrides(ConfigFieldAttribute fieldAttribute)
    {
        ConfigSerializerSettings updatedSettings = this;
        if (fieldAttribute.ArrayHandling is not ArrayHandling.NONE && fieldAttribute.ArrayHandling != this.ArrayHandling)
        {
            updatedSettings = updatedSettings with { ArrayHandling = fieldAttribute.ArrayHandling };
        }

        if (!string.IsNullOrEmpty(fieldAttribute.ArraySeparator) && fieldAttribute.ArraySeparator != this.ArraySeparator)
        {
            updatedSettings = updatedSettings with { ArraySeparator = fieldAttribute.ArraySeparator! };
        }

        return updatedSettings;
    }
}
