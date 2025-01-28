using System.Text.Json;
using System.Text.RegularExpressions;
using NuGet.Versioning;
using SharpYaml;
using Xunit;
using YamlDotNet.Serialization;

namespace Clarius.OpenLaw;

public partial class Misc
{
    static readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        Converters = { new JsonDictionaryConverter() },
    };

    [Theory]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g81-87000tcanyel.json")]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g56-95000scanyel.json")]
    public void ConvertJsonToYaml(string jsonFile)
    {
        var json = File.ReadAllText(jsonFile).ReplaceLineEndings();

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);

        var serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlDictionaryConverter())
            .WithTypeConverter(new YamlListConverter())
            .Build();

        var yaml = serializer.Serialize(dictionary);

        // Save the YAML to a file
        var yamlFile = Path.ChangeExtension(jsonFile, ".yaml");
        File.WriteAllText($@"..\..\..\{yamlFile}", yaml);
    }
}
