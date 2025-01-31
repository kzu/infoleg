using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

[Description("Normaliza el formato de archivos JSON.")]
public class FormatCommand(IAnsiConsole console) : Command<FormatCommand.FormatSettings>
{
    static readonly JsonSerializerOptions readOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonDictionaryConverter() },
    };

    static readonly JsonSerializerOptions writeOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    public class FormatSettings : CommandSettings
    {
        [Description("Ubicación opcional para descarga de archivos. Por defecto '%AppData%\\clarius\\openlaw'")]
        [CommandOption("--dir")]
        public string Directory { get; set; } = Environment.ExpandEnvironmentVariables("%AppData%\\clarius\\openlaw");
    }

    public override int Execute(CommandContext context, FormatSettings settings)
    {
        if (Directory.Exists(settings.Directory))
        {
            console.Progress()
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                ])
                .Start(ctx =>
                {
                    Parallel.ForEach(Directory.EnumerateFiles(settings.Directory, "*.json", SearchOption.AllDirectories), file =>
                    {
                        var task = ctx.AddTask($"Formateando {file}");
                        task.IsIndeterminate = true;
                        FormatFile(file);
                        task.Value(100);
                    });
                });
        }

        return 0;
    }

    void FormatFile(string file)
    {
        var json = File.ReadAllText(file);
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, readOptions);
        File.WriteAllText(file, JsonSerializer.Serialize(dictionary, writeOptions));
    }
}
