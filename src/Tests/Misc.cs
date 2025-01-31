using System.Text;
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
    [InlineData(@"SaijSamples\123456789-0abc-defg-g23-85000scanyel.json")]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g81-87000tcanyel.json")]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g56-95000scanyel.json")]
    public void ConvertJsonToYaml(string jsonFile)
    {
        var json = File.ReadAllText(jsonFile).ReplaceLineEndings();

        var dictionary = DictionaryConverter.Parse(json);
        Assert.NotNull(dictionary);

        var yaml = DictionaryConverter.ToYaml(dictionary);

        // Save the YAML to a file
        var yamlFile = Path.ChangeExtension(jsonFile, ".yaml");
        File.WriteAllText($@"..\..\..\{yamlFile}", yaml);
    }

    [Theory]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g23-85000scanyel.json")]
    [InlineData(@"SaijSamples\123456789-0abc-defg-g56-95000scanyel.json")]
    public void ConvertJsonToMarkdown(string jsonFile)
    {
        var json = File.ReadAllText(jsonFile).ReplaceLineEndings();

        var dictionary = DictionaryConverter.Parse(json);
        Assert.NotNull(dictionary);

        var markdown = DictionaryConverter.ToMarkdown(dictionary);

        // Save the Markdown to a file
        var markdownFile = Path.ChangeExtension(jsonFile, ".md");
        File.WriteAllText($@"..\..\..\{markdownFile}", markdown);
    }
}
