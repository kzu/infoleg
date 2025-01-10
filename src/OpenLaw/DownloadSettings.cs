using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

public class DownloadSettings : CommandSettings
{
    [CommandOption("--all")]
    public bool All { get; set; }
}