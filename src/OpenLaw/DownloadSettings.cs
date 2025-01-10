using System.ComponentModel;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

public class DownloadSettings : CommandSettings
{
    [DefaultValue(true)]
    [CommandOption("--all")]
    public bool All { get; set; } = true;

    [CommandOption("--dir")]
    public string Directory { get; set; } = Environment.ExpandEnvironmentVariables("%AppData%\\clarius\\openlaw");
}