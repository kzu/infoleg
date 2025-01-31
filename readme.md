![Icon](assets/img/icon.png) OpenLaw
============

Plataforma de código abierto para normas de Argentina.

## Instalación

```
dotnet tool install -g dotnet-openlaw --source https://clarius.blob.core.windows.net/nuget/index.json
```

## Uso:

<!-- include src/dotnet-openlaw/help.md -->
```shell
USAGE:
    openlaw [OPTIONS] <COMMAND>

OPTIONS:
    -h, --help    Prints help information

COMMANDS:
    saij     
```

<!-- src/dotnet-openlaw/help.md -->

<!-- include src/dotnet-openlaw/saij-download.md -->
```shell
USAGE:
    openlaw saij download [OPTIONS]

OPTIONS:
                  DEFAULT                                                       
    -h, --help               Prints help information                            
        --all     True       Descargar todos los documentos, no solamente Leyes 
                             de alcance Nacional                                
        --dir                Ubicación opcional para descarga de archivos       
```

<!-- src/dotnet-openlaw/saij-download.md -->

<!-- include src/dotnet-openlaw/convert.md -->

<!-- include src/dotnet-openlaw/format.md -->
