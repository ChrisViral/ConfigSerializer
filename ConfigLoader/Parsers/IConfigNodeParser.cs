namespace ConfigLoader.Parsers;

public interface IConfigNodeParser
{
    object Parse(ConfigNode node);

    ConfigNode Save(object obj);
}
