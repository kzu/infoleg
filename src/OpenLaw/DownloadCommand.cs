using Spectre.Console;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

public class DownloadCommand(IAnsiConsole console) : AsyncCommand<DownloadSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, DownloadSettings settings)
    {
        return Task.FromResult(0);
    }
}