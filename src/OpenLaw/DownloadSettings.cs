using System.ComponentModel;
using Spectre.Console.Cli;

namespace Clarius.OpenLaw;

public class DownloadSettings : CommandSettings
{
    [Description("Descargar todos los documentos, no solamente Leyes de alcance Nacional.")]
    [DefaultValue(true)]
    [CommandOption("--all")]
    public bool All { get; set; } = false;

    [Description("Ubicación opcional para descarga de archivos.")]
    [CommandOption("--dir")]
    public string Directory { get; set; } = Environment.ExpandEnvironmentVariables("%AppData%\\clarius\\openlaw");
}