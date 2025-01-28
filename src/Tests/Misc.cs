using System.Text.Json;
using System.Text.RegularExpressions;
using NuGet.Versioning;
using SharpYaml;
using Xunit;
using YamlDotNet.Serialization;

namespace Clarius.OpenLaw;

public partial class Misc
{
    [Theory]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g81-87000tcanyel.json")]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g56-95000scanyel.json")]
    public void ConvertJsonToYaml(string jsonFile)
    {
        var json = File.ReadAllText(jsonFile).ReplaceLineEndings();

        var options = new JsonSerializerOptions
        {
            Converters = { new DictionaryConverter() },
        };

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);

        EnsureMultiline(dictionary!);

        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlDictionaryConverter())
            .WithTypeConverter(new YamlListConverter())
            .Build();

        var yaml = serializer.Serialize(dictionary);

        // Save the YAML to a file
        var yamlFile = Path.ChangeExtension(jsonFile, ".yaml");
        File.WriteAllText($@"..\..\..\{yamlFile}", yaml);
    }

    void EnsureMultiline(IDictionary<string, object> model)
    {
        static string ReplaceParagraphs(string value)
        {
            var replaced = value.Replace("[[p]]", "\n").Replace("[[/p]]", "\n");
            var multiline = MultilineExpr().Replace(replaced, "\n\n");
            
            return multiline.Trim();
        }

        foreach (var key in model.Keys)
        {
            if (model[key] is IDictionary<string, object> submodel)
            {
                EnsureMultiline(submodel);
            }
            else if (model[key] is string value)
            {
                if (value.Contains("[[p]]"))
                {
                    model[key] = ReplaceParagraphs(value);
                }
            }
            else if (model[key] is IList<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is IDictionary<string, object> itemmodel)
                    {
                        EnsureMultiline(itemmodel);
                    }
                    if (list[i] is string itemvalue && itemvalue.Contains("[[p]]"))
                    {
                        list[i] = ReplaceParagraphs(itemvalue);
                    }
                }
            }
        }
    }

    [GeneratedRegex(@"(\r?\n){3,}", RegexOptions.Compiled)]
    private static partial Regex MultilineExpr();
}
