using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Clarius.OpenLaw.Tests;

public class SaijClientTests(ITestOutputHelper output)
{
    static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    static SaijClient CreateClient(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHttpClient()
            .BuildServiceProvider();

        return new SaijClient(
            services.GetRequiredService<IHttpClientFactory>(),
            new Progress<ProgressMessage>(x => output.WriteLine($"{x.Percentage}% {x.Message}")));
    }

    [Fact]
    public async Task WhenRetrievingTwiceReturnsSameTimestamp()
    {
        var client = CreateClient(output);
        JsonNode? first = default;

        await foreach (var doc in client.EnumerateAsync())
        {
            first = doc;
            break;
        }

        Assert.NotNull(first);

        JsonNode? second = default;
        var id = first["document"]?["metadata"]?["uuid"]?.GetValue<string>();
        Assert.NotNull(id);

        await foreach (var doc in client.EnumerateAsync())
        {
            var id2 = doc["document"]?["metadata"]?["uuid"]?.GetValue<string>();
            if (id == id2)
            {
                second = doc;
                break;
            }
        }

        Assert.NotNull(second);
        Assert.Equal(
            first["document"]?["metadata"]?["timestamp"]?.GetValue<long>(),
            second["document"]?["metadata"]?["timestamp"]?.GetValue<long>());
    }

    [Fact]
    public async Task EnumerateAsync()
    {
        var client = CreateClient(output);
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

    [Fact(Skip = "Only for debugging")]
    public async Task EnumerateAllAsync()
    {
        var client = CreateClient(output);
        var count = 0;

        await foreach (var doc in client.EnumerateAsync())
        {
            output.WriteLine(doc.ToJsonString(options));
            count++;
        }

        Assert.True(count >= 5550, "Did not get expected docs or greater");
    }

    [Theory]
    [InlineData("123456789-0abc-defg-g23-85000scanyel")]
    [InlineData("123456789-0abc-defg-g56-95000scanyel")]
    [InlineData("123456789-0abc-defg-704-6000xvorpyel")]
    [InlineData("123456789-0abc-defg-382-5100bvorpyel")]
    public async Task CanFetchSpecificById(string id)
    {
        var client = CreateClient(output);

        var doc = await client.FetchAsync(id);

        Assert.NotNull(doc);

        await File.WriteAllTextAsync(@$"..\..\..\SaijSamples\{id}.json",
            doc.ToJsonString(options),
            System.Text.Encoding.UTF8);
    }
}