using System.IO;
using System.Text;
using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Devlooped;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

public class DownloadCommand(IAnsiConsole console, IHttpClientFactory http) : AsyncCommand<DownloadSettings>
{
    static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public override async Task<int> ExecuteAsync(CommandContext context, DownloadSettings settings)
    {
        Directory.CreateDirectory(settings.Directory);

        await console.Progress()
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
            ])
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Descargando...");
                var client = new SaijClient(http, new Progress<ProgressMessage>(x =>
                {
                    task.Description = x.Message;
                    task.Value(x.Percentage);
                }));

                var tipo = settings.All ? null : "Ley";
                var jurisdiccion = settings.All ? null : "Nacional";

                await foreach (var doc in client.EnumerateAsync())
                {
                    if (doc["document"]?["metadata"]?["uuid"]?.GetValue<string>() is not string id ||
                        doc["document"]?["metadata"]?["timestamp"]?.GetValue<long>() is not long timestamp)
                        continue;

                    var file = Path.Combine(settings.Directory, id + ".json");
                    // Skip if file exists and has the same timestamp
                    if (File.Exists(file) && await GetJsonTimestampAsync(file) == timestamp)
                        continue;

                    File.WriteAllText(file, doc.ToJsonString(options));
                }
            });

        return 0;
    }

    async Task<long> GetJsonTimestampAsync(string file)
    {
        var jq = await Cli.Wrap(JQ.Path)
            .WithArguments([".document.metadata.timestamp", file, "-r"])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(Encoding.UTF8);

        var value = jq.StandardOutput.Trim();
        return long.TryParse(value, out var timestamp) ? timestamp : 0;
    }
}