namespace ConfigLoader.Parsers;

public interface IConfigNodeParser : IConfigParserBase
{
    object Parse(ConfigNode node);

    ConfigNode Save(object obj);
}
