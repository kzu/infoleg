using Spectre.Console;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

public class DownloadCommand : Command<DownloadSettings>
{
    readonly IAnsiConsole console;

    public DownloadCommand(IAnsiConsole console)
    {
        this.console = console;
    }

    public override int Execute(CommandContext context, DownloadSettings settings)
    {
        console.WriteLine(settings.All ? "Downloading all items." : "Downloading specific item.");
        return 0;
    }
}