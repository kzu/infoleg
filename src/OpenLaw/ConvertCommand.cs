using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;

namespace Clarius.OpenLaw;

[Description("Convierte archivos JSON a YAML y Markdown.")]
public class ConvertCommand(IAnsiConsole console) : Command<ConvertCommand.ConvertSettings>
{
    public override int Execute(CommandContext context, ConvertSettings settings)
    {
        if (settings.File is not null)
        {
            ConvertFile(settings.File, true);
            return 0;
        }

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
                        var task = ctx.AddTask($"Convirtiendo {file}");
                        task.IsIndeterminate = true;
                        ConvertFile(file, true);
                        task.Value(100);
                    });
                });
        }

        return 0;
    }

    static void ConvertFile(string file, bool overwrite) => DictionaryConverter.ConvertFile(file, overwrite);

    public class ConvertSettings : CommandSettings
    {
        public override ValidationResult Validate()
        {
            if (!string.IsNullOrWhiteSpace(File) && !System.IO.File.Exists(File))
                return ValidationResult.Error("El archivo especificado '{File}' no existe.");

            return base.Validate();
        }

        [Description("Archivo a convertir. Opcional.")]
        [CommandArgument(0, "[file]")]
        public string? File { get; set; }

        [Description("Ubicación de archivos a convertir. Por defecto '%AppData%\\clarius\\openlaw'")]
        [CommandOption("--dir")]
        public string Directory { get; set; } = Environment.ExpandEnvironmentVariables("%AppData%\\clarius\\openlaw");
    }
}
