using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Clarius.OpenLaw.Tests;

public class SaijClientTests(ITestOutputHelper output)
{
    static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task EnumerateAsync()
    {
        var services = new ServiceCollection()
            .AddHttpClient()
            .BuildServiceProvider();

        var client = new SaijClient(
            services.GetRequiredService<IHttpClientFactory>(),
            new Progress<string>(x => output.WriteLine(x)));

        JsonNode? json = default;

        await foreach (var doc in client.EnumerateAsync())
        {
            output.WriteLine(doc.ToJsonString(options));
            json = doc;
            break;
        }

        Assert.NotNull(json);
        // TODO: parse into loosely structured markdown
    }
}