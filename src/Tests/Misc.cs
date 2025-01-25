using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;
using YamlDotNet.Serialization;

namespace Clarius.OpenLaw;

public class Misc
{
    [Theory]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g81-87000tcanyel.json")]
    public void ConvertJsonToYaml(string jsonFile)
    {
        var json = File.ReadAllText(jsonFile);

        var options = new JsonSerializerOptions
        {
            Converters = { new DictionaryConverter() },
        };

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);

        // Serialize the dictionary to YAML
        var serializer = new SerializerBuilder().Build();
        var yaml = serializer.Serialize(dictionary);

        // Save the YAML to a file
        var yamlFile = Path.ChangeExtension(jsonFile, ".yaml");
        File.WriteAllText($@"..\..\..\{yamlFile}", yaml);
    }
}
