namespace ConfigLoader.Parsers;

public interface IConfigValueParser : IConfigParserBase
{
    object Parse(string value);

    string Save(object value);
}
