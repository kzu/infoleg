using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Spectre.Console;
using Spectre.Console.Cli;
using static Spectre.Console.AnsiConsole;

namespace Clarius.OpenLaw;

public static class App
{
    public static CommandApp Create(out IServiceProvider services)
    {
        var collection = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<TypeRegistrar>()
            .AddDotNetConfig()
            .Build();

        collection.AddSingleton(config)
            .AddSingleton<IConfiguration>(_ => config);

        collection.AddHttpClient()
            .ConfigureHttpClientDefaults(defaults => defaults.ConfigureHttpClient(http =>
                {
                    http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(ThisAssembly.Info.Product, ThisAssembly.Info.InformationalVersion));
                    if (Debugger.IsAttached)
                        http.Timeout = TimeSpan.FromMinutes(10);
                }).AddStandardResilienceHandler());

        collection.AddHttpClient("saij")
            .AddStandardResilienceHandler(config => config.Retry.ShouldHandle = args => new ValueTask<bool>(
                // We'll get a 403 if we hit the rate limit, so we'll consider that transient.
                (args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                HttpClientResiliencePredicates.IsTransient(args.Outcome)) &&
                // We'll get a 500 error when enumerating past the available items too :/
                args.Outcome.Result?.StatusCode != System.Net.HttpStatusCode.InternalServerError));

        var needsNewLine = false;
        collection.AddSingleton<IProgress<string>>(new Progress<string>(message =>
        {
            if (needsNewLine)
                WriteLine();

            Write(new Markup($"> 🧠 [grey]{message.EscapeMarkup()}...[/]"));
            needsNewLine = true;
        }));

        collection.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        var registrar = new TypeRegistrar(collection);
        var app = new CommandApp(registrar);
        registrar.Services.AddSingleton<ICommandApp>(app);

        app.Configure(config =>
        {
            config.AddBranch("saij", saij =>
            {
                saij.AddCommand<DownloadCommand>("download");
            });

            config.AddCommand<ConvertCommand>("convert");
            config.AddCommand<FormatCommand>("format");

            if (Environment.GetEnvironmentVariables().Contains("NO_COLOR"))
                config.Settings.HelpProviderStyles = null;
        });

        services = registrar.Services.BuildServiceProvider();

        return app;
    }
}
