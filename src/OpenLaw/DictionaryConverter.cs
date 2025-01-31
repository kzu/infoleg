using System.Text;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace Clarius.OpenLaw;

public class DictionaryConverter
{
    static readonly JsonSerializerOptions options = new()
    {
        Converters = { new JsonDictionaryConverter() },
    };

    public static void ConvertFile(string jsonFile, bool overwrite)
    {
        var yamlDir = Path.Combine(Path.GetDirectoryName(jsonFile) ?? "", "yaml");
        var yamlFile = Path.Combine(yamlDir, Path.ChangeExtension(Path.GetFileName(jsonFile), ".yaml"));
        Directory.CreateDirectory(yamlDir);

        var mdDir = Path.Combine(Path.GetDirectoryName(jsonFile) ?? "", "md");
        var mdFile = Path.Combine(mdDir, Path.ChangeExtension(Path.GetFileName(jsonFile), ".md"));
        Directory.CreateDirectory(mdDir);

        Dictionary<string, object?>? dictionary = null;

        if (overwrite || !File.Exists(yamlFile))
        {
            dictionary = Parse(File.ReadAllText(jsonFile));
            if (dictionary is null)
                return;

            File.WriteAllText(yamlFile, ToYaml(dictionary), Encoding.UTF8);
        }

        if (overwrite || !File.Exists(mdFile))
        {
            if (dictionary is null)
                dictionary = Parse(File.ReadAllText(jsonFile));
            if (dictionary is null)
                return;

            File.WriteAllText(mdFile, ToMarkdown(dictionary), Encoding.UTF8);
        }
    }

    public static Dictionary<string, object?>? Parse(string json)
        => JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);

    public static string ToYaml(Dictionary<string, object?> dictionary)
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlDictionaryConverter())
            .WithTypeConverter(new YamlListConverter())
            .Build();

        return serializer.Serialize(dictionary);
    }

    public static string ToMarkdown(Dictionary<string, object?> dictionary)
    {
        var output = new StringBuilder();
        ProcessDictionary(0, dictionary!, output);
        return output.ToString();
    }

    static void ProcessObject(int depth, object? obj, StringBuilder output)
    {
        if (obj is Dictionary<string, object?> dictionary)
        {
            ProcessDictionary(depth, dictionary, output);
        }
        else if (obj is List<object?> list)
        {
            foreach (var item in list)
            {
                ProcessObject(depth, item, output);
            }
        }
    }

    static void ProcessDictionary(int depth, Dictionary<string, object?> dictionary, StringBuilder output)
    {
        var title = dictionary
            .Where(x => x.Key.StartsWith("titulo-", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault().Value;

        if (title is not null)
        {
            depth++;
            output.AppendLine().AppendLine($"{new string('#', depth)} {title}");
        }

        foreach (var kvp in dictionary)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            if (value is null)
                continue;

            if (key == "texto" &&
                // We may have section title with text without an article #
                (dictionary.ContainsKey("numero-articulo") || title is not null))
            {
                output.AppendLine().AppendLine(value.ToString());
            }
            else
            {
                ProcessObject(depth, value, output);
            }
        }

        if (title is not null)
        {
            depth--;
        }
    }
}
