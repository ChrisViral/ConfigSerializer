namespace ConfigLoader.Parsers;

public interface IConfigValueParser
{
    object Parse(string value);

    string Save(object value);
}
