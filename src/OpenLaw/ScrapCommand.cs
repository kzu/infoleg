using System.ComponentModel;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

[Description("Descarga una norma dado su identificador de InfoLEG.")]
public class ScrapCommand(IHttpClientFactory httpFactory) : AsyncCommand<ScrapCommand.ScrapSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, ScrapSettings settings)
    {
        var path = Environment.ExpandEnvironmentVariables("%AppData%\\infoleg");
        Directory.CreateDirectory(path);

        return Task.FromResult(0);
    }

    public class ScrapSettings : CommandSettings
    {
        [Description(@"El identificador de la norma en InfoLEG")]
        [CommandArgument(0, "<ID>")]
        public required string ID { get; set; }
    }
}
